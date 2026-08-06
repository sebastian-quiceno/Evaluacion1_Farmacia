# Diagnóstico Arquitectónico SOLID — Principio de Responsabilidad Única (SRP)

**Sistema**: SolucionFarmacia (C# .NET 8)  
**Fase**: 01-diagnostico (AS-IS)  
**Agente Especialista**: `worker_srp_m2` (Single Responsibility Principle Specialist)  
**Fecha de Emisión**: 2026-08-05  
**Proyectos Evaluados**: `BibFarmacia` (26 archivos `.cs`) y `AppFarmaciaConsola` (`Program.cs`, 378 líneas)

---

## 1. Introducción y Definición del Principio

El **Principio de Responsabilidad Única (Single Responsibility Principle — SRP)**, formulado por Robert C. Martin, establece que:

> *"Una clase debe tener una, y solo una, razón para cambiar."*

En términos de diseño arquitectónico, una "razón para cambiar" corresponde a una responsabilidad o interés no cohesivo asignado a un componente. Cuando un módulo asume múltiples responsabilidades (por ejemplo, gestión de estado de dominio, I/O de archivos en disco, formato de interfaz de usuario de consola, validaciones o instanciación de servicios), cualquier modificación en las reglas de negocio, formatos de almacenamiento o requerimientos estéticos impacta al mismo componente. Esto incrementa de manera drástica el riesgo de regresiones, dificulta la reutilización de código y anula la posibilidad de realizar pruebas unitarias automatizadas.

El presente documento expone el diagnóstico exhaustivo de cumplimiento y violación del SRP en el sistema heredado **SolucionFarmacia**, evaluando la totalidad de clases, interfaces, aspectos, fábricas, eventos y servicios presentes en la solución.

---

## 2. Metodología de Evaluación

Para la evaluación del principio SRP sobre el código fuente actual (AS-IS), se aplicó el siguiente procedimiento:

1. **Identificación de Responsabilidades**: Inspección detallada de cada clase y módulo en ambos proyectos (`BibFarmacia` y `AppFarmaciaConsola`).
2. **Análisis de Ejes de Cambio (Reasons to Change)**: Enumeración explícita de los diferentes motivos o actores por los cuales la clase requeriría ser modificada en el futuro.
3. **Detección de Mezcla de Capas**: Identificación de acoplamientos entre lógica de dominio, persistencia física (`System.IO.File`), presentación visual (`System.Console`) e infraestructura de eventos.
4. **Trazabilidad de Evidencia**: Citación del fragmento exacto de código, ruta relativa del archivo, nombre de clase y rango exacto de líneas.
5. **Documentación de Cumplimiento**: Registro explícito de aquellos componentes que sí mantienen una única responsabilidad cohesiva.
6. **Formulación de Propuestas de Rediseño (Fix Sugerido)**: Definición del mecanismo de refactorización requerido (extracción de repositorios, separación de validadores, formateadores UI, etc.).

---

## 3. Hallazgos Detallados de Violación del Principio (SRP)

### H-SRP-01: `AppFarmaciaConsola/Program.cs` — Script Todopoderoso (God Script)

- **Ubicación**: `AppFarmaciaConsola/Program.cs` (Líneas 1 a 378)
- **Clase/Módulo**: Punto de entrada de la aplicación (`Top-Level Statements`)
- **Síntoma Observado**: `Program.cs` acumula la totalidad del control de la aplicación en un único archivo de 378 líneas, asumiendo 7 responsabilidades completamente heterogéneas.

#### Fragmento de Código Relevante:
```csharp
// Instanciación directa de servicios (L8-18)
ServicioProducto servicioProducto = new ServicioProducto();
ServicioCliente servicioCliente = new ServicioCliente();
ServicioUsuario servicioUsuario = new ServicioUsuario();
ServicioMovimiento servicioMovimiento = new ServicioMovimiento();

// Carga inicial desde archivos hardcodeados (L78-87)
Console.WriteLine(servicioProducto.CargarDesdeArchivo("productos.txt"));
Console.WriteLine(servicioCliente.Cargar("clientes.txt"));
Console.WriteLine(servicioUsuario.Cargar("usuarios.txt"));

// Búsqueda LINQ directa en la UI y mutación directa de dominio (L263-288)
var productoVenta = servicioProducto.ObtenerProductos()
    .FirstOrDefault(p => p.Nombre.ToLower().Contains(nombreVenta.ToLower()));

if (productoVenta != null)
{
    Console.Write("Cantidad: ");
    int cantidad = int.Parse(Console.ReadLine()!);
    productoVenta.Stock -= cantidad; // <--- Mutación directa de estado
    Movimiento venta = new Movimiento(DateTime.Now, cantidad, "Venta", productoVenta);
    servicioMovimiento.RegistrarMovimiento(venta);
}
```

#### Razones para Cambiar Identificadas (7 Ejes de Cambio):
1. **Presentación de UI y Formato Estético**: Modificaciones en la interfaz de consola, bordes ASCII (`===`), títulos, y esquemas de color (`ConsoleColor.Cyan`, `Green`, `Red`, `Magenta`, `Yellow`).
2. **Entrada/Salida de Consola y Parsing**: Cambios en la captura de entradas (`Console.ReadLine()`) o conversión insegura de datos numéricos (`int.Parse(...)`).
3. **Orquestación del Flujo de la Aplicación**: Ajustes en la secuencia de ciclo de vida (Login -> Menú Principal -> Selección de Opción -> Finalización).
4. **Lógica de Consultas de Negocio**: Expresiones LINQ de filtrado (`.FirstOrDefault(...)`) escritas directamente en las cláusulas `case` del menú.
5. **Mutación de Estado de Dominio**: Modificación directa de inventario (`productoVenta.Stock -= cantidad;`), violando la encapsulación de las reglas de negocio.
6. **Instanciación y Acoplamiento de Servicios**: Creación rígida con `new` de servicios concretos y construcción manual de transacciones (`new Movimiento(...)`).
7. **Gestión de Configuración y Rutas de Archivos**: Manejo directo de nombres de archivos persistentes (`"productos.txt"`, `"clientes.txt"`, `"usuarios.txt"`).

- **Impacto en el Negocio**: Severidad **Alta**. Cualquier modificación en la estética o interfaz de usuario corre el riesgo de corromper la ejecución del flujo transaccional de ventas o provocar fallos catastróficos en producción.
- **Fix Sugerido**: Descomponer `Program.cs` aplicando Clean Architecture: extraer un controlador de consola (`ConsoleUI`), un lector seguro de entradas (`ConsoleInputReader`), mediadores/casos de uso de negocio (`RealizarVentaUseCase`) y configurar inyección de dependencias (`Microsoft.Extensions.DependencyInjection`).

---

### H-SRP-02: `BibFarmacia/Servicios/ServicioProducto.cs` — Servicio Monolítico Multirresponsabilidad

- **Ubicación**: `BibFarmacia/Servicios/ServicioProducto.cs` (Líneas 12 a 119)
- **Clase/Módulo**: `ServicioProducto`
- **Síntoma Observado**: Mezcla la gestión del catálogo de productos en memoria con reglas de evaluación de alertas de inventario, I/O físico de archivos CSV y construcción hardcodeada de objetos de dominio.

#### Fragmento de Código Relevante:
```csharp
// Evaluación de reglas de alerta (L47-73)
public void VerificarVencimiento()
{
    foreach (var producto in productos)
    {
        int dias = (producto.FechaVencimiento - DateTime.Now).Days;
        if (dias <= 30)
        {
            EventoVencimiento.Disparar(producto);
        }
    }
}

// Persistencia física e instanciación rígida (L75-118)
public string CargarDesdeArchivo(string ruta)
{
    if (!File.Exists(ruta)) return "Archivo no encontrado";
    string[] lineas = File.ReadAllLines(ruta);
    foreach (string linea in lineas)
    {
        string[] datos = linea.Split(';');
        Laboratorio laboratorio = new Laboratorio(datos[5], "Medellin", "4444444"); // <--- Objeto inventado
        MedicamentoCapsula medicamento = new MedicamentoCapsula(
            datos[0], decimal.Parse(datos[1]), int.Parse(datos[2]), int.Parse(datos[3]),
            DateTime.Parse(datos[4]), laboratorio, Enum.TipoRelleno.Gel); // <--- Instanciación concreta
        productos.Add(medicamento);
    }
    return "Productos cargados";
}
```

#### Razones para Cambiar Identificadas (4 Ejes de Cambio):
1. **Gestión del Catálogo en Memoria**: Modificación de las operaciones de consulta o adición de productos (`AgregarProducto`, `ObtenerProductos`).
2. **Evaluación de Reglas y Políticas de Alertas**: Cambios en los umbrales de stock mínimo o en la ventana de días de vencimiento (ej. cambiar de 30 días a 15 días o parametrizar por categoría).
3. **Persistencia e I/O de Archivos**: Cambios en la fuente de datos (migrar de archivo de texto CSV a base de datos relacional SQL, JSON o API REST) o del delimitador del CSV.
4. **Construcción de Entidades de Dominio**: Ajustes en los datos por defecto asignados al laboratorio (`"Medellin"`, `"4444444"`) o en la instanciación exclusiva de `MedicamentoCapsula`.

- **Impacto en el Negocio**: Severidad **Alta**. Imposibilita la extensión para vender productos cosméticos/comestibles (SC-1) o servicios (SC-2), ya que la carga de archivos asume forzosamente medicamentos y la verificación de expiración exige fechas válidas.
- **Fix Sugerido**: Delegar la persistencia a un repositorio aislado (`IProductoRepository` / `CsvProductoRepository`), extraer la evaluación de alertas a un servicio de dominio (`StockAlertEvaluator`), y desacoplar la creación de objetos mediante fábricas deserializadoras.

---

### H-SRP-03: `BibFarmacia/Servicios/ServicioCliente.cs` — Mezcla de Negocio, I/O CSV y Eventos

- **Ubicación**: `BibFarmacia/Servicios/ServicioCliente.cs` (Líneas 12 a 82)
- **Clase/Módulo**: `ServicioCliente`
- **Síntoma Observado**: La clase combina la administración del estado de los clientes en memoria y acumulación de puntos con la lectura física del disco (`File.ReadAllLines`), parsing CSV e instanciación de eventos.

#### Fragmento de Código Relevante:
```csharp
public void AcumularPuntos(Cliente cliente, int puntos)
{
    cliente.Puntos += puntos;
    EventoPuntos.Disparar(cliente.Nombre, puntos);
}

public string Cargar(string ruta)
{
    if (!File.Exists(ruta)) return "Archivo no encontrado";
    string[] lineas = File.ReadAllLines(ruta);
    foreach (string linea in lineas)
    {
        string[] datos = linea.Split(';');
        Cliente cliente = new Cliente(datos[0], datos[1], datos[2], datos[3]);
        clientes.Add(cliente);
    }
    return "Clientes cargados";
}
```

#### Razones para Cambiar Identificadas (3 Ejes de Cambio):
1. **Reglas de Dominio de Clientes y Fidelización**: Cambios en la lógica de acumulación de puntos o reglas de clientes.
2. **Persistencia y Acceso a Datos**: Modificaciones en el formato de archivo, ruta o migración a base de datos.
3. **Gestión de Eventos de Infraestructura**: Cambios en la instanciación o despacho de `EventoPuntos`.

- **Impacto en el Negocio**: Severidad **Alta**. Imposible reutilizar la lógica de negocio de clientes en pruebas unitarias en memoria sin requerir la presencia de archivos físicos en disco.
- **Fix Sugerido**: Extraer la carga de datos a un componente `IClienteRepository` e inyectar el publicador de eventos por constructor.

---

### H-SRP-04: `BibFarmacia/Servicios/ServicioUsuario.cs` — Mezcla de Negocio, Autenticación y I/O CSV

- **Ubicación**: `BibFarmacia/Servicios/ServicioUsuario.cs` (Líneas 12 a 74)
- **Clase/Módulo**: `ServicioUsuario`
- **Síntoma Observado**: Mezcla la colección de usuarios en memoria con la invocación de helpers estáticos de autenticación y lectura directa de archivos CSV desde disco.

#### Fragmento de Código Relevante:
```csharp
public bool Login(string user, string password)
{
    return AspectoAutenticacion.Login(usuarios, user, password);
}

public string Cargar(string ruta)
{
    if (!File.Exists(ruta)) return "Archivo no encontrado";
    string[] lineas = File.ReadAllLines(ruta);
    foreach (string linea in lineas)
    {
        string[] datos = linea.Split(';');
        Usuario usuario = new Usuario(datos[0], datos[1], datos[2], datos[3], datos[4], datos[5]);
        usuarios.Add(usuario);
    }
    return "Usuarios cargados";
}
```

#### Razones para Cambiar Identificadas (3 Ejes de Cambio):
1. **Gestión de Usuarios en Memoria**: Ajustes en la administración del listado de usuarios.
2. **Estrategia de Autenticación**: Cambios en el mecanismo de validación de credenciales (ej. migrar de texto plano a hashes BCrypt/Argon2).
3. **Persistencia e I/O de Archivos**: Cambios en el almacenamiento o formato del archivo `usuarios.txt`.

- **Impacto en el Negocio**: Severidad **Alta**. Vulnerabilidad de seguridad y riesgo de mantenimiento al tener acoplada la lectura de credenciales con la lógica del servicio.
- **Fix Sugerido**: Mover I/O a `IUsuarioRepository` e inyectar un servicio de autenticación (`IAuthenticationService`).

---

### H-SRP-05: `BibFarmacia/Clases/Producto.cs` — Entidad de Dominio con Presentación de Consola

- **Ubicación**: `BibFarmacia/Clases/Producto.cs` (Líneas 8 a 35)
- **Clase/Módulo**: `Producto`
- **Síntoma Observado**: La entidad abstracta `Producto` encapsula el estado del dominio (`Nombre`, `Precio`, `Stock`, `StockMinimo`, `FechaVencimiento`), pero contiene el método `MostrarInformacion()` que escribe directamente en `Console.WriteLine`.

#### Fragmento de Código Relevante:
```csharp
public virtual void MostrarInformacion()
{
    Console.WriteLine($"Producto: {Nombre}");
    Console.WriteLine($"Precio: {Precio}");
    Console.WriteLine($"Stock: {Stock}");
}
```

#### Razones para Cambiar Identificadas (2 Ejes de Cambio):
1. **Modificación del Estado o Modelo de Dominio**: Adición o cambio de atributos/reglas de negocio de un producto.
2. **Modificación de la Capa de Presentación o Canal de Salida**: Cambios en la forma de desplegar la información (ej. formateo JSON, interfaz gráfica, aplicación web o servicios REST).

- **Impacto en el Negocio**: Severidad **Media**. Impide utilizar la librería de clases `BibFarmacia` en aplicaciones web, móviles o servicios backend sin generar salidas colaterales no deseadas en la consola estándar.
- **Fix Sugerido**: Eliminar `MostrarInformacion()` de la clase de dominio `Producto`. La responsabilidad de formatear y desplegar productos pertenece exclusivamente a la capa de UI o a vistas/presentadores dedicados.

---

### H-SRP-06: `BibFarmacia/Aspectos/AspectoValidacion.cs` — Validador Estático Multientidad

- **Ubicación**: `BibFarmacia/Aspectos/AspectoValidacion.cs` (Líneas 11 a 45)
- **Clase/Módulo**: `AspectoValidacion`
- **Síntoma Observado**: Clase estática que agrupa reglas de validación para múltiples entidades de dominio no relacionadas (`Cliente` y `Producto`) y retorna cadenas de texto orientadas a la UI (`string`).

#### Fragmento de Código Relevante:
```csharp
public static string ValidarCliente(Cliente cliente)
{
    if (string.IsNullOrWhiteSpace(cliente.Nombre)) return "Nombre inválido";
    if (cliente.Cedula.Length < 3) return "Cédula inválida";
    return "Cliente válido";
}

public static string ValidarProducto(Producto producto)
{
    if (producto.Precio <= 0) return "Precio inválido";
    if (producto.Stock < 0) return "Stock inválido";
    return "Producto válido";
}
```

#### Razones para Cambiar Identificadas (3 Ejes de Cambio):
1. **Reglas de Validación de Cliente**: Modificaciones en la validación de clientes (cédula, correo, teléfono).
2. **Reglas de Validación de Producto**: Modificaciones en la validación de productos (precios, reglas de stock).
3. **Formato/Contrato del Resultado de Validación**: Cambios en la estructura del resultado de validación (ej. retornar objetos `ValidationResult` en lugar de mensajes `string`).

- **Impacto en el Negocio**: Severidad **Media**. Cualquier cambio en la lógica de validación de un producto pone en riesgo de regresión la validación de clientes al compartir la misma clase estática.
- **Fix Sugerido**: Dividir en componentes validadores independientes por entidad (`ClienteValidator`, `ProductoValidator`) implementando la interfaz `IValidator<T>`.

---

### H-SRP-07: `BibFarmacia/Factories/ProductoFactory.cs` — Creación de Objetos con Políticas de Negocio Hardcodeadas

- **Ubicación**: `BibFarmacia/Factories/ProductoFactory.cs` (Líneas 11 a 44)
- **Clase/Módulo**: `ProductoFactory`
- **Síntoma Observado**: Mezcla el mecanismo de construcción de instancias con políticas de negocio quemadas en código (magic numbers y valores por defecto).

#### Fragmento de Código Relevante:
```csharp
public static MedicamentoCapsula CrearCapsula(string nombre, decimal precio, int stock, Laboratorio laboratorio)
{
    return new MedicamentoCapsula(nombre, precio, stock, 5, DateTime.Now.AddMonths(6), laboratorio, TipoRelleno.Gel);
}

public static MedicamentoLiquido CrearLiquido(string nombre, decimal precio, int stock, Laboratorio laboratorio)
{
    return new MedicamentoLiquido(nombre, precio, stock, 5, DateTime.Now.AddMonths(12), laboratorio, MaterialEnvase.Vidrio, 120);
}
```

#### Razones para Cambiar Identificadas (2 Ejes de Cambio):
1. **Construcción e Instanciación**: Cambios en la firma o parámetros requeridos para crear medicamentos.
2. **Políticas de Negocio por Defecto**: Cambios en las reglas corporativas de inventario (umbrales de stock mínimo por defecto `5`, vigencias por defecto `6` / `12` meses, tipos de envase o relleno por defecto).

- **Impacto en el Negocio**: Severidad **Media**. Modificar la política comercial de vencimientos o stock mínimo requiere editar la fábrica de objetos y redeplegar las librerías del núcleo.
- **Fix Sugerido**: Recibir la configuración o parámetros de políticas de negocio (`ProductPolicyOptions`) como argumentos de la fábrica o inyectar un proveedor de políticas.

---

### H-SRP-08: `BibFarmacia/Eventos/` (`EventoStockMinimo.cs`, `EventoVencimiento.cs`, `EventoPuntos.cs`, `EventoMovimiento.cs`) — Mezcla de Despacho de Eventos y Formato de Mensajes

- **Ubicación**: `BibFarmacia/Eventos/` (Archivos: `EventoStockMinimo.cs` L17-22, `EventoVencimiento.cs` L19-24, `EventoPuntos.cs` L17-23, `EventoMovimiento.cs` L17-22)
- **Clase/Módulo**: Clases publicadoras de eventos en la capa de infraestructura/eventos.
- **Síntoma Observado**: Las clases de eventos mezclan la responsabilidad de notificar a los suscriptores con la construcción y formateo de cadenas de texto quemadas en español.

#### Fragmento de Código Relevante (`EventoStockMinimo.cs` L17-22):
```csharp
public void Disparar(Producto producto)
{
    StockMinimo?.Invoke(
        $"ALERTA: stock mínimo de {producto.Nombre} - Stock actual: {producto.Stock}");
}
```

#### Razones para Cambiar Identificadas (2 Ejes de Cambio):
1. **Mecanismo de Despacho de Eventos**: Cambios en la infraestructura de eventos o invocación del delegado.
2. **Formato y Localización de Mensajes**: Cambios en el texto de las alertas, idioma/traducción o estructura del mensaje.

- **Impacto en el Negocio**: Severidad **Baja**. Imposibilidad de internacionalizar (i18n) o cambiar el formato de los mensajes de alerta sin modificar las clases de eventos.
- **Fix Sugerido**: Publicar argumentos de eventos fuertemente tipados (`StockMinimoEventArgs`, `VencimientoEventArgs`) que contengan la entidad de dominio y permitir que los receptores/UI den el formato deseado.

---

## 4. Evidencia de Cumplimiento del Principio (SRP)

Durante la auditoría estática, se identificaron varios componentes del modelo de dominio e interfaces que **SÍ CUMPLEN** rigurosamente con el Principio de Responsabilidad Única. A continuación se presenta la evidencia explícita:

1. **`Persona.cs`** (`BibFarmacia/Clases/Persona.cs`, L9–L27)
   - **Evidencia**: Modificador `abstract class Persona`. Almacena exclusivamente el estado y los atributos de identidad básica de una persona (`Nombre`, `Cedula`, `Telefono`, `Correo`). No contiene métodos de I/O, UI ni persistencia.
   - **Razón Única de Cambio**: Modificaciones en la estructura de los datos de identidad básica de una persona.

2. **`Cliente.cs`** (`BibFarmacia/Clases/Cliente.cs`, L9–L25)
   - **Evidencia**: Modificador `class Cliente : Persona`. Representa la entidad cliente ampliando el estado con la propiedad `Puntos` y el método cohesivo `AcumularPuntos(int puntos)`.
   - **Razón Única de Cambio**: Modificaciones en la estructura de datos del cliente o en la regla interna de acumulación directa.

3. **`Usuario.cs`** (`BibFarmacia/Clases/Usuario.cs`, L8–L22)
   - **Evidencia**: Modificador `class Usuario : Persona`. Representa la entidad usuario ampliando el estado con credenciales (`UserName`, `Password`). No realiza validaciones de autenticación ni lectura de archivos.
   - **Razón Única de Cambio**: Modificaciones en las propiedades asociadas a las credenciales del usuario.

4. **`Laboratorio.cs`** (`BibFarmacia/Clases/Laboratorio.cs`, L9–L24)
   - **Evidencia**: Clase de dominio pura. Encapsula los atributos de un laboratorio farmacéutico (`Nombre`, `Direccion`, `Telefono`) y su constructor.
   - **Razón Única de Cambio**: Modificaciones en los atributos informativos del laboratorio.

5. **`Medicamento.cs`**, **`MedicamentoCapsula.cs`**, **`MedicamentoLiquido.cs`** (`BibFarmacia/Clases/`)
   - **Evidencia**: Clases especializadas de dominio que únicamente extienden el estado del producto con atributos específicos (`Laboratorio`, `TipoRelleno`, `MaterialEnvase`, `Mililitros`).
   - **Razón Única de Cambio**: Modificaciones en las propiedades específicas del tipo de medicamento.

6. **`Movimiento.cs`** (`BibFarmacia/Clases/Movimiento.cs`, L9–L26)
   - **Evidencia**: Entidad de registro transaccional que contiene exclusivamente `Fecha`, `Cantidad`, `Tipo` y `Producto`.
   - **Razón Única de Cambio**: Ajustes en los atributos del registro de movimiento de inventario.

7. **`MaterialEnvase.cs`** y **`TipoRelleno.cs`** (`BibFarmacia/Enums/`)
   - **Evidencia**: Enumeraciones de dominio simples que definen únicamente los valores posibles para materiales y rellenos.
   - **Razón Única de Cambio**: Adición o eliminación de un valor en el catálogo de enumeración.

8. **`IDescuento.cs`** y **`IServicioNotificacion.cs`** (`BibFarmacia/Interfaces/`)
   - **Evidencia**: Interfaces magras y cohesivas de un solo método (`CalcularDescuento` y `EnviarNotificacion` respectivamente).
   - **Razón Única de Cambio**: Modificación de la firma del contrato específico.

---

## 5. Tabla de Resumen Consolidada de Evaluación SRP

A continuación se presenta la tabla sintética obligatoria que consolida la evaluación del Principio de Responsabilidad Única para la totalidad de clases y módulos de la solución:

| Principio | ¿Cumple? | Evidencia (archivo / línea) | Fix sugerido |
|---|---|---|---|
| **SRP** | **NO** | `AppFarmaciaConsola/Program.cs` (L1–L378) | Extraer controladores UI, parsers de consola, mediadores de casos de uso y DI. |
| **SRP** | **NO** | `BibFarmacia/Servicios/ServicioProducto.cs` (L12–L119) | Separar persistencia a `IProductoRepository` y monitoreo a `StockAlertEvaluator`. |
| **SRP** | **NO** | `BibFarmacia/Servicios/ServicioCliente.cs` (L12–L82) | Separar lectura CSV a `IClienteRepository` y desacoplar la instanciación de eventos. |
| **SRP** | **NO** | `BibFarmacia/Servicios/ServicioUsuario.cs` (L12–L74) | Mover lectura de disco a `IUsuarioRepository` e inyectar `IAuthenticationService`. |
| **SRP** | **NO** | `BibFarmacia/Clases/Producto.cs` (L29–L34) | Eliminar `MostrarInformacion()` y delegar presentación a la capa de UI. |
| **SRP** | **NO** | `BibFarmacia/Aspectos/AspectoValidacion.cs` (L11–L45) | Dividir en validadores independientes `ClienteValidator` y `ProductoValidator`. |
| **SRP** | **NO** | `BibFarmacia/Factories/ProductoFactory.cs` (L11–L44) | Parametrizar políticas por defecto y eliminar valores numéricos/fechas quemados. |
| **SRP** | **NO** | `BibFarmacia/Aspectos/AspectoAutenticacion.cs` (L13–L21) | Desacoplar búsqueda en listas y crear un `IAuthenticationService` inyectable. |
| **SRP** | **NO** | `BibFarmacia/Servicios/ServicioMovimiento.cs` (L15, L21–L22) | Inyectar despachador de eventos por constructor en lugar de instanciar con `new`. |
| **SRP** | **NO** | `BibFarmacia/Eventos/EventoStockMinimo.cs` (L17–L22) | Emitir `StockMinimoEventArgs` fuertemente tipados sin cadenas de texto quemadas. |
| **SRP** | **NO** | `BibFarmacia/Eventos/EventoVencimiento.cs` (L19–L24) | Emitir `VencimientoEventArgs` fuertemente tipados sin cadenas de texto quemadas. |
| **SRP** | **NO** | `BibFarmacia/Eventos/EventoPuntos.cs` (L17–L23) | Separar la infraestructura de disparo del formateo de mensajes. |
| **SRP** | **NO** | `BibFarmacia/Eventos/EventoMovimiento.cs` (L17–L22) | Separar el evento del formateo de texto incrustado en el publicador. |
| **SRP** | **SÍ** | `BibFarmacia/Clases/Persona.cs` (L9–L27) | N/A (Entidad pura de datos abstracta de identidad). |
| **SRP** | **SÍ** | `BibFarmacia/Clases/Cliente.cs` (L9–L25) | N/A (Entidad de dominio con responsabilidad cohesiva). |
| **SRP** | **SÍ** | `BibFarmacia/Clases/Usuario.cs` (L8–L22) | N/A (Entidad de dominio con responsabilidad cohesiva). |
| **SRP** | **SÍ** | `BibFarmacia/Clases/Laboratorio.cs` (L9–L24) | N/A (Entidad pura de datos de laboratorio). |
| **SRP** | **SÍ** | `BibFarmacia/Clases/Medicamento.cs` (L9–L24) | N/A (Entidad de dominio derivada cohesiva). |
| **SRP** | **SÍ** | `BibFarmacia/Clases/MedicamentoCapsula.cs` (L11–L29) | N/A (Entidad especializadora cohesiva). |
| **SRP** | **SÍ** | `BibFarmacia/Clases/MedicamentoLiquido.cs` (L11–L32) | N/A (Entidad especializadora cohesiva). |
| **SRP** | **SÍ** | `BibFarmacia/Clases/Movimiento.cs` (L9–L26) | N/A (Entidad de registro transaccional cohesiva). |
| **SRP** | **SÍ** | `BibFarmacia/Enums/MaterialEnvase.cs` (L9–L13) | N/A (Enum exclusivo de dominio). |
| **SRP** | **SÍ** | `BibFarmacia/Enums/TipoRelleno.cs` (L9–L13) | N/A (Enum exclusivo de dominio). |
| **SRP** | **SÍ** | `BibFarmacia/Interfaces/IDescuento.cs` (L9–L12) | N/A (Interfaz segregada y enfocada). |
| **SRP** | **SÍ** | `BibFarmacia/Interfaces/IServicioNotificacion.cs` (L9–L12) | N/A (Interfaz segregada y enfocada). |
| **SRP** | **SÍ** | `BibFarmacia/Servicios/ServicioDescuento.cs` (L11–L17) | N/A (Servicio enfocado exclusivamente en cálculo de descuento). |
| **SRP** | **SÍ** | `BibFarmacia/Servicios/ServicioNotificacion.cs` (L10–L16) | N/A (Servicio enfocado exclusivamente en notificaciones). |

---

## 6. Conclusiones y Roadmap de Refactorización para SRP

1. **Descomposición del Punto de Entrada**: `Program.cs` debe dejar de actuar como un script monolítico y reducirse a configurar el contenedor de dependencias y ejecutar la aplicación mediante un orquestador o controlador UI.
2. **Separación de Persistencia y Negocio**: Extraer toda la lógica de I/O de archivos (`File.ReadAllLines`, `linea.Split(';')`) hacia repositorios dedicados (`IProductoRepository`, `IClienteRepository`, `IUsuarioRepository`).
3. **Aislamiento de la UI**: Eliminar invocaciones a `Console.WriteLine` dentro de entidades de dominio (`Producto.cs`) y servicios (`ServicioNotificacion.cs`).
4. **Descomposición de Validadores y Eventos**: Crear validadores por entidad (`IValidator<T>`) y refactorizar eventos para que transmitan `EventArgs` fuertemente tipados sin formateo de cadenas quemadas.
