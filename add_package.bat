@echo off
set /p package_name=Enter the name of the package:
dotnet add package %package_name% --package-directory "X:\installations\.nuget\packages"
