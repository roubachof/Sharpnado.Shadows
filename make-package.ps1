$formsVersion = "3.6.0.220655"

echo "  <<<< WARNING >>>>> You need to launch 2 times this script to make sure Xamarin.Forms version was correctly resolved..."

$netstandardProject = ".\Shadows\Shadows\Shadows.csproj"
$droidProject = ".\Shadows\Shadows.Droid\Shadows.Droid.csproj"
$iosProject = ".\Shadows\Shadows.iOS\Shadows.iOS.csproj"
$uwpProject = ".\Shadows\Shadows.UWP\Shadows.UWP.csproj"
$tizenProject = ".\Shadows\Shadows.Tizen\Shadows.Tizen.csproj"

echo "  Setting Xamarin.Forms version to $formsVersion"

$findXFVersion = '(Xamarin.Forms">\s+<Version>)(.+)(</Version>)'
$replaceString = "`$1 $formsVersion `$3"

(Get-Content $netstandardProject -Raw) -replace $findXFVersion, "$replaceString" | Out-File $netstandardProject
(Get-Content $droidProject -Raw) -replace $findXFVersion, "$replaceString" | Out-File $droidProject
(Get-Content $iosProject -Raw) -replace $findXFVersion, "$replaceString" | Out-File $iosProject
(Get-Content $uwpProject -Raw) -replace $findXFVersion, "$replaceString" | Out-File $uwpProject
(Get-Content $tizenProject -Raw) -replace $findXFVersion, "$replaceString" | Out-File $tizenProject

echo "  building Sharpnado.Shadows solution"
msbuild .\Shadows\Shadows.sln /t:Clean,Restore,Build /p:Configuration=Release > build.txt

$version = (Get-Item Shadows\Shadows\bin\Release\netstandard2.0\Sharpnado.Shadows.dll).VersionInfo.FileVersion

echo "  packaging Sharpnado.Shadows.nuspec (v$version)"
nuget pack .\Sharpnado.Shadows.nuspec -Version $version