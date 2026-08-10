# Principios SOLID aplicados y argumentados — Fase 3

## 1. SRP — Single Responsibility Principle

### 1.1. `Program.cs` (monolito AS-IS) → `Program` (composition root) + `MenuConsola` (interacción)

- **Tipo de intervención:** partición real de una sola clase en dos.
- **Evidencia AS-IS:** `Program.cs` construye los cuatro servicios, suscribe los manejadores de los cuatro `Evento*`,
  corre el bucle de login y corre el bucle de menú (7 opciones, incluida la venta) — todo en el mismo archivo. Es la
  causa raíz de **H-13** (Program.cs monolito sin DI) y del **Punto de Dolor #1** priorizado en la Fase 2.
- **Por qué esta frontera y no otra:** se usó el test de "razón de cambio". `Program` cambia únicamente cuando cambia
  **qué se construye o cómo se conecta** (agregar un servicio, cambiar `NotificadorConsola` por otra implementación
  de `INotificador`). `MenuConsola` cambia únicamente cuando cambia **qué opciones ve el usuario o cómo interactúa
  con ellas**. Estas dos razones de cambio ya convivían sin ningún límite en el AS-IS: agregar una opción de menú y
  cambiar cómo se instancia un servicio tocaban el mismo archivo. No se partió en 3 o más piezas (por ejemplo,
  separando el login del resto del menú) porque no hay evidencia de que el login cambie por una razón distinta a la
  del resto de la interacción con el usuario — habría sido partición sin motivo.
- **Qué se gana:** agregar una opción de menú nueva no puede romper el cableado de dependencias (y viceversa).
  Cambiar `NotificadorConsola` por otra implementación de `INotificador` no toca una sola línea de `MenuConsola`.

### 1.2. La venta (bloque de código en línea del AS-IS) → 4 clases con una responsabilidad cada una

- **Tipo de intervención:** partición — pero no de una clase, sino de un **bloque de lógica sin clase propia**. La
  opción 4 del menú (venta) en el AS-IS no era una clase: era código de consola que hacía `producto.Stock -=
  cantidad` y creaba un `Movimiento` en línea, sin `Venta`, `Factura` ni `MedioDePago` en `BibFarmacia` : *"no hay hoy dónde enganchar esa regla, hay que diseñarla desde
  cero".
- **En cuántas piezas queda:** `LineaDeVenta` (subtotal de un ítem con su cantidad), `CestaDeCompra` (agregar/sumar
  líneas), `CasodeUsoProcesarVenta` (orquesta: aplica el descuento del convenio y dispara el evento) y
  `EventoVentaProcesada` (el dato que viaja entre quien vende y quien reacciona a la venta). *(El diagrama ya no
  incluye `AutorizadorDeCredito`: el equipo evaluó modelar autorización de crédito para SC-3 y decidió no hacerlo —
  no había ningún caso de uso que fuera a ejercitar esa capacidad dentro del alcance de esta entrega. `Convenio`
  quedó enfocado solo en la regla de descuento.)*
- **Por qué esta frontera y no otra:** se evitó deliberadamente crear un único `ServicioVenta` que hiciera todo,
  porque **actualizar stock** y **registrar el movimiento** ya eran responsabilidad de `GestorInventario` y
  `MovimientoService` respectivamente (heredada del AS-IS: `ServicioProducto.VerificarStock`,
  `ServicioMovimiento.RegistrarMovimiento`). Meterlas dentro de una clase de venta habría reproducido, con otro
  nombre, el mismo defecto que tenía `Program.cs`: tres razones de cambio (calcular precio, actualizar inventario,
  trazar movimiento) en un solo lugar. En su lugar, `CasodeUsoProcesarVenta` dispara `EventoVentaProcesada` y
  `GestorInventario`/`MovimientoService` reaccionan cada uno con su propio método `AlProcesarVenta(evento)`, sin que
  la venta necesite conocer cómo se actualiza el inventario ni cómo se registra el movimiento.
  El subtotal de una línea (`LineaDeVenta.CalcularSubtotal`) se separó de la suma del carrito
  (`CestaDeCompra.CalcularSubtotal`) porque son cosas que cambian por motivos distintos: SC-3 afecta el descuento
  sobre el **total** del carrito, no el cálculo de una línea individual — la frontera SRP es, a la vez, el punto
  exacto donde se engancha el descuento sin tocar ninguna de las dos clases.
- **Qué se gana:** SC-3 (descuento por convenio) se enganchó dentro de `CasodeUsoProcesarVenta` sin tocar cómo se
  descuenta stock o se registra un movimiento. Un cambio futuro en cómo se registra un movimiento no puede romper el
  cálculo del checkout, y viceversa.

### 1.3. `GestorServiciosMedicos` — creada con SRP desde el diseño (SC-2), no partida de nada existente

