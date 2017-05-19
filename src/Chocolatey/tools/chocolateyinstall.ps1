$ErrorActionPreference = 'Stop';
$packageName = 'carnac'
$url = 'Download Url Here'
$zipFileHash = 'Zip File Hash Here'
$toolsDir = "$(Split-Path -parent $MyInvocation.MyCommand.Definition)"
$installDir = "$toolsDir\Carnac"

# Download carnac package from GitHub
Install-ChocolateyZipPackage "$packageName" "$url" "$installDir" -Checksum $zipFileHash -ChecksumType 'sha256'
# Run the Squirrel.Windows installer to install carnac
Install-ChocolateyInstallPackage "$packageName" -FileType "exe" -File "$installDir\\Setup.exe"
