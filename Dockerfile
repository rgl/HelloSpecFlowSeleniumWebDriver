# escape=`
# see https://hub.docker.com/_/microsoft-dotnet-core-sdk
# see https://github.com/dotnet/dotnet-docker/blob/master/src/sdk/3.1/nanoserver-1809/amd64/Dockerfile
FROM mcr.microsoft.com/dotnet/core/sdk:3.1
SHELL ["pwsh.exe", "-Command", "$ErrorActionPreference = 'Stop'; $ProgressPreference = 'SilentlyContinue';"]
# download calculator-example-html.
RUN mkdir c:\calculator-example-html | Out-Null; `
    (New-Object System.Net.WebClient).DownloadFile( `
        'https://github.com/rgl/calculator-example-html/releases/download/v1.0.0/calculator-example-html_1.0.0_windows_amd64.zip', `
        'c:\calculator-example-html\calculator-example-html.zip'); `
    tar xf c:\calculator-example-html\calculator-example-html.zip -C c:\calculator-example-html; `
    rm 'c:\calculator-example-html\calculator-example-html.zip'
WORKDIR c:/build
# restore packages
COPY *.csproj .
RUN dotnet restore
# build.
COPY . .
RUN dotnet build --no-restore --configuration Release
# setup for running the tests.
ENV BROWSER_DRIVER_HEADLESS="1"
ENV BROWSER_DRIVER_WINDOW_SIZE="1024x768"
ENV CALCULATOR_SERVICE_LISTEN_ADDRESS="*:0"
ENV CALCULATOR_SERVICE_PATH="C:\calculator-example-html\calculator-example-html.exe"
ENTRYPOINT ["dotnet.exe", "test", "--no-build", "--configuration", "Release", "--logger", "junit", "--test-adapter-path", "."]
