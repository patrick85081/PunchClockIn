FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["LogonWorkOnService/LogonWorkOnService.csproj", "LogonWorkOnService/"]
RUN dotnet restore "LogonWorkOnService/LogonWorkOnService.csproj"
COPY . .
WORKDIR "/src/LogonWorkOnService"
RUN dotnet build "LogonWorkOnService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "LogonWorkOnService.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "LogonWorkOnService.dll"]
