FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["LogonService/LogonService.csproj", "LogonService/"]
RUN dotnet restore "LogonService/LogonService.csproj"
COPY . .
WORKDIR "/src/LogonWorkOnService"
RUN dotnet build "LogonService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "LogonService.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "LogonService.dll"]
