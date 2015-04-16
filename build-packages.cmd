

echo "Building Nuget packages"

del build\*.nupkg
mkdir build


nuget pack -Prop Configuration=Release app\Pomona.Common\Pomona.Common.csproj -build -symbols -OutputDirectory build
nuget pack -Prop Configuration=Release app\Pomona\Pomona.csproj -build -symbols -OutputDirectory build
nuget pack -Prop Configuration=Release app\Pomona.Security\Pomona.Security.csproj -build -symbols -OutputDirectory build
nuget pack -Prop Configuration=Release app\Pomona.Scheduler\Pomona.Scheduler.csproj -build -symbols -OutputDirectory build
nuget pack -Prop Configuration=Release app\Pomona.Profiling.MiniProfiler\Pomona.Profiling.MiniProfiler.csproj -build -symbols -OutputDirectory build
nuget pack -Prop Configuration=Release app\Pomona.NHibernate3\Pomona.NHibernate3.csproj -build -symbols -OutputDirectory build
nuget pack -Prop Configuration=Release app\Pomona.NHibernate4\Pomona.NHibernate4.csproj -build -symbols -OutputDirectory build
nuget pack -Prop Configuration=Release tests\Pomona.TestHelpers\Pomona.TestHelpers.csproj -build -symbols -OutputDirectory build
nuget pack -Prop Configuration=Release tests\Pomona.TestingClient\Pomona.TestingClient.csproj -build -symbols -OutputDirectory build
