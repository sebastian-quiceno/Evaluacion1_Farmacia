# Inversión de dependencias — Fase 3 

## Inventario: cuántas inversiones reales hay y de qué tipo

Se identificaron **dos inversiones de dependencia nuevas** (ambas ligadas a Puntos de Dolor / SC), y **una heredada
del AS-IS que se conserva sin cambios**. Las tres se documentan abajo con sus 4 piezas. Después se documenta,
explícitamente, **qué NO se invirtió a propósito** — es el límite consciente del rediseño que exige Parte C,
instrucción 9.

---

## Caso 1 — Notificación de eventos de negocio (`INotificador`)

Resuelve el **Punto de Dolor #3**, priorizado en la Fase 2.

| Pieza | Elemento |
|---|---|
| **Módulo de alto nivel** | `GestorInventario`, `ClienteService`, `MovimientoService` — la política de negocio: *avisar* cuando el stock esté bajo, algo vaya a vencer, se acumulen puntos o se registre un movimiento. A ellos no les importa **cómo** se entrega el aviso. |
| **Módulo de bajo nivel** | `NotificadorConsola` — el detalle técnico de **cómo** se avisa hoy: imprimir en la consola con colores. |
| **Abstracción que los desacopla** | `INotificador.Notificar(mensaje: string, tipo: TipoNotificacion): void`. |
| **Composition root** | `Program.ConstruirDependencias()` — construye **una** instancia de `NotificadorConsola` y la inyecta en los tres servicios (relaciones `GestorInventario "1" --> "1" INotificador`, `ClienteService "1" --> "1" INotificador`, `MovimientoService "1" --> "1" INotificador` en el diagrama). Es el **único** lugar del programa que menciona `NotificadorConsola` por su nombre concreto. |

**Evidencia AS-IS de por qué esto era una inversión pendiente:** cada servicio creaba con `new` su propio objeto
`Evento*` concreto dentro de un constructor sin parámetros — `CONTEXTO-RETO-FARMACIA.md` B.4 lo confirma
explícitamente: *"no hay inyección de dependencias en la capa de servicios hoy"*. No había ninguna abstracción de
por medio entre la política ("avisar") y el detalle ("consola").

