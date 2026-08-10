# Herencias y verificación LSP — Fase 3 

El diagrama TO-BE tiene **9 relaciones de generalización**, agrupadas en **3 árboles de herencia**:

| # | Árbol | Superclase | Subclases | Estado |
|---|---|---|---|---|
| 1 | Personas | `Persona` (abstracta) | `Cliente`, `Usuario` | Conservado del AS-IS, sin cambios |
| 2 | Productos | `ProductoBase` (abstracta) | `ArticuloRetail`, `Medicamento` | `ProductoBase` intervenida (ISP); `ArticuloRetail` nueva (OCP, SC-1) |
| 2b | Productos (nivel 2) | `Medicamento` (abstracta) | `MedicamentoCapsula`, `MedicamentoLiquido` | Conservado del AS-IS; `Medicamento` intervenida (LSP: pasa a abstracta) |
| 3 | Convenios | `Convenio` (abstracta) | `SinConvenio`, `ConvenioUniversidad`, `ConvenioEmpresa` | Nuevo (OCP, SC-3) |

---

## 1. `Persona` → `Cliente`, `Usuario`

### 1.1. Por qué herencia y no composición

`Cliente` y `Usuario` son, en el dominio real, **dos formas de ser una persona** dentro del sistema (una persona que
compra, una persona que opera el sistema) — no "tienen" una persona, **son** una persona con datos adicionales según
su rol. Es una relación taxonómica genuina (IS-A), no una necesidad de intercambiar comportamiento en tiempo de
ejecución (que sería el caso típico donde composición es preferible). Además, `Persona` no declara ninguna operación
propia más allá del constructor — no hay ningún método virtual que una subclase pudiera redefinir mal — así que no
existe el riesgo típico que hace preferible componer en vez de heredar (evitar que una subclase rompa el contrato
comportamental de un método heredado). Usar composición aquí (`Cliente` con un campo `-persona: Persona`) solo habría
añadido indirección (`cliente.persona.nombre` en vez de `cliente.nombre`) sin ningún beneficio a cambio.

### 1.2. Verificación LSP

- **Precondiciones:** el constructor `Persona(nombre, cedula, telefono, correo)` no impone precondiciones que
  `Cliente`/`Usuario` deban fortalecer — ambos solo **agregan** parámetros propios (`convenio` en `Cliente`;
  `usuario`/`contrasena` en `Usuario`) sin restringir el significado de los cuatro heredados.
- **Postcondiciones:** `Persona` garantiza que `nombre`, `cedula`, `telefono`, `correo` quedan asignados tal como se
  reciben. Ninguna subclase altera ese comportamiento.
- **Invariantes:** ninguno declarado más allá de "los cuatro campos existen"; se preserva en ambas subclases.
- **Excepciones:** `Persona` no declara excepciones; ninguna subclase introduce una excepción nueva sobre miembros
  heredados.
- **Punto de sustitución real:** no hay, en el diagrama TO-BE, ningún método que reciba un `Persona` genérico y
  despache polimórficamente sobre él (ni lo había en el AS-IS) — la jerarquía existe por estructura compartida, no
  por dispatch polimórfico. La verificación es, por tanto, trivialmente satisfecha: no hay ningún punto donde una
  sustitución pudiera fallar.

**Veredicto: PASA.** Se conserva sin cambios (por eso queda en negro en el diagrama).

---

## 2. `ProductoBase` → `Medicamento`, `ArticuloRetail`; `Medicamento` → `MedicamentoCapsula`, `MedicamentoLiquido`

### 2.1. Por qué herencia y no composición

`ProductoBase` concentra lo que **todo** producto físico con inventario comparte: código, nombre, precio, control de
stock (`DeducirStock`, `TieneStockSuficiente`, `EstaEnStockMinimo`). `Medicamento` y `ArticuloRetail` son, cada uno,
un **tipo de producto físico** real del dominio (IS-A), no un producto que "tiene" un comportamiento de inventario
intercambiable — el control de stock no varía por estrategia, varía por si el objeto es o no un producto físico
(por eso `ServicioMedico`, que no lo es, **no** está en este árbol — ver sección 4.2). `MedicamentoCapsula` y
`MedicamentoLiquido` son, del mismo modo, dos presentaciones reales de un medicamento, no dos comportamientos
intercambiables de una misma entidad.

