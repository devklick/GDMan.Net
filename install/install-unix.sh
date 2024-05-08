#!/bin/bash

target_os=$1
zip_path="$HOME/Downloads/gdman.zip"
install_dir="$HOME/gdman"

# Download the latest release information from GitHub API
echo "Finding latest version"
response=$(curl -s https://api.github.com/repos/devklick/GDMan/releases/latest)
echo $response

# Extract the browser download URL for the asset
download_url=$(echo "$response" | jq -r '.assets[] | select(.name | test("'${target_os}'")) | .browser_download_url')
echo "Found '$download_url'";

# Download the zip file to the Downloads directory
echo "Downloading"
wget -q "$download_url" -O "$zip_path"

# Extract the contents of the zip file to the installation directory
echo "Extracting"
unzip -qq -o "$zip_path" -d "$install_dir"

# Remove the downloaded zip file
rm "$zip_path"
echo "Removing archive"

# Add gdman directory to the PATH if not already present
if [[ ! ":$PATH:" == *":$install_dir:"* ]]; then
    echo "Adding gdman directory to PATH"
    export PATH="$install_dir:$PATH"
    echo 'export PATH="'${install_dir}':$PATH"' >> "$HOME/.bashrc"

    if [ -e "$HOME/.zshrc" ]; then
        echo 'export PATH="'${install_dir}':$PATH"' >> "$HOME/.zshrc"
    else
        echo ".zshrc not found"
    fi
fi
