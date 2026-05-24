# Build script for Cleaner Sv installer
param(
    [string]$ProjectDir = "$PSScriptRoot\..\Cleaner",
    [string]$InstallerDir = $PSScriptRoot,
    [string]$TempDir = "$env:TEMP\CleanerSvInstaller"
)

$ErrorActionPreference = "Stop"

Write-Host "=== Cleaner Sv Installer Build ===" -ForegroundColor Cyan

# 1. Publish the app
Write-Host "[1/4] Publishing Cleaner Sv..." -ForegroundColor Yellow
$projPath = Join-Path $ProjectDir "Cleaner.csproj"
& dotnet publish $projPath -c Release -r win-x64 --self-contained false
if ($LASTEXITCODE -ne 0) { throw "Publish failed" }

# 2. Prepare temp build directory (no spaces in path)
Write-Host "[2/4] Preparing build files..." -ForegroundColor Yellow
if (Test-Path $TempDir) { Remove-Item -Path $TempDir -Recurse -Force }
New-Item -ItemType Directory -Path $TempDir -Force | Out-Null

$pubDir = Join-Path $ProjectDir "bin\Release\net8.0-windows\win-x64\publish"
& robocopy $pubDir $TempDir /E /NP /NJH /NJS
Copy-Item (Join-Path $ProjectDir "logo.ico") -Destination $TempDir
Copy-Item (Join-Path $ProjectDir "btc.txt") -Destination $TempDir
Copy-Item (Join-Path $InstallerDir "license.rtf") -Destination $TempDir

# 3. Generate installer .wxs with absolute paths
Write-Host "[3/4] Generating installer WXS..." -ForegroundColor Yellow
$wxsContent = @"
<?xml version="1.0" encoding="UTF-8"?>
<Wix xmlns="http://wixtoolset.org/schemas/v4/wxs"
     xmlns:ui="http://wixtoolset.org/schemas/v4/wxs/ui">
  <Package
    Name="Cleaner Sv"
    Manufacturer="Agdala"
    Version="1.0.0"
    UpgradeCode="12345678-1234-1234-1234-123456789abc"
    Scope="perMachine">

    <MajorUpgrade DowngradeErrorMessage="A newer version of Cleaner Sv is already installed." />

    <Icon Id="AppIcon" SourceFile="$TempDir\logo.ico" />
    <Property Id="ARPPRODUCTICON" Value="AppIcon" />

    <WixVariable Id="WixUILicenseRtf" Value="$TempDir\license.rtf" />
    <Property Id="WIXUI_INSTALLDIR" Value="INSTALLFOLDER" />

    <Media Id="1" Cabinet="CleanerSv.cab" EmbedCab="yes" CompressionLevel="high" />

    <UI>
      <ui:WixUI Id="WixUI_InstallDir" />
    </UI>

    <UIRef Id="WixUI_Common" />

    <StandardDirectory Id="ProgramFiles6432Folder">
      <Directory Id="INSTALLFOLDER" Name="Cleaner Sv">
        <Component Id="MainFiles" Guid="87654321-4321-4321-4321-210987654321">
$(Get-ChildItem $TempDir -Filter "*.exe" -Name | ForEach-Object { "          <File Id=`"$([System.IO.Path]::GetFileNameWithoutExtension($_))Exe`" Source=`"$TempDir\$_`" Vital=`"true`" />" })
$(Get-ChildItem $TempDir -Filter "*.dll" -Name | ForEach-Object { "          <File Id=`"$([System.IO.Path]::GetFileNameWithoutExtension($_) -replace '[^a-zA-Z0-9]','')`" Source=`"$TempDir\$_`" />" })
$(Get-ChildItem $TempDir -Filter "*.json" -Name | ForEach-Object { "          <File Id=`"$([System.IO.Path]::GetFileNameWithoutExtension($_))Json`" Source=`"$TempDir\$_`" />" })
          <File Id="LogoIco" Source="$TempDir\logo.ico" />
          <File Id="BtcTxt" Source="$TempDir\btc.txt" />
          <RemoveFile Id="CrashLog" Name="CleanerCrash.txt" On="uninstall" />
        </Component>
      </Directory>
    </StandardDirectory>

    <StandardDirectory Id="DesktopFolder">
      <Component Id="DesktopShortcut" Guid="b1234567-1234-1234-1234-123456789abc">
        <Shortcut Id="DesktopCleanerSv" Name="Cleaner Sv"
                  Target="[INSTALLFOLDER]Cleaner.exe" WorkingDirectory="INSTALLFOLDER"
                  Icon="AppIcon" />
        <RegistryValue Root="HKCU" Key="Software\Agdala\CleanerSv"
                       Name="desktop_shortcut" Type="integer" Value="1" KeyPath="yes" />
      </Component>
    </StandardDirectory>

    <StandardDirectory Id="ProgramMenuFolder">
      <Directory Id="CleanerSvProgramMenuDir" Name="Cleaner Sv">
        <Component Id="StartMenuShortcut" Guid="c1234567-1234-1234-1234-123456789abc">
          <Shortcut Id="StartMenuCleanerSv" Name="Cleaner Sv"
                    Target="[INSTALLFOLDER]Cleaner.exe" WorkingDirectory="INSTALLFOLDER"
                    Icon="AppIcon" />
          <Shortcut Id="UninstallShortcut" Name="Desinstalar Cleaner Sv"
                    Target="[System64Folder]msiexec.exe"
                    Arguments="/x [ProductCode]" />
          <RegistryValue Root="HKCU" Key="Software\Agdala\CleanerSv"
                         Name="startmenu_shortcut" Type="integer" Value="1" KeyPath="yes" />
        </Component>
      </Directory>
    </StandardDirectory>

    <Feature Id="Complete" Level="1">
      <ComponentRef Id="MainFiles" />
      <ComponentRef Id="DesktopShortcut" />
      <ComponentRef Id="StartMenuShortcut" />
    </Feature>
  </Package>
</Wix>
"@
Set-Content -Path "$TempDir\Installer.wxs" -Value $wxsContent -Encoding UTF8

# 4. Build the MSI
Write-Host "[4/4] Building installer MSI..." -ForegroundColor Yellow
$outputMsi = Join-Path $InstallerDir "CleanerSv.msi"
$extDll = "$env:USERPROFILE\.wix\extensions\WixToolset.UI.wixext\7.0.0\wixext7\WixToolset.UI.wixext.dll"
& wix build "$TempDir\Installer.wxs" -ext $extDll -o $outputMsi
if ($LASTEXITCODE -ne 0) { throw "WiX build failed" }

# 5. Cleanup temp
Remove-Item -Path $TempDir -Recurse -Force -ErrorAction SilentlyContinue

Write-Host "`n=== Installer created: $outputMsi ===" -ForegroundColor Green
Write-Host "Size: $((Get-Item $outputMsi).Length / 1KB) KB" -ForegroundColor Green
