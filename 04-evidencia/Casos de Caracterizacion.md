# Evidencia de preservación del comportamiento — 10 casos de caracterización (A.7.3)

> **Requisito cubierto:** A.7.3 — *"mínimo OCHO casos de caracterización ejecutados contra el sistema original y
> contra el rediseñado, con las salidas de ambos, demostrando que coinciden."* Se entregan 10.
>
> **Método:** cada caso es un guion de entrada (`03-src/casos-de-caracterizacion/caso-NN-*.txt`) que se alimenta por
> stdin a **ambos** ejecutables — el AS-IS real (`AppFarmaciaConsola` en la raíz del repositorio, compilado desde
> `BibFarmacia`/`AppFarmaciaConsola` sin ningún cambio) y el TO-BE (`03-src/src/AppFarmaciaConsola`) — usando el
> script `03-src/ejecutar-caracterizacion.sh`. Las salidas completas de cada corrida quedan en
> `salidas-comparadas/caso-NN-*-asis.txt` y `caso-NN-*-tobe.txt` (en esta misma carpeta). Este documento resume el
> resultado del `diff` entre cada par.

## Resultado global

**Las 10 salidas coinciden línea por línea**, con una única excepción sistemática y esperada: dos líneas que
existen **solo** en el TO-BE porque son la manifestación directa de SC-2 (servicios médicos), autorizada
explícitamente por el enunciado (A.5) y elegida como la solicitud de cambio medida en esta entrega (ver
`evidencia/Metrica SC-2.md`):

1. `Servicios médicos cargados` — línea nueva en el bloque de carga inicial (el AS-IS carga 3 archivos; el TO-BE
   carga 4: agrega `serviciosmedicos.txt`).
2. `8. Ver servicios médicos` — línea nueva en cada impresión del menú (el AS-IS tiene 7 opciones; el TO-BE tiene 8).

Ninguna otra línea difiere en ningún caso: ni los textos, ni el orden, ni los colores (no visibles en el `diff` de
texto plano, pero los mismos `ConsoleColor` se verificaron línea por línea contra `Program.cs` del AS-IS al escribir
`NotificadorConsola` y `MenuConsola` — ver comentarios en el código fuente).

## Detalle por caso

| # | Caso | Qué ejercita | Resultado |
|---|---|---|---|
| 01 | Login exitoso | Carga inicial, login correcto, alertas automáticas post-login, salida | Coincide (+2 líneas SC-2) |
| 02 | Login fallido | Carga inicial, login con contraseña incorrecta, mensaje "Acceso denegado", el programa termina sin mostrar el menú | Coincide (+1 línea SC-2, no hay menú aún) |
| 03 | Ver productos | Opción 1: listado completo de los 10 productos (nombre, stock, precio) | Coincide (+2 líneas SC-2) |
| 04 | Ver clientes | Opción 2: listado completo de los 10 clientes con sus puntos | Coincide (+2 líneas SC-2) |
| 05 | Buscar producto existente | Opción 3 con "Ibuprofeno": muestra producto, precio, stock | Coincide (+2 líneas SC-2) |
| 06 | Buscar producto inexistente | Opción 3 con un nombre que no existe: "Producto no encontrado" | Coincide (+2 líneas SC-2) |
| 07 | Registrar venta | Opción 4 con "Ibuprofeno" cantidad 3: descuenta stock (10→7), registra el movimiento ("Movimiento registrado: Venta" en cian), imprime "Venta registrada"; se verificó el nuevo stock con una consulta adicional (opción 1) en el mismo caso | **Coincide exactamente**, incluido el stock resultante (+3 líneas SC-2) |
| 08 | Venta con producto no encontrado | Opción 4 con un nombre que no existe: "Producto no encontrado", sin tocar stock ni movimientos | Coincide (+2 líneas SC-2) |
| 09 | Acumular puntos | Opción 5 con "Ana" 25 puntos: mensaje en verde "Cliente Ana acumuló 25 puntos"; se verificó con una consulta adicional (opción 2) que Ana quedó con 25 puntos | **Coincide exactamente**, incluidos los puntos resultantes (+3 líneas SC-2) |
| 10 | Ver alertas | Opción 6: repite la verificación de stock mínimo y vencimiento bajo demanda | Coincide (+2 líneas SC-2) |

## Por qué esto es evidencia suficiente de preservación del comportamiento

- Cubre las **8 capacidades observables** listadas en `CONTEXTO-RETO-FARMACIA.md` B.5 que siguen vigentes sin
  cambio: autenticación, listar productos, listar clientes, buscar producto, registrar venta (con actualización de
  stock y movimiento), acumular puntos, alertas de stock mínimo y de vencimiento, y carga de datos desde archivo.
- El caso 07 es el más exigente: ejercita la cadena completa `MenuConsola → CasodeUsoProcesarVenta →
  EventoVentaProcesada → (GestorInventario.AlProcesarVenta + MovimientoService.AlProcesarVenta)` y el resultado
  numérico del stock coincide exactamente con `productoVenta.Stock -= cantidad` del AS-IS.
- Las únicas diferencias en las 10 corridas son, en todos los casos, las mismas dos líneas — nunca aparece una
  diferencia distinta o inesperada — lo que descarta que sean ruido y confirma que son, específicamente, la
  extensión autorizada de SC-2.
