#!/bin/sh
set -e

mkdir -p /app/wwwroot/uploads/publicidad

for archivo in tasas.svg tasas-post.svg; do
  if [ ! -f /app/wwwroot/uploads/publicidad/$archivo ] && [ -f /app/seed/uploads/publicidad/$archivo ]; then
    cp /app/seed/uploads/publicidad/$archivo /app/wwwroot/uploads/publicidad/$archivo
  fi
done

chmod -R 775 /app/wwwroot/uploads || true

exec dotnet ElectronicaVallarta.dll
