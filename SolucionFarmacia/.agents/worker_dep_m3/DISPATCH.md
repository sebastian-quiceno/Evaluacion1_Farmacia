# DISPATCH — Worker Dependency Map

You are `teamwork_preview_worker_dep` (Dependency Mapping Specialist).
Working Directory: `c:\Users\tomax\OneDrive\Documentos\Universidad\6to Semestre\Arquitectura de software\SolucionFarmacia\SolucionFarmacia\.agents\worker_dep_m3`
Target File to Create: `c:\Users\tomax\OneDrive\Documentos\Universidad\6to Semestre\Arquitectura de software\SolucionFarmacia\SolucionFarmacia\01-diagnostico\mapa-dependencias.md`

## Original Requirements
Read `c:\Users\tomax\OneDrive\Documentos\Universidad\6to Semestre\Arquitectura de software\SolucionFarmacia\SolucionFarmacia\.agents\ORIGINAL_REQUEST.md`

## Input Evidence Reports & SOLID Reports
Read:
- `c:\Users\tomax\OneDrive\Documentos\Universidad\6to Semestre\Arquitectura de software\SolucionFarmacia\SolucionFarmacia\01-diagnostico\analisis-dip.md`
- `c:\Users\tomax\OneDrive\Documentos\Universidad\6to Semestre\Arquitectura de software\SolucionFarmacia\SolucionFarmacia\.agents\explorer_services_m1\analysis.md`
- `c:\Users\tomax\OneDrive\Documentos\Universidad\6to Semestre\Arquitectura de software\SolucionFarmacia\SolucionFarmacia\.agents\explorer_console_m1\analysis.md`

## Mandatory Task
Create `01-diagnostico/mapa-dependencias.md` following Requirement R4:
1. Clearly classify all classes into:
   - **High-Level Modules** (Business rules, domain entities, services): `ServicioProducto`, `ServicioCliente`, `ServicioUsuario`, `ServicioMovimiento`, `ServicioDescuento`, domain classes.
   - **Low-Level Modules** (Technical details & infrastructure): File system (`File.ReadAllLines`), Console (`Console.WriteLine`), static aspects (`AspectoAutenticacion.Login`), static factories (`ProductoFactory`), text files (`productos.txt`, `clientes.txt`, `usuarios.txt`).
2. Document direct concrete dependencies:
   - `new` keyword instantiations per module (which class instantiates which)
   - Direct static method invocations
   - Direct System.IO calls and hardcoded file paths
3. Analyze where Dependency Inversion IS applied today (e.g. `ServicioDescuento : IDescuento`, `ServicioNotificacion : IServicioNotificacion` syntactically) vs where DIP is NOT applied (everywhere else).
4. Include dependency matrices, Mermaid dependency flowcharts, and a structural coupling summary. Write clear Spanish Markdown.
