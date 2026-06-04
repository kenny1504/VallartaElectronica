#!/bin/sh
set -e

mkdir -p /app/wwwroot/uploads/publicidad

if [ ! -f /app/wwwroot/uploads/publicidad/tasas.svg ] && [ -f /app/seed/uploads/publicidad/tasas.svg ]; then
  cp /app/seed/uploads/publicidad/tasas.svg /app/wwwroot/uploads/publicidad/tasas.svg
fi

chmod -R 775 /app/wwwroot/uploads || true

exec dotnet ElectronicaVallarta.dll
