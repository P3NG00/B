
::
:: ***REMOVED*** ***REMOVED***
:: P3
::
:: Created
:: 2021.11.29
::
:: Edited
:: 2021.11.29
::

@echo off

:: TODO pick random color set (make more nice color sets)
color fc

mode 18,4

:: Echo this while loading, cuz it may take a while to load from USB
echo.
echo.   Loading
echo.   Project_B...

start "" "Y:\\installations\\VSCode-win32-x64-1.62.3\\Code.exe" "Y:\\B\\B.code-workspace"
