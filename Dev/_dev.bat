:: Shortcut to fire up dev because lazy

@echo off
cls
cd ..

:: Start Visual Studio
start Toolkit.sln

cd Toolkit.Frontend

:: Start terminal
start "" "C:\Program Files\Git\git-bash.exe"

:: Start Visual Studio Code
code .

:: Start ngrok
::start D:\Programs\Ngrok\ngrok.exe http https://localhost:7241/ --host-header=localhost

