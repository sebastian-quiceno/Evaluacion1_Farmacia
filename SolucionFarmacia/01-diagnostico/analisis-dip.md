# Diagnóstico Arquitectónico SOLID: Principio de Inversión de Dependencias (DIP)

**Módulo**: Diagnóstico AS-IS (Fase 1)  
**Proyecto**: SolucionFarmacia (`BibFarmacia` y `AppFarmaciaConsola`)  
**Especialista**: Agente DIP (`worker_dip_m2`)  
**Fecha**: 2026-08-05  
**Ubicación del Documento**: `01-diagnostico/analisis-dip.md`  

---

## 1. Definición del Principio de Inversión de Dependencias (DIP)

El **Principio de Inversión de Dependencias (Dependency Inversion Principle - DIP)** es el quinto principio de la arquitectura SOLID y establece dos reglas fundamentales:

1. **Los módulos de alto nivel no deben depender de módulos de bajo nivel. Ambos deben depender de abstracciones.**
2. **Las abstracciones no deben depender de detalles. Los detalles deben depender de abstracciones.**

### 1.1 Módulos de Alto Nivel vs. Módulos de Bajo Nivel

Para evaluar de forma rigurosa la arquitectura actual del sistema de farmacia, es indispensable clasificar las clases de la solución según su nivel de abstracción:

*   **Módulos de Alto Nivel (Reglas de Negocio y Orquestación)**:
    *   `ServicioProducto`: Gestión del catálogo, control de stock y reglas de vencimiento.
    *   `ServicioCliente`: Gestión de clientes y acumulación de puntos de fidelización.
    *   `ServicioUsuario`: Gestión de usuarios del sistema y autenticación de accesos.
    *   `ServicioMovimiento`: Registro de transacciones de venta e historial de movimientos.
    *   `ServicioDescuento`: Aplicación de políticas de descuento sobre ventas.
    *   `Program.cs`: Punto de entrada y orquestador del flujo interactivo de la aplicación de consola.
*   **Módulos de Bajo Nivel (Detalles Técnicos e Infraestructura)**:
    *   Acceso a archivos físicos en disco mediante `System.IO.File` (`File.ReadAllLines`, `File.Exists`).
    *   Dispositivos de entrada/salida como la consola del sistema (`System.Console.WriteLine`, `System.Console.ReadLine`).
    *   Aspectos estáticos concretos como `AspectoAutenticacion.Login`.
    *   Mecanismos del reloj del sistema operativo (`System.DateTime.Now`).
    *   Factories concretas estáticas (`ProductoFactory`).
    *   Rutas y nombres de archivos de texto en disco (`"productos.txt"`, `"clientes.txt"`, `"usuarios.txt"`).

### 1.2 El Problema de la Inversión Ausente en SolucionFarmacia

En una arquitectura limpia con DIP adecuadamente aplicado, las clases de servicio (alto nivel) y la interfaz de usuario (alto nivel) interactúan exclusivamente mediante **interfaces o abstracciones** (ej. `IProductoRepository`, `INotificador`, `IClock`, `IDescuento`). Las implementaciones concretas de bajo nivel (archivos CSV, salidas por consola, reloj de sistema) se inyectan dinámicamente mediante Inyección de Dependencias (DI).

En el sistema heredado **SolucionFarmacia**, la relación de dependencia está completamente **invertida en el sentido incorrecto**: los módulos de alto nivel están acoplados directamente mediante instanciación con `new`, invocación de métodos estáticos y llamadas directas a APIs del sistema operativo a detalles de bajo nivel.

---

## 2. Evaluación Detallada del DIP en la Solución AS-IS

A continuación se detalla el análisis exhaustivo de las violaciones al DIP encontradas tanto en la biblioteca de clases (`BibFarmacia`) como en la aplicación de consola (`AppFarmaciaConsola`).

---

### 2.1 Módulo de Servicios (`BibFarmacia/Servicios/`)

#### 2.1.1 `ServicioCliente.cs` — Acoplamiento a I/O de Archivos y Eventos Concretos
*   **Ubicación**: `BibFarmacia/Servicios/ServicioCliente.cs`, Líneas 16, 20-22, 47-81.
*   **Síntoma**:
    *   El método `Cargar(string ruta)` (L47-81) invoca directamente a la clase estática de infraestructura `System.IO.File` mediante `File.Exists(ruta)` (L52) y `File.ReadAllLines(ruta)` (L58).
    *   El servicio asume la responsabilidad de parsear manualmente líneas de texto delimitadas por punto y coma (CSV) mediante `linea.Split(';')` (L63) e instanciar directamente objetos de dominio `new Cliente(...)` (L66).
    *   El constructor de `ServicioCliente` (L18-23) instancia directamente colecciones de bajo nivel `new List<Cliente>()` y el evento concreto `new EventoPuntos()` (L22).
