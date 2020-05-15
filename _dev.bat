@echo off
cls

:: Start Visual Studio
start HealthCheck.sln

cd HealthCheck.Frontend

:: Run yarn watch
start "" "C:\Program Files\Git\git-bash.exe" -c "yarn; yarn watch;"

:: Start Visual Studio Code
code .


::pause
