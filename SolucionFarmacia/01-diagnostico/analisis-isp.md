# Diagnóstico Arquitectónico SOLID: Principio de Segregación de Interfaces (ISP)

**Proyecto**: Sistema Farmacia (Reto Técnico Universitario - 6to Semestre)  
**Documento**: Análisis de Diagnóstico AS-IS para Interface Segregation Principle (ISP)  
**Autor**: Agente Especialista en ISP (`worker_isp_m2`)  
**Fecha**: 2026-08-05  
**Estado**: Completado  

---

## 1. Introducción y Marco Teórico de ISP

El **Principio de Segregación de Interfaces** (*Interface Segregation Principle - ISP*), el cuarto principio de SOLID formulado por Robert C. Martin, establece:

> *"Los clientes no deben estar obligados a depender de interfaces o métodos que no utilizan."*

En términos prácticos de diseño orientados a objetos en C# .NET 8, ISP busca evitar **interfaces o clases "gordas" (Fat Interfaces / Monolithic Classes)** que acumulan responsabilidades disímiles. Cuando una interfaz o contrato de clase es demasiado amplia, los clientes que la consumen terminan acoplándose a métodos y propiedades irrelevantes para su contexto de ejecución. Esto produce acoplamiento innecesario, recompilaciones en cascada y fragilidad ante cambios.

Este informe presenta el diagnóstico exhaustivo de ISP sobre el sistema de farmacia heredado, evaluando los 26 archivos de la biblioteca `BibFarmacia` y el punto de entrada `AppFarmaciaConsola/Program.cs`.

---

## 2. Inventario Exhaustivo de Interfaces y Clases Analizadas

Para este diagnóstico se revisaron el 100% de los elementos del sistema, clasificándolos según la existencia y uso de abstracciones e interfaces:

### 2.1 Interfaces Existentes (Únicas 2 interfaces en toda la solución)
1. `BibFarmacia/Interfaces/IDescuento.cs` (Líneas 9–12): Interfaz con 1 solo método (`CalcularDescuento`).
2. `BibFarmacia/Interfaces/IServicioNotificacion.cs` (Líneas 9–12): Interfaz con 1 solo método (`EnviarNotificacion`).

### 2.2 Clases de Dominio (`BibFarmacia/Clases/`)
3. `Persona.cs` (L9–L24): Clase abstracta base. **Implementa 0 interfaces**.
4. `Cliente.cs` (L9–L25): Hereda de `Persona`. **Implementa 0 interfaces**.
5. `Usuario.cs` (L8–L22): Hereda de `Persona`. **Implementa 0 interfaces**.
6. `Laboratorio.cs` (L9–L24): Clase de datos. **Implementa 0 interfaces**.
7. `Producto.cs` (L8–L35): Clase abstracta de producto. **Implementa 0 interfaces**.
8. `Medicamento.cs` (L9–L24): Hereda de `Producto`. **Implementa 0 interfaces**.
9. `MedicamentoCapsula.cs` (L11–L29): Hereda de `Medicamento`. **Implementa 0 interfaces**.
10. `MedicamentoLiquido.cs` (L11–L32): Hereda de `Medicamento`. **Implementa 0 interfaces**.
11. `Movimiento.cs` (L9–L26): Entidad de transacción. **Implementa 0 interfaces**.

### 2.3 Servicios Core y Concretos (`BibFarmacia/Servicios/`)
12. `ServicioDescuento.cs` (L11–L17): Implementa `IDescuento`.
13. `ServicioNotificacion.cs` (L10–L16): Implementa `IServicioNotificacion`.
14. `ServicioProducto.cs` (L12–L119): Clase monolítica concreta. **Implementa 0 interfaces**.
15. `ServicioCliente.cs` (L12–L82): Clase monolítica concreta. **Implementa 0 interfaces**.
16. `ServicioUsuario.cs` (L12–L74): Clase monolítica concreta. **Implementa 0 interfaces**.
17. `ServicioMovimiento.cs` (L11–L39): Clase monolítica concreta. **Implementa 0 interfaces**.

### 2.4 Emisores y Delegados de Eventos (`BibFarmacia/Eventos/`)
18. `EventoStockMinimo.cs` (L10–L23): Manejador de evento. **Implementa 0 interfaces**.
19. `EventoVencimiento.cs` (L11–L25): Manejador de evento. **Implementa 0 interfaces**.
20. `EventoPuntos.cs` (L9–L24): Manejador de evento. **Implementa 0 interfaces**.
21. `EventoMovimiento.cs` (L9–L23): Manejador de evento. **Implementa 0 interfaces**.

