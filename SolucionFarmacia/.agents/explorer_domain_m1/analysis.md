# Domain Model Analysis & SOLID Architectural Diagnosis (BibFarmacia Domain Layer)

**Working Directory**: `c:\Users\tomax\OneDrive\Documentos\Universidad\6to Semestre\Arquitectura de software\SolucionFarmacia\SolucionFarmacia\.agents\explorer_domain_m1`  
**Target Scope**: `BibFarmacia/Clases/`, `BibFarmacia/Enums/`, `BibFarmacia/Aspectos/`, `BibFarmacia/Factories/`, `BibFarmacia/Eventos/` (18 C# files)  
**Date**: 2026-08-05  

---

## 1. Executive Summary

An in-depth static analysis was conducted on all 18 C# source files belonging to the core domain model, enums, aspect helpers, factories, and event definitions within `BibFarmacia`. While the domain model captures fundamental pharmacy concepts (`Producto`, `Medicamento`, `Persona`, `Cliente`, `Usuario`, `Laboratorio`, `Movimiento`), the implementation suffers from structural rigidity, improper coupling, missing abstractions, and multiple SOLID principle violations. These issues severely restrict the domain's extensibility to support non-pharmaceutical items (cosmetics, beverages), services (inyectología), or complex agreements (SC-1, SC-2, SC-3).

---

## 2. Complete Domain Catalog & Structure

Below is the complete structural inventory of the 18 analyzed domain files:

### 2.1 Enums (`BibFarmacia/Enums/`)
1. **`MaterialEnvase.cs`** (`namespace BibFarmacia.Enum`, lines 9-13)
   - `public enum MaterialEnvase`: `Vidrio`, `Plastico`.
   - *Note*: Namespace typo (`BibFarmacia.Enum` singular vs folder `Enums`).
2. **`TipoRelleno.cs`** (`namespace BibFarmacia.Enum`, lines 9-13)
   - `public enum TipoRelleno`: `Gel`, `Polvo`.
   - *Note*: Namespace typo (`BibFarmacia.Enum` singular vs folder `Enums`).

### 2.2 Domain Entities & Hierarchy (`BibFarmacia/Clases/`)
3. **`Persona.cs`** (`namespace BibFarmacia.Clases`, lines 9-24)
   - `public abstract class Persona`
   - Properties: `public string Nombre { get; set; }` (L11), `public string Cedula { get; set; }` (L12), `public string Telefono { get; set; }` (L13), `public string Correo { get; set; }` (L14).
   - Constructor: `protected Persona(string nombre, string cedula, string telefono, string correo)` (L16-23).
4. **`Cliente.cs`** (`namespace BibFarmacia.Clases`, lines 9-25) — inherits `Persona`
   - Properties: `public int Puntos { get; set; }` (L11).
   - Constructor: `public Cliente(string nombre, string cedula, string telefono, string correo)` (L13-18).
   - Methods: `public void AcumularPuntos(int puntos)` (L20-23).
5. **`Usuario.cs`** (`namespace BibFarmacia.Clases`, lines 8-22) — inherits `Persona`
   - Properties: `public string UserName { get; set; }` (L10), `public string Password { get; set; }` (L11).
   - Constructor: `public Usuario(string nombre, string cedula, string telefono, string correo, string userName, string password)` (L13-20).
6. **`Laboratorio.cs`** (`namespace BibFarmacia.Clases`, lines 9-24)
   - Properties: `public string Nombre { get; set; }` (L11), `public string Direccion { get; set; }` (L12), `public string Telefono { get; set; }` (L13).
   - Constructor: `public Laboratorio(string nombre, string direccion, string telefono)` (L15-22).
7. **`Producto.cs`** (`namespace BibFarmacia.Clases`, lines 8-35)
   - `public abstract class Producto`
   - Properties: `public string Nombre { get; set; }` (L10), `public decimal Precio { get; set; }` (L11), `public int Stock { get; set; }` (L12), `public int StockMinimo { get; set; }` (L13), `public DateTime FechaVencimiento { get; set; }` (L14).
   - Constructor: `protected Producto(string nombre, decimal precio, int stock, int stockMinimo, DateTime fechaVencimiento)` (L16-27).
   - Methods: `public virtual void MostrarInformacion()` (L29-34).
8. **`Medicamento.cs`** (`namespace BibFarmacia.Clases`, lines 9-24) — inherits `Producto`
   - Properties: `public Laboratorio Laboratorio { get; set; }` (L11).
   - Constructor: `public Medicamento(string nombre, decimal precio, int stock, int stockMinimo, DateTime fechaVencimiento, Laboratorio laboratorio)` (L13-23).
9. **`MedicamentoCapsula.cs`** (`namespace BibFarmacia.Clases`, lines 11-29) — inherits `Medicamento`
   - Properties: `public TipoRelleno TipoRelleno { get; set; }` (L13).
   - Constructor: `public MedicamentoCapsula(...)` (L15-28).
10. **`MedicamentoLiquido.cs`** (`namespace BibFarmacia.Clases`, lines 11-32) — inherits `Medicamento`
    - Properties: `public MaterialEnvase MaterialEnvase { get; set; }` (L13), `public int Mililitros { get; set; }` (L14).
    - Constructor: `public MedicamentoLiquido(...)` (L16-31).
11. **`Movimiento.cs`** (`namespace BibFarmacia.Clases`, lines 9-26)
    - Properties: `public DateTime Fecha { get; set; }` (L11), `public int Cantidad { get; set; }` (L12), `public string Tipo { get; set; }` (L13), `public Producto Producto { get; set; }` (L14).
    - Constructor: `public Movimiento(DateTime fecha, int cantidad, string tipo, Producto producto)` (L16-25).

### 2.3 Aspect Static Classes (`BibFarmacia/Aspectos/`)
12. **`AspectoAutenticacion.cs`** (`namespace BibFarmacia.Aspectos`, lines 11-22)
    - `public static class AspectoAutenticacion`
    - Methods: `public static bool Login(List<Usuario> usuarios, string user, string password)` (L13-21).
13. **`AspectoValidacion.cs`** (`namespace BibFarmacia.Aspectos`, lines 11-45)
    - `public static class AspectoValidacion`
    - Methods: `public static string ValidarCliente(Cliente cliente)` (L13-28), `public static string ValidarProducto(Producto producto)` (L30-44).

### 2.4 Factories (`BibFarmacia/Factories/`)
14. **`ProductoFactory.cs`** (`namespace BibFarmacia.Factories`, lines 11-44)
    - `public static class ProductoFactory`
    - Methods: `public static MedicamentoCapsula CrearCapsula(string nombre, decimal precio, int stock, Laboratorio laboratorio)` (L13-27), `public static MedicamentoLiquido CrearLiquido(string nombre, decimal precio, int stock, Laboratorio laboratorio)` (L28-43).

### 2.5 Events (`BibFarmacia/Eventos/`)
15. **`EventoMovimiento.cs`** (`namespace BibFarmacia.Eventos`, lines 9-23)
    - Delegates & Events: `public delegate void DelegadoMovimiento(string mensaje);` (L11-12), `public event DelegadoMovimiento? MovimientoRegistrado;` (L14-15).
    - Methods: `public void Disparar(string tipo)` (L17-22).
16. **`EventoPuntos.cs`** (`namespace BibFarmacia.Eventos`, lines 9-24)
    - Delegates & Events: `public delegate void DelegadoPuntos(string mensaje);` (L11-12), `public event DelegadoPuntos? PuntosAcumulados;` (L14-15).
    - Methods: `public void Disparar(string cliente, int puntos)` (L17-23).
17. **`EventoStockMinimo.cs`** (`namespace BibFarmacia.Eventos`, lines 10-23)
    - Delegates & Events: `public delegate void DelegadoStock(string mensaje);` (L12-13), `public event DelegadoStock? StockMinimo;` (L15).
    - Methods: `public void Disparar(Producto producto)` (L17-22).
18. **`EventoVencimiento.cs`** (`namespace BibFarmacia.Eventos`, lines 11-25)
    - Delegates & Events: `public delegate void DelegadoVencimiento(string mensaje);` (L13-14), `public event DelegadoVencimiento? Vencimiento;` (L16-17).
    - Methods: `public void Disparar(Producto producto)` (L19-24).

---

## 3. SOLID Principles Evaluation

### 3.1 Single Responsibility Principle (SRP)
*A class should have one, and only one, reason to change.*

#### Violations Identified
1. **`Producto.cs` (lines 29-34)**
   - **Symptom**: `Producto` encapsulates domain state (`Nombre`, `Precio`, `Stock`, `StockMinimo`, `FechaVencimiento`) but also contains UI presentation logic in `MostrarInformacion()` which writes directly to standard output (`Console.WriteLine`).
   - **Reasons to change**: 1) Modifications to domain entity properties or business rules, 2) Changes to presentation format or output stream (e.g. logging to GUI, web, or file).
   - **Suggested Fix**: Remove `MostrarInformacion()` from `Producto`. Move presentation formatting to a dedicated formatter or UI view model component.

