# Análisis SOLID — Módulo de Servicios, Interfaces y Utilidades (BibFarmacia)

**Agente Specialist:** `explorer_services_m1` (Services & Business Logic Specialist)  
**Fecha:** 2026-08-05  
**Ámbito de Investigación:** 
- `BibFarmacia/Servicios/` (`ServicioCliente.cs`, `ServicioDescuento.cs`, `ServicioMovimiento.cs`, `ServicioNotificacion.cs`, `ServicioProducto.cs`, `ServicioUsuario.cs`)
- `BibFarmacia/Interfaces/` (`IDescuento.cs`, `IServicioNotificacion.cs`)
- `BibFarmacia/Utilidades/` (Directorio examinado: actualmente vacío)

---

## 1. Resumen Ejecutivo de Hallazgos

El análisis del módulo de servicios de `BibFarmacia` revela que, si bien las dos interfaces existentes (`IDescuento` e `IServicioNotificacion`) son delgadas y cumplen formalmente con SRP e ISP, la capa de servicios presenta severos vicios de diseño arquitectónico. Los 6 servicios concretos carecen de abstracciones (ninguno implementa interfaces salvo `ServicioDescuento` y `ServicioNotificacion`), no utilizan inyección de dependencias, instancian directamente colecciones en memoria, eventos de infraestructura y entidades mediante `new`, y mezclan la lógica de negocio con lectura física de archivos CSV (`File.ReadAllLines`) y salidas por consola (`Console.WriteLine`).

Total de archivos de código analizados: 8 archivos `.cs` (6 servicios, 2 interfaces) + 1 directorio de utilidades.

---

## 2. Inventario Consolidado de Hallazgos