- **Tipo de intervención:** clase nueva, sin equivalente en el AS-IS (el sistema actual no vende servicios).
- **Por qué esta frontera y no meterlo en `GestorInventario`:** un `ServicioMedico` no tiene stock — mezclarlo en
  `GestorInventario` habría obligado a esa clase a manejar dos colecciones con reglas distintas (una con control de
  stock, otra sin). La frontera elegida no es "por tipo de clase" sino **por si el catálogo es o no inventariable**,
  que es justo la tensión que A.5.2 anticipa para SC-2.
- **Qué se gana:** `GestorInventario` conserva una sola razón de cambio (cómo se controla el inventario de lo que sí
  tiene stock); agregar un tercer catálogo en el futuro se decide por esa misma pregunta, no por tocar servicios
  existentes.

### 1.4. `Cliente.AcumularPuntos` como único punto de mutación de `-puntos` — consolidación, no partición

- **Tipo de intervención:** ninguna clase se parte; se **elimina una duplicación de responsabilidad** ya confirmada
  como hallazgo real.
- **Evidencia AS-IS:** había **dos** implementaciones independientes de "sumar puntos": `Cliente.AcumularPuntos`
  (código muerto, nunca invocado) y `ServicioCliente.AcumularPuntos`, que hacía `cliente.Puntos += puntos;`
  directamente sobre un campo público, sin pasar por el método del dominio (`ServicioCliente.cs:40` vs.
  `Cliente.cs:20-23`, citado en `CONTEXTO-RETO-FARMACIA.md` B.2 y B.6). Dos implementaciones de la misma regla es el
  síntoma de que nadie era responsable único de "cómo cambian los puntos de un cliente".
- **Por qué esta frontera:** `-puntos` se vuelve privado; `ClienteService.AcumularPuntos` delega en
  `cliente.AcumularPuntos(puntos)` en vez de reimplementar la regla. El dueño de la responsabilidad es la propia
  entidad, no el servicio que la orquesta.
- **Qué se gana:** si mañana cambia la regla de acumulación (p. ej. puntos dobles según convenio), se cambia en un
  solo lugar — ya no puede existir una segunda implementación divergente y muerta como la que ya existía.

---

## 2. OCP — Open/Closed Principle

### 2.1. `Convenio` (abstracta) + `SinConvenio` / `ConvenioUniversidad` / `ConvenioEmpresa` — SC-3

- **Tipo de intervención:** jerarquía nueva, reemplaza a un método único (no una clase) que tenía la regla vieja.
- **Evidencia AS-IS:** `ServicioDescuento.CalcularDescuento` tenía un **10 % fijo hardcodeado**, sin invocarse en
  ningún punto de `Program.cs` — el descuento no era observable en el flujo actual (confirmado, `CONTEXTO` B.2/B.6).
  Con esa arquitectura, agregar un convenio nuevo habría significado agregar un `if`/`switch` dentro del único
  método de descuento: **modificar código existente** por cada entidad nueva.
- **Por qué esta frontera (una clase de dominio `Convenio` por entidad, no un único método parametrizado):** la
  alternativa más simple habría sido generalizar `ServicioDescuento.CalcularDescuento` para que reciba el porcentaje
  como parámetro — pero eso no resuelve que cada tipo de entidad (empresa, banco, universidad...) pueda necesitar, a
  futuro, una regla de cálculo distinta a un simple porcentaje fijo (p. ej. un descuento escalonado por monto de
  compra). Modelar cada entidad como una subclase de `Convenio`, en vez de una fórmula parametrizada, deja esa
  puerta abierta sin tener que rediseñar el mecanismo otra vez. 
- **Qué se gana:** agregar "convenio con cooperativa" (que el enunciado ya anticipa) es agregar **una clase nueva**
  (`ConvenioCooperativa : Convenio`) sin tocar `Cliente`, `CestaDeCompra`, `CasodeUsoProcesarVenta` ni ninguna otra
  clase existente. Esta es la extensión que la Fase 4 (A.7.4) va a medir con la tabla de clases creadas vs.
  modificadas.

### 2.2. `ArticuloRetail` como nueva subclase de `ProductoBase` — SC-1

- **Tipo de intervención:** clase nueva agregada a una jerarquía existente, sin modificarla.
- **Evidencia AS-IS (línea base ya medida en `Soluciones de cambio.docx`):** agregar hoy un tipo de producto obliga
  como mínimo a tocar `ServicioProducto.CargarDesdeArchivo` (que construye **siempre** un `MedicamentoCapsula`
  hardcodeado, sin despacho por tipo — **H-12**) y `ProductoFactory` (agregar un método `Crear*` nuevo). Es
  modificación de código compartido por todos los productos existentes.
