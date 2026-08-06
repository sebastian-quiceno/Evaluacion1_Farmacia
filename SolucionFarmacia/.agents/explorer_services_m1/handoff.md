# Handoff Report — Explorer Services (M1)

**Agent ID:** `explorer_services_m1` (Services & Business Logic Specialist)  
**Working Directory:** `c:\Users\tomax\OneDrive\Documentos\Universidad\6to Semestre\Arquitectura de software\SolucionFarmacia\SolucionFarmacia\.agents\explorer_services_m1`  
**Date:** 2026-08-05  

---

## 1. Observation

Direct observations from source code examination across `BibFarmacia/Servicios/`, `BibFarmacia/Interfaces/`, and `BibFarmacia/Utilidades/`:

- **Obs-1 (`BibFarmacia/Servicios/ServicioCliente.cs` L47-81):**
  ```csharp
  public string Cargar(string ruta)
  {
      if (!File.Exists(ruta)) return "Archivo no encontrado";
      string[] lineas = File.ReadAllLines(ruta);
      foreach (string linea in lineas) {
          string[] datos = linea.Split(';');
          Cliente cliente = new Cliente(datos[0], datos[1], datos[2], datos[3]);
          clientes.Add(cliente);
      }
      return "Clientes cargados";
  }
  ```
  `File.ReadAllLines` is called directly inside a high-level business service. `EventoPuntos` is instantiated with `new EventoPuntos()` at L22.

- **Obs-2 (`BibFarmacia/Servicios/ServicioProducto.cs` L47-118):**
  - `VerificarStock` (L47-57) loops `productos` and triggers `EventoStock.Disparar(producto)`.
  - `VerificarVencimiento` (L59-73) evaluates `(producto.FechaVencimiento - DateTime.Now).Days <= 30` without type checking.
  - `CargarDesdeArchivo` (L75-118) uses `File.ReadAllLines` and hardcodes creation of `Laboratorio` (L93-97) and `MedicamentoCapsula` (L99-107) with `TipoRelleno.Gel`. `EventoStockMinimo` and `EventoVencimiento` are instantiated with `new` at L23-24.

- **Obs-3 (`BibFarmacia/Servicios/ServicioUsuario.cs` L27-73):**
  - `Login` (L31) delegates directly to static method `AspectoAutenticacion.Login(usuarios, user, password)`.
  - `Cargar` (L37-73) invokes `File.ReadAllLines` and manually splits CSV lines to construct `Usuario`.

- **Obs-4 (`BibFarmacia/Servicios/ServicioDescuento.cs` L11-17):**
  ```csharp
  public class ServicioDescuento : IDescuento
  {
      public decimal CalcularDescuento(decimal precio)
      {
          return precio * 0.10m;
      }
  }
  ```
  Calculates a flat 10% discount hardcoded in the method body.

- **Obs-5 (`BibFarmacia/Servicios/ServicioNotificacion.cs` L10-16):**
  ```csharp
  public class ServicioNotificacion : IServicioNotificacion
  {
      public void EnviarNotificacion(string mensaje)
      {
          Console.WriteLine($"[NOTIFICACION] {mensaje}");
      }
  }
  ```
  Couples notification logic directly to `Console.WriteLine`.

- **Obs-6 (`BibFarmacia/Servicios/ServicioMovimiento.cs` L15, L21-22):**
  Instantiates public field `EventoMovimiento` with `new EventoMovimiento()`.

- **Obs-7 (`BibFarmacia/Interfaces/IDescuento.cs` & `IServicioNotificacion.cs`):**
  Both interfaces contain single methods (`CalcularDescuento` and `EnviarNotificacion` respectively). No interfaces exist for `ServicioCliente`, `ServicioProducto`, `ServicioUsuario`, or `ServicioMovimiento`.

- **Obs-8 (`BibFarmacia/Utilidades/`):**
  Directory exists on disk at `c:\Users\tomax\OneDrive\Documentos\Universidad\6to Semestre\Arquitectura de software\SolucionFarmacia\SolucionFarmacia\BibFarmacia\Utilidades` and contains 0 files.

---

## 2. Logic Chain

1. **SRP Analysis:**
   - From Obs-1, Obs-2, and Obs-3: `ServicioCliente`, `ServicioProducto`, and `ServicioUsuario` combine in-memory domain collection management with raw file system I/O (`File.ReadAllLines`), string splitting/parsing, and hardcoded domain construction.
   - Therefore, these classes have multiple distinct reasons to change (changes to business rules vs changes to CSV persistence format/storage). This is a direct violation of SRP.

2. **OCP Analysis:**
   - From Obs-4: `ServicioDescuento` hardcodes `precio * 0.10m`. Supporting SC-3 (discounts by institutional agreements like universities or banks) requires modifying the source code of `ServicioDescuento.cs` to add conditional branches.
   - From Obs-2: `ServicioProducto.CargarDesdeArchivo` hardcodes `MedicamentoCapsula`. Adding SC-1 (cosmetics, groceries) or SC-2 (services) cannot be supported via file loading without modifying `ServicioProducto.cs`. Additionally, `VerificarVencimiento` assumes all items in `List<Producto>` have valid expiration dates, which breaks when introducing non-expiring services (SC-2).
   - Therefore, the service layer fails OCP.

