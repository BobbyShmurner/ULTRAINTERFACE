param (
	[Switch] $Help,
	[Switch] $Release,
	[Switch] $DontInstall,

	[Switch] $DontBuildExampleMod,
	[Switch] $DontBuildUltrainterface
)

function Write-Help {
	Write-Output " -Release`t`tBuilds in release mode"
	Write-Output " -DontInstall`t`tWon't install the NuGet Package or the Example mod"
	Write-Output " -Help`t`t`tShows this message"
}

function Write-Status {
	param ($message)

	$Host.UI.RawUI.ForegroundColor = "Cyan"
	Write-Output $message
	$Host.UI.RawUI.ForegroundColor = $OriginalColor
}

function Write-Error {
	param ($message)

	$Host.UI.RawUI.ForegroundColor = "Red"
	Write-Output $message
	$Host.UI.RawUI.ForegroundColor = $OriginalColor
}

function Exit-Fail {
	Write-Error "`n -- Build FAILED! --"
	exit 1
}

function Test-Nuget {
	Write-Status " - Testing NuGet Install"
	$NuGetTest = Get-Command "$NuGetPath" -ErrorAction 'SilentlyContinue'

	if ($NuGetTest.Length -eq 0) {
		Write-Error "NuGet Not Found!`n`nFailed to run `"$NuGetPath`"`nPlease download NuGet and ensure it has been added to PATH, or that you've specified the correct path in the `"config.ps1`" script"
		Exit-Fail
	}
}

function Setup-Package-Dirs {
	Write-Status " - Removing Files"

	Remove-Item -Recurse -Force $LocalNuGetSource/ultrainterface/0.0.1/ -ErrorAction 'SilentlyContinue'
	Remove-Item -Recurse -Force $NuGetPackageCache/ultrainterface/0.0.1/ -ErrorAction 'SilentlyContinue'
	Remove-Item -Recurse -Force ./ULTRAINTERFACE/Package/contentFiles/ -ErrorAction 'SilentlyContinue'

	Remove-Item -Force ./ULTRAINTERFACE/Package/ULTRAINTERFACE.nupkg -ErrorAction 'SilentlyContinue'
	Remove-Item -Force ./ULTRAINTERFACE/resources/ultrainterface -ErrorAction 'SilentlyContinue'

	Write-Status " - Making Directories"

	New-Item ./ULTRAINTERFACE/Package/contentFiles/any/any/resources/ -ItemType Directory | Out-Null
	New-Item ./ULTRAINTERFACE/Package/contentFiles/any/any/src/ -ItemType Directory | Out-Null
	New-Item ./ULTRAINTERFACE/resources/ -ItemType Directory -ErrorAction 'SilentlyContinue' | Out-Null
}

