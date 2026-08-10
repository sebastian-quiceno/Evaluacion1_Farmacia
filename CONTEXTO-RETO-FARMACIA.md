# Contexto completo del reto — Sistema FARMACIA

> **Propósito de este documento.** Es la fuente única de verdad para un agente que va a asistir en el reto.
> Contiene (A) el enunciado y la rúbrica del documento oficial, **filtrado exclusivamente al contexto de la farmacia**,
> y (B) el estado actual del modelo UML, **reconstruido a partir de `UML-EstadoActual.dia` y verificado línea por línea
> contra el código fuente de `BibFarmacia`**.
>
> **Regla de lectura:** todo lo que aparece en la Parte A es requisito del cliente/evaluador y no es negociable.
> La Parte B **ya no es una transcripción literal y bug-a-bug del `.dia`**: los defectos de transcripción y notación que
> tenía el archivo original (nombres, tipos, parámetros, dirección de relaciones, atributos redeclarados) se **corrigieron
> contra el código fuente real**, con la evidencia (archivo y línea) citada donde aporta trazabilidad. La sección B.6
> resume qué se corrigió, qué se confirmó como hallazgo real y una sugerencia de la herramienta que quedó **refutada**
> con evidencia.

## Nota de estado — Fases 1 a 4 completas, entregable reorganizado según A.9

**Actualización:** las Fases 1 a 4 ya están completas (diagnóstico AS-IS, solicitudes de cambio, diseño TO-BE e
implementación). El repositorio se reorganizó siguiendo la estructura de carpetas que pide A.9 del enunciado
(`00-lectura-en-frio` a `04-evidencia`, más el código AS-IS original sin modificar en la raíz). El código fuente
AS-IS (`BibFarmacia` + `AppFarmaciaConsola`) **sí está presente**, en la raíz de este repositorio, junto a
`SolucionFarmacia.sln`.

**Implicación para cualquier agente que lea esto:** toda cita `archivo:línea` de la Parte B es evidencia verificada
contra ese código AS-IS real. La Parte A (enunciado y rúbrica) sigue vigente al 100 %.

Documentos hermanos de esta misma Fase 1-2 (ahora en `01-diagnostico/`, salvo el diagrama TO-BE y los 4 documentos
de argumentación de Fase 3, que están en `02-diseno/`):

| Archivo | Contenido |
|---|---|
| `01-diagnostico/UML-EstadoActual.dia` | Diagrama de clases AS-IS, corregido y verificado contra el código viejo. |
| `01-diagnostico/MapadeDependencias.dia` | Mapa de dependencias AS-IS (alto/bajo nivel, dirección de acoplamiento). |
| `01-diagnostico/InventarioDeHallazgos.xlsx` | 13 hallazgos (H-01 a H-13) con archivo:línea, principio, impacto, severidad/origen; incluye una refutación (H-11). |
| `01-diagnostico/Puntos de Dolor Priorizados.docx` | Los 3 puntos de dolor priorizados, con criterio de priorización y justificación #1>#2>#3. |
| `01-diagnostico/Soluciones de cambio.docx` | Diseño propuesto para SC-1, SC-2 y SC-3, cada una con línea base medida sobre el código viejo. |

Si alguno de estos archivos no llegó a esta sesión, hay que pedírselo al usuario antes de asumir hallazgos o líneas
base — no hay que inventarlos (Parte C, instrucción 1).

---

# PARTE A — Enunciado oficial (contexto FARMACIA)

## A.0. Encabezado del reto

- **Nombre:** Reto técnico de ingreso — Modernización arquitectónica de un sistema corporativo heredado.
- **Curso:** Arquitectura de Software.
- **Modalidad:** equipos de máximo 4 integrantes.
- **Porcentaje del curso:** 25 %.
- **Escala de evaluación:** 0.0 – 5.0.

## A.1. Correo de la Líder Técnica (contextualización)

*De: Líder Técnica, Dirección de Ingeniería. Para: los ingenieros junior recién vinculados al equipo de la dirección de sistemas. Asunto: su primera asignación.*

Contenido, en sus puntos verificables:

1. El sistema entregado es **una pieza crítica de la operación**: si deja de funcionar un día, el negocio se detiene.
2. Lo desarrolló hace varios años **un proveedor que ya no está** en la empresa.
3. **Funciona y hace lo que tiene que hacer**, y ese es exactamente el problema: *funciona, pero nadie se atreve a tocarlo*.
4. **No hay documentación, no hay diagramas, no hay pruebas, no hay a quién preguntarle.** Lo único que existe es el código fuente.
5. Cada cambio pequeño que pide el negocio toma semanas, obliga a tocar media docena de archivos y rompe algo en otro lado que nadie había previsto. **Ya no es un problema técnico: es un problema de costos y de credibilidad.**
6. La líder pide dos cosas que el código actual no le da:
   - que alguien le explique **con evidencia POR QUÉ** el sistema es tan costoso de cambiar, en términos de **recursos y riesgos**;
   - que alguien le entregue **una versión que sí pueda evolucionar**.
7. **No quiere un sistema nuevo.** Quiere *el mismo sistema, con la misma conducta observable*, sostenido por una arquitectura que resista los próximos tres años.

### A.1.1. Restricción dura (crítica)

> **El comportamiento observable del sistema no puede cambiar. Ni una salida, ni una regla de negocio, ni un cálculo. Lo único que puede cambiar es la estructura interna.**
>
> Las **únicas excepciones** son las solicitudes de cambio autorizadas explícitamente en la sección A.5.

Violar esto tiene penalización explícita (ver A.10).

### A.1.2. Sobre el uso de herramientas de IA (política del cliente)

- En la empresa se usa IA y **el equipo también debe usarla: es parte del trabajo**.
- Pero el pago no es por el resultado de la herramienta: *"la herramienta les va a listar problemas. **A ustedes les pago por decidir cuáles importan, cuáles no, en qué orden se atacan y qué se deja como está.***"*
- **Un informe generado que el equipo no pueda defender línea por línea no es válido**, habla muy mal del equipo y es un riesgo, porque la líder tomará decisiones de presupuesto con base en lo que el equipo diga.
- Por eso, buena parte de lo evaluado **no es el resultado de la herramienta, sino lo que el equipo hizo con ese resultado**: qué aceptó, qué descartó, qué encontró por su cuenta y **qué decidió NO hacer**.

> **Implicación operativa para el agente:** toda salida generada debe venir acompañada de razonamiento defendible y trazable. Nada de listas declarativas. Nada que el equipo no pueda sustentar en video sin leer.

## A.2. Conformación del equipo y responsabilidades

Todos deben conocer la solución completa, pero cada integrante **responde formalmente por un frente** y lo sustenta.

| Rol | Responde por |
|---|---|
| **Arquitecto de dominio** | Identificación de responsabilidades y límites de cada clase (SRP), modelo del dominio, jerarquías de herencia y su validez frente a LSP. |
| **Arquitecto de dependencias** | Mapa de dependencias, abstracciones (interfaces), inversión e inyección de dependencias, composition root (DIP, ISP). |
| **Ingeniero de comportamiento** | Pruebas de caracterización, evidencia de que la conducta observable se preservó, escenarios de ejecución del programa principal. |
| **Integrador y evidencia** | Consistencia diagrama–código, estructura del entregable, bitácora de uso de IA, métricas antes/después. |

- Los roles **se declaran en el entregable y en el video**.
- La nota es **grupal**, pero **se ajusta individualmente** según el dominio que cada integrante demuestre (ver A.8 y A.10).
- **Si el equipo es de menos personas, estos roles los toman los integrantes que haya** (se reparten, no se eliminan).