| ID | Ubicación (archivo / clase / línea) | Síntoma observado | Principio comprometido | Impacto en el negocio | Severidad | Fix sugerido |
|---|---|---|---|---|---|---|
| H-SERV-01 | `BibFarmacia/Servicios/ServicioCliente.cs` / `ServicioCliente` / L47-81 | `Cargar` lee archivos con `File.ReadAllLines` y parsea CSV directamente en el servicio de negocio. | **SRP, DIP** | Imposible cambiar la persistencia (ej. a Base de Datos) o probar en memoria sin modificar la lógica del cliente. Alto riesgo de fallos por cambios de formato. | **Alta** | Extraer `IClienteRepository` o `IClienteDataReader` para aislar la lectura del disco. |
| H-SERV-02 | `BibFarmacia/Servicios/ServicioCliente.cs` / `ServicioCliente` / L16, L22 | El servicio instancia públicamente el evento concreto `EventoPuntos` con `new`. | **DIP, SRP** | Acoplamiento rígido con el sistema de eventos de infraestructura. Impide reemplazar el bus de eventos o simular eventos en pruebas unitarias. | **Media** | Inyectar una abstracción de publicación de eventos (`IEventBus` o delegados) por constructor. |
| H-SERV-03 | `BibFarmacia/Servicios/ServicioProducto.cs` / `ServicioProducto` / L75-118 | `CargarDesdeArchivo` usa `File.ReadAllLines` y hardcodea instanciación de `Laboratorio` ("Medellin", "4444444") y `MedicamentoCapsula`. | **SRP, OCP, DIP** | No permite cargar otros tipos de productos (cosméticos, abarrotes SC-1 o servicios SC-2). Datos faltantes son inventados de forma rígida. | **Alta** | Separar la persistencia/parsing a un `IProductoRepository` y usar una fábrica abstracta o deserializador polimórfico. |
| H-SERV-04 | `BibFarmacia/Servicios/ServicioProducto.cs` / `ServicioProducto` / L47-73 | El servicio mezcla la gestión de la colección de productos con la lógica de monitoreo y reglas de expiración/stock. | **SRP** | Si cambian las políticas de alertas o umbrales de vencimiento/stock, se debe modificar la clase principal de productos. | **Media** | Mover la verificación de reglas de stock y vencimiento a clases de especificación o servicios de dominio dedicados (`StockRuleEvaluator`). |
| H-SERV-05 | `BibFarmacia/Servicios/ServicioProducto.cs` / `ServicioProducto` / L16-17, L23-24 | Instanciación directa de `EventoStockMinimo` y `EventoVencimiento` mediante `new` en campos públicos. | **DIP, SRP** | Imposible cambiar el mecanismo de notificación/eventos sin modificar `ServicioProducto`. Dificulta la automatización de pruebas. | **Media** | Abstraer el despacho de eventos a través de interfaces de eventos inyectadas. |
| H-SERV-06 | `BibFarmacia/Servicios/ServicioUsuario.cs` / `ServicioUsuario` / L37-73 | `Cargar` lee archivos en disco (`File.ReadAllLines`) y parsea CSV de usuarios. | **SRP, DIP** | Mezcla I/O de archivos con la gestión de usuarios. Impide migrar a una fuente de datos segura o encriptada sin alterar la clase. | **Alta** | Delegar el almacenamiento a `IUsuarioRepository`. |
| H-SERV-07 | `BibFarmacia/Servicios/ServicioUsuario.cs` / `ServicioUsuario` / L27-35 | `Login` depende directamente del método estático `AspectoAutenticacion.Login`. | **DIP** | Imposible cambiar la estrategia de autenticación (ej. OAuth, JWT, Hashing) o mockear la autenticación para pruebas. | **Alta** | Inyectar una abstracción `IAutenticador` por constructor en lugar de llamar métodos estáticos. |
| H-SERV-08 | `BibFarmacia/Servicios/ServicioDescuento.cs` / `ServicioDescuento` / L13-16 | Implementa `IDescuento` retornando un valor quemado (10% fijo) `return precio * 0.10m;`. | **OCP** | Imposible aplicar descuentos por tipo de cliente, convenios (SC-3) o promociones dinámicas sin modificar esta clase o agregar condicionales. | **Alta** | Implementar el patrón Strategy con estrategias configurables de descuento (`ClienteDescuentoStrategy`, `ConvenioDescuentoStrategy`). |
| H-SERV-09 | `BibFarmacia/Servicios/ServicioNotificacion.cs` / `ServicioNotificacion` / L12-15 | Dependencia rígida de `Console.WriteLine` para notificar al usuario. | **DIP, OCP** | No se pueden enviar notificaciones por correo, SMS o guardar logs en archivo sin modificar o reemplazar la clase. | **Media** | Crear implementaciones separadas (`ConsoleNotificacionService`, `EmailNotificacionService`) bajo `IServicioNotificacion`. |
| H-SERV-10 | `BibFarmacia/Servicios/ServicioMovimiento.cs` / `ServicioMovimiento` / L15, L21-22 | Instancia directamente `EventoMovimiento` con `new` en un campo público. | **DIP** | Dependencia directa de una clase concreta de evento en lugar de un despachador abstracto. | **Baja** | Inyectar la publicación de eventos por constructor. |
| H-SERV-11 | `BibFarmacia/Servicios/*` / Todos los servicios | Ningún servicio principal implementa interfaces de abstracción (`IServicioCliente`, `IServicioProducto`, etc.) y carecen de constructores con DI. | **DIP, ISP** | La capa de presentación (`AppFarmaciaConsola`) depende directamente de implementaciones concretas de servicios monolíticos. | **Alta** | Definir interfaces de servicios y repositorios, e introducirlas mediante Inyección de Dependencias (DI). |
| H-SERV-12 | `BibFarmacia/Utilidades/` / Directorio Utilidades | La carpeta está completamente vacía y no aporta abstracciones de utilidad o helpers requeridos. | **OCP, DIP** | Falta de componentes reutilizables para I/O, parsing de CSV, o validaciones comunes. | **Baja** | Crear utilidades/helpers abstraídos de I/O y formateo en esta carpeta si aplica. |
| H-SERV-13 | `BibFarmacia/Interfaces/IDescuento.cs` & `IServicioNotificacion.cs` | Interfaces magras y cohesivas que cumplen SRP y ISP, pero son las únicas 2 interfaces en toda la solución. | **ISP (Cumple), DIP (Insuficiente)** | Aunque cumplen individualmente, hay una notable ausencia de interfaces para repositorios, entidades y servicios principales. | **Media** | Diseñar un esquema de interfaces completo para repositorios, servicios y proveedores de eventos. |

---

## 3. Análisis Detallado por Principio SOLID

### 3.1. SRP — Single Responsibility Principle (Responsabilidad Única)

Un módulo o clase debe tener una, y solo una, razón para cambiar.

#### Archivos con violaciones de SRP:

1. **`BibFarmacia/Servicios/ServicioCliente.cs`** (Líneas 12-82)
   - **Razones para cambiar identificadas:**
     1. Cambios en la gestión de la colección en memoria de clientes o cálculo de puntos (`AcumularPuntos`, L36-45).
     2. Cambios en el formato o medio de persistencia del archivo CSV (`Cargar`, L47-81, usa `File.ReadAllLines` y `linea.Split(';')`).
     3. Cambios en la infraestructura de eventos de puntos (`EventoPuntos`, L16, L22).
   - **Código fuente relevante:**
     ```csharp
     47: public string Cargar(string ruta)
     48: {
     52:     if (!File.Exists(ruta)) return "Archivo no encontrado";
     57:     string[] lineas = File.ReadAllLines(ruta);
     60:     foreach (string linea in lineas)
     61:     {
     62:         string[] datos = linea.Split(';');
     65:         Cliente cliente = new Cliente(datos[0], datos[1], datos[2], datos[3]);
     72:         clientes.Add(cliente);
     73:     }
     ```
   - **Fix sugerido:** Mover `Cargar` a un componente `ClienteCsvReader` o `IClienteRepository`.

2. **`BibFarmacia/Servicios/ServicioProducto.cs`** (Líneas 12-119)
   - **Razones para cambiar identificadas:**
     1. Gestión del catálogo de productos en memoria (`AgregarProducto`, `ObtenerProductos`, L27-45).
     2. Reglas de negocio para monitoreo de alertas de stock mínimo (`VerificarStock`, L47-57) y días de vencimiento (`VerificarVencimiento`, L59-73).
     3. Persistencia e I/O de disco (`CargarDesdeArchivo`, L75-118).
     4. Creación e invención de objetos de dominio secundarios con valores fijos (crea `Laboratorio` con `"Medellin"` y `"4444444"`, L93-97, y `MedicamentoCapsula` con `TipoRelleno.Gel`, L99-107).
   - **Código fuente relevante:**
     ```csharp
     93: Laboratorio laboratorio = new Laboratorio(datos[5], "Medellin", "4444444");
     99: MedicamentoCapsula medicamento = new MedicamentoCapsula(
    101:     datos[0], decimal.Parse(datos[1]), int.Parse(datos[2]), int.Parse(datos[3]),
    105:     DateTime.Parse(datos[4]), laboratorio, Enum.TipoRelleno.Gel);
     ```
   - **Fix sugerido:** Delegar la persistencia a un repositorio y la evaluación de alertas a un evaluador de reglas de dominio.

3. **`BibFarmacia/Servicios/ServicioUsuario.cs`** (Líneas 12-74)
   - **Razones para cambiar identificadas:**
     1. Almacenamiento y gestión de la colección de usuarios (`AgregarUsuario`, L21-25).
     2. Orquestación de login (`Login`, L27-35).
     3. Lectura y parsing del archivo de texto plano de usuarios (`Cargar`, L37-73).
   - **Fix sugerido:** Extraer la carga de archivo a un `IUsuarioRepository`.

#### Archivos que CUMPLEN formalmente con SRP:
- `BibFarmacia/Servicios/ServicioDescuento.cs`: Tiene como única responsabilidad la aplicación del cálculo de descuento.
- `BibFarmacia/Interfaces/IDescuento.cs`: Contrato enfocado exclusivamente en `CalcularDescuento`.
- `BibFarmacia/Interfaces/IServicioNotificacion.cs`: Contrato enfocado exclusivamente en `EnviarNotificacion`.

---

### 3.2. OCP — Open/Closed Principle (Abierto/Cerrado)

Las entidades de software deben estar abiertas a la extensión pero cerradas a la modificación.

#### Archivos con violaciones de OCP:

1. **`BibFarmacia/Servicios/ServicioDescuento.cs`** (Líneas 11-17)
   - **Evidencia:**
     ```csharp
     13: public decimal CalcularDescuento(decimal precio)
     14: {
     15:     return precio * 0.10m;
     16: }
     ```
   - **Evaluación contra Solicitudes de Cambio (SC):**
     - **SC-3 (Convenios con entidades/bancos/universidades):** Imposible de soportar sin modificar la clase `ServicioDescuento.cs`. Si se agregan tipos de convenio o porcentajes dinámicos por cliente, habría que modificar directamente la línea 15 introduciendo condicionales `if/switch`.
   - **Fix sugerido:** Transformar `ServicioDescuento` o `IDescuento` para admitir estrategias de descuento (`IDescuentoStrategy`), cerrando la clase a modificaciones.

