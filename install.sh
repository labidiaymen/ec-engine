#!/bin/bash
# ECEngine Installer Script
# Usage: curl -fsSL https://raw.githubusercontent.com/labidiaymen/ec-engine/main/install.sh | bash

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Configuration
REPO="labidiaymen/ec-engine"
INSTALL_DIR="$HOME/.eec"
BIN_DIR="$HOME/.local/bin"

# Create bin directory if it doesn't exist
mkdir -p "$BIN_DIR"

# Function to print colored output
print_status() {
    echo -e "${BLUE}[INFO]${NC} $1"
}

print_success() {
    echo -e "${GREEN}[SUCCESS]${NC} $1"
}

print_warning() {
    echo -e "${YELLOW}[WARNING]${NC} $1"
}

print_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

# Function to detect OS and architecture
detect_platform() {
    local os=""
    local arch=""
    
    # Detect OS
    case "$(uname -s)" in
        Linux*)
            os="linux"
            ;;
        Darwin*)
            os="darwin"
            ;;
        CYGWIN*|MINGW*|MSYS*)
            os="windows"
            ;;
        *)
            print_error "Unsupported operating system: $(uname -s)"
            exit 1
            ;;
    esac
    
    # Detect architecture
    case "$(uname -m)" in
        x86_64|amd64)
            arch="x64"
            ;;
        arm64|aarch64)
            if [ "$os" = "darwin" ]; then
                arch="arm64"
            else
                arch="x64"  # Fallback to x64 for Linux ARM
            fi
            ;;
        *)
            print_warning "Unsupported architecture: $(uname -m), falling back to x64"
            arch="x64"
            ;;
    esac
    
    echo "${os}-${arch}"
}

# Function to get the latest release version
get_latest_version() {
    local version
    version=$(curl -s "https://api.github.com/repos/$REPO/releases/latest" | grep -o '"tag_name": "[^"]*' | grep -o '[^"]*$')
    
    if [ -z "$version" ]; then
        print_error "Failed to get latest version"
        exit 1
    fi
    
    echo "$version"
}

# Function to download and install ECEngine
install_ecengine() {
    local platform="$1"
    local version="$2"
    local filename="ecengine-${platform}.tar.gz"
    local url="https://github.com/$REPO/releases/download/$version/$filename"
    
    print_status "Installing eec $version for $platform..."
    
    # Create install directory
    rm -rf "$INSTALL_DIR"
    mkdir -p "$INSTALL_DIR"
    
    # Download the release
    print_status "Downloading from $url..."
    if ! curl -fL "$url" -o "/tmp/$filename"; then
        print_error "Failed to download eec release"
        print_error "URL: $url"
        exit 1
    fi
    
    # Extract the archive
    print_status "Extracting eec..."
    if ! tar -xzf "/tmp/$filename" -C "$INSTALL_DIR"; then
        print_error "Failed to extract eec"
        exit 1
    fi
    
    # Make executable
    chmod +x "$INSTALL_DIR/eec"
    
    # Create symlink in bin directory
    ln -sf "$INSTALL_DIR/eec" "$BIN_DIR/eec"
    
    # Clean up
    rm -f "/tmp/$filename"
}

# Function to update PATH if needed
update_path() {
    local shell_profile=""
    
    # Detect shell and profile file
    case "$SHELL" in
        */bash)
            if [ -f "$HOME/.bash_profile" ]; then
                shell_profile="$HOME/.bash_profile"
            else
                shell_profile="$HOME/.bashrc"
            fi
            ;;
        */zsh)
            shell_profile="$HOME/.zshrc"
            ;;
        */fish)
            shell_profile="$HOME/.config/fish/config.fish"
            ;;
        *)
            shell_profile="$HOME/.profile"
            ;;
    esac
    
    # Check if bin directory is in PATH
    if [[ ":$PATH:" != *":$BIN_DIR:"* ]]; then
        print_status "Adding $BIN_DIR to PATH in $shell_profile"
        
        if [ "$SHELL" = */fish ]; then
            echo "set -gx PATH $BIN_DIR \$PATH" >> "$shell_profile"
        else
            echo "export PATH=\"$BIN_DIR:\$PATH\"" >> "$shell_profile"
        fi
        
        print_warning "Please restart your shell or run: source $shell_profile"
    fi
}

# Function to verify installation
verify_installation() {
    if [ -x "$BIN_DIR/eec" ]; then
        print_success "eec installed successfully!"
        print_status "Installation location: $INSTALL_DIR"
        print_status "Binary location: $BIN_DIR/eec"
        
        # Try to get version if eec is in PATH
        if command -v eec >/dev/null 2>&1; then
            print_status "eec version: $(eec --version 2>/dev/null || echo 'Version info not available')"
        fi
        
        echo ""
        print_success "🎉 eec is ready to use!"
        echo ""
        echo "Try running:"
        echo "  eec --help"
        echo "  eec -i  # Interactive mode"
        echo ""
    else
        print_error "Installation verification failed"
        exit 1
    fi
}

# Main installation process
main() {
    echo "eec Installer"
    echo "============="
    echo ""
    
    # Check for required commands
    for cmd in curl tar; do
        if ! command -v "$cmd" >/dev/null 2>&1; then
            print_error "Required command not found: $cmd"
            exit 1
        fi
    done
    
    # Detect platform
    local platform
    platform=$(detect_platform)
    print_status "Detected platform: $platform"
    
    # Get latest version
    local version
    version=$(get_latest_version)
    print_status "Latest version: $version"
    
    # Install eec
    install_ecengine "$platform" "$version"
    
    # Update PATH if needed
    update_path
    
    # Verify installation
    verify_installation
}

# Run main function
main "$@"
