$ErrorActionPreference = 'Stop';
$packageName = 'carnac'
$url = 'https://github.com/Code52/carnac/releases/download/1.0.0/carnac.1.0.0.zip'
$zipFileHash = 'ca64d790c9e11474e6716262e05014daba3e869b7e2f438f23944f5f90a67064'
$toolsDir = "$(Split-Path -parent $MyInvocation.MyCommand.Definition)"
$installDir = "$toolsDir\Carnac"

# Download carnac package from GitHub
Install-ChocolateyZipPackage "$packageName" "$url" "$installDir" -Checksum $zipFileHash -ChecksumType 'sha256'
# Run the Squirrel.Windows installer to install carnac
Install-ChocolateyInstallPackage "$packageName" -FileType "exe" -File "$installDir\\Setup.exe"
