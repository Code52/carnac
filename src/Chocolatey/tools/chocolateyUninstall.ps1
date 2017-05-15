$ErrorActionPreference = 'Stop';
$packageName = 'carnac'
$installLocation = "$env:LOCALAPPDATA\$packageName"

# Uninstall carnac from Programs and Features
Uninstall-ChocolateyPackage "$packageName" -FileType "exe" -File "$installLocation\Update.exe" -SilentArgs "--uninstall"

# Remove the left over files from the Squirrel.Windows install location
Remove-Item $installLocation -Recurse -Force