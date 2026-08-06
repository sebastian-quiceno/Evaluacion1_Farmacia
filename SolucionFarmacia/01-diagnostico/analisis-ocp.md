# Diagnóstico Arquitectónico SOLID: Principio Abierto/Cerrado (OCP)

**Proyecto**: Solución Farmacia (BibFarmacia + AppFarmaciaConsola)  
**Evaluador**: Agente Especialista en OCP (`worker_ocp`)  
**Fase**: Diagnóstico AS-IS (Fase 1 — Diagnóstico Arquitectónico SOLID)  
**Fecha**: 2026-08-05  
**Ubicación del Documento**: `01-diagnostico/analisis-ocp.md`  

---

## 1. Resumen Ejecutivo

El Principio Abierto/Cerrado (**Open/Closed Principle — OCP**), formulado por Bertrand Meyer y adaptado por Robert C. Martin para el paradigma orientado a objetos, establece que:

> *«Las entidades de software (clases, módulos, funciones) deben estar **abiertas a la extensión**, pero **cerradas a la modificación**.»*

En un sistema conforme a OCP, la adición de nuevas características, comportamientos o tipos de datos debe lograrse agregando nuevo código (nuevas clases, implementaciones o estrategias) **sin modificar el código fuente existente ya probado y desplegado**.

Tras una evaluación estática minuciosa sobre los 26 archivos C# de la biblioteca `BibFarmacia` y el script de consola `AppFarmaciaConsola/Program.cs` (378 líneas), se concluye que **el sistema AS-IS viola severamente el principio OCP en sus componentes clave de negocio, fábrica, validación, servicios e interfaz de usuario**. 

Las violaciones se manifiestan a través de:
1. **Lógica de negocio hardcodeada** (descuentos fijos del 10%, valores por defecto mágicos).
2. **Instanciación concreta de subtipos en métodos monolíticos** (`CargarDesdeArchivo` instanciando únicamente `MedicamentoCapsula`).
3. **Fábricas estáticas rígidas** (`ProductoFactory` con métodos estáticos explícitos por subtipo).
4. **Validaciones estáticas centrales** (`AspectoValidacion` mezclando reglas de múltiples entidades).
5. **Menú de usuario monolítico** (`Program.cs` estructurado en un bloque `switch` cerrado de 7 opciones).
6. **Incapacidad de soportar las 3 Solicitudes de Cambio Futuras (SC-1, SC-2, SC-3)** sin realizar modificaciones invasivas y de alto riesgo sobre el código fuente original.

---

## 2. Metodología de Evaluación y Alcance

La investigación se llevó a cabo analizando la totalidad de la solución C# .NET 8:
- **`BibFarmacia`**: 26 archivos distribuidos en `Clases/`, `Servicios/`, `Factories/`, `Aspectos/`, `Interfaces/`, `Eventos/` y `Enums/`.
- **`AppFarmaciaConsola`**: `Program.cs` y los archivos de entrada `productos.txt`, `clientes.txt`, `usuarios.txt`.

Cada módulo fue examinado respondiendo a la pregunta fundamental de OCP:
> *«¿Qué sucede si el negocio exige agregar un nuevo caso, entidad, canal o regla? ¿Se extiende el sistema añadiendo nuevo código o se debe abrir un archivo existente para editar sus líneas de código incorporando estructuras `if/else` o `switch`?»*

Asimismo, cada hallazgo fue contrastado contra las tres **Solicitudes de Cambio Futuras (SC)** definidas para el proyecto:
- **SC-1 (Nuevas Categorías de Productos)**: Venta de cosméticos y comestibles (gaseosas, agua, helados, snacks).
- **SC-2 (Venta de Servicios de Salud)**: Inyectología, cambio de vendajes y curaciones básicas.
- **SC-3 (Convenios Institucionales)**: Descuentos y créditos con empresas, bancos, cooperativas, mutuales y universidades.

---

## 3. Diagnóstico Detallado de Hallazgos OCP

A continuación se presentan los 7 hallazgos detallados de violación al principio OCP detectados en la solución.

---

### Hallazgo H-OCP-01: Descuento Plano Hardcodeado en `ServicioDescuento`

