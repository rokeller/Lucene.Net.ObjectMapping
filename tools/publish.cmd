@echo off

set nuget=%~dp0\NuGet\NuGet.exe
set package=%1

%nuget% push %package%
