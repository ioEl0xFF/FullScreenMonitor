@echo off
chcp 65001 >nul
setlocal enabledelayedexpansion

echo [DEBUG] Starting installer...
echo [DEBUG] Current directory: %CD%
echo [DEBUG] Running user: %USERNAME%
echo.

:: 管理者権限チェック
echo [DEBUG] Checking administrator privileges...
net session >nul 2>&1
if %errorLevel% neq 0 (
    echo [ERROR] This installer requires administrator privileges.
    echo [ERROR] Please run as administrator.
    echo.
    echo Solution:
    echo 1. Right-click on install.bat
    echo 2. Select "Run as administrator"
    echo.
    pause
    exit /b 1
)
echo [DEBUG] Administrator privileges confirmed

:: アプリケーション情報
echo [DEBUG] Setting application information...
set "APP_NAME=FullScreenMonitor"
set "APP_VERSION=1.0.0"
set "APP_DESCRIPTION=Full Screen Monitoring Application"
echo [DEBUG] Application name: %APP_NAME%

:: アーキテクチャ検出
echo [DEBUG] Detecting architecture...
echo [DEBUG] PROCESSOR_ARCHITECTURE: %PROCESSOR_ARCHITECTURE%
if "%PROCESSOR_ARCHITECTURE%"=="AMD64" (
    set "ARCH=x64"
    set "INSTALL_DIR=%ProgramFiles%\%APP_NAME%"
    echo [DEBUG] x64 architecture detected
) else (
    set "ARCH=x86"
    set "INSTALL_DIR=%ProgramFiles(x86)%\%APP_NAME%"
    echo [DEBUG] x86 architecture detected
)
echo [DEBUG] Installation directory: %INSTALL_DIR%

echo ========================================
echo %APP_NAME% Installer v%APP_VERSION%
echo ========================================
echo.
echo Architecture: %ARCH%
echo Installation directory: %INSTALL_DIR%
echo.

:: 既存のインストール確認
echo [DEBUG] Checking for existing installation...
echo [DEBUG] Check target: %INSTALL_DIR%
if exist "%INSTALL_DIR%" (
    echo [DEBUG] Existing installation found
    echo An existing installation was found.
    set /p "OVERWRITE=Do you want to overwrite the existing installation? (y/N): "
    if /i not "!OVERWRITE!"=="y" (
        echo [DEBUG] User cancelled installation
        echo Installation cancelled.
        pause
        exit /b 0
    )
    echo [DEBUG] Removing existing installation...
    echo Removing existing installation...
    rmdir /s /q "%INSTALL_DIR%" 2>nul
    if exist "%INSTALL_DIR%" (
        echo [ERROR] Failed to remove existing installation
        echo Error: Failed to remove existing installation.
        echo Please manually delete and try again: %INSTALL_DIR%
        pause
        exit /b 1
    )
    echo [DEBUG] Existing installation removal completed
) else (
    echo [DEBUG] No existing installation found
)

:: インストールディレクトリ作成
echo [DEBUG] Creating installation directory...
echo Creating installation directory...
mkdir "%INSTALL_DIR%" 2>nul
if not exist "%INSTALL_DIR%" (
    echo [ERROR] Failed to create installation directory
    echo Error: Failed to create installation directory.
    echo Target: %INSTALL_DIR%
    pause
    exit /b 1
)
echo [DEBUG] Installation directory created: %INSTALL_DIR%

:: アプリケーションファイルのコピー
echo [DEBUG] Copying application files...
echo Copying application files...
echo [DEBUG] Current directory contents:
dir /b 2>nul
echo.
if not exist "FullScreenMonitor.exe" (
    echo [ERROR] FullScreenMonitor.exe not found
    echo Error: FullScreenMonitor.exe not found.
    echo Please place this bat file in the same folder as the application files.
    echo.
    echo Current directory: %CD%
    echo Required file: FullScreenMonitor.exe
    pause
    exit /b 1
)
echo [DEBUG] FullScreenMonitor.exe found

echo [DEBUG] Copying FullScreenMonitor.exe...
copy "FullScreenMonitor.exe" "%INSTALL_DIR%\" >nul
if %errorLevel% neq 0 (
    echo [ERROR] Failed to copy FullScreenMonitor.exe
    pause
    exit /b 1
)
echo [DEBUG] FullScreenMonitor.exe copy completed

echo [DEBUG] Copying DLL files...
copy "*.dll" "%INSTALL_DIR%\" >nul 2>nul
echo [DEBUG] DLL files copy completed

if exist "Resources" (
    echo [DEBUG] Copying Resources folder...
    xcopy "Resources" "%INSTALL_DIR%\Resources\" /E /I /Q >nul
    if %errorLevel% neq 0 (
        echo [ERROR] Failed to copy Resources folder
    ) else (
        echo [DEBUG] Resources folder copy completed
    )
) else (
    echo [DEBUG] Resources folder not found
)

:: アンインストーラーのコピー
if exist "uninstall.bat" (
    echo [DEBUG] Copying uninstall.bat...
    copy "uninstall.bat" "%INSTALL_DIR%\" >nul
    if %errorLevel% neq 0 (
        echo [ERROR] Failed to copy uninstall.bat
    ) else (
        echo [DEBUG] uninstall.bat copy completed
    )
) else (
    echo [DEBUG] uninstall.bat not found
)