*   **Impacto Técnico y de Negocio**:
    *   Imposibilidad de realizar pruebas unitarias sobre la lógica de `ServicioCliente` sin depender de la existencia física de archivos en el sistema de archivos local.
    *   Cualquier migración de persistencia (por ejemplo, a SQL Server, PostgreSQL, MongoDB o una API REST) requerirá modificar directamente el código fuente de `ServicioCliente.cs`.
    *   Si cambia la estructura del archivo CSV (orden de columnas o separador), la regla de negocio de clientes se rompe.
*   **Fragmento de Código Afectado**:
    ```csharp
    // BibFarmacia/Servicios/ServicioCliente.cs:57-73
    string[] lineas = File.ReadAllLines(ruta);

    foreach (string linea in lineas)
    {
        string[] datos = linea.Split(';');
        Cliente cliente = new Cliente(
            datos[0], datos[1], datos[2], datos[3]);
        clientes.Add(cliente);
    }
    ```
*   **Fix Sugerido**:
    *   Definir la interfaz `IClienteRepository` con el método `List<Cliente> ObtenerTodos()`.
    *   Mover la lectura de archivos CSV a una clase concreta `CsvClienteRepository : IClienteRepository`.
    *   Inyectar `IClienteRepository` y una abstracción de publicación de eventos `IEventBus` a través del constructor de `ServicioCliente`.

---

#### 2.1.2 `ServicioProducto.cs` — Dependencia de Disco, Acoplamiento de Parsing y Creación Rígida
*   **Ubicación**: `BibFarmacia/Servicios/ServicioProducto.cs`, Líneas 21, 23-24, 75-118.
*   **Síntoma**:
    *   El método `CargarDesdeArchivo(string ruta)` (L75-118) depende directamente de `File.Exists` y `File.ReadAllLines`.
    *   Adicionalmente, `ServicioProducto` hardcodea la instanciación directa de entidades secundarias como `new Laboratorio(datos[5], "Medellin", "4444444")` (L93-97) y `new MedicamentoCapsula(...)` (L99-107) asignando por defecto el valor de enum `TipoRelleno.Gel`.
    *   El constructor (L19-25) asigna directamente `EventoStock = new EventoStockMinimo()` y `EventoVencimiento = new EventoVencimiento()`.
*   **Impacto Técnico y de Negocio**:
    *   `ServicioProducto` no puede procesar productos que provengan de otra fuente de datos o que tengan formatos de laboratorio distintos.
    *   La lógica del servicio de productos queda amarrada a crear exclusivamente `MedicamentoCapsula`. Si se desea cargar un `MedicamentoLiquido`, cosmetico o bebida (SC-1), `CargarDesdeArchivo` falla conceptualmente y requiere ser editado.
    *   No es posible sustituir los eventos de alerta por notificadores por correo o logs en archivo sin modificar el servicio.
*   **Fragmento de Código Afectado**:
    ```csharp
    // BibFarmacia/Servicios/ServicioProducto.cs:93-107
    Laboratorio laboratorio = new Laboratorio(
        datos[5], "Medellin", "4444444");

    MedicamentoCapsula medicamento = new MedicamentoCapsula(
        datos[0],
        decimal.Parse(datos[1]),
        int.Parse(datos[2]),
        int.Parse(datos[3]),
        DateTime.Parse(datos[4]),
        laboratorio,
        Enum.TipoRelleno.Gel);
    ```
*   **Fix Sugerido**:
    *   Definir la interfaz `IProductoRepository`.
    *   Mover el parsing y la construcción de productos al repositorio o a un deserializador polimórfico.
    *   Inyectar `IProductoRepository` en `ServicioProducto` por constructor.

---

