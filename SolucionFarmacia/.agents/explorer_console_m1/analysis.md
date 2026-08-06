# Análisis Arquitectónico SOLID: AppFarmaciaConsola (`Program.cs`) y Archivos de Datos

**Agente**: `teamwork_preview_explorer_3` (Especialista en Consola y Punto de Entrada)  
**Fecha**: 2026-08-05  
**Ubicación del Análisis**: `c:\Users\tomax\OneDrive\Documentos\Universidad\6to Semestre\Arquitectura de software\SolucionFarmacia\SolucionFarmacia\.agents\explorer_console_m1\analysis.md`  
**Objetivo**: Diagnóstico exhaustivo del estado actual (AS-IS) de `AppFarmaciaConsola/Program.cs` (378 líneas) y sus interacciones con los servicios de `BibFarmacia` y archivos de datos de entrada (`productos.txt`, `clientes.txt`, `usuarios.txt`).

---

## 1. Resumen Ejecutivo y Delimitación del Problema

`AppFarmaciaConsola/Program.cs` constituye el punto de entrada monolítico (Top-Level Statements en C# .NET 8) de la solución. Tras una inspección detallada línea por línea, se ha determinado que `Program.cs` actúa como un **"God Script" (Script Todopoderoso)** que asume responsabilidades de UI, I/O de consola, orquestación de flujo de trabajo, lógica de búsqueda LINQ, instanciación directa de servicios de dominio, mutación directa de entidades de negocio y manejo hardcodeado de archivos.

El archivo adolece de severas violaciones a los 5 principios SOLID:
- **SRP**: Mezcla 7 responsabilidades distintas en 378 líneas.
- **OCP**: La adición de cualquier nueva funcionalidad o tipo de producto/servicio (SC-1, SC-2, SC-3) requiere editar directamente menús, estructuras `switch`, flujos de captura y manipulación de propiedades.
- **LSP**: Asume comportamiento uniforme en la colección de `Producto` (acceso y mutación directa de `Stock`), lo que impedirá la sustitución adecuada cuando se introduzcan servicios o productos no inventariables.
- **ISP**: Depende directamente de clases concretas monolíticas ("fat services") en lugar de interfaces segregadas según el rol requerido por la UI.
- **DIP**: Acoplamiento 100% rígido mediante instanciación directa con `new` (`ServicioProducto`, `ServicioCliente`, `ServicioUsuario`, `ServicioMovimiento`, `Movimiento`), sin inyección de dependencias ni uso de abstracciones, además de rutas relativas quemadas en código (`"productos.txt"`).

---

## 2. Mapeo Estructural y Funcional Línea por Línea de `Program.cs`

| Rango de Líneas | Sección / Componente | Descripción de Responsabilidades y Operaciones |
|---|---|---|
| **L1–L4** | Directivas `using` | Importa `BibFarmacia.Aspectos`, `BibFarmacia.Clases`, `BibFarmacia.Factories`, `BibFarmacia.Servicios`. (Nota: `Aspectos` y `Factories` se importan aunque no se usan explícitamente en el código). |
| **L6** | Configuración de Consola | Establece `Console.Title = "Sistema Farmacia";`. |
| **L8–L18** | Instanciación Concreta de Servicios | Crea instancias concretas mediante `new`: `ServicioProducto`, `ServicioCliente`, `ServicioUsuario`, `ServicioMovimiento`. |
| **L20–L66** | Cableado de Eventos a UI | Suscripción lambda directa a eventos de dominio (`StockMinimo`, `Vencimiento`, `PuntosAcumulados`, `MovimientoRegistrado`) para formatear y formatear salidas de consola con colores (`ConsoleColor.Red`, `Yellow`, `Green`, `Cyan`). |
| **L67–L90** | Carga Inicial de Datos | Presenta mensaje de carga y llama a la carga desde archivos de texto hardcodeados: `servicioProducto.CargarDesdeArchivo("productos.txt")`, `servicioCliente.Cargar("clientes.txt")`, `servicioUsuario.Cargar("usuarios.txt")`. |
| **L91–L134** | Flujo de Autenticación / Login | Renderiza encabezado en azul, solicita credenciales por consola (`Console.ReadLine()`), invoca `servicioUsuario.Login(user, password)`. Si falla, imprime "Acceso denegado" en rojo y finaliza la aplicación (`return;`). |
| **L135–L140** | Verificación Inicial de Alertas | Dispara validaciones automáticas al inicio: `servicioProducto.VerificarStock()` y `servicioProducto.VerificarVencimiento()`. |
| **L141–L168** | Bucle de Menú e Input del Usuario | Bucle `while (opcion != 7)`, renderizado del menú magenta y lectura de opción mediante `int.Parse(Console.ReadLine()!)` **sin captura de excepciones**. |
| **L169–L197** | `switch` - Case 1: Ver productos | Formatea tabla Cyan con encabezado `Nombre\t\tStock\tPrecio`. Itera sobre `servicioProducto.ObtenerProductos()` e imprime atributos en bruto. |
| **L198–L217** | `switch` - Case 2: Ver clientes | Formatea encabezado Green. Itera sobre `servicioCliente.ObtenerClientes()` e imprime `Nombre` y `Puntos`. |
| **L218–L254** | `switch` - Case 3: Buscar producto | Pide nombre de producto por consola. Realiza búsqueda LINQ directamente en la UI (`ObtenerProductos().FirstOrDefault(...)`). Muestra precio y stock o mensaje de error. |
| **L255–L304** | `switch` - Case 4: Registrar venta | Solicita nombre de producto y cantidad. Realiza búsqueda LINQ en la UI. **Mutación directa de estado**: `productoVenta.Stock -= cantidad;`. Instancia directamente entidad de dominio `new Movimiento(...)` y llama a `servicioMovimiento.RegistrarMovimiento(venta)`. |
| **L305–L342** | `switch` - Case 5: Acumular puntos | Solicita cliente y puntos por consola (`int.Parse(...)`). Realiza búsqueda LINQ en la UI e invoca `servicioCliente.AcumularPuntos(clientePuntos, puntos)`. |
| **L343–L355** | `switch` - Case 6: Ver alertas | Invoca `VerificarStock()` y `VerificarVencimiento()` en `servicioProducto`. |
| **L356–L367** | `switch` - Case 7: Salir | Muestra mensaje de salida en red y rompe la condición del bucle. |
| **L368–L374** | `switch` - `default` | Maneja opciones fuera del rango 1-7. |
| **L377–L378** | Finalización | Muestra "FIN DEL SISTEMA" al terminar el ciclo. |

---

## 3. Análisis Detallado de los 5 Principios SOLID en `Program.cs`

### 3.1 Single Responsibility Principle (SRP) — Principio de Responsabilidad Única

> **Definición**: Una clase debe tener una, y solo una, razón para cambiar.

#### Diagnóstico
`Program.cs` viola flagrantemente el principio SRP al acumular 7 razones totalmente distintas para cambiar:

1. **Presentación de Interfaz de Usuario y Formato Estético**: Cambios en colores de consola, títulos, encabezados, formateo de tablas (`\t\t`), bordes ASCII (`===========`).
2. **Entrada/Salida de Consola y Parsing de Datos**: Cambios en cómo se capturan datos (`Console.ReadLine()`), conversiones numéricas de opciones, cantidades y puntos (`int.Parse(...)`).
3. **Orquestación de Flujo de Aplicación y Ciclo de Vida**: Modificaciones en la secuencia de ejecución: Carga → Login → Menú Principal → Salida.
4. **Búsqueda y Filtrado de Datos de Negocio (Lógica de Dominio en la UI)**: Expresiones LINQ escritas en el controlador de interfaz (`ObtenerProductos().FirstOrDefault(p => p.Nombre.ToLower().Contains(...))`) en L226-231, L263-269, L313-319.
5. **Mutación de Estado de Dominio y Reglas de Inventario**: Descuento directo de stock `productoVenta.Stock -= cantidad;` en L280-281, eludiendo la encapsulación y validación de inventario en el servicio.
6. **Instanciación y Acoplamiento de Dependencias de Servicios**: Creación directa de servicios con `new` (L8-18) y construcción de objetos transaccionales `new Movimiento(...)` (L283-288).
7. **Gestión de Rutas y Configuración de Archivos Persistentes**: Nombres de archivo hardcodeados (`"productos.txt"`, `"clientes.txt"`, `"usuarios.txt"`) en L79, L83, L87.

#### Evidencia en Código
- **L167, L277, L327**: `int.Parse(Console.ReadLine()!)` mezcla entrada de consola con conversión insegura de tipos.
- **L229–L231, L266–L269, L316–L319**: Expresiones LINQ `.FirstOrDefault(p => p.Nombre.ToLower().Contains(...))` duplicadas en UI.
- **L280–L281**: `productoVenta.Stock -= cantidad;` modifica la entidad de negocio directamente dentro de la vista de consola.

---

### 3.2 Open/Closed Principle (OCP) — Principio de Abierto/Cerrado

> **Definición**: Las entidades de software deben estar abiertas a la extensión, pero cerradas a la modificación.

#### Diagnóstico
`Program.cs` no está cerrado a la modificación. Cualquier cambio funcional en el sistema requiere editar quirúrgicamente el archivo fuente de `Program.cs`.

#### Evaluación frente a Solicitudes de Cambio Futuras (SC-1, SC-2, SC-3)

1. **SC-1 (Productos Cosméticos y Comestibles)**:
   - Requiere modificar el listado (Case 1, L187-194) si se requiere mostrar categorías, registros sanitarios o vencimientos específicos.
   - Requiere modificar el menú principal para agregar submenús o filtros por tipo de producto.
2. **SC-2 (Venta de Servicios: Inyectología, Vendajes, Curaciones)**:
   - Los servicios no poseen stock ni fechas de vencimiento. La llamada `productoVenta.Stock -= cantidad;` (L280) fallará semánticamente o lanzará excepciones si `Stock` no es aplicable a un Servicio.
   - La adición de la opción "Registrar servicio" obliga a cambiar la estructura del menú, actualizar la condición `while (opcion != 7)` a `while (opcion != 8)`, agregar un `case 8` en el `switch`, e implementar un flujo interactivo nuevo en `Program.cs`.
3. **SC-3 (Convenios y Créditos con Entidades)**:
   - Registrar una venta con convenio requiere capturar el convenio y tipo de pago en el Case 4 (L255-303), agregando más líneas de prompts, validaciones y lógica condicional dentro del `switch`.
4. **Inconsistencia en la Firma de Carga de Archivos**:
   - L78-79: `servicioProducto.CargarDesdeArchivo("productos.txt")`
   - L82: `servicioCliente.Cargar("clientes.txt")`
   - L86: `servicioUsuario.Cargar("usuarios.txt")`
   - No existe un contrato ni interfaz polimórfica para la carga de datos. Si se agrega un nuevo servicio, `Program.cs` debe ser modificado adaptándose a la firma arbitraria del nuevo servicio.

#### Evidencia en Código
- **L145, L169–L374**: Estructura `while` + `switch` monolítica e inextensible.
- **L78–L87**: Inconsistencia en llamadas a métodos de carga sin interfaz común (`CargarDesdeArchivo` vs `Cargar`).

---

### 3.3 Liskov Substitution Principle (LSP) — Principio de Sustitución de Liskov

> **Definición**: Los objetos de un programa deben ser reemplazables por instancias de sus subtipos sin alterar la corrección del programa.

#### Diagnóstico y Análisis
En `Program.cs`, el tratamiento de los objetos retornados por `servicioProducto.ObtenerProductos()` asume implícitamente que todo `Producto` es una entidad física con propiedad `Stock` mutable:

- **Asunción de Stock Mutable en Ventas (L280)**: `productoVenta.Stock -= cantidad;`.
  - Si una jerarquía de clases introduce una subclase de `Producto` como `ServicioSalud` o `ProductoDigital` donde la propiedad `Stock` lance un `NotSupportedException` o permanezca fija, la ejecución de `Program.cs` se rompe.
- **Acoplamiento de Presentación Monomórfica (L190-193, L237-245)**:
  - `Program.cs` imprime solo `Nombre`, `Stock` y `Precio`. Las particularidades de las subclases (por ejemplo, `MedicamentoCapsula.TipoRelleno` o `MedicamentoLiquido.MaterialEnvase`) son ignoradas por completo.
  - Para mostrar datos específicos de subclases, `Program.cs` tendría que recurrir a casteos de tipo (`is` / `as` / pattern matching), lo que destruiría el polimorfismo y violaría LSP.

#### Puntos de Cumplimiento
- La iteración `foreach (var producto in servicioProducto.ObtenerProductos())` compila y ejecuta limpiamente con el modelo actual debido a que todos los elementos retornados son derivaciones de la clase base abstracta `Producto` que declaran `Nombre`, `Stock` y `Precio`.

---

### 3.4 Interface Segregation Principle (ISP) — Principio de Segregación de Interfaces

> **Definición**: Los clientes no deben estar obligados a depender de interfaces que no utilizan.

#### Diagnóstico
`Program.cs` no utiliza ninguna interfaz para interactuar con la capa de servicios de `BibFarmacia`. En su lugar, depende directamente de **clases concretas monolíticas ("Fat Classes")**:

1. **Dependencia Directa de Implementaciones Monolíticas**: `Program.cs` mantiene referencias completas a `ServicioProducto`, `ServicioCliente`, `ServicioUsuario` y `ServicioMovimiento`.
2. **Exposición Indiscriminada de Métodos**: La interfaz pública de `ServicioProducto` expone métodos de carga (`CargarDesdeArchivo`), métodos de verificación (`VerificarStock`, `VerificarVencimiento`), y consultas de dominio (`ObtenerProductos`). `Program.cs` accede a todo este catálogo sin contratos segregados (e.g., `IProductoReader`, `IStockChecker`, `IVentaService`).
3. **Acoplamiento a Eventos Concretos**: En L22-65, `Program.cs` navega por propiedades anidadas de eventos concretos:
   - `servicioProducto.EventoStock.StockMinimo`
   - `servicioProducto.EventoVencimiento.Vencimiento`
   - `servicioCliente.EventoPuntos.PuntosAcumulados`
   - `servicioMovimiento.EventoMovimiento.MovimientoRegistrado`  
   No existe una interfaz `INotificacionListener` o de suscripción unificada.

#### Evidencia en Código
- **L8, L11, L14, L17**: Declaraciones de variables de tipo concreto `ServicioProducto`, `ServicioCliente`, `ServicioUsuario`, `ServicioMovimiento`.

---

### 3.5 Dependency Inversion Principle (DIP) — Principio de Inversión de Dependencias

> **Definición**: Los módulos de alto nivel no deben depender de módulos de bajo nivel. Ambos deben depender de abstracciones. Las abstracciones no deben depender de detalles. Los detalles deben depender de abstracciones.

#### Diagnóstico
`Program.cs` (módulo de alto nivel / orquestador de la aplicación) viola totalmente DIP al depender de implementaciones concretas y detalles técnicos de bajo nivel:

1. **Instanciación Rígida con `new`**:
   - `ServicioProducto servicioProducto = new ServicioProducto();` (L8-9)
   - `ServicioCliente servicioCliente = new ServicioCliente();` (L11-12)
   - `ServicioUsuario servicioUsuario = new ServicioUsuario();` (L14-15)
   - `ServicioMovimiento servicioMovimiento = new ServicioMovimiento();` (L17-18)
   - `Movimiento venta = new Movimiento(...);` (L283-288)
2. **Dependencia de Archivos Fisicos y Rutas Relativas Quemadas**:
   - `"productos.txt"` (L79)
   - `"clientes.txt"` (L83)
   - `"usuarios.txt"` (L87)  
   Imposibilita cambiar el origen de datos a base de datos, memoria o API sin modificar `Program.cs`.
3. **Ausencia Total de Inyección de Dependencias (DI)**: No existe un contenedor DI, ni constructores ni fábricas abstraídas. No es posible realizar pruebas unitarias sobre `Program.cs` ni simular (mockear) las dependencias de servicio o de I/O de consola.
4. **Acoplamiento Directo a la Consola del Sistema**: Invocaciones directas e incondicionales a `System.Console` (`Console.WriteLine`, `Console.ReadLine`, `Console.ForegroundColor`), impidiendo reutilizar el flujo de la aplicación en entornos web, GUI o de prueba.

---

## 4. Análisis de Archivos de Datos (`productos.txt`, `clientes.txt`, `usuarios.txt`)

Los archivos de texto plano ubicados en `AppFarmaciaConsola/` presentan las siguientes características y restricciones:

1. **`productos.txt`** (10 registros):
   - Formato: `Nombre;Precio;Stock;StockMinimo;FechaVencimiento;Laboratorio`
   - Ejemplo: `Dolex;5000;2;5;2025-10-10;MK`
   - **Problema de diseño**: La estructura presupone que todos los elementos son medicamentos farmacéuticos producidos por un laboratorio. No hay campo de discriminación de tipo/categoría, lo que imposibilita la carga polimórfica directa para SC-1 (cosméticos/comestibles) o SC-2 (servicios) sin cambiar el parser en `ServicioProducto`.
2. **`clientes.txt`** (10 registros):
   - Formato: `Nombre;Cedula;Telefono;Correo`
   - Ejemplo: `Carlos;123;3001111111;carlos@gmail.com`
3. **`usuarios.txt`** (5 registros):
   - Formato: `Nombre;Cedula;Telefono;Correo;Username;Password`
   - Ejemplo: `Administrador;999;3000000000;admin@gmail.com;admin;1234`

**Impacto en `Program.cs`**: Las rutas relativas directas (`"productos.txt"`, `"clientes.txt"`, `"usuarios.txt"`) fallan si la aplicación se ejecuta desde un directorio de trabajo distinto a la carpeta raíz de salida de build.

---

## 5. Tabla Consolidada de Hallazgos SOLID en Consola y Punto de Entrada

| ID | Ubicación (archivo / clase / línea) | Síntoma observado | Principio comprometido | Impacto en el negocio | Severidad | Fix Sugerido |
|---|---|---|---|---|---|---|
| **H-CON-01** | `Program.cs`: L8, L11, L14, L17, L283 | Instanciación directa con `new` de servicios concretos y entidades de dominio. | **DIP** | Alto costo y riesgo al intentar cambiar la infraestructura, probar el código o migrar a una base de datos. | **Alta** | Inyectar abstracciones (`IServicioProducto`, etc.) mediante un contenedor DI (`Microsoft.Extensions.DependencyInjection`). |
| **H-CON-02** | `Program.cs`: L6–L378 | Una sola clase/script maneja menú, I/O, colores, parsing, búsquedas LINQ y mutación de estado. | **SRP** | Alta fragilidad; cualquier cambio en la interfaz o formato rompe la orquestación y el manejo de ventas. | **Alta** | Extraer controladores de interfaz (`ConsoleUI`), parsers de input (`ConsoleInputReader`) y casos de uso/orquestadores. |
| **H-CON-03** | `Program.cs`: L145, L169–L374 | Estructura `switch` fija para menú. Agregar opciones (SC-1, SC-2, SC-3) exige modificar `Program.cs`. | **OCP** | Incremento exponencial del tiempo de desarrollo y riesgo de introducir bugs en opciones existentes. | **Alta** | Implementar el patrón Command/Strategy para las acciones del menú (`IMenuCommand`), permitiendo registrar nuevas acciones sin alterar el bucle principal. |
| **H-CON-04** | `Program.cs`: L280–L281 | Mutación directa `productoVenta.Stock -= cantidad;` desde el código de la UI de consola. | **SRP / LSP** | Violación de la encapsulación de dominio; riesgo de inconsistencia de datos y fallos con productos no inventariables. | **Alta** | Delegar la transacción de venta completa al servicio de dominio (`servicioVenta.RegistrarVenta(...)`), el cual encapsulará la regla de descuento. |
| **H-CON-05** | `Program.cs`: L226–L231, L263–L269, L313–L319 | Expresiones LINQ de búsqueda escritas directamente dentro de las cláusulas `case` del menú. | **SRP** | Lógica de búsqueda duplicada e ineficiente; imposible de reutilizar en otros clientes (GUI, Web, API). | **Media** | Encapsular métodos de búsqueda con criterios en los servicios de dominio (`BuscarPorNombre`). |
| **H-CON-06** | `Program.cs`: L167, L277, L327 | Parsing de enteros con `int.Parse(...)` sin manejo de excepciones (`FormatException`, `OverflowException`). | **Robustez / SRP** | Caídas inesperadas del sistema ante cualquier error tipográfico del usuario en la consola. | **Media** | Crear un helper de lectura de consola seguro (`ConsoleInput.ReadInt(...)`) con `int.TryParse` y re-intento. |
| **H-CON-07** | `Program.cs`: L79, L83, L87 | Nombres de archivos y rutas relativas quemados directamente en el código del punto de entrada. | **DIP** | Imposibilidad de cambiar de entorno (desarrollo, pruebas, producción) o modificar la ubicación de archivos. | **Media** | Abstraer la configuración de archivos mediante `IConfiguration` o proveedores de repositorios. |
| **H-CON-08** | `Program.cs`: L78–L87 | Firmas heterogéneas para la carga de datos (`CargarDesdeArchivo` vs `Cargar`). | **OCP / ISP** | Mayor curva de aprendizaje y acoplamiento a detalles específicos de implementación de cada servicio. | **Baja** | Homologar contratos de carga bajo una interfaz genérica `ICargable<T>` o abstraer la persistencia por completo. |

---

## 6. Conclusión de la Investigación de Consola

El punto de entrada `AppFarmaciaConsola/Program.cs` requiere una refactorización profunda bajo una arquitectura en capas o limpia (Clean Architecture / Ports and Adapters). La consola debe ser degradada a un mero adaptador de entrada/salida (UI Driver), delegando la orquestación a mediadores/casos de uso e inyectando los servicios de dominio a través de abstracciones.
