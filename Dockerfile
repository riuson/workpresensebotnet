FROM mcr.microsoft.com/dotnet/sdk:6.0 as build-env

WORKDIR /src
COPY ./.git/ ./.git
COPY ./Sources/ ./Sources
RUN ls -la
RUN dotnet restore ./Sources/WorkPresenceBotNet.sln
RUN dotnet publish ./Sources/WorkPresenceBotNet.sln -c Release -o /out

FROM mcr.microsoft.com/dotnet/aspnet:6.0

WORKDIR /app
COPY --from=build-env /out/ /app
COPY ./appsettings* /app
ENTRYPOINT ["dotnet", "/app/ServerApp.dll"]