@@ -1,12 +1,12 @@
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY . .

RUN dotnet restore Rafiq.API/Rafiq.API.csproj
RUN dotnet publish Rafiq.API/Rafiq.API.csproj -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app .

