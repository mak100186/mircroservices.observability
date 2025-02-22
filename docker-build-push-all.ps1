# Hardcoded Docker Hub username
$DockerHubUsername = "mak100186"

#Step 1: start the docker deamon and desktop if not already started
Write-Host "Step 1: Ensure docker is running..."

# Define the path to the script you want to call
$scriptToCall = ".\ensure-docker-is-running.ps1"

# Check if the script exists
if (Test-Path $scriptToCall) {    
    # Call the script and wait for its completion
    & $scriptToCall    
} else {
    Write-Host "Error: Secondary script not found at $scriptToCall"
}

# Step 2: Find all Dockerfiles in the current directory and subdirectories
# Find all Dockerfiles in the current directory that start with 'Dockerfile.'
$Dockerfiles = Get-ChildItem -Path . -Filter "Dockerfile.*"

foreach ($Dockerfile in $Dockerfiles) {
    $ImageName = $Dockerfile.Name -replace '^Dockerfile\.', ''
    $FullImageName = "${DockerHubUsername}/${ImageName}".ToLower()

    Write-Host "Building Docker image: $FullImageName"

    # Build the Docker image
    docker build -t $FullImageName -f $Dockerfile.Name .
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
    }
}

Write-Host "Build and push process completed for all Dockerfiles."