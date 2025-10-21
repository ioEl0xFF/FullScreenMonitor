@echo off
chcp 65001 >nul
setlocal enabledelayedexpansion

:: 管理者権限チェック
net session >nul 2>&1
if %errorLevel% neq 0 (
    echo This uninstaller requires administrator privileges.
    echo Please run as administrator.
    pause
    exit /b 1
)

:: アプリケーション情報
set "APP_NAME=FullScreenMonitor"

:: アーキテクチャ検出
if "%PROCESSOR_ARCHITECTURE%"=="AMD64" (
    set "ARCH=x64"
    set "INSTALL_DIR=%ProgramFiles%\%APP_NAME%"
) else (
    set "ARCH=x86"
    set "INSTALL_DIR=%ProgramFiles(x86)%\%APP_NAME%"
)

echo ========================================
echo %APP_NAME% Uninstaller
echo ========================================
echo.
echo Architecture: %ARCH%
echo Installation directory: %INSTALL_DIR%
echo.

:: インストール確認
if not exist "%INSTALL_DIR%" (
    echo Error: %APP_NAME% is not installed.
    echo Installation directory: %INSTALL_DIR%
    pause
    exit /b 1
)

:: アンインストール確認
echo Warning: This operation will completely remove %APP_NAME%.
echo.
set /p "CONFIRM_UNINSTALL=Do you want to proceed with uninstallation? (y/N): "
if /i not "!CONFIRM_UNINSTALL!"=="y" (
    echo Uninstallation cancelled.
    pause
    exit /b 0
)

:: プロセスの終了
echo Terminating application...
taskkill /f /im "FullScreenMonitor.exe" >nul 2>&1
timeout /t 2 /nobreak >nul

:: スタートアップ登録の削除
echo Removing startup registration...
reg delete "HKCU\Software\Microsoft\Windows\CurrentVersion\Run" /v "%APP_NAME%" /f >nul 2>&1

:: デスクトップショートカットの削除
echo Removing desktop shortcut...
if exist "%USERPROFILE%\Desktop\%APP_NAME%.lnk" (
    del "%USERPROFILE%\Desktop\%APP_NAME%.lnk" 2>nul
)

:: スタートメニューショートカットの削除
echo Removing Start Menu shortcuts...
set "START_MENU_DIR=%APPDATA%\Microsoft\Windows\Start Menu\Programs"
if exist "%START_MENU_DIR%\%APP_NAME%.lnk" (
    del "%START_MENU_DIR%\%APP_NAME%.lnk" 2>nul
)
if exist "%START_MENU_DIR%\%APP_NAME% (Uninstall).lnk" (
    del "%START_MENU_DIR%\%APP_NAME% (Uninstall).lnk" 2>nul
)

:: 設定ファイルの削除確認
echo.
set "SETTINGS_DIR=%APPDATA%\%APP_NAME%"
if exist "%SETTINGS_DIR%" (
    echo Settings files found: %SETTINGS_DIR%
    set /p "DELETE_SETTINGS=Do you want to delete settings files as well? (y/N): "
    if /i "!DELETE_SETTINGS!"=="y" (
        echo Deleting settings files...
        rmdir /s /q "%SETTINGS_DIR%" 2>nul
        if exist "%SETTINGS_DIR%" (
            echo Warning: Some settings files could not be deleted.
            echo Please delete manually: %SETTINGS_DIR%
        ) else (
            echo Settings files deleted.
        )
    ) else (
        echo Settings files preserved: %SETTINGS_DIR%
    )
)

:: アプリケーションフォルダの削除
echo Removing application files...
rmdir /s /q "%INSTALL_DIR%" 2>nul
if exist "%INSTALL_DIR%" (
    echo Error: Failed to delete application folder.
    echo Please delete manually: %INSTALL_DIR%
    echo.
    echo The following files may be in use:
    echo - FullScreenMonitor.exe is running
    echo - Antivirus software is scanning files
    echo - Other processes are accessing files
    echo.
    pause
    exit /b 1
)

:: レジストリのクリーンアップ（アプリケーション固有の設定）
echo Cleaning up registry...
reg delete "HKCU\Software\%APP_NAME%" /f >nul 2>&1

:: アンインストール完了
echo.
echo ========================================
echo Uninstallation completed successfully!
echo ========================================
echo.
echo %APP_NAME% has been successfully uninstalled.
echo.
echo The following items have been removed:
echo - Application files: %INSTALL_DIR%
echo - Startup registration
echo - Desktop shortcut
echo - Start Menu shortcuts
if /i "!DELETE_SETTINGS!"=="y" (
    echo - Settings files: %SETTINGS_DIR%
) else (
    echo - Settings files: Preserved
)
echo.
echo Thank you for using %APP_NAME%.
pause
exit /b 0