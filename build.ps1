param (
	[Switch] $Release,
	[Switch] $DontInstallLocally,

	[Switch] $OnlyBuildPackage,
	[Switch] $OnlyBuildExampleMod
)

. ./config.ps1

$OriginalColor = $Host.UI.RawUI.ForegroundColor

function Write-Error {
	param ($message)

	$Host.UI.RawUI.ForegroundColor = "Red"
	Write-Output $message
	$Host.UI.RawUI.ForegroundColor = $OriginalColor
}

function Exit-Fail {
	Write-Error "`n-- Build FAILED! --"
	exit 1
}

function Test-Nuget {
	$NuGetTest = Get-Command "$NuGetPath" -ErrorAction 'SilentlyContinue'

	if ($NuGetTest.Length -eq 0) {
		Write-Error "NuGet Not Found!`n`nFailed to run `"$NuGetPath`"`nPlease download NuGet and ensure it has been added to PATH, or that you've specified the correct path in the `"config.ps1`" script"
		Exit-Fail
	}
}

function Setup-Package-Dirs {
	Write-Output "- Removing Files"

	Remove-Item -Recurse -Force $LocalNuGetSource/ultrainterface/0.0.1/ -ErrorAction 'SilentlyContinue'
	Remove-Item -Recurse -Force $NuGetPackageCache/ultrainterface/0.0.1/ -ErrorAction 'SilentlyContinue'
	Remove-Item -Recurse -Force ./ULTRAINTERFACE/Package/contentFiles/ -ErrorAction 'SilentlyContinue'

	Remove-Item -Force ./ULTRAINTERFACE/Package/ULTRAINTERFACE.nupkg -ErrorAction 'SilentlyContinue'
	Remove-Item -Force ./ULTRAINTERFACE/resources/ultrainterface -ErrorAction 'SilentlyContinue'

	Write-Output "- Making Directories"

	New-Item ./ULTRAINTERFACE/Package/contentFiles/any/any/resources/ -ItemType Directory | Out-Null
	New-Item ./ULTRAINTERFACE/Package/contentFiles/any/any/src/ -ItemType Directory | Out-Null
	New-Item ./ULTRAINTERFACE/resources/ -ItemType Directory -ErrorAction 'SilentlyContinue' | Out-Null
}

function Wait-For-Asset-Bundles {
	if (Test-Path "./UnityProject/build.lock") {
		Write-Output "`n-- Waiting for Asset Bundles to build --`n"

		while (Test-Path "./UnityProject/build.lock") { Start-Sleep -Milliseconds 100 }
	}
}

function Check-For-Asset-Bundles {
	if (!(Test-Path "./UnityProject/Assets/StreamingAssets/ultrainterface")) {
		Write-Error "No Asset Bundle found! Please make sure you have built the asset bundle in Unity"
		Write-Error "The Bundle should be located at `"./UnityProject/Assets/StreamingAssets/ultrainterface`""
		Exit-Fail
	}
}

function Copy-Package-Files {
	Write-Output "- Copying Package Files"

	Copy-Item ./UnityProject/Assets/StreamingAssets/ultrainterface ./ULTRAINTERFACE/resources/
	Copy-Item ./ULTRAINTERFACE/resources/* ./ULTRAINTERFACE/Package/contentFiles/any/any/resources/ -Recurse
	Copy-Item ./ULTRAINTERFACE/src/* ./ULTRAINTERFACE/Package/contentFiles/any/any/src/ -Recurse
}

function Create-Package {
	Write-Output "- Creating NuGet Package: `n"

	. $NuGetPath pack ./ULTRAINTERFACE/Package/ULTRAINTERFACE.nuspec -OutputDirectory ./ULTRAINTERFACE/Package/ -OutputFileNamesWithoutVersion
	if ($LASTEXITCODE -ne 0) {
		Write-Error "`nFailed to pack the NuGet Package!"
		Exit-Fail
	}
}

function Build-Package {
	Wait-For-Asset-Bundles
	Check-For-Asset-Bundles

	Setup-Package-Dirs
	Copy-Package-Files

	Create-Package
}

function Install-Package {
	Write-Output "- Installing NuGet Package: `n"

	. $NuGetPath add ./ULTRAINTERFACE/Package/ULTRAINTERFACE.nupkg -Source $LocalNuGetSource
	if ($LASTEXITCODE -ne 0) {
		Write-Error "`nFailed to install the NuGet Package!"
		Exit-Fail
	}
}

function Clean-Up-Build {
	Write-Output "`n- Cleaning up"

	Remove-Item -Recurse -Force ./ULTRAINTERFACE/Package/contentFiles/ -ErrorAction 'SilentlyContinue'
	Remove-Item -Force ./ULTRAINTERFACE/resources/ultrainterface -ErrorAction 'SilentlyContinue'
}

Test-Nuget

$NuGetPackageCache = ((.$NuGetPath locals global-packages -list) -replace ".*global-packages: ").TrimEnd('\').TrimEnd('/')

if (!$OnlyBuildExampleMod) {
	Build-Package

	if (!$DontInstallLocally) {
		Install-Package
	}

	Clean-Up-Build

	if (!$DontInstallLocally -and !$OnlyBuildPackage) {
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
}

if (!$OnlyBuildPackage) {
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

		Remove-Item -Recurse -Force $NuGetPackageCache/ultrainterface/0.0.1/contentFiles/any/any/src/ -ErrorAction 'SilentlyContinue'
		New-Item -ItemType Junction -Path $NuGetPackageCache/ultrainterface/0.0.1/contentFiles/any/any/src -Target (Resolve-Path ./ULTRAINTERFACE/src/) -ErrorAction 'Stop' | Out-Null
	}

	if (!(Test-Path $UltrakillInstall)) {
		$Host.UI.RawUI.ForegroundColor = "Red"
		Write-Output "`n- Could not find ULTRAKILL install at `"$UltrakillInstall`"!"
		Write-Output "- Cannot copy the ExampleUI mod to the scripts folder"
		Write-Output "- Please specify the correct path in the `"config.ps1`" script to allow for auto-install of the mod"
		$Host.UI.RawUI.ForegroundColor = $OriginalColor
	} else {
		New-Item $UltrakillInstall/BepInEx/scripts -ItemType Directory -ErrorAction 'SilentlyContinue' | Out-Null

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
} else {
	$Host.UI.RawUI.ForegroundColor = "Green"
	Write-Output "`n-- Build Complete! --"
	$Host.UI.RawUI.ForegroundColor = $OriginalColor
}