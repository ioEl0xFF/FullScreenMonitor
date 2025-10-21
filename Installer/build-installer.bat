@echo off
chcp 65001 >nul
setlocal enabledelayedexpansion

:: アプリケーション情報
set "APP_NAME=FullScreenMonitor"
set "APP_VERSION=1.0.0"

echo ========================================
echo %APP_NAME% Installer Build Script
echo ========================================
echo.

:: プロジェクトルートディレクトリの確認
if not exist "FullScreenMonitor.csproj" (
    echo Error: FullScreenMonitor.csproj not found.
    echo Please run this script from the project root directory.
    pause
    exit /b 1
)

:: 既存のpublishディレクトリのクリーンアップ
echo Cleaning up existing build files...
if exist "publish\win-x64-installer" rmdir /s /q "publish\win-x64-installer"
if exist "publish\win-x86-installer" rmdir /s /q "publish\win-x86-installer"

:: x64版のビルド
echo.
echo ========================================
echo Building x64 version...
echo ========================================
echo.
dotnet publish -c Release -r win-x64 --self-contained true -o "publish\win-x64-installer"
if %errorLevel% neq 0 (
    echo Error: Failed to build x64 version.
    pause
    exit /b 1
)

:: x86版のビルド
echo.
echo ========================================
echo Building x86 version...
echo ========================================
echo.
dotnet publish -c Release -r win-x86 --self-contained true -o "publish\win-x86-installer"
if %errorLevel% neq 0 (
    echo Error: Failed to build x86 version.
    pause
    exit /b 1
)

:: インストーラーファイルのコピー
echo.
echo ========================================
echo Copying installer files...
echo ========================================

:: x64版にインストーラーファイルをコピー
if exist "Installer\install.bat" (
    copy "Installer\install.bat" "publish\win-x64-installer\" >nul
    echo Copied install.bat to x64 version.
) else (
    echo Warning: Installer\install.bat not found.
)

if exist "Installer\uninstall.bat" (
    copy "Installer\uninstall.bat" "publish\win-x64-installer\" >nul
    echo Copied uninstall.bat to x64 version.
) else (
    echo Warning: Installer\uninstall.bat not found.
)

if exist "Installer\README_INSTALL.txt" (
    copy "Installer\README_INSTALL.txt" "publish\win-x64-installer\" >nul
    echo Copied README_INSTALL.txt to x64 version.
) else (
    echo Warning: Installer\README_INSTALL.txt not found.
)

:: x86版にインストーラーファイルをコピー
if exist "Installer\install.bat" (
    copy "Installer\install.bat" "publish\win-x86-installer\" >nul
    echo Copied install.bat to x86 version.
)

if exist "Installer\uninstall.bat" (
    copy "Installer\uninstall.bat" "publish\win-x86-installer\" >nul
    echo Copied uninstall.bat to x86 version.
)

if exist "Installer\README_INSTALL.txt" (
    copy "Installer\README_INSTALL.txt" "publish\win-x86-installer\" >nul
    echo Copied README_INSTALL.txt to x86 version.
)

:: ZIPファイルの作成
echo.
echo ========================================
echo Creating ZIP files...
echo ========================================

:: PowerShellを使用してZIPファイルを作成
set "ZIP_X64=%APP_NAME%-Installer-x64-v%APP_VERSION%.zip"
set "ZIP_X86=%APP_NAME%-Installer-x86-v%APP_VERSION%.zip"

echo Creating x64 ZIP file: %ZIP_X64%
powershell -Command "Compress-Archive -Path 'publish\win-x64-installer\*' -DestinationPath 'publish\%ZIP_X64%' -Force"
if %errorLevel% neq 0 (
    echo Error: Failed to create x64 ZIP file.
) else (
    echo Created x64 ZIP file: publish\%ZIP_X64%
)

echo Creating x86 ZIP file: %ZIP_X86%
powershell -Command "Compress-Archive -Path 'publish\win-x86-installer\*' -DestinationPath 'publish\%ZIP_X86%' -Force"
if %errorLevel% neq 0 (
    echo Error: Failed to create x86 ZIP file.
) else (
    echo Created x86 ZIP file: publish\%ZIP_X86%
)

:: ファイルサイズの表示
echo.
echo ========================================
echo Build Completed
echo ========================================
echo.

if exist "publish\%ZIP_X64%" (
    for %%A in ("publish\%ZIP_X64%") do echo x64 version: %%~nxA (%%~zA bytes)
)

if exist "publish\%ZIP_X86%" (
    for %%A in ("publish\%ZIP_X86%") do echo x86 version: %%~nxA (%%~zA bytes)
)

echo.
echo Installer packages have been created successfully.
echo.
echo Distribution files:
echo - publish\%ZIP_X64%
echo - publish\%ZIP_X86%
echo.
echo Usage:
echo 1. Download the appropriate architecture ZIP file
echo 2. Extract the ZIP file
echo 3. Run install.bat as administrator
echo.
pause
exit /b 0