2. **`BibFarmacia/Servicios/ServicioProducto.cs`** (Líneas 47-118)
   - **Evidencia:**
     - `VerificarVencimiento` (L59-73) calcula `(producto.FechaVencimiento - DateTime.Now).Days <= 30`. Asume que TODOS los productos tienen fecha de vencimiento.
     - `CargarDesdeArchivo` (L75-118) hardcodea la instanciación de `MedicamentoCapsula`.
   - **Evaluación contra Solicitudes de Cambio (SC):**
     - **SC-1 (Cosméticos, comestibles):** Al agregar productos sin vencimiento o cosméticos con diferentes reglas de vencimiento, `VerificarVencimiento` se romperá o requerirá modificar `ServicioProducto.cs` agregando chequeos por tipo (`if (p is Medicamento)`). Además, `CargarDesdeArchivo` solo sabe crear `MedicamentoCapsula`, imposibilitando la carga de comestibles o cosméticos sin modificar la línea 99.
     - **SC-2 (Servicios: inyectología, curaciones):** Los servicios no se vencen ni tienen stock en inventario físico. Invocar `VerificarStock` o `VerificarVencimiento` fallará conceptualmente o requerirá modificar `ServicioProducto` con branching por tipo.
   - **Fix sugerido:** Aplicar polimorfismo mediante métodos virtuales/abstractos en la jerarquía de productos o estrategias de validación/monitoreo por tipo de producto.

3. **`BibFarmacia/Servicios/ServicioNotificacion.cs`** (Líneas 10-16)
   - **Evidencia:**
     ```csharp
     14: Console.WriteLine($"[NOTIFICACION] {mensaje}");
     ```
   - **Síntoma:** Está cerrado a la extensión de canales. Para notificar por Email o SMS en lugar de consola, se debe modificar esta clase o crear una nueva sin un mecanismo de canal extensible.
   - **Fix sugerido:** Implementar un `CompositeNotificationService` o canales inyectados.

---

### 3.3. LSP — Liskov Substitution Principle (Sustitución de Liskov)

Las subclases o implementaciones deben ser sustituibles por sus clases base o interfaces sin alterar el comportamiento correcto del programa.

#### Evaluación de LSP en Servicios e Interfaces:

1. **`BibFarmacia/Servicios/ServicioDescuento.cs` vs `IDescuento`**
   - **Análisis:** `ServicioDescuento` implementa `IDescuento.CalcularDescuento(decimal precio)`. Retorna `precio * 0.10m`. No lanza excepciones inesperadas (`NotImplementedException`, `ArgumentException`), por lo que desde el punto de vista del tipo de retorno no rompe la sustitución sintáctica.
   - **Observación de Contrato:** No valida que `precio >= 0`. Si se pasa un precio negativo, retornará un descuento negativo, lo cual viola invariantes de negocio.
   - **Estado:** Cumple sintácticamente con LSP, pero carece de precondiciones de contrato.

2. **`BibFarmacia/Servicios/ServicioNotificacion.cs` vs `IServicioNotificacion`**
   - **Análisis:** Implementa `EnviarNotificacion(string mensaje)` imprimiendo en consola. Cumple la firma del contrato `void`.
   - **Estado:** Cumple con LSP.

3. **Falta de sustitución polimórfica en `ServicioProducto.CargarDesdeArchivo`**
   - **Análisis:** La firma opera con `List<Producto>`, pero en la carga de archivos (L99) solo se crean instancias de `MedicamentoCapsula`. `MedicamentoLiquido` u otros subtipos de `Producto` no pueden ser cargados. Aunque no rompe LSP directamente en ejecución, invalida la sustitución polimórfica deseada en la capa de persistencia.

---

### 3.4. ISP — Interface Segregation Principle (Segregación de Interfaces)

Los clientes no deben estar obligados a depender de interfaces que no utilizan.

#### Evaluación de ISP:

1. **Interfaces Existentes (`IDescuento.cs` e `IServicioNotificacion.cs`)**
   - `IDescuento` (L9-12): Contiene exactamente 1 método (`CalcularDescuento`).
   - `IServicioNotificacion` (L9-12): Contiene exactamente 1 método (`EnviarNotificacion`).
   - **Estado:** **CUMPLEN CERO GORDURA / 100% COHESIVAS.** Son interfaces segregadas ideales.

2. **Ausencia de Interfaces en la Capa de Servicios**
   - Ninguno de los 4 servicios principales (`ServicioCliente`, `ServicioProducto`, `ServicioUsuario`, `ServicioMovimiento`) implementa una interfaz.
   - **Síntoma:** Los clientes (como `Program.cs` en la consola) están obligados a depender de clases concretas monolíticas que exponen TODOS sus métodos públicos (métodos de carga de archivos, gestión de colecciones y eventos mezclados), violando el espíritu de ISP al no ofrecer interfaces de cliente segregadas (ej. `IProductoReader`, `IProductoService`).

---

### 3.5. DIP — Dependency Inversion Principle (Inversión de Dependencias)

