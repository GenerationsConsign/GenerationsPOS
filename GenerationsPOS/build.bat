@ECHO OFF
call dotnet publish -c Debug --runtime win-x64 -f net7.0 --self-contained
pause
