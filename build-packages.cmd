

echo "Building Nuget packages"



.nuget\nuget.exe pack Pomona.Common\Pomona.Common.csproj -build -symbols -OutputDirectory build
.nuget\nuget.exe pack Pomona\Pomona.csproj -build -symbols -OutputDirectory build