- **Ubicación**: `BibFarmacia/Servicios/ServicioDescuento.cs`
- **Clase / Método**: `ServicioDescuento.CalcularDescuento(decimal precio)`
- **Líneas de Código**: 11–17

#### Código Fuente AS-IS
```csharp
11: public class ServicioDescuento : IDescuento
12: {
13:     public decimal CalcularDescuento(decimal precio)
14:     {
15:         return precio * 0.10m;
16:     }
17: }
```

#### Análisis de Violación OCP
La clase `ServicioDescuento` implementa la interfaz `IDescuento`, pero retoma un porcentaje fijo y quemado en código (`0.10m` = 10%). 

La clase está **completamente cerrada a la extensión**. Si la farmacia requiere aplicar reglas de descuento dinámicas (por categoría de producto, por día de la semana, por monto acumulado o por convenio institucionales), **es obligatorio abrir y modificar el archivo `ServicioDescuento.cs`**, insertando lógica condicional (`if/switch`).

#### Impacto en Solicitudes de Cambio (SC-1, SC-2, SC-3)
- **Impacto SC-3 (Convenios)**: **Falla Crítica**. Para implementar convenios con universidades (15%), empresas (20%) o bancos (12%), `ServicioDescuento.cs` debe ser editado quirúrgicamente para recibir al cliente o convenio y evaluar condicionales.
- **Impacto SC-1 y SC-2**: No permite excluir del descuento los servicios de salud (SC-2) ni aplicar promociones especiales a cosméticos (SC-1) sin modificar la línea 15.

#### Fix Sugerido Mínimo
Refactorizar aplicando el **Patrón Strategy** (`IDescuentoStrategy`), cerrando `ServicioDescuento` a modificaciones y permitiendo inyectar o registrar estrategias de descuento sin tocar el código fuente del servicio.

---

### Hallazgo H-OCP-02: Carga de Archivo Rígida e Instanciación Concreta de `MedicamentoCapsula`

- **Ubicación**: `BibFarmacia/Servicios/ServicioProducto.cs`
- **Clase / Método**: `ServicioProducto.CargarDesdeArchivo(string ruta)`
- **Líneas de Código**: 75–118 (en particular L93–107)

#### Código Fuente AS-IS
```csharp
75: public string CargarDesdeArchivo(string ruta)
76: {
...
88:     foreach (string linea in lineas)
89:     {
90:         string[] datos = linea.Split(';');
91: 
93:         Laboratorio laboratorio = new Laboratorio(
94:             datos[5],
95:             "Medellin",
96:             "4444444");
97: 
99:         MedicamentoCapsula medicamento = new MedicamentoCapsula(
100:             datos[0],
101:             decimal.Parse(datos[1]),
102:             int.Parse(datos[2]),
103:             int.Parse(datos[3]),
104:             DateTime.Parse(datos[4]),
105:             laboratorio,
106:             Enum.TipoRelleno.Gel);
107: 
108:         productos.Add(medicamento);
109:     }
```

#### Análisis de Violación OCP
El método `CargarDesdeArchivo` presupone que **todos los registros del archivo de texto corresponden indefectiblemente a `MedicamentoCapsula`**, instanciando rígida y directamente `new Laboratorio(...)` con datos inventados ("Medellin", "4444444") y `new MedicamentoCapsula(...)` con `TipoRelleno.Gel`.

El método está **cerrado a la extensión**. No hay manera de cargar `MedicamentoLiquido` ni cualquier nuevo subtipo de producto sin modificar `ServicioProducto.cs` añadiendo ramificaciones `if/else` o `switch` según un identificador de tipo.

#### Impacto en Solicitudes de Cambio (SC-1, SC-2, SC-3)
- **Impacto SC-1 (Cosméticos, comestibles)**: **Falla Crítica**. Imposible cargar cosméticos o snacks sin editar `ServicioProducto.cs` L99. Intentar parsear una línea de cosmético con esta lógica causará errores o creará medicamentos ficticios.
- **Impacto SC-2 (Servicios de Salud)**: **Falla Crítica**. Los servicios (inyectología) no tienen laboratorio ni tipo de relleno. La carga fallará o exigirá modificar el método introduciendo validaciones por tipo.

#### Fix Sugerido Mínimo
Separar la persistencia e I/O a un repositorio/lector de datos (`IProductoDataReader`) que utilice un registro dinámico de parsers por tipo de producto (Factory / Registry de Deserialización).

