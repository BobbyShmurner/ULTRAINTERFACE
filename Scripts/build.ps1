param (
	[Switch] $Help,
	[Switch] $Release,
	[Switch] $BuildExampleMod,
	
	[Switch] $DontBuildLibrary,
	[Switch] $DontInstallLibrary,
	[Switch] $DontInstallExampleMod,
	[Switch] $DontUseScriptEngine
)

function Write-Help {
	Write-Output " -Release`t`t`tBuilds in Release Mode"
	Write-Output " -BuildExampleMod`t`tBuilds the Example Mod"
	Write-Output " -DontInstallLibrary`t`tWon't install the NuGet Package"
	Write-Output " -DontInstallExampleMod`t`tWon't install the Example Mod"
	Write-Output " -DontBuildLibrary`t`tWon't build the ULTRAINTERFACE Library or its NuGet Package"
	Write-Output " -DontUseScriptEngine`t`tInstall the Example Mod to `"BepInEx/plugins`" instead of `"BepInEx/scripts`""
	Write-Output ""
	Write-Output " -Help`t`t`t`tShows this message"
}

function Setup-Package-Dirs {
	Write-Status " - Removing Files"

	Remove-Item -Recurse -Force $LocalNuGetSource/ultrainterface/0.0.1/ -ErrorAction 'SilentlyContinue'
	Remove-Item -Recurse -Force $NuGetPackageCache/ultrainterface/0.0.1/ -ErrorAction 'SilentlyContinue'
	Remove-Item -Recurse -Force $PSScriptRoot/../ULTRAINTERFACE/Package/contentFiles/ -ErrorAction 'SilentlyContinue'

	Remove-Item -Force $PSScriptRoot/../ULTRAINTERFACE/Package/ULTRAINTERFACE.nupkg -ErrorAction 'SilentlyContinue'
	Remove-Item -Force $PSScriptRoot/../ULTRAINTERFACE/resources/ultrainterface -ErrorAction 'SilentlyContinue'

	Write-Status " - Making Directories"

	New-Item $PSScriptRoot/../ULTRAINTERFACE/Package/contentFiles/any/any/resources/ -ItemType Directory | Out-Null
	New-Item $PSScriptRoot/../ULTRAINTERFACE/Package/contentFiles/any/any/src/ -ItemType Directory | Out-Null
	New-Item $PSScriptRoot/../ULTRAINTERFACE/resources/ -ItemType Directory -ErrorAction 'SilentlyContinue' | Out-Null
}

function Wait-For-Asset-Bundles {
	if (Test-Path "$PSScriptRoot/../UnityProject/build.lock") {
		Write-Status "`n -- Waiting for Asset Bundles to build --`n"

		while (Test-Path "$PSScriptRoot/../UnityProject/build.lock") { Start-Sleep -Milliseconds 100 }
	}
}

