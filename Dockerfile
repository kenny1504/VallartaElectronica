FROM node:20-bookworm-slim AS cliente
WORKDIR /src

COPY package.json package-lock.json ./
RUN npm ci

COPY . .
RUN npm run build:css

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS compilacion
WORKDIR /src

COPY ElectronicaVallarta.csproj ./
RUN dotnet restore

COPY . .
COPY --from=cliente /src/wwwroot/css/site.css /src/wwwroot/css/site.css
RUN dotnet publish -c Release -o /app/publicado /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

ENV ASPNETCORE_URLS=http://0.0.0.0:80
EXPOSE 80

COPY --from=compilacion /app/publicado ./

ENTRYPOINT ["dotnet", "ElectronicaVallarta.dll"]
