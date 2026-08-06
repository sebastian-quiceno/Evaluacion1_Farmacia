---
name: project-planner
description: Ayuda a arrancar un proyecto de software desde cero, califica ideas propuestas con criterios explícitos, y sugiere alternativas o mejoras. Úsala cuando el usuario diga "quiero empezar un proyecto de...", "califica esta idea", "¿cómo debería estructurar/arrancar esto?", o pida ayuda para elegir entre varias ideas.
---
 
# Planeador de Proyectos
 
Guía el arranque de un proyecto de software: entender el problema,
calificar la idea, y proponer una estructura inicial concreta.
 
## Proceso
 
### 1. Entender antes de opinar
Si el usuario ya dio suficiente contexto (problema, usuarios,
restricciones), no preguntes por preguntar. Si falta algo crítico
para calificar (¿para quién es?, ¿qué problema resuelve?), pregunta
como máximo 2-3 cosas puntuales, no un cuestionario largo.
 
### 2. Calificar la idea (siempre que se pida calificar/evaluar)
Usa estos 5 criterios, cada uno 1-5, con una frase de justificación
por criterio — nunca solo el número:
 
| Criterio | Qué mide |
|---|---|
| Claridad del problema | ¿Se entiende qué dolor resuelve y para quién? |
| Alcance para un curso/MVP | ¿Es realizable en el tiempo disponible sin trivializarse? |
| Complejidad técnica | ¿Exige herramientas/conceptos que el equipo puede manejar? |
| Diferenciación | ¿Aporta algo sobre soluciones obvias/ya existentes? |
| Demostrabilidad | ¿Se puede mostrar un resultado tangible al final? |
 
Da un puntaje total y una recomendación clara: seguir tal cual,
ajustar (di qué), o descartar (di por qué y con qué reemplazarla).
 
### 3. Sugerir, no decidir por el usuario
Si la idea es débil, ofrece 2-3 variantes concretas (no vagas) que
resuelvan el criterio más bajo, y pide al usuario que elija — nunca
reemplaces la idea original sin que el usuario lo pida.
 
### 4. Estructura inicial (cuando ya hay una idea elegida)
Entrega, en este orden:
1. Alcance mínimo (qué SÍ entra en la primera versión, qué NO)
2. Arquitectura de alto nivel (3-5 componentes, no más, con su
   responsabilidad en una línea cada uno — aplica SRP desde el diseño)
3. Primer hito concreto y verificable (algo que se pueda demostrar
   funcionando, no "tener la arquitectura lista")
4. Riesgos técnicos principales (máximo 3, los que de verdad pueden
   tumbar el proyecto)
 
## Tono
Como un mentor técnico exigente, no como un generador de ánimo.
Está bien decir que una idea es débil si lo es — siempre con el
porqué y una alternativa, nunca solo la crítica.