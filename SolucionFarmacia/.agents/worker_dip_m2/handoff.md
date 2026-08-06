# Handoff Report — Worker DIP (M2)

## 1. Observation
- Created target diagnostic report at `c:\Users\tomax\OneDrive\Documentos\Universidad\6to Semestre\Arquitectura de software\SolucionFarmacia\SolucionFarmacia\01-diagnostico\analisis-dip.md`.
- Analyzed all C# source files across `BibFarmacia` (26 .cs files) and `AppFarmaciaConsola` (`Program.cs`, 378 lines) for Dependency Inversion Principle (DIP) compliance.
- Direct code observations with exact line numbers:
  - `ServicioCliente.cs` (L47-81): `File.ReadAllLines(ruta)`, `File.Exists(ruta)`, `linea.Split(';')`, `new Cliente(...)`, `new EventoPuntos()`.
  - `ServicioProducto.cs` (L75-118): `File.ReadAllLines(ruta)`, `File.Exists(ruta)`, `new Laboratorio(...)`, `new MedicamentoCapsula(...)`, `new EventoStockMinimo()`, `new EventoVencimiento()`.
  - `ServicioUsuario.cs` (L31, L37-73): `AspectoAutenticacion.Login(usuarios, user, password)`, `File.ReadAllLines(ruta)`.
  - `ServicioNotificacion.cs` (L14): `Console.WriteLine($"[NOTIFICACION] {mensaje}")`.
  - `ProductoFactory.cs` (L24, L39): `DateTime.Now.AddMonths(...)`, `new MedicamentoCapsula(...)`, `new MedicamentoLiquido(...)`.
  - `Program.cs` (L8-18, L79, L83, L87, L283): Direct `new` instantiations for `ServicioProducto`, `ServicioCliente`, `ServicioUsuario`, `ServicioMovimiento`, `Movimiento`, and hardcoded file strings `"productos.txt"`, `"clientes.txt"`, `"usuarios.txt"`.
  - Syntactical compliance vs real DIP: `ServicioDescuento : IDescuento` and `ServicioNotificacion : IServicioNotificacion` exist, but no caller uses Dependency Injection or injects them.

## 2. Logic Chain
- **Step 1**: High-level modules (business services `ServicioCliente`, `ServicioProducto`, `ServicioUsuario`, and entry point `Program.cs`) depend directly on low-level infrastructure details (`File.ReadAllLines`, `Console.WriteLine`, static `AspectoAutenticacion`, static `DateTime.Now`, hardcoded text files).
- **Step 2**: Because these dependencies are hardcoded via direct `new` calls and static calls rather than injected via interfaces (`IProductoRepository`, `IClienteRepository`, `IAutenticador`, `IDateTimeProvider`), high-level logic cannot be tested in isolation with mocks/stubs.
- **Step 3**: Change Requests SC-1 (cosmetics/food), SC-2 (services), and SC-3 (agreements/discounts) require modifying high-level services whenever storage mechanisms, file formats, or third-party integrations change, directly violating DIP and OCP.
- **Step 4**: A comprehensive diagnostic document `analisis-dip.md` was authored including high vs low-level module classification, detailed evidence with exact line numbers, impact analysis on SC-1/2/3, mandatory summary table, and a clear TO-BE refactoring plan.

## 3. Caveats
- No caveats. All 27 C# source files across `BibFarmacia` and `AppFarmaciaConsola` were inspected and verified against DIP requirements.

## 4. Conclusion
- The target deliverable `01-diagnostico/analisis-dip.md` has been successfully created with 14 comprehensive DIP findings and full adherence to all mandatory format rules.

## 5. Verification Method
- **File Inspection**:
  - Open `01-diagnostico/analisis-dip.md` and verify it contains sections 1 to 6.
  - Verify section 3 contains the required summary table with columns: `Principio | ¿Cumple? | Evidencia (archivo/línea) | Fix sugerido`.
  - Confirm all code snippets match the actual source files in `BibFarmacia/` and `AppFarmaciaConsola/`.
