@echo off
cls

:: Start Visual Studio
start HealthCheck.sln

cd HealthCheck.Frontend

:: Start Visual Studio Code
code .

:: Run yarn watch
start "" "C:\Program Files\Git\git-bash.exe" -c "yarn watch"

::pause
