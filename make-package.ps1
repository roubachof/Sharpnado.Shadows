$formsVersion = "3.6.0.220655"

$netstandardProject = ".\Shadows\Shadows\Shadows.csproj"
$droidProject = ".\Shadows\Shadows.Droid\Shadows.Droid.csproj"
$iosProject = ".\Shadows\Shadows.iOS\Shadows.iOS.csproj"
$uwpProject = ".\Shadows\Shadows.UWP\Shadows.UWP.csproj"
$tizenProject = ".\Shadows\Shadows.Tizen\Shadows.Tizen.csproj"

$droidBin = ".\Shadows\Shadows.Droid\bin\Release"
$droidObj = ".\Shadows\Shadows.Droid\obj\Release"

rm *.txt

echo "  Setting Xamarin.Forms version to $formsVersion"

$findXFVersion = '(Xamarin.Forms">\s+<Version>)(.+)(</Version>)'
$replaceString = "`$1 $formsVersion `$3"

(Get-Content $netstandardProject -Raw) -replace $findXFVersion, "$replaceString" | Out-File $netstandardProject
(Get-Content $droidProject -Raw) -replace $findXFVersion, "$replaceString" | Out-File $droidProject
(Get-Content $iosProject -Raw) -replace $findXFVersion, "$replaceString" | Out-File $iosProject
(Get-Content $uwpProject -Raw) -replace $findXFVersion, "$replaceString" | Out-File $uwpProject
(Get-Content $tizenProject -Raw) -replace $findXFVersion, "$replaceString" | Out-File $tizenProject

echo "  deleting android bin-obj folders"
rm -Force -Recurse $droidBin
rm -Force -Recurse $droidObj

echo "  cleaning Sharpnado.Shadows solution"
msbuild .\Shadows\Shadows.sln /t:Clean

if ($LastExitCode -gt 0)
{
    echo "  Error while cleaning solution"
    return
}

echo "  restoring Sharpnado.Shadows solution packages"
msbuild .\Shadows\Shadows.sln /t:Restore

if ($LastExitCode -gt 0)
{
    echo "  Error while restoring packages"
    return
}

echo "  building Sharpnado.Shadows solution"
$errorCode = msbuild .\Shadows\Shadows.sln /t:Build /p:Configuration=Release

if ($LastExitCode -gt 0)
{
    echo "  Error while building solution"
    return
}

$version = (Get-Item Shadows\Shadows\bin\Release\netstandard2.0\Sharpnado.Shadows.dll).VersionInfo.FileVersion

echo "  packaging Sharpnado.Shadows.nuspec (v$version)"
nuget pack .\Sharpnado.Shadows.nuspec -Version $version