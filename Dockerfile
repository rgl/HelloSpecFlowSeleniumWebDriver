# escape=`
# see https://hub.docker.com/_/microsoft-dotnet-core-sdk
# see https://github.com/dotnet/dotnet-docker/blob/master/src/sdk/3.1/nanoserver-1809/amd64/Dockerfile
FROM mcr.microsoft.com/dotnet/core/sdk:3.1 as dotnet

#FROM mcr.microsoft.com/windows/servercore:1809 # NB chrome does not work in this base image.
FROM mcr.microsoft.com/windows:1809
SHELL ["powershell.exe", "-Command", "$ErrorActionPreference = 'Stop'; $ProgressPreference = 'SilentlyContinue';"]
WORKDIR c:/build
RUN Set-ExecutionPolicy -ExecutionPolicy Bypass -Force; `
    Invoke-Expression (New-Object System.Net.WebClient).DownloadString('https://chocolatey.org/install.ps1'); `
    choco feature disable -name showDownloadProgress
# NB there's a big caveat with this package, it will always install the
#    latest version regardless of the package version because google chrome
#    does not have a version specific download address.
# TODO switch to chromium, maybe an ungoogled version.
#      see https://github.com/Hibbiki/chromium-win32
#      see https://chocolatey.org/packages/chromium/86.0.4240.111#files
RUN choco install -y GoogleChrome
# install the dotnet sdk.
COPY --from=dotnet ["c:/Program Files/dotnet", "c:/Program Files/dotnet"]
RUN [Environment]::SetEnvironmentVariable('PATH', '{0};C:\Program Files\dotnet' -f $env:PATH, 'Machine')
# trigger the first run experience by running an arbitrary dotnet command.
RUN dotnet help
# download calculator-example-html.
RUN mkdir c:\calculator-example-html | Out-Null; `
    (New-Object System.Net.WebClient).DownloadFile( `
        'https://github.com/rgl/calculator-example-html/releases/download/v1.0.0/calculator-example-html_1.0.0_windows_amd64.zip', `
        'c:\calculator-example-html\calculator-example-html.zip'); `
    tar xf c:\calculator-example-html\calculator-example-html.zip -C c:\calculator-example-html; `
    rm 'c:\calculator-example-html\calculator-example-html.zip'
# restore packages
COPY *.csproj .
RUN dotnet restore
# build.
COPY . .
RUN dotnet build --no-restore
# setup for running the tests.
ENV BROWSER_DRIVER_HEADLESS="1"
ENV BROWSER_DRIVER_WINDOW_SIZE="1024x768"
ENV BROWSER_DRIVER_LOG_PATH="c:\host\chromedriver.log"
ENV CALCULATOR_SERVICE_PATH="C:\calculator-example-html\calculator-example-html.exe"
ENTRYPOINT ["dotnet.exe", "test", "--no-build", "--logger", "junit", "--test-adapter-path", "."]