#### 2.1.3 `ServicioUsuario.cs` — Invocación Estática de `AspectoAutenticacion` e I/O de Archivos
*   **Ubicación**: `BibFarmacia/Servicios/ServicioUsuario.cs`, Líneas 31, 37-73.
*   **Síntoma**:
    *   En el método `Login` (L27-35), `ServicioUsuario` delega la validación de credenciales invocando directamente el método estático `AspectoAutenticacion.Login(usuarios, user, password)` (L31).
    *   En `Cargar` (L37-73), depende de `File.ReadAllLines` y parsea manualmente el CSV de usuarios.
*   **Impacto Técnico y de Negocio**:
    *   Incapacidad para cambiar el algoritmo de autenticación (ej. hashing BCrypt, OAuth2, JWT) o sustituir la autenticación por un mock en pruebas unitarias debido al acoplamiento a una clase estática.
    *   Riesgo de seguridad al leer contraseñas en texto plano desde archivos planos sin una abstracción de cifrado o repositorio seguro.
*   **Fragmento de Código Afectado**:
    ```csharp
    // BibFarmacia/Servicios/ServicioUsuario.cs:27-35
    public bool Login(string user, string password)
    {
        return AspectoAutenticacion.Login(
            usuarios,
            user,
            password);
    }
    ```
*   **Fix Sugerido**:
    *   Definir la interfaz `IAutenticador` o `IAuthenticationService`.
    *   Inyectar `IAutenticador` y `IUsuarioRepository` en `ServicioUsuario`.

---

#### 2.1.4 `ServicioNotificacion.cs` — Acoplamiento Directo a la Consola del Sistema
*   **Ubicación**: `BibFarmacia/Servicios/ServicioNotificacion.cs`, Línea 14.
*   **Síntoma**:
    *   Aunque la clase implementa la interfaz `IServicioNotificacion`, su método `EnviarNotificacion(string mensaje)` invoca directamente a `System.Console.WriteLine($"[NOTIFICACION] {mensaje}")`.
*   **Impacto Técnico y de Negocio**:
    *   La notificación está rígida al canal de salida por pantalla física. Si el sistema evoluciona a un servicio web, API o proceso en segundo plano, las notificaciones se perderán o imprimirán en la consola del servidor sin dejar rastro.
    *   No permite enviar notificaciones a múltiples canales (email, SMS, WhatsApp) sin modificar la clase o crear múltiples clases sin un despacho dinámico.
*   **Fragmento de Código Afectado**:
    ```csharp
    // BibFarmacia/Servicios/ServicioNotificacion.cs:12-15
    public void EnviarNotificacion(string mensaje)
    {
        Console.WriteLine($"[NOTIFICACION] {mensaje}");
    }
    ```
*   **Fix Sugerido**:
    *   Parametrizar o abstraer el canal de salida mediante un `ISink` o `ILogger` (por ejemplo `ILogger<ServicioNotificacion>` de `Microsoft.Extensions.Logging`).

---

### 2.2 Módulo de Factories y Aspectos (`BibFarmacia/Factories/` y `BibFarmacia/Aspectos/`)

#### 2.2.1 `ProductoFactory.cs` — Dependencia del Reloj de Sistema (`DateTime.Now`) y Tipos Concretos
*   **Ubicación**: `BibFarmacia/Factories/ProductoFactory.cs`, Líneas 13-27, 28-43.
*   **Síntoma**:
    *   Los métodos estáticos `CrearCapsula` (L13) y `CrearLiquido` (L28) invocan directamente `DateTime.Now.AddMonths(6)` (L24) y `DateTime.Now.AddMonths(12)` (L39).
    *   Ambos métodos retornan clases concretas (`MedicamentoCapsula` y `MedicamentoLiquido`) en lugar de retornar la abstracción `Producto` o una interfaz `IProducto`.
    *   Adicionalmente, instancian directamente con `new` los productos concretos hardcodeando valores de `stockMinimo = 5`, `TipoRelleno.Gel` y `MaterialEnvase.Vidrio`.
*   **Impacto Técnico y de Negocio**:
    *   **Pruebas Unitarias No Deterministas**: Cualquier prueba unitaria ejecutada sobre `ProductoFactory` producirá una fecha de vencimiento diferente dependiendo del día y segundo en que se ejecute la prueba, imposibilitando aserciones exactas de fecha.
    *   Los clientes de la fábrica quedan acoplados a las clases concretas de los medicamentos.
*   **Fragmento de Código Afectado**:
    ```csharp
    // BibFarmacia/Factories/ProductoFactory.cs:19-26
    return new MedicamentoCapsula(
        nombre,
        precio,
        stock,
        5,
        DateTime.Now.AddMonths(6),
        laboratorio,
        TipoRelleno.Gel);
    ```