## A.3. Fase 0 — Lectura en frío (obligatoria y previa a cualquier herramienta)

**Antes de ejecutar cualquier análisis asistido**, *cada integrante* lee el código fuente por su cuenta y escribe, **a mano o en un documento propio**, **una página** con:

1. Qué cree que hace el sistema y cuáles son sus **entidades principales**.
2. **Los tres lugares del código** que, en su criterio, harían **costoso un cambio**, y **por qué**.
3. **Una pregunta** que le haría al programador original si pudiera.

Condiciones:

- El análisis de cada integrante **se entrega al finalizar la clase** y luego **como anexo al trabajo final**.
- **No se califica que hayan acertado**: se califica que **exista el registro** y que **en el video contrasten esas hipótesis iniciales con lo que finalmente encontraron**.
- Consigna: **primero el criterio, después la herramienta.**

> **Implicación operativa para el agente:** esta fase **no es delegable a IA**. Es producción individual, previa, y su ausencia se castiga con 0.0 en el criterio 6 (ver A.10).

## A.4. Fase 1 — Diagnóstico del sistema actual (AS-IS)

Debe entregarse un diagnóstico **presentable ante un comité de presupuesto**. Contenido obligatorio:

1. **Diagrama UML del estado actual (notación extendida)**, reconstruido a partir del código fuente, con **todas las clases, atributos, operaciones, relaciones y multiplicidades reales**. **No un diagrama idealizado**: el que refleja lo que realmente está escrito.
2. **Inventario de hallazgos** en forma de tabla (estructura mínima abajo). **Cada hallazgo debe ser trazable a un archivo y a una línea concreta.**
3. **Mapa de dependencias**: qué clase depende de cuáles, cuáles son de **alto nivel** (reglas del negocio) y cuáles de **bajo nivel** (detalles técnicos), y **dónde se invierte esa relación hoy**.
4. **Los tres puntos de dolor priorizados**, con el **criterio de priorización explícito**. La líder quiere saber **por qué el número 1 está antes que el número 2**.

### A.4.1. Estructura mínima del inventario de hallazgos

| ID | Ubicación (archivo / clase / línea) | Síntoma observado | Principio comprometido | Impacto en el negocio | Severidad y origen |
|---|---|---|---|---|---|
| H-01 | | | | *Traducido a costo, riesgo o tiempo de cambio* | *Alta / Media / Baja — Propio o Asistido* |

### A.4.2. Dos exigencias no negociables sobre el inventario

- **Al menos TRES hallazgos marcados como "Propio"**: encontrados por el equipo leyendo el código, **no sugeridos por la herramienta**. Deben poder **mostrar en el video cómo llegaron a ellos**.
- **Al menos UNA sugerencia de la herramienta debe estar REFUTADA**: algo que la IA marcó como problema y que el equipo, **con argumento técnico**, descarta. Hay que explicar **por qué se equivocó o por qué no aplica en este contexto**.

## A.5. Fase 2 — Los cambios que vienen (SOLICITUDES DE CAMBIO — FARMACIA)

> Principio del enunciado: *un diseño no se juzga en abstracto: se juzga contra el futuro que tiene que soportar.*
> **Estas solicitudes ya están aprobadas por el negocio para el próximo trimestre. La arquitectura tiene que hacerlas baratas.**

| Código | Solicitud de cambio — **Farmacia** |
|---|---|
| **SC-1** | La farmacia necesita **no solamente vender productos farmacéuticos sino también cosméticos y productos comestibles** (gaseosas, agua, helados, snacks en general). |
| **SC-2** | La farmacia también **venderá servicios** como: **inyectología, cambio de vendajes, curaciones básicas**, entre otros. |
| **SC-3** | La farmacia **manejará convenios con diferentes entidades** para ofrecer **descuentos en compras** y también **crédito** para que sea descontado a los **empleados, clientes o asociados**, según la entidad. Se quieren tener convenios con: **empresas (empleados), bancos (clientes), cooperativas (asociados), entidades mutuales (asociados), universidades y colegios (estudiantes y profesores)**. |

### A.5.1. Medición obligatoria sobre el código ACTUAL (línea base)

**Para cada una de las tres solicitudes, ANTES de rediseñar**, hay que medir sobre el código actual:

- **Cuántas clases y cuántos archivos habría que modificar** para implementarla hoy.
- **Qué comportamiento existente correría riesgo de romperse.**

Esta medición es la **línea base** contra la que se compara la métrica de la Fase 4 (ver A.7 y criterio 5 de la rúbrica).

### A.5.2. Notas de lectura de las SC (tensión arquitectónica que cada una presiona)

Derivado del anexo A.11 y del contenido de cada SC:

- **SC-1** introduce **nuevos tipos de producto sin fecha de vencimiento, sin laboratorio y sin lógica farmacéutica** → presiona **OCP** (agregar tipos sin modificar) y **LSP** (¿un snack es sustituible donde se espera un `Producto` con vencimiento y stock mínimo?).
- **SC-2** introduce **ítems vendibles que no son inventariables**: un servicio no tiene stock, ni vencimiento, ni laboratorio → presiona **LSP** e **ISP** (operaciones aplicables solo a algunos tipos), y rompe el supuesto de que "todo lo vendible es un `Producto` con stock".
- **SC-3** introduce **reglas de cálculo variables por entidad y por tipo de vinculado**, más un mecanismo de **crédito** (un medio de pago nuevo) → presiona **OCP**, **SRP** (el cálculo de descuento hoy es un método único) y **DIP** (la política de convenio es una abstracción que el núcleo debe consumir, no conocer).

## A.6. Fase 3 — Diseño de la nueva arquitectura (TO-BE)

Requisitos del diseño:

1. **Diagrama UML en notación extendida**, con la **convención de color acordada en clase**:
   - **en negro** lo que se **conserva** del diseño original;
   - **en color** cada **elemento intervenido**;
   - **una convención de color por principio aplicado**, con su **respectiva leyenda**.
2. **Los cinco principios SOLID deben quedar aplicados y, sobre todo, ARGUMENTADOS.**
   > *"No me sirve una lista que diga «aplicamos SRP»: quiero saber qué clase se partió, en cuántas, por qué esa frontera y no otra, y qué gano yo con eso."*
3. **Toda herencia nueva o conservada debe estar justificada**:
   - **por qué herencia y no composición**;
   - **verificación explícita de que la subclase es sustituible por la superclase**: **precondiciones, postcondiciones, invariantes, excepciones**;
   - **si una jerarquía existente no pasa esa verificación, hay que decir cómo se reemplaza.**
4. **Toda inversión de dependencia debe indicar**:
   - **quién es el módulo de alto nivel**;
   - **quién es el de bajo nivel**;
   - **cuál es la abstracción que los desacopla**;
   - **en qué punto del programa se resuelve la construcción de los objetos (composition root)** — *marcado en el enunciado como **componente investigativo***.
5. **Registros de Decisión Arquitectónica (ADR): MÍNIMO CINCO**, uno por cada decisión estructural relevante. **Cada ADR debe contener**:
   - **contexto y evidencia que la motiva** (con **referencia a un hallazgo del inventario**);
   - **al menos DOS alternativas evaluadas**;
   - **la decisión tomada**;
   - **el costo o consecuencia negativa que se acepta**;
   - **el o los principios involucrados**.
   > *"Una decisión sin alternativa descartada no es una decisión: es lo primero que se les ocurrió."*