:: ショートカット作成の確認
echo.
echo [DEBUG] Starting shortcut creation confirmation
set /p "CREATE_DESKTOP_SHORTCUT=Do you want to create a desktop shortcut? (Y/n): "
if /i not "!CREATE_DESKTOP_SHORTCUT!"=="n" (
    echo [DEBUG] Creating desktop shortcut...
    call :CreateShortcut "%USERPROFILE%\Desktop\%APP_NAME%.lnk" "%INSTALL_DIR%\FullScreenMonitor.exe"
    if %errorLevel% neq 0 (
        echo [ERROR] Failed to create desktop shortcut
    ) else (
        echo [DEBUG] Desktop shortcut creation completed
        echo Desktop shortcut created.
    )
) else (
    echo [DEBUG] Desktop shortcut creation skipped
)

:: スタートメニューショートカット作成
echo [DEBUG] Creating Start Menu shortcut...
echo Creating Start Menu shortcut...
set "START_MENU_DIR=%APPDATA%\Microsoft\Windows\Start Menu\Programs"
echo [DEBUG] Start Menu directory: %START_MENU_DIR%
if not exist "%START_MENU_DIR%" (
    echo [DEBUG] Creating Start Menu directory...
    mkdir "%START_MENU_DIR%" 2>nul
)
call :CreateShortcut "%START_MENU_DIR%\%APP_NAME%.lnk" "%INSTALL_DIR%\FullScreenMonitor.exe"
if %errorLevel% neq 0 (
    echo [ERROR] Failed to create Start Menu shortcut
) else (
    echo [DEBUG] Start Menu shortcut creation completed
)

:: アンインストール用ショートカット作成
if exist "%INSTALL_DIR%\uninstall.bat" (
    echo [DEBUG] Creating uninstall shortcut...
    call :CreateShortcut "%START_MENU_DIR%\%APP_NAME% (Uninstall).lnk" "%INSTALL_DIR%\uninstall.bat"
    if %errorLevel% neq 0 (
        echo [ERROR] Failed to create uninstall shortcut
    ) else (
        echo [DEBUG] Uninstall shortcut creation completed
    )
) else (
    echo [DEBUG] uninstall.bat not found, skipping uninstall shortcut
)

:: スタートアップ登録の確認
echo.
echo [DEBUG] Starting startup registration confirmation
set /p "REGISTER_STARTUP=Do you want to start the application automatically when Windows starts? (Y/n): "
if /i not "!REGISTER_STARTUP!"=="n" (
    echo [DEBUG] Registering to startup...
    reg add "HKCU\Software\Microsoft\Windows\CurrentVersion\Run" /v "%APP_NAME%" /t REG_SZ /d "\"%INSTALL_DIR%\FullScreenMonitor.exe\"" /f >nul
    if %errorLevel% neq 0 (
        echo [ERROR] Failed to register to startup
    ) else (
        echo [DEBUG] Startup registration completed
        echo Registered to startup.
    )
) else (
    echo [DEBUG] Startup registration skipped
)

:: インストール完了
echo.
echo [DEBUG] Installation process completed
echo ========================================
echo Installation completed successfully!
echo ========================================
echo.
echo Installation directory: %INSTALL_DIR%
echo.
echo [DEBUG] Starting application launch confirmation
echo Do you want to start the application now? (Y/n)
set /p "LAUNCH_APP="
if /i not "!LAUNCH_APP!"=="n" (
    echo [DEBUG] Starting application...
    start "" "%INSTALL_DIR%\FullScreenMonitor.exe"
    if %errorLevel% neq 0 (
        echo [ERROR] Failed to start application
    ) else (
        echo [DEBUG] Application startup completed
    )
) else (
    echo [DEBUG] Application startup skipped
)

echo.
echo [DEBUG] Exiting installer
echo Installation completed successfully.
pause
exit /b 0

:: ショートカット作成関数
:CreateShortcut
set "SHORTCUT_PATH=%~1"
set "TARGET_PATH=%~2"

echo [DEBUG] Starting shortcut creation function: %SHORTCUT_PATH% -> %TARGET_PATH%

:: VBScriptを使用してショートカットを作成
echo [DEBUG] Creating VBScript file...
echo Set oWS = WScript.CreateObject("WScript.Shell") > "%TEMP%\CreateShortcut.vbs"
echo sLinkFile = "%SHORTCUT_PATH%" >> "%TEMP%\CreateShortcut.vbs"
echo Set oLink = oWS.CreateShortcut(sLinkFile) >> "%TEMP%\CreateShortcut.vbs"
echo oLink.TargetPath = "%TARGET_PATH%" >> "%TEMP%\CreateShortcut.vbs"
echo oLink.WorkingDirectory = "%~dp2" >> "%TEMP%\CreateShortcut.vbs"
echo oLink.Description = "%APP_DESCRIPTION%" >> "%TEMP%\CreateShortcut.vbs"
echo oLink.Save >> "%TEMP%\CreateShortcut.vbs"

echo [DEBUG] Executing VBScript...
cscript //nologo "%TEMP%\CreateShortcut.vbs" >nul
if %errorLevel% neq 0 (
    echo [ERROR] Failed to execute VBScript
    del "%TEMP%\CreateShortcut.vbs" 2>nul
    exit /b 1
)

echo [DEBUG] Removing temporary file...
del "%TEMP%\CreateShortcut.vbs" 2>nul
echo [DEBUG] Shortcut creation completed
exit /b 0