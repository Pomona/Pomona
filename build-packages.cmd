@echo off

echo "Building Nuget packages"

del build\*.nupkg
mkdir build

call "%VS140COMNTOOLS%\VsDevCmd.bat"

msbuild Pomona.sln /target:Build /p:Configuration=Release

nuget pack -Prop Configuration=Release app\Pomona.Common\Pomona.Common.csproj -build -OutputDirectory build
nuget pack -Prop Configuration=Release app\Pomona\Pomona.csproj -build -OutputDirectory build
nuget pack -Prop Configuration=Release app\Pomona.Security\Pomona.Security.csproj -build -OutputDirectory build
nuget pack -Prop Configuration=Release app\Pomona.Scheduler\Pomona.Scheduler.csproj -build -OutputDirectory build
nuget pack -Prop Configuration=Release app\Pomona.Profiling.MiniProfiler\Pomona.Profiling.MiniProfiler.csproj -build -OutputDirectory build
nuget pack -Prop Configuration=Release app\Pomona.NHibernate3\Pomona.NHibernate3.csproj -build -OutputDirectory build
nuget pack -Prop Configuration=Release app\Pomona.NHibernate4\Pomona.NHibernate4.csproj -build -OutputDirectory build
nuget pack -Prop Configuration=Release tests\Pomona.TestHelpers\Pomona.TestHelpers.csproj -build -OutputDirectory build
nuget pack -Prop Configuration=Release tests\Pomona.TestingClient\Pomona.TestingClient.csproj -build -OutputDirectory build
