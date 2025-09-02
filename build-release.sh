#!/bin/bash
# Build script for ECEngine releases
# Usage: ./build-release.sh [version]

set -e

# Extract version from ECEngine.csproj if not provided as argument
if [ -z "$1" ]; then
    VERSION=$(grep '<Version>' ECEngine.csproj | sed 's/.*<Version>\(.*\)<\/Version>.*/\1/' | tr -d ' ')
    if [ -z "$VERSION" ]; then
        VERSION="1.0.0"
        echo "Warning: Could not extract version from ECEngine.csproj, using default: $VERSION"
    fi
    
    # Extract base version number (remove pre-release suffix for AssemblyVersion)
    BASE_VERSION=$(echo "$VERSION" | sed 's/-.*$//')
else
    VERSION="$1"
    BASE_VERSION=$(echo "$VERSION" | sed 's/-.*$//')
fi

BUILD_DIR="./release-build"
PLATFORMS=("osx-x64" "osx-arm64" "linux-x64" "win-x64")

echo "Building ECEngine v$VERSION for all supported platforms..."
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
    
    # Build the project with self-contained deployment (non-AOT)
    dotnet publish -c Release -r "$platform" --self-contained true \
        -p:PublishAot=false \
        -p:PublishSingleFile=true \
        -p:PublishTrimmed=false \
        -p:Version="$VERSION" \
        -p:AssemblyVersion="$BASE_VERSION" \
        -p:FileVersion="$BASE_VERSION" \
        -p:InformationalVersion="$VERSION" \
        -o "$platform_dir"
    
    # Create archive with only the essential executable file
    cd "$BUILD_DIR"
    if [[ "$platform" == "win-x64" ]]; then
        # Create ZIP for Windows (only the exe)
        cd "$platform"
        zip "../ecengine-$VERSION-windows-x64.zip" eec.exe
        cd ..
    else
        # Create tar.gz for Unix-like systems (only the executable)
        if [[ "$platform" == "osx-x64" ]]; then
            archive_name="ecengine-$VERSION-darwin-x64.tar.gz"
        elif [[ "$platform" == "osx-arm64" ]]; then
            archive_name="ecengine-$VERSION-darwin-arm64.tar.gz"
        elif [[ "$platform" == "linux-x64" ]]; then
            archive_name="ecengine-$VERSION-linux-x64.tar.gz"
        else
            archive_name="ecengine-$VERSION-$platform.tar.gz"
        fi
        
        tar -czf "$archive_name" -C "$platform" eec
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
