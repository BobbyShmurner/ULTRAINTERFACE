param (
	[Switch] $Release,
	[Switch] $DontInstallLocally,
	[String] $Version = "0.0.1"
)

# ---- Config ---- #

$UltrakillInstall = "/ssd/Steam/steamapps/common/ULTRAKILL" # The path to your ULTRAKILL install
$LocalNuGetSource = "./LocalNuGetSource" # Where to store your local nuget cache (for some reason this is also required on top of the nuget cache)

# -- End Config -- #

$NuGetPackageCache = ((nuget locals global-packages -list) -replace ".*global-packages: ").TrimEnd('\').TrimEnd('/')
$OriginalColor = $Host.UI.RawUI.ForegroundColor

Write-Output "- Removing Files"

Remove-Item -Recurse -Force $LocalNuGetSource/ultrainterface/$Version/ -ErrorAction 'SilentlyContinue'
Remove-Item -Recurse -Force $NuGetPackageCache/ultrainterface/$Version/ -ErrorAction 'SilentlyContinue'
Remove-Item -Recurse -Force ./ULTRAINTERFACE/Package/contentFiles/ -ErrorAction 'SilentlyContinue'

Remove-Item -Force ./ULTRAINTERFACE/Package/ULTRAINTERFACE.nupkg -ErrorAction 'SilentlyContinue'
Remove-Item -Force ./ULTRAINTERFACE/resources/ultrainterface -ErrorAction 'SilentlyContinue'

Write-Output "- Making Directories"

New-Item ./ULTRAINTERFACE/Package/contentFiles/any/any/resources/ -ItemType Directory | Out-Null
New-Item ./ULTRAINTERFACE/Package/contentFiles/any/any/src/ -ItemType Directory | Out-Null

if (Test-Path "./UnityProject/build.lock") {
	Write-Output "`n-- Waiting for Asset Bundles to build --`n"

	while (Test-Path "./UnityProject/build.lock") { Start-Sleep -Milliseconds 100 }
}

Write-Output "- Copying Files"

Copy-Item ./UnityProject/Assets/StreamingAssets/ultrainterface ./ULTRAINTERFACE/resources/
Copy-Item ./ULTRAINTERFACE/resources/* ./ULTRAINTERFACE/Package/contentFiles/any/any/resources/ -Recurse
Copy-Item ./ULTRAINTERFACE/src/* ./ULTRAINTERFACE/Package/contentFiles/any/any/src/ -Recurse

Write-Output "- Creating and Installing NuGet Package: `n"

nuget pack ./ULTRAINTERFACE/Package/ULTRAINTERFACE.nuspec -OutputDirectory ./ULTRAINTERFACE/Package/ -OutputFileNamesWithoutVersion
$packExitCode = $LASTEXITCODE

if (!$DontInstallLocally) {
	nuget add ./ULTRAINTERFACE/Package/ULTRAINTERFACE.nupkg -Source $LocalNuGetSource
	$installExitCode = $LASTEXITCODE
} else {
	$installExitCode = 0
}

Write-Output "`n- Cleaning up"

Remove-Item -Recurse -Force ./ULTRAINTERFACE/Package/contentFiles/ -ErrorAction 'SilentlyContinue'

if ($packExitCode -ne 0 -or $installExitCode -ne 0) {
	$Host.UI.RawUI.ForegroundColor = "Red"
	Write-Output "`n-- Build FAILED! --"
	$Host.UI.RawUI.ForegroundColor = $OriginalColor

	exit 1
}

if (!$DontInstallLocally) {
	Write-Output "- Adding local source to NuGet.Config"

	[xml] $doc = Get-Content("./ExampleUI/NuGet.Config")

	$newNode = $doc.CreateElement("add")

	$keyAttribute = $doc.CreateAttribute("key")
	$keyAttribute.Value = "ULTRAINTERFACE"

	$valueAttribute = $doc.CreateAttribute("value")
	$valueAttribute.Value = (Resolve-Path $LocalNuGetSource)

	$newNode.Attributes.Append($keyAttribute) | Out-Null
	$newNode.Attributes.Append($valueAttribute) | Out-Null

	$packageSources = $doc.configuration.packageSources
	$packageSources.AppendChild($newNode) | Out-Null

	$doc.Save((Resolve-Path "./ExampleUI/NuGet.Config")) | Out-Null
}

Write-Output "- Building Example Mod: `n"

if ($Release) {
	dotnet publish ./ExampleUI/ExampleUI.csproj -c Release -r win-x64
} else {
	dotnet build ./ExampleUI/ExampleUI.csproj -r win-x64
}

$buildExitCode = $LASTEXITCODE

if (!$DontInstallLocally) {
	Write-Output "`n- Reverting changes to NuGet.Config"

	$packageSources.RemoveChild($newNode) | Out-Null
	$doc.Save((Resolve-Path "./ExampleUI/NuGet.Config")) | Out-Null

	Write-Output "- Linking NuGet Files to Source Files"

	Remove-Item -Recurse -Force $NuGetPackageCache/ultrainterface/$Version/contentFiles/any/any/src/ -ErrorAction 'SilentlyContinue'
	New-Item -ItemType SymbolicLink -Path $NuGetPackageCache/ultrainterface/$Version/contentFiles/any/any/src -Target (Resolve-Path ./ULTRAINTERFACE/src/) | Out-Null
}

if (!(Test-Path $UltrakillInstall)) {
	$Host.UI.RawUI.ForegroundColor = "Red"
	Write-Output "`n- Could not find ULTRAKILL install at `"$UltrakillInstall`"!"
	Write-Output "- Cannot copy the ExampleUI mod to the scripts folder"
	Write-Output "- Please specify the correct path at the top of this script to allow for auto-install of the mod"
	$Host.UI.RawUI.ForegroundColor = $OriginalColor
} else {
	Write-Output "- Copying Example Mod to Scripts Folder"
	if ($Release) {
		Copy-Item ./ExampleUI/bin/Release/net471/win-x64/publish/ExampleUI.dll $UltrakillInstall/BepInEx/scripts
	} else {
		Copy-Item ./ExampleUI/bin/Debug/net471/win-x64/ExampleUI.dll $UltrakillInstall/BepInEx/scripts
	}
}

if ($buildExitCode -ne 0) {
	$Host.UI.RawUI.ForegroundColor = "Red"
	Write-Output "`n-- Build FAILED! --"
	$Host.UI.RawUI.ForegroundColor = $OriginalColor

	exit 1
} else {
	$Host.UI.RawUI.ForegroundColor = "Green"
	Write-Output "`n-- Build Complete! --"
	$Host.UI.RawUI.ForegroundColor = $OriginalColor
}