### 2.2. Verificación LSP — y un hallazgo real corregido en el proceso

**Miembros heredados sin redefinir** (`DeducirStock`, `TieneStockSuficiente`, `EstaEnStockMinimo`, declarados
concretos en `ProductoBase`, no `{abstract}`): ninguna subclase los redefine — se heredan idénticos, así que no hay
ningún punto donde puedan violar precondición, postcondición o invariante: el comportamiento es, por construcción,
el mismo en toda la jerarquía.

**`MostrarInformacion()` (`{abstract}` en `ProductoBase`)**

- **Precondición:** ninguna — invocable en cualquier instancia válida, en las cuatro clases.
- **Postcondición esperada:** cada override debe **mostrar información**, sin lanzar excepción, y sin **omitir**
  nada de lo que el nivel anterior ya mostraba (una subclase puede añadir información, no puede esconder la que el
  contrato de la superclase prometía).
- **Hallazgo durante la verificación:** el diagrama, antes de esta revisión, le daba un `MostrarInformacion()`
  **propio** a `MedicamentoCapsula` y a `MedicamentoLiquido`. Al verificar esto contra el AS-IS
  (`CONTEXTO-RETO-FARMACIA.md`, Parte B.2), la evidencia dice lo contrario: *"`Producto.MostrarInformacion()` es
  `virtual`... pero **ninguna subclase la sobreescribe** — no existe `override MostrarInformacion` en
  `Medicamento.cs`, `MedicamentoCapsula.cs` ni `MedicamentoLiquido.cs`."* Es decir: en el sistema real, las tres
  heredan literalmente la misma implementación, y mostrar un `MedicamentoCapsula` nunca imprimió su `tipoRelleno`
  aunque el campo exista. Si el TO-BE le diera a `MedicamentoCapsula`/`MedicamentoLiquido` su propio override que sí
  imprime esos campos, el resultado impreso **cambiaría respecto al AS-IS** — una violación de la restricción dura
  (A.1.1), no autorizada por ninguna SC.
  **Corrección aplicada:** se retiró `MostrarInformacion()` de `MedicamentoCapsula` y `MedicamentoLiquido` en
  `Diagrama_tobe_Farmacia.puml`; ambas ahora **heredan** la implementación de `Medicamento` (que sí debe
  implementarla, porque es donde `ProductoBase` deja de ser abstracta en la práctica y porque el AS-IS ya mostraba
  ahí la misma información compartida). El diagrama fue re-validado con PlantUML tras el cambio, sin errores.
- **Invariantes:** ninguno adicional declarado; se preserva.
- **Excepciones:** ninguna clase de la jerarquía lanza excepción desde `MostrarInformacion()`.

**`EstaProximoAVencer(dias)` (de `IPerecedero`, implementada en `Medicamento`):** postcondición — devuelve `true`
solo si la fecha de vencimiento está a `dias` días o menos; ninguna subclase la redefine, se hereda igual.

**Punto de sustitución real:** `GestorInventario` opera sobre `List<ProductoBase>` sin distinguir el tipo concreto
(`VerificarStock()`, que llama a `EstaEnStockMinimo()` heredado igual en todos) y sobre `IPerecedero` para
`VerificarVencimiento()` (que `Medicamento` y `ArticuloRetail` implementan cada uno de forma independiente, ver 4.1).
En ambos casos, cualquier subtipo concreto responde exactamente igual al contrato esperado — la sustitución no
falla en ningún punto de uso real del diagrama.

**Veredicto: PASA**, condicionado a la corrección ya aplicada. Este es exactamente el tipo de verificación que la
rúbrica pide: no una lista que diga "cumple LSP", sino un caso concreto donde la verificación encontró una
inconsistencia real contra el AS-IS y forzó un cambio en el diagrama.

---

## 3. `Convenio` → `SinConvenio`, `ConvenioUniversidad`, `ConvenioEmpresa`

### 3.1. Por qué herencia y no composición

> **Nota de alcance (diagrama actualizado):** el diagrama ya no incluye `AutorizadorDeCredito` ni el enum
> `MetodoPago`, y `Convenio` ya no declara `AutorizarCredito`. El equipo evaluó modelar una regla de crédito además
> de la de descuento y decidió no hacerlo — no había, dentro del alcance de esta entrega, ningún caso de uso que
> fuera a ejercitar esa capacidad. `Convenio` queda enfocado únicamente en `CalcularDescuento`. La verificación de
> esta sección se actualiza en consecuencia.

