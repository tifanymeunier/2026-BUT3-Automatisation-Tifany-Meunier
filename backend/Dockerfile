FROM mcr.microsoft.com/dotnet/sdk:10.0 as build
WORKDIR /app

COPY src/TaskList.Api/TaskList.Api.csproj src/TaskList.Api/

RUN dotnet restore src/TaskList.Api/TaskList.Api.csproj

COPY src/ src/

RUN dotnet publish src/TaskList.Api/TaskList.Api.csproj -c Release -o /out




FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app

RUN apt update
RUN apt-get install -y curl

COPY --from=build /out .

ENV ASPNETCORE_URLS=http://+:8080

HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
  CMD curl -f http://localhost:8080/api/v1/health || exit 1

USER $APP_UID

WORKDIR /out

ENTRYPOINT ["dotnet", "Api.dll"]


