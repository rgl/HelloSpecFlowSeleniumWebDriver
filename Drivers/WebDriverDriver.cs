using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Remote;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace HelloSpecFlowSeleniumWebDriver.Drivers
{
    class WebDriverDriver : IDisposable
    {
        public IWebDriver WebDriver { get; }

        public WebDriverDriver()
        {
            WebDriver = CreateWebDriver();
        }

        public void Dispose()
        {
            WebDriver.Dispose();
        }

        private IWebDriver CreateWebDriver()
        {
            var windowSizeOption = Environment.GetEnvironmentVariable("BROWSER_DRIVER_WINDOW_SIZE");
            var windowClientSizeOption = Environment.GetEnvironmentVariable("BROWSER_DRIVER_WINDOW_CLIENT_SIZE");
            var headlessOption = Environment.GetEnvironmentVariable("BROWSER_DRIVER_HEADLESS");
            var logPathOption = Environment.GetEnvironmentVariable("BROWSER_DRIVER_LOG_PATH");
            var driverUrl = Environment.GetEnvironmentVariable("BROWSER_DRIVER_URL");

            var windowSize = ParseSize(string.IsNullOrEmpty(windowClientSizeOption) ? windowSizeOption : windowClientSizeOption);
            var windowClientSize = ParseSize(windowClientSizeOption);
            var headless = headlessOption == "1" || headlessOption == "true";
            var logPath = string.IsNullOrEmpty(logPathOption) ? "chromedriver.log" : logPathOption;

            // see https://sites.google.com/a/chromium.org/chromedriver/capabilities
            var chromeOptions = new ChromeOptions();

            if (windowSize.HasValue)
            {
                if (!windowClientSize.HasValue)
                {
                    Console.WriteLine($"Configuring the window size to {windowSize.Value}...");
                }
                chromeOptions.AddArguments($"--window-size={windowSize.Value.Width},{windowSize.Value.Height}");
            }

            // enable headless mode when requested.
            // see https://developers.google.com/web/updates/2017/04/headless-chrome
            // see https://intoli.com/blog/running-selenium-with-headless-chrome/
            if (headless)
            {
                Console.WriteLine("Configuring chrome to run headless...");
                chromeOptions.AddArguments("--headless");
                chromeOptions.AddArguments("--disable-gpu");
            }

            RemoteWebDriver wd;

            if (string.IsNullOrEmpty(driverUrl))
            {
                Console.WriteLine("Using local chrome web-driver...");

                var service = ChromeDriverService.CreateDefaultService();
                service.LogPath = logPath;
                service.EnableVerboseLogging = true;

                wd = new ChromeDriver(service, chromeOptions);
            }
            else
            {
                Console.WriteLine($"Using remote chrome web-driver at {driverUrl}...");

                wd = new RemoteWebDriver(new Uri(driverUrl), chromeOptions);
            }

            // move the window to the top-left corner.
            wd.Manage().Window.Position = new Point(0, 0);

            // resize the window client size.
            if (windowClientSize.HasValue)
            {
                Console.WriteLine($"Resizing the window client (content) size to {windowClientSize.Value}...");
                var js = (IJavaScriptExecutor)wd;
                var initialWindowPadding = (IReadOnlyCollection<object>)js.ExecuteScript("return [window.outerWidth-window.innerWidth, window.outerHeight-window.innerHeight];");
                wd.Manage().Window.Size = new Size(
                    windowClientSize.Value.Width + Convert.ToInt32(initialWindowPadding.ElementAt(0)),
                    windowClientSize.Value.Height + Convert.ToInt32(initialWindowPadding.ElementAt(1))
                );
            }

            return wd;
        }

        private static Size? ParseSize(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return null;
            }

            var parts = value.Split('x');

            if (parts.Length != 2)
            {
                return null;
            }

            return new Size(int.Parse(parts[0]), int.Parse(parts[1]));
        }
    }
}
