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
COPY docker-entrypoint.sh /app/docker-entrypoint.sh

RUN mkdir -p /app/wwwroot/uploads/publicidad \
    && mkdir -p /app/seed/uploads/publicidad \
    && if [ -f /app/wwwroot/uploads/publicidad/tasas.svg ]; then cp /app/wwwroot/uploads/publicidad/tasas.svg /app/seed/uploads/publicidad/tasas.svg; fi \
    && sed -i 's/\r$//' /app/docker-entrypoint.sh \
    && chmod -R 775 /app/wwwroot/uploads \
    && chmod +x /app/docker-entrypoint.sh

ENTRYPOINT ["/app/docker-entrypoint.sh"]
