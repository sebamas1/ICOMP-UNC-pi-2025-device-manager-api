# Delete old containers
Write-Host "###### Step 1: Deleting old containers..."

docker-compose -f compose.yml down --remove-orphans

Write-Host " ------- Old containers deleted successfully. ------- "

# Build and run containers
Write-Host "###### Step 2: Building and running containers..."
docker-compose -f compose.yml up --build -d 

Write-Host " ------- Containers are being built and started in detached mode. ------- "

# Wait for containers to be ready
Write-Host "###### Step 3: Waiting for containers to be ready..."
Start-Sleep -Seconds 10

# Check if the containers are running
Write-Host "###### Step 4: Checking if containers are running..."

$success = $false
while (-not $success) {
    $containers = docker ps --filter "status=running" --format "{{.Names}}"
    Write-Host "Currently running containers: $containers" 
    # Check if the device-manager container is running

    if ($containers -match "device-api") {
        Write-Host "Device Manager API container is running."
        $success = $true
    } else {
        Write-Host "Device Manager API container is not running. Retrying in 5 seconds..."
        $success = $false
        Start-Sleep -Seconds 5
    }

    # # Check if the SQL Server container is running
    # if ($containers -match "sqlserver") {
    #     Write-Host "SQL Server container is running."
    #     $success = $true
    # } else {
    #     Write-Host "SQL Server container is not running. Retrying in 5 seconds..."
    #     $success = $false
    #     Start-Sleep -Seconds 5
    # }
}
# Final message
Write-Host "###### All containers are up and running successfully. ######"