@echo off

:: 2022.02.16

:: Delete existing NuGet.Config file
del "%appdata%\NuGet\NuGet.Config"

:: Restore project
dotnet restore

:: Kill VSCode to restart plugins
taskkill /f /im code.exe