- **Por qué esta frontera:** `ArticuloRetail` extiende `ProductoBase` al mismo nivel que `Medicamento`, sin tocar
  `Medicamento`/`MedicamentoCapsula`/`MedicamentoLiquido` ni el contrato de `ProductoBase`. `GestorInventario` opera
  sobre `ProductoBase`/`IControlableEnInventario`, sin necesitar saber si el objeto es un medicamento o un artículo
  de retail.
- **Qué se gana:** vender cosméticos o snacks es agregar una clase, no despachar por tipo dentro de un método
  compartido — elimina exactamente el patrón que produjo H-12.

---

## 3. LSP — Liskov Substitution Principle

### 3.1. `Medicamento`: de concreta a abstracta

- **Tipo de intervención:** cambio de modificador sobre una clase existente (no split).
- **Evidencia AS-IS:** `Medicamento` es una clase concreta e instanciable (`CONTEXTO` B.1: "¿Abstracta? No"). Eso es
  un riesgo LSP latente: un `Medicamento` "puro" (sin presentación cápsula ni líquida) no cumple el invariante
  implícito del dominio — todo medicamento real tiene una presentación — y no sería sustituible de forma
  significativa donde el resto del sistema espera un medicamento completo.
- **Verificación base (la formal, punto por punto, queda para el entregable de A.6 punto 3):** las dos subclases
  (`MedicamentoCapsula`, `MedicamentoLiquido`) solo **agregan** un atributo propio y sobreescriben
  `MostrarInformacion()` **sumando** información, sin fortalecer precondiciones ni debilitar lo que
  `EstaProximoAVencer()`/`DeducirStock()` prometen. Marcar `Medicamento` como `abstract` no cambia ese contrato: solo
  impide instanciar la pieza "media" que no tiene sentido de negocio por sí sola.
- **Qué se gana:** ya no es posible crear por accidente un `Medicamento` sin presentación — el compilador lo impide,
  cerrando el riesgo antes de que SC-1/SC-2 agreguen más tipos al árbol.

### 3.2. `ArticuloRetail` hereda de `ProductoBase`, no de `Medicamento`

- **Tipo de intervención:** decisión de diseño sobre dónde ubicar una clase nueva (ligada a 2.2).
- **Por qué esta frontera:** un `ArticuloRetail` (p. ej. una gaseosa) no tiene laboratorio. Heredarlo de
  `Medicamento` para "reusar" `fechaVencimiento` habría arrastrado también `Laboratorio`, un campo sin sentido de
  negocio para un snack — una violación LSP silenciosa (forzar al consumidor del contrato a lidiar con un campo que
  no aplica, o a inventar un laboratorio ficticio). En vez de eso, `IPerecedero` es una interfaz aparte que
  `Medicamento` y `ArticuloRetail` implementan cada uno de forma independiente.
- **Qué se gana:** cualquiera de los dos es sustituible donde se espera un `IPerecedero` (p. ej.
  `GestorInventario.VerificarVencimiento`), sin arrastrar atributos que no le pertenecen. Se evita, antes de que
  ocurra, la violación LSP que SC-1 habría introducido si se hubiera forzado el nuevo tipo dentro del árbol de
  `Medicamento`.

### 3.3. `ServicioMedico` NO hereda de `ProductoBase` — SC-2

- **Tipo de intervención:** decisión negativa de herencia (se decide explícitamente no heredar).
- **Por qué:** responde directamente a la tensión que A.5.2 anticipa para SC-2: *"¿es un servicio sustituible donde
  se espera un `Producto` con stock?"* No. Un servicio no tiene stock ni `stockMinimo`; forzarlo a heredar de
  `ProductoBase` habría significado que `DeducirStock()`/`EstaEnStockMinimo()` fallaran o se comportaran de forma
  sorprendente sobre un `ServicioMedico` — exactamente lo que LSP prohíbe (el cliente del contrato ya no puede
  confiar en lo que la superclase promete).
- **Qué se gana:** `ServicioMedico` solo implementa `IVendible` (lo único que realmente comparte con los productos).
  No se pretende que sea sustituible por un `ProductoBase`, y el diseño no lo obliga a fingirlo.

---

## 4. ISP — Interface Segregation Principle

### 4.1. `Producto` (AS-IS, contrato único e implícito) → `IVendible` + `IControlableEnInventario` + `IPerecedero`

- **Tipo de intervención:** una clase abstracta con un contrato ancho se reemplaza por tres interfaces segregadas.
- **Evidencia AS-IS:** `Producto` obligaba a **toda** subclase a cargar con `stock`, `stockMinimo` y
  `FechaVencimiento` sin excepción (`CONTEXTO` B.2: los cuatro campos se declaran de una vez en `Producto`). Esa es
  la interfaz implícita ancha que SC-2 rompe: un servicio médico es vendible pero no es inventariable ni perecedero,
  y con el contrato AS-IS no hay forma de modelarlo sin cargar atributos que no le corresponden.