2. **`AspectoValidacion.cs` (lines 11-45)**
   - **Symptom**: Static class combining validation rules for unrelated domain entities (`Cliente` in `ValidarCliente` L13-28 and `Producto` in `ValidarProducto` L30-44). Moreover, validation logic returns UI-oriented `string` message results instead of structured validation result objects.
   - **Reasons to change**: 1) Customer validation rules change, 2) Product validation rules change, 3) Validation output format changes.
   - **Suggested Fix**: Separate into individual validator components implementing `IValidator<Cliente>` and `IValidator<Producto>`.

3. **`AspectoAutenticacion.cs` (lines 11-22)**
   - **Symptom**: `AspectoAutenticacion` mixes authentication logic with `List<Usuario>` collection searching (`u.UserName == user && u.Password == password`). It is misnamed as an "Aspect" while functioning as a static helper tightly coupled to `Usuario` state.
   - **Reasons to change**: 1) Authentication logic/security rules, 2) Storage/user retrieval mechanism.
   - **Suggested Fix**: Delegate user lookup to a user repository / service, and encapsulate authentication in an `IAuthenticationService`.

4. **`ProductoFactory.cs` (lines 19-26, 34-42)**
   - **Symptom**: `ProductoFactory` mixes creation mechanics with hardcoded business defaults (`stockMinimo = 5`, `DateTime.Now.AddMonths(...)`, `TipoRelleno.Gel`, `MaterialEnvase.Vidrio`, `120 ml`).
   - **Reasons to change**: 1) Creation signature or construction logic, 2) Business policy changes regarding default stock limits, expiration offsets, or packaging materials.
   - **Suggested Fix**: Pass defaults as configurable parameters or strategy objects rather than embedding magic numbers/values inside the factory.

