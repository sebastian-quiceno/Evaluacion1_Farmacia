# Diagnóstico Arquitectónico SOLID: Principio de Sustitución de Liskov (LSP)

**Proyecto**: Solución Farmacia (BibFarmacia & AppFarmaciaConsola)  
**Fase**: Fase 1 — Diagnóstico AS-IS  
**Agente Especialista**: `teamwork_preview_worker_lsp` (Especialista en Principio de Sustitución de Liskov)  
**Fecha**: 2026-08-05  
**Archivo de Salida**: `01-diagnostico/analisis-lsp.md`  

---

## 1. Resumen Ejecutivo

El **Principio de Sustitución de Liskov (LSP - *Liskov Substitution Principle*)**, formulado por Barbara Liskov, establece que *si $S$ es un subtipo de $T$, los objetos de tipo $T$ en un programa pueden ser reemplazados por objetos de tipo $S$ sin alterar ninguna de las propiedades deseables del programa (corrección, comportamiento esperado, invariantes de estado y cumplimiento de contratos)*.

Tras evaluar rigurosamente los 26 archivos `.cs` de la biblioteca de clases `BibFarmacia` y el script principal `AppFarmaciaConsola/Program.cs` (378 líneas), se concluye que **el sistema viola gravemente el Principio LSP en sus jerarquías clave de dominio y en su capa de presentación/orquestación**.

Aunque algunas subclases cumplen formalmente con las firmas sintácticas de C# y no lanzan excepciones explícitas como `NotImplementedException`, **el modelo de dominio actual fuerza contratos inapropiados en la clase base abstracta (`Producto.cs`)**, asume comportamientos de inventario físico mutable en el cliente de consola, y omite la sobreescritura polimórfica de métodos informativos (`MostrarInformacion()`). Esto genera una rigidez estructural que impedirá la incorporación fluida de las solicitudes de cambio del negocio (SC-1: productos no medicamentosos como cosméticos y comestibles, SC-2: servicios de salud como inyectología y curaciones, SC-3: convenios y créditos corporativos).

---

## 2. Inventario de Jerarquías Analizadas

Se identificaron y auditaron cuatro (4) jerarquías principales de clases e interfaces a lo largo de los dos proyectos de la solución:

```
                      [ Persona (abstract) ]
                             /        \
                            /          \
                [ Cliente ]            [ Usuario ]


                      [ Producto (abstract) ]
                                 |
                          [ Medicamento ]
                             /        \
                            /          \
               [ MedicamentoCapsula ]  [ MedicamentoLiquido ]


     [ IDescuento ]                       [ IServicioNotificacion ]
           |                                          |
  [ ServicioDescuento ]                    [ ServicioNotificacion ]
```

### 2.1. Jerarquía de Personas (`BibFarmacia/Clases/`)
- **Clase Base Abstracta**: `Persona` (`Persona.cs`)
- **Subclases Concretas**: `Cliente` (`Cliente.cs`), `Usuario` (`Usuario.cs`)

### 2.2. Jerarquía de Productos (`BibFarmacia/Clases/`)
- **Clase Base Abstracta**: `Producto` (`Producto.cs`)
- **Subclase**: `Medicamento` (`Medicamento.cs`)
- **Subclases Hojas**: `MedicamentoCapsula` (`MedicamentoCapsula.cs`), `MedicamentoLiquido` (`MedicamentoLiquido.cs`)

### 2.3. Jerarquías de Interfaces y Servicios (`BibFarmacia/Interfaces/` y `BibFarmacia/Servicios/`)
- **Interfaz**: `IDescuento` (`IDescuento.cs`) $\rightarrow$ **Implementación**: `ServicioDescuento` (`ServicioDescuento.cs`)
- **Interfaz**: `IServicioNotificacion` (`IServicioNotificacion.cs`) $\rightarrow$ **Implementación**: `ServicioNotificacion` (`ServicioNotificacion.cs`)

---

## 3. Evaluación Detallada de Hallazgos contra LSP

