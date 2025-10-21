FullScreenMonitor Installation Guide
=====================================

■ Overview
FullScreenMonitor is a Windows resident application that automatically minimizes
other windows on the same monitor when a specified application enters full screen mode.

■ System Requirements
- OS: Windows 10/11
- Architecture: x64 or x86
- Administrator privileges: Required for installation/uninstallation

■ Installation Instructions

1. Architecture Check
   Check your PC's architecture:
   - 64-bit Windows: Use x64 version
   - 32-bit Windows: Use x86 version

   How to check:
   - Control Panel → System → System type
   - Or run "systeminfo" in Command Prompt

2. Installation Steps
   a) Download the appropriate architecture ZIP file
   b) Extract the ZIP file to any folder
   c) Right-click on "install.bat"
   d) Select "Run as administrator"
   e) Follow the installer instructions

3. Installation Options
   - Create desktop shortcut: Recommended
   - Register to startup: Recommended (auto-start with Windows)

■ Uninstallation Instructions

1. Uninstallation Steps
   a) Select "FullScreenMonitor (Uninstall)" from Start Menu
   b) Or run "uninstall.bat" as administrator from installation folder
   c) Follow the uninstaller instructions

2. Uninstallation Options
   - Delete settings files: Choose as needed
     (If not deleted, settings will be preserved for reinstallation)

■ Usage Instructions

1. First Launch
   - When you start the application, an icon will appear in the system tray
   - Double-click the tray icon or right-click → "Open Settings" to open settings

2. Configuration Options
   - Monitored processes: Process names of applications to monitor for full screen
     (Examples: chrome, firefox, msedge, notepad)
   - Monitoring interval: Check interval for full screen state (100ms-2000ms)
   - Startup registration: Whether to auto-start the application with Windows

3. Basic Operations
   - System tray icon:
     * Double-click: Open settings
     * Right-click: Context menu (Open Settings, Exit)
   - Settings window: Add/remove monitored processes, change various settings

■ Troubleshooting

1. Installation Issues

   Q: "Administrator privileges required" message appears
   A: Right-click on install.bat and select "Run as administrator"

   Q: "FullScreenMonitor.exe not found" message appears
   A: Place install.bat in the same folder as the application files

   Q: Failed to create installation directory
   A: Antivirus software may be blocking file access. Temporarily disable it and try again

2. Uninstallation Issues

   Q: Failed to delete application folder
   A: Try the following steps:
      - Ensure FullScreenMonitor.exe is not running
      - Terminate the process in Task Manager
      - Temporarily disable antivirus software
      - Delete the folder manually

   Q: Settings files cannot be deleted
   A: Delete manually: %AppData%\FullScreenMonitor\

3. Runtime Issues

   Q: Monitored processes are not detected
   A: Check the following:
      - Process name is correct (case insensitive)
      - Application is actually in full screen mode
      - Monitoring interval is appropriate (default: 500ms)

   Q: Some windows are not minimized
   A: This may be due to:
      - System windows and special windows are excluded from minimization
      - Windows on different monitors are only minimized on the target monitor
      - Some applications restrict minimization

   Q: Settings are not saved
   A: Check the following:
      - Write permissions to %AppData%\FullScreenMonitor\ folder
      - Antivirus software is not blocking file access

■ File Structure

Post-installation file structure:
- Installation directory: %ProgramFiles%\FullScreenMonitor\ (x64) or %ProgramFiles(x86)%\FullScreenMonitor\ (x86)
- Settings file: %AppData%\FullScreenMonitor\settings.json
- Startup registration: HKCU\Software\Microsoft\Windows\CurrentVersion\Run

■ Support

If problems persist, please check:
- README.md for detailed descriptions
- GitHub Issues page for known issues
- Re-verify system requirements

■ License

This software is released under the MIT License.

■ Update History

v1.0.0 (2024)
- Initial release
- Basic full screen monitoring functionality
- System tray resident
- Settings window
- Startup registration functionality
- Installer/uninstaller