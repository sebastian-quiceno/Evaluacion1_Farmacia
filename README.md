# Sistema FARMACIA — Modernización arquitectónica (Reto técnico de ingreso)

## Roles del equipo

| Rol | Integrante |
|---|---|
| Arquitecto de dominio | Sebastian Quiceno |
| Arquitecto de dependencias | Laura Suárez |
| Ingeniero de comportamiento | Tomas Castaño |
| Integrador y evidencia | Sebastian Orcasita |

## Enlace al video

https://www.youtube.com/watch?v=FgLQRvQbLxk


## Estructura del entregable

```
00-lectura-en-frio/    Las 4 hojas de hipótesis iniciales 
01-diagnostico/        Diagrama AS-IS (.dia), mapa de dependencias, inventario de hallazgos, línea base de las 3 SC
02-diseno/              Diagrama TO-BE (.puml + .png) con leyenda de colores, y los 4 documentos de argumentación
                        (SOLID, LSP, DIP, ADR)
03-src/                 Código fuente rediseñado (compilable), programa principal, casos de caracterización
04-evidencia/           Salidas comparadas AS-IS vs. TO-BE, métrica de SC-2, bitácora de uso de IA


```

## Cómo ejecutar el sistema rediseñado (TO-BE)

Requiere .NET 8 SDK.

```bash
cd 03-src
dotnet build SolucionDefinitiva.sln
cd src/AppFarmaciaConsola
dotnet run
```

Usuario de prueba: `admin` / Contraseña: `1234` (mismas credenciales que el sistema AS-IS — `usuarios.txt` no cambió).

## Cómo reproducir la evidencia de preservación del comportamiento

```bash
cd 03-src
./ejecutar-caracterizacion.sh
```

Ejecuta los 10 casos de `03-src/casos-de-caracterizacion/` contra ambos sistemas y guarda las salidas en
`04-evidencia/salidas-comparadas/`. El resumen ya interpretado está en
`04-evidencia/Casos de Caracterizacion.md`. El script asume que ambas soluciones (`SolucionFarmacia.sln` en la raíz
y `03-src/SolucionDefinitiva.sln`) ya están compiladas.

## Qué cambió respecto al AS-IS (resumen — detalle completo en `02-diseno/` y `04-evidencia/`)

- **Comportamiento observable preservado al 100 %** en las 7 opciones de menú originales — confirmado en los 10
  casos de `04-evidencia/Casos de Caracterizacion.md`.
- **Única adición observable:** la opción **8 (Ver servicios médicos)** — consecuencia directa de implementar
  **SC-2**, la solicitud de cambio elegida y medida en esta entrega (`04-evidencia/Metrica SC-2.md`).
- **SC-1** (`ArticuloRetail`) y **SC-3** (`Convenio` y subtipos) existen en el código, compilan y son fieles al
  diagrama TO-BE (A.7.1), pero no están cableadas a un escenario interactivo en esta entrega — límite de alcance
  declarado en `04-evidencia/Bitacora de Uso de IA.md`.


