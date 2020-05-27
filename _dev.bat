@echo off
cls

:: Start Visual Studio
start HealthCheck.sln

:: Start Visual Studio Code
cd HealthCheck.Frontend
code .

::pause