5. **`EventoStockMinimo.cs` (lines 17-22) & `EventoVencimiento.cs` (lines 19-24)**
   - **Symptom**: Event publisher classes combine event triggering with hardcoded string message formatting (`"ALERTA: stock mínimo de ..."` and `"ALERTA: ... próximo a vencer"`).
   - **Reasons to change**: 1) Event dispatching mechanism, 2) Message text/localization/formatting.
   - **Suggested Fix**: Publish strongly-typed event arguments (e.g., `StockMinimoReachedEventArgs` containing the product reference) and let subscribers format the notification text.

#### Compliance Points
- **`Persona.cs`**, **`Laboratorio.cs`**, **`Movimiento.cs`**: Pure data entities holding domain state without mixing I/O or external concerns.
- **`MaterialEnvase.cs`**, **`TipoRelleno.cs`**: Single responsibility as value enums representing specific domain value scopes.

---

### 3.2 Open/Closed Principle (OCP)
*Software entities should be open for extension, but closed for modification.*

#### Violations Identified
1. **`Producto.cs` / `Medicamento.cs` Class Hierarchy Rigidity (lines 8-35 in `Producto.cs`, lines 9-24 in `Medicamento.cs`)**
   - **Symptom**: Base class `Producto` assumes every product in the domain has physical `Stock` (L12) and `FechaVencimiento` (L14). Subclass `Medicamento` assumes every product is a drug produced by a `Laboratorio` (L11).
   - **Impact on Change Requests**:
     - **SC-1 (Cosmetics, Beverages, Snacks)**: Cosmetics or beverages do not have a pharmaceutical `Laboratorio`. Adding them requires either making `Laboratorio` nullable in `Medicamento` or creating siblings of `Medicamento` directly under `Producto`, but `Producto` still forces `FechaVencimiento` (e.g. non-perishable goods).
     - **SC-2 (Services: Inyectología, Curaciones)**: Services do NOT have physical stock or expiration dates. Inheriting from `Producto` forces services to carry unused `Stock` and `FechaVencimiento` properties or throw errors when accessed.
   - **Suggested Fix**: Refactor `Producto` using interface segregation or composition (`IItem`, `ISellable`, `IStockable`, `IPerishable`).

