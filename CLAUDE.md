# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project overview

A C# .NET 8 pharmacy management exercise (academic project). A console app (`AppFarmaciaConsola`) drives a class library (`BibFarmacia`) that models products, clients, users and stock/expiration movements, backed by flat semicolon-delimited `.txt` files instead of a database.

## Common commands

```
dotnet build SolucionFarmacia.sln          # build the whole solution
dotnet run --project AppFarmaciaConsola     # run the console app (must run from AppFarmaciaConsola/ or pass --project)
```

There is no test project in the solution and no lint/format config — don't invent test or lint commands.

When running, the app reads `clientes.txt`, `productos.txt`, and `usuarios.txt` from the working directory (copied to the output dir on build) and then prompts for a username/password login (see `usuarios.txt` for valid credentials, e.g. `admin` / `1234`).

## Solution structure

- `BibFarmacia` — class library (`net8.0`) with all domain logic.
- `AppFarmaciaConsola` — console app (`net8.0`), references `BibFarmacia`, contains only `Program.cs` (top-level statements) plus the three `.txt` data files.

## Architecture

`BibFarmacia` is organized by role, not by feature:

- `Clases/` — domain models. `Persona` (abstract) → `Cliente`, `Usuario`. `Producto` (abstract) → `Medicamento` → `MedicamentoCapsula`, `MedicamentoLiquido`. Also `Laboratorio`, `Movimiento`.
- `Servicios/` — one service per aggregate (`ServicioProducto`, `ServicioCliente`, `ServicioUsuario`, `ServicioMovimiento`, `ServicioDescuento`, `ServicioNotificacion`). Each owns an in-memory `List<T>` (no persistence beyond the initial `.txt` load) and exposes `Cargar`/`CargarDesdeArchivo` to parse a `;`-delimited file into objects.
- `Eventos/` — one class per notification type (`EventoStockMinimo`, `EventoVencimiento`, `EventoPuntos`, `EventoMovimiento`), each wrapping a single C# delegate/event. Services hold an `Evento*` instance and call `.Disparar(...)` to raise it; `Program.cs` subscribes to these events and does the actual console output (color-coded per event type). This is the pub/sub seam between business logic and presentation.
- `Aspectos/` — static helper classes for cross-cutting concerns: `AspectoAutenticacion.Login(...)` (used by `ServicioUsuario.Login`), `AspectoValidacion` (`ValidarCliente`, `ValidarProducto` — currently not wired into any service, available for callers to use).
- `Factories/` — `ProductoFactory` static factory for constructing `MedicamentoCapsula`/`MedicamentoLiquido` with sensible defaults (stock mínimo, fecha de vencimiento).
- `Interfaces/` — `IDescuento` (implemented by `ServicioDescuento`), `IServicioNotificacion` (implemented by `ServicioNotificacion`).
- `Enums/` — `MaterialEnvase`, `TipoRelleno`. Note: the **folder** is `Enums/` but the **namespace** is `BibFarmacia.Enum` (singular) — always `using BibFarmacia.Enum;`, not `BibFarmacia.Enums`.

`AppFarmaciaConsola/Program.cs` wires everything together in one file: instantiate services → subscribe console-output handlers to each `Evento*` → load the three `.txt` files → login loop → menu loop (view products/clients, search product, register a sale, add client points, view stock/expiration alerts).

## Data files and gotchas

- `productos.txt`: `nombre;precio;stock;stockMinimo;fechaVencimiento;laboratorio`. `ServicioProducto.CargarDesdeArchivo` always constructs a `MedicamentoCapsula` (hardcoded `TipoRelleno.Gel`) for every row regardless of intent — there is no per-row type dispatch to `MedicamentoLiquido`.
- `clientes.txt`: `nombre;cedula;telefono;correo`.
- `usuarios.txt`: `nombre;cedula;telefono;correo;userName;password`.
- All three `Cargar`/`CargarDesdeArchivo` methods swallow parse errors into a returned message string (`ex.Message`) rather than throwing — check the returned string, don't assume success.
