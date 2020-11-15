using CliWrap;
using CliWrap.EventStream;
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace HelloSpecFlowSeleniumWebDriver.Drivers
{
    class CalculatorServiceLauncherDriver : IDisposable
    {
        private CommandWrapper _commandWrapper;
        private string _address;

        public void Dispose()
        {
            if (_commandWrapper != null)
            {
                _commandWrapper.Dispose();
                _commandWrapper = null;
            }
        }

        public string Start()
        {
            if (_commandWrapper != null)
            {
                return _address;
            }

            var address = Environment.GetEnvironmentVariable("CALCULATOR_SERVICE_LISTEN_ADDRESS") ?? "localhost:0";

            if (address.StartsWith("*:"))
            {
                var hostIpAddress = Dns.GetHostEntry(Dns.GetHostName()).AddressList.Where(a => a.AddressFamily == AddressFamily.InterNetwork).First();

                address = $"{hostIpAddress}:{address.Split(':', 2)[1]}";
            }

            // TODO add support for linux using https://www.nuget.org/packages/SharpZipLib/ to extract the tarball.

            var processPath = Environment.GetEnvironmentVariable("CALCULATOR_SERVICE_PATH");

            if (processPath == null)
            {
                // NB if you change this, also change it in the Dockerfile.
                var version = "1.0.0";
                var url = $"https://github.com/rgl/calculator-example-html/releases/download/v{version}/calculator-example-html_{version}_windows_amd64.zip";
                processPath = Path.Combine(Path.GetTempPath(), $"CalculatorServiceLauncherDriver/calculator-example-html-v{version}.exe");

                if (!File.Exists(processPath))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(processPath));

                    using var webClient = new WebClient();
                    var zipData = webClient.DownloadData(url);
                    var zipArchive = new ZipArchive(new MemoryStream(zipData), ZipArchiveMode.Read);
                    var processEntry = zipArchive.GetEntry("calculator-example-html.exe");
                    processEntry.ExtractToFile(processPath);
                }
            }

            _commandWrapper = new CommandWrapper();

            return _address = _commandWrapper.StartAndWaitForListenAddress(processPath, address);
        }

        private class CommandWrapper : IDisposable
        {
            private CancellationTokenSource _cancellationTokenSource;

            public void Dispose()
            {
                _cancellationTokenSource.Cancel();
            }

            public string StartAndWaitForListenAddress(string processPath, string address)
            {
                _cancellationTokenSource = new CancellationTokenSource();

                var actualAddressLock = new ManualResetEvent(false);

                string actualAddress = null;

                Task.Run(async () =>
                {
                    var isActualListenAddressSet = false;
                    var listenAddressRegex = new Regex(@"Listening at http://(?<address>.+)");
                    var command = Cli.Wrap(processPath).WithArguments(new[] { "-listen", address });

                    await foreach (var @event in command.ListenAsync(_cancellationTokenSource.Token))
                    {
                        switch (@event)
                        {
                            case StartedCommandEvent startedEvent:
                                break;
                            case StandardOutputCommandEvent stdOutEvent:
                                Console.WriteLine($"Calculator: {stdOutEvent.Text}");
                                if (!isActualListenAddressSet)
                                {
                                    var match = listenAddressRegex.Match(stdOutEvent.Text);
                                    if (match.Success)
                                    {
                                        actualAddress = match.Groups["address"].Value;
                                        actualAddressLock.Set();
                                        isActualListenAddressSet = true;
                                    }
                                }
                                break;
                            case StandardErrorCommandEvent stdErrEvent:
                                Console.WriteLine($"Calculator Error: {stdErrEvent.Text}");
                                break;
                            case ExitedCommandEvent exitedEvent:
                                return;
                        }
                    }
                });

                actualAddressLock.WaitOne();

                if (string.IsNullOrEmpty(actualAddress))
                {
                    throw new Exception("Failed to start Calculator");
                }

                return actualAddress;
            }
        }
    }
}