## A.7. Fase 4 — Implementación y evidencia

1. **Código fuente rediseñado** en **C# u otro lenguaje orientado a objetos**, **compilando y ejecutándose**, **fiel al diagrama**:
   - **cada clase e interfaz del diagrama TO-BE existe en el código con el mismo nombre**;
   - **no hay clases relevantes en el código que no aparezcan en el diagrama**.
2. **Programa principal de demostración** que recorra **los escenarios de uso más importantes** del sistema.
3. **Evidencia de preservación del comportamiento**: **mínimo OCHO casos de caracterización** ejecutados **contra el sistema original y contra el rediseñado**, **con las salidas de ambos**, demostrando que **coinciden**.
4. **Implementación de UNA de las tres solicitudes de cambio** (la que el equipo elija, **justificando la elección**), acompañada de la **métrica comparativa**: **clases creadas frente a clases modificadas, en la arquitectura vieja y en la nueva**.
   > *Esa tabla es la prueba empírica de que el principio abierto/cerrado quedó realmente aplicado.*
5. **Bitácora de uso de IA**: registro de las consultas relevantes con **tres columnas**:
   - **qué propuso la herramienta**;
   - **qué decidió el equipo** (*aceptado / corregido / rechazado*);
   - **con qué argumento**.
   > **Se evalúa la calidad del juicio, no la cantidad de registros.**

## A.8. Fase 5 — Sustentación en video

- **Máximo 20 minutos**, **compartido por enlace (no adjunto)**.
- **Deben participar los cuatro integrantes, con cámara**, y **cada uno presenta el frente del que es responsable**.

Guion sugerido:

| Tiempo | Contenido |
|---|---|
| 0 – 3 min | El sistema actual: qué hace, **diagrama AS-IS** y **los tres puntos de dolor priorizados**. |
| 3 – 6 min | **Hallazgos**: los propios, **el que refutaron a la herramienta** y el **contraste con las hipótesis de la lectura en frío**. |
| 6 – 12 min | **Diagrama TO-BE recorrido en detalle**: cada intervención en color, con su **ADR**, su **alternativa descartada** y el **principio** que cumple. |
| 12 – 17 min | **Ejecución en vivo**: comportamiento preservado y la **solicitud de cambio implementada**, con la **métrica antes/después**. |
| 17 – 20 min | **Lo que la IA propuso y no hicimos**; **el punto donde decidimos no aplicar un principio y por qué**; **qué queda como deuda técnica consciente**. |

Nota: el enunciado menciona **"preguntas de contradicción"** que los evaluadores hacen a cada integrante (aparecen en las reglas y en el criterio 7 de la rúbrica).

## A.9. Entregables y organización

| Carpeta | Contenido |
|---|---|
| `/00-lectura-en-frio` | Las **cuatro hojas de hipótesis iniciales, sin modificar**. |
| `/01-diagnostico` | **Diagrama AS-IS (fuente editable + imagen)**, **inventario de hallazgos**, **mapa de dependencias**, **línea base de las tres solicitudes de cambio**. |
| `/02-diseno` | **Diagrama TO-BE con leyenda de colores (fuente editable + imagen)** y **los ADR**. |
| `/03-src` | **En el GitHub de un integrante**: el **código fuente rediseñado, compilable**, con el **programa principal** y los **casos de caracterización**. |
| `/04-evidencia` | **Salidas comparadas antes/después**, **métrica de la solicitud de cambio implementada**, **bitácora de uso de IA**. |
| `README` | **Roles del equipo**, **instrucciones de ejecución** y **enlace al video**. |

**Fechas límite:**
- Grupo **Martes** → **domingo 9 de agosto**.
- Grupo **Miércoles** → **lunes 10 de agosto**.
- **Hora límite: 23:59 del día límite** en ambos casos.

## A.10. Reglas y penalizaciones

| Situación | Penalización |
|---|---|
| **Código que no compila o no ejecuta** | El **criterio de implementación se califica en 0.0**. |
| **Inconsistencia entre el diagrama TO-BE y el código entregado** | La nota de los **criterios 2 y 4 no puede superar 3.0**. |
| **Ausencia de la bitácora de uso de IA o de las hojas de lectura en frío** | El **criterio 6 se califica en 0.0**. |
| **Cambio no autorizado en el comportamiento observable** | **−0.5 sobre la nota final por cada caso**. |
| **Integrante que no participa en el video o no responde su pregunta de contradicción** | Su **nota individual se reduce en al menos 1.5**. |
| **Video que excede los 20 minutos** | **No se evalúa el contenido posterior al minuto 20**. |
| **Entrega tardía** | **−0.5 por cada 24 horas de retraso**. |

## A.11. Anexo — Catálogo de solicitudes de cambio (referencia del líder técnico)

Familias de cambio que el enunciado sugiere para instanciar SC-1, SC-2 y SC-3. Sirven como **lente de análisis** para justificar qué principio presiona cada solicitud de la farmacia:

- **Nuevo subtipo de una entidad existente con reglas de cálculo propias** → presiona **OCP** y **LSP**.
- **Cambio del medio de persistencia o del origen de los datos** (de archivo plano a base de datos o servicio externo) → presiona **DIP**.
- **Nuevo canal de notificación o de salida** (correo, mensajería, exportación a otro formato) → presiona **DIP** e **ISP**.
- **Cambio en una regla de negocio parametrizable** (tarifas, topes, porcentajes o vigencias hoy escritas en el código) → presiona **SRP** y **OCP**.
- **Operación aplicable solo a algunos tipos** y no a todos los que hoy comparten jerarquía o interfaz → presiona **ISP** y **LSP**.
- **Nuevo reporte o consulta que combina información de varias entidades** → presiona **SRP** y revela **acoplamientos ocultos**.

## A.12. Rúbrica de evaluación (escala 0.0 – 5.0)

**Nota final del equipo = Σ (nota del criterio × peso).** Los niveles son referencias; **se admiten valores intermedios**.

### Criterio 1 — Diagnóstico AS-IS y arqueología del código (**15 %**)

| Nivel | Descriptor |
|---|---|
| **Excelente (4.5–5.0)** | El diagrama **refleja fielmente el código**. Los hallazgos son **trazables a archivo y línea**, están **traducidos a impacto de negocio** y **priorizados con criterio explícito y defendible**. Hay **hallazgos propios de valor real**. |
| **Satisfactorio (3.5–4.4)** | Diagrama fiel con **omisiones menores**. Hallazgos trazables y en su mayoría bien clasificados; la priorización existe pero **su criterio es parcialmente implícito**. |
| **Mínimo aceptable (3.0–3.4)** | El diagrama representa el sistema de forma **incompleta o idealizada**. Hallazgos **genéricos, poco trazables** al código; la priorización es **enunciativa**. |
| **Insuficiente (0.0–2.9)** | Diagrama que **no corresponde al código entregado**. Listado de problemas genérico, sin evidencia ni ubicación, **indistinguible de una salida automática**. |

### Criterio 2 — Diseño TO-BE y diagrama UML extendido (**20 %**)

