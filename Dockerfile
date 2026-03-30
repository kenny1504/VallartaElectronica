# =========================
# Etapa 1: Frontend (Tailwind)
# =========================
FROM node:20-bookworm-slim AS cliente
WORKDIR /src

COPY package.json package-lock.json ./
RUN npm ci

COPY . .
RUN npm run build:css

# =========================
# Etapa 2: Build .NET
# =========================
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS compilacion
WORKDIR /src

COPY ElectronicaVallarta.csproj ./
RUN dotnet restore

COPY . .

# Copiar CSS generado desde Node
COPY --from=cliente /src/wwwroot/css/site.css /src/wwwroot/css/site.css

RUN dotnet publish -c Release -o /app/publicado /p:UseAppHost=false

# =========================
# Etapa 3: Runtime
# =========================
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

#  Puerto unificado
ENV ASPNETCORE_URLS=http://0.0.0.0:8080

EXPOSE 8080

COPY --from=compilacion /app/publicado ./

ENTRYPOINT ["dotnet", "ElectronicaVallarta.dll"]