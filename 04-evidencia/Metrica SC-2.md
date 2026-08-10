# Métrica de extensibilidad — SC-2 implementada 

> **Solicitud elegida:** SC-2 (servicios médicos: inyectología, cambio de vendajes, curaciones básicas). Elegida
> porque, de las tres, es la que se puede implementar **agregando exclusivamente clases nuevas** sin modificar el
> contrato de ninguna clase de dominio existente — la demostración más limpia de OCP disponible en este diseño (ver
> justificación completa al final del documento, sección "por qué SC-2 y no las otras dos").

## Línea base — arquitectura AS-IS (medida en Fase 2, `../01-diagnostico/Soluciones de cambio.docx`)

| Tipo | Archivo | Naturaleza del cambio |
|---|---|---|
| Modificado | `BibFarmacia/Clases/Producto.cs` | Adaptarla para que algo sin stock/stockMinimo/fechaVencimiento pueda ser facturable — cambia el contrato de una clase de la que ya heredan `Medicamento`, `MedicamentoCapsula` y `MedicamentoLiquido`. |
| Modificado | `AppFarmaciaConsola/Program.cs` | El flujo de venta (`productoVenta.Stock -= cantidad`, línea 280) asume que todo lo vendido tiene stock — hay que reescribirlo para admitir algo que no lo tenga. |
| Modificado | `BibFarmacia/Clases/Movimiento.cs` | El campo `Producto producto` debe generalizarse para aceptar también un servicio médico. |
| Creado | Una clase de servicio médico | — |
| Creado | Un servicio/gestor para administrarla | — |
| Creado | Una abstracción común tipo "Facturable" | — |

**Línea base: 3 clases creadas / 3 clases existentes modificadas**, y las 3 modificaciones tocan **lógica de negocio
ya en producción** compartida por los medicamentos que ya existen. Riesgo confirmado en su momento
(`Soluciones de cambio.docx`, SC-2): *"Romper el flujo crítico de venta por cambio de modelo en Movimiento y en la
construcción de la transacción."*

## Arquitectura TO-BE — esta entrega

| Tipo | Archivo | Naturaleza del cambio |
|---|---|---|
| Creado | `BibFarmacia/Dominio/ServicioMedico.cs` | Clase nueva; implementa únicamente `IVendible`. |
| Creado | `BibFarmacia/Servicios/GestorServiciosMedicos.cs` | Clase nueva; servicio independiente de `GestorInventario`. |
| Modificado (inserción pura) | `AppFarmaciaConsola/MenuConsola.cs` | Se agrega el `case 8` y el método `MostrarServiciosMedicos()`; **ninguna línea existente se cambia ni se borra**. |
| Modificado (inserción pura) | `AppFarmaciaConsola/Program.cs` (`ConstruirDependencias`) | Se agregan 2 líneas (construir `GestorServiciosMedicos`, cargar `serviciosmedicos.txt`); **ninguna línea existente se cambia**. |

**TO-BE: 2 clases creadas / 2 archivos con inserciones puras.** Cero clases del dominio (`ProductoBase`,
`Medicamento`, `MedicamentoCapsula`, `MedicamentoLiquido`, `ArticuloRetail`, `Cliente`, `Convenio`...) cambiaron su
contrato, sus campos o su lógica interna. `GestorInventario`, `CasodeUsoProcesarVenta` y `MovimientoService` — los
tres puntos por donde pasa toda venta — **no se tocaron en absoluto**: `ServicioMedico` participa en el flujo de
venta únicamente por implementar `IVendible`, la misma interfaz que ya usaban `LineaDeVenta` y `Movimiento`.

## Interpretación

La comparación cruda (3 creadas / 3 modificadas → 2 creadas / 2 modificadas) ya es favorable, pero la diferencia
real está en el **tipo** de modificación, no solo en el conteo:

- **Arquitectura vieja:** las 3 modificaciones tocan lógica de negocio compartida por clases que ya existen y ya
  están en uso (el contrato de `Producto`, el cálculo de stock en el flujo de venta, el tipo del campo en
  `Movimiento`) — riesgo real de romper el comportamiento de los medicamentos existentes.
- **Arquitectura nueva:** las 2 modificaciones son **inserciones puras** — una línea nueva en un constructor, un
  `case` nuevo en un `switch` — sin tocar ni una sola línea preexistente. Nada que ya funcionaba pudo dejar de
  funcionar por este cambio (y las 10 salidas comparadas en `Casos de Caracterizacion.md` lo confirman: cero
  diferencias fuera de las dos líneas que SC-2 agrega).

Esto es posible porque la abstracción que SC-2 necesitaba (`IVendible`, "lo mínimo que cualquier cosa facturable
necesita") **ya existía** en el diseño antes de decidir implementar SC-2 — fue una decisión de ISP tomada en la
Fase 3 por razones más generales, no una abstracción construida a la medida de esta solicitud. Esa es, precisamente,
la prueba empírica que pide: el costo de extender el sistema bajó porque la arquitectura ya estaba preparada,
no porque esta solicitud en particular fuera fácil por casualidad.

## Por qué SC-2 y no las otras dos (justificación de la elección)

- **SC-1** (nuevos tipos de producto) también habría sido limpia bajo esta arquitectura — `ArticuloRetail` ya existe
  como clase nueva bajo `ProductoBase`, sin tocar `Medicamento`/`MedicamentoCapsula`/`MedicamentoLiquido`. Se
  descartó como la elegida porque cargarla realmente desde `productos.txt` habría exigido extender el formato del
  archivo con una columna de tipo (para no repetir el error de H-12: sin esa columna, no hay dato real del que
  despachar), lo que mezcla la métrica de "clases creadas vs. modificadas" con una migración de formato de datos que
  no es comparable 1 a 1 con la línea base medida en Fase 2.
- **SC-3** (convenios) es la más rica en principios SOLID demostrados (ya documentada extensamente en
  `../02-diseno/Principios SOLID Argumentados.md`, `../02-diseno/Herencias y Verificacion LSP.md` y `../02-diseno/Inversion de Dependencias (DIP).md`),
  pero es la **menos pura** para esta métrica específica: además de crear la jerarquía `Convenio` (4 clases nuevas),
  obliga a modificar `Cliente` (agregar el campo `convenio` al constructor) y `ClienteService.Cargar` (asignar el
  convenio al cargar cada fila) — dos modificaciones reales sobre clases de dominio existentes, aunque acotadas y de
  bajo riesgo gracias al invariante de `SinConvenio`.
- **SC-2** es la única de las tres que se puede demostrar con **cero modificaciones a clases de dominio
  existentes** — por eso es la elegida para la métrica de este criterio y la única que se conecta al menú
  interactivo en esta entrega. `ArticuloRetail` y la jerarquía `Convenio` ya existen, compilan y participan
  correctamente del tipado (`ArticuloRetail` implementa `ProductoBase`/`IPerecedero`; `SinConvenio` ya se ejercita
  en cada venta de mostrador, ver `MenuConsola.RegistrarVenta`), satisfaciendo la fidelidad al diagrama TO-BE
   — pero `ConvenioUniversidad`/`ConvenioEmpresa` y la carga de `ArticuloRetail` desde archivo no están
  cableadas a un escenario interactivo en esta entrega, por ser un límite consciente de alcance (ver README).
