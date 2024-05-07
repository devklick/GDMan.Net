#!/bin/bash

zip_path="$HOME/Downloads/gdman.zip"
install_dir="$HOME/gdman"

# Download the latest release information from GitHub API
response=$(curl -s https://api.github.com/repos/devklick/GDMan/releases/latest)

# Extract the browser download URL for the win-x64.zip asset
download_url=$(echo "$response" | jq -r '.assets[] | select(.name | test("linux-x64.zip")) | .browser_download_url')

# Download the zip file to the Downloads directory
wget "$download_url" -O "$zip_path"

# Extract the contents of the zip file to the AppData directory
unzip -o "$zip_path" -d "$install_dir"

# Remove the downloaded zip file
rm "$zip_path"

# Add gdman directory to the PATH if not already present
if [[ ! ":$PATH:" == *":$install_dir:"* ]]; then
    echo 'export PATH="$install_dir:$PATH"' >> "$HOME/.bashrc"
    if [ -e "$HOME/.zshrc" ]; then
        echo 'export PATH="$install_dir:$PATH"' >> "$HOME/.bashrc"
    fi
    export PATH="$install_dir:$PATH"
fi