---

### Hallazgo H-OCP-03: Fábrica Estática Rígida en `ProductoFactory`

- **Ubicación**: `BibFarmacia/Factories/ProductoFactory.cs`
- **Clase / Métodos**: `ProductoFactory.CrearCapsula`, `ProductoFactory.CrearLiquido`
- **Líneas de Código**: 11–44

#### Código Fuente AS-IS
```csharp
11: public static class ProductoFactory
12: {
13:     public static MedicamentoCapsula CrearCapsula(
14:         string nombre, decimal precio, int stock, Laboratorio laboratorio)
15:     {
16:         return new MedicamentoCapsula(
17:             nombre, precio, stock, 5,
18:             DateTime.Now.AddMonths(6), laboratorio, TipoRelleno.Gel);
19:     }
28:     public static MedicamentoLiquido CrearLiquido(
29:         string nombre, decimal precio, int stock, Laboratorio laboratorio)
30:     {
31:         return new MedicamentoLiquido(
32:             nombre, precio, stock, 5,
33:             DateTime.Now.AddMonths(12), laboratorio, MaterialEnvase.Vidrio, 120);
34:     }
35: }
```

#### Análisis de Violación OCP
`ProductoFactory` es una clase estática que expone métodos explícitos para cada subtipo concreto de medicamento. Además, quema en código valores por defecto como `stockMinimo = 5`, fechas de vencimiento a 6 y 12 meses, `TipoRelleno.Gel` y `MaterialEnvase.Vidrio`.

Para incorporar una nueva variante o categoría de producto en el sistema, **se debe editar `ProductoFactory.cs` agregando un nuevo método estático** (`CrearCosmetico`, `CrearServicioInyectologia`, `CrearSnack`).

#### Impacto en Solicitudes de Cambio (SC-1, SC-2, SC-3)
- **Impacto SC-1**: Exige modificar `ProductoFactory.cs` para crear métodos de creación de cosméticos y alimentos.
- **Impacto SC-2**: Exige modificar `ProductoFactory.cs` para crear métodos de instanciación de servicios de salud.

#### Fix Sugerido Mínimo
Implementar una fábrica abstracta o contenedor de creación registrado (`IProductoFactory` o `IDictionary<TipoProducto, Func<Parametros, Producto>>`), permitiendo registrar nuevos creadores de productos sin modificar el código de la fábrica existente.

---

### Hallazgo H-OCP-04: Métodos Estáticos de Validación Monolíticos en `AspectoValidacion`

- **Ubicación**: `BibFarmacia/Aspectos/AspectoValidacion.cs`
- **Clase / Métodos**: `AspectoValidacion.ValidarCliente`, `AspectoValidacion.ValidarProducto`
- **Líneas de Código**: 11–45

#### Código Fuente AS-IS
```csharp
11: public static class AspectoValidacion
12: {
13:     public static string ValidarCliente(Cliente cliente)
14:     {
15:         if (string.IsNullOrWhiteSpace(cliente.Nombre)) return "Nombre inválido";
16:         if (cliente.Cedula.Length < 3) return "Cédula inválida";
17:         return "Cliente válido";
18:     }
30:     public static string ValidarProducto(Producto producto)
31:     {
32:         if (producto.Precio <= 0) return "Precio inválido";
33:         if (producto.Stock < 0) return "Stock inválido";
34:         return "Producto válido";
35:     }
36: }
```

#### Análisis de Violación OCP
`AspectoValidacion` agrupa validaciones estáticas para distintas entidades del sistema (`Cliente` y `Producto`). 

Si se requiere agregar reglas de validación específicas para un subtipo de producto (por ejemplo, validar que los medicamentos tengan laboratorio asignado, que los alimentos tengan fecha de caducidad futura, o que los cosméticos tengan registro sanitario), **se debe modificar el método `ValidarProducto` agregando bloques `if (producto is Medicamento)`**. Asimismo, no hay forma de agregar validadores para nuevas entidades (`Usuario`, `Convenio`, `Servicio`) sin editar `AspectoValidacion.cs`.

