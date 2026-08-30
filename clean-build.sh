#!/bin/bash

set -e

echo "Stopping .NET build servers..."
dotnet build-server shutdown

echo "Removing bin and obj..."
rm -rf bin obj

echo "Running clean build..."
dotnet build --no-incremental /p:UseSharedCompilation=false

echo ""
echo "Clean build complete."