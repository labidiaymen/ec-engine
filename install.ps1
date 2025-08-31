# ECEngine Windows Installer Script
# Usage: iwr -useb https://raw.githubusercontent.com/labidiaymen/ec-engine/main/install.ps1 | iex

param(
    [string]$InstallDir = "$env:USERPROFILE\.eec",
    [string]$BinDir = "$env:USERPROFILE\.local\bin"
)

$ErrorActionPreference = "Stop"

# Configuration
$Repo = "labidiaymen/ec-engine"

# Colors for output (if supported)
function Write-Info { param($Message) Write-Host "[INFO] $Message" -ForegroundColor Blue }
function Write-Success { param($Message) Write-Host "[SUCCESS] $Message" -ForegroundColor Green }
function Write-Warning { param($Message) Write-Host "[WARNING] $Message" -ForegroundColor Yellow }
function Write-Error { param($Message) Write-Host "[ERROR] $Message" -ForegroundColor Red }

# Function to get the latest release version
function Get-LatestVersion {
    try {
        $response = Invoke-RestMethod -Uri "https://api.github.com/repos/$Repo/releases/latest"
        return $response.tag_name
    }
    catch {
        Write-Error "Failed to get latest version: $_"
        exit 1
    }
}

# Function to download and install ECEngine
function Install-ECEngine {
    param($Version)
    
    $filename = "ecengine-windows-x64.zip"
    $url = "https://github.com/$Repo/releases/download/$Version/$filename"
    $tempFile = "$env:TEMP\$filename"
    
    Write-Info "Installing eec $Version for Windows x64..."
    
    # Create install directory
    if (Test-Path $InstallDir) {
        Remove-Item -Recurse -Force $InstallDir
    }
    New-Item -ItemType Directory -Force -Path $InstallDir | Out-Null
    New-Item -ItemType Directory -Force -Path $BinDir | Out-Null
    
    # Download the release
    Write-Info "Downloading from $url..."
    try {
        Invoke-WebRequest -Uri $url -OutFile $tempFile
    }
    catch {
        Write-Error "Failed to download eec release: $_"
        Write-Error "URL: $url"
        exit 1
    }
    
    # Extract the archive
    Write-Info "Extracting eec..."
    try {
        Expand-Archive -Path $tempFile -DestinationPath $InstallDir -Force
    }
    catch {
        Write-Error "Failed to extract eec: $_"
        exit 1
    }
    
    # Create batch file wrapper in bin directory
    $batchContent = @"
@echo off
"$InstallDir\eec.exe" %*
"@
    $batchFile = "$BinDir\eec.bat"
    Set-Content -Path $batchFile -Value $batchContent
    
    # Clean up
    Remove-Item -Force $tempFile -ErrorAction SilentlyContinue
}

# Function to update PATH if needed
function Update-Path {
    $currentPath = [Environment]::GetEnvironmentVariable("PATH", "User")
    
    if ($currentPath -notlike "*$BinDir*") {
        Write-Info "Adding $BinDir to user PATH"
        
        $newPath = if ($currentPath) { "$BinDir;$currentPath" } else { $BinDir }
        [Environment]::SetEnvironmentVariable("PATH", $newPath, "User")
        
        # Update current session PATH
        $env:PATH = "$BinDir;$env:PATH"
        
        Write-Warning "PATH updated. You may need to restart your terminal for changes to take effect."
    }
}

# Function to verify installation
function Test-Installation {
    $eecPath = "$BinDir\eec.bat"
    
    if (Test-Path $eecPath) {
        Write-Success "eec installed successfully!"
        Write-Info "Installation location: $InstallDir"
        Write-Info "Binary location: $eecPath"
        
        # Try to get version
        try {
            $version = & $eecPath --version 2>$null
            if ($version) {
                Write-Info "eec version: $version"
            }
        }
        catch {
            Write-Info "Version info not available"
        }
        
        Write-Host ""
        Write-Success "🎉 eec is ready to use!"
        Write-Host ""
        Write-Host "Try running:"
        Write-Host "  eec --help"
        Write-Host "  eec -i  # Interactive mode"
        Write-Host ""
    }
    else {
        Write-Error "Installation verification failed"
        exit 1
    }
}

# Main installation process
function Main {
    Write-Host "eec Windows Installer"
    Write-Host "===================="
    Write-Host ""
    
    # Check for PowerShell version
    if ($PSVersionTable.PSVersion.Major -lt 5) {
        Write-Error "PowerShell 5.0 or later is required"
        exit 1
    }
    
    # Get latest version
    $version = Get-LatestVersion
    Write-Info "Latest version: $version"
    
    # Install eec
    Install-ECEngine -Version $version
    
    # Update PATH
    Update-Path
    
    # Verify installation
    Test-Installation
}

# Run main function
Main