| Nivel | Descriptor |
|---|---|
| **Excelente (4.5–5.0)** | **Notación extendida correcta y completa** (visibilidad, tipos, multiplicidades, estereotipos). **Convención de color aplicada con leyenda.** La estructura **resuelve efectivamente los puntos de dolor priorizados** y es **coherente de extremo a extremo**. |
| **Satisfactorio (3.5–4.4)** | Notación correcta con **imprecisiones puntuales**. Colores y leyenda presentes. El diseño resuelve los problemas principales, con **alguna decisión estructural débil o poco integrada**. |
| **Mínimo aceptable (3.0–3.4)** | Diagrama legible pero con **errores de notación o relaciones ambiguas**. El rediseño atiende **solo parcialmente** los puntos de dolor identificados. |
| **Insuficiente (0.0–2.9)** | Diagrama **incompleto**, con **errores graves de notación**, **sin diferenciación de lo intervenido**, o que **no guarda relación con el diagnóstico previo**. |

### Criterio 3 — Argumentación arquitectónica: ADR, SOLID, herencias e inversión de dependencias (**20 %**)

| Nivel | Descriptor |
|---|---|
| **Excelente (4.5–5.0)** | Los **cinco principios** están aplicados y **argumentados sobre evidencia del código**. **Cada ADR presenta alternativas reales evaluadas y el costo aceptado.** **Toda herencia se verifica contra LSP** y **cada inversión identifica alto nivel, bajo nivel, abstracción y composition root**. |
| **Satisfactorio (3.5–4.4)** | Los cinco principios aplicados y argumentados correctamente, **aunque alguno se sustenta más en la teoría que en la evidencia del sistema**. ADR completos con alternativas, **algunas tratadas superficialmente**. |
| **Mínimo aceptable (3.0–3.4)** | Principios aplicados de forma **desigual**: uno o dos quedan **solo enunciados**. ADR presentes pero **sin alternativas reales o sin consecuencias declaradas**. Herencias e inversiones **descritas, no justificadas**. |
| **Insuficiente (0.0–2.9)** | Argumentación **declarativa** («aplicamos SRP») sin sustento en el diseño; principios **ausentes o mal comprendidos**; ADR **inexistentes o meramente descriptivos** de lo hecho. |

### Criterio 4 — Implementación fiel y preservación del comportamiento (**15 %**)

| Nivel | Descriptor |
|---|---|
| **Excelente (4.5–5.0)** | El código **compila, ejecuta y corresponde uno a uno con el diagrama**. Los **casos de caracterización demuestran de forma convincente** que la conducta observable se conservó. **Programa principal claro y representativo.** |
| **Satisfactorio (3.5–4.4)** | El código compila y corresponde al diagrama **salvo detalles menores**. Evidencia de preservación presente y suficiente, con **cobertura de escenarios algo limitada**. |
| **Mínimo aceptable (3.0–3.4)** | El código ejecuta pero **se aparta del diagrama en aspectos relevantes**, o la evidencia de preservación es **escasa o poco concluyente**. |
| **Insuficiente (0.0–2.9)** | **No compila, no ejecuta**, o la implementación **no corresponde al diseño presentado**. Sin evidencia de preservación del comportamiento. |

### Criterio 5 — Prueba de extensibilidad y métricas antes/después (**10 %**)

| Nivel | Descriptor |
|---|---|
| **Excelente (4.5–5.0)** | La solicitud de cambio **se implementa mayoritariamente AGREGANDO código nuevo**. La métrica comparativa es **correcta, honesta y bien interpretada**; el equipo **explica qué habría pasado con las otras dos solicitudes**. |
| **Satisfactorio (3.5–4.4)** | Solicitud implementada correctamente y métrica presente y bien construida, con **interpretación algo limitada**. |
| **Mínimo aceptable (3.0–3.4)** | Solicitud implementada pero con **modificaciones extensas de clases existentes**, o **métrica presentada sin análisis**. |
| **Insuficiente (0.0–2.9)** | **No se implementó** la solicitud de cambio, o **no hay comparación con la línea base** del sistema original. |

### Criterio 6 — Criterio propio frente a la IA (**10 %**)

| Nivel | Descriptor |
|---|---|
| **Excelente (4.5–5.0)** | La bitácora **evidencia juicio técnico real**: propuestas **corregidas o rechazadas con argumento sólido**, **hallazgos propios verificables** y **un límite al rediseño bien fundamentado para evitar sobre-ingeniería**. |
| **Satisfactorio (3.5–4.4)** | Bitácora completa con decisiones justificadas; **alguna refutación es débil** o el **límite al rediseño está poco desarrollado**. |
| **Mínimo aceptable (3.0–3.4)** | Bitácora presente pero **mayormente descriptiva**; **se acepta casi todo lo sugerido sin discusión** y el límite al rediseño es **superficial**. |
| **Insuficiente (0.0–2.9)** | **Sin bitácora**, o entregable **indistinguible de una salida automática** y que el equipo **no logra defender en la sustentación**. |

### Criterio 7 — Sustentación en video y dominio individual (**10 %**)

| Nivel | Descriptor |
|---|---|
| **Excelente (4.5–5.0)** | Los cuatro participan, **dominan su frente y también el conjunto**. **Responden las preguntas de contradicción con solvencia.** Exposición ordenada, dentro del tiempo, con **ejecución en vivo convincente**. |
| **Satisfactorio (3.5–4.4)** | Todos participan con buen dominio de su frente; **alguna respuesta a las preguntas de contradicción es imprecisa**. Exposición clara y dentro del tiempo. |
| **Mínimo aceptable (3.0–3.4)** | **Participación desigual**: uno o más integrantes **leen** o muestran dominio limitado. Exposición **desordenada o excedida en tiempo**. |
| **Insuficiente (0.0–2.9)** | **No participan todos**, se expone **sin dominio del contenido**, o **no se muestra el sistema en ejecución**. |

### A.12.1. Tabla de pesos (resumen)

| # | Criterio | Peso |
|---|---|---|
| 1 | Diagnóstico AS-IS y arqueología del código | 15 % |
| 2 | Diseño TO-BE y diagrama UML extendido | 20 % |
| 3 | Argumentación arquitectónica: ADR, SOLID, herencias, DIP | 20 % |
| 4 | Implementación fiel y preservación del comportamiento | 15 % |
| 5 | Prueba de extensibilidad y métricas antes/después | 10 % |
| 6 | Criterio propio frente a la IA | 10 % |
| 7 | Sustentación en video y dominio individual | 10 % |
| | **Total** | **100 %** |

---

# PARTE B — Estado actual del sistema (verificado contra `BibFarmacia`)

> **Fuente:** `UML-EstadoActual.dia` (26 elementos de clase, 27 relaciones), **corregido y verificado contra el código
> fuente de `BibFarmacia`**. Donde el `.dia` original tenía un error de transcripción, notación o dirección de relación,
> se corrigió aquí para que el diagrama represente **lo que el código realmente hace**, tal como exige el criterio 1 de
> la rúbrica ("no un diagrama idealizado... el que refleja lo que realmente está escrito"). La evidencia archivo:línea
> de cada corrección relevante y de cada hallazgo confirmado queda en **B.6**.
>
> Convención de visibilidad: `+` público, `-` privado, `#` protegido, `~` paquete/implementación.
> Convención de propiedad: `{static}` = miembro estático, `{virtual}` = método virtual/redefinible, `{abstract}` = clase o método abstracto.

## B.1. Inventario de elementos