function Check-For-Asset-Bundles {
	if (!(Test-Path "$PSScriptRoot/../UnityProject/Assets/StreamingAssets/ultrainterface")) {
		Write-Error "No Asset Bundle found! Please make sure you have built the asset bundle in Unity"
		Write-Error "The Bundle should be located at `"./UnityProject/Assets/StreamingAssets/ultrainterface`""
		Exit-Fail
	}
}

function Copy-Package-Files {
	Write-Status " - Copying Package Files"

	Copy-Item $PSScriptRoot/../UnityProject/Assets/StreamingAssets/ultrainterface $PSScriptRoot/../ULTRAINTERFACE/resources/
	Copy-Item $PSScriptRoot/../ULTRAINTERFACE/resources/* $PSScriptRoot/../ULTRAINTERFACE/Package/contentFiles/any/any/resources/ -Recurse
	Copy-Item $PSScriptRoot/../ULTRAINTERFACE/src/* $PSScriptRoot/../ULTRAINTERFACE/Package/contentFiles/any/any/src/ -Recurse
}

function Create-Package {
	Write-Status " - Creating NuGet Package: `n"

	. $NuGetPath pack $PSScriptRoot/../ULTRAINTERFACE/Package/ULTRAINTERFACE.nuspec -OutputDirectory $PSScriptRoot/../ULTRAINTERFACE/Package/ -OutputFileNamesWithoutVersion
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

	. $NuGetPath add $PSScriptRoot/../ULTRAINTERFACE/Package/ULTRAINTERFACE.nupkg -Source $LocalNuGetSource
	if ($LASTEXITCODE -ne 0) {
		Write-Error "`nFailed to install the NuGet Package!"
		Exit-Fail
	}

	Write-Output ""
}

function Clean-Up-Build {
	Write-Status " - Cleaning up"

	Remove-Item -Recurse -Force $PSScriptRoot/../ULTRAINTERFACE/Package/contentFiles/ -ErrorAction 'SilentlyContinue'
	Remove-Item -Force $PSScriptRoot/../ULTRAINTERFACE/resources/ultrainterface -ErrorAction 'SilentlyContinue'
}

function Restore-Example-Mod {
	Write-Status " - Restoring Example Mod: `n"

	$LocalNuGetSource = (Resolve-Path $LocalNuGetSource).Path
	dotnet restore $PSScriptRoot/../ExampleUI/ExampleUI.csproj -r win-x64 -s "https://nuget.bepinex.dev/v3/index.json" -s "https://api.nuget.org/v3/index.json" -s "$LocalNuGetSource"

	if ($LASTEXITCODE -ne 0) {
		Write-Error "`nFailed to restore dependencies for the Example Mod!"
		Exit-Fail
	}

	Write-Output ""
}

function Build-Example-Mod {
	Write-Status " - Building Example Mod: `n"

	if ($Release) {
		dotnet publish $PSScriptRoot/../ExampleUI/ExampleUI.csproj --no-restore -c Release -r win-x64
	} else {
		dotnet build $PSScriptRoot/../ExampleUI/ExampleUI.csproj --no-restore -r win-x64
	}
	
	if ($LASTEXITCODE -ne 0) {
		Write-Error "`nFailed to build the Example Mod!"
		Exit-Fail
	}

	Write-Output ""
}

function Link-Source-To-NuGet-Install {
	if ($DontBuildLibrary) { return }
	Write-Status " - Linking NuGet Files to Source Files"

	Remove-Item -Recurse -Force $NuGetPackageCache/ultrainterface/0.0.1/contentFiles/any/any/src/ -ErrorAction 'SilentlyContinue'
	New-Item -ItemType Junction -Path $NuGetPackageCache/ultrainterface/0.0.1/contentFiles/any/any/src -Target (Resolve-Path $PSScriptRoot/../ULTRAINTERFACE/src/) -ErrorAction 'Stop' | Out-Null
}

function Install-Example-Mod {
	Write-Status " - Installing Example Mod"
	New-Item $UltrakillInstall/BepInEx/scripts -ItemType Directory -ErrorAction 'SilentlyContinue' | Out-Null

	if ($DontUseScriptEngine) {
		$InstallPath = "$UltrakillInstall/BepInEx/plugins"
	} else {
		$InstallPath = "$UltrakillInstall/BepInEx/scripts"
	}

	if ($Release) {
		Copy-Item $PSScriptRoot/../ExampleUI/bin/Release/net471/win-x64/publish/ExampleUI.dll $InstallPath
	} else {
		Copy-Item $PSScriptRoot/../ExampleUI/bin/Debug/net471/win-x64/ExampleUI.dll $InstallPath
	}
}

function Main {
	if ($Help) {
		Write-Help
		exit 0
	}

	Test-Program $NuGetPath NuGet

	$NuGetPackageCache = ((.$NuGetPath locals global-packages -list) -replace ".*global-packages: ").TrimEnd('\').TrimEnd('/')

	if (!$DontBuildLibrary) {
		Build-Package

		if (!$DontInstallLibrary) {
			Install-Package
		}

		Clean-Up-Build
	}

	if ($BuildExampleMod) {
		Restore-Example-Mod

		# Linking has to happen AFTER restoring, otherwise there are permission errors when restoring
		Link-Source-To-NuGet-Install
		
		Build-Example-Mod

		if (!$DontInstallExampleMod) {
			if (!(Test-Path $UltrakillInstall)) {
				Write-Error " - Could not find ULTRAKILL install at `"$UltrakillInstall`"!"
				Write-Error " - Cannot copy the ExampleUI mod to the scripts folder"
				Write-Error " - Please specify the correct path in the `"config.ps1`" script to allow for auto-install of the mod"
			} else {
				Install-Example-Mod
			}
		}
	} else {
		Link-Source-To-NuGet-Install
	}

	$Host.UI.RawUI.ForegroundColor = "Green"
	Write-Output "`n -- Build Complete! --"
	$Host.UI.RawUI.ForegroundColor = $OriginalColor
}

. "$PSScriptRoot/common.ps1"
Main