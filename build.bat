@echo off
setlocal

REM --- Configuration ---
set "PROJECT_NAME=FireWithMoney"
set "GAME_PATH=D:\Programs\Steam\steamapps\common\Escape from Duckov"
set "MODS_DIR=%GAME_PATH%\Duckov_Data\Mods"
set "OUTPUT_DIR=Release\%PROJECT_NAME%"

echo ==========================================
echo Building %PROJECT_NAME%...
echo ==========================================

REM Clean previous build
if exist "bin" rmdir /s /q "bin"
if exist "obj" rmdir /s /q "obj"
if exist "Release" rmdir /s /q "Release"

REM Build the project
dotnet build %PROJECT_NAME%.csproj -c Release
if %errorlevel% neq 0 (
    echo Build failed!
    pause
    exit /b %errorlevel%
)

echo.
echo ==========================================
echo Packaging Mod...
echo ==========================================

REM Create output directory
if not exist "%OUTPUT_DIR%" mkdir "%OUTPUT_DIR%"

REM Copy files
copy /Y "bin\Release\netstandard2.1\%PROJECT_NAME%.dll" "%OUTPUT_DIR%\"
copy /Y "info.ini" "%OUTPUT_DIR%\"
REM Copy Harmony dependency
copy /Y "..\Escape-From-Duckov-Coop-Mod-Preview\Shared\0Harmony.dll" "%OUTPUT_DIR%\"

echo.
echo Build success! Output is in: %OUTPUT_DIR%

REM Optional: Install to Game
set /p INSTALL="Do you want to install the mod to the game? (Y/N): "
if /i "%INSTALL%"=="Y" (
    echo.
    echo Installing to %MODS_DIR%\%PROJECT_NAME%...
    if not exist "%MODS_DIR%" mkdir "%MODS_DIR%"
    xcopy /E /I /Y "%OUTPUT_DIR%" "%MODS_DIR%\%PROJECT_NAME%"
    echo Installation complete!
)

echo.
pause
