# Project: SolucionFarmacia SOLID Architectural Diagnosis (Phase 1 — AS-IS)

## Architecture
- Legacy C# .NET 8 system comprising two projects:
  - `BibFarmacia`: Domain models, services, events, factories, interfaces, aspects (26 `.cs` files)
  - `AppFarmaciaConsola`: Console application (`Program.cs`, 378 lines) + data files

## Feature & Scope Inventory
| # | Feature / Deliverable | Description | Milestone | Source | Status |
|---|------------------------|-------------|-----------|--------|--------|
| 1 | Codebase Survey & Mapping | Read all files in BibFarmacia & AppFarmaciaConsola, map classes/members | M1 | Survey | DONE |
| 2 | `01-diagnostico/analisis-srp.md` | SRP detailed analysis report | M2 | R1 | DONE |
| 3 | `01-diagnostico/analisis-ocp.md` | OCP detailed analysis report | M2 | R1 | DONE |
| 4 | `01-diagnostico/analisis-lsp.md` | LSP detailed analysis report | M2 | R1 | DONE |
| 5 | `01-diagnostico/analisis-isp.md` | ISP detailed analysis report | M2 | R1 | DONE |
| 6 | `01-diagnostico/analisis-dip.md` | DIP detailed analysis report | M2 | R1 | DONE |
| 7 | `01-diagnostico/diagrama-as-is.md` | Mermaid UML AS-IS class diagram | M3 | R3 | DONE |
| 8 | `01-diagnostico/mapa-dependencias.md` | Dependency map (High vs Low level, inversion analysis) | M3 | R4 | DONE |
| 9 | `01-diagnostico/inventario-hallazgos.md` | Consolidated findings table (25 findings, 6 columns) | M4 | R2 | DONE |
| 10 | `01-diagnostico/puntos-dolor.md` | Top 3 pain points, prioritized & evaluated against SC-1, SC-2, SC-3 | M4 | R5 | DONE |

## Milestones
| # | Name | Scope | Dependencies | Status |
|---|------|-------|-------------|--------|
| M1 | Codebase Survey & Mapping | Comprehensive code inspection of BibFarmacia & AppFarmaciaConsola | None | DONE |
| M2 | SOLID Principle Reports | Create individual SRP, OCP, LSP, ISP, DIP analysis files | M1 | DONE |
| M3 | Architecture Diagrams & Dependency Map | Create Mermaid UML class diagram and Dependency map | M1 | DONE |
| M4 | Findings Consolidation & Pain Points | Create consolidated inventory and evaluated top 3 pain points | M2, M3 | DONE |

## Code Layout
- Input Codebase:
  - `BibFarmacia/`: Aspectos, Clases, Enums, Eventos, Factories, Interfaces, Servicios, Utilidades
  - `AppFarmaciaConsola/`: Program.cs, productos.txt, clientes.txt, usuarios.txt
- Diagnostic Deliverables Output Directory:
  - `01-diagnostico/`
