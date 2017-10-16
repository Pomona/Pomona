@echo off

echo "Building Nuget packages"

del build\*.nupkg
mkdir build

call "%VS140COMNTOOLS%\VsDevCmd.bat"

msbuild Pomona.sln /target:Build /p:Configuration=Release

nuget pack -Prop Configuration=Release app\Pomona.Common\Pomona.Common.csproj -OutputDirectory build
nuget pack -Prop Configuration=Release app\Pomona\Pomona.csproj -OutputDirectory build
nuget pack -Prop Configuration=Release app\Pomona.Nancy\Pomona.Nancy.csproj -OutputDirectory build
nuget pack -Prop Configuration=Release app\Pomona.Security\Pomona.Security.csproj -OutputDirectory build
nuget pack -Prop Configuration=Release app\Pomona.Scheduler\Pomona.Scheduler.csproj -OutputDirectory build
nuget pack -Prop Configuration=Release app\Pomona.Profiling.MiniProfiler\Pomona.Profiling.MiniProfiler.csproj -OutputDirectory build
nuget pack -Prop Configuration=Release app\Pomona.NHibernate3\Pomona.NHibernate3.csproj -OutputDirectory build
nuget pack -Prop Configuration=Release app\Pomona.NHibernate4\Pomona.NHibernate4.csproj -OutputDirectory build
nuget pack -Prop Configuration=Release tests\Pomona.TestHelpers\Pomona.TestHelpers.csproj -OutputDirectory build
