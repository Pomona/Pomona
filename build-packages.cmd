

echo "Building Nuget packages"
.nuget\nuget.exe pack Pomona.Common\Pomona.Common.csproj -build
.nuget\nuget.exe pack Pomona\Pomona.csproj -build