2. **`ProductoFactory.cs` (lines 11-44)**
   - **Symptom**: `ProductoFactory` exposes explicit static creation methods `CrearCapsula` and `CrearLiquido`.
   - **Impact on Change Requests**: Adding a new product type (e.g., `MedicamentoJarabe`, `Cosmetico`, `ServicioInyectologia`) requires modifying `ProductoFactory.cs` to write new static factory methods (`CrearJarabe`, `CrearCosmetico`), directly violating OCP.
   - **Suggested Fix**: Implement abstract factory or factory pattern with registered creation delegates (`IProductoFactory` or `IDictionary<TipoProducto, IProductoCreator>`).

3. **`AspectoValidacion.cs` (lines 13-44)**
   - **Symptom**: Contains hardcoded static methods `ValidarCliente` and `ValidarProducto`.
   - **Impact**: Adding validation for `Usuario`, `Laboratorio`, `Movimiento`, or new product categories requires editing `AspectoValidacion.cs` to add new methods or adding `switch/if` branches for specific subtypes.
   - **Suggested Fix**: Use polymorphic validators implementing a generic `IValidator<T>` interface.

4. **`Producto.cs` `MostrarInformacion()` Non-Polymorphic Output (lines 29-34 in `Producto.cs`)**
   - **Symptom**: `MostrarInformacion()` is defined as `virtual` in `Producto`, but `Medicamento`, `MedicamentoCapsula`, and `MedicamentoLiquido` DO NOT override it.
   - **Impact**: Calling `MostrarInformacion()` on a `MedicamentoCapsula` or `MedicamentoLiquido` prints only base `Producto` attributes (`Nombre`, `Precio`, `Stock`), completely missing `Laboratorio`, `TipoRelleno`, `MaterialEnvase`, and `Mililitros`. Displaying subclass-specific details requires changing caller code to check types (`is` or `switch`).
   - **Suggested Fix**: Override `MostrarInformacion()` in derived classes or delegate string representation to an external formatter/view provider.

#### Compliance Points
- **`Persona.cs` -> `Cliente.cs`, `Usuario.cs`**: The `Persona` abstraction allows extending the domain with new person types (e.g., `Empleado`, `Proveedor`) by inheritance without modifying `Persona.cs`.

---

### 3.3 Liskov Substitution Principle (LSP)
*Subtypes must be substitutable for their base types without altering correctness.*

#### Violations Identified
1. **`Producto` Hierarchy Behavioral Loss (`Producto.cs` L29-34 vs Subclasses)**
   - **Symptom**: `Producto.MostrarInformacion()` provides a base virtual implementation that prints `Nombre`, `Precio`, `Stock`. Subclasses (`MedicamentoCapsula`, `MedicamentoLiquido`) inherit this method without overriding it.
   - **Impact**: Substituting a `MedicamentoCapsula` object into code expecting a `Producto` and calling `MostrarInformacion()` results in incomplete information display, violating expected polymorphic behavioral completeness.

