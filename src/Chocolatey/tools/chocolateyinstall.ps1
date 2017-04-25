$ErrorActionPreference = 'Stop';
$packageName = 'carnac'
$url = 'Download Url Here'
$zipFileHash = 'Zip File Hash Here'
$toolsDir = "$(Split-Path -parent $MyInvocation.MyCommand.Definition)"
$installDir = "$toolsDir\Carnac"

# Include Carnac.exe as a GUI in the bin install and ignore vshost.exe
New-Item -ItemType file -force -path "$installDir\Carnac.exe.gui" | out-null
New-Item -ItemType file -force -path "$installDir\Carnac.vshost.exe.ignore" | out-null

Install-ChocolateyZipPackage "$packageName" "$url" "$installDir" -Checksum $zipFileHash -ChecksumType 'sha256'

# Create User start menu link
$startMenuLink=$("$env:appdata\Microsoft\Windows\Start Menu\Programs\Carnac.lnk")
Install-ChocolateyShortcut -shortcutFilePath $startMenuLink -targetPath "$installDir\Carnac.exe" | out-null
