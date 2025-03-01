param (	
    #[Parameter(Mandatory = $true)] #uncomment this and the commandline will ask for this variable.
    [string]$Version,
    [string]$ProjectRoot,
    [string]$DockerHubUsername,
    [string]$AccessToken
)

# Function to check if Docker is running and responsive
function Test-DockerRunning {
    try {
        $null = & docker info 2>&1
        return $LASTEXITCODE -eq 0
    } catch {
        return $false
    }
}

# Function to check if the repository exists
function Check-RepositoryExists {
    param (
        [string]$RepoName
    )
    $checkRepoUrl = "https://hub.docker.com/v2/repositories/$DockerHubUsername/$RepoName/"
    try {
        $response = Invoke-RestMethod -Uri $checkRepoUrl -Method Get -Headers $headers
        return $true
    } catch {
        return $false
    }
}

# Check if the version is provided
if (-not $Version) {
	# Get the version from the environment variable
	$Version = $env:DOCKER_IMAGE_VERSION
	
	# Check if the version is provided in env variable
	if (-not $Version) {
		Write-Host "Error: DOCKER_IMAGE_VERSION is not set."
		exit 1
	}
}

# Check if the version is provided
if (-not $ProjectRoot) {
	# Check if environment variables are set
	$ProjectRoot = $env:PROJECT_ROOT
	
	# Check if the version is provided in env variable
	if (-not $ProjectRoot) {
		Write-Host "Error: PROJECT_ROOT environment variable is not set."
		exit 1
	}
}

# Check if the version is provided
if (-not $DockerHubUsername) {
	# Check if environment variables are set
	$DockerHubUsername = $env:DOCKER_HUB_USERNAME
	
	# Check if the version is provided in env variable
	if (-not $DockerHubUsername) {
		Write-Host "Error: DOCKER_HUB_USERNAME environment variable is not set."
		exit 1
	}
}

# Check if the version is provided
if (-not $AccessToken) {
	# Check if environment variables are set
	$AccessToken = $env:DOCKER_HUB_ACCESS_TOKEN
	
	# Check if the version is provided in env variable
	if (-not $AccessToken) {
		Write-Host "Error: DOCKER_HUB_ACCESS_TOKEN environment variable is not set."
		exit 1
	}
}

Write-Host "Using vars: ProjcetRoot:$ProjectRoot, DockerHubUsername:$DockerHubUsername, Version: $Version"

# Define the API endpoints
$createRepoUrl = "https://hub.docker.com/v2/repositories/"

# Define the headers
$headers = @{
    "Authorization" = "Bearer $AccessToken"
    "Content-Type"  = "application/json"
}

#Step 1: start the docker deamon and desktop if not already started
Write-Host "Step 1: Ensure docker is running..."

# Check if Docker is already running
if (Test-DockerRunning) {
    Write-Host "Docker is already running and responsive."
} else {
    # Docker is not running, so let's start Docker Desktop
    $dockerDesktopPath = "${env:ProgramFiles}\Docker\Docker\Docker Desktop.exe"

    if (Test-Path $dockerDesktopPath) {
        Write-Host "Starting Docker Desktop..."
        Start-Process $dockerDesktopPath -ArgumentList "--minimize" -WindowStyle Minimized

        # Wait for Docker to become responsive
        $timeout = 180  # 3 minutes
        $timer = [Diagnostics.Stopwatch]::StartNew()

        while ($timer.Elapsed.TotalSeconds -lt $timeout) {
            if (Test-DockerRunning) {                
                $timer.Stop()
                break
            }
            Start-Sleep -Seconds 5
            Write-Host "Waiting for Docker to become responsive..."
        }                
    } else {
        Write-Host "Docker Desktop executable not found. Please ensure Docker is installed correctly."
        exit 1
    }
	
	if (Test-DockerRunning) {
		Write-Host "Docker is running and responsive."
	} else {
		Write-Host "Docker is not responding. Please check Docker Desktop manually."
		exit 1
	}
}

#Step 3: Create repositories if they dont exist

Write-Host "Searching for docker files at: $ProjectRoot"

# Step 3: Find all Dockerfiles in the current directory and subdirectories
# Find all Dockerfiles in the current directory that start with 'Dockerfile.'
$Dockerfiles = Get-ChildItem -Path $ProjectRoot -Filter "Dockerfile.*"

# Initialize a counter for total docker files to process
$totalDockerFiles = $Dockerfiles.Count

# Initialize a counter for successfully pushed Docker images
$successfulPushCount = 0

foreach ($Dockerfile in $Dockerfiles) {
	$DockerfilePath = $Dockerfile.FullName
	$RepoName = $Dockerfile.Name -replace '^Dockerfile\.', ''
    $ImageName = $Dockerfile.Name -replace '^Dockerfile\.', ''
    $FullImageName = "${DockerHubUsername}/${ImageName}:${Version}".ToLower()

	# Check if the repository exists
    if (Check-RepositoryExists -RepoName $RepoName) {
        Write-Host "Repository '$RepoName' already exists."
    } else {
        Write-Host "Repository '$RepoName' does not exist. Creating repository..."

        # Define the request body
        $requestBody = @{
            name = $RepoName
			namespace = $DockerHubUsername
            is_private = $false  # Set to $true if you want the repository to be private
        } | ConvertTo-Json

        # Make the HTTP POST request to create the repository
        $response = Invoke-RestMethod -Uri $createRepoUrl -Method Post -Headers $headers -Body $requestBody

        # Check the response
        if ($response) {
            Write-Host "Repository '$RepoName' created successfully."
        } else {
            Write-Host "Failed to create repository '$RepoName'. Skipping..."
			continue
        }
    }
	
    Write-Host "Building Docker image: $FullImageName"

    # Build the Docker image
    docker build -t $FullImageName -f $DockerfilePath $ProjectRoot
    if ($LASTEXITCODE -ne 0) {
        Write-Host "Failed to build Docker image $FullImageName. Skipping..."
        continue
    }

    Write-Host "Pushing Docker image $FullImageName to Docker Hub"
    docker push $FullImageName
    if ($LASTEXITCODE -ne 0) {
        Write-Host "Failed to push Docker image $FullImageName to Docker Hub."
    } else {
        Write-Host "Successfully pushed $FullImageName to Docker Hub."
		$successfulPushCount++
    }
}

Write-Host "Total number of Dockerfiles processed: $totalDockerfiles"
Write-Host "Total number of successfully pushed Docker images: $successfulPushCount"


