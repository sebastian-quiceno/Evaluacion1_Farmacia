# Handoff Report — Worker LSP (M2 - Liskov Substitution Principle Specialist)

## 1. Observation
- Evaluated `BibFarmacia` (26 C# files across Clases, Servicios, Interfaces, Eventos, Factories, Aspectos) and `AppFarmaciaConsola/Program.cs` (378 lines).
- Directly observed the following code structures and signatures:
  1. `BibFarmacia/Clases/Producto.cs` (L12, L14): Declares `public int Stock { get; set; }` and `public DateTime FechaVencimiento { get; set; }` in the abstract base class constructor `protected Producto(...)`.
  2. `BibFarmacia/Clases/Medicamento.cs` (L11): Enforces `public Laboratorio Laboratorio { get; set; }` across all drug subtypes.
  3. `BibFarmacia/Clases/Producto.cs` (L29–34): Declares `public virtual void MostrarInformacion()`, but derived classes `MedicamentoCapsula` (L11–29) and `MedicamentoLiquido` (L11–32) do not override `MostrarInformacion()`.
  4. `AppFarmaciaConsola/Program.cs` (L280–281): Direct property mutation `productoVenta.Stock -= cantidad;` assuming all `Producto` instances are mutable physical inventory.
  5. `BibFarmacia/Clases/Cliente.cs` (L20–23): `AcumularPuntos(int puntos)` performs `Puntos += puntos` without validating `puntos > 0`.
  6. `BibFarmacia/Servicios/ServicioDescuento.cs` (L13–16): `CalcularDescuento(decimal precio)` returns `precio * 0.10m` without checking `precio >= 0`.
  7. Syntactical adherence observed in `Persona` -> `Cliente` / `Usuario` constructors and `IServicioNotificacion` -> `ServicioNotificacion`.

## 2. Logic Chain
1. *From Observation 1*: Base class `Producto` forces physical stock and expiration on all derived instances. Health services (SC-2: inyectología, curaciones) are non-physical, non-perishable services. Inheriting services from `Producto` forces dummy values or throwing unsupported exceptions, violating LSP.
2. *From Observation 2*: `Medicamento` forces a `Laboratorio` property. Non-pharmaceutical goods (SC-1: cosmetics, beverages) do not have a pharmaceutical laboratory. Inheriting them from `Medicamento` or creating dummy laboratories breaks domain semantics when treated polymorphically.
3. *From Observation 3*: `Producto.MostrarInformacion()` is virtual, but subclasses omit `override`. Operating on `List<Producto>` and calling `MostrarInformacion()` omits subtype details (`MaterialEnvase`, `TipoRelleno`, `Mililitros`), degrading behavioral completeness.
4. *From Observation 4*: `Program.cs` directly mutates `productoVenta.Stock -= cantidad`. If a non-stock item or service is passed as `productoVenta`, the UI layer mutates invalid state, breaking subtype substitutability.
5. *From Observation 5 & 6*: `AcumularPuntos` and `CalcularDescuento` lack precondition validation, allowing negative points or negative prices to corrupt domain invariants.

## 3. Caveats
- No code modifications were made to `BibFarmacia` or `AppFarmaciaConsola` during this Phase 1 AS-IS diagnostic phase.
- Verification is based on static code analysis and contract evaluation against future change requests (SC-1, SC-2, SC-3).

## 4. Conclusion
The AS-IS implementation of `BibFarmacia` and `AppFarmaciaConsola` violates the Liskov Substitution Principle (LSP) in 5 major areas spanning domain model design, incomplete polymorphic method overrides, UI-level state mutation, and weak contract invariants. `01-diagnostico/analisis-lsp.md` has been successfully created with all mandatory sections, snippets, exact line references, impact matrices, and the required summary table.

## 5. Verification Method
- Inspect the generated artifact: `01-diagnostico/analisis-lsp.md`.
- Verify the presence of the mandatory table with columns: `Principio | ¿Cumple? | Evidencia (archivo/línea) | Fix sugerido`.
- Spot-check referenced lines in source files:
  - `BibFarmacia/Clases/Producto.cs` (L12, L14, L29–34)
  - `BibFarmacia/Clases/Medicamento.cs` (L11)
  - `AppFarmaciaConsola/Program.cs` (L280–281)
  - `BibFarmacia/Clases/Cliente.cs` (L20–23)
  - `BibFarmacia/Servicios/ServicioDescuento.cs` (L13–16)
