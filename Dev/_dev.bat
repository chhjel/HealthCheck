:: Shortcut to fire up dev because lazy

@echo off
cls
cd ..

:: Start Visual Studio
start HealthCheck.sln

cd HealthCheck.Frontend

:: Start terminal
start "" "C:\Program Files\Git\git-bash.exe"

:: Start Visual Studio Code
code .
