# escape=`
#FROM mcr.microsoft.com/windows/servercore:1809 # NB does not work in this base image.
FROM mcr.microsoft.com/windows:1809
SHELL ["powershell.exe", "-Command", "$ErrorActionPreference = 'Stop'; $ProgressPreference = 'SilentlyContinue';"]
WORKDIR c:/app
RUN Set-ExecutionPolicy -ExecutionPolicy Bypass -Force; `
    Invoke-Expression (New-Object System.Net.WebClient).DownloadString('https://chocolatey.org/install.ps1'); `
    choco feature disable -name showDownloadProgress
# NB there's a big caveat with this package, it will always install the
#    latest version regardless of the package version because google chrome
#    does not have a version specific download address. and thats why we
#    also ignore the checksums.
# TODO switch to chromium, maybe an ungoogled version.
#      see https://github.com/Hibbiki/chromium-win32
#      see https://chocolatey.org/packages/chromium/86.0.4240.111#files
RUN choco install -y GoogleChrome --ignore-checksums
RUN choco install -y chromedriver
EXPOSE 9515
ENTRYPOINT ["chromedriver.exe", "--port=9515", "--whitelisted-ips"]
