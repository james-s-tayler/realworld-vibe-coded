#!/usr/bin/env bash
# Cake build script for Unix/Linux/macOS

set -e

SCRIPT_DIR=$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )
TOOLS_DIR=$SCRIPT_DIR/tools
CAKE_VERSION=4.0.0
CAKE_DLL=$TOOLS_DIR/Cake.dll

if [ "$CAKE_VERSION" = "" ]; then
    echo "An error occurred while parsing Cake / .NET Core version."
    exit 1
fi

# Make sure the tools folder exist.
if [ ! -d "$TOOLS_DIR" ]; then
  mkdir "$TOOLS_DIR"
fi

###########################################################################
# INSTALL CAKE
###########################################################################

CAKE_INSTALLED_VERSION=$(dotnet-cake --version 2>&1)

if [ "$?" -ne 0 ] || [ "$CAKE_INSTALLED_VERSION" != "$CAKE_VERSION" ]; then
    if [ ! -f "$CAKE_DLL" ] || [ "$CAKE_INSTALLED_VERSION" != "$CAKE_VERSION" ]; then
        echo "Installing Cake $CAKE_VERSION..."
        dotnet tool install --global Cake.Tool --version $CAKE_VERSION
        if [ $? -ne 0 ]; then
            echo "An error occurred while installing Cake."
            exit 1
        fi
    fi

    # Make sure that Cake has been installed.
    if [ ! -f "$CAKE_DLL" ]; then
        echo "Could not find Cake.dll at '$CAKE_DLL'."
        exit 1
    fi
else
    CAKE_EXE=$(which dotnet-cake)
fi

###########################################################################
# RUN BUILD SCRIPT
###########################################################################

echo "Running build script..."
exec dotnet-cake ./build.cake "$@"