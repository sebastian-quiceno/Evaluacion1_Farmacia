# ADR — Registros de Decisión Arquitectónica — Fase 3 (A.6, punto 5)

---

## ADR-001 — Segregar el contrato de producto en `IVendible` / `IControlableEnInventario` / `IPerecedero`

- **Estado:** Aceptada.
- **Contexto y evidencia:** **H-06** (`ServicioProducto.cs:14` — el servicio depende directamente de `Producto` en
  bloque, sin ninguna interfaz que exprese qué necesita realmente cada consumidor). Se agrava con SC-2 (A.5.2): un
  servicio médico es vendible pero no es inventariable ni perecedero, y el contrato único de `Producto` no permite
  modelar eso sin forzar atributos que no aplican.
- **Alternativas evaluadas:**
  1. Una única interfaz ancha `IProducto` con todo (código, nombre, precio, stock, vencimiento), donde
     `ServicioMedico` implementaría métodos de stock que no usa (o lanzaría `NotImplementedException`).
  2. Tres interfaces segregadas por necesidad real de cada consumidor: `IVendible`, `IControlableEnInventario`,
     `IPerecedero`.
- **Decisión:** Opción 2.
- **Costo/consecuencia aceptada:** Más tipos declarados en el diagrama (3 interfaces en vez de 1) — cuesta más
  leer la arquitectura de un vistazo, a cambio de que ninguna clase implemente algo que no usa.
- **Principios involucrados:** ISP (principal), LSP (secundario — evita forzar una sustituibilidad falsa que SC-2
  habría introducido).

---

## ADR-002 — `Medicamento` pasa de concreta a abstracta

- **Estado:** Aceptada.
- **Contexto y evidencia:** **H-05** (`Medicamento.cs:9` — *"la clase parece utilizarse como una clase base o
  plantilla, pero puede instanciarse directamente"*, severidad Media/Asistida). El propio hallazgo sugiere evaluar
  si debería declararse abstracta.
- **Alternativas evaluadas:**
  1. Disciplina de equipo: dejar `Medicamento` concreta y documentar como convención "no instanciar directamente"
     (regla social, no verificable por el compilador).
  2. Marcar `Medicamento` como `abstract`, forzando la regla en tiempo de compilación.
- **Decisión:** Opción 2.
- **Costo/consecuencia aceptada:** Cualquier necesidad futura de un "medicamento genérico sin presentación" (poco
  probable, pero posible en un reporte agregado) exige introducir un tipo adicional en vez de instanciar
  `Medicamento` directamente.
- **Principios involucrados:** LSP.

---

## ADR-003 — Eliminar `ProductoFactory`; construcción parametrizada dirigida por los datos del archivo

- **Estado:** Aceptada.
- **Contexto y evidencia:** **H-02** (`ProductoFactory.cs:19,34` — `StockMinimo`, `FechaVencimiento`, `TipoRelleno`
  y `MaterialEnvase` quedan hardcodeados dentro de los métodos de la fábrica) y **H-12**
  (`ServicioProducto.cs:93-107` — `CargarDesdeArchivo` siempre construye un `MedicamentoCapsula` con
  `TipoRelleno.Gel` fijo, sin despacho real por tipo, y además hardcodea dirección/teléfono del laboratorio).
- **Alternativas evaluadas:**
  1. Conservar `ProductoFactory` tal cual, agregarle parámetros para los valores hoy fijos, y corregir el despacho
     por tipo dentro de `CargarDesdeArchivo` — parche mínimo sobre la estructura existente.
  2. Eliminar `ProductoFactory`: cada constructor (`MedicamentoCapsula`, `MedicamentoLiquido`, `ArticuloRetail`)
     recibe explícitamente todos sus parámetros, y `GestorInventario.CargarDesdeArchivo` hace el despacho real por
     tipo leyendo el dato de cada fila.
- **Decisión:** Opción 2 — SC-1 y SC-2 iban a obligar a `ProductoFactory` a crecer un método `Crear*` por cada tipo
  nuevo, lo cual es, en sí mismo, una violación de OCP dentro de la propia fábrica.
- **Costo/consecuencia aceptada:** `GestorInventario.CargarDesdeArchivo` concentra el conocimiento de todos los
  subtipos de producto — es, deliberadamente, el único método al que se le permite conocerlos a todos (mismo patrón
  que `ClienteService.Cargar` con `Convenio`, documentado en `Inversion de Dependencias (DIP).md`, Caso 2).
- **Principios involucrados:** OCP (principal), SRP (la fábrica deja de decidir valores de negocio que no le
  correspondían).

---

## ADR-004 — `ProductoBase` protege su invariante de stock frente a código externo a la jerarquía (encapsulamiento)

- **Estado:** Aceptada (revisada — visibilidad ajustada de privada a protegida).
- **Contexto y evidencia:** **H-07** (`Producto.cs:10-14` — *"las entidades de dominio no realizan validaciones
  sobre su propio estado ni garantizan el cumplimiento de sus reglas de negocio"*). Se confirma en `Program.cs`
  (opción 4 del menú, venta): `producto.Stock -= cantidad` se ejecuta directamente sobre el campo, sin ninguna
  garantía de que el resultado no quede negativo.
- **Alternativas evaluadas:**
  1. Mantener `stock`/`stockMinimo` como campos públicos, accesibles desde cualquier código (AS-IS).
  2. Hacerlos **privados** (`-stock`/`-stockMinimo`) en `ProductoBase`, de forma que ni siquiera las subclases
     (`Medicamento`, `ArticuloRetail`) puedan tocarlos directamente — solo los métodos de `ProductoBase`.
  3. Hacerlos **protegidos** (`#stock`/`#stockMinimo`), visibles para `ProductoBase` y sus subclases, pero **no**
     para código externo a la jerarquía (`GestorInventario`, `MenuConsola`, etc.), que sigue obligado a pasar por
     `DeducirStock(cantidad)`, `TieneStockSuficiente(cantidad)`, `EstaEnStockMinimo()`.
- **Decisión:** Opción 3 — se descartó la opción 2 porque las subclases de producto son parte del mismo dominio y
  pueden necesitar implementar o participar en reglas propias de stock (p. ej. una futura subclase con una regla de
  reposición distinta); negarles el acceso directo habría obligado a exponer esos mismos datos igual, solo que por
  otro camino (getters protegidos), sin ninguna ganancia real.
- **Costo/consecuencia aceptada:** La protección deja de ser absoluta — una subclase mal escrita **sí podría**, en
  teoría, mutar `stock` sin pasar por `DeducirStock` (el compilador no lo impide entre una clase y su propia
  jerarquía, solo la disciplina de quien escriba la subclase). Es una garantía más débil que la privada estricta,
  pero sigue cerrando el problema que motivó H-07: ningún código **externo** a la jerarquía de producto (como
  `Program.cs` hacía en el AS-IS) puede volver a tocar el stock directamente.
- **Principios involucrados:** SRP (la jerarquía es dueña de sus propias reglas de consistencia frente al resto del
  sistema).

---

## ADR-005 — `INotificador` entre los servicios y la salida por consola

- **Estado:** Aceptada.
- **Contexto y evidencia:** **H-06** (`ServicioProducto.cs:16-17` — dependencia directa de `EventoStockMinimo` y
  `EventoVencimiento` concretos, instanciados con `new` dentro del propio servicio) y **H-11** (refutación ya
  registrada en Fase 1: `IServicioNotificacion` **ya existía** en el AS-IS, pero *"solamente [se crea e implementa]
  cada una en un servicio... pero en ningún punto desacoplan clases entre sí"* — es decir, simulaba aplicar DIP sin
  hacerlo realmente).
- **Alternativas evaluadas:**
  1. Reutilizar `IServicioNotificacion` (ya existente en el código AS-IS).
  2. Diseñar una abstracción nueva, `INotificador`, con inyección real desde un composition root.
- **Decisión:** Opción 2 — se descartó la opción 1 explícitamente por la refutación ya registrada en H-11:
  arrastrar `IServicioNotificacion` al TO-BE habría heredado el mismo defecto (interfaz decorativa, sin ningún
  punto real de desacople) con un nombre distinto.
- **Costo/consecuencia aceptada:** Una capa de indirección adicional (interfaz + clase concreta + inyección por
  constructor) para una funcionalidad que, hoy, sigue siendo únicamente "imprimir en consola con colores".
- **Principios involucrados:** DIP.

---

## ADR-006 — `Convenio` como jerarquía de dominio (no como fórmula parametrizable ni como estrategia inyectada)

- **Estado:** Aceptada.
- **Contexto y evidencia:** **H-08** (`ServicioDescuento.cs:15` — *"el método `CalcularDescuento`... está utilizando
  un porcentaje quemado"*, sin invocarse en ningún punto de `Program.cs`).
- **Alternativas evaluadas:**
  1. Generalizar `CalcularDescuento` para que reciba un porcentaje configurable como parámetro — arreglo mínimo,
     sigue siendo un único método que no distingue reglas de cálculo distintas por tipo de entidad.
  2. Una jerarquía de estrategias inyectada en `Cliente` (`EstrategiaDescuento`), separada de la entidad convenio.
  3. Una única clase de dominio abstracta `Convenio` (con `CalcularDescuento`), con una subclase concreta por
     entidad (`SinConvenio`, `ConvenioUniversidad`, `ConvenioEmpresa`, ampliable).
- **Decisión:** Opción 3 — se descartó la opción 1 porque un porcentaje parametrizado no permite que cada tipo de
  entidad tenga, a futuro, una regla de cálculo distinta a un simple porcentaje fijo; se descartó la opción 2
  porque una jerarquía de estrategias inyectada por separado es, funcionalmente, el patrón Strategy con otro
  nombre — prohibido explícitamente por el profesor. *(El diseño evaluó originalmente incluir también una regla de
  crédito — `AutorizarCredito` — en la misma clase `Convenio`, con una clase `AutorizadorDeCredito` consumiéndola.
  Se decidió no modelarla en esta entrega: ninguna SC exige, dentro del alcance actual, un caso de uso que la
  ejercite, y mantenerla habría sido capacidad especulativa sin consumidor real.)*
- **Costo/consecuencia aceptada:** No se puede combinar el descuento de dos convenios distintos para un mismo
  cliente (p. ej. "descuento de universidad y descuento adicional de empresa" a la vez) — el modelo asume un
  convenio por cliente, tal como lo describe el enunciado, pero sería una limitación real si el negocio pidiera
  combinarlos.
- **Principios involucrados:** OCP (principal — agregar convenio es agregar clase), SRP (la regla deja de vivir en
  un método compartido), DIP (el núcleo de venta consume `Convenio` como abstracción sin conocer los subtipos —
  detallado en `Inversion de Dependencias (DIP).md`, Caso 2).

---

## ADR-007 — Límite consciente: NO introducir una abstracción de repositorio para la persistencia en archivo

- **Estado:** Aceptada (decisión de **no** intervenir).
- **Contexto y evidencia:** **H-03** (`ServicioCliente.cs:25,31,47`) y **H-04** (`ServicioProducto.cs:27,42,75`) —
  ambos con el mismo síntoma: *"el servicio está combinando responsabilidades de lógica de aplicación con acceso a
  datos (persistencia)"*, con la misma recomendación implícita de la herramienta: delegar la persistencia a
  repositorios.
- **Alternativas evaluadas:**
  1. Introducir `IRepositorio<T>` (o equivalente) entre cada servicio y el archivo, desacoplando lectura/escritura
     de la lógica de aplicación.
  2. Dejar la persistencia embebida en cada servicio, tal como está en el AS-IS.
- **Decisión:** Opción 2 — **se rechaza explícitamente la sugerencia de la herramienta** (aunque técnicamente
  correcta en abstracto) por tres razones concretas: (1) el enunciado del reto **prohíbe bases de datos**, así que
  no existe, dentro del alcance de este proyecto, una segunda implementación real que alguna vez vaya a sustituir
  al archivo plano — a diferencia de `INotificador` o de `Convenio` (donde sí hay o va a haber más de una
  implementación concreta); (2) el profesor prohibió el uso de patrones con nombre, y `IRepositorio<T>` es,
  literalmente, el patrón Repository; (3) una interfaz con un solo implementador permanente es indirección sin
  beneficio — exactamente el tipo de sobre-ingeniería que el criterio 6 de la rúbrica penaliza.
- **Costo/consecuencia aceptada:** H-03 y H-04 **quedan sin resolver**, declarados como deuda técnica consciente.
  Las pruebas de caracterización de la Fase 4 van a necesitar tocar archivos reales (o recibir la ruta como
  parámetro) para poder ejercitar estos servicios, en vez de poder aislar la lógica de negocio con un doble de
  prueba.
- **Principios involucrados:** SRP y DIP — deliberadamente **no** aplicados más allá de lo que ya hay.

---

## Hallazgos del inventario que quedan fuera de estos 7 ADR, y por qué

- **H-01** (`Producto.cs:29` — `MostrarInformacion()` mezcla lógica de presentación con lógica de negocio dentro de
  la misma clase, SRP): sigue presente en el TO-BE (`MostrarInformacion()` continúa viviendo en `ProductoBase`/
  `Medicamento`/`ArticuloRetail`/`ServicioMedico`). No se le dedica un ADR propio porque **ninguna de las 3 SC lo
  toca**, y extraer un `Formateador`/`Presentador` sin tener a la vista el texto exacto que imprime hoy
  `Producto.MostrarInformacion()` arriesgaría cambiar el formato observable (A.1.1) sin poder verificarlo — se deja
  como candidato a resolverse en Fase 4, con el código fuente real en mano, no como decisión de Fase 3.
- **H-09** (`ServicioUsuario.cs:29-33` — autenticación resuelta contra una implementación fija, DIP): no se
  introduce una abstracción `IAutenticador` inyectable, por el mismo argumento que ADR-007 (YAGNI): ninguna de las 3
  SC pide un segundo mecanismo de autenticación, y especular con uno habría sido exactamente la sobre-ingeniería que
  el criterio 6 penaliza. Queda como deuda técnica consciente, igual que H-03/H-04.
- **H-10** (`TipoRelleno.cs:7-14` — el enum limita el comportamiento de cómo se rellena cada tipo, OCP): tampoco lo
  toca ninguna SC (SC-1 agrega categorías de producto nuevas, no nuevas formas de rellenar una cápsula existente);
  convertirlo en una jerarquía polimórfica sería una extensión especulativa sin caso de uso real que la exija.
- **H-11**: ya está resuelto — es la refutación de Fase 1, y se reutiliza como evidencia directa dentro de
  ADR-005 (por qué no se reutilizó `IServicioNotificacion`).

---