### 2.5 Punto de Entrada y Aplicación de Consola (`AppFarmaciaConsola/`)
22. `Program.cs` (L1–L378): Consumidor directo de todos los servicios y clases concretas.

---

## 3. Evidencia de Cumplimiento del Principio ISP

A pesar de la escasez de interfaces en la solución, existen dos componentes específicos que representan ejemplos impecables de cumplimiento con ISP:

### 3.1 Evidencia C-ISP-01: Interfaces Magras e Implemetaciones Altamente Cohesivas
- **Archivo/Líneas**: 
  - `BibFarmacia/Interfaces/IDescuento.cs` (L9–L12)
  - `BibFarmacia/Interfaces/IServicioNotificacion.cs` (L9–L12)
  - `BibFarmacia/Servicios/ServicioDescuento.cs` (L11–L17)
  - `BibFarmacia/Servicios/ServicioNotificacion.cs` (L10–L16)
- **Código observado**:
  ```csharp
  // IDescuento.cs
  namespace BibFarmacia.Interfaces
  {
      public interface IDescuento
      {
          decimal CalcularDescuento(decimal precio);
      }
  }

  // IServicioNotificacion.cs
  namespace BibFarmacia.Interfaces
  {
      public interface IServicioNotificacion
      {
          void EnviarNotificacion(string mensaje);
      }
  }
  ```
- **Análisis de cumplimiento**: Both interfaces are single-method contracts. They focus on exactly one operation required by their callers. The concrete classes `ServicioDescuento` and `ServicioNotificacion` implement 100% of the contract without leaving empty methods, throwing `NotImplementedException`, or carrying unused parameters.

### 3.2 Evidencia C-ISP-02: Delegados de Eventos con Firmas Angostas
- **Archivo/Líneas**: `BibFarmacia/Eventos/` (`EventoStockMinimo.cs` L12, `EventoVencimiento.cs` L13, `EventoPuntos.cs` L11, `EventoMovimiento.cs` L11).
- **Código observado**:
  ```csharp
  public delegate void DelegadoStock(string mensaje);
  public delegate void DelegadoVencimiento(string mensaje);
  ```
- **Análisis de cumplimiento**: The delegates exposed by event classes pass simple, focused types (`string mensaje`) to subscribers. Subscribers in `Program.cs` only receive the string message needed for presentation without being forced to deal with complex internal sender state.

---

## 4. Evidencia de Violaciones del Principio ISP (Hallazgos Detallados)

A continuación se detallan los hallazgos de violaciones graves a ISP en el código fuente:

### Hallazgo H-ISP-01: Ausencia Total de Interfaces Segregadas en las Entidades de Dominio
- **Ubicación**: 
  - `BibFarmacia/Clases/Producto.cs` (L8–L35)
  - `BibFarmacia/Clases/Cliente.cs` (L9–L25)
  - `BibFarmacia/Clases/Usuario.cs` (L8–L22)
  - `BibFarmacia/Clases/Movimiento.cs` (L9–L26)
- **Síntoma Observado**:
  La clase base abstracta `Producto` y sus derivadas concentran todas las propiedades posibles del dominio en un solo bloque monolítico concretado sin interfaces roles:
  ```csharp
  public abstract class Producto
  {
      public string Nombre { get; set; }
      public decimal Precio { get; set; }
      public int Stock { get; set; }
      public int StockMinimo { get; set; }
      public DateTime FechaVencimiento { get; set; }
      public virtual void MostrarInformacion() { ... }
  }
  ```
  Ninguna entidad del dominio implementa interfaces de rol como `IVendible`, `IStockable`, `IVencible` o `IIdentificable`.
- **Análisis de Violación ISP**:
  Los clientes del sistema que solo requieren el precio o el nombre de un artículo (por ejemplo, el módulo de facturación, la vista resumida de cliente o el calculador de descuentos) se ven obligados a depender de la entidad concreta `Producto` completa. Esta dependencia fuerza al cliente a conocer propiedades de inventario (`Stock`, `StockMinimo`) y control de vencimientos (`FechaVencimiento`) que no le incumben.