| # | Elemento | Estereotipo | ¿Abstracta? |
|---|---|---|---|
| 1 | `Persona` | `<<Abstract>>` | Sí |
| 2 | `Usuario` | — | No |
| 3 | `Cliente` | — | No |
| 4 | `Producto` | `<<Abstract>>` | Sí |
| 5 | `Medicamento` | — | No |
| 6 | `MedicamentoCapsula` | — | No |
| 7 | `MedicamentoLiquido` | — | No |
| 8 | `TipoRelleno` | `<<enum>>` | No |
| 9 | `MaterialEnvase` | `<<enum>>` | No |
| 10 | `Laboratorio` | — | No |
| 11 | `Movimiento` | — | No |
| 12 | `ServicioCliente` | — | No |
| 13 | `ServicioMovimiento` | — | No |
| 14 | `ServicioProducto` | — | No |
| 15 | `ServicioUsuario` | — | No |
| 16 | `AspectoValidacion` | `<<static>>` | No |
| 17 | `AspectoAutenticacion` | `<<static>>` | No |
| 18 | `EventoMovimiento` | — | No |
| 19 | `EventoPuntos` | — | No |
| 20 | `EventoStockMinimo` | — | No |
| 21 | `EventoVencimiento` | — | No |
| 22 | `IServicioNotificacion` | `<<Interface>>` | No |
| 23 | `ServicioNotificacion` | — | No |
| 24 | `ServicioDescuento` | — | No |
| 25 | `IDescuento` | `<<Interface>>` | No |
| 26 | `ProductoFactory` | `<<static>>` | No |

**Correcciones aplicadas a este inventario (verificadas contra código):**

