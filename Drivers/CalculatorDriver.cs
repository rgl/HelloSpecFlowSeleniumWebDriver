using OpenQA.Selenium;
using System.Globalization;

namespace HelloSpecFlowSeleniumWebDriver.Drivers
{
    class CalculatorDriver
    {
        private readonly CalculatorServiceLauncherDriver _calculatorServiceLauncherDriver;
        private readonly WebDriverDriver _webDriverDriver;

        public CalculatorDriver(CalculatorServiceLauncherDriver calculatorServiceLauncherDriver, WebDriverDriver webDriverDriver)
        {
            _calculatorServiceLauncherDriver = calculatorServiceLauncherDriver;
            _webDriverDriver = webDriverDriver;
        }

        public double Result
        {
            get
            {
                var resultElement = _webDriverDriver.WebDriver.FindElement(By.CssSelector(".calculator__display"));
                return double.Parse(resultElement.Text);
            }
        }

        public void Clear()
        {
            var address = _calculatorServiceLauncherDriver.Start();

            _webDriverDriver.WebDriver.Navigate().GoToUrl($"http://{address}");
        }

        public void TypeNumber(double number)
        {
            foreach (var digit in number.ToString(CultureInfo.InvariantCulture))
            {
                var digitElement = _webDriverDriver.WebDriver.FindElement(By.XPath($"//div[@class='calculator__keys']/button[text()='{digit}']"));
                digitElement.Click();
            }
        }

        public void TypeOperator(char @operator)
        {
            var element = _webDriverDriver.WebDriver.FindElement(By.XPath($"//div[@class='calculator__keys']/button[text()='{@operator}']"));
            element.Click();
        }

        public void AddNumbers(double first, double second)
        {
            TypeNumber(first);
            TypeOperator('+');
            TypeNumber(second);
            TypeOperator('=');
        }
    }
}
