# DISPATCH — Forensic Auditor

You are `teamwork_preview_auditor_1` (Forensic Integrity Auditor).
Working Directory: `c:\Users\tomax\OneDrive\Documentos\Universidad\6to Semestre\Arquitectura de software\SolucionFarmacia\SolucionFarmacia\.agents\auditor_1_m4`
Project Root: `c:\Users\tomax\OneDrive\Documentos\Universidad\6to Semestre\Arquitectura de software\SolucionFarmacia\SolucionFarmacia`

## Task
Perform forensic integrity verification across all 9 deliverables in `01-diagnostico/`:
- `diagrama-as-is.md`
- `inventario-hallazgos.md`
- `mapa-dependencias.md`
- `puntos-dolor.md`
- `analisis-srp.md`
- `analisis-ocp.md`
- `analisis-lsp.md`
- `analisis-isp.md`
- `analisis-dip.md`

Verify:
1. No fabricated claims or fake line numbers.
2. All cited code snippets match actual source files in `BibFarmacia/` and `AppFarmaciaConsola/Program.cs`.
3. No dummy or placeholder content.
4. No integrity violations or cheating.

Read original requirements at `c:\Users\tomax\OneDrive\Documentos\Universidad\6to Semestre\Arquitectura de software\SolucionFarmacia\SolucionFarmacia\.agents\ORIGINAL_REQUEST.md`.

## Output
Write `handoff.md` in your working directory with explicit verdict: `CLEAN` or `INTEGRITY_VIOLATION`.
Report verdict via `send_message` to parent.