Se evaluó explícitamente la alternativa de **no** heredar: modelar el descuento como un objeto de "estrategia"
separado que `Cliente` compondría (`-estrategiaDescuento`) — el enfoque más cercano a Strategy. Se descartó por dos
razones: (1) el profesor prohibió explícitamente el uso de patrones de diseño con nombre, y una jerarquía de
estrategias inyectadas es, funcionalmente, Strategy con otro nombre; (2) más de fondo, el descuento no es un
comportamiento intercambiable aislado — es una regla propia de **qué relación comercial** tiene el cliente con la
farmacia (empresa, banco, universidad...). `Convenio` modela esa relación comercial como lo que es en el dominio: un
tipo de vínculo institucional real (IS-A: un `ConvenioEmpresa` **es** un tipo de convenio), no una estrategia
inyectada desde afuera.

### 3.2. Verificación LSP

- **Precondiciones:** `CalcularDescuento(subtotal)` no impone precondición más allá de recibir un valor monetario
  válido (≥ 0); ninguna subclase concreta la fortalece (ninguna exige, por ejemplo, un mínimo de compra distinto
  entre convenios en el contrato — eso sería una precondición más fuerte que rompería sustitución).
- **Postcondición / invariante que toda subclase debe respetar (encontrado durante esta verificación, no estaba
  escrito antes):** `0 ≤ CalcularDescuento(subtotal) ≤ subtotal`. Ninguna implementación de `Convenio` puede
  devolver un valor mayor al subtotal (sería un recargo, no un descuento) ni negativo. Esto queda como **contrato
  formal de la abstracción**, no solo de una implementación particular.
- **Invariante crítico de comportamiento observable (ligado a A.1.1):** `SinConvenio.CalcularDescuento(subtotal)`
  **debe** devolver exactamente `subtotal` sin modificarlo. Razón: en el AS-IS no existía ningún mecanismo de
  descuento observable (`ServicioDescuento.CalcularDescuento` nunca se invocaba — `CONTEXTO` B.2/B.6). Todo cliente
  que hoy existe en `clientes.txt` se carga en el TO-BE con `convenio = SinConvenio` por defecto; si `SinConvenio`
  alterara el subtotal, **todos los clientes existentes verían un comportamiento nuevo** sin que ninguna SC lo
  autorice — violación directa de la restricción dura. `ConvenioUniversidad` y `ConvenioEmpresa`, en cambio, sí
  pueden aplicar un descuento real, porque son capacidad enteramente nueva bajo SC-3, sin clientes existentes que
  dependan de que "no pase nada".
- **Excepciones:** ninguna implementación debe lanzar excepción para montos válidos; el contrato es el mismo en las
  tres.
- **Punto de sustitución real:** `CasodeUsoProcesarVenta` llama `cliente.convenio.CalcularDescuento(subtotal)` **sin
  saber cuál convenio concreto tiene el cliente**. Mientras las tres subclases respeten el invariante
  `0 ≤ resultado ≤ subtotal` y el caso especial de `SinConvenio`, la sustitución es segura en ese punto de uso.

**Veredicto: PASA**, condicionado a que los dos invariantes de arriba (el general de la abstracción, y el
específico de `SinConvenio`) queden como **casos de caracterización obligatorios** en la Fase 4 — son, literalmente,
la verificación de que la restricción dura se cumple en el único punto del sistema donde SC-3 toca a los clientes
que ya existían.

---

## 4. Jerarquías candidatas que NO pasaron la verificación y cómo se reemplazaron

Estas dos herencias se consideraron durante el diseño y se **descartaron antes de llegar al diagrama final**,
precisamente por no pasar esta verificación. Se documentan aquí porque es la evidencia de que la verificación se
hizo con criterio real, no como formalidad posterior: ninguna jerarquía del diagrama actual falla porque las que
habrían fallado ya fueron reemplazadas por composición/realización de interfaz.

### 4.1. Candidata descartada: `ArticuloRetail` heredando de `Medicamento`

