@echo off
setlocal

set OUTPUT_DIR=bin\Publish
set APP_PROJECT=src\PageLeaf\PageLeaf.csproj

echo Cleaning output directory...
if exist %OUTPUT_DIR% rd /s /q %OUTPUT_DIR%

echo Publishing PageLeaf (SingleFile, No-Self-Contained)...
dotnet publish %APP_PROJECT% -c Release -o %OUTPUT_DIR%

echo.
echo Publish completed!
echo Files are located in: %OUTPUT_DIR%
echo.

echo -----FINISH-----
pause > nul
