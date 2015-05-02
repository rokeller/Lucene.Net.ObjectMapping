@echo off

if "%KEYFILE%" == "" (
    echo Error: The key file is not specified.
    goto :EOF
)

set nuget=%~dp0\NuGet\NuGet.exe
set src=%~dp0\..\src

pushd %src%\Lucene.Net.ObjectMapping

msbuild /p:Configuration=Release;Platform=AnyCPU /m Lucene.Net.ObjectMapping.csproj /t:Clean,Build

echo Sign the output assembly ...
sn -R bin\Release\Lucene.Net.ObjectMapping.dll %KEYFILE%

echo Verify that the output assembly was signed ...
sn -v bin\Release\Lucene.Net.ObjectMapping.dll

popd