- **Fix Sugerido**:
  Segregar `Producto` en interfaces de grano fino según el rol de negocio:
  ```csharp
  public interface IIdentificable { string Nombre { get; } }
  public interface IVendible : IIdentificable { decimal Precio { get; } }
  public interface IStockable { int Stock { get; set; } int StockMinimo { get; } }
  public interface IPerishable { DateTime FechaVencimiento { get; } }
  ```

---

### Hallazgo H-ISP-02: Clases Monolíticas de Servicio ("Fat Services") sin Interfaces Segregadas por Rol de Cliente
- **Ubicación**: 
  - `BibFarmacia/Servicios/ServicioProducto.cs` (L12–L119)
  - `BibFarmacia/Servicios/ServicioCliente.cs` (L12–L82)
  - `BibFarmacia/Servicios/ServicioUsuario.cs` (L12–L74)
  - `BibFarmacia/Servicios/ServicioMovimiento.cs` (L11–L39)
- **Síntoma Observado**:
  `ServicioProducto` expone 5 responsabilidades operativas distintas en sus métodos públicos sin implementar ninguna interfaz:
  ```csharp
  public class ServicioProducto
  {
      public string AgregarProducto(Producto producto) { ... }
      public List<Producto> ObtenerProductos() { ... }
      public void VerificarStock() { ... }
      public void VerificarVencimiento() { ... }
      public string CargarDesdeArchivo(string ruta) { ... }
  }
  ```
- **Análisis de Violación ISP**:
  Un módulo de presentación o consulta que solo requiere listar los productos para mostrarlos en pantalla (`ObtenerProductos()`) se ve obligado a depender de la clase pesada `ServicioProducto`. Esta clase arrastra métodos de persistencia en disco (`CargarDesdeArchivo`), métodos de evaluación de alertas en segundo plano (`VerificarStock`, `VerificarVencimiento`) y manipulación directa de eventos (`EventoStock`, `EventoVencimiento`). No existen contratos de servicio segregados para lectura, escritura, alertas o almacenamiento.
- **Fix Sugerido**:
  Dividir el servicio en contratos de interfaz específicos por cliente:
  ```csharp
  public interface IProductoConsultaService { List<Producto> ObtenerProductos(); }
  public interface IProductoPersistenciaService { string CargarDesdeArchivo(string ruta); }
  public interface IProductoAlertService { void VerificarStock(); void VerificarVencimiento(); }
  ```

---

### Hallazgo H-ISP-03: Disparadores de Eventos Acoplados a Objetos Concretos Pesados ("Fat Event Arguments")
- **Ubicación**: 
  - `BibFarmacia/Eventos/EventoStockMinimo.cs` (L17–L22)
  - `BibFarmacia/Eventos/EventoVencimiento.cs` (L19–L24)
- **Código observado**:
  ```csharp
  // EventoStockMinimo.cs
  public void Disparar(Producto producto)
  {
      StockMinimo?.Invoke($"ALERTA: stock mínimo de {producto.Nombre}");
  }

  // EventoVencimiento.cs
  public void Disparar(Producto producto)
  {
      Vencimiento?.Invoke($"ALERTA: {producto.Nombre} próximo a vencer");
  }
  ```
- **Análisis de Violación ISP**:
  Los métodos `Disparar` de ambas clases de eventos exigen como argumento una instancia completa de la clase pesada `Producto`, a pesar de que internamente solo leen un único atributo de texto: `producto.Nombre` (L21 y L23).
  Esta firma gorda impide reutilizar el sistema de alertas de stock o expiración para cualquier otro elemento de la farmacia que no pertenezca a la jerarquía de `Producto` (por ejemplo, insumos médicos, bolsas de empaque, o servicios).
- **Fix Sugerido**:
  Segregar la firma del método `Disparar` usando una interfaz angosta como `INombrable` o pasando directamente los datos requeridos:
  ```csharp
  public interface INombrable { string Nombre { get; } }
  public void Disparar(INombrable item) { StockMinimo?.Invoke($"ALERTA: stock mínimo de {item.Nombre}"); }
  ```

---

### Hallazgo H-ISP-04: Dependencia Rígida en `Program.cs` de Servicios Fat Concretos sin Segregación de Roles
- **Ubicación**: `AppFarmaciaConsola/Program.cs` (L8–L18, L78–L87, L145–L367)
- **Código observado**:
  ```csharp
  ServicioProducto servicioProducto = new ServicioProducto();
  ServicioCliente servicioCliente = new ServicioCliente();
  ServicioUsuario servicioUsuario = new ServicioUsuario();
  ServicioMovimiento servicioMovimiento = new ServicioMovimiento();
  ```
