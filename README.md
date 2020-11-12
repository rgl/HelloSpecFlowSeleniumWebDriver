# About

[![Build status](https://github.com/rgl/HelloSpecFlowSeleniumWebDriver/workflows/Build/badge.svg)](https://github.com/rgl/HelloSpecFlowSeleniumWebDriver/actions?query=workflow%3ABuild)

Example C# SpecFlow (and Selenium) Web UI tests (using [calculator-example-html](https://github.com/rgl/calculator-example-html)).

Also see the [rgl/HelloSeleniumWebDriver](https://github.com/rgl/HelloSeleniumWebDriver) repository.

## Usage

List the available tests:

```bash
dotnet test --list-tests
```

You should see something like:

```plain
The following Tests are available:
    Add two numbers
```

Run the tests:

```bash
dotnet test
```

**NB** You can run a sub-set of the tests with the [`--filter` command line argument](https://docs.microsoft.com/en-us/dotnet/core/tools/dotnet-test#filter-option-details),
e.g. `--filter '(Name~Add two numbers)'`.

To save the test reports in the [JUnit format](https://github.com/spekt/junit.testlogger#usage), run as:

```bash
dotnet test --logger junit --test-adapter-path .
```

This will save the test results into the `TestResults/TestResults.xml` file.

### Configuration

You can modify the Web Driver behavior using the following environment variables:

| Environment Variable | Example | Description |
|----------------------|---------|-------------|
| `BROWSER_DRIVER_WINDOW_SIZE` | `800x600` | The browser window size |
| `BROWSER_DRIVER_WINDOW_CLIENT_SIZE` | `800x600` | The browser client area size; this overrides the value of `BROWSER_DRIVER_WINDOW_SIZE` |
| `BROWSER_DRIVER_HEADLESS` | `1` | Run the browser in headless mode |
| `BROWSER_DRIVER_LOG_PATH` | `chromedriver.log` | Path where the web driver logs will be saved |
| `BROWSER_DRIVER_URL` | `http://chrome:9515` | Remote chromedriver url. When set, chromedriver will not be started locally, instead, it will be used remotely. |
| `CALCULATOR_SERVICE_PATH` | `C:\calculator-example-html.exe` | Path for the [calculator-example-html](https://github.com/rgl/calculator-example-html) binary |
| `CALCULATOR_SERVICE_LISTEN_ADDRESS` | `localhost:0` | The address where the calculator service will listen on. Use the `*` host to listen in the hostname IP address. Use the `0` port to choose a random listening port. |

## Alternatives

* [cypress](https://www.cypress.io/)
* [Playwright](https://playwright.dev/)
* [Puppeteer](https://pptr.dev/)
