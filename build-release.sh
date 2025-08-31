#!/bin/bash
# Build script for ECEngine releases
# Usage: ./build-release.sh [version]

set -e

VERSION=${1:-"1.0.0"}
BUILD_DIR="./release-build"
PLATFORMS=("linux-x64" "osx-x64" "osx-arm64" "win-x64")

echo "Building ECEngine v$VERSION for all platforms..."
echo "=============================================="

# Clean build directory
rm -rf "$BUILD_DIR"
mkdir -p "$BUILD_DIR"

# Build for each platform
for platform in "${PLATFORMS[@]}"; do
    echo ""
    echo "Building for $platform..."
    
    # Create platform directory
    platform_dir="$BUILD_DIR/$platform"
    mkdir -p "$platform_dir"
    
    # Build the project
    dotnet publish -c Release -r "$platform" --self-contained true \
        -p:PublishAot=true \
        -p:StripSymbols=true \
        -p:Version="$VERSION" \
        -p:AssemblyVersion="$VERSION" \
        -p:FileVersion="$VERSION" \
        -o "$platform_dir"
    
    # Create archive
    cd "$BUILD_DIR"
    if [[ "$platform" == "win-x64" ]]; then
        # Create ZIP for Windows
        cd "$platform"
        zip -r "../ecengine-windows-x64.zip" .
        cd ..
    else
        # Create tar.gz for Unix-like systems
        if [[ "$platform" == "osx-x64" ]]; then
            archive_name="ecengine-darwin-x64.tar.gz"
        elif [[ "$platform" == "osx-arm64" ]]; then
            archive_name="ecengine-darwin-arm64.tar.gz"
        else
            archive_name="ecengine-$platform.tar.gz"
        fi
        
        tar -czf "$archive_name" -C "$platform" .
    fi
    cd ..
done

echo ""
echo "Build complete! Archives created in $BUILD_DIR:"
ls -la "$BUILD_DIR"/*.{tar.gz,zip} 2>/dev/null || echo "No archives found"

echo ""
echo "Test the binaries:"
for platform in "${PLATFORMS[@]}"; do
    binary_path="$BUILD_DIR/$platform/eec"
    if [[ "$platform" == "win-x64" ]]; then
        binary_path="$BUILD_DIR/$platform/eec.exe"
    fi
    
    if [[ -f "$binary_path" ]]; then
        echo "  $platform: $binary_path"
    fi
done

echo ""
echo "To test locally, extract an archive and run:"
echo "  ./eec --version"
echo "  ./eec --help"
