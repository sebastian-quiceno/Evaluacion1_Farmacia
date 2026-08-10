#!/bin/bash
# Ejecuta cada caso de caracterización contra el sistema AS-IS (raíz del repo) y contra el
# sistema TO-BE (03-src), y guarda ambas salidas en 04-evidencia/salidas-comparadas/ para comparar.
# Requiere que ambos proyectos ya estén compilados (dotnet build).

set -e

REPO_ROOT="$(cd "$(dirname "$0")/.." && pwd)"
ASIS_DIR="$REPO_ROOT/AppFarmaciaConsola"
TOBE_DIR="$REPO_ROOT/03-src/src/AppFarmaciaConsola"
CASOS_DIR="$REPO_ROOT/03-src/casos-de-caracterizacion"
RESULT_DIR="$REPO_ROOT/04-evidencia/salidas-comparadas"

mkdir -p "$RESULT_DIR"

for caso in "$CASOS_DIR"/*.txt; do
    nombre=$(basename "$caso" .txt)

    echo "== $nombre =="

    (cd "$ASIS_DIR" && dotnet run --no-build < "$caso") > "$RESULT_DIR/$nombre-asis.txt" 2>&1 || true
    (cd "$TOBE_DIR" && dotnet run --no-build < "$caso") > "$RESULT_DIR/$nombre-tobe.txt" 2>&1 || true
done

echo "Listo. Salidas guardadas en: $RESULT_DIR"
