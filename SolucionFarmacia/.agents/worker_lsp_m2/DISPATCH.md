1: # DISPATCH — Worker LSP
2: 
3: You are `teamwork_preview_worker_lsp` (Liskov Substitution Principle Specialist).
4: Working Directory: `c:\Users\tomax\OneDrive\Documentos\Universidad\6to Semestre\Arquitectura de software\SolucionFarmacia\SolucionFarmacia\.agents\worker_lsp_m2`
5: Target File to Create: `c:\Users\tomax\OneDrive\Documentos\Universidad\6to Semestre\Arquitectura de software\SolucionFarmacia\SolucionFarmacia\01-diagnostico\analisis-lsp.md`
6: 
7: ## Original Requirements
8: Read `c:\Users\tomax\OneDrive\Documentos\Universidad\6to Semestre\Arquitectura de software\SolucionFarmacia\SolucionFarmacia\.agents\ORIGINAL_REQUEST.md`
9: 
10: ## Input Evidence Reports from Explorers
11: Read:
12: - `c:\Users\tomax\OneDrive\Documentos\Universidad\6to Semestre\Arquitectura de software\SolucionFarmacia\SolucionFarmacia\.agents\explorer_domain_m1\analysis.md`
13: - `c:\Users\tomax\OneDrive\Documentos\Universidad\6to Semestre\Arquitectura de software\SolucionFarmacia\SolucionFarmacia\.agents\explorer_services_m1\analysis.md`
14: - `c:\Users\tomax\OneDrive\Documentos\Universidad\6to Semestre\Arquitectura de software\SolucionFarmacia\SolucionFarmacia\.agents\explorer_console_m1\analysis.md`
15: 
16: ## Mandatory Task
17: Create `01-diagnostico/analisis-lsp.md` following the mandatory methodology:
18: 1. Identify relevant hierarchies across both projects:
19:    - `Persona` (abstract) -> `Cliente`, `Usuario`
20:    - `Producto` (abstract) -> `Medicamento` -> `MedicamentoCapsula`, `MedicamentoLiquido`
21:    - `IDescuento` -> `ServicioDescuento`
22:    - `IServicioNotificacion` -> `ServicioNotificacion`
23: 2. Evaluate LSP: Do any subclasses throw exceptions, ignore inherited methods, or break the expected contract?
24:    - `Producto.cs` (L12, L14): Enforces physical `Stock` and `FechaVencimiento` on all derived products. Health services (SC-2: inyectología, curaciones) do not have physical stock or expiration dates. Treating services as `Producto` forces dummy values or broken contracts.
25:    - `Medicamento.cs` (L11): Enforces `Laboratorio` property on all medications. Non-pharmaceutical items (SC-1: cosmetics, beverages) do not have a pharmaceutical laboratory, breaking expectations when treated as `Medicamento`.
26:    - `Producto.cs` (L29-34): `MostrarInformacion()` is NOT overridden in `MedicamentoCapsula` or `MedicamentoLiquido`, so calling `MostrarInformacion()` polymorphically on a list of `Producto` omits subtype details (`MaterialEnvase`, `TipoRelleno`).
27:    - `Program.cs` (L280-281): Direct stock deduction `productoVenta.Stock -= cantidad` assumes all `Producto` instances have mutable physical inventory.
28:    - Syntactical compliance: `Persona` -> `Cliente`/`Usuario`, `ServicioDescuento`, and `ServicioNotificacion` adhere to signatures without throwing `NotImplementedException`, but lack contract pre/post-conditions (e.g. negative discount check).
29: 3. Provide exact code snippets, exact file paths, class names, line numbers, and minimum fixes.
30: 4. Include mandatory summary table: `Principio | ¿Cumple? | Evidencia (archivo/línea) | Fix sugerido`.
31: 5. Write clear, professional Markdown in Spanish with at least 5 detailed findings.
32: 
33: ## 2026-08-05T10:54:51Z
34: Read your dispatch instructions in c:\Users\tomax\OneDrive\Documentos\Universidad\6to Semestre\Arquitectura de software\SolucionFarmacia\SolucionFarmacia\.agents\worker_lsp_m2\DISPATCH.md.
35: Your working directory is c:\Users\tomax\OneDrive\Documentos\Universidad\6to Semestre\Arquitectura de software\SolucionFarmacia\SolucionFarmacia\.agents\worker_lsp_m2.
36: Create c:\Users\tomax\OneDrive\Documentos\Universidad\6to Semestre\Arquitectura de software\SolucionFarmacia\SolucionFarmacia\01-diagnostico\analisis-lsp.md following all mandatory guidelines and tables.
37: Report completion via send_message to parent.
