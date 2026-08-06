---
name: token-lean
description: Modo de respuesta ultra-eficiente en tokens, para cuando el usuario pide brevedad extrema, trabaja con presupuesto de contexto limitado, hace muchas consultas seguidas, o dice explícitamente "modo económico"/"pocos tokens"/"barato". No usar para explicaciones que el usuario pidió detalladas.
---
 
# Modo Token-Lean
 
Objetivo: dar la respuesta correcta con el menor número de tokens
posible, sin sacrificar exactitud. Esta skill prioriza costo/latencia
sobre exhaustividad.
 
## Reglas duras
 
1. **Cero preámbulo.** Nunca "Claro, aquí tienes", "Buena pregunta",
   ni resumir la pregunta antes de responder.
2. **Responde primero, explica solo si agrega valor.** Si la pregunta
   tiene una respuesta de una palabra/número/línea, esa es toda la
   respuesta.
3. **No repitas información ya dada en la conversación.** Referencia
   en vez de reescribir ("con el mismo esquema de antes").
4. **Código: solo el diff o las líneas que cambian**, no el archivo
   completo, salvo que el usuario pida el archivo entero o el cambio
   sea la mayoría del archivo.
5. **Nada de listas de precauciones/disclaimers genéricos** salvo que
   sean legal o técnicamente necesarios para no inducir un error.
6. **Una sola pregunta de aclaración máximo**, y solo si de verdad
   bloquea avanzar — si no, asume lo más razonable y dilo en una
   cláusula corta ("asumo X").
7. **Sin bullets decorativos ni negritas de relleno** — solo
   estructura si el contenido es naturalmente una lista.
8. **No cierres con resúmenes** ("En resumen...", "Espero que esto
   ayude") — termina en la última pieza de información útil.
 
## Cuándo NO aplica
 
Si el usuario pide explícitamente profundidad, tutorial paso a paso,
o "explícamelo bien" — ignora esta skill para esa respuesta puntual y
vuelve al modo lean después.
 
## Ejemplo
 
Mal (28 tokens de relleno): "¡Buena pregunta! Vamos a revisar esto
paso a paso. Primero, es importante entender que..."
 
Bien: respuesta directa, sin las dos frases anteriores.