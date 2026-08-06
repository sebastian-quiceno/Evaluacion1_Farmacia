# Handoff Report — Single Responsibility Principle (SRP) Diagnosis

**Agent**: `worker_srp_m2` (Single Responsibility Principle Specialist)  
**Target File Created**: `c:\Users\tomax\OneDrive\Documentos\Universidad\6to Semestre\Arquitectura de software\SolucionFarmacia\SolucionFarmacia\01-diagnostico\analisis-srp.md`  
**Date**: 2026-08-05  

---

## 1. Observation

Direct static analysis was performed on all source files of the `SolucionFarmacia` solution: 26 `.cs` files in `BibFarmacia` and 1 `.cs` file (`Program.cs`, 378 lines) in `AppFarmaciaConsola`.

Key evidence observed:
1. **`AppFarmaciaConsola/Program.cs`** (L1–L378): Merges UI output formatting, user input parsing (`int.Parse`), application flow orchestration, LINQ queries, direct domain state mutation (`productoVenta.Stock -= cantidad;` at L280), service instantiations with `new` (L8-18), and hardcoded text file bootstrapping (`"productos.txt"`, `"clientes.txt"`, `"usuarios.txt"` at L78-87).
2. **`BibFarmacia/Servicios/ServicioProducto.cs`** (L12–L119): Merges in-memory list management (`AgregarProducto`), stock/expiration alert evaluation (`VerificarStock`, `VerificarVencimiento` at L47-73), physical CSV file I/O (`File.ReadAllLines` at L86), and hardcoded domain entity instantiation (`new Laboratorio("Medellin", "4444444")`, `new MedicamentoCapsula(...)` at L93-107).
3. **`BibFarmacia/Servicios/ServicioCliente.cs`** (L12–L82) & **`ServicioUsuario.cs`** (L12–L74): Merge domain collection handling with direct file reading (`File.ReadAllLines`) and CSV parsing.
4. **`BibFarmacia/Clases/Producto.cs`** (L29–L34): Encapsulates domain state (`Nombre`, `Precio`, `Stock`, `FechaVencimiento`) while containing UI output code `MostrarInformacion()` (`Console.WriteLine`).
5. **`BibFarmacia/Aspectos/AspectoValidacion.cs`** (L11–L45): Combines validation logic for multiple unrelated domain entities (`Cliente` and `Producto`), returning UI-formatted `string` error messages.
6. **`BibFarmacia/Factories/ProductoFactory.cs`** (L11–L44): Embeds hardcoded default business policies (`stockMinimo = 5`, expiration offsets `DateTime.Now.AddMonths(...)`, default `TipoRelleno.Gel`, `MaterialEnvase.Vidrio`) into creation methods.
7. **`BibFarmacia/Eventos/`** (`EventoStockMinimo.cs`, `EventoVencimiento.cs`, `EventoPuntos.cs`, `EventoMovimiento.cs`): Event publishers embed hardcoded Spanish string formatting inside `Disparar()` methods.
8. **Compliant classes**: `Persona.cs`, `Cliente.cs`, `Usuario.cs`, `Laboratorio.cs`, `Medicamento.cs`, `MedicamentoCapsula.cs`, `MedicamentoLiquido.cs`, `Movimiento.cs`, `MaterialEnvase.cs`, `TipoRelleno.cs`, `IDescuento.cs`, `IServicioNotificacion.cs`, `ServicioDescuento.cs`, `ServicioNotificacion.cs`.

---

## 2. Logic Chain

1. **SRP Definition**: SRP requires that a module has one, and only one, reason to change. Each non-cohesive responsibility assigned to a class adds an independent axis of change.
2. **Identification of Multiple Reasons to Change**:
   - `Program.cs` has 7 reasons to change (UI aesthetics, input parsing, application workflow, LINQ query logic, domain state mutation, service wiring, file configuration).
   - `ServicioProducto.cs` has 4 reasons to change (in-memory product catalog, alert evaluation thresholds, physical file I/O & CSV format, default object construction).
   - `ServicioCliente.cs` and `ServicioUsuario.cs` have 3 reasons to change each (domain rules, file I/O, event/authentication mechanisms).
   - `Producto.cs` has 2 reasons to change (domain state representation, console output format).
   - `AspectoValidacion.cs` has 3 reasons to change (customer validation rules, product validation rules, validation result string formatting).
   - `ProductoFactory.cs` has 2 reasons to change (creation signature mechanics, default business policy values).
3. **Assessment of Business & Technical Impact**:
   - Coupling domain logic to `Console.WriteLine` or `File.ReadAllLines` prevents unit testing, reusability in Web/API contexts, and extension for non-drug products (SC-1) or services (SC-2).
   - Changing validation, notification text, or business policies forces modifications across shared core classes, creating high regression risks.
4. **Summary Consolidation**: Every class in the solution was evaluated and cataloged in the mandatory summary table (`Principio | ¿Cumple? | Evidencia (archivo/línea) | Fix sugerido`).

---

## 3. Caveats

- The scope of this analysis is strictly AS-IS static diagnosis for the Single Responsibility Principle (SRP). It does not perform code refactoring on C# source files, as Phase 1 focuses exclusively on diagnostic documentation.
- Other SOLID principles (OCP, LSP, ISP, DIP) are covered by dedicated specialist agents; cross-principle impacts are noted where SRP overlaps with OCP/DIP.

---

## 4. Conclusion

The system `SolucionFarmacia` exhibits severe architectural debt regarding SRP. Core domain entities (`Producto`), services (`ServicioProducto`, `ServicioCliente`, `ServicioUsuario`), factories (`ProductoFactory`), static helpers (`AspectoValidacion`), and the console entry point (`Program.cs`) suffer from responsibility pollution, mixing domain logic with file I/O, UI output, hardcoded defaults, and event dispatching. The generated diagnostic artifact `01-diagnostico/analisis-srp.md` provides complete line-level traceability, multi-reason analysis, compliance evidence, and actionable refactor guidelines.

---

## 5. Verification Method

To verify this diagnostic work:
1. Inspect the generated artifact at `c:\Users\tomax\OneDrive\Documentos\Universidad\6to Semestre\Arquitectura de software\SolucionFarmacia\SolucionFarmacia\01-diagnostico\analisis-srp.md`.
2. Cross-reference line numbers and code snippets against `BibFarmacia` and `AppFarmaciaConsola` source files.
3. Run `dotnet build` in `c:\Users\tomax\OneDrive\Documentos\Universidad\6to Semestre\Arquitectura de software\SolucionFarmacia\SolucionFarmacia` to verify that the C# solution builds cleanly without regressions.
