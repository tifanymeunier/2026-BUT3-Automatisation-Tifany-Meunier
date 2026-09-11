FROM mcr.microsoft.com/dotnet/sdk:10.0

WORKDIR /app

COPY . .

RUN dotnet restore api.slnx

RUN dotnet publish src/TaskList.Api/TaskList.Api.csproj -c Release -o app/out

ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT dotnet TaskList.Api.dll