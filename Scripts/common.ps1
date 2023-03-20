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

function Test-Program([string]$Cmd, [string]$DisplayName, [string]$ErrMsg = "Please download $DisplayName and ensure it has been added to PATH, or that you've specified the correct path in the `"config.ps1`" script") {
	$Test = Get-Command "$Cmd" -ErrorAction 'SilentlyContinue'

	if ($Test.Length -eq 0) {
		Write-Error "$DisplayName Not Found!`n`nFailed to run `"$Cmd`""
		Write-Error $ErrMsg
		Exit-Fail
	}
}

$OriginalColor = $Host.UI.RawUI.ForegroundColor
$Config = Get-Content "$PSScriptRoot/../config.cfg" | ConvertFrom-StringData