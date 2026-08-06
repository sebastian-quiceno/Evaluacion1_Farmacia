# Handoff Report — Worker ISP (`worker_isp_m2`)

## 1. Observation
- Verified all source C# files in `BibFarmacia` (26 files) and `AppFarmaciaConsola` (`Program.cs`, 378 lines).
- Confirmed that only 2 interfaces exist in the solution:
  - `BibFarmacia/Interfaces/IDescuento.cs` (lines 9–12): single method `CalcularDescuento`.
  - `BibFarmacia/Interfaces/IServicioNotificacion.cs` (lines 9–12): single method `EnviarNotificacion`.
- Confirmed compliance points:
  - `ServicioDescuento` (lines 11–17) and `ServicioNotificacion` (lines 10–16) implement their single-method interfaces completely without empty/dummy implementations or unused parameters.
  - Event delegates (`DelegadoStock`, `DelegadoVencimiento`, `DelegadoPuntos`, `DelegadoMovimiento`) pass simple `string mensaje` arguments.
- Confirmed violation points:
  - `BibFarmacia/Clases/Producto.cs` (lines 8–35), `Cliente.cs`, `Usuario.cs`, `Movimiento.cs`: 0 domain interfaces implemented. Clients must depend on full concrete entities carrying stock, minimum stock, and expiration properties even when only needing name or price.
  - `BibFarmacia/Servicios/ServicioProducto.cs` (lines 12–119), `ServicioCliente.cs` (lines 12–82), `ServicioUsuario.cs` (lines 12–74), `ServicioMovimiento.cs` (lines 11–39): 0 service interfaces implemented. Monolithic classes expose file persistence, list management, rule checking, and event firing.
  - `BibFarmacia/Eventos/EventoStockMinimo.cs` (lines 17–22) and `EventoVencimiento.cs` (lines 19–24): `Disparar(Producto producto)` forces passing full concrete `Producto` objects when only `producto.Nombre` is used.
  - `AppFarmaciaConsola/Program.cs` (lines 8–18, 78–87): depends on fat concrete classes with no client role segregation.
  - Inconsistent loading signatures (`CargarDesdeArchivo` vs `Cargar`) without an `IDataLoader<T>` or `IRepository<T>` interface.

## 2. Logic Chain
1. The Interface Segregation Principle (ISP) dictates that clients should not be forced to depend on methods or interfaces they do not use.
2. In `BibFarmacia`, the existing interfaces (`IDescuento`, `IServicioNotificacion`) comply perfectly with ISP as they are focused and single-purpose.
3. However, across the rest of the system, there is a total absence of interfaces for domain entities and core services.
4. Because `Producto` is an unsegregated monolithic class containing `Stock`, `StockMinimo`, and `FechaVencimiento`, any client requiring item information is forced to depend on physical inventory attributes. This directly blocks SC-2 (services like inyectología which have no stock or expiration).
5. Because `ServicioProducto` exposes 5 distinct responsibilities without segregated client interfaces (`IProductoConsultaService`, `IProductoPersistenciaService`, `IStockAlertService`), `Program.cs` is tightly coupled to concrete implementation details.
6. Passing full `Producto` instances into `EventoStockMinimo.Disparar` and `EventoVencimiento.Disparar` prevents event reuse for non-product items.
7. Therefore, creating `01-diagnostico/analisis-isp.md` with these observations, exact line numbers, logic, compliance/violation evidence, SC impact analysis, and the required summary table provides a comprehensive ISP diagnosis.

## 3. Caveats
- No code in `BibFarmacia` or `AppFarmaciaConsola` was modified in this diagnostic phase, adhering strictly to the prompt instructions ("solo leer, NO modificar").
- Proposed refactored interfaces in Section 7 of `analisis-isp.md` are recommendations for Phase 2 (TO-BE redesign).

## 4. Conclusion
The diagnosis for ISP is complete and documented in `01-diagnostico/analisis-isp.md`. The system features ideal single-method interfaces for `IDescuento` and `IServicioNotificacion`, but suffers from a systemic lack of interfaces across domain entities, core services, event dispatchers, and persistence layers, resulting in fat concrete dependencies throughout `Program.cs`.

## 5. Verification Method
- Check existence and contents of `01-diagnostico/analisis-isp.md`:
  - Contains executive summary and catalog of all classes/interfaces.
  - Contains ISP compliance evidence (`IDescuento`, `IServicioNotificacion`).
  - Contains at least 5 detailed ISP violation findings with exact file paths, line numbers, and code snippets.
  - Evaluates impact against SC-1, SC-2, and SC-3.
  - Includes mandatory summary table (`Principio | ¿Cumple? | Evidencia (archivo/línea) | Fix sugerido`).
  - Written in professional Spanish.
