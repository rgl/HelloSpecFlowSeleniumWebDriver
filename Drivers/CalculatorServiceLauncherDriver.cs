using CliWrap;
using CliWrap.EventStream;
using System;
using System.IO;
using System.IO.Compression;
using System.Net;
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

        public string Start(string address)
        {
            if (_commandWrapper != null)
            {
                return _address;
            }

            // TODO add support for linux using https://www.nuget.org/packages/SharpZipLib/ to extract the tarball.

            var version = "1.0.0";
            var url = $"https://github.com/rgl/calculator-example-html/releases/download/v{version}/calculator-example-html_{version}_windows_amd64.zip";
            var processPath = Path.Combine(Path.GetTempPath(), $"CalculatorServiceLauncherDriver/calculator-example-html-v{version}.exe");

            if (!File.Exists(processPath))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(processPath));

                using var webClient = new WebClient();
                var zipData = webClient.DownloadData(url);
                var zipArchive = new ZipArchive(new MemoryStream(zipData), ZipArchiveMode.Read);
                var processEntry = zipArchive.GetEntry("calculator-example-html.exe");
                processEntry.ExtractToFile(processPath);
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
                                break;
                            case ExitedCommandEvent exitedEvent:
                                return;
                        }
                    }
                });

                actualAddressLock.WaitOne();

                return actualAddress;
            }
        }
    }
}