2. **Inappropriate Abstraction for Non-Stock/Non-Physical Items (`Producto.cs` L10-14)**
   - **Symptom**: Abstract class `Producto` enforces `Stock` and `FechaVencimiento` across all subtypes. If a future `Servicio` or digital item is modeled as a `Producto` (to fit into sales/movement lists), operations modifying `Stock` or checking expiration will fail or produce nonsensical behavior.
   - **Suggested Fix**: Split `Producto` into smaller interfaces/contracts (`IVendible`, `IInventariable`, `IVencible`).

3. **Unchecked State Invariants in Derived Classes (`Cliente.cs` L20-23)**
   - **Symptom**: `Cliente.AcumularPuntos(int puntos)` performs `Puntos += puntos` without validating if `puntos > 0`. Passing a negative integer reduces points unexpectedly, violating base assumptions about accumulative operations.

#### Compliance Points
- **`Cliente` and `Usuario` as `Persona`**: Both `Cliente` and `Usuario` correctly initialize base `Persona` fields (`Nombre`, `Cedula`, `Telefono`, `Correo`) and can be safely substituted where `Persona` is required for identity checks.

---

### 3.4 Interface Segregation Principle (ISP)
*Clients should not be forced to depend on methods or properties they do not use.*

#### Violations Identified
1. **Total Absence of Domain Interfaces in `BibFarmacia/Clases/`**
   - **Symptom**: None of the domain classes (`Producto`, `Persona`, `Cliente`, `Usuario`, `Laboratorio`, `Movimiento`) implement domain interfaces (e.g. `IVendible`, `IStockable`, `IIdentificable`).
   - **Impact**: Any service or client component that needs to interact with products for pricing or billing is forced to depend on the entire monolithic `Producto` class (including stock, minimum stock, expiration date, etc.).
   - **Suggested Fix**: Introduce segregated interfaces:
     - `IIdentificable` (`Cedula`/`Id`, `Nombre`)
     - `IVendible` (`Nombre`, `Precio`)
     - `IInventariable` (`Stock`, `StockMinimo`)
     - `IPerishable` (`FechaVencimiento`)

2. **Coupling in Event Handlers (`EventoStockMinimo.cs` L18, `EventoVencimiento.cs` L20)**
   - **Symptom**: Event trigger signatures `Disparar(Producto producto)` depend on the full `Producto` class when they only access `producto.Nombre`.
   - **Impact**: Events cannot be reused for other entities that might suffer low stock or expiration (e.g. raw materials, supplies) unless they inherit from `Producto`.
   - **Suggested Fix**: Pass `INombrable` or specific event data classes instead of full concrete `Producto` instances.

#### Compliance Points
- **Narrow Event Delegates (`BibFarmacia/Eventos/`)**: `DelegadoMovimiento`, `DelegadoPuntos`, `DelegadoStock`, `DelegadoVencimiento` define focused single-method delegate signatures taking specific parameters (`string mensaje`).

---

### 3.5 Dependency Inversion Principle (DIP)
*High-level modules should not depend on low-level modules. Both should depend on abstractions.*

#### Violations Identified
1. **`ProductoFactory.cs` Direct Concrete Instantiation & Low-Level System Dependency (lines 19-26, 34-42)**
   - **Symptom**: `ProductoFactory` directly instantiates concrete classes `new MedicamentoCapsula(...)` and `new MedicamentoLiquido(...)` and returns concrete types instead of abstract `Producto` or interfaces. Furthermore, it depends directly on low-level system clock `DateTime.Now` (L24, L39).
   - **Impact**: Cannot unit test factory creation deterministically without system time dependencies. Callers are coupled to concrete product types.
   - **Suggested Fix**: Inject a clock abstraction (`IDateTimeProvider`) and return abstract `Producto` or interface `IProducto`.