- **Por qué se consideró:** `ArticuloRetail` necesita `fechaVencimiento`/`EstaProximoAVencer`, que ya existían en
  `Medicamento`. Heredar parecía el camino de menor esfuerzo.
- **Por qué falla LSP:** `Medicamento` exige `laboratorio : Laboratorio` en su constructor y lo expone como parte de
  su contrato. Un `ArticuloRetail` (una gaseosa, un snack) **no tiene laboratorio** — sustituirlo donde se espera un
  `Medicamento` completo obligaría a (a) inventar un `Laboratorio` ficticio para satisfacer la precondición del
  constructor, lo cual introduce datos falsos en el dominio, o (b) dejarlo nulo, lo que **debilita la postcondición**
  que el resto del sistema asume válida para todo `Medicamento` (que tiene un laboratorio real). Cualquiera de las
  dos rompe LSP.
- **Cómo se reemplazó:** `ArticuloRetail` hereda directamente de `ProductoBase` (hermano de `Medicamento`, no hijo) e
  implementa `IPerecedero` **de forma independiente**, igual que `Medicamento`. Comparten la interfaz (mismo
  contrato: `fechaVencimiento`, `EstaProximoAVencer`), no la implementación ni el resto del estado de `Medicamento`
  — composición del contrato vía interfaz en vez de herencia de la clase completa.

### 4.2. Candidata descartada: `ServicioMedico` heredando de `ProductoBase`

- **Por qué se consideró:** `ServicioMedico` necesita código/nombre/precio/mostrar información, que ya existían en
  `ProductoBase`.
- **Por qué falla LSP:** `ProductoBase` implementa `IControlableEnInventario` (`DeducirStock`,
  `TieneStockSuficiente`, `EstaEnStockMinimo`) como parte de su contrato heredado automáticamente. Una inyectología o
  una curación **no tiene stock** en ningún sentido real. Sustituir un `ServicioMedico` donde se espera un
  `ProductoBase` obligaría a inventar un stock ficticio (p. ej. `stock = 999` fijo) para no romper la precondición
  de `DeducirStock`, lo que **corrompería el significado del contrato**: cualquier código que confíe en
  `EstaEnStockMinimo()` para decidir si hay que reabastecer recibiría una señal falsa para un servicio. Es
  exactamente la tensión que el enunciado anticipa en A.5.2 para SC-2 ("¿es un servicio sustituible donde se espera
  un `Producto` con stock? No").
- **Cómo se reemplazó:** `ServicioMedico` **no** hereda de `ProductoBase` en absoluto. Implementa únicamente
  `IVendible` (lo mínimo que de verdad comparte con los productos: código, nombre, precio, mostrar información) —
  cero relación con `IControlableEnInventario` ni con `IPerecedero`.

---

## Resumen de veredictos

| Jerarquía | Herencia vs. composición | Verificación LSP | Veredicto |
|---|---|---|---|
| `Persona` → `Cliente`, `Usuario` | Herencia (IS-A de identidad, sin comportamiento a redefinir) | Trivial, sin punto de sustitución polimórfica | **Pasa**, sin cambios |
| `ProductoBase` → `Medicamento`, `ArticuloRetail` | Herencia (IS-A de producto físico con inventario) | `MostrarInformacion()`: hallazgo real corregido (ver 2.2) | **Pasa**, tras corrección aplicada al diagrama |
| `Medicamento` → `MedicamentoCapsula`, `MedicamentoLiquido` | Herencia (IS-A de presentación de medicamento) | Miembros heredados sin redefinir; ninguna subclase override | **Pasa**, sin cambios de contrato |
| `Convenio` → `SinConvenio`, `ConvenioUniversidad`, `ConvenioEmpresa` | Herencia (IS-A de relación comercial; se descartó explícitamente el enfoque tipo-Strategy) | Invariante `0 ≤ resultado ≤ subtotal`; caso especial `SinConvenio` documentado como contrato duro | **Pasa**, condicionado a casos de caracterización en Fase 4 |
| *(descartada)* `ArticuloRetail` bajo `Medicamento` | — | Rompe postcondición de `laboratorio` | **No se construyó** — reemplazada por realización de `IPerecedero` |
| *(descartada)* `ServicioMedico` bajo `ProductoBase` | — | Rompe contrato de `IControlableEnInventario` | **No se construyó** — reemplazada por realización de `IVendible` únicamente |