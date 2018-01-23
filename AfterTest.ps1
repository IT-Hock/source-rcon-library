param (
	[string]$Version = "0.0.1"
)
$base_dir = split-path -parent $MyInvocation.MyCommand.Definition
$output_dir = Join-Path $base_dir ..\output
$src_dir = Join-Path $base_dir ..\RCONServerLib
$nuspec = Get-ChildItem $src_dir\*.nuspec -Recurse

Write-Host "Nuspec: $nuspec"
Remove-Item -force -Recurse $output_dir -ErrorAction SilentlyContinue > $null
New-Item -ItemType directory -Path $output_dir > $null
nuget pack $nuspec.FullName -Properties "Version=$Version" -OutputDirectory $output_dir