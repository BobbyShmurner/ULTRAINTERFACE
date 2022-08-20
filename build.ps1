# -------------------------------------------------------------	#
#           THIS SCRIPT WILL NOT WORK ON YOUR PC!!!				#
#																#
#    Feel free to use it as a base to your own build script		#
# Just don't even try to run this, because it simpy won't work	#
#																#
#    The only reason this script is in the repo is because		#
#     I didn't wanna loose it in case something happens to		#
#                 my local copy of the script					#
# 																#
#   Also checkout the GitHub build action for a step-by-step	#
#  look at how to build and use a local copy of ULTRAINTERFACE	#
#																#
#     If you aren't planning on developing ULTRAINTERFACE,		#
#        but instead are just looking for a build of the		#
#     master branch, head over to the Actions tab on GitHub		#
#         for a compiled version of the master branch			#
# ------------------------------------------------------------- #

$OriginalColor = $Host.UI.RawUI.ForegroundColor
$Host.UI.RawUI.ForegroundColor = "Red"

echo "# --------------------------------------------------------------------------------- #"
echo "#                    THIS SCRIPT WILL NOT WORK ON YOUR PC!!!!!                      #"
echo "#            IT IS NOT DESIGNED TO RUN ON OTHER PEOPLE'S COMPUTERS!!!!!             #"
echo "# THE ONLY REASON IT IS IN THE REPO IS BECAUSE I DON'T WANNA LOOSE THIS SCRIPT!!!!! #"
echo "#                                                                                   #"
echo "#           View the comment at the top of this script for more info                #"
echo "# --------------------------------------------------------------------------------- #"

$Host.UI.RawUI.ForegroundColor = $OriginalColor

if ($env:HOME -ne "/home/bobby" -or $env:CAN_RUN_ULTRAINTERFACE_BUILD_SCRIPT -ne "1") {
	exit 1
}

echo "`n- Removing Files"

rm -rf ~/Documents/dev/NuGetSource/ultrainterface/0.0.1/
rm -rf /home/bobby/.nuget/packages/ultrainterface/
rm -rf ./ULTRAINTERFACE/Package/contentFiles/

rm -f ./ULTRAINTERFACE/Package/ULTRAINTERFACE.nupkg
rm -f ./ULTRAINTERFACE/resources/ultrainterface

echo "- Making Directories"

mkdir -p ./ULTRAINTERFACE/Package/contentFiles/any/any/resources/
mkdir -p ./ULTRAINTERFACE/Package/contentFiles/any/any/src/

if (Test-Path "./UnityProject/build.lock") {
	echo "`n-- Waiting for Asset Bundles to build --`n"

	while (Test-Path "./UnityProject/build.lock") { Start-Sleep -Milliseconds 100 }
}

echo "- Copying Files"

cp ./UnityProject/Assets/StreamingAssets/ultrainterface ./ULTRAINTERFACE/resources/
cp -a ./ULTRAINTERFACE/resources/. ./ULTRAINTERFACE/Package/contentFiles/any/any/resources/
cp -a ./ULTRAINTERFACE/src/. ./ULTRAINTERFACE/Package/contentFiles/any/any/src/

echo "- Creating and Installing NuGet Package: `n"

nuget pack ULTRAINTERFACE/Package/ULTRAINTERFACE.nuspec -OutputDirectory ./ULTRAINTERFACE/Package/ -OutputFileNamesWithoutVersion
nuget add ULTRAINTERFACE/Package/ULTRAINTERFACE.nupkg -Source ~/Documents/dev/NuGetSource

echo "`n- Building Example Mod: `n"

dotnet build ./ExampleUI/ExampleUI.csproj
$buildExitCode = $LASTEXITCODE

echo "`n- Linking NuGet Files to Source Files"

$cwd = (Get-Location).path

rm -rf /home/bobby/.nuget/packages/ultrainterface/0.0.1/contentFiles/any/any/src/
ln -s $cwd/ULTRAINTERFACE/src ~/.nuget/packages/ultrainterface/0.0.1/contentFiles/any/any/

echo "- Cleaning up"

rm -rf ./ULTRAINTERFACE/Package/contentFiles/
rm -f ./ULTRAINTERFACE/Package/ULTRAINTERFACE.nupkg

echo "- Copying Example Mod to Scripts Folder"

cp ./ExampleUI/bin/Debug/net471/ExampleUI.dll /ssd/Steam/steamapps/common/ULTRAKILL/BepInEx/scripts

if ($buildExitCode -ne 0) {
	$Host.UI.RawUI.ForegroundColor = "Red"
	echo "`n-- Build FAILED! --"
	$Host.UI.RawUI.ForegroundColor = $OriginalColor
} else {
	$Host.UI.RawUI.ForegroundColor = "Green"
	echo "`n-- Build Complete! --"
	$Host.UI.RawUI.ForegroundColor = $OriginalColor
}

$process = Get-Process "ULTRAKILL.exe" -ErrorAction SilentlyContinue
if (-not $process) {
	echo "`n- Launching ULTRAKILL:`n"
	steam steam://rungameid/1229490
	echo "`n- Lanched ULTRAKILL!"
}