- **Análisis de Violación ISP**:
  El punto de entrada de la aplicación (`Program.cs`) depende 100% de las clases concretas monolíticas. No se emplean interfaces segregadas para regular qué parte del servicio consume la interfaz de usuario. En el bucle principal (`switch`), la UI interactúa sin restricciones con métodos de negocio, estado interno y eventos.
  Si el día de mañana se desea crear un rol de usuario "Cajero" que solo debería consultar precios y registrar ventas pero no ejecutar cargas de archivos ni modificar stock directamente, la arquitectura actual lo imposibilita porque la clase `ServicioProducto` expone todo a todos sus clientes sin segregación.
- **Fix Sugerido**:
  Inyectar servicios a `Program.cs` a través de interfaces segregadas de rol (`ICatalogReader`, `IVentaProcessor`, `IAlertChecker`).

---

### Hallazgo H-ISP-05: Inconsistencia y Falta de Interfaces Segregadas de Persistencia / Carga de Archivos
- **Ubicación**: 
  - `BibFarmacia/Servicios/ServicioProducto.cs` (L75: `public string CargarDesdeArchivo(string ruta)`)
  - `BibFarmacia/Servicios/ServicioCliente.cs` (L47: `public string Cargar(string ruta)`)
  - `BibFarmacia/Servicios/ServicioUsuario.cs` (L37: `public string Cargar(string ruta)`)
- **Análisis de Violación ISP**:
  No existe un contrato de interfaz común ni segregado para operaciones de I/O de archivos o repositorios (`ICargable<T>` o `IDataLoader`). Cada clase de servicio inventa su propio nombre de método (`CargarDesdeArchivo` vs `Cargar`).
  Los clientes no pueden tratar polimórficamente la carga de datos del sistema ni acoplarse a un contrato limpio de persistencia, violando el principio de segregación al obligar a los clientes a aprender y acoplarse a la implementación específica de cada servicio.
- **Fix Sugerido**:
  Crear una interfaz genérica y segregada para la carga de datos:
  ```csharp
  public interface IDataLoader<T>
  {
      string Cargar(string ruta);
  }
  ```

---

## 5. Evaluación de Extensibilidad ante Solicitudes de Cambio (SC-1, SC-2, SC-3)

La falta de segregación de interfaces impone severas barreras para implementar los nuevos requerimientos del negocio:

| Solicitud de Cambio | Descripción del Requerimiento | Limitación por Violación de ISP | Impacto Arquitectónico y Riesgo de Ruptura |
|--------------------|-------------------------------|----------------------------------|--------------------------------------------|
| **SC-1** | Venta de cosméticos, comestibles (gaseosas, helados, snacks) | `Producto` obliga a tener `FechaVencimiento` y los eventos de stock piden el objeto `Producto` completo. | Al no existir una interfaz `IVendible` aislada de `IPerishable`, se obliga a los comestibles no perecederos o cosméticos a llevar propiedades y validaciones de vencimiento farmacéutico irrelevantes. |
| **SC-2** | Venta de servicios (inyectología, curaciones, vendajes) | **Impacto Crítico**. Los servicios son vendibles pero **NO** tienen stock ni vencimiento. | Si se intenta derivar `ServicioInyectologia : Producto`, la subclase heredará `Stock` y `FechaVencimiento` que no usa. Si `ServicioProducto` o `Program.cs` invocan `VerificarStock()` o `Stock -= cantidad`, la lógica fallará. La segregación en `IVendible` (sin `IStockable`) es indispensable. |
| **SC-3** | Convenios corporativos (empresas, bancos, universidades) | `Cliente` carece de interfaces segregadas para convenios (`IClienteConvenio`, `ICreditoConvenio`). | Modificar `Cliente` directamente para agregar datos de convenios sobrecarga la clase base para clientes particulares comunes, violando ISP. |

---

## 6. Tabla Resumen Obligatoria de Evaluaciones ISP

A continuación se consolidan los hallazgos y evaluaciones en la tabla con el formato obligatorio especificado:

| Principio | ¿Cumple? | Evidencia (archivo / línea) | Fix sugerido |
|-----------|----------|-----------------------------|--------------|
| **ISP** | **CUMPLE** | `BibFarmacia/Interfaces/IDescuento.cs` (L9–L12)<br>`BibFarmacia/Servicios/ServicioDescuento.cs` (L11–L17) | Mantener `IDescuento` como interfaz de un solo método (`CalcularDescuento`). |
| **ISP** | **CUMPLE** | `BibFarmacia/Interfaces/IServicioNotificacion.cs` (L9–L12)<br>`BibFarmacia/Servicios/ServicioNotificacion.cs` (L10–L16) | Mantener `IServicioNotificacion` como interfaz de un solo método (`EnviarNotificacion`). |
| **ISP** | **CUMPLE** | `BibFarmacia/Eventos/EventoStockMinimo.cs` (L12)<br>`BibFarmacia/Eventos/EventoVencimiento.cs` (L13) | Mantener firmas de delegados angostas (`string mensaje`). |
| **ISP** | **NO CUMPLE** | `BibFarmacia/Clases/Producto.cs` (L8–L35)<br>`Cliente.cs` (L9–L25)<br>`Usuario.cs` (L8–L22) | Extraer interfaces de dominio segregadas: `IVendible`, `IStockable`, `IPerishable`, `IIdentificable`. |
| **ISP** | **NO CUMPLE** | `BibFarmacia/Servicios/ServicioProducto.cs` (L12–L119)<br>`ServicioCliente.cs` (L12–L82) | Segregar servicios monolíticos en interfaces por rol: `IProductoConsultaService`, `IProductoPersistenciaService`, `IStockAlertService`. |
| **ISP** | **NO CUMPLE** | `BibFarmacia/Eventos/EventoStockMinimo.cs` (L17–L22)<br>`EventoVencimiento.cs` (L19–L24) | Modificar el método `Disparar(Producto p)` para recibir una interfaz angosta `INombrable` en vez del objeto pesado `Producto`. |
| **ISP** | **NO CUMPLE** | `AppFarmaciaConsola/Program.cs` (L8–L18, L78–L87) | Refactorizar `Program.cs` para consumir interfaces segregadas (`ICatalogReader`, `IVentaProcessor`) inyectadas por DI. |
| **ISP** | **NO CUMPLE** | `BibFarmacia/Servicios/ServicioProducto.cs` (L75)<br>`ServicioCliente.cs` (L47)<br>`ServicioUsuario.cs` (L37) | Definir un contrato genérico de persistencia `IDataLoader<T>` o `IRepository<T>` para unificar la carga de archivos. |

---

## 7. Propuesta de Rediseño Arquitectónico (Refactorización ISP)

Para resolver las violaciones de ISP en la Fase 2 (TO-BE), se propone la creación de los siguientes contratos de interfaz segregados en `BibFarmacia/Interfaces/`:

### 7.1 Interfaces de Dominio Segregadas
```csharp
namespace BibFarmacia.Interfaces
{
    public interface IIdentificable
    {
        string Nombre { get; }
    }

    public interface IVendible : IIdentificable
    {
        decimal Precio { get; }
    }

    public interface IStockable
    {
        int Stock { get; set; }
        int StockMinimo { get; }
    }

    public interface IPerishable
    {
        DateTime FechaVencimiento { get; }
    }
}
```

### 7.2 Interfaces de Servicios Segregadas por Cliente
```csharp
namespace BibFarmacia.Interfaces
{
    public interface IProductoConsultaService
    {
        List<Producto> ObtenerProductos();
    }

    public interface IProductoPersistenciaService
    {
        string CargarDesdeArchivo(string ruta);
    }

    public interface IStockAlertService
    {
        void VerificarStock();
        void VerificarVencimiento();
    }
}
```

---

## 8. Conclusión del Diagnóstico ISP

El análisis revela un contraste claro en la arquitectura AS-IS: mientras que las dos interfaces explícitas existentes (`IDescuento` y `IServicioNotificacion`) representan ejemplos ideales de segregación de interfaces, **el resto del sistema carece totalmente de interfaces de abstracción**. 

La ausencia de interfaces en el modelo de dominio y en los servicios principales convierte a la solución en un conjunto de componentes fuertemente acoplados a clases monolíticas concretas. Esta rigidez impide agregar nuevos servicios (SC-2) sin romper el comportamiento del sistema. La introducción de interfaces segregadas es un requisito crítico para habilitar la extensibilidad y la mantenibilidad requeridas.