#### Impacto en Solicitudes de Cambio (SC-1, SC-2, SC-3)
- **Impacto SC-1**: Validar registros sanitarios o temperaturas de almacenamiento de comestibles exige editar `AspectoValidacion.cs`.
- **Impacto SC-2**: Validar que los servicios de salud especifiquen el profesional a cargo exige modificar `AspectoValidacion.cs`.
- **Impacto SC-3**: Validar cupos de crédito o vigencias de convenio para clientes exige modificar `ValidarCliente` en `AspectoValidacion.cs`.

#### Fix Sugerido Mínimo
Extraer la validación a una interfaz genérica `IValidator<T>` con implementaciones polimórficas independientes (`ClienteValidator`, `MedicamentoValidator`, `CosmeticoValidator`, `ConvenioValidator`).

---

### Hallazgo H-OCP-05: Menú Monolítico Estructurado en Bloque `switch` en `Program.cs`

- **Ubicación**: `AppFarmaciaConsola/Program.cs`
- **Módulo**: Punto de entrada de Consola
- **Líneas de Código**: 145–374

#### Código Fuente AS-IS
```csharp
145: while (opcion != 7)
146: {
...
156:     Console.WriteLine("1. Ver productos");
157:     Console.WriteLine("2. Ver clientes");
158:     Console.WriteLine("3. Buscar producto");
159:     Console.WriteLine("4. Registrar venta");
160:     Console.WriteLine("5. Acumular puntos");
161:     Console.WriteLine("6. Ver alertas");
162:     Console.WriteLine("7. Salir");
...
169:     switch (opcion)
170:     {
171:         case 1: ... break;
198:         case 2: ... break;
218:         case 3: ... break;
255:         case 4: ... break;
305:         case 5: ... break;
343:         case 6: ... break;
356:         case 7: ... break;
374:     }
375: }
```

#### Análisis de Violación OCP
El menú principal de la aplicación de consola está implementado mediante una estructura monolítica `while (opcion != 7)` combinada con un `switch (opcion)` de 7 opciones fijas.

Cualquier ampliación del sistema que requiera una nueva acción de usuario en consola (por ejemplo: "Registrar servicio de salud", "Consultar convenios corporativos", "Filtrar por cosméticos") **obliga a abrir y modificar `Program.cs`**, cambiar el límite del bucle (`opcion != 8`), agregar un nuevo texto al menú y añadir un nuevo bloque `case 8:` en el `switch`.

#### Impacto en Solicitudes de Cambio (SC-1, SC-2, SC-3)
- **Impacto SC-1**: Para filtrar o ver productos por categoría (cosméticos/comestibles) se debe modificar el `case 1` o agregar opciones al `switch` en `Program.cs`.
- **Impacto SC-2**: Para registrar la venta de un servicio de inyectología/curación se debe modificar el `case 4` o añadir un `case` de servicios en `Program.cs`.
- **Impacto SC-3**: Para aplicar convenios corporativos durante la venta se exige modificar el código dentro del `case 4` en `Program.cs`.

#### Fix Sugerido Mínimo
Implementar el **Patrón Command** (`IConsoleCommand` / `IMenuOption`) manteniendo un registro o diccionario de comandos (`Dictionary<int, IMenuOption>`). Para agregar una nueva opción al menú, se crea una nueva clase que implementa `IConsoleCommand` y se registra en el menú sin modificar el bucle principal de `Program.cs`.

---

### Hallazgo H-OCP-06: Asunción de Atributos Físicos en `Producto` y Alertas Rígidas en `ServicioProducto`

- **Ubicación**: `BibFarmacia/Clases/Producto.cs` (L8–35) y `BibFarmacia/Servicios/ServicioProducto.cs` (L47–73)
- **Clases**: `Producto`, `ServicioProducto`
- **Líneas de Código**: `Producto.cs` L12–14; `ServicioProducto.cs` L47–73

#### Código Fuente AS-IS
```csharp
// BibFarmacia/Clases/Producto.cs
12: public int Stock { get; set; }
13: public int StockMinimo { get; set; }
14: public DateTime FechaVencimiento { get; set; }

// BibFarmacia/Servicios/ServicioProducto.cs
47: public void VerificarStock()
48: {
49:     foreach (var producto in productos)
50:     {
51:         if (producto.Stock <= producto.StockMinimo)
52:             EventoStock.Disparar(producto);
53:     }
54: }
59: public void VerificarVencimiento()
60: {
61:     foreach (var producto in productos)
62:     {
63:         int dias = (producto.FechaVencimiento - DateTime.Now).Days;
64:         if (dias <= 30) EventoVencimiento.Disparar(producto);
65:     }
66: }
```