2. **`Movimiento.cs` Concrete Association (line 14)**
   - **Symptom**: `Movimiento.Producto` holds a reference directly to abstract class `Producto` rather than an interface.
   - **Impact**: Cannot register movements for non-`Producto` items (such as services under SC-2) without refactoring `Movimiento`.
   - **Suggested Fix**: Change property type to `IItem` or `IVendible`.

3. **`AspectoAutenticacion.cs` Concrete Collection Coupling (line 14)**
   - **Symptom**: `AspectoAutenticacion.Login(List<Usuario> usuarios, ...)` depends on concrete `List<Usuario>` instead of `IEnumerable<Usuario>`, `IReadOnlyCollection<Usuario>`, or a user repository abstraction `IUsuarioRepository`.
   - **Suggested Fix**: Accept `IEnumerable<Usuario>` or delegate to `IUsuarioRepository`.

4. **Namespace Convention Flaw (`MaterialEnvase.cs` L7, `TipoRelleno.cs` L7)**
   - **Symptom**: Namespace declared as `BibFarmacia.Enum` (singular) while physical folder and standard .NET conventions use `BibFarmacia.Enums` (plural). Causes namespace clutter and inconsistent imports.

---

## 4. Evaluation Against Change Requests (SC-1, SC-2, SC-3)

| Change Request | Current Domain Limitation | Impacted Files | Expected Breakage / Effort |
|----------------|---------------------------|----------------|----------------------------|
| **SC-1**: Selling cosmetics, beverages, snacks | `Producto` -> `Medicamento` hierarchy assumes every product is a drug with a `Laboratorio`. `ProductoFactory` only creates `MedicamentoCapsula` & `MedicamentoLiquido`. | `Producto.cs`, `Medicamento.cs`, `ProductoFactory.cs`, `AspectoValidacion.cs` | High effort. Adding non-medicament products requires creating new subclasses or modifying `Medicamento`, plus updating `ProductoFactory` and `AspectoValidacion`. |
| **SC-2**: Selling services (inyectología, curaciones) | `Producto` forces `Stock`, `StockMinimo`, and `FechaVencimiento`. `Movimiento` references `Producto`. | `Producto.cs`, `Movimiento.cs`, `AspectoValidacion.cs` | High risk of failure. Services have no stock or expiration. Creating `Servicio : Producto` violates LSP as stock reduction methods fail or require dummy values. |
| **SC-3**: Entity agreements (discounts, credit limits) | `Cliente` only has `Puntos`. No abstraction for customer categories, credit limits, or entity agreements. | `Cliente.cs`, `Persona.cs`, `AspectoValidacion.cs` | Medium effort. `Cliente` must be modified to add fields or references to agreement policies, violating OCP. |

---

## 5. Consolidated Domain Findings Inventory

Below is the structured catalog of all 15 findings detected within the `BibFarmacia` domain layer:

