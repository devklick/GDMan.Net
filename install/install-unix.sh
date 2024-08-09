#!/bin/bash

t=$1
z="$HOME/Downloads/gdman.zip"
d="$HOME/gdman"

# Find the latest release for the target OS
echo Finding latest version
r=$(curl -s https://api.github.com/repos/devklick/GDMan/releases/latest)
u=$(echo "$r" | jq -r --arg t "$t" '.assets[] | select(.name | test($t)) | .browser_download_url')
v=$(echo "$r" | jq -r '.tag_name')
echo Found $v;

# Download the zip file to the Downloads directory
echo Downloading
wget -q "$u" -O "$z"

# Extract the contents of the zip file to the installation directory
echo Extracting
unzip -qq -o "$z" -d "$d"

# Remove the downloaded zip file
rm "$z"
echo Removing archive

# Add gdman directory to the PATH if not already present
if [[ ! ":$PATH:" == *":$d:"* ]]; then
    echo "Adding gdman directory to PATH"
    export PATH="$PATH:$d"
    echo 'export PATH="$PATH:'${d}'"' >> "$HOME/.bashrc"

    if [ -e "$HOME/.zshrc" ]; then
        echo 'export PATH="$PATH:'${d}'"' >> "$HOME/.zshrc"
    else
        echo ".zshrc not found"
    fi
fi

echo 
echo GDMan $v installed successfully
echo For information on usage, invoke:
echo 
echo gdman --help