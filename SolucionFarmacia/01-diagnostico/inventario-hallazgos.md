# Inventario Consolidado de Hallazgos Arquitectónicos SOLID (AS-IS)

**Sistema**: SolucionFarmacia (C# .NET 8)  
**Fase**: Phase 1 — Diagnóstico Arquitectónico AS-IS  
**Documento**: Inventario Consolidado de Hallazgos  
**Fecha de Emisión**: 2026-08-05  
**Proyectos Analizados**: `BibFarmacia` (26 archivos `.cs`) y `AppFarmaciaConsola` (`Program.cs`, 378 líneas)  
**Integrador de Diagnóstico**: `teamwork_preview_worker_inv` (Master Findings Inventory Specialist)  

---

## 1. Resumen del Diagnóstico

El presente documento consolida la totalidad de hallazgos de violaciones arquitectónicas identificadas durante la auditoría estática realizada sobre la solución heredada **SolucionFarmacia**. La evaluación abarcó de manera rigurosa los cinco principios SOLID (**Single Responsibility**, **Open/Closed**, **Liskov Substitution**, **Interface Segregation**, **Dependency Inversion**) cruzándolos contra las necesidades de extensibilidad expresadas en las Solicitudes de Cambio Futuras del negocio:

- **SC-1**: Comercialización de nuevos productos no medicamentosos (cosméticos, bebidas, helados, snacks).
- **SC-2**: Venta de servicios médicos intangibles (inyectología, cambio de vendajes, curaciones básicas).
- **SC-3**: Implementación de convenios corporativos e institucionales (empresas, bancos, universidades) para descuentos y líneas de crédito.

El inventario maestro se compone de **25 hallazgos únicos y trazables**, detallando la ubicación exacta a nivel de archivo, clase y líneas de código, el síntoma técnico detectado, el principio comprometido, la traducción explícita a impacto en el negocio y la severidad correspondiente.

---

## 2. Tabla Maestra Consolidada de Hallazgos (25 Hallazgos)

| ID | Ubicación (archivo / clase / línea) | Síntoma observado | Principio comprometido | Impacto en el negocio | Severidad |
|---|---|---|---|---|---|
| **H-01** | `AppFarmaciaConsola/Program.cs`<br>**Clase/Módulo**: `Top-Level Statements`<br>**Líneas**: 1–378 | `Program.cs` actúa como un "God Script" monolítico acumulando 7 responsabilidades heterogéneas (UI consola, parsing E/S, ciclo de vida, consultas LINQ, mutación de inventario, instanciación rígida y rutas de archivos). | **SRP** | **Riesgo de Regresión y Falla Operativa en Producción**: Al acoplar la interfaz visual con la lógica de ventas en un solo archivo, cualquier cambio cosmético en pantalla puede corromper el cálculo de ventas o congelar la atención en caja, generando pérdidas económicas directas y retrasos en atención a clientes. | **Alta** |
| **H-02** | `BibFarmacia/Servicios/ServicioProducto.cs`<br>**Clase**: `ServicioProducto`<br>**Líneas**: 12–119 | `ServicioProducto` combina la gestión del catálogo en memoria, la evaluación de reglas de expiración, el I/O físico de archivos CSV y la instanciación concreta de entidades. | **SRP** | **Incapacidad de Expandir el Portafolio (SC-1 y SC-2)**: La mezcla de carga de archivos CSV con verificaciones de vencimiento imposibilita la introducción de cosméticos/comestibles o servicios de salud (inyectología), bloqueando el lanzamiento de nuevas fuentes de ingresos. | **Alta** |
| **H-03** | `BibFarmacia/Servicios/ServicioCliente.cs`<br>**Clase**: `ServicioCliente`<br>**Líneas**: 12–82 | `ServicioCliente` mezcla la administración del catálogo de clientes y acumulación de puntos con la lectura física de archivos CSV y el disparo directo de eventos. | **SRP** | **Imposibilidad de Automatización de Pruebas y Riesgo Financiero**: No se pueden ejecutar pruebas automatizadas sobre la lógica de fidelización sin depender de archivos físicos en disco. Aumenta los costos de QA y expone a la farmacia a errores o fraude en la asignación de puntos. | **Alta** |
| **H-04** | `BibFarmacia/Servicios/ServicioUsuario.cs`<br>**Clase**: `ServicioUsuario`<br>**Líneas**: 12–74 | `ServicioUsuario` combina la gestión de la colección de usuarios en memoria con la invocación de helpers estáticos de autenticación y la lectura directa de archivos CSV en disco. | **SRP** | **Riesgo Crítico de Seguridad y Vulnerabilidad de Datos**: Acoplar la lectura de credenciales desde archivos de texto plano con la gestión de usuarios expone las contraseñas del personal a filtraciones de datos, incumpliendo normativas de privacidad y elevando costos de auditoría. | **Alta** |
| **H-05** | `BibFarmacia/Clases/Producto.cs`<br>**Clase**: `Producto`<br>**Líneas**: 29–34 | La entidad abstracta de dominio `Producto` incluye el método `MostrarInformacion()` con acoplamiento directo a `Console.WriteLine` de la capa de interfaz. | **SRP** | **Imposibilidad de Omnicanalidad y Reutilización**: La inclusión de salidas por consola en la entidad de dominio impide utilizar `BibFarmacia` en aplicaciones móviles, e-commerce web o APIs REST, forzando a reescribir las reglas de negocio para cada nuevo canal. | **Media** |
| **H-06** | `BibFarmacia/Servicios/ServicioDescuento.cs`<br>**Clase**: `ServicioDescuento`<br>**Líneas**: 11–17 | `ServicioDescuento` hardcodea un porcentaje de descuento fijo del 10% (`0.10m`) directamente en el código fuente. | **OCP** | **Bloqueo del Requerimiento SC-3 y Pérdida de Competitividad Comercial**: El descuento fijo del 10% impide negociar convenios corporativos diferenciados (15% universidades, 20% empresas). Implementar nuevos convenios exige modificar y redesplegar el núcleo del sistema, aumentando el *time-to-market*. | **Alta** |
| **H-07** | `BibFarmacia/Servicios/ServicioProducto.cs`<br>**Clase**: `ServicioProducto`<br>**Líneas**: 75–118 | `CargarDesdeArchivo` asume rígida e indefectiblemente que todos los registros del archivo son `MedicamentoCapsula`, instanciando laboratorios inventados y tipos de relleno fijos. | **OCP** | **Bloqueo de SC-1 y SC-2 / Riesgo de Corrupción de Inventario**: La carga masiva asume forzosamente medicamentos en cápsula. Intentar cargar comestibles, cosméticos o servicios médicos causará fallos en el procesamiento de catálogos o registrará información falsa en el inventario. | **Alta** |
| **H-08** | `BibFarmacia/Factories/ProductoFactory.cs`<br>**Clase**: `ProductoFactory`<br>**Líneas**: 11–44 | `ProductoFactory` expone métodos estáticos rígidos por subtipo concreto (`CrearCapsula`, `CrearLiquido`) y quema reglas de negocio por defecto (stock mínimo 5, expiraciones a 6/12 meses). | **OCP** | **Rigidez Operativa y Costos Elevados de Mantenimiento**: Los parámetros por defecto quemados (umbrales de stock mínimo de 5 unidades, vencimientos fijos) impiden adaptar la política de inventarios a promociones sin modificar y compilar las fábricas del sistema. | **Media** |
| **H-09** | `BibFarmacia/Aspectos/AspectoValidacion.cs`<br>**Clase**: `AspectoValidacion`<br>**Líneas**: 11–45 | `AspectoValidacion` agrupa métodos estáticos de validación para múltiples entidades (`Cliente`, `Producto`) en una sola clase centralizada. | **OCP** | **Alto Riesgo de Regresión Cross-Entidad**: Validar clientes y productos dentro de la misma clase estática genera un acoplamiento que duplica el riesgo de romper la validación de clientes al realizar modificaciones sobre las reglas comerciales de productos. | **Media** |
| **H-10** | `AppFarmaciaConsola/Program.cs`<br>**Clase/Módulo**: `Top-Level Statements`<br>**Líneas**: 145–374 | El menú principal está estructurado en un bucle `while (opcion != 7)` con un bloque `switch (opcion)` cerrado de 7 opciones fijas. | **OCP** | **Retraso en el Desarrollo y Rigidez de Menú**: Cualquier nueva funcionalidad requerida por el negocio (ej. consultar convenios o registrar servicios) exige alterar el flujo principal del menú y redesplegar la aplicación, incrementando horas de desarrollo. | **Alta** |
| **H-11** | `BibFarmacia/Clases/Producto.cs`<br>**Clase**: `Producto`<br>**Líneas**: 10–14, 16–27 | La clase base abstracta `Producto` impone obligatoriamente las propiedades `Stock`, `StockMinimo` y `FechaVencimiento` a todos los subtipos derivados. | **LSP** | **Bloqueo Absoluto de SC-2 (Venta de Servicios de Salud)**: Forzar propiedades de inventario físico y fecha de vencimiento a la abstracción de producto impedirá registrar servicios intangibles (inyectología), provocando caídas del sistema o inconsistencias en reportes. | **Alta** |
| **H-12** | `BibFarmacia/Clases/Medicamento.cs`<br>**Clase**: `Medicamento`<br>**Líneas**: 9–24 | La clase intermedia `Medicamento` exige de manera obligatoria una referencia al objeto `Laboratorio` para todos sus subtipos. | **LSP** | **Bloqueo de SC-1 (Productos No Farmacéuticos)**: Obligar a asociar un `Laboratorio` a la jerarquía de medicamentos impide catalogar productos de consumo general (gaseosas, snacks) o cosméticos sin inventar laboratorios ficticios en la base de datos. | **Alta** |
| **H-13** | `BibFarmacia/Clases/Producto.cs` (L29–34)<br>`BibFarmacia/Clases/MedicamentoCapsula.cs` (L11–29)<br>`BibFarmacia/Clases/MedicamentoLiquido.cs` (L11–32) | `Producto` define `MostrarInformacion()` como un método `virtual`, pero las subclases derivadas omiten proporcionar una implementación `override`. | **LSP** | **Pérdida de Visibilidad Comercial y Desinformación al Cliente**: Al invocar la presentación de productos desde una referencia abstracta, el sistema omite mostrar atributos clave (tipo de relleno, mililitros, envase), ocasionando confusión y errores de despacho en caja. | **Media** |
| **H-14** | `AppFarmaciaConsola/Program.cs`<br>**Clase/Módulo**: `Top-Level Statements`<br>**Líneas**: 280–281 | `Program.cs` ejecuta la mutación directa de inventario `productoVenta.Stock -= cantidad` sobre la referencia abstracta `Producto`. | **LSP** | **Riesgo Crítico de Corrupción de Inventarios y Pérdidas Financieras**: Restar el stock en la UI sin validaciones provocará registros de inventario negativo o ventas no respaldadas cuando se comercialicen servicios de salud (SC-2), generando descuadres contables. | **Alta** |
| **H-15** | `BibFarmacia/Clases/Cliente.cs` (L20–23)<br>`BibFarmacia/Servicios/ServicioDescuento.cs` (L13–16) | Los métodos de negocio `Cliente.AcumularPuntos` y `ServicioDescuento.CalcularDescuento` carecen de validaciones de precondiciones (puntos o precios negativos). | **LSP** | **Vulnerabilidad a Transacciones Maliciosas o Errores de Registro**: La falta de validación de precondiciones permite registrar acumulación de puntos negativos o descuentos sobre precios negativos, exponiendo a la empresa a pérdidas de dinero por fraudes o errores humanos. | **Media** |
| **H-16** | `BibFarmacia/Clases/Producto.cs` (L8–35)<br>`Cliente.cs` (L9–25)<br>`Usuario.cs` (L8–22) | Ausencia total de interfaces de dominio segregadas por rol de negocio (`IVendible`, `IStockable`, `IPerishable`, `IIdentificable`). | **ISP** | **Incapacidad de Crear Roles Diferenciados y Rigidez de Integración**: Al no disponer de interfaces de dominio por rol, los módulos de facturación o reportes dependen de toda la estructura de producto, aumentando el esfuerzo técnico para integrar nuevos ítems comerciales. | **Alta** |
| **H-17** | `BibFarmacia/Servicios/ServicioProducto.cs` (L12–119)<br>`ServicioCliente.cs` (L12–82) | Clases de servicio monolíticas ("Fat Services") que concentran múltiples responsabilidades sin implementar interfaces segregadas por rol de cliente. | **ISP** | **Costos de Pruebas Elevados y Acoplamiento Excesivo**: Los servicios monolíticos obligan a cualquier cliente que requiera una consulta básica a cargar módulos pesados de persistencia y alertas, elevando la complejidad del mantenimiento y la posibilidad de fallos en cascada. | **Alta** |
| **H-18** | `BibFarmacia/Eventos/EventoStockMinimo.cs` (L17–22)<br>`EventoVencimiento.cs` (L19–24) | Los métodos `Disparar` de los emisores de eventos exigen una instancia completa de la clase pesada `Producto` para leer únicamente su propiedad `Nombre`. | **ISP** | **Rigidez del Sistema de Alertas Institucionales**: La incapacidad de emitir alertas sobre objetos que no sean estrictamente `Producto` impide reutilizar la infraestructura de notificaciones para insumos médicos, equipos o avisos a clientes. | **Baja** |
| **H-19** | `AppFarmaciaConsola/Program.cs`<br>**Clase/Módulo**: `Top-Level Statements`<br>**Líneas**: 8–18, 78–87 | La interfaz de consola depende 100% de servicios concretos sin utilizar interfaces segregadas de consulta, facturación o administración. | **ISP** | **Incapacidad de Escalar a Diferentes Perfiles de Usuario**: La consola accede directamente a todas las capacidades administrativas, impidiendo limitar el acceso según el perfil de empleado (cajero vs. administrador), lo que genera riesgos de seguridad operativa. | **Alta** |
| **H-20** | `BibFarmacia/Servicios/ServicioProducto.cs` (L75)<br>`ServicioCliente.cs` (L47)<br>`ServicioUsuario.cs` (L37) | Inexistencia de un contrato de interfaz común o segregado para la persistencia e I/O de datos (`IDataLoader<T>` o `IRepository<T>`). | **ISP** | **Sobrecosto de Desarrollo en Integraciones de Persistencia**: La falta de una interfaz unificada de persistencia duplica el tiempo de desarrollo necesario para integrar el sistema con nuevas fuentes de datos o bases de datos relacionales. | **Media** |
| **H-21** | `BibFarmacia/Servicios/ServicioCliente.cs`<br>**Clase**: `ServicioCliente`<br>**Líneas**: 47–81, 22 | `ServicioCliente` depende directamente de `System.IO.File` para la lectura de disco e instancia concretamente `new EventoPuntos()`. | **DIP** | **Falta de Testabilidad y Dependencia de Infraestructura Física**: No es posible verificar las reglas de negocio de clientes en ambientes de integración continua (CI/CD) sin archivos físicos en el servidor, aumentando el tiempo de pruebas manuales y los costos de despliegue. | **Alta** |
| **H-22** | `BibFarmacia/Servicios/ServicioProducto.cs`<br>**Clase**: `ServicioProducto`<br>**Líneas**: 75–118, 23–24 | `ServicioProducto` depende de `File.ReadAllLines` e instancia explícitamente `new Laboratorio()`, `new MedicamentoCapsula()` y los eventos de stock/vencimiento. | **DIP** | **Imposibilidad de Simular Escenarios de Inventario (Mocks)**: El acoplamiento del servicio de productos al sistema de archivos local impide simular fallos de stock o vencimientos en pruebas automatizadas, dejando ocultos errores críticos antes de llegar a producción. | **Alta** |
| **H-23** | `BibFarmacia/Servicios/ServicioUsuario.cs`<br>**Clase**: `ServicioUsuario`<br>**Líneas**: 31, 37–73 | `ServicioUsuario` invoca directamente el método estático `AspectoAutenticacion.Login` e I/O de disco sin una abstracción de autenticación inyectada. | **DIP** | **Bloqueo a la Modernización de Seguridad de Accesos**: La invocación estática de autenticación imposibilita migrar el login hacia servicios modernos (Directorio Activo, OAuth2, JWT) sin refactorizar por completo los servicios de usuario. | **Alta** |
| **H-24** | `BibFarmacia/Factories/ProductoFactory.cs`<br>**Clase**: `ProductoFactory`<br>**Líneas**: 24, 39 | `ProductoFactory` depende directamente del reloj del sistema operativo (`DateTime.Now`) y retorna tipos concretos en lugar de abstracciones. | **DIP** | **Incapacidad de Probar Lógica Temporal y Falta de Determinismo**: Al depender de `DateTime.Now`, las pruebas sobre caducidad de medicamentos producen resultados cambiantes según la hora de ejecución, imposibilitando un control de calidad automatizado. | **Media** |
| **H-25** | `AppFarmaciaConsola/Program.cs`<br>**Clase/Módulo**: `Top-Level Statements`<br>**Líneas**: 8–18, 79, 83, 87, 283 | Ausencia total de un contenedor de Inyección de Dependencias (DI); instanciación rígida con `new`, rutas de archivos hardcodeadas y creación directa de transacciones. | **DIP** | **Bloqueo Total a la Arquitectura Empresarial y Alta Fragilidad**: La ausencia de Inyección de Dependencias impida reemplazar componentes, configurar diferentes entornos (dev, test, prod) o adaptar el software a SC-1, SC-2 y SC-3 sin reescribir gran parte de la aplicación. | **Alta** |

---

## 3. Desgloses Estadísticos de los Hallazgos

A continuación se presentan las métricas consolidadas del inventario arquitectónico, analizando la distribución de las fallas por principio SOLID, nivel de severidad y capa de la solución.

### 3.1 Distribución por Principio SOLID Comprometiendo

| Principio SOLID | Número de Hallazgos | Porcentaje del Total (%) | Hallazgos Asociados |
|---|---|---|---|
| **Single Responsibility Principle (SRP)** | 5 | 20.0% | H-01, H-02, H-03, H-04, H-05 |
| **Open/Closed Principle (OCP)** | 5 | 20.0% | H-06, H-07, H-08, H-09, H-10 |
| **Liskov Substitution Principle (LSP)** | 5 | 20.0% | H-11, H-12, H-13, H-14, H-15 |
| **Interface Segregation Principle (ISP)** | 5 | 20.0% | H-16, H-17, H-18, H-19, H-20 |
| **Dependency Inversion Principle (DIP)** | 5 | 20.0% | H-21, H-22, H-23, H-24, H-25 |
| **TOTAL** | **25** | **100.0%** | — |

> **Observación**: La distribución uniforme (5 hallazgos por principio) refleja un deterioro generalizado en todas las dimensiones de diseño orientado a objetos de la solución AS-IS.

---

### 3.2 Distribución por Categoría de Severidad

| Severidad | Descripción del Criterio | Número de Hallazgos | Porcentaje (%) |
|---|---|---|---|
| **Alta** | Defectos que bloquean directamente las solicitudes de cambio comerciales (SC-1, SC-2, SC-3), corrompen estado de dominio, comprometen la seguridad o impiden ejecutar pruebas unitarias. | 17 | 68.0% |
| **Media** | Violaciones que generan rigidez, duplican el riesgo de regresiones entre entidades, impiden la reutilización omnicanal o debilitan invariantes de negocio. | 7 | 28.0% |
| **Baja** | Ineficiencias menores en el formateo de mensajes o desacoplamiento de infraestructura secundaria sin impacto catastrófico inmediato. | 1 | 4.0% |
| **TOTAL** | — | **25** | **100.0%** |

```
[██████████████████████████████████████████  68%  ] Alta (17)
[█████████████████                           28%  ] Media (7)
[██                                           4%  ] Baja (1)
```

---

### 3.3 Distribución por Capa Arquitectónica Afectada

| Capa Arquitectónica | Componentes Incluidos | Número de Hallazgos | Porcentaje (%) | Hallazgos Asociados |
|---|---|---|---|---|
| **Presentación / Consola** | `AppFarmaciaConsola/Program.cs` | 5 | 20.0% | H-01, H-10, H-14, H-19, H-25 |
| **Servicios de Negocio** | `BibFarmacia/Servicios/` (`ServicioProducto`, `ServicioCliente`, `ServicioUsuario`, `ServicioDescuento`, etc.) | 10 | 40.0% | H-02, H-03, H-04, H-06, H-07, H-17, H-20, H-21, H-22, H-23 |
| **Dominio y Entidades** | `BibFarmacia/Clases/` (`Producto`, `Medicamento`, `Cliente`, `Persona`, etc.) | 6 | 24.0% | H-05, H-11, H-12, H-13, H-15, H-16 |
| **Infraestructura, Fábricas y Aspectos** | `BibFarmacia/Factories/`, `Aspectos/`, `Eventos/` | 4 | 16.0% | H-08, H-09, H-18, H-24 |
| **TOTAL** | — | **25** | **100.0%** | — |

---

## 4. Evaluación Sintética del Impacto en Solicitudes de Cambio (SC-1, SC-2, SC-3)

La consolidación de los 25 hallazgos demuestra numéricamente por qué la arquitectura AS-IS no puede soportar las metas de crecimiento del negocio sin un rediseño profundo:

1. **Impacto en SC-1 (Nuevas Categorías de Productos: Cosméticos y Comestibles)**:
   - **Hallazgos bloqueantes directos**: H-02, H-07, H-08, H-12, H-16, H-22.
   - **Causa raíz**: La jerarquía `Medicamento` exige `Laboratorio` obligatoriamente (H-12) y `ServicioProducto.CargarDesdeArchivo` instancia sólo cápsulas (H-07).

2. **Impacto en SC-2 (Venta de Servicios de Salud: Inyectología y Curaciones)**:
   - **Hallazgos bloqueantes directos**: H-02, H-07, H-11, H-14, H-16.
   - **Causa raíz**: `Producto.cs` impone `Stock` y `FechaVencimiento` (H-11), y `Program.cs` decrementa stock directamente (`Stock -= cantidad`, H-14). Los servicios médicos carecen de inventario físico, por lo que el flujo actual fallará en ejecución.

3. **Impacto en SC-3 (Convenios e Integraciones Corporativas / Créditos)**:
   - **Hallazgos bloqueantes directos**: H-06, H-09, H-10, H-15, H-23, H-25.
   - **Causa raíz**: `ServicioDescuento` retoma un porcentaje fijo del 10% quemado en código (H-06) y `Program.cs` carece de inyección de dependencias para resolver la estrategia de descuento por convenio (H-25).

---

## 5. Conclusiones y Siguientes Pasos

1. **Priorización de Refactorización**: El 68% de los hallazgos presentan **Severidad Alta**, lo que exige que la Fase 2 (Rediseño TO-BE) priorice la extracción de interfaces de dominio, repositorios de persistencia y la introducción de un contenedor de Inyección de Dependencias.
2. **Desacoplamiento de Consola**: `Program.cs` debe ser despojado de toda lógica de negocio y mutación de estado, convirtiéndose en un controlador delgado de menú alimentado por Inyección de Dependencias.
3. **Pauta para la Fase TO-BE**: El rediseño debe aplicar los patrones **Strategy** (para descuentos por convenio), **Command** (para el menú de consola), **Factory / Repository** (para persistencia e instanciación polimórfica) e **Inversión de Dependencias** mediante `Microsoft.Extensions.DependencyInjection`.
