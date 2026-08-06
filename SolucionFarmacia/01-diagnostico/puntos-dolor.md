# Diagnóstico Arquitectónico SOLID — Los 3 Puntos de Dolor Priorizados

**Sistema**: Solución Farmacia (`BibFarmacia` y `AppFarmaciaConsola`)  
**Fase**: 01-diagnostico (AS-IS)  
**Agente Especialista**: `worker_pain_m4` (Architectural Pain Points Specialist)  
**Fecha de Emisión**: 2026-08-05  
**Ubicación del Entregable**: `01-diagnostico/puntos-dolor.md`  

---

## 1. Introducción y Propósito del Diagnóstico

El presente documento consolida y sintetiza los hallazgos técnicos derivados de los cinco (5) análisis SOLID individuales presentados previamente (`analisis-srp.md`, `analisis-ocp.md`, `analisis-lsp.md`, `analisis-isp.md` y `analisis-dip.md`). El objetivo fundamental es abstraer la densidad técnica del diagnóstico AS-IS e identificar **EXACTAMENTE TRES (3) PUNTOS DE DOLOR ARQUITECTÓNICOS CRÍTICOS** que amenazan la estabilidad, mantenibilidad y capacidad de evolución del sistema de farmacia.

Para demostrar el impacto real de estos tres problemas sobre la capacidad de cambio de la empresa, cada punto de dolor es sometido a un riguroso análisis de impacto frente a las tres (3) **Solicitudes de Cambio Futuras (SC)** planteadas por la organización:

- **SC-1 (Nuevas Categorías de Productos)**: Venta de cosméticos, comestibles y abarrotes (gaseosas, agua, helados, snacks).
- **SC-2 (Venta de Servicios de Salud)**: Prestación e integración de servicios médicos como inyectología, cambio de vendajes y curaciones básicas.
- **SC-3 (Convenios e Instituciones)**: Gestión de convenios con entidades externas (empresas, bancos, cooperativas, mutuales, universidades) para otorgamiento de descuentos y crédito corporativo.

---

## 2. Criterio Explícito y Defendible de Priorización

