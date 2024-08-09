$z = Join-Path -Path $env:USERPROFILE -ChildPath "Downloads\gdman.zip"
$d = Join-Path -Path $env:USERPROFILE -ChildPath "gdman"

Write-Host Finding latest version
$r = (iwr -useb https://api.github.com/repos/devklick/GDMan/releases/latest | ConvertFrom-Json)
$a = $r.assets | Where-Object { $_.name -match 'win-x64.zip' }
$u = $a | Select-Object -ExpandProperty browser_download_url
$v = $r.tag_name
Write-Host Found $v

Write-Host Downloading
Invoke-WebRequest -Uri $u -OutFile $z

Write-Host Extracting
Expand-Archive -Path $z -DestinationPath $d -Force

Remove-Item -Path $z
Write-Host Removing archive

if (-not ($env:PATH -split ";" -contains $d)) {
    Write-Host Adding gdman directory to PATH
    $env:PATH = "$env:PATH;$d"
    [System.Environment]::SetEnvironmentVariable("PATH", $env:PATH, [System.EnvironmentVariableTarget]::User)
}

Write-Host 
Write-Host GDMan $v installed successfully
Write-Host "For information on usage, invoke:"
Write-Host 
Write-Host gdman --help