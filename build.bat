@echo off

echo Building PowerMode...
dotnet publish -c Release -r win-x64 --self-contained false -p:PublishSingleFile=true
if %ERRORLEVEL% neq 0 goto :error

echo Writing build info...
for /f "tokens=*" %%T in ('powershell -Command "Get-Date -Format \"yyyy-MM-dd HH:mm\""') do set "BUILDTIME=%%T"
for /f "tokens=*" %%S in ('powershell -Command "Get-Date -Format \"yyyyMMdd-HHmm\""') do set "BUILDSTAMP=%%S"
echo Built %BUILDTIME% > "bin\Release\net8.0-windows\win-x64\publish\BUILD_INFO.txt"

echo Zipping...
powershell -Command "Get-ChildItem -Path 'bin\Release\net8.0-windows\win-x64\publish' -Exclude '*.pdb' | Compress-Archive -DestinationPath 'PowerMode-%BUILDSTAMP%.zip' -Force"

echo.
echo Build successful!
echo   PowerMode-%BUILDSTAMP%.zip
exit /b 0

:error
echo.
echo Build failed!
exit /b 1