Para establecer el orden de precedencia estricto de los puntos de dolor (**Punto de Dolor #1 > Punto de Dolor #2 > Punto de Dolor #3**), se formuló un **Marco Multidimensional de Priorización Arquitectónica** basado en cuatro (4) vectores de evaluación técnica y estratégica:

1. **Amplitud del Riesgo Sistémico y Contaminación en Cascada**: Un defecto conceptual en la capa de dominio contamina inevitablemente las capas superiores (Servicios y UI). Por el contrario, un defecto en la capa de UI no corrompe la integridad del dominio.
2. **Bloqueo a la Capacidad de Evolución del Negocio (Business Evolution Capacity)**: Medido por el nivel de parálisis que la falla impone sobre el cumplimiento de las solicitudes de cambio reales (SC-1, SC-2, SC-3).
3. **Bloqueo a la Testabilidad Automatizada (Testability Blockade)**: Grado en que la falla impide la ejecución de pruebas unitarias aisladas en memoria sin requerir la presencia de archivos físicos o interacción humana por consola.
4. **Acoplamiento Estructural y Complejidad Ciclomática**: Cantidad de componentes que deben abrirse y modificarse para soportar un nuevo requerimiento, incrementando exponencialmente el riesgo de regresión en producción.

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                       CADENA DE PRIORIZACIÓN ARQUITECTÓNICA                  │
├─────────────────────────────────────────────────────────────────────────────┤
│  PUNTO DE DOLOR #1: Modelo de Dominio Monolítico e Inflexible (Capa Núcleo)  │
│  └─► Fundamento: Si las entidades base son rígidas y rompen contratos       │
│      polimórficos, ninguna capa superior puede funcionar correctamente.     │
├─────────────────────────────────────────────────────────────────────────────┤
│  PUNTO DE DOLOR #2: Acoplamiento de Persistencia, I/O y UI en Servicios      │
│  └─► Fundamento: Impide testabilidad, bloquea persistencia y acopla la       │
│      lógica de aplicación a archivos físicos CSV y llamadas a consola.      │
├─────────────────────────────────────────────────────────────────────────────┤
│  PUNTO DE DOLOR #3: Script Monolítico y Controlador Todopoderoso en UI       │
│  └─► Fundamento: Representa la manifestación visible del acoplamiento en la   │
│      capa de presentación, cerrando la adición de nuevos comandos/opciones.  │
└─────────────────────────────────────────────────────────────────────────────┘
```

### Justificación Defendible de Precedencia:

- **¿Por qué el Punto de Dolor #1 precede al #2?**  
  La clase abstracta `Producto` y su jerarquía constituyen el núcleo del modelo de dominio del negocio. Si la representación de un ítem comercial está intrínsecamente viciada (imponiendo stock físico y fechas de vencimiento a conceptos que no los poseen), ningún servicio de aplicación (Punto #2) puede operar correctamente, sin importar cuán limpio esté su código I/O. Arreglar los servicios manteniendo un dominio defectuoso mantendrá el sistema invalido ante servicios de salud (SC-2) o cosméticos (SC-1).

- **¿Por qué el Punto de Dolor #2 precede al #3?**  
  Los servicios de negocio (`ServicioProducto`, `ServicioCliente`, etc.) representan las reglas de aplicación del sistema. Al estar acoplados directamente al disco (`File.ReadAllLines`) y a la consola (`Console.WriteLine`), impiden la automatización de pruebas y la reutilización en otros canales (web, móvil, API). El script de consola (`Program.cs`, Punto #3) es únicamente un consumidor de estos servicios. Resolver `Program.cs` sin desacoplar los servicios mantendría la aplicación atada a archivos CSV hardcodeados e intransferible a otros entornos.

---

## 3. Punto de Dolor #1: Modelo de Dominio Monolítico e Inflexible (`Producto` y Jerarquías Rígidas)

- **Ubicación Principal**: `BibFarmacia/Clases/Producto.cs` (L8–35), `BibFarmacia/Clases/Medicamento.cs` (L9–24), `BibFarmacia/Clases/MedicamentoCapsula.cs` (L11–29), `BibFarmacia/Clases/MedicamentoLiquido.cs` (L11–32).
- **Principios SOLID Comprometidos**: **SRP**, **OCP**, **LSP**, **ISP**, **DIP**.
- **Severidad Arquitectónica**: **CRÍTICA**.

### 3.1 Descripción del Problema Técnico
La clase abstracta base `Producto` impone como invariantes de estado obligatorias las propiedades `Stock` (int), `StockMinimo` (int) y `FechaVencimiento` (DateTime) para **todos** los elementos comercializables por la farmacia, además de incrustar el método `MostrarInformacion()` con salida a `Console.WriteLine`. Adicionalmente, la jerarquía intermedia `Medicamento` fuerza la asociación obligatoria con un objeto `Laboratorio`. No existe ninguna interfaz de rol (`IVendible`, `IStockable`, `IPerishable`, `IIdentificable`) que permita categorizar composicionalmente los comportamientos del dominio.

Esta rigidez estructural genera una ruptura del **Principio de Sustitución de Liskov (LSP)** y del **Principio Abierto/Cerrado (OCP)**, obligando a que cualquier nuevo tipo de ítem de venta tenga que "encajar a la fuerza" en la estructura de un medicamento farmacéutico perecedero con laboratorio.

---

### 3.2 Evaluación Detallada contra las Solicitudes de Cambio (SC)

#### A. Evaluación contra SC-1 (Cosméticos, comestibles: gaseosas, agua, helados, snacks)
- **Archivos / Clases que deben modificarse hoy**: **5 archivos**  
  (`BibFarmacia/Clases/Producto.cs`, `BibFarmacia/Clases/Medicamento.cs`, `BibFarmacia/Factories/ProductoFactory.cs`, `BibFarmacia/Servicios/ServicioProducto.cs`, `BibFarmacia/Aspectos/AspectoValidacion.cs`).
- **Comportamiento que corre riesgo de romperse o regresar**:
  1. **Corrupción de Datos de Laboratorio**: Al no ser medicamentos, los comestibles o cosméticos no poseen un registro de laboratorio farmacéutico. Para reutilizar las rutinas actuales (`ServicioProducto.cs` L93–97), el sistema se ve forzado a registrar laboratorios ficticios o quemados (e.g. `"Medellin"`, `"4444444"`), corrompiendo los reportes comerciales.
  2. **Alertas Falsas de Caducidad**: Los productos comestibles no perecederos a corto plazo o cosméticos de larga duración serán evaluados incondicionalmente por `ServicioProducto.VerificarVencimiento()` (L59–66), disparando falsas alertas de vencimiento a los 30 días si no se les asigna una fecha futura arbitraria.
  3. **Fallos en Deserialización**: La carga desde archivo CSV en `ServicioProducto` fallará o instanciará comestibles como `MedicamentoCapsula` con `TipoRelleno.Gel`.

#### B. Evaluación contra SC-2 (Venta de servicios de salud: inyectología, vendajes, curaciones)
- **Archivos / Clases que deben modificarse hoy**: **6 archivos**  
  (`BibFarmacia/Clases/Producto.cs`, `BibFarmacia/Clases/Medicamento.cs`, `BibFarmacia/Factories/ProductoFactory.cs`, `BibFarmacia/Servicios/ServicioProducto.cs`, `BibFarmacia/Aspectos/AspectoValidacion.cs`, `AppFarmaciaConsola/Program.cs`).
- **Comportamiento que corre riesgo de romperse o regresar**: **FALLA CRÍTICA EN PRODUCCIÓN**.
  1. **Ruptura Semántica y Contable de Inventario**: En `Program.cs` (L280), el proceso de venta ejecuta directamente `productoVenta.Stock -= cantidad`. Un servicio de inyectología o curación **no tiene stock físico**. Si se crea `ServicioSalud : Producto` inicializando `Stock = 0`, la primera venta dejará el stock en `-1`, `-2`, etc. Si se intenta desactivar el setter arrojando `NotSupportedException`, la aplicación colapsará inmediatamente al realizar una venta.
  2. **Alertas Automáticas de Stock Crítico Irrelevantes**: `ServicioProducto.VerificarStock()` (L47–54) comparará incondicionalmente el stock del servicio contra `StockMinimo` (5 unidades por defecto), disparando eventos de alerta `EventoStockMinimo` ininterrumpidos en pantalla.

#### C. Evaluación contra SC-3 (Convenios e instituciones para descuentos y crédito)
- **Archivos / Clases que deben modificarse hoy**: **4 archivos**  
  (`BibFarmacia/Clases/Producto.cs`, `BibFarmacia/Servicios/ServicioDescuento.cs`, `BibFarmacia/Servicios/ServicioCliente.cs`, `AppFarmaciaConsola/Program.cs`).
- **Comportamiento que corre riesgo de romperse o regresar**:
  1. **Incapacidad de Exclusión por Categoria**: Los convenios institucionales suelen aplicar descuentos diferenciados (e.g. 20% en medicamentos, 10% en cosméticos, 0% en servicios de salud). Al no existir interfaces de rol en el dominio, es imposible aplicar estas reglas en `ServicioDescuento` sin agregar condicionales por tipo (`if (producto is Medicamento)`), destruyendo el polimorfismo.

---

## 4. Punto de Dolor #2: Acoplamiento Rígido de Persistencia, I/O y Salida UI en los Servicios ("Fat Services")

- **Ubicación Principal**: `BibFarmacia/Servicios/ServicioProducto.cs` (L12–119), `ServicioCliente.cs` (L12–82), `ServicioUsuario.cs` (L12–74), `ServicioMovimiento.cs` (L11–39), `ServicioNotificacion.cs` (L10–16).
- **Principios SOLID Comprometidos**: **SRP**, **DIP**, **ISP**, **OCP**.
- **Severidad Arquitectónica**: **ALTA**.

### 4.1 Descripción del Problema Técnico
Las clases de servicio del sistema asumen simultáneamente múltiples responsabilidades no cohesivas:
1. Administración del catálogo/estado en memoria.
2. Lectura y parsing físico de archivos planos CSV desde el sistema de archivos local (`File.ReadAllLines`, `linea.Split(';')`).
3. Formateo y emisión de cadenas de texto hacia la consola pública (`Console.WriteLine`).
4. Invocación estática de lógica de infraestructura y aspectos (`AspectoAutenticacion.Login`).
5. Instanciación directa de dependencias con `new` (`new EventoStockMinimo()`, `new List<T>()`).

No existe una sola interfaz de repositorio (`IProductoRepository`, `IClienteRepository`, `IUsuarioRepository`) ni abstracción de tiempo (`IDateTimeProvider`) o logging. Esto bloquea la posibilidad de ejecutar pruebas unitarias automatizadas (Unit Testing) y amarra las reglas de negocio al almacenamiento en texto plano.

---

### 4.2 Evaluación Detallada contra las Solicitudes de Cambio (SC)

#### A. Evaluación contra SC-1 (Cosméticos, comestibles: gaseosas, agua, helados, snacks)
- **Archivos / Clases que deben modificarse hoy**: **4 archivos**  
  (`BibFarmacia/Servicios/ServicioProducto.cs`, `BibFarmacia/Servicios/ServicioCliente.cs`, `BibFarmacia/Factories/ProductoFactory.cs`, `AppFarmaciaConsola/Program.cs`).
- **Comportamiento que corre riesgo de romperse o regresar**:
  1. **Ruptura del Parser CSV Monolítico**: `ServicioProducto.CargarDesdeArchivo` (L75–118) parsea líneas asumiendo exactamente 6 columnas separadas por `;` correspondientes a medicamentos. Si se agrega un archivo `cosmeticos.txt` o se intenta mezclar productos en `productos.txt` con columnas diferentes (e.g. sin laboratorio ni registro sanitario), el método arrojará excepciones `IndexOutOfRangeException` o `FormatException` durante el arranque.
  2. **Bloqueo a Pruebas Automatizadas**: Imposible probar la lógica de adición de comestibles sin crear y borrar archivos físicos `.txt` en el disco duro durante el test runner.

#### B. Evaluación contra SC-2 (Venta de servicios de salud: inyectología, vendajes, curaciones)
- **Archivos / Clases que deben modificarse hoy**: **4 archivos**  
  (`BibFarmacia/Servicios/ServicioProducto.cs`, `BibFarmacia/Servicios/ServicioMovimiento.cs`, `BibFarmacia/Servicios/ServicioNotificacion.cs`, `AppFarmaciaConsola/Program.cs`).
- **Comportamiento que corre riesgo de romperse o regresar**:
  1. **Persistencia Rígida Incompatible**: `ServicioProducto` carece de abstracción para cargar o registrar entidades que no sean archivos físicos de productos. La integración de servicios de salud requerirá duplicar métodos de carga o modificar el servicio existente con ramificaciones condicionales.
  2. **Notificaciones Acopladas a Consola**: `ServicioNotificacion` hardcodea `Console.WriteLine`. La notificación de prestación de servicios médicos hacia sistemas externos o comprobantes impresos queda bloqueada por la salida fija a pantalla.

#### C. Evaluación contra SC-3 (Convenios e instituciones para descuentos y crédito)
- **Archivos / Clases que deben modificarse hoy**: **5 archivos**  
  (`BibFarmacia/Servicios/ServicioDescuento.cs`, `BibFarmacia/Servicios/ServicioCliente.cs`, `BibFarmacia/Servicios/ServicioUsuario.cs`, `BibFarmacia/Servicios/ServicioNotificacion.cs`, `AppFarmaciaConsola/Program.cs`).
- **Comportamiento que corre riesgo of romperse o regresar**:
  1. **Ruptura de `ServicioDescuento`**: El servicio calcula un valor plano hardcodeado (`precio * 0.10m`, L15). Para soportar convenios corporativos con bancos (12%), universidades (15%) o empresas (20%), la clase debe ser modificada quirúrgicamente eliminando su comportamiento probado.
  2. **Fragilidad en Parsing de Clientes**: Modificar `ServicioCliente.Cargar` (L47–81) para leer los datos de convenio corporativo romperá la lectura del archivo actual `clientes.txt` al intentar acceder a posiciones de arreglo inexistentes.

---

## 5. Punto de Dolor #3: Script Monolítico y Controlador Todopoderoso en Presentación (`Program.cs` / `switch`)

- **Ubicación Principal**: `AppFarmaciaConsola/Program.cs` (Líneas 1 a 378).
- **Principios SOLID Comprometidos**: **SRP**, **OCP**, **DIP**, **ISP**.
- **Severidad Arquitectónica**: **MEDIA-ALTA**.

### 5.1 Descripción del Problema Técnico
`Program.cs` funciona como un script "God Object" de 378 líneas que acumula 7 responsabilidades heterogéneas:
1. Instanciación directa de servicios mediante `new` (sin contenedor de Inyección de Dependencias).
2. Orquestación del ciclo de vida (Login $\rightarrow$ Menú $\rightarrow$ Salida).
3. Pintado y formateo visual de la consola (colores, títulos, marcos ASCII).
4. Lectura de teclado y conversiones inseguras de tipo (`int.Parse(Console.ReadLine()!)`).
5. Consultas de lógica de negocio en la vista mediante LINQ (`.FirstOrDefault(...)`).
6. Mutación directa del estado de dominio (`productoVenta.Stock -= cantidad`).
7. Menú principal de 7 opciones cerrado mediante un bloque `switch (opcion)`.

Cualquier adición de una opción de usuario o flujo comercial exige modificar el archivo `Program.cs`, alterar el límite del bucle `while (opcion != 7)` y agregar casos al `switch`, violando flagrantemente el **Principio Abierto/Cerrado (OCP)**.

---

### 5.2 Evaluación Detallada contra las Solicitudes de Cambio (SC)

#### A. Evaluación contra SC-1 (Cosméticos, comestibles: gaseosas, agua, helados, snacks)
- **Archivos / Clases que deben modificarse hoy**: **1 archivo crítico** (`AppFarmaciaConsola/Program.cs` — modificando `case 1`, `case 3` y `case 4`).
- **Comportamiento que corre riesgo de romperse o regresar**:
  1. **Degradación del Menú Consola**: Para permitir la búsqueda o filtrado por categorías de comestibles/cosméticos, debe editarse el bloque `switch` en `Program.cs`, incrementando la complejidad ciclomática del script.
  2. **Excepciones de Referencia Nula (NullReference)**: La búsqueda en `case 3` (L218–254) asume que la propiedad `Nombre` del producto no es nula y realiza búsquedas `.ToLower()`. Datos inconsistentes de nuevos comestibles causarán caídas no controladas de la consola.

#### B. Evaluación contra SC-2 (Venta de servicios de salud: inyectología, vendajes, curaciones)
- **Archivos / Clases que deben modificarse hoy**: **1 archivo crítico** (`AppFarmaciaConsola/Program.cs` — modificando `case 4`, `case 1` y agregando un nuevo `case` al `switch`).
- **Comportamiento que corre riesgo de romperse o regresar**:
  1. **Ejecución de Mutación Indebida en UI**: En `case 4` (L255–304), la consola efectúa `productoVenta.Stock -= cantidad`. Al vender un servicio de inyectología, el operador ingresará la cantidad (e.g. `1`), y la UI intentará decrementar una propiedad física inexistente o irrelevante en un servicio, corrompiendo el estado en memoria.
  2. **Carga Insegura de Datos**: El parsing directo con `int.Parse` en la UI causará excepciones si el operador ingresa caracteres no numéricos al solicitar un servicio.

#### C. Evaluación contra SC-3 (Convenios e instituciones para descuentos y crédito)
- **Archivos / Clases que deben modificarse hoy**: **1 archivo crítico** (`AppFarmaciaConsola/Program.cs` — modificando el flujo de login, la autenticación y el `case 4`).
- **Comportamiento que corre riesgo de romperse o regresar**:
  1. **Contaminación del Flujo de Venta**: Para aplicar un convenio institucional durante la venta (`case 4`), se debe agregar lógica de consulta de cliente, verificación de cupo de crédito y selección de empresa dentro del mismo bloque `case 4`, convirtiendo un método que ya tiene 50 líneas en un bloque ilegible y propenso a errores de regresión.

---

## 6. Matriz Resumen Consolidada de Puntos de Dolor vs. Solicitudes de Cambio

La siguiente matriz ofrece la visión comparativa ejecutiva de los tres puntos de dolor priorizados evaluados frente a las tres solicitudes de cambio del negocio:

| Punto de Dolor Priorizado | Solicitud de Cambio SC-1<br>(Cosméticos y Comestibles) | Solicitud de Cambio SC-2<br>(Servicios de Salud) | Solicitud de Cambio SC-3<br>(Convenios e Instituciones) | Impacto Arquitectónico Global |
|---|---|---|---|---|
| **PUNTO DE DOLOR #1**<br>Modelo de Dominio Monolítico e Inflexible (`Producto`) | **5 archivos a modificar**<br>• Asunción de `Laboratorio`.<br>• Alertas falsas de vencimiento.<br>• Fallos en deserialización CSV. | **6 archivos a modificar**<br>• **Falla Crítica**: Mutación de `Stock` en intangibles.<br>• Falsas alertas de stock cero.<br>• Violación grave de LSP. | **4 archivos a modificar**<br>• Imposibilidad de eximir servicios o aplicar descuentos diferenciados por categoría sin condicionales por tipo. | **Severidad: CRÍTICA**<br>Bloquea la representación correcta de entidades de negocio en todo el sistema. |
| **PUNTO DE DOLOR #2**<br>Acoplamiento de Persistencia, I/O y UI en Servicios | **4 archivos a modificar**<br>• Parser CSV monolítico rígido a 6 columnas.<br>• Imposible realizar Unit Testing en memoria. | **4 archivos a modificar**<br>• Persistencia fija en `productos.txt`.<br>• Notificaciones acopladas a `Console.WriteLine`. | **5 archivos a modificar**<br>• `ServicioDescuento` calcula 10% fijo hardcodeado.<br>• Reading de `clientes.txt` rompe con campos extra. | **Severidad: ALTA**<br>Impide la testabilidad automatizada y amarra las reglas de negocio a archivos físicos CSV. |
| **PUNTO DE DOLOR #3**<br>Script Monolítico y Controlador Todopoderoso (`Program.cs`) | **1 archivo crítico**<br>• Edición masiva de `switch`.<br>• `NullReferenceException` en búsquedas LINQ directas en UI. | **1 archivo crítico**<br>• `productoVenta.Stock -= cantidad` directo en UI.<br>• Excepciones por `int.Parse` inseguro en pantalla. | **1 archivo crítico**<br>• Disparo de complejidad ciclomática en `case 4` de ventas.<br>• Mezcla de flujos de crédito y venta en la UI. | **Severidad: MEDIA-ALTA**<br>Cierra la adición de nuevas acciones de usuario y viola el patrón Command y DI. |

---

## 7. Conclusiones y Hoja de Ruta para el Rediseño (Fase 2 — TO-BE)

Para resolver definitivamente los tres puntos de dolor priorizados y habilitar la extensibilidad limpia del sistema, la arquitectura TO-BE deberá aplicar los siguientes patrones y principios de rediseño en la Fase 2:

1. **Refactorización del Dominio (Punto de Dolor #1)**:
   - Segregar la jerarquía de `Producto` mediante interfaces composables de rol: `IVendible` (Nombre, Precio), `IStockable` (Stock, StockMinimo), `IPerishable` (FechaVencimiento) e `IIdentificable`.
   - Permitir que los servicios de salud (SC-2) implementen exclusivamente `IVendible`, liberándose de atributos de inventario físico o expiración.

2. **Desacoplamiento de Persistencia y Servicios (Punto de Dolor #2)**:
   - Extraer la I/O de archivos físicos hacia repositorios desacoplados (`IProductoRepository`, `IClienteRepository`, `IUsuarioRepository`).
   - Implementar el **Patrón Strategy** (`IDescuentoStrategy`) para soportar dinámicamente convenios institucionales (SC-3) sin editar la clase `ServicioDescuento`.
   - Inyectar la infraestructura de eventos y loggers mediante interfaces (`ILogger`, `IEventBus`).

3. **Descomposición del Orquestador UI (Punto de Dolor #3)**:
   - Eliminar el bloque `switch` monolítico de `Program.cs` aplicando el **Patrón Command** (`IConsoleCommand`) con registro dinámico de opciones de menú.
   - Configurar un contenedor de **Inyección de Dependencias (DI Container)** (`Microsoft.Extensions.DependencyInjection`) en `Program.cs` para resolver abstracciones en lugar de instanciar tipos concretos con `new`.

---
