@echo off
setlocal enabledelayedexpansion

echo ========================================================
echo    Building AudioCinematicBoost (Cinema Playback Edition)
echo ========================================================

:: 1. Close any running instance of AudioCinematicBoost so file is not locked
taskkill /IM AudioCinematicBoost.exe /F >nul 2>&1
timeout /t 1 /nobreak >nul 2>&1

:: 2. Search for Roslyn csc.exe in Visual Studio / Build Tools
set "CSC_PATH="

for %%P in (
    "C:\Program Files (x86)\Microsoft Visual Studio\2022\BuildTools\MSBuild\Current\Bin\Roslyn\csc.exe"
    "C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\Roslyn\csc.exe"
    "C:\Program Files\Microsoft Visual Studio\2022\Professional\MSBuild\Current\Bin\Roslyn\csc.exe"
    "C:\Program Files\Microsoft Visual Studio\2022\Enterprise\MSBuild\Current\Bin\Roslyn\csc.exe"
    "C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\Roslyn\csc.exe"
    "C:\Program Files (x86)\Microsoft Visual Studio\2019\BuildTools\MSBuild\Current\Bin\Roslyn\csc.exe"
    "C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe"
) do (
    if exist %%P (
        set "CSC_PATH=%%~P"
        goto :FoundCompiler
    )
)

:FoundCompiler
if "%CSC_PATH%"=="" (
    echo [ERROR] No C# compiler found on this system.
    echo Please install Visual Studio Build Tools or .NET SDK.
    pause
    exit /b 1
)

echo [INFO] Using compiler: %CSC_PATH%
echo [INFO] Compiling AudioCinematicBoost.exe...

"%CSC_PATH%" -target:winexe -win32manifest:app.manifest -win32icon:app.ico -resource:assets\app.ico,app.ico -resource:assets\logo.png,logo.png -platform:anycpu -optimize+ -nullable:enable -r:System.dll,System.Core.dll,System.Windows.Forms.dll,System.Drawing.dll -out:AudioCinematicBoost.exe Program.cs AudioProcessor.cs AudioEngineHealer.cs SettingsForm.cs TrayAppContext.cs AppResources.cs

if %ERRORLEVEL% equ 0 (
    echo ========================================================
    echo  [SUCCESS] AudioCinematicBoost.exe built successfully!
    echo ========================================================
) else (
    echo ========================================================
    echo  [ERROR] Compilation failed.
    echo ========================================================
)

if "%1" neq "--nopause" pause