- **`MedicamentoLiquido`** ya no se marca `{abstract}`: en código es `public class MedicamentoLiquido : Medicamento` sin la palabra `abstract`, y `ProductoFactory.CrearLiquido` la instancia directamente con `new MedicamentoLiquido(...)` ([MedicamentoLiquido.cs:11](BibFarmacia/Clases/MedicamentoLiquido.cs#L11), [ProductoFactory.cs:34](BibFarmacia/Factories/ProductoFactory.cs#L34)).
- **`laboratorio` → `Laboratorio`**: el nombre de la clase en C# lleva mayúscula inicial ([Laboratorio.cs:9](BibFarmacia/Clases/Laboratorio.cs#L9)); en C# el nombre de tipo importa mayúsculas/minúsculas.
- **`Producto.MostrarInformacion()` es `virtual`** en código (`public virtual void MostrarInformacion()`, [Producto.cs:29](BibFarmacia/Clases/Producto.cs#L29)), pero **ninguna subclase la sobreescribe** — no existe `override MostrarInformacion` en `Medicamento.cs`, `MedicamentoCapsula.cs` ni `MedicamentoLiquido.cs`. Se marca `{virtual}` solo en `Producto` y se retira de las subclases en B.2 (antes aparecía repetida en las cuatro, sugiriendo redefinición que no existe).
- **`AspectoValidacion`, `AspectoAutenticacion` y `ProductoFactory`** son `public static class` en C# ([AspectoValidacion.cs:11](BibFarmacia/Aspectos/AspectoValidacion.cs#L11), [AspectoAutenticacion.cs:11](BibFarmacia/Aspectos/AspectoAutenticacion.cs#L11), [ProductoFactory.cs:11](BibFarmacia/Factories/ProductoFactory.cs#L11)), lo que vuelve estáticos todos sus miembros por definición del lenguaje; se marcan `{static}` en B.2.

## B.2. Detalle de cada clase (verificado contra código)

> Nota de forma: los atributos heredados **no se repiten** en las subclases (antes se listaban de nuevo en cada nivel de
> herencia, lo que sugería duplicación de campos que no existe en el código: en `Medicamento.cs`, `MedicamentoCapsula.cs`
> y `MedicamentoLiquido.cs` solo se declara lo propio de cada clase; el resto llega por `: base(...)`). Se muestran solo
> una vez, en la clase donde el código realmente los declara.

### Dominio — Personas

```
<<Abstract>> Persona                      {abstract}
  - nombre: string
  - cedula: string
  - telefono: string
  - correo: string
  + Persona(nombre: string, cedula: string, telefono: string, correo: string)
```

```
Usuario
  - userName: string
  - password: string
  + Usuario(nombre: string, cedula: string, telefono: string, correo: string,
            userName: string, password: string)
```

```
Cliente
  - puntos: int
  + Cliente(nombre: string, cedula: string, telefono: string, correo: string)
  + AcumularPuntos(puntos: int): void
```

> **Nota de evidencia:** `Cliente.AcumularPuntos(puntos)` nunca es invocado en todo el código. `ServicioCliente.AcumularPuntos`
> ([ServicioCliente.cs:36-45](BibFarmacia/Servicios/ServicioCliente.cs#L36-L45)) hace `cliente.Puntos += puntos;` directamente
> en vez de llamar a `cliente.AcumularPuntos(puntos)`. Hay dos implementaciones independientes de la misma regla de negocio
> ("sumar puntos"), una de ellas muerta. Ver B.6.

### Dominio — Productos

```
<<Abstract>> Producto                     {abstract}
  - nombre: string
  - precio: decimal
  - stock: int
  - stockMinimo: int
  - FechaVencimiento: DateTime
  + Producto(nombre: string, precio: decimal, stock: int, stockMinimo: int,
             fechaVencimiento: DateTime)
  + MostrarInformacion(): void            {virtual}
```

```
Medicamento
  + Laboratorio: Laboratorio
  + Medicamento(nombre: string, precio: decimal, stock: int, stockMinimo: int,
                fechaVencimiento: DateTime, laboratorio: Laboratorio)
```

```
MedicamentoCapsula
  + TipoRelleno: TipoRelleno
  + MedicamentoCapsula(nombre: string, precio: decimal, stock: int, stockMinimo: int,
                       fechaVencimiento: DateTime, laboratorio: Laboratorio,
                       tipoRelleno: TipoRelleno)
```

```
MedicamentoLiquido
  + MaterialEnvase: MaterialEnvase
  + Mililitros: int
  + MedicamentoLiquido(nombre: string, precio: decimal, stock: int, stockMinimo: int,
                       fechaVencimiento: DateTime, laboratorio: Laboratorio,
                       materialEnvase: MaterialEnvase, mililitros: int)
```

```
<<enum>> TipoRelleno
  + Gel
  + Polvo
```

```
<<enum>> MaterialEnvase
  + Vidrio
  + Plastico
```

```
Laboratorio
  - nombre: string
  - direccion: string
  - telefono: string
  + Laboratorio(nombre: string, direccion: string, telefono: string)
```

### Dominio — Inventario

```
Movimiento
  - fecha: DateTime
  - cantidad: int
  - tipo: string
  - producto: Producto
  + Movimiento(fecha: DateTime, cantidad: int, tipo: string, producto: Producto)
```

### Capa de servicios

```
ServicioCliente
  - clientes: List<Cliente>
  + EventoPuntos: EventoPuntos
  + ServicioCliente()
  + AgregarCliente(cliente: Cliente): void
  + ObtenerClientes(): List<Cliente>
  + AcumularPuntos(cliente: Cliente, puntos: int): void
  + Cargar(ruta: string): string
```

```
ServicioMovimiento
  - movimientos: List<Movimiento>
  + EventoMovimiento: EventoMovimiento
  + ServicioMovimiento()
  + RegistrarMovimiento(movimiento: Movimiento): void
  + ObtenerMovimientos(): List<Movimiento>
```

```
ServicioProducto
  - productos: List<Producto>
  + EventoStock: EventoStockMinimo
  + EventoVencimiento: EventoVencimiento
  + ServicioProducto()
  + AgregarProducto(producto: Producto): string
  + ObtenerProductos(): List<Producto>
  + VerificarStock(): void
  + VerificarVencimiento(): void
  + CargarDesdeArchivo(ruta: string): string
```

```
ServicioUsuario
  - usuarios: List<Usuario>
  + ServicioUsuario()
  + AgregarUsuario(usuario: Usuario): void
  + Login(user: string, password: string): bool
  + Cargar(ruta: string): string
```

> **Nota de evidencia:** los cuatro constructores de arriba son **parameterless** en código
> ([ServicioCliente.cs:18](BibFarmacia/Servicios/ServicioCliente.cs#L18), [ServicioMovimiento.cs:17](BibFarmacia/Servicios/ServicioMovimiento.cs#L17),
> [ServicioProducto.cs:19](BibFarmacia/Servicios/ServicioProducto.cs#L19), [ServicioUsuario.cs:16](BibFarmacia/Servicios/ServicioUsuario.cs#L16)):
> cada servicio crea internamente su propia `List<T>` y su(s) propio(s) objeto(s) `Evento*` con `new`. Ninguno recibe esas
> dependencias por parámetro — no hay inyección de dependencias en la capa de servicios hoy. Esto es relevante para el
> mapa de dependencias (B.4) y para cualquier ADR de DIP en la Fase 3.

### Aspectos (transversales)

```
<<static>> AspectoValidacion
  + ValidarCliente(cliente: Cliente): string     {static}
  + ValidarProducto(producto: Producto): string  {static}
```

```
<<static>> AspectoAutenticacion
  + Login(usuarios: List<Usuario>, user: string, password: string): bool  {static}
```

### Eventos (delegados y eventos de C#)

```
EventoMovimiento
  + void DelegadoMovimiento(string mensaje): delegate
  + MovimientoRegistrado: event DelegadoMovimiento
  + Disparar(tipo: string): void
```

```
EventoPuntos
  + void DelegadoPuntos(string mensaje): delegate
  + PuntosAcumulados: event DelegadoPuntos
  + Disparar(cliente: string, puntos: int): void
```

```
EventoStockMinimo
  + void DelegadoStock(string mensaje): delegate
  + StockMinimo: event DelegadoStock
  + Disparar(producto: Producto): void
```

```
EventoVencimiento
  + void DelegadoVencimiento(string mensaje): delegate
  + Vencimiento: event DelegadoVencimiento
  + Disparar(producto: Producto): void
```

> Los cuatro `Evento*` se modelan aquí como clase con un `delegate` y un `event` propios (así están declarados en código,
> p. ej. [EventoMovimiento.cs:11-15](BibFarmacia/Eventos/EventoMovimiento.cs#L11-L15)). Es una decisión de notación,
> no un defecto: Dia no tiene un estereotipo `<<delegate>>` nativo para el plugin UML usado.

### Interfaces y sus implementaciones

```
<<Interface>> IServicioNotificacion
  ~ EnviarNotificacion(mensaje: string): void
```

```
ServicioNotificacion
  + EnviarNotificacion(mensaje: string): void
```

```
<<Interface>> IDescuento
  ~ CalcularDescuento(precio: decimal): decimal
```

```
ServicioDescuento
  + CalcularDescuento(precio: decimal): decimal
```

### Fábrica

```
<<static>> ProductoFactory
  + CrearCapsula(nombre: string, precio: decimal, stock: int,
                 laboratorio: Laboratorio): MedicamentoCapsula   {static}
  + CrearLiquido(nombre: string, precio: decimal, stock: int,
                 laboratorio: Laboratorio): MedicamentoLiquido   {static}
```

> **Nota de evidencia:** `stockMinimo` y `fechaVencimiento` no son parámetros de estos métodos; quedan fijos **dentro**
> del método: `CrearCapsula` usa `stockMinimo = 5` y vencimiento `DateTime.Now.AddMonths(6)`; `CrearLiquido` usa
> `stockMinimo = 5`, vencimiento `DateTime.Now.AddMonths(12)`, `MaterialEnvase.Vidrio` y `120` mililitros
> ([ProductoFactory.cs:13-43](BibFarmacia/Factories/ProductoFactory.cs#L13-L43)). Es una regla de negocio implícita
> que la restricción dura (A.1.1) obliga a preservar si la fábrica se refactoriza.

## B.3. Relaciones (verificadas contra código)

### B.3.1. Generalizaciones (5)

| Subclase | Superclase | Etiqueta |
|---|---|---|
| `Usuario` | `Persona` | hereda |
| `Cliente` | `Persona` | hereda |
| `Medicamento` | `Producto` | hereda |
| `MedicamentoCapsula` | `Medicamento` | hereda |
| `MedicamentoLiquido` | `Medicamento` | hereda |

Jerarquías resultantes:

```
Persona (abstract)
 ├── Usuario
 └── Cliente

Producto (abstract)
 └── Medicamento
      ├── MedicamentoCapsula
      └── MedicamentoLiquido
```

### B.3.2. Realizaciones (2)

| Implementación | Interfaz | Etiqueta |
|---|---|---|
| `ServicioNotificacion` | `IServicioNotificacion` | implementa |
| `ServicioDescuento` | `IDescuento` | implementa |

### B.3.3. Asociaciones (12), con multiplicidad y rol derivados del código

| Origen | Destino | Multiplicidad | Rol (nombre del campo en código) | Etiqueta |
|---|---|---|---|---|
| `MedicamentoCapsula` | `TipoRelleno` | 1 — 1 | `TipoRelleno` | tiene |
| `MedicamentoLiquido` | `MaterialEnvase` | 1 — 1 | `MaterialEnvase` | tiene |
| `Medicamento` | `Laboratorio` | 1 — 1 | `Laboratorio` | tiene |
| `Movimiento` | `Producto` | 1 — 1 | `Producto` | tiene |
| `ServicioCliente` | `Cliente` | 1 — 0..* | `clientes` | tiene |
| `ServicioMovimiento` | `Movimiento` | 1 — 0..* | `movimientos` | tiene |
| `ServicioProducto` | `Producto` | 1 — 0..* | `productos` | tiene |
| `ServicioUsuario` | `Usuario` | 1 — 0..* | `usuarios` | tiene |
| `ServicioMovimiento` | `EventoMovimiento` | 1 — 1 | `EventoMovimiento` | tiene |
| `ServicioCliente` | `EventoPuntos` | 1 — 1 | `EventoPuntos` | tiene |
| `ServicioProducto` | `EventoStockMinimo` | 1 — 1 | `EventoStock` | tiene |
| `ServicioProducto` | `EventoVencimiento` | 1 — 1 | `EventoVencimiento` | tiene |

> **Corrección de dirección:** las dos últimas filas (`ServicioProducto → EventoStockMinimo` / `EventoVencimiento`) antes
> apuntaban al revés. `ServicioProducto` es quien declara los campos `EventoStock` y `EventoVencimiento`
> ([ServicioProducto.cs:16-17](BibFarmacia/Servicios/ServicioProducto.cs#L16-L17)), así que la flecha de posesión
> sale de `ServicioProducto`, igual que en `ServicioMovimiento → EventoMovimiento` y `ServicioCliente → EventoPuntos`.
> La multiplicidad `0..*` en las colecciones (`clientes`, `movimientos`, `productos`, `usuarios`) refleja que se
> inicializan como `new List<T>()` vacía — no hay garantía de al menos un elemento.

### B.3.4. Dependencias (6)

| Origen | Destino | Etiqueta |
|---|---|---|
| `ServicioUsuario` | `AspectoAutenticacion` | depende |
| `AspectoValidacion` | `Cliente` | depende |
| `AspectoValidacion` | `Producto` | depende |
| `ProductoFactory` | `MedicamentoCapsula` | crea |
| `ProductoFactory` | `MedicamentoLiquido` | crea |
| `ProductoFactory` | `Laboratorio` | usa (parámetro) |

> **Dos correcciones respecto al listado original:**
> 1. Se **eliminaron** las cuatro dependencias `ServicioUsuario→Usuario`, `ServicioCliente→Cliente`,
>    `ServicioMovimiento→Movimiento`, `ServicioProducto→Producto`: duplicaban la asociación ya declarada en B.3.3 (cada
>    servicio guarda una `List<T>` de ese tipo como **atributo**, así que es una asociación/agregación, no una
>    dependencia adicional).
> 2. Se **invirtió el sentido** de `Producto/Cliente → AspectoValidacion`. Es `AspectoValidacion` quien conoce a
>    `Producto` y `Cliente` como tipos de parámetro de sus métodos `ValidarProducto`/`ValidarCliente`
>    ([AspectoValidacion.cs:13-44](BibFarmacia/Aspectos/AspectoValidacion.cs#L13-L44)); ni `Producto` ni `Cliente`
>    tienen referencia alguna a `AspectoValidacion`. Además, **ninguna clase del sistema llama a `AspectoValidacion`**
>    (confirmado por búsqueda global en el código: solo aparece en su propio archivo) — la dependencia existe en el
>    código pero no se ejercita en ningún flujo de ejecución actual. Ver B.6.

## B.4. Mapa de dependencias derivado del diagrama actual

**Alto nivel declarado (reglas de negocio):** `Producto` y su jerarquía, `Persona` y su jerarquía, `Movimiento`.
**Bajo nivel / detalles técnicos:** `ProductoFactory`, `ServicioNotificacion`, la carga desde archivo (`Cargar`, `CargarDesdeArchivo`), los `Evento*`, los `Aspecto*`.
**Intermedio (orquestación):** los cuatro `Servicio*`.

Sentido real de las flechas hoy:

```
ServicioUsuario   ──► Usuario             (concreta, agregación 1—0..*)
ServicioUsuario   ──► AspectoAutenticacion (clase estática concreta)
ServicioCliente   ──► Cliente             (concreta, agregación 1—0..*)
ServicioCliente   ──► EventoPuntos        (concreta, composición 1—1)
ServicioMovimiento──► Movimiento          (concreta, agregación 1—0..*)
ServicioMovimiento──► EventoMovimiento    (concreta, composición 1—1)
ServicioProducto  ──► Producto            (abstracta, agregación 1—0..*)
ServicioProducto  ──► EventoStockMinimo   (concreta, composición 1—1)
ServicioProducto  ──► EventoVencimiento   (concreta, composición 1—1)
ProductoFactory   ──► MedicamentoCapsula, MedicamentoLiquido, Laboratorio (concretas)
Movimiento        ──► Producto            (abstracta)
Medicamento       ──► Laboratorio         (concreta)
AspectoValidacion ──► Producto, Cliente   (concretas — sin ningún llamador en el código)
```

**Puntos donde HOY existe inversión de dependencia:** prácticamente ninguno efectivo.
Existen dos interfaces (`IServicioNotificacion`, `IDescuento`), pero **ninguna clase del sistema las consume**: no hay
una sola instancia de `ServicioDescuento` ni de `ServicioNotificacion` creada en `Program.cs`, y ningún `Servicio*`
depende de esas interfaces. Las abstracciones están declaradas pero **no están cableadas a nada**. La única dependencia
que apunta a una abstracción real es la que `ServicioProducto` y `Movimiento` tienen sobre `Producto` (clase abstracta,
no interfaz).

**No existe composition root modelado en `BibFarmacia`:** toda la instanciación y el cableado de eventos ocurre en
`AppFarmaciaConsola/Program.cs`, fuera de la librería de dominio — es el composition root real del sistema, pero no
aparece como clase en este diagrama porque el diagrama modela `BibFarmacia`. Debe ubicarse explícitamente en el TO-BE
(requisito del criterio 3).

## B.5. Comportamiento observable, confirmado contra el código fuente

Capacidades observables, ya verificadas (no solo inferidas) contra `BibFarmacia` y `AppFarmaciaConsola/Program.cs`:

1. **Autenticación de usuarios** (`ServicioUsuario.Login` delega en `AspectoAutenticacion.Login`).
2. **Registro y consulta de clientes** (`ServicioCliente.AgregarCliente / ObtenerClientes`).
3. **Acumulación de puntos por cliente** (`ServicioCliente.AcumularPuntos` + `EventoPuntos`; `Cliente.AcumularPuntos`
   existe pero no se usa — ver nota en B.2).
4. **Registro y consulta de productos** (`ServicioProducto.AgregarProducto / ObtenerProductos`), con tres tipos de
   producto concretos: medicamento genérico, cápsula y líquido.
5. **Alerta de stock mínimo** (`ServicioProducto.VerificarStock` + `EventoStockMinimo`).
6. **Alerta de vencimiento** (`ServicioProducto.VerificarVencimiento` + `EventoVencimiento`).
7. **Registro de movimientos de inventario** con fecha, cantidad, tipo y producto (`ServicioMovimiento`, `EventoMovimiento`).
8. **Carga de datos desde archivo** (`ServicioUsuario.Cargar`, `ServicioCliente.Cargar`, `ServicioProducto.CargarDesdeArchivo`),
   las tres con la misma firma `(ruta: string): string`.
9. **Validación de cliente y de producto** (`AspectoValidacion.ValidarCliente/ValidarProducto`) — **sin ningún llamador
   en el sistema actual**, confirmado por búsqueda global.
10. **Cálculo de un descuento** (`ServicioDescuento.CalcularDescuento`) — sin consumidor visible en el diagrama ni en `Program.cs`.
11. **Envío de una notificación** (`ServicioNotificacion.EnviarNotificacion`) — sin consumidor visible en el diagrama ni en `Program.cs`.
12. **Creación de productos vía fábrica** (`ProductoFactory.CrearCapsula / CrearLiquido`).

> **Confirmado contra el código:** la operación de **venta** (opción 4 del menú en `Program.cs`) existe, pero es lógica
> de consola directa — descuenta `producto.Stock -= cantidad` y crea un `Movimiento` en línea, sin una clase `Venta`,
> `Factura`, `Pedido` ni `MedioDePago` en `BibFarmacia`. El diagrama AS-IS de este documento **no la modela** porque
> vive fuera de `BibFarmacia`, en `Program.cs`. Este punto es relevante para SC-3 (crédito y descuentos por convenio):
> no hay hoy dónde enganchar esa regla, hay que diseñarla desde cero en el TO-BE.

## B.6. Nota metodológica de corrección (reemplaza a la antigua Parte C)

Esta sección documenta, de forma trazable, qué cambió entre el `.dia` original y la Parte B de arriba, y por qué. Sustituye
al listado de defectos que antes vivía en una Parte C separada: cada punto ya quedó corregido en línea en B.1–B.5; aquí
solo se deja el resumen con su evidencia, para que el equipo pueda defenderlo en el video sin tener que releer el `.dia`.

**Errores de transcripción del `.dia` corregidos (el código tenía razón, el diagrama no):**

- Constructores de `ServicioCliente`, `ServicioMovimiento` y `ServicioProducto` dibujados con parámetros que no existen
  (son parameterless) — ver nota en B.2, evidencia en [ServicioCliente.cs:18](BibFarmacia/Servicios/ServicioCliente.cs#L18),
  [ServicioMovimiento.cs:17](BibFarmacia/Servicios/ServicioMovimiento.cs#L17), [ServicioProducto.cs:19](BibFarmacia/Servicios/ServicioProducto.cs#L19).
- `ServicioProducto.CargarDesdeArchivo` dibujado sin el parámetro `ruta: string` que sí tiene en código
  ([ServicioProducto.cs:75](BibFarmacia/Servicios/ServicioProducto.cs#L75)).
- `AspectoAutenticacion.Login` dibujado con segundo parámetro `usuario: Usuario`; en código es `user: string`
  ([AspectoAutenticacion.cs:13](BibFarmacia/Aspectos/AspectoAutenticacion.cs#L13)).
- Clase `laboratorio` en minúscula; en código es `Laboratorio` ([Laboratorio.cs:9](BibFarmacia/Clases/Laboratorio.cs#L9)).
- Constructor de `MedicamentoLiquido` etiquetado `Medicamento(...)`; en código es `MedicamentoLiquido(...)`
  ([MedicamentoLiquido.cs:16](BibFarmacia/Clases/MedicamentoLiquido.cs#L16)).
- `MedicamentoLiquido` marcada `{abstract}` sin serlo — corregido en B.1.
- `ServicioUsuario` con constructor mal escrito (`SevicioUsuario`) — corregido a `ServicioUsuario()`.
- Typos de tipo genérico: `List<Clientes>` → `List<Cliente>`, `List<Productos>` → `List<Producto>`.
- `EventoStockMinimo.Disparar` con nombre y tipo de parámetro invertidos — corregido a `(producto: Producto)`.
- Asociaciones `EventoStockMinimo/EventoVencimiento → ServicioProducto` con dirección invertida respecto a los campos
  reales de `ServicioProducto` — corregido en B.3.3.
- Dependencia `Producto/Cliente → AspectoValidacion` con dirección invertida — corregido en B.3.4.
- Dependencias duplicadas (asociación + dependencia entre el mismo par de clases) — se dejó solo la asociación, que es
  la que corresponde a un campo `List<T>` en código.
- Ninguna operación marcada `{virtual}`/`{static}` pese a que `Producto.MostrarInformacion()` es `virtual` en código y
  las tres clases `<<static>>` son `static class` — corregido en B.1 y B.2.
- Atributos heredados redeclarados en cada subclase (`nombre`, `precio`, `stock`... repetidos en `Medicamento`,
  `MedicamentoCapsula` y `MedicamentoLiquido`; `nombre/cedula/telefono/correo` repetidos en `Usuario` y `Cliente`) sin
  que el código los redeclare — corregido: cada clase muestra solo lo que declara.

**Una sugerencia quedó REFUTADA (exigencia A.4.2):** el `.dia` original parecía no tener multiplicidad en ninguna de
sus 12 asociaciones. Verificado directamente en el XML del archivo (`multipicity_a` / `multipicity_b` en cada objeto
`UML - Association`), **10 de las 12 sí tenían multiplicidad declarada** (`1`/`1` o `1`/`n`); solo faltaba en
`ServicioUsuario—Usuario` y `MedicamentoLiquido—MaterialEnvase`. Lo que sí estaba vacío en las 12 era el **rol** en los
extremos (`role_a`/`role_b`), no la multiplicidad. Se descarta tratar "cero multiplicidades" como hallazgo: es
parcialmente falso y habría sido defendido con un argumento incorrecto frente al comité.

**Hallazgos reales, confirmados contra código y conservados como evidencia para el inventario de la Fase 1 (A.4.1):**

- `Cliente.AcumularPuntos(puntos)` es código muerto; `ServicioCliente.AcumularPuntos` duplica la regla manipulando
  `cliente.Puntos` directamente ([ServicioCliente.cs:40](BibFarmacia/Servicios/ServicioCliente.cs#L40) vs.
  [Cliente.cs:20-23](BibFarmacia/Clases/Cliente.cs#L20-L23)).
- `ProductoFactory.CrearCapsula`/`CrearLiquido` fijan `stockMinimo`, fecha de vencimiento, `TipoRelleno`/`MaterialEnvase`
  y mililitros por dentro, sin parámetro — regla de negocio oculta ([ProductoFactory.cs:13-43](BibFarmacia/Factories/ProductoFactory.cs#L13-L43)).
- `IDescuento` e `IServicioNotificacion` no tienen consumidor en todo el sistema — abstracciones declaradas pero no
  cableadas (confirmado: `ServicioDescuento` y `ServicioNotificacion` no se instancian en `Program.cs`).
- `AspectoValidacion` no tiene ningún llamador en todo el sistema — confirmado por búsqueda global de texto.
- La persistencia (archivo plano) está incrustada directamente en `ServicioCliente`, `ServicioUsuario` y
  `ServicioProducto` (métodos `Cargar`/`CargarDesdeArchivo`); no existe una abstracción de repositorio.
- No existe entidad `Venta`/`Factura`/`Pedido`/`MedioDePago`: la venta vive como lógica de consola en `Program.cs`
  (opción 4 del menú), fuera de `BibFarmacia`.

Estos seis puntos son candidatos directos a filas del inventario de hallazgos (A.4.1) con columna "Propio" o "Asistido"
según cómo cada equipo llegó a ellos — este documento deja la evidencia archivo:línea, no decide la clasificación por
el equipo (esa decisión es, precisamente, lo que pide A.1.2 que el equipo no delegue).

---

# PARTE C — Instrucciones operativas para el agente

1. **No inventes ubicaciones de código.** Todo hallazgo del inventario exige **archivo y línea reales**. Si no tienes el fuente, marca la celda como `PENDIENTE-VERIFICAR` en vez de rellenarla.
2. **Distingue siempre `Propio` de `Asistido`** en la columna de origen. Todo lo que produzcas tú es `Asistido` por definición. El equipo necesita **al menos tres `Propio`** y debe poder demostrar cómo llegó a ellos.
3. **Produce al menos una refutación defendible**: identifica sugerencias que suenan a buenas prácticas pero que en este sistema **no aplican** o serían sobre-ingeniería, y argumenta por qué. Esto es exigencia explícita (A.4.2) y peso del criterio 6.
4. **La restricción dura manda:** ninguna propuesta puede alterar salidas, reglas ni cálculos observables. Si una refactorización cambiaría una salida (aunque sea el formato de un mensaje impreso), **decláralo y descártala**.
5. **Nada de listas declarativas.** Cada afirmación arquitectónica debe venir con: evidencia → alternativa descartada → costo aceptado. Si no puedes construir esa cadena, no lo propongas.
6. **Fase 0 no es delegable.** No generes las hojas de lectura en frío.
7. **Cuando propongas TO-BE, verifica cada herencia contra LSP explícitamente** (precondiciones, postcondiciones, invariantes, excepciones) y responde siempre "por qué herencia y no composición".
8. **Cada inversión de dependencia debe nombrar los cuatro elementos**: módulo de alto nivel, módulo de bajo nivel, abstracción desacoplante y punto de resolución (composition root).
9. **Declara un límite al rediseño.** La rúbrica premia explícitamente un **límite bien fundamentado para evitar sobre-ingeniería** y penaliza el rediseño total. Debe existir una lista de "cosas que decidimos NO tocar y por qué".
10. **Toda interacción relevante contigo debe quedar en la bitácora** con las tres columnas: qué propusiste, qué decidió el equipo (aceptado/corregido/rechazado), con qué argumento.
