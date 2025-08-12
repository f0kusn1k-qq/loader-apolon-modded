@echo off

:: Удаляем автозапуск (Run)
reg delete "HKCU\Software\Microsoft\Windows\CurrentVersion\Run" /v SpaceX /f

:: Удаляем весь раздел SpaceX с настройками и модулями
reg delete "HKCU\Software\SpaceX" /f

pause >nul