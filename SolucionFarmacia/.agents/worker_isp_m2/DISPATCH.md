# DISPATCH — Worker ISP

You are `teamwork_preview_worker_isp` (Interface Segregation Principle Specialist).
Working Directory: `c:\Users\tomax\OneDrive\Documentos\Universidad\6to Semestre\Arquitectura de software\SolucionFarmacia\SolucionFarmacia\.agents\worker_isp_m2`
Target File to Create: `c:\Users\tomax\OneDrive\Documentos\Universidad\6to Semestre\Arquitectura de software\SolucionFarmacia\SolucionFarmacia\01-diagnostico\analisis-isp.md`

## Original Requirements
Read `c:\Users\tomax\OneDrive\Documentos\Universidad\6to Semestre\Arquitectura de software\SolucionFarmacia\SolucionFarmacia\.agents\ORIGINAL_REQUEST.md`

## Input Evidence Reports from Explorers
Read:
- `c:\Users\tomax\OneDrive\Documentos\Universidad\6to Semestre\Arquitectura de software\SolucionFarmacia\SolucionFarmacia\.agents\explorer_domain_m1\analysis.md`
- `c:\Users\tomax\OneDrive\Documentos\Universidad\6to Semestre\Arquitectura de software\SolucionFarmacia\SolucionFarmacia\.agents\explorer_services_m1\analysis.md`
- `c:\Users\tomax\OneDrive\Documentos\Universidad\6to Semestre\Arquitectura de software\SolucionFarmacia\SolucionFarmacia\.agents\explorer_console_m1\analysis.md`

## Mandatory Task
Create `01-diagnostico/analisis-isp.md` following the mandatory methodology:
1. Identify all interfaces and classes across both projects:
   - `IDescuento.cs` (1 method: `CalcularDescuento`)
   - `IServicioNotificacion.cs` (1 method: `EnviarNotificacion`)
   - Domain entities: `Persona`, `Cliente`, `Usuario`, `Producto`, `Medicamento`, `MedicamentoCapsula`, `MedicamentoLiquido`, `Laboratorio`, `Movimiento`.
   - Core services: `ServicioCliente`, `ServicioProducto`, `ServicioUsuario`, `ServicioMovimiento`.
2. Evaluate ISP:
   - **Compliance Evidence**: `IDescuento` and `IServicioNotificacion` are small, focused, single-purpose interfaces. Implementations (`ServicioDescuento`, `ServicioNotificacion`) do not contain unused or empty methods.
   - **Violation Evidence**: Total absence of interfaces for domain entities (e.g. no `IVendible`, `IStockable`, `IVencible`, `IPromocionable`).
   - **Violation Evidence**: Total absence of interfaces for key services (`ServicioProducto`, `ServicioCliente`, `ServicioUsuario`, `ServicioMovimiento`). Clients like `Program.cs` are forced to depend on fat, monolithic concrete classes containing all methods (e.g. `Program.cs` needs to read products, but must instantiate `ServicioProducto` which also includes file saving, stock validation, expiration checking, and event triggering).
3. Provide exact code snippets, exact file paths, class names, line numbers, and minimum fixes.
4. Include mandatory summary table: `Principio | ¿Cumple? | Evidencia (archivo/línea) | Fix sugerido`.
5. Write clear, professional Markdown in Spanish with at least 5 detailed findings.