3. **LSP Analysis:**
   - From Obs-4 and Obs-5: `ServicioDescuento` and `ServicioNotificacion` implement `IDescuento` and `IServicioNotificacion` without throwing exceptions or violating method signatures.
   - Therefore, syntactically they comply with LSP, although contract invariants (such as non-negative discount inputs) are absent.

4. **ISP Analysis:**
   - From Obs-7: `IDescuento` and `IServicioNotificacion` are thin, single-method interfaces that comply with ISP.
   - However, none of the 4 main services implement interfaces. External consumers (e.g. `AppFarmaciaConsola`) depend directly on concrete monolithic service classes, forcing them to depend on all methods of those classes.

5. **DIP Analysis:**
   - From Obs-1, Obs-2, Obs-3, Obs-5, and Obs-6: High-level business services depend directly on low-level file system APIs (`File.ReadAllLines`), console output streams (`Console.WriteLine`), static aspect methods (`AspectoAutenticacion.Login`), and concrete event/entity classes (`new EventoPuntos()`, `new MedicamentoCapsula(...)`).
   - None of the services accept abstractions via constructor parameters (all constructors are parameterless).
   - Therefore, the service layer fundamentally violates DIP.

---

## 3. Caveats

- **Scope boundary:** Investigation was restricted to `BibFarmacia/Servicios/`, `BibFarmacia/Interfaces/`, and `BibFarmacia/Utilidades/`. Classes in `BibFarmacia/Clases/`, `Aspectos/`, `Eventos/`, `Factories/`, and `AppFarmaciaConsola/Program.cs` were only referenced to trace cross-cutting dependencies.
- **Runtime execution:** Findings are based on static code analysis of C# source code. No dynamic runtime profiling or mock execution was performed.
- **Empty Utilidades:** `BibFarmacia/Utilidades/` is currently an empty directory. No code was analyzed in this folder because no files are present.

---

## 4. Conclusion

The service module of `BibFarmacia` suffers from critical architectural issues:
- **SRP:** Monolithic services mixing domain logic with file I/O and CSV parsing.
- **OCP:** Rigid implementation of discounts and file loading that prevents seamless implementation of SC-1 (cosmetics/groceries), SC-2 (services), and SC-3 (agreements).
- **LSP:** Basic compliance for existing interfaces, but domain polymorphism is bypassed in file loading.
- **ISP:** Existing interfaces are well-segregated, but main services lack interfaces entirely.
- **DIP:** Severe reliance on `File.ReadAllLines`, static methods, `Console.WriteLine`, and `new` instantiations without Dependency Injection.

**Actionable Recommendation:** Refactor the service layer by extracting `IRepository` interfaces for I/O, introducing `IStrategy` for discounts, introducing constructor Dependency Injection across all services, and utilizing `BibFarmacia/Utilidades/` for common CSV/I/O helpers.

---

## 5. Verification Method

To independently verify the observations and findings in this handoff:

1. **Verify Files and Code Snippets:**
   - View `BibFarmacia/Servicios/ServicioCliente.cs` lines 47–81 (`File.ReadAllLines`, `new Cliente`).
   - View `BibFarmacia/Servicios/ServicioProducto.cs` lines 47–118 (`VerificarVencimiento`, `CargarDesdeArchivo`, `new MedicamentoCapsula`).
   - View `BibFarmacia/Servicios/ServicioUsuario.cs` lines 27–35 (`AspectoAutenticacion.Login`) and lines 37–73 (`File.ReadAllLines`).
   - View `BibFarmacia/Servicios/ServicioDescuento.cs` lines 11–17 (`return precio * 0.10m`).
   - View `BibFarmacia/Servicios/ServicioNotificacion.cs` lines 10–16 (`Console.WriteLine`).
   - View `BibFarmacia/Interfaces/IDescuento.cs` and `BibFarmacia/Interfaces/IServicioNotificacion.cs`.
   - Inspect `BibFarmacia/Utilidades/` folder to confirm it is empty.

2. **Build Verification Command:**
   Run `dotnet build` from workspace root to verify solution compilation:
   ```powershell
   dotnet build "c:\Users\tomax\OneDrive\Documentos\Universidad\6to Semestre\Arquitectura de software\SolucionFarmacia\SolucionFarmacia\SolucionFarmacia.sln"
   ```

3. **Invalidation Conditions:**
   This analysis is invalidated if:
   - File I/O operations are moved out of service classes into dedicated repository/data access components.
   - `ServicioDescuento` is refactored to accept discount strategies or dynamic parameters.
   - Interfaces are introduced for `ServicioCliente`, `ServicioProducto`, `ServicioUsuario`, and `ServicioMovimiento` with constructor DI.
