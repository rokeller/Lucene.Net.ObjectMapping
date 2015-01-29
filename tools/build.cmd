@echo off

set nuget=%~dp0\NuGet\NuGet.exe
set src=%~dp0\..\src

pushd %src%\Lucene.Net.ObjectMapping

msbuild /p:Configuration=Release;Platform=AnyCPU /m Lucene.Net.ObjectMapping.csproj /t:Clean,Build
sn -R bin\Release\Lucene.Net.ObjectMapping.dll %KEYFILE%

popd