function Wait-For-Asset-Bundles {
	if (Test-Path "./UnityProject/build.lock") {
		Write-Status "`n -- Waiting for Asset Bundles to build --`n"

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
	Write-Status " - Copying Package Files"

	Copy-Item ./UnityProject/Assets/StreamingAssets/ultrainterface ./ULTRAINTERFACE/resources/
	Copy-Item ./ULTRAINTERFACE/resources/* ./ULTRAINTERFACE/Package/contentFiles/any/any/resources/ -Recurse
	Copy-Item ./ULTRAINTERFACE/src/* ./ULTRAINTERFACE/Package/contentFiles/any/any/src/ -Recurse
}

function Create-Package {
	Write-Status " - Creating NuGet Package: `n"

	. $NuGetPath pack ./ULTRAINTERFACE/Package/ULTRAINTERFACE.nuspec -OutputDirectory ./ULTRAINTERFACE/Package/ -OutputFileNamesWithoutVersion
	if ($LASTEXITCODE -ne 0) {
		Write-Error "`nFailed to pack the NuGet Package!"
		Exit-Fail
	}

	Write-Output ""
}

function Build-Package {
	Wait-For-Asset-Bundles
	Check-For-Asset-Bundles

	Setup-Package-Dirs
	Copy-Package-Files

	Create-Package
}

function Install-Package {
	Write-Status " - Installing NuGet Package: `n"

	. $NuGetPath add ./ULTRAINTERFACE/Package/ULTRAINTERFACE.nupkg -Source $LocalNuGetSource
	if ($LASTEXITCODE -ne 0) {
		Write-Error "`nFailed to install the NuGet Package!"
		Exit-Fail
	}

	Write-Output ""
}

function Clean-Up-Build {
	Write-Status " - Cleaning up"

	Remove-Item -Recurse -Force ./ULTRAINTERFACE/Package/contentFiles/ -ErrorAction 'SilentlyContinue'
	Remove-Item -Force ./ULTRAINTERFACE/resources/ultrainterface -ErrorAction 'SilentlyContinue'
}

function Restore-Example-Mod {
	Write-Status " - Restoring Example Mod: `n"

	$LocalNuGetSource = (Resolve-Path $LocalNuGetSource).Path
	dotnet restore .\ExampleUI\ExampleUI.csproj -r win-x64 -s "https://nuget.bepinex.dev/v3/index.json" -s "https://api.nuget.org/v3/index.json" -s "$LocalNuGetSource"

	if ($LASTEXITCODE -ne 0) {
		Write-Error "`nFailed to restore dependencies for the Example Mod!"
		Exit-Fail
	}

	Write-Output ""
}

function Build-Example-Mod {
	Write-Status " - Building Example Mod: `n"

	if ($Release) {
		dotnet publish ./ExampleUI/ExampleUI.csproj --no-restore -c Release -r win-x64
	} else {
		dotnet build ./ExampleUI/ExampleUI.csproj --no-restore -r win-x64
	}
	
	if ($LASTEXITCODE -ne 0) {
		Write-Error "`nFailed to build the Example Mod!"
		Exit-Fail
	}

	Write-Output ""
}

function Link-Source-To-NuGet-Install {
	Write-Status " - Linking NuGet Files to Source Files"

	Remove-Item -Recurse -Force $NuGetPackageCache/ultrainterface/0.0.1/contentFiles/any/any/src/ -ErrorAction 'SilentlyContinue'
	New-Item -ItemType Junction -Path $NuGetPackageCache/ultrainterface/0.0.1/contentFiles/any/any/src -Target (Resolve-Path ./ULTRAINTERFACE/src/) -ErrorAction 'Stop' | Out-Null
}

function Install-Example-Mod {
	New-Item $UltrakillInstall/BepInEx/scripts -ItemType Directory -ErrorAction 'SilentlyContinue' | Out-Null

	Write-Status " - Copying Example Mod to Scripts Folder"
	if ($Release) {
		Copy-Item ./ExampleUI/bin/Release/net471/win-x64/publish/ExampleUI.dll $UltrakillInstall/BepInEx/scripts
	} else {
		Copy-Item ./ExampleUI/bin/Debug/net471/win-x64/ExampleUI.dll $UltrakillInstall/BepInEx/scripts
	}
}

function Main {
	$OriginalColor = $Host.UI.RawUI.ForegroundColor
	. ./config.ps1

	if ($Help) {
		Write-Help
		exit 0
	}

	Test-Nuget

	$NuGetPackageCache = ((.$NuGetPath locals global-packages -list) -replace ".*global-packages: ").TrimEnd('\').TrimEnd('/')

	if (!$DontBuildUltrainterface) {
		Build-Package

		if (!$DontInstall) {
			Install-Package
		}

		Clean-Up-Build
	}

	if (!$DontBuildExampleMod) {
		Restore-Example-Mod

		if (!$DontInstall) {
			Link-Source-To-NuGet-Install
		}
		
		Build-Example-Mod

		if (!$DontInstall) {
			if (!(Test-Path $UltrakillInstall)) {
				Write-Error " - Could not find ULTRAKILL install at `"$UltrakillInstall`"!"
				Write-Error " - Cannot copy the ExampleUI mod to the scripts folder"
				Write-Error " - Please specify the correct path in the `"config.ps1`" script to allow for auto-install of the mod"
			} else {
				Install-Example-Mod
			}
		}

	} else {
		if (!$DontInstall) {
			Link-Source-To-NuGet-Install
		}
	}

	$Host.UI.RawUI.ForegroundColor = "Green"
	Write-Output "`n -- Build Complete! --"
	$Host.UI.RawUI.ForegroundColor = $OriginalColor
}

Main