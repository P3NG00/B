@echo off
set /p driveLetter=Enter drive letter: 
set git=%driveLetter%:\installations\PortableGit\bin\git.exe
echo use %%git%% to reference '%git%'