#### Análisis de Violación OCP
La clase abstracta `Producto` obliga a que **todos sus subtipos contengan `Stock`, `StockMinimo` y `FechaVencimiento`**. A su vez, `ServicioProducto` itera sobre la lista de productos asumiendo ciegamente que todos están sujetos a control de inventario físico y alertas de caducidad a 30 días.

Si se introduce un elemento que no caduca (comestible no perecedero) o un elemento intangible (servicio de salud), los métodos `VerificarStock` y `VerificarVencimiento` dispararán alertas erróneas o requerirán ser modificados introduciendo filtros por tipo (`if (producto is Medicamento)`), violando OCP.

#### Impacto en Solicitudes de Cambio (SC-1, SC-2, SC-3)
- **Impacto SC-1**: Para manejar comestibles con reglas de vencimiento distintas (ej. bebidas con 15 días o snacks no perecederos) se debe modificar `VerificarVencimiento`.
- **Impacto SC-2**: **Falla Crítica**. Un servicio de inyectología o curación no tiene stock ni vencimiento. Si se hereda de `Producto`, `VerificarStock` y `VerificarVencimiento` generarán falsos positivos constantes a menos que se abra `ServicioProducto.cs` para editar ambos métodos.

#### Fix Sugerido Mínimo
Segregar los contratos de producto mediante interfaces composables (`IStockable`, `IPerishable`, `IVendible`) y abstraer la verificación de alertas mediante evaluadores de reglas de alertas (`IAlertRule<T>`).

---

### Hallazgo H-OCP-07: Acoplamiento Rígido de Notificaciones a Consola en `ServicioNotificacion`

- **Ubicación**: `BibFarmacia/Servicios/ServicioNotificacion.cs`
- **Clase / Método**: `ServicioNotificacion.EnviarNotificacion(string mensaje)`
- **Líneas de Código**: 10–16

#### Código Fuente AS-IS
```csharp
10: public class ServicioNotificacion : IServicioNotificacion
11: {
12:     public void EnviarNotificacion(string mensaje)
13:     {
14:         Console.WriteLine($"[NOTIFICACION] {mensaje}");
15:     }
16: }
```

#### Análisis de Violación OCP
`ServicioNotificacion` implementa la interfaz `IServicioNotificacion`, pero hardcodea la salida hacia `Console.WriteLine`. 

Está **cerrado a la extensión de canales de notificación**. Si el negocio requiere enviar notificaciones por Correo Electrónico, SMS, o guardar logs en archivo ante eventos críticos, no hay manera de extender esta clase sin editarla o sustituirla sin un mecanismo de composición extensible.

#### Fix Sugerido Mínimo
Implementar una notificación compuesta (`CompositeNotificacionService`) o registrar múltiples suscriptores de `IServicioNotificacion` en un bus o lista.

---

## 4. Matriz de Impacto ante Solicitudes de Cambio Futuras (SC-1, SC-2, SC-3)

La siguiente matriz sintetiza el impacto directo del diseño AS-IS cuando se intenta implementar cada una de las tres solicitudes de cambio reales requeridas por el negocio:

