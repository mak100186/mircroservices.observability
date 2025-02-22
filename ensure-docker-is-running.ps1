# Function to check if Docker is running and responsive
function Test-DockerRunning {
    try {
        $null = & docker info 2>&1
        return $LASTEXITCODE -eq 0
    } catch {
        return $false
    }
}

# Check if Docker is already running
if (Test-DockerRunning) {
    Write-Host "Docker is already running and responsive."
    exit 0
}

# Docker is not running, so let's start Docker Desktop
$dockerDesktopPath = "${env:ProgramFiles}\Docker\Docker\Docker Desktop.exe"

if (Test-Path $dockerDesktopPath) {
    Write-Host "Starting Docker Desktop..."
    Start-Process $dockerDesktopPath -ArgumentList "--minimize" -WindowStyle Minimized

    # Wait for Docker to become responsive
    $timeout = 20  # 3 minutes
    $timer = [Diagnostics.Stopwatch]::StartNew()

    while ($timer.Elapsed.TotalSeconds -lt $timeout) {
        if (Test-DockerRunning) {
            Write-Host "Docker is now running and responsive."
            $timer.Stop()
            exit 0
        }
        Start-Sleep -Seconds 5
        Write-Host "Waiting for Docker to become responsive..."
    }

    $timer.Stop()
    Write-Host "Timeout reached. Docker is not responding. Please check Docker Desktop manually."
    exit 1
} else {
    Write-Host "Docker Desktop executable not found. Please ensure Docker is installed correctly."
    exit 1
}