*   **Fix Sugerido**:
    *   Crear una abstracción para el proveedor de tiempo `IDateTimeProvider` con el método `DateTime Now { get; }`.
    *   Convertir `ProductoFactory` en una clase de instancia que implemente `IProductoFactory` e inyectarle `IDateTimeProvider`.
    *   Hacer que los métodos de la fábrica retornen `Producto` o `IProducto`.

---

#### 2.2.2 `AspectoAutenticacion.cs` — Acoplamiento a `List<Usuario>` Concreta
*   **Ubicación**: `BibFarmacia/Aspectos/AspectoAutenticacion.cs`, Línea 14.
*   **Síntoma**:
    *   El método estático `Login` recibe como primer parámetro la colección concreta `List<Usuario> usuarios`.
*   **Impacto Técnico y de Negocio**:
    *   Rígido acoplamiento a una estructura de datos en memoria concreta (`List<T>`). No permite pasar un `IEnumerable<Usuario>`, un `HashSet<Usuario>`, ni realizar consultas asíncronas directas contra una base de datos (`IQueryable<Usuario>`).
*   **Fix Sugerido**:
    *   Cambiar la firma a `IEnumerable<Usuario>` o eliminar la clase estática a favor de un `IAuthenticationService` inyectable.

---

### 2.3 Aplicación de Consola (`AppFarmaciaConsola/Program.cs`)

#### 2.3.1 `Program.cs` — Ausencia Total de Inyección de Dependencias (DI) e Instanciación Rígida con `new`
*   **Ubicación**: `AppFarmaciaConsola/Program.cs`, Líneas 8-18, 283.
*   **Síntoma**:
    *   `Program.cs` instancian directamente todas las implementaciones concretas de los servicios:
        *   `ServicioProducto servicioProducto = new ServicioProducto();` (L8-9)
        *   `ServicioCliente servicioCliente = new ServicioCliente();` (L11-12)
        *   `ServicioUsuario servicioUsuario = new ServicioUsuario();` (L14-15)
        *   `ServicioMovimiento servicioMovimiento = new ServicioMovimiento();` (L17-18)
    *   En el registro de ventas (L283), instancia directamente la entidad `new Movimiento(...)`.
    *   No existe un contenedor de Inyección de Dependencias (DI Container) ni uso de interfaces para declarar las variables.
*   **Impacto Técnico y de Negocio**:
    *   No se puede sustituir ningún servicio por una versión de prueba (Mock/Stub), por una implementación con almacenamiento en Base de Datos o por una versión con cachés.
    *   El punto de entrada principal depende 100% de la infraestructura física del sistema.
*   **Fragmento de Código Afectado**:
    ```csharp
    // AppFarmaciaConsola/Program.cs:8-18
    ServicioProducto servicioProducto = new ServicioProducto();
    ServicioCliente servicioCliente = new ServicioCliente();
    ServicioUsuario servicioUsuario = new ServicioUsuario();
    ServicioMovimiento servicioMovimiento = new ServicioMovimiento();
    ```
*   **Fix Sugerido**:
    *   Configurar `Host.CreateDefaultBuilder()` o `ServiceCollection` de .NET 8.
    *   Registrar interfaces y sus implementaciones (`services.AddSingleton<IProductoRepository, CsvProductoRepository>()`, etc.).
    *   Resolver los servicios mediante el contenedor de dependencias.

---

#### 2.3.2 `Program.cs` — Rutas y Nombres de Archivos Hardcodeados en Código
*   **Ubicación**: `AppFarmaciaConsola/Program.cs`, Líneas 79, 83, 87.
*   **Síntoma**:
    *   El orquestador de consola pasa cadenas de caracteres fijas ("quemadas") con los nombres de archivo:
        *   `"productos.txt"` (L79)
        *   `"clientes.txt"` (L83)
        *   `"usuarios.txt"` (L87)
*   **Impacto Técnico y de Negocio**:
    *   Imposibilidad de cambiar el origen o la ubicación de los archivos sin recompilar la aplicación.
    *   Falla inmediata de ejecución si la consola se ejecuta desde una carpeta de trabajo (CWD) diferente a donde se encuentran los archivos `.txt`.