- **Por qué esta frontera (tres interfaces, no una ni cinco):** se segregó por **quién necesita qué operación**, no
  por clase:
  - `IVendible` — lo mínimo que cualquier cosa facturable necesita (código, nombre, precio, mostrar información).
    Lo implementan `ProductoBase` **y** `ServicioMedico`.
  - `IControlableEnInventario` — solo quien tiene stock. La implementa `ProductoBase`; `ServicioMedico`
    deliberadamente **no** la implementa.
  - `IPerecedero` — solo quien vence. La implementan `Medicamento` y `ArticuloRetail`; `ServicioMedico` **no** la
    implementa (una curación no vence).
  Se descartó una única interfaz `IProducto` con todo, porque habría reproducido el mismo problema del AS-IS con
  otro nombre. Se descartó partir aún más fino (p. ej. separar `precio` de `nombre` en interfaces distintas) porque
  no hay ningún caso de uso real, ni en el AS-IS ni en las tres SC, donde algo tenga precio pero no nombre — habría
  sido sobre-ingeniería sin consumidor que la necesite.
- **Qué se gana:** `GestorServiciosMedicos` administra `ServicioMedico` sin heredar ni implementar código de control
  de stock que nunca va a usar. `GestorInventario.VerificarVencimiento()` puede iterar solo sobre `IPerecedero` sin
  importarle si es un `Medicamento` o un `ArticuloRetail`. Resuelve la tensión que A.5.2 anticipa explícitamente para
  SC-2 ("presiona ISP").

---

## 5. DIP — Dependency Inversion Principle

### 5.1. `INotificador` entre los servicios y la salida por consola (Punto de Dolor #3)

- **Tipo de intervención:** se introduce una abstracción nueva entre módulos que antes se acoplaban directo a una
  implementación concreta.
- **Evidencia AS-IS:** cada servicio (`ServicioCliente`, `ServicioMovimiento`, `ServicioProducto`) crea con `new` su
  propio objeto `Evento*` concreto dentro de un constructor sin parámetros — dependencia directa a una
  implementación concreta de infraestructura, sin ninguna abstracción de por medio (*"no hay
  inyección de dependencias en la capa de servicios hoy"*). Ese acoplamiento es, exactamente, el **Punto de Dolor
  #3** priorizado en la Fase 2.
- **Alto nivel / bajo nivel (identificación completa de las 4 piezas):** el alto nivel es la
  política de negocio — *avisar* cuando el stock esté bajo, algo vaya a vencer, se sumen puntos o se registre un
  movimiento. El bajo nivel es **cómo** se avisa (hoy: consola con colores). `INotificador.Notificar(mensaje, tipo)`
  es la abstracción que los desacopla; `GestorInventario`, `ClienteService` y `MovimientoService` dependen de ella,
  no de `NotificadorConsola`. `Program` (composition root) es quien construye `NotificadorConsola` y la inyecta.
- **Por qué esta frontera:** se evaluó dejar el acoplamiento a `Evento*` concretos "porque la salida es
  siempre en consola" — se descartó porque es exactamente el defecto ya documentado como Punto de Dolor #3, y porque
  A.11 anticipa como familia de cambio genérica *"nuevo canal de notificación... presiona DIP e ISP"*, algo que el
  negocio ya insinuó que puede pedir.
- **Qué se gana:** cambiar el canal de notificación (p. ej. a un archivo de log) es agregar una clase que implemente
  `INotificador` y cambiar una línea en el composition root — cero cambios en `GestorInventario`, `ClienteService` o
  `MovimientoService`.

---

## Resumen — trazabilidad principio → hallazgo/punto de dolor que resuelve

| Principio | Intervención principal | Hallazgo / Punto de dolor que ataca |
|---|---|---|
| SRP | `Program` / `MenuConsola`; venta partida en 4 piezas; consolidación de `AcumularPuntos` | H-13, Punto de Dolor #1; ausencia de clase de venta (B.5); duplicación de regla de puntos |
| OCP | `Convenio` + subtipos; `ArticuloRetail` | Descuento fijo sin uso real (SC-3); H-12 (hardcoding de tipo en carga, SC-1) |
| LSP | `Medicamento` abstracta; `ArticuloRetail`/`ServicioMedico` fuera del árbol de `Medicamento` | Riesgo de instanciación de tipo "medio"; tensión LSP de SC-1/SC-2 (A.5.2) |
| ISP | `IVendible` / `IControlableEnInventario` / `IPerecedero` | Contrato ancho de `Producto`; tensión ISP de SC-2 (A.5.2) |
| DIP | `INotificador` + composition root en `Program` | H-04/B.4 (sin inyección de dependencias); Punto de Dolor #3 |