$ErrorActionPreference = 'Stop';
$packageName = 'carnac'
$url = 'https://github.com/downloads/Code52/carnac/Carnac.zip'
$toolsDir = "$(Split-Path -parent $MyInvocation.MyCommand.Definition)"

# Include Carnac.exe as a GUI in the bin install and ignore vshost.exe
New-Item -ItemType file -force -path "$toolsDir\Carnac\Carnac.exe.gui" | out-null
New-Item -ItemType file -force -path "$toolsDir\Carnac\Carnac.vshost.exe.ignore" | out-null

Install-ChocolateyZipPackage "$packageName" "$url" "$toolsDir"

# Create User start menu link
$startMenuLink=$("$env:appdata\Microsoft\Windows\Start Menu\Programs\Carnac.lnk")
Install-ChocolateyShortcut -shortcutFilePath $startMenuLink -targetPath "$toolsDir\Carnac\Carnac.exe" | out-null
