

echo "Building Nuget packages"

mkdir build


.nuget\nuget.exe pack app\Pomona.Common\Pomona.Common.csproj -build -symbols -OutputDirectory build
.nuget\nuget.exe pack app\Pomona\Pomona.csproj -build -symbols -OutputDirectory build
.nuget\nuget.exe pack tests\Pomona.TestHelpers\Pomona.TestHelpers.csproj -build -symbols -OutputDirectory build
