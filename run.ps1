@('tmp', 'TestResults') | ForEach-Object {
	if (!(Test-Path $_)) {
		mkdir $_ | Out-Null
	}
	rm $_/*
}
Write-Output "Building the tests container image..."
docker build --iidfile tmp/docker-image-id .
Write-Output "Running tests..."
docker run --rm `
	-v "$PWD\TestResults:c:\build\TestResults" `
	-v "$PWD\tmp:c:\host" `
	"$(cat tmp/docker-image-id)"