| ID | Location (File / Class / Line) | Observed Symptom | Principle | Business Impact | Severity |
|----|--------------------------------|------------------|-----------|-----------------|----------|
| **H-DOM-01** | `BibFarmacia/Clases/Producto.cs`<br>`Producto`<br>L29-34 | `MostrarInformacion()` prints directly to `Console.WriteLine`. | **SRP** | High cost to reuse domain logic in web/GUI/mobile interfaces; risk of unintended console output in automated tests. | **Media** |
| **H-DOM-02** | `BibFarmacia/Aspectos/AspectoValidacion.cs`<br>`AspectoValidacion`<br>L11-45 | Mixes validation for `Cliente` and `Producto`, returning UI string messages. | **SRP** | Adding new entity validations increases class size and risk of regression across unrelated domain modules. | **Media** |
| **H-DOM-03** | `BibFarmacia/Aspectos/AspectoAutenticacion.cs`<br>`AspectoAutenticacion`<br>L13-21 | Combines credential checking with direct `List<Usuario>` collection filtering. | **SRP** | Cannot change user storage or authentication rules without modifying shared aspect logic. | **Baja** |
| **H-DOM-04** | `BibFarmacia/Factories/ProductoFactory.cs`<br>`ProductoFactory`<br>L19-26, L34-42 | Hardcodes business rules (`stockMinimo=5`, expiration offsets, defaults) inside creation code. | **SRP** | Business policy changes for stock or expiration require editing factory code and re-deploying core libraries. | **Media** |
| **H-DOM-05** | `BibFarmacia/Eventos/EventoStockMinimo.cs` L17-22<br>`BibFarmacia/Eventos/EventoVencimiento.cs` L19-24 | Event dispatchers embed hardcoded Spanish string formatting in `Disparar()`. | **SRP** | Cannot localize or change notification message layout without editing core event classes. | **Baja** |
| **H-DOM-06** | `BibFarmacia/Clases/Producto.cs` L8-35<br>`BibFarmacia/Clases/Medicamento.cs` L9-24 | Hierarchy forces physical stock, expiration, and `Laboratorio` on all products. | **OCP** | High cost to introduce non-pharmaceutical items (SC-1) or services (SC-2); requires modifying base domain classes. | **Alta** |
| **H-DOM-07** | `BibFarmacia/Factories/ProductoFactory.cs`<br>`ProductoFactory`<br>L11-44 | Creation logic uses explicit static methods (`CrearCapsula`, `CrearLiquido`). | **OCP** | Every new product type requires editing `ProductoFactory.cs` to add methods, increasing maintenance cost. | **Alta** |
| **H-DOM-08** | `BibFarmacia/Aspectos/AspectoValidacion.cs`<br>`AspectoValidacion`<br>L13-44 | Hardcoded `ValidarCliente` and `ValidarProducto` static methods. | **OCP** | Adding validation for new entities requires modifying `AspectoValidacion.cs`, risking side effects. | **Media** |
| **H-DOM-09** | `BibFarmacia/Clases/Producto.cs` L29-34<br>`MedicamentoCapsula.cs` L11-29 | Base `MostrarInformacion()` is not overridden in `MedicamentoCapsula` or `MedicamentoLiquido`. | **OCP / LSP** | Loss of subclass detail when treating items polymorphically; requires type casting in UI layers. | **Media** |
| **H-DOM-10** | `BibFarmacia/Clases/Producto.cs`<br>`Producto`<br>L10-14 | Enforces `Stock` and `FechaVencimiento` for all product subclasses. | **LSP** | Modeling services (SC-2) as `Producto` causes runtime failures or forced dummy values for stock/expiration. | **Alta** |
| **H-DOM-11** | `BibFarmacia/Clases/Cliente.cs`<br>`Cliente`<br>L20-23 | `AcumularPuntos(int puntos)` accepts negative integers without invariant checks. | **LSP** | Risk of corrupting customer points balance due to unvalidated state modification. | **Baja** |
| **H-DOM-12** | `BibFarmacia/Clases/` (All 9 class files) | Zero domain interfaces implemented across `Producto`, `Persona`, `Cliente`, etc. | **ISP** | Clients needing price or identity are forced to depend on heavy concrete domain entities. | **Alta** |
| **H-DOM-13** | `BibFarmacia/Eventos/EventoStockMinimo.cs` L18<br>`EventoVencimiento.cs` L20 | Event methods take full concrete `Producto` objects when only reading `Nombre`. | **ISP** | Events cannot be reused for non-`Producto` entities needing stock/expiration alerts. | **Baja** |
| **H-DOM-14** | `BibFarmacia/Factories/ProductoFactory.cs`<br>`ProductoFactory`<br>L19, L34, L24, L39 | Factory directly instantiates concrete classes and depends on `DateTime.Now`. | **DIP** | Inability to unit test creation logic deterministically; high coupling to concrete types. | **Alta** |
| **H-DOM-15** | `BibFarmacia/Enums/MaterialEnvase.cs` L7<br>`TipoRelleno.cs` L7 | Namespace declared as `BibFarmacia.Enum` instead of project standard `BibFarmacia.Enums`. | **DIP / Naming** | Compiler confusion and inconsistent `using` directives across the codebase. | **Baja** |

---
