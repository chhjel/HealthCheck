@echo off
cls

:: Start Visual Studio
start HealthCheck.sln

:: Start Visual Studio Code
cd HealthCheck.Frontend
code .

:: Start terminal
"C:\Program Files\Git\git-bash.exe" --cd=./HealthCheck.Frontend

::pause