Los módulos de alto nivel no deben depender de módulos de bajo nivel. Ambos deben depender de abstracciones. Las abstracciones no deben depender de detalles.

#### Archivos con violaciones de DIP:

1. **`BibFarmacia/Servicios/ServicioCliente.cs`**
   - **Líneas 52, 58:** Dependencia directa de `System.IO.File` (`File.Exists`, `File.ReadAllLines`).
   - **Líneas 20, 22:** Dependencia de `new List<Cliente>()` y `new EventoPuntos()`.
   - **Ausencia de DI:** El constructor (L18-23) es totalmente paramétrico y sin inyección.

2. **`BibFarmacia/Servicios/ServicioProducto.cs`**
   - **Líneas 80, 86:** Dependencia directa de `System.IO.File` (`File.Exists`, `File.ReadAllLines`).
   - **Líneas 21, 23, 24:** Dependencia directa de `new List<Producto>()`, `new EventoStockMinimo()`, `new EventoVencimiento()`.
   - **Líneas 93, 99:** Instanciación directa con `new` de `Laboratorio` y `MedicamentoCapsula`.

3. **`BibFarmacia/Servicios/ServicioUsuario.cs`**
   - **Líneas 42, 48:** Dependencia directa de `System.IO.File`.
   - **Línea 31:** Dependencia directa de método estático `AspectoAutenticacion.Login(usuarios, user, password)`. High-level business logic depende de un método estático concreto.

4. **`BibFarmacia/Servicios/ServicioMovimiento.cs`**
   - **Líneas 19, 21:** Instancia directa con `new` de `List<Movimiento>` y `EventoMovimiento`.

5. **`BibFarmacia/Servicios/ServicioNotificacion.cs`**
   - **Línea 14:** Dependencia directa del detalle de infraestructura `Console.WriteLine`.

---

## 4. Evaluación de Extensibilidad ante Solicitudes de Cambio (SC-1, SC-2, SC-3)

| Solicitud de Cambio | Impacto en Módulo de Servicios | Archivos Afectados | Riesgos de Ruptura |
|---|---|---|---|
| **SC-1** (Cosméticos, comestibles) | `ServicioProducto.cs` no puede cargar ni procesar cosméticos o comestibles sin ser modificado. `CargarDesdeArchivo` instanciaría solo `MedicamentoCapsula`. `VerificarVencimiento` asumirá fecha de expiración obligatoria. | `ServicioProducto.cs` (L59-73, L75-118) | Error de casteo o datos nulos si un producto no es medicamento. Carga de archivo fallida o corrupta. |
| **SC-2** (Servicios: inyectología, curaciones) | `ServicioProducto.cs` procesa servicios como si tuvieran stock físico y fecha de expiración. `VerificarStock` y `VerificarVencimiento` fallarán lógicamente. | `ServicioProducto.cs` (L47-73) | Notificaciones falsas de stock mínimo (0 <= stockMinimo) o excepciones en cálculo de fechas. |
| **SC-3** (Convenios corporativos/bancarios) | `ServicioDescuento.cs` aplica un 10% fijo hardcodeado sin distinguir tipo de cliente o convenio. `ServicioCliente.cs` no almacena la entidad del convenio. | `ServicioDescuento.cs` (L13-16), `ServicioCliente.cs` (L65-70) | Imposibilidad de otorgar descuentos diferenciados sin llenar el código de `if/else` o `switch`. |

---

## 5. Conclusión y Recomendaciones de Rediseño

1. **Introducir Capa de Abstracciones (Interfaces de Repositorio y Servicio):**
   - Crear `IClienteRepository`, `IProductoRepository`, `IUsuarioRepository` en `BibFarmacia/Interfaces/`.
   - Definir interfaces de servicios para la capa de negocio (`IServicioProducto`, `IServicioCliente`, etc.).
2. **Eliminar I/O Directo de los Servicios:**
   - Mover la lectura de archivos CSV (`File.ReadAllLines`) a clases repository concretas (`CsvClienteRepository`, `CsvProductoRepository`).
3. **Inyección de Dependencias (DI):**
   - Refactorizar constructores de los servicios para recibir sus dependencias (`IRepository`, `IEventDispatcher`, `IAutenticador`).
4. **Patrón Strategy para Descuentos (SC-3):**
   - Refactorizar `ServicioDescuento` usando el patrón Strategy para admitir diferentes reglas de descuento por convenio o tipo de cliente.
5. **Aprovechar la carpeta `BibFarmacia/Utilidades/`:**
   - Crear utilidades de formateo o abstracciones de lectura/escritura de archivos (`ICsvParser<T>`) para evitar código duplicado.
