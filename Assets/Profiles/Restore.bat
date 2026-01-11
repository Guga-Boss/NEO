@echo off

set BASE_DIR=%~dp0
set SOURCE="%BASE_DIR%profile 0_backup"
set DEST="%BASE_DIR%profile 0"

if not exist %SOURCE% exit /b

if exist %DEST% rmdir /s /q %DEST%

xcopy %SOURCE% %DEST% /E /I /H /K /Y > nul

exit
