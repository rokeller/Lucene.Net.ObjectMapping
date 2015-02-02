@echo off

set nuget=%~dp0\NuGet\NuGet.exe
set src=%~dp0\..\src

pushd %src%\Lucene.Net.ObjectMapping

%nuget% pack Lucene.Net.ObjectMapping.csproj -Prop Configuration=Release -Symbols -IncludeReferencedProjects

popd
