@echo off

dotnet publish "./LogonService/LogonService.csproj" ^
 -p:PublishSingleFile=true ^
 -p:PublishReadyToRun=false ^
 -p:PublishTrimmed=false ^
 -r win-x64 ^
 -c Release ^
 -o Release/Services ^
 -f net6.0 --self-contained
