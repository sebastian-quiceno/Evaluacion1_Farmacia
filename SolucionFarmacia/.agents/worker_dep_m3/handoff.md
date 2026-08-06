# Handoff Report — Dependency Mapping Specialist (`worker_dep_m3`)

## 1. Observation

- **Artifact Created**: `c:\Users\tomax\OneDrive\Documentos\Universidad\6to Semestre\Arquitectura de software\SolucionFarmacia\SolucionFarmacia\01-diagnostico\mapa-dependencias.md` (417 líneas, 27.9 KB).
- **Source Code Examined**: 27 archivos `.cs` en `BibFarmacia` y `AppFarmaciaConsola` + 3 archivos `.txt` de datos.
- **Direct Instantiations (`new`) Verified**:
  - `Program.cs:8-18`: `new ServicioProducto()`, `new ServicioCliente()`, `new ServicioUsuario()`, `new ServicioMovimiento()`.
  - `Program.cs:283`: `new Movimiento(...)`.
  - `ServicioCliente.cs:22`: `new EventoPuntos()`.
  - `ServicioCliente.cs:66-70`: `new Cliente(...)`.
  - `ServicioProducto.cs:23-24`: `new EventoStockMinimo()`, `new EventoVencimiento()`.
  - `ServicioProducto.cs:93-107`: `new Laboratorio(...)`, `new MedicamentoCapsula(...)`.
  - `ServicioUsuario.cs:31, 61-67`: `AspectoAutenticacion.Login`, `new Usuario(...)`.
  - `ProductoFactory.cs:19-42`: `new MedicamentoCapsula(...)`, `new MedicamentoLiquido(...)`, `DateTime.Now.AddMonths(...)`.
- **System.IO & Hardcoded Paths Verified**:
  - `ServicioCliente.cs:52, 58`: `File.Exists(ruta)`, `File.ReadAllLines(ruta)`.
  - `ServicioProducto.cs:80, 86`: `File.Exists(ruta)`, `File.ReadAllLines(ruta)`.
  - `ServicioUsuario.cs:42, 48`: `File.Exists(ruta)`, `File.ReadAllLines(ruta)`.
  - `Program.cs:79, 83, 87`: `"productos.txt"`, `"clientes.txt"`, `"usuarios.txt"`.
- **Syntactic vs Real DIP Inversion Verified**:
  - `ServicioDescuento.cs:10` implements `IDescuento`, but `Program.cs` never instantiates or injects `IDescuento`.
  - `ServicioNotificacion.cs:10` implements `IServicioNotificacion`, but line 14 directly calls `Console.WriteLine(...)` without an abstract sink or logger interface, and is not injected via DI anywhere.

## 2. Logic Chain

1. **High-Level vs Low-Level Classification**:
   - High-level business rules, domain entities, and services (`ServicioProducto`, `ServicioCliente`, `ServicioUsuario`, `ServicioMovimiento`, `ServicioDescuento`, `Persona`, `Cliente`, `Usuario`, `Producto`, `Medicamento`, `MedicamentoCapsula`, `MedicamentoLiquido`, `Laboratorio`, `Movimiento`, `IDescuento`, `IServicioNotificacion`, and UI orchestrator logic in `Program.cs`) contain the core domain policies.
   - Low-level modules (`System.IO.File`, `System.Console`, `AspectoAutenticacion`, `AspectoValidacion`, `ProductoFactory`, `DateTime.Now`, `.txt` files) contain infrastructure, OS I/O, static mechanisms, and rigid instantiations.
2. **Coupling Analysis**:
   - High-level services directly instantiate low-level infrastructure and hardcoded domain types (`new Laboratorio`, `new MedicamentoCapsula`), invoke OS file APIs (`File.ReadAllLines`), and call static aspects (`AspectoAutenticacion.Login`).
   - High-level UI (`Program.cs`) directly instantiates concrete service implementations with `new` and passes hardcoded string paths (`"productos.txt"`).
3. **DIP Evaluation**:
   - Syntactic DIP exists in only 2 minor interfaces (`IDescuento`, `IServicioNotificacion`), but lacks IoC/DI injection and real decoupling.
   - Real DIP is missing across 95% of the codebase, preventing unit testing without local files and creating high risk for Change Requests (SC-1, SC-2, SC-3).

## 3. Caveats

- No source code files in `BibFarmacia/` or `AppFarmaciaConsola/` were modified during this diagnostic phase, in full compliance with Phase 1 read-only constraints.
- Future TO-BE refactoring in Phase 2 will require introducing interface abstractions (`IProductoRepository`, `IClienteRepository`, `IUsuarioRepository`, `IDateTimeProvider`, `IEventBus`, `IServicioProducto`) and an IoC container (`Microsoft.Extensions.DependencyInjection`).

## 4. Conclusion

The document `01-diagnostico/mapa-dependencias.md` has been successfully created. It thoroughly details:
1. High-Level vs. Low-Level Module Classification tables.
2. Direct Concrete Dependencies (instantiations with `new`, static invocations, System.IO calls, hardcoded file paths).
3. Syntactic DIP vs. Real DIP gap analysis.
4. Dependency Matrix and Structural Coupling Metrics (Fan-In, Fan-Out, Instability Index $I$).
5. Mermaid Flowcharts (AS-IS flow vs. TO-BE target architecture).
6. Resumen of structural coupling and Change Request (SC-1, SC-2, SC-3) impact assessment.

## 5. Verification Method

- Inspect `01-diagnostico/mapa-dependencias.md` to confirm line references, table structures, and Mermaid diagrams.
- Verify section contents against requirement R4 of `ORIGINAL_REQUEST.md`.