*   **Fragmento de Código Afectado**:
    ```csharp
    // AppFarmaciaConsola/Program.cs:78-87
    Console.WriteLine(servicioProducto.CargarDesdeArchivo("productos.txt"));
    Console.WriteLine(servicioCliente.Cargar("clientes.txt"));
    Console.WriteLine(servicioUsuario.Cargar("usuarios.txt"));
    ```
*   **Fix Sugerido**:
    *   Mover la configuración de rutas a un archivo `appsettings.json` inyectado mediante `IConfiguration`.

---

### 2.4 Cumplimiento Sintáctico vs. Inversión Real

Es relevante destacar que en el sistema existen dos interfaces declaradas:
*   `IDescuento` (`BibFarmacia/Interfaces/IDescuento.cs`) implementada por `ServicioDescuento`.
*   `IServicioNotificacion` (`BibFarmacia/Interfaces/IServicioNotificacion.cs`) implementada por `ServicioNotificacion`.

**¿Por qué esto representa únicamente cumplimiento sintáctico pero NO cumplimiento real del DIP?**
1.  **Sin Inyección**: En ningún lugar de la solución (`Program.cs` ni otros servicios) se inyecta `IDescuento` ni `IServicioNotificacion` a través de constructores.
2.  **No se utilicen**: `Program.cs` declara las variables directamente por su tipo concreto (`ServicioProducto`, `ServicioCliente`, etc.) omitiendo el uso de abstracciones.
3.  **Implementación Concreta de Bajo Nivel**: `ServicioNotificacion` sigue acoplado a `Console.WriteLine` (bajo nivel) en lugar de depender de un sink de logs abstraído.

---

## 3. Tabla Resumen de Evaluación del Principio DIP

A continuación se presenta la tabla oficial consolidada que evalúa el cumplimiento del Principio de Inversión de Dependencias en todos los componentes analizados:

| Principio | ¿Cumple? | Evidencia (archivo/línea) | Fix sugerido |
| :--- | :--- | :--- | :--- |
| **DIP** | **NO** | `BibFarmacia/Servicios/ServicioCliente.cs`<br>L47-81 | Extraer `IClienteRepository` para desacoplar `File.ReadAllLines` y el parsing CSV de la lógica del servicio. |
| **DIP** | **NO** | `BibFarmacia/Servicios/ServicioCliente.cs`<br>L22 | Inyectar el publicador de eventos por constructor (`IEventBus`) en lugar de instanciar `new EventoPuntos()`. |
| **DIP** | **NO** | `BibFarmacia/Servicios/ServicioProducto.cs`<br>L75-118 | Extraer `IProductoRepository` y deserializador polimórfico; eliminar `File.ReadAllLines` e instanciación directa de `Laboratorio` y `MedicamentoCapsula`. |
| **DIP** | **NO** | `BibFarmacia/Servicios/ServicioProducto.cs`<br>L23-24 | Inyectar abstraídos de eventos (`IEventBus`) en lugar de instanciar `new EventoStockMinimo()` y `new EventoVencimiento()`. |
| **DIP** | **NO** | `BibFarmacia/Servicios/ServicioUsuario.cs`<br>L37-73 | Delegar la persistencia de usuarios a `IUsuarioRepository` para aislar el I/O de disco. |
| **DIP** | **NO** | `BibFarmacia/Servicios/ServicioUsuario.cs`<br>L31 | Inyectar una abstracción `IAutenticador` en lugar de llamar directamente al método estático `AspectoAutenticacion.Login`. |
| **DIP** | **NO** | `BibFarmacia/Servicios/ServicioNotificacion.cs`<br>L14 | Inyectar un proveedor de salida/log (`ILogger` o `INotificadorSink`) en lugar de llamar directamente a `Console.WriteLine`. |
| **DIP** | **NO** | `BibFarmacia/Factories/ProductoFactory.cs`<br>L24, L39 | Inyectar `IDateTimeProvider` para abstraer `DateTime.Now` y retornar abstracciones (`Producto` / `IProducto`) en lugar de tipos concretos. |
| **DIP** | **NO** | `BibFarmacia/Aspectos/AspectoAutenticacion.cs`<br>L14 | Sustituir el parámetro rígido `List<Usuario>` por `IEnumerable<Usuario>` o encapsular en `IUsuarioRepository`. |
| **DIP** | **NO** | `AppFarmaciaConsola/Program.cs`<br>L8-18 | Usar el contenedor de DI de .NET (`IServiceCollection`) e inyectar interfaces de servicios (`IProductoService`, etc.) en lugar de `new ServicioProducto()`. |
| **DIP** | **NO** | `AppFarmaciaConsola/Program.cs`<br>L79, L83, L87 | Mover los nombres de archivo hardcodeados (`"productos.txt"`, etc.) a un proveedor de configuración (`IConfiguration` / `appsettings.json`). |
| **DIP** | **NO** | `AppFarmaciaConsola/Program.cs`<br>L283 | Delegar la creación y registro de transacciones al servicio inyectado en lugar de `new Movimiento(...)` directo en la UI. |
| **DIP** | **Parcial** | `BibFarmacia/Servicios/ServicioDescuento.cs`<br>L10 | Sintácticamente implementa `IDescuento`, pero ningún cliente inyecta ni consume `IDescuento` mediante Inyección de Dependencias. |
| **DIP** | **Parcial** | `BibFarmacia/Servicios/ServicioNotificacion.cs`<br>L10 | Sintácticamente implementa `IServicioNotificacion`, pero no se inyecta por DI y internamente depende de `Console.WriteLine`. |

---

## 4. Impacto de las Violaciones DIP en las Solicitudes de Cambio (SC)

La falta de inversión de dependencias representa la barrera técnica más alta para extender el sistema ante las tres solicitudes de cambio futuras formuladas por el negocio:

### 4.1 SC-1: Venta de Cosméticos, Comestibles y Abarrotes
*   **Limitación DIP Actual**: `ServicioProducto.CargarDesdeArchivo` (L99) instancia directamente `new MedicamentoCapsula(...)` y `Laboratorio` (L93). `Program.cs` depende directamente de esta firma concreta.
*   **Efecto**: Para cargar cosméticos o bebidas desde archivo o base de datos, es imposible reutilizar `ServicioProducto` sin modificar su código interno, ya que está acoplado al parsing de laboratorios y medicamentos.
*   **Con DIP Aplicado**: Con un `IProductoRepository` inyectado y un deserializador polimórfico, se pueden agregar nuevos repositorios o formatos de productos sin tocar `ServicioProducto` ni `Program.cs`.

### 4.2 SC-2: Venta de Servicios (Inyectología, Curaciones, Vendajes)
*   **Limitación DIP Actual**: `Program.cs` instancia servicios de productos que asumen I/O de archivos físicos de productos e inventarios mutables. `Movimiento` (L14) depende concretamente de la clase abstracta `Producto`.
*   **Efecto**: Registrar un servicio médico requiere crear instancias de un repositorio o entidad que no posee stock ni vencimiento. Al no haber abstracciones como `IVendible` o `IServicioSaludRepository`, el flujo de consola y los servicios concretos fallan.
*   **Con DIP Aplicado**: `ServicioMovimiento` dependería de `IVendible` (abstracción), permitiendo registrar tanto productos como servicios de inyectología indistintamente.

### 4.3 SC-3: Convenios Corporativos, Bancarios y Descuentos Especiales
*   **Limitación DIP Actual**: `ServicioDescuento` no se inyecta en ninguna parte del sistema; `Program.cs` ni siquiera lo crea ni utiliza. Además, `ServicioUsuario` depende del método estático `AspectoAutenticacion.Login`.
*   **Efecto**: Imposible aplicar reglas de descuento dinámicas por convenio o consultar convenios desde un servicio externo/banco sin modificar masivamente `ServicioDescuento.cs` y `Program.cs`.
*   **Con DIP Aplicado**: Se inyectaría un `IDescuentoStrategyFactory` o `IDescuentoService` en el orquestador de ventas, permitiendo resolver el descuento según la entidad del convenio en tiempo de ejecución.

---

## 5. Plan de Rediseño TO-BE para Cumplimiento Estricto del DIP

Para transformar la arquitectura actual en un diseño desacoplado, mantenible y 100% testeable, se establece la siguiente hoja de ruta de refactorización para la Fase 2 (TO-BE):

```
                               ┌───────────────────────────┐
                               │   AppFarmaciaConsola     │
                               │        (Program)          │
                               └─────────────┬─────────────┘
                                             │ Inyecta via DI
                                             ▼
                               ┌───────────────────────────┐
                               │   Interfaces de Servicio  │
                               │    (IServicioProducto,    │
                               │     IServicioCliente)     │
                               └─────────────┬─────────────┘
                                             │ Implementa
                                             ▼
┌───────────────────────────┐  ┌───────────────────────────┐  ┌───────────────────────────┐
│  Interfaces Repositorio   │◄─┤   Servicios de Negocio    ├─►│ Provider Abstracciones    │
│    (IProductoRepository,  │  │   (ServicioProducto, etc) │  │  (IDateTimeProvider,      │
│     IClienteRepository)   │  └───────────────────────────┘  │   IEventBus, ILogger)     │
└─────────────▲─────────────┘                                 └─────────────▲─────────────┘
              │ Implementa                                                  │ Implementa
┌─────────────┴─────────────┐                                 ┌─────────────┴─────────────┐
│  Implementaciones Csv/Db  │                                 │ Implementaciones Concretas│
│   (CsvProductoRepository) │                                 │  (SystemClock, ConsoleLog)│
└───────────────────────────┘                                 └───────────────────────────┘
```

### 5.1 Paso 1: Creación del Paquete de Abstracciones (`BibFarmacia/Interfaces/`)
*   `IProductoRepository`: Métodos `IEnumerable<Producto> ObtenerTodos()`, `void Guardar(Producto producto)`.
*   `IClienteRepository`: Métodos `IEnumerable<Cliente> ObtenerTodos()`, `void Guardar(Cliente cliente)`.
*   `IUsuarioRepository`: Métodos `IEnumerable<Usuario> ObtenerTodos()`, `Usuario? ObtenerPorUsername(string username)`.
*   `IDateTimeProvider`: Propiedad `DateTime Now { get; }`.
*   `IEventBus`: Método `void Publicar<TEvent>(TEvent evento)`.
*   `IServicioProducto`, `IServicioCliente`, `IServicioUsuario`, `IServicioMovimiento`.

### 5.2 Paso 2: Implementación de Infraestructura Desacoplada
*   Crear `BibFarmacia.Infraestructura.Persistencia`:
    *   `CsvProductoRepository : IProductoRepository` (contiene la lógica de `File.ReadAllLines` y parsing CSV).
    *   `CsvClienteRepository : IClienteRepository`.
    *   `CsvUsuarioRepository : IUsuarioRepository`.
*   Crear `BibFarmacia.Infraestructura.Tiempo`:
    *   `SystemDateTimeProvider : IDateTimeProvider`.

### 5.3 Paso 3: Inyección de Dependencias en Servicios
*   Refactorizar `ServicioProducto`:
    ```csharp
    public class ServicioProducto : IServicioProducto
    {
        private readonly IProductoRepository _repository;
        private readonly IEventBus _eventBus;

        public ServicioProducto(
            IProductoRepository repository,
            IEventBus eventBus)
        {
            _repository = repository;
            _eventBus = eventBus;
        }
    }
    ```

### 5.4 Paso 4: Configuración del Contenedor de IoC en `Program.cs`
*   Usar `Microsoft.Extensions.DependencyInjection` en `AppFarmaciaConsola`:
    ```csharp
    var serviceProvider = new ServiceCollection()
        .AddSingleton<IDateTimeProvider, SystemDateTimeProvider>()
        .AddSingleton<IProductoRepository>(sp => 
            new CsvProductoRepository(config["Rutas:Productos"]))
        .AddSingleton<IClienteRepository>(sp => 
            new CsvClienteRepository(config["Rutas:Clientes"]))
        .AddSingleton<IServicioProducto, ServicioProducto>()
        .AddSingleton<IServicioCliente, ServicioCliente>()
        .BuildServiceProvider();

    var servicioProducto = serviceProvider.GetRequiredService<IServicioProducto>();
    ```

---

## 6. Conclusión

El diagnóstico AS-IS confirma que **SolucionFarmacia viola el Principio de Inversión de Dependencias (DIP) en todas sus capas principales**. Los servicios de alto nivel dependen directamente de APIs de bajo nivel (`File.ReadAllLines`, `Console.WriteLine`, `DateTime.Now`), instancian colecciones y eventos concretos con `new`, invocan métodos estáticos de autenticación y están acoplados a rutas de archivo fijas.

La adopción de interfaces de repositorio, proveedores de tiempo y un contenedor de Inyección de Dependencias en la Fase 2 resolverá radicalmente estas deficiencias, garantizando un sistema modular, extensible ante las solicitudes SC-1, SC-2 y SC-3, y 100% asegurable mediante pruebas unitarias automatizadas.
