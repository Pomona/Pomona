#!/bin/bash

cd "$(dirname "$0")"

PREDICATE="cat != TODO"
CONFIG=Debug
MSBUILD=msbuild
NUGET=nuget

for arg in "$@"; do
    case $arg in
    --debug)
        CONFIG=Debug
        ;;
    --release)
        CONFIG=Release
        ;;
    -h|--help)
        echo "Available options:"
        echo "    --release   Uses release configuration"
        echo "    --debug     Uses debug configuration (default)"
        exit 0
        ;;
    *)
        echo "ERROR: Invalid argument '$arg'" >&2
        exit 1
        ;;
    esac
done

case "$(uname -s)" in
   CYGWIN*|MINGW*|MSYS*)
     DOTNET=""
     if ! which "$MSBUILD"; then
        vswhere=packages/vswhere/tools/vswhere.exe
        if [ ! -f $vswhere ]; then
            "$NUGET" install vswhere -ExcludeVersion -Output packages
        fi
        MSBUILD="$(cygpath -u "`$vswhere -requires Microsoft.Component.MSBuild -property installationPath`"/MSBuild/*/bin/MSBuild.exe)"
     fi
     ;;
   *)
     DOTNET=mono
     PREDICATE="$PREDICATE && cat != WindowsRequired"
     ;;
esac

runner=./packages/NUnit.ConsoleRunner.3.7.0/tools/nunit3-console.exe

if [ ! -f "$runner" ]; then
    "$NUGET" restore Pomona.sln
fi

assemblies=""

for i in Pomona.UnitTests Pomona.SystemTests Pomona.IndependentClientTests Pomona.SystemTests.ClientCompatibility;
do
    assembly=tests/$i/bin/$CONFIG/$i.dll
    if [ ! -f "$assembly" ]; then
        "$MSBUILD" Pomona.sln //p:Configuration=$CONFIG
    fi
    assemblies="$assemblies $assembly"
done

$DOTNET $runner $assemblies TestResult.$CONFIG.xml --where "$PREDICATE" --labels=All
