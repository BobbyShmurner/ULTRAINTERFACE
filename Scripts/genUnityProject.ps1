. "$PSScriptRoot/common.ps1"

Test-Program "$($Config.NStripPath)" "NStrip"
Test-Program "ilspycmd" "ILSpyCmd" "Please install by running the following command:`n`ndotnet tool install ilspycmd -g"