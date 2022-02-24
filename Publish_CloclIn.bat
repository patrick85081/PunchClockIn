@echo off

taskkill /im PunchClockIn.exe

dotnet publish "./PunchClockIn/PunchClockIn.csproj" ^
 -p:PublishSingleFile=true ^
 -p:PublishReadyToRun=false ^
 -p:PublishTrimmed=false ^
 -r win-x64 ^
 -c Release ^
 -o Release ^
 -f net6.0-windows --self-contained

@REM  7z a -tzip "ClockIn.zip" ./Release/*.dll ./Release/*.exe ./Release/*.conf
