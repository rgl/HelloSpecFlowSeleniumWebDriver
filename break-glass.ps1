# see https://github.com/rgl/frp-github-actions-reverse-shell

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$ProgressPreference = 'SilentlyContinue'
trap {
    Write-Output "ERROR: $_"
    Write-Output (($_.ScriptStackTrace -split '\r?\n') -replace '^(.*)$','ERROR: $1')
    Exit 1
}

mkdir -Force .break-glass | Out-Null
Push-Location .break-glass

# install frp.
if (!(Test-Path frps.exe)) {
    Write-Output 'Downloading frp...'
    (New-Object System.Net.WebClient).DownloadFile(
        'https://github.com/fatedier/frp/releases/download/v0.34.2/frp_0.34.2_windows_amd64.zip',
        "$PWD/frp.zip")
    Write-Output 'Extracting frp...'
    tar xf frp.zip --strip-components=1
}

# when running in CI override the frpc tls files.
if (Test-Path env:FRPC_TLS_CA_CERTIFICATE) {
    Write-Output 'Configuring certificates...'
    mkdir -Force ca | Out-Null
    Set-Content -Encoding Ascii ca/github-key.pem $env:FRPC_TLS_KEY
    Set-Content -Encoding Ascii ca/github.pem $env:FRPC_TLS_CERTIFICATE
    Set-Content -Encoding Ascii ca/ca.pem $env:FRPC_TLS_CA_CERTIFICATE
}

# set password when requested.
if (Test-Path env:RUNNER_PASSWORD) {
    Write-Output "Setting the $env:USERNAME user password..."
    Get-LocalUser $env:USERNAME `
        | Set-LocalUser `
            -Password (
                ConvertTo-SecureString `
                    -AsPlainText `
                    -Force `
                    $env:RUNNER_PASSWORD
            )
}

Write-Output "Creating the frpc configuration file..."
Set-Content -Encoding Ascii frpc.ini @"
[common]
server_addr = {{ .Envs.FRPS_DOMAIN }}
server_port = 6969

# configure TLS.
# see tls_enable at https://github.com/fatedier/frp/blob/v0.34.2/pkg/config/client_common.go#L106-L109
# see tls_key_file at https://github.com/fatedier/frp/blob/v0.34.2/pkg/config/client_common.go#L113-L116
# see tls_cert_file at https://github.com/fatedier/frp/blob/v0.34.2/pkg/config/client_common.go#L110-L112
# see tls_trusted_ca_file at https://github.com/fatedier/frp/blob/v0.34.2/pkg/config/client_common.go#L117-L120
tls_enable = true
tls_key_file = ca/github-key.pem
tls_cert_file = ca/github.pem
tls_trusted_ca_file = ca/ca.pem

# set client metadata.
# NB all properties that start with "meta_" will be added as metadata.
# NB currently there is no way to see these in the server dashboard.
# see commit that introduced this feature https://github.com/fatedier/frp/commit/a57679f8375986abc970d22bad52644ba62a4969
# see metas at https://github.com/fatedier/frp/blob/v0.34.2/pkg/config/client_common.go#L129-L130
meta_github_repository = {{ .Envs.GITHUB_REPOSITORY }}
meta_github_ref = {{ .Envs.GITHUB_REF }}
meta_github_revision = {{ .Envs.GITHUB_SHA }}

[rdp]
type = tcp
local_ip = 127.0.0.1
local_port = 3389
remote_port = 6001
"@

Write-Output 'Running frpc...'
./frpc -c frpc.ini 2>&1 | Select-Object {$_ -replace '[0-9\.]+:6969','***:6969'}; cmd /c exit 0
