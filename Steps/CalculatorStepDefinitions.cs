using HelloSpecFlowSeleniumWebDriver.Drivers;
using TechTalk.SpecFlow;
using Xunit;

namespace HelloSpecFlowSeleniumWebDriver.Steps
{
    [Binding]
    sealed class CalculatorStepDefinitions
    {
        private readonly CalculatorDriver _calculatorDriver;
        private double _firstNumber;
        private double _secondNumber;

        public CalculatorStepDefinitions(CalculatorDriver calculatorDriver)
        {
            _calculatorDriver = calculatorDriver;
        }

        [Given("the first number is (.*)")]
        public void GivenTheFirstNumberIs(double number)
        {
            _calculatorDriver.Clear();
            _firstNumber = number;
            _secondNumber = double.NaN;
        }

        [Given("the second number is (.*)")]
        public void GivenTheSecondNumberIs(double number)
        {
            _secondNumber = number;
        }

        [When("the two numbers are added")]
        public void WhenTheTwoNumbersAreAdded()
        {
            _calculatorDriver.AddNumbers(_firstNumber, _secondNumber);
        }

        [Then("the result should be (.*)")]
        public void ThenTheResultShouldBe(double result)
        {
            Assert.Equal(result, _calculatorDriver.Result);
        }
    }
}