| Solicitud de Cambio | Descripción del Requerimiento | Archivos que deben MODIFICARSE en el código AS-IS | Comportamiento que corre riesgo de ROMPERSE | Severidad OCP |
|---|---|---|---|---|
| **SC-1** | **Venta de cosméticos y comestibles** (bebidas, snacks, helados) | `BibFarmacia/Clases/Producto.cs`<br>`BibFarmacia/Factories/ProductoFactory.cs`<br>`BibFarmacia/Servicios/ServicioProducto.cs`<br>`BibFarmacia/Aspectos/AspectoValidacion.cs`<br>`AppFarmaciaConsola/Program.cs` | • Carga de CSV fallida por falta de campos de laboratorio.<br>• Alertas falsas de vencimiento para productos no perecederos.<br>• Formateo de UI roto en la consola. | **ALTA** |
| **SC-2** | **Venta de servicios de salud** (inyectología, vendajes, curaciones) | `BibFarmacia/Clases/Producto.cs`<br>`BibFarmacia/Factories/ProductoFactory.cs`<br>`BibFarmacia/Servicios/ServicioProducto.cs`<br>`AppFarmaciaConsola/Program.cs` | • Error semántico y de ejecución al restar stock en ventas (`Stock -= cantidad`).<br>• Alertas de stock crítico (stock=0) automáticas e inapropiadas.<br>• Violación del contrato de `Producto`. | **CRÍTICA** |
| **SC-3** | **Convenios institucionales** (empresas, bancos, universidades) | `BibFarmacia/Servicios/ServicioDescuento.cs`<br>`BibFarmacia/Clases/Cliente.cs`<br>`BibFarmacia/Aspectos/AspectoValidacion.cs`<br>`AppFarmaciaConsola/Program.cs` | • Descuento plano del 10% aplicado incorrectamente a convenios.<br>• Complejidad ciclomática disparada por `if/else` en el servicio de descuento.<br>• Imposibilidad de otorgar crédito. | **ALTA** |

---

## 5. Evidencia de Cumplimiento OCP en el Proyecto AS-IS

Para garantizar un diagnóstico objetivo y riguroso, se documentan los componentes que **SÍ cumplen** con el Principio Abierto/Cerrado en el código actual:

### 1. Jerarquía de Personas (`Persona.cs` -> `Cliente.cs`, `Usuario.cs`)
- **Ubicación**: `BibFarmacia/Clases/Persona.cs` (L9–24)
- **Evidencia**: `Persona` se define como una clase abstracta pura con propiedades generales (`Nombre`, `Cedula`, `Telefono`, `Correo`) y constructor `protected`.
- **Análisis OCP**: **Cumple**. Si el sistema requiere agregar nuevos tipos de personas (ej. `Empleado`, `Proveedor`, `Medico`), es posible crear nuevas subclases que hereden de `Persona` sin modificar una sola línea del archivo `Persona.cs`.

### 2. Contratos de Interfaz Base (`IDescuento.cs` e `IServicioNotificacion.cs`)
- **Ubicación**: `BibFarmacia/Interfaces/IDescuento.cs` y `BibFarmacia/Interfaces/IServicioNotificacion.cs`
- **Evidencia**: Ambas interfaces definen contratos cohesivos y limpios de un solo método (`CalcularDescuento` y `EnviarNotificacion`).
- **Análisis OCP**: **Cumple en definición**. Las interfaces como tal están correctamente cerradas a modificación y abiertas a nuevas implementaciones. El fallo en la solución radica en que los servicios y la consola no aprovechan estas interfaces mediante patrones polimórficos (Strategy, DI) para permitir la extensibilidad.

---

## 6. Tabla Resumen Obligatoria

A continuación se consolida la evaluación del Principio Abierto/Cerrado (OCP) para todos los módulos analizados:

| Principio | ¿Cumple? | Evidencia (archivo / línea) | Fix Sugerido |
|---|---|---|---|
| **OCP** | ❌ NO | `BibFarmacia/Servicios/ServicioDescuento.cs` (L13–16) | Implementar **Patrón Strategy** (`IDescuentoStrategy`) para soportar convenios (SC-3) sin editar la clase. |
| **OCP** | ❌ NO | `BibFarmacia/Servicios/ServicioProducto.cs` (L75–118) | Utilizar fábrica/parser de deserialización polimórfica (`IProductoDataReader`) para cargar nuevos productos (SC-1) y servicios (SC-2). |
| **OCP** | ❌ NO | `BibFarmacia/Factories/ProductoFactory.cs` (L13–44) | Sustituir métodos estáticos por un registro dinámico de creadores (`IProductoFactory` / Abstract Factory). |
| **OCP** | ❌ NO | `BibFarmacia/Aspectos/AspectoValidacion.cs` (L13–44) | Implementar validadores genéricos polimórficos (`IValidator<T>`) por tipo de entidad. |
| **OCP** | ❌ NO | `AppFarmaciaConsola/Program.cs` (L145–374) | Aplicar **Patrón Command** (`IConsoleCommand`) para registrar opciones de menú sin modificar el `switch`/`while` principal. |
| **OCP** | ❌ NO | `BibFarmacia/Clases/Producto.cs` (L12–14) / `ServicioProducto.cs` (L47–73) | Segregar interfaces (`IStockable`, `IPerishable`) y desacoplar las reglas de alerta de stock/vencimiento. |
| **OCP** | ❌ NO | `BibFarmacia/Servicios/ServicioNotificacion.cs` (L10–16) | Implementar `CompositeNotificacionService` para soportar múltiples canales de notificación. |
| **OCP** | ✅ SÍ | `BibFarmacia/Clases/Persona.cs` (L9–24) | Mantiene abierta la jerarquía de personas para nuevas subclases (`Empleado`, `Proveedor`) sin modificar `Persona.cs`. |
| **OCP** | ✅ SÍ | `BibFarmacia/Interfaces/IDescuento.cs` (L9–12) & `IServicioNotificacion.cs` (L9–12) | Definición de contratos magros abiertos a múltiples implementaciones. |

