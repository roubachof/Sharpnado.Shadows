$formsVersion = "3.6.0.220655"

$netstandardProject = ".\Shadows\Shadows\Shadows.csproj"
$droidProject = ".\Shadows\Shadows.Droid\Shadows.Droid.csproj"
$iosProject = ".\Shadows\Shadows.iOS\Shadows.iOS.csproj"
$uwpProject = ".\Shadows\Shadows.UWP\Shadows.UWP.csproj"
$tizenProject = ".\Shadows\Shadows.Tizen\Shadows.Tizen.csproj"

rm *.txt

echo "  Setting Xamarin.Forms version to $formsVersion"

$findXFVersion = '(Xamarin.Forms">\s+<Version>)(.+)(</Version>)'
$replaceString = "`$1 $formsVersion `$3"

(Get-Content $netstandardProject -Raw) -replace $findXFVersion, "$replaceString" | Out-File $netstandardProject
(Get-Content $droidProject -Raw) -replace $findXFVersion, "$replaceString" | Out-File $droidProject
(Get-Content $iosProject -Raw) -replace $findXFVersion, "$replaceString" | Out-File $iosProject
(Get-Content $uwpProject -Raw) -replace $findXFVersion, "$replaceString" | Out-File $uwpProject
(Get-Content $tizenProject -Raw) -replace $findXFVersion, "$replaceString" | Out-File $tizenProject

echo "  cleaning Sharpnado.Shadows solution"
$errorCode = msbuild .\Shadows\Shadows.sln /t:Clean > clean.txt

if ($errorCode -gt 0)
{
    echo "  Error while cleaning solution, see clean.txt for infos"
    return 1
}

echo "  restoring Sharpnado.Shadows solution packages"
msbuild .\Shadows\Shadows.sln /t:Restore > restore.txt

if ($errorCode -gt 0)
{
    echo "  Error while cleaning solution, see restore.txt for infos"
    return 2
}

echo "  building Sharpnado.Shadows solution"
$errorCode = msbuild .\Shadows\Shadows.sln /t:Build /p:Configuration=Release > build.txt

if ($errorCode -gt 0)
{
    echo "  Error while cleaning solution, see build.txt for infos"
    return 3
}

echo "  building Sharpnado.Shadows.UWP x64 solution"
$errorCode = msbuild .\Shadows\Shadows.UWP\Shadows.UWP.csproj /t:Build /p:Configuration=Release /p:Platform=x64 > build.x64.txt

if ($errorCode -gt 0)
{
    echo "  Error while cleaning solution, see build.x64.txt for infos"
    return 4
}

$version = (Get-Item Shadows\Shadows\bin\Release\netstandard2.0\Sharpnado.Shadows.dll).VersionInfo.FileVersion

echo "  packaging Sharpnado.Shadows.nuspec (v$version)"
nuget pack .\Sharpnado.Shadows.nuspec -Version $version