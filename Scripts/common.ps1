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

function Test-Program([string]$Cmd, [string]$DisplayName) {
	$NuGetTest = Get-Command "$Cmd" -ErrorAction 'SilentlyContinue'

	if ($NuGetTest.Length -eq 0) {
		Write-Error "$DisplayName Not Found!`n`nFailed to run `"$Cmd`"`nPlease download $DisplayName and ensure it has been added to PATH, or that you've specified the correct path in the `"config.ps1`" script"
		Exit-Fail
	}
}

$OriginalColor = $Host.UI.RawUI.ForegroundColor
. "$PSScriptRoot/../config.ps1"