---

## 7. Plan de Refactorización y Diseños Modulares Sugeridos

Para transformar el sistema AS-IS en un diseño flexible y totalmente conforme a OCP, se proponen los siguientes patrones de diseño mínimos:

### 1. Refactorización de Descuentos (SC-3) con Patrón Strategy

```csharp
// Abstracción Abierta a Extensión
public interface IDescuentoStrategy
{
    bool AplicaPara(Cliente cliente, Producto producto);
    decimal CalcularDescuento(decimal precio);
}

// Estrategia por Convenio Institucional (SC-3)
public class ConvenioDescuentoStrategy : IDescuentoStrategy
{
    private readonly string convenio;
    private readonly decimal porcentaje;

    public ConvenioDescuentoStrategy(string convenio, decimal porcentaje)
    {
        this.convenio = convenio;
        this.porcentaje = porcentaje;
    }

    public bool AplicaPara(Cliente cliente, Producto producto) => cliente.Convenio == convenio;
    public decimal CalcularDescuento(decimal precio) => precio * porcentaje;
}

// Servicio de Descuento Cerrado a Modificación
public class ServicioDescuento
{
    private readonly List<IDescuentoStrategy> estrategias;

    public ServicioDescuento(IEnumerable<IDescuentoStrategy> estrategias)
    {
        this.estrategias = estrategias.ToList();
    }

    public decimal CalcularDescuento(Cliente cliente, Producto producto, decimal precio)
    {
        var estrategia = estrategias.FirstOrDefault(e => e.AplicaPara(cliente, producto));
        return estrategia != null ? estrategia.CalcularDescuento(precio) : 0m;
    }
}
```

### 2. Refactorización del Menú de Consola con Patrón Command

```csharp
// Abstracción de Comandos de UI
public interface IConsoleCommand
{
    int OptionNumber { get; }
    string Description { get; }
    void Execute();
}

// Orquestador del Menú de Consola (Cerrado a Modificación)
public class MenuController
{
    private readonly Dictionary<int, IConsoleCommand> commands;

    public MenuController(IEnumerable<IConsoleCommand> commandList)
    {
        commands = commandList.ToDictionary(c => c.OptionNumber);
    }

    public void ShowMenuAndExecute()
    {
        foreach (var cmd in commands.Values.OrderBy(c => c.OptionNumber))
        {
            Console.WriteLine($"{cmd.OptionNumber}. {cmd.Description}");
        }

        if (int.TryParse(Console.ReadLine(), out int option) && commands.TryGetValue(option, out var selectedCommand))
        {
            selectedCommand.Execute();
        }
    }
}
```

---

## 8. Conclusión

El diagnóstico demuestra que la arquitectura AS-IS de la `SolucionFarmacia` se encuentra rígida y acoplada. Cualquier intento de evolucionar el software para incorporar cosméticos (SC-1), servicios de salud (SC-2) o convenios institucionales (SC-3) requerirá modificar múltiples archivos fuente clave, disparando los costos de mantenimiento y el riesgo de regresiones. 

La adopción de interfaces composables, el patrón Strategy para la lógica condicional, el patrón Command para la consola y la inyección de dependencias son pasos indispensables para lograr un sistema 100% abierto a la extensión y cerrado a la modificación.