**Alternativa evaluada y descartada:** un método de la abstracción por cada tipo de evento (`NotificarStockMinimo`,
`NotificarVencimiento`, `NotificarPuntos`, `NotificarMovimiento`), replicando uno a uno los cuatro `Evento*` del
AS-IS. Se descartó porque no habría desacoplado nada — habría sido la misma interfaz ancha con otro nombre (cuatro
métodos concretos en vez de cuatro clases concretas), y cada canal nuevo de notificación (A.11: *"nuevo canal de
notificación... presiona DIP e ISP"*) habría tenido que implementar los cuatro igual. Un único
`Notificar(mensaje, tipo)` con un enum discriminador logra el mismo resultado con una interfaz mínima.

**Qué se gana:** cambiar el canal de notificación (a un archivo de log, por ejemplo) es agregar una clase que
implemente `INotificador` y cambiar una línea en `Program.ConstruirDependencias()` — cero cambios en
`GestorInventario`, `ClienteService` o `MovimientoService`.

---

## Caso 2 — Reglas de descuento por convenio (`Convenio`)

| Pieza | Elemento |
|---|---|
| **Módulo de alto nivel** | `CasodeUsoProcesarVenta` — aplica el descuento sobre el subtotal del carrito según la política de negocio: *"cóbrese la venta según lo que le corresponda al cliente"*, sin que le importe **a qué entidad concreta** está afiliado. |
| **Módulo de bajo nivel** | `SinConvenio`, `ConvenioUniversidad`, `ConvenioEmpresa` — el detalle de **cada** relación comercial concreta. |
| **Abstracción que los desacopla** | `Convenio` (`CalcularDescuento(subtotal): decimal`). |
| **Punto de resolución** | **No es el composition root de `Program`.** Es `ClienteService.Cargar(ruta)`: al leer cada fila de `clientes.txt`, decide **qué subtipo concreto de `Convenio`** instanciar para ese cliente (según el dato de la fila) y lo deja fijado en `Cliente.convenio` desde la construcción. `CasodeUsoProcesarVenta` y `AutorizadorDeCredito` **jamás** mencionan un subtipo concreto — consumen `Convenio` únicamente a través de la abstracción. |

**Por qué el punto de resolución no es `Program` :** en el caso de
`INotificador` hay **una sola** implementación activa a la vez, elegida una vez al arrancar el programa — encaja en
el composition root clásico. En el caso de `Convenio`, la implementación concreta **varía por registro de datos**
(cada cliente tiene el suyo, y hay varios a la vez conviviendo en memoria) — no tiene sentido "elegir un
`Convenio`" en `Main()`. La resolución de qué implementación concreta usar se mueve, correctamente, al único lugar
que procesa esos datos: la carga de clientes. Esto es exactamente el mismo mecanismo que ya usaba
`ProductoFactory`/`ServicioProducto.CargarDesdeArchivo` en el AS-IS para decidir, por fila, qué subtipo de producto
construir (aunque en el AS-IS lo hacía mal — **H-12**, siempre construía `MedicamentoCapsula` sin importar el dato).
No se introduce ninguna clase "fábrica" nueva para esto (el profe prohibió patrones con nombre): la decisión
vive como lógica interna de `ClienteService.Cargar`, igual que la decisión de tipo ya vivía dentro de
`ServicioProducto.CargarDesdeArchivo` en el AS-IS.

**Invariante de compatibilidad hacia atrás (ligado a A.1.1, ya documentado en
`Herencias y Verificacion LSP.md`):** todo cliente que exista hoy en `clientes.txt` (sin columna de convenio) debe
cargarse con `convenio = SinConvenio`, cuyo contrato obliga a devolver el subtotal sin modificar — así el
comportamiento observable de los clientes actuales no cambia con SC-3.

**Alternativa evaluada y descartada:** una jerarquía separada de "estrategias de descuento" (`EstrategiaDescuento`)
inyectada en `Cliente`, en vez de una clase de dominio `Convenio`. Ya documentado en
`Principios SOLID Argumentados.md` (sección OCP): se descartó porque una jerarquía de estrategias inyectadas es,
funcionalmente, el patrón Strategy con otro nombre — prohibido explícitamente por el profesor.

**Qué se gana:** agregar "convenio con cooperativa" es agregar una clase nueva y enseñarle a `ClienteService.Cargar`
a reconocer el dato correspondiente — `CasodeUsoProcesarVenta` no se toca.

---

## Caso 3 (heredado, conservado) — Catálogo de productos (`ProductoBase`)

No es una intervención nueva de la Fase 3: **ya era una inversión de dependencia real en el AS-IS**, y se conserva.
Se documenta aquí porque el punto 4 de A.6 pide identificar **toda** inversión del diagrama, no solo las nuevas.

| Pieza | Elemento |
|---|---|
| **Módulo de alto nivel** | `GestorInventario` — la política de "administrar lo que hay para vender con control de stock", sin que le importe si un ítem es un medicamento o un artículo de retail. |
| **Módulo de bajo nivel** | `Medicamento`, `MedicamentoCapsula`, `MedicamentoLiquido`, `ArticuloRetail`. |
| **Abstracción que los desacopla** | `ProductoBase` (clase abstracta, no interfaz — igual que en el AS-IS). |
| **Punto de resolución** | `GestorInventario.CargarDesdeArchivo(ruta)` — mismo mecanismo que el Caso 2: decide, por fila de `productos.txt`, qué subtipo concreto construir. |

**Evidencia de que ya existía en el AS-IS:** En el diagrama AS-IS lo dice explícitamente: *"la única
dependencia que apunta a una abstracción real es la que `ServicioProducto` y `Movimiento` tienen sobre `Producto`
(clase abstracta, no interfaz)"*. Lo único que cambia en el TO-BE es que ahora `ArticuloRetail` se suma como un
segundo tipo de bajo nivel bajo la misma abstracción (consecuencia de OCP, ya documentado), no que se haya invertido
algo que antes no lo estaba.

---

## Qué NO se invirtió — límite consciente del rediseño

- **La persistencia en archivo plano (`Cargar`/`CargarDesdeArchivo`) se deja embebida dentro de cada servicio, sin
  una abstracción `IRepositorio<T>` de por medio.** Se evaluó introducirla — es la recomendación "de manual" para
  desacoplar persistencia de lógica de negocio (H-04/hallazgo ya documentado: *"la persistencia... está incrustada
  directamente... no existe una abstracción de repositorio"*). Se descartó por tres razones concretas: (1) el
  enunciado del reto y el profe no querían que usáramos **bases de datos**, así que no existe, dentro del alcance de este
  proyecto, una segunda implementación real que alguna vez vaya a sustituir al archivo plano — a diferencia de
  `INotificador` (donde un canal de correo o log es una posibilidad real y anticipada por A.11) o de `Convenio`
  (donde ya hay tres implementaciones concretas conviviendo), aquí solo existiría **una** implementación para
  siempre, y una interfaz con un solo implementador permanente es indirección sin beneficio; (2) el profesor
  prohibió el uso de patrones con nombre, y `IRepositorio<T>` es, literalmente, el patrón Repository; (3) es
  exactamente el tipo de límite que la rúbrica premia explícitamente (**criterio 6**: *"un límite al rediseño bien
  fundamentado para evitar sobre-ingeniería"*). Queda como **deuda técnica consciente**, declarada aquí, no como
  omisión.
- **`IVendible`, `IControlableEnInventario`, `IPerecedero` no se tratan como inversión de dependencia** — son ISP, no DIP: ningún punto del programa elige entre implementaciones
  intercambiables de estas interfaces; el tipo concreto de cada objeto es parte de lo que ese objeto **es**, no una
  estrategia sustituible en tiempo de ejecución.

---

## Resumen — las 4 piezas de cada inversión

| Caso | Alto nivel | Bajo nivel | Abstracción | Punto de resolución |
|---|---|---|---|---|
| Notificación (nuevo — Punto de Dolor #3) | `GestorInventario`, `ClienteService`, `MovimientoService` | `NotificadorConsola` | `INotificador` | `Program.ConstruirDependencias()` — composition root clásico, una sola vez al arrancar |
| Convenio / descuento (nuevo — SC-3) | `CasodeUsoProcesarVenta` | `SinConvenio`, `ConvenioUniversidad`, `ConvenioEmpresa` | `Convenio` | `ClienteService.Cargar(ruta)` — resolución por dato, uno por cada cliente cargado |
| Catálogo de productos (heredado, conservado) | `GestorInventario` | `Medicamento`, `MedicamentoCapsula`, `MedicamentoLiquido`, `ArticuloRetail` | `ProductoBase` | `GestorInventario.CargarDesdeArchivo(ruta)` — resolución por dato, mismo mecanismo que el caso anterior |

**Hallazgo del componente investigativo:** no todas las inversiones de dependencia de este sistema se resuelven en
el mismo punto. Hay un composition root clásico (`Program`, para dependencias singleton que se eligen una vez al
arrancar) y hay puntos de resolución **por dato** (`ClienteService.Cargar`, `GestorInventario.CargarDesdeArchivo`,
para jerarquías donde la implementación concreta varía por cada registro cargado desde archivo). Tratar ambos casos
como si fueran "un composition root" habría sido impreciso; el enunciado pide identificar **dónde** se resuelve la
construcción, y la respuesta correcta no es la misma para los tres casos.

---