A continuación se exponen detalladamente los cinco (5) hallazgos principales detectados, documentando la ubicación exacta, el fragmento de código relevante, el análisis técnico del fallo polimórfico o contractual, su impacto en el negocio y la recomendación de refactorización mínima.

---

### Hallazgo H-LSP-01: Imposición de Stock Físico y Fecha de Vencimiento en la Clase Base `Producto`

- **Ubicación**: `BibFarmacia/Clases/Producto.cs` (Líneas 10–14, 16–27)
- **Clase**: `Producto` (Clase base abstracta)
- **Código Fuente Relevante**:
```csharp
 8: public abstract class Producto
 9: {
10:     public string Nombre { get; set; }
11:     public decimal Precio { get; set; }
12:     public int Stock { get; set; }
13:     public int StockMinimo { get; set; }
14:     public DateTime FechaVencimiento { get; set; }
15: 
16:     protected Producto(string nombre,
17:         decimal precio,
18:         int stock,
19:         int stockMinimo,
20:         DateTime fechaVencimiento)
21:     { ... }
```

- **Análisis de la Violación a LSP**:
  - La clase abstracta `Producto` asume implícitamente que *todo* bien comercializado por la farmacia es un producto físico, inventariable y perecedero. Impone las propiedades `Stock`, `StockMinimo` y `FechaVencimiento` en el constructor protegido a todos los subtipos derivados.
  - Al evaluar la **Solicitud de Cambio SC-2** (venta de servicios intangibles de salud como inyectología, cambio de vendajes y curaciones básicas), estos servicios carecen por naturaleza de unidades de inventario físico en bodega y de una fecha de vencimiento.
  - Si se intenta modelar un servicio creando una subclase `ServicioSalud : Producto`, el desarrollador se verá obligado a:
    1. Pasar valores arbitrarios o "dummy" (e.g. `Stock = 999999`, `FechaVencimiento = DateTime.MaxValue`).
    2. O lanzar excepciones en los métodos o getters/setters (`throw new NotSupportedException()`), lo cual rompe directamente la sustitución de Liskov al invocar rutinas como `VerificarStock()` o `VerificarVencimiento()` en `ServicioProducto.cs` (L47–73).

- **Impacto en el Negocio**:
  - Imposibilidad de integrar la venta de servicios (SC-2) sin generar fallos en producción, notificaciones de vencimiento falsas o corrupción en los reportes de inventario.

- **Fix Sugerido (Arquitectónico)**:
  - Desacoplar la jerarquía monolithic de `Producto` introduciendo interfaces segregadas o un diseño basado en composición:
    - `IItemComercial` (`Nombre`, `Precio`)
    - `IInventariable` (`Stock`, `StockMinimo`, `ReducirStock()`, `AumentarStock()`)
    - `IPerecedero` (`FechaVencimiento`, `EstaVencido()`)
  - La clase `ServicioSalud` implementará solo `IItemComercial`, mientras que `Medicamento` implementará `IItemComercial`, `IInventariable` y `IPerecedero`.

---

### Hallazgo H-LSP-02: Acoplamiento Obligatorio de `Laboratorio` en la Jerarquía de `Medicamento`

- **Ubicación**: `BibFarmacia/Clases/Medicamento.cs` (Líneas 9–24)
- **Clase**: `Medicamento` (Hereda de `Producto`)
- **Código Fuente Relevante**:
```csharp
 9: public class Medicamento : Producto
10: {
11:     public Laboratorio Laboratorio { get; set; }
12: 
13:     public Medicamento(string nombre,
14:         decimal precio,
15:         int stock,
16:         int stockMinimo,
17:         DateTime fechaVencimiento,
18:         Laboratorio laboratorio)
19:         : base(nombre, precio, stock,
20:               stockMinimo, fechaVencimiento)
21:     {
22:         Laboratorio = laboratorio;
23:     }
24: }
```

- **Análisis de la Violación a LSP**:
  - La clase intermedia `Medicamento` establece la presencia obligatoria de un objeto `Laboratorio` (L11).
  - Al evaluar la **Solicitud de Cambio SC-1** (venta de productos no farmacéuticos como cosméticos, champús, gaseosas, helados y snacks), estos productos no son fabricados por un laboratorio farmacéutico ni requieren registro de laboratorio en la regulación de la farmacia.
  - Si el sistema actual intenta reutilizar `Medicamento` o si se asume que todos los ítems de venta derivan de `Medicamento` (como lo hace `ProductoFactory.cs` L11–44 y `ServicioProducto.cs` L93–99), se fuerza la creación de instancias de `Laboratorio` ficticias (e.g. `"Medellin"`, `"4444444"`), violando la semántica y expectativas del contrato polimórfico del dominio.

- **Impacto en el Negocio**:
  - Dificultad para registrar y categorizar adecuadamente nuevos productos no farmacéuticos (SC-1). Alto riesgo de inconsistencia de datos al poblar la base de datos o archivos con laboratorios inventados.

- **Fix Sugerido**:
  - Reestructurar el árbol de herencia para que `Medicamento` sea un subtipo especializado dentro de los productos de la farmacia, permitiendo que existan otros subtipos hermanos como `ProductoCosmetico` o `ProductoComestible` que dependan de un proveedor genérico o no requieran la propiedad `Laboratorio`.

---

### Hallazgo H-LSP-03: Omisión de Sobreescritura Polimórfica de `MostrarInformacion()` en Subclases Derivadas

- **Ubicación**: `BibFarmacia/Clases/Producto.cs` (Líneas 29–34), `BibFarmacia/Clases/MedicamentoCapsula.cs` (Líneas 11–29), `BibFarmacia/Clases/MedicamentoLiquido.cs` (Líneas 11–32)
- **Clases**: `Producto`, `MedicamentoCapsula`, `MedicamentoLiquido`
- **Código Fuente Relevante**:
```csharp
// BibFarmacia/Clases/Producto.cs (L29-34)
29: public virtual void MostrarInformacion()
30: {
31:     Console.WriteLine($"Producto: {Nombre}");
32:     Console.WriteLine($"Precio: {Precio}");
33:     Console.WriteLine($"Stock: {Stock}");
34: }

// BibFarmacia/Clases/MedicamentoCapsula.cs (L11-29) - NO sobrescribe MostrarInformacion()
11: public class MedicamentoCapsula : Medicamento
12: {
13:     public TipoRelleno TipoRelleno { get; set; }
14:     // ... Constructor únicamente, sin override de MostrarInformacion()
29: }

// BibFarmacia/Clases/MedicamentoLiquido.cs (L11-32) - NO sobrescribe MostrarInformacion()
11: public class MedicamentoLiquido : Medicamento
12: {
13:     public MaterialEnvase MaterialEnvase { get; set; }
14:     public int Mililitros { get; set; }
15:     // ... Constructor únicamente, sin override de MostrarInformacion()
32: }
```

- **Análisis de la Violación a LSP**:
  - `Producto.cs` define `MostrarInformacion()` como un método `virtual` diseñado explícitamente para la extensión polimórfica en subclases.
  - Sin embargo, **ninguna** de las subclases concretas (`Medicamento`, `MedicamentoCapsula`, `MedicamentoLiquido`) proporciona una implementación `override`.
  - Cuando un cliente opera con una lista polimórfica de tipo `List<Producto>` e invoca `producto.MostrarInformacion()`, el programa ejecuta únicamente la versión base de `Producto.cs`, omitiendo completamente los detalles esenciales de los subtipos específicos (como el laboratorio, el `TipoRelleno` de la cápsula, o el `MaterialEnvase` y los `Mililitros` del líquido).
  - Esto viola el principio de sustitución de Liskov a nivel de comportamiento esperado: sustituir una instancia de `Producto` por una de `MedicamentoCapsula` degrada la información presentada al cliente, forzando a la capa de UI (`Program.cs`) a inspeccionar manualmente las subclases mediante type casting (`is` / `as`), destruyendo el polimorfismo.

- **Impacto en el Negocio**:
  - Pérdida de visibilidad de los atributos específicos de los productos en la interfaz o reportes. Elevado costo de mantenimiento al tener que agregar condicionales por tipo en la UI cada vez que se cree una nueva clase de producto.

- **Fix Sugerido**:
  - Implementar la sobreescritura (`override`) del método `MostrarInformacion()` en `Medicamento`, `MedicamentoCapsula` y `MedicamentoLiquido` para que cada subtipo imprima de forma polimórfica y completa sus propios atributos. Alternativamente, separar la lógica de presentación utilizando el patrón Formatter/Presenter.

---

### Hallazgo H-LSP-04: Asunción de Inventario Físico Mutable en el Orquestador de Consola (`Program.cs`)

- **Ubicación**: `AppFarmaciaConsola/Program.cs` (Líneas 280–281)
- **Método / Fragmento**: Bloque de registro de venta en `Program.cs`
- **Código Fuente Relevante**:
```csharp
271: if (productoVenta != null)
272: {
273:     Console.Write("Cantidad: ");
274:     int cantidad = int.Parse(Console.ReadLine()!);
275: 
280:     productoVenta.Stock -= cantidad;
281: 
283:     Movimiento venta = new Movimiento(
284:         DateTime.Now,
285:         cantidad,
286:         "Venta",
287:         productoVenta);
288: 
290:     servicioMovimiento.RegistrarMovimiento(venta);
291: }
```

- **Análisis de la Violación a LSP**:
  - En la línea 280, el punto de entrada de la aplicación asume de forma directa e incondicional que la referencia `productoVenta` (de tipo abstracto `Producto`) posee una propiedad física `Stock` que admite la operación de decremento mutable directo (`-= cantidad`).
  - Si en el futuro se introduce una subclase de `Producto` para representar servicios (SC-2, e.g. `ServicioInyectologia`) donde el `Stock` sea conceptualmente infinito, no modificable o retorne 0, la ejecución del decremento directo causará un comportamiento erróneo (stocks negativos como `-1`, `-2`) o lanzará excepciones de tiempo de ejecución si la propiedad fuera de solo lectura o arrojara error al intentar modificarla.
  - El cliente (`Program.cs`) está fuertemente acoplado a detalles de implementación física en lugar de invocar una abstracción de comportamiento como `productoVenta.ReducirStock(cantidad)` o delegar la transacción al servicio de dominio `ServicioMovimiento`.

- **Impacto en el Negocio**:
  - Riesgo inminente de corrupción de datos de inventario y fallos durante el proceso de venta cuando se introduzcan servicios de inyectología o curaciones (SC-2).

- **Fix Sugerido**:
  - Encapsular la modificación de stock dentro de métodos de dominio validados (`ReducirStock(int cantidad)`) e introducir una interfaz `IInventariable` para que la UI solo decremente stock en objetos que verdaderamente soporten control de inventario físico.

---

### Hallazgo H-LSP-05: Ausencia de Validaciones de Invariantes y Precondiciones en `Cliente.AcumularPuntos` y `ServicioDescuento.CalcularDescuento`

- **Ubicación**: `BibFarmacia/Clases/Cliente.cs` (Líneas 20–23), `BibFarmacia/Servicios/ServicioDescuento.cs` (Líneas 13–16)
- **Clases**: `Cliente` (subclase de `Persona`), `ServicioDescuento` (implementa `IDescuento`)
- **Código Fuente Relevante**:
```csharp
// BibFarmacia/Clases/Cliente.cs (L20-23)
20: public void AcumularPuntos(int puntos)
21: {
22:     Puntos += puntos;
23: }

// BibFarmacia/Servicios/ServicioDescuento.cs (L13-16)
13: public decimal CalcularDescuento(decimal precio)
14: {
15:     return precio * 0.10m;
16: }
```

- **Análisis de la Violación a LSP**:
  - El Principio de Liskov exige que las subclases e implementaciones mantengan las invariantes de estado y respeten las precondiciones/postcondiciones del contrato implícito del sistema.
  - En `Cliente.cs`, el método `AcumularPuntos(int puntos)` realiza una adición directa `Puntos += puntos` sin validar si `puntos > 0`. Si un cliente de la clase pasa un número negativo (e.g. `AcumularPuntos(-50)`), el saldo de puntos del cliente disminuye inesperadamente, violando la regla semántica de acumulabilidad e invariantes del dominio.
  - En `ServicioDescuento.cs`, `CalcularDescuento(decimal precio)` no valida que `precio >= 0`. Si se pasa un precio negativo, el método retorna un descuento negativo, alterando la corrección matemática de la transacción.
  - Aunque estas clases cumplen sintácticamente con C# (no lanzan `NotImplementedException`), debilitan las precondiciones y rompen los contratos implícitos de negocio que cualquier cliente espera al interactuar con las abstracciones.

- **Impacto en el Negocio**:
  - Riesgo de inconsistencia y fraude en el saldo de puntos de fidelización de clientes. Posibilidad de errores contables en la aplicación de descuentos.

- **Fix Sugerido**:
  - Implementar validaciones de precondiciones e invariantes en los métodos de dominio (Guard Clauses):
    - En `Cliente.AcumularPuntos`: `if (puntos <= 0) throw new ArgumentException("Los puntos a acumular deben ser positivos.");`
    - En `ServicioDescuento.CalcularDescuento`: `if (precio < 0) throw new ArgumentOutOfRangeException(nameof(precio), "El precio no puede ser negativo.");`

---

## 4. Evaluación de Cumplimiento Sintáctico y Puntos Positivos

Es importante señalar que el sistema exhibe ciertos puntos de cumplimiento estructural que demuestran una base de diseño inicial correcta sobre la cual se puede refactorizar:

1. **Jerarquía `Persona` $\rightarrow$ `Cliente`, `Usuario`**:
   - Ambas subclases concretas (`Cliente.cs` y `Usuario.cs`) invocan adecuadamente el constructor protegido de la clase abstracta base `Persona.cs` (`: base(nombre, cedula, telefono, correo)`), heredando e inicializando correctamente los atributos de identidad.
   - Ninguna de las subclases modifica o anula el comportamiento de los atributos de la clase base `Persona`, permitiendo que cualquier rutina que maneje colecciones de `Persona` (por ejemplo, validación de cédulas o datos de contacto) opere de manera polimórfica sin errores de ejecución.

2. **Implementación de Interfaces Magras (`IDescuento` y `IServicioNotificacion`)**:
   - `ServicioNotificacion.cs` implementa la interfaz `IServicioNotificacion` cumpliendo la firma del método `EnviarNotificacion(string mensaje)`. No lanza excepciones no controladas y ejecuta la acción esperada sin alterar la firma.
   - `ServicioDescuento.cs` implementa la interfaz `IDescuento` respetando la firma de `CalcularDescuento(decimal precio) -> decimal`.

---

## 5. Matriz de Impacto ante Solicitudes de Cambio (SC-1, SC-2, SC-3)

La siguiente tabla resume el comportamiento del sistema AS-IS desde la perspectiva del Principio de Sustitución de Liskov al intentar implementar las tres solicitudes de cambio requeridas por el negocio:

| Solicitud de Cambio | Descripción | Limitación/Violación de LSP en Estado AS-IS | Impacto y Riesgo de Ruptura |
|---|---|---|---|
| **SC-1** | Venta de cosméticos, comestibles (gaseosas, helados, snacks). | `Medicamento` exige la propiedad `Laboratorio` obligatoriamente. Si los productos no farmacéuticos se heredan de `Medicamento` o de `Producto` forzando `FechaVencimiento`/`Laboratorio`, se rompen los contratos de subtipo. | **Alto**. Forzaría datos "dummy" o excepciones en repositorios y factories al procesar comestibles o cosméticos. |
| **SC-2** | Venta de servicios de salud (inyectología, curaciones, vendajes). | `Producto.cs` impone `Stock`, `StockMinimo` y `FechaVencimiento`. `Program.cs` L280 ejecuta `productoVenta.Stock -= cantidad`. Los servicios no tienen stock físico ni vencimiento. | **Crítico**. Tratar a un `ServicioSalud` como `Producto` causará stocks negativos (`-1`), notificaciones de vencimiento erróneas o caídas de aplicación si la propiedad se desactiva. |
| **SC-3** | Convenios corporativos (empresas, bancos, universidades) para crédito y descuentos. | `ServicioDescuento.cs` aplica un porcentaje fijo (10%) sin validar el tipo de cliente ni precondiciones de convenio. `Cliente.cs` no admite extensiones de políticas de crédito. | **Medio**. Imposibilidad de sustituir estrategias de descuento polimórficas por convenio sin modificar el código fuente existente. |

---

## 6. Tabla Resumen Obligatoria de Diagnóstico LSP

Conforme a la metodología del diagnóstico arquitectónico SOLID, a continuación se consolida la matriz de evaluación del Principio de Sustitución de Liskov para el proyecto:

| Principio | ¿Cumple? | Evidencia (archivo/línea) | Fix sugerido |
|---|---|---|---|
| **LSP (Liskov Substitution)** | **NO** | `BibFarmacia/Clases/Producto.cs` (L12, L14)<br>`BibFarmacia/Servicios/ServicioProducto.cs` (L47-73) | Separar `Producto` en interfaces segregadas (`IItemComercial`, `IInventariable`, `IPerecedero`) para permitir que los servicios de salud (SC-2) no hereden obligatoriamente stock ni vencimiento. |
| **LSP (Liskov Substitution)** | **NO** | `BibFarmacia/Clases/Medicamento.cs` (L11)<br>`BibFarmacia/Factories/ProductoFactory.cs` (L11-44) | Desacoplar la relación obligatoria con `Laboratorio` en la jerarquía base de productos, creando subclases específicas para cosméticos y comestibles (SC-1). |
| **LSP (Liskov Substitution)** | **NO** | `BibFarmacia/Clases/Producto.cs` (L29-34)<br>`MedicamentoCapsula.cs` (L11-29)<br>`MedicamentoLiquido.cs` (L11-32) | Sobrescribir (`override`) el método virtual `MostrarInformacion()` en todas las subclases derivadas (`MedicamentoCapsula`, `MedicamentoLiquido`) para garantizar completitud polimórfica. |
| **LSP (Liskov Substitution)** | **NO** | `AppFarmaciaConsola/Program.cs` (L280-281) | Eliminar la mutación directa de `productoVenta.Stock -= cantidad` en la UI de consola. Delegar la transacción a un servicio de inventario o invocar métodos de dominio abstraídos (`ReducirStock`). |
| **LSP (Liskov Substitution)** | **NO** | `BibFarmacia/Clases/Cliente.cs` (L20-23)<br>`BibFarmacia/Servicios/ServicioDescuento.cs` (L13-16) | Incorporar cláusulas de guarda (*Guard Clauses*) en métodos de negocio para validar precondiciones (puntos positivos, precios mayores o iguales a cero) y preservar invariantes de estado. |
| **LSP (Liskov Substitution)** | **SÍ (Sintáctico)** | `BibFarmacia/Clases/Persona.cs` (L9-24)<br>`BibFarmacia/Clases/Cliente.cs` (L9-25)<br>`BibFarmacia/Clases/Usuario.cs` (L8-22) | Mantener la jerarquía de herencia de `Persona`, introduciendo eventualmente interfaces de rol como `ICliente` e `IUsuario` para soportar convenios (SC-3). |
| **LSP (Liskov Substitution)** | **SÍ (Sintáctico)** | `BibFarmacia/Interfaces/IServicioNotificacion.cs` (L9-12)<br>`BibFarmacia/Servicios/ServicioNotificacion.cs` (L10-16) | Preservar la abstracción de notificación, permitiendo crear nuevas implementaciones polimórficas (`EmailNotificacionService`, `SmsNotificacionService`) bajo el mismo contrato. |
