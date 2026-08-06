# Original User Request

## 2026-08-05T15:52:40Z

<USER_REQUEST>
Diagnóstico arquitectónico SOLID (Fase 1 — AS-IS) de un sistema de farmacia heredado en C# .NET 8, con un agente especialista por cada principio SOLID y un integrador que consolide los hallazgos.

Working directory: `c:\Users\tomax\OneDrive\Documentos\Universidad\6to Semestre\Arquitectura de software\SolucionFarmacia\SolucionFarmacia`

Integrity mode: development

---

## Contexto del proyecto

Este es un **reto técnico universitario** (Arquitectura de Software, 6to semestre). Se nos entregó un sistema de farmacia heredado en C# .NET 8 con fallas arquitectónicas que debemos diagnosticar y luego rediseñar aplicando SOLID. El sistema tiene dos proyectos:

- **BibFarmacia** — Librería de clases (26 archivos .cs): clases de dominio, servicios, eventos, factories, interfaces, aspectos
- **AppFarmaciaConsola** — Aplicación de consola (Program.cs, 378 líneas): login, menú, ventas, puntos, alertas

### Estructura de archivos fuente (solo leer, NO modificar)

```
BibFarmacia/
├── Aspectos/        → AspectoAutenticacion.cs, AspectoValidacion.cs
├── Clases/          → Persona.cs, Cliente.cs, Usuario.cs, Laboratorio.cs,
│                      Producto.cs, Medicamento.cs, MedicamentoCapsula.cs,
│                      MedicamentoLiquido.cs, Movimiento.cs
├── Enums/           → MaterialEnvase.cs, TipoRelleno.cs
├── Eventos/         → EventoMovimiento.cs, EventoPuntos.cs,
│                      EventoStockMinimo.cs, EventoVencimiento.cs
├── Factories/       → ProductoFactory.cs
├── Interfaces/      → IDescuento.cs, IServicioNotificacion.cs
├── Servicios/       → ServicioCliente.cs, ServicioDescuento.cs,
│                      ServicioMovimiento.cs, ServicioNotificacion.cs,
│                      ServicioProducto.cs, ServicioUsuario.cs
├── Utilidades/      → (vacío)
AppFarmaciaConsola/
├── Program.cs
├── productos.txt, clientes.txt, usuarios.txt
```

### Jerarquía de clases actual
- `Persona` (abstract) → `Cliente`, `Usuario`
- `Producto` (abstract) → `Medicamento` → `MedicamentoCapsula`, `MedicamentoLiquido`
- `Laboratorio`, `Movimiento` (standalone)
- Interfaces: `IDescuento`, `IServicioNotificacion`
- Implementaciones: `ServicioDescuento : IDescuento`, `ServicioNotificacion : IServicioNotificacion`

### Solicitudes de cambio futuras (para evaluar extensibilidad)
- **SC-1**: La farmacia necesita vender no solo productos farmacéuticos sino también cosméticos, comestibles (gaseosas, agua, helados, snacks)
- **SC-2**: La farmacia también venderá servicios como: inyectología, cambio de vendajes, curaciones básicas
- **SC-3**: Manejar convenios con diferentes entidades (empresas, bancos, cooperativas, mutuales, universidades) para descuentos y crédito

---

## Metodología de análisis por agente (Skill: experto_solid)

Cada agente DEBE seguir este proceso obligatorio al analizar el código:

1. **Identificar las clases/módulos relevantes** en el código del proyecto
2. **Evaluar su principio SOLID asignado** rigurosamente:
   - **S — Single Responsibility**: ¿esta clase tiene más de una razón para cambiar? Nombrar las razones explícitamente
   - **O — Open/Closed**: ¿agregar un caso nuevo obliga a modificar código existente (if/else, switch por tipo)? Señalar la línea
   - **L — Liskov Substitution**: ¿alguna subclase lanza excepciones, ignora métodos heredados, o cambia el contrato esperado?
   - **I — Interface Segregation**: ¿hay interfaces/clases con métodos que algunos implementadores no usan o implementan vacíos?
   - **D — Dependency Inversion**: ¿las clases de alto nivel instancian directamente clases concretas de bajo nivel en vez de depender de una abstracción inyectada?
3. **Para cada violación encontrada**: citar el fragmento exacto del código, nombrar el principio violado, dar el fix mínimo
4. **Si no hay violaciones** en un aspecto, decirlo explícitamente — es evidencia de que sí se revisó
5. **Formato de salida**: tabla con columnas: Principio | ¿Cumple? | Evidencia (archivo/línea) | Fix sugerido

---

## Requirements

### R1. Análisis SOLID por principio (5 análisis, uno por principio)

Organizar el trabajo con un agente especialista por cada principio SOLID. Cada agente lee TODO el código fuente del proyecto (ambos proyectos: BibFarmacia y AppFarmaciaConsola) y produce un análisis exhaustivo desde la perspectiva de su principio asignado, siguiendo la metodología descrita arriba. Cada hallazgo debe ser **trazable**: archivo exacto, clase, y línea(s) del código.

### R2. Inventario consolidado de hallazgos

Compilar TODOS los hallazgos de los 5 agentes en una única tabla con este formato obligatorio:

| ID | Ubicación (archivo / clase / línea) | Síntoma observado | Principio comprometido | Impacto en el negocio | Severidad |
|----|------|------|------|------|------|
| H-01 | ... | ... | SRP/OCP/LSP/ISP/DIP | Traducido a costo, riesgo o tiempo de cambio | Alta / Media / Baja |

### R3. Diagrama UML de clases AS-IS

Generar un diagrama de clases en formato **Mermaid** que refleje fielmente el código actual — NO un diagrama idealizado. Debe incluir:
- Todas las clases e interfaces con sus atributos y métodos
- Relaciones de herencia, composición, dependencia y uso
- Multiplicidades reales
- Visibilidad de miembros (public, private, protected)

### R4. Mapa de dependencias

Documentar qué clase depende de cuáles otras, distinguiendo:
- Clases de **alto nivel** (reglas del negocio): Servicios
- Clases de **bajo nivel** (detalles técnicos): acceso a archivos, consola, factories
- Dónde se invierte la relación hoy y dónde NO se invierte

### R5. Los 3 puntos de dolor priorizados

Identificar los **tres problemas más críticos** del sistema con:
- Criterio de priorización explícito (por qué el #1 está antes que el #2)
- Para cada punto de dolor, evaluar contra las 3 solicitudes de cambio (SC-1, SC-2, SC-3):
  - Cuántas clases/archivos habría que modificar para implementarla hoy
  - Qué comportamiento existente correría riesgo de romperse

### R6. Entregable organizado

Todos los artefactos de salida se guardan en la carpeta `01-diagnostico/` dentro del working directory:
- `01-diagnostico/diagrama-as-is.md` — Diagrama UML Mermaid
- `01-diagnostico/inventario-hallazgos.md` — Tabla consolidada
- `01-diagnostico/mapa-dependencias.md` — Mapa de dependencias
- `01-diagnostico/puntos-dolor.md` — Los 3 puntos de dolor priorizados
- `01-diagnostico/analisis-srp.md` — Análisis detallado SRP
- `01-diagnostico/analisis-ocp.md` — Análisis detallado OCP
- `01-diagnostico/analisis-lsp.md` — Análisis detallado LSP
- `01-diagnostico/analisis-isp.md` — Análisis detallado ISP
- `01-diagnostico/analisis-dip.md` — Análisis detallado DIP

---

## Acceptance Criteria

### Completitud del análisis SOLID
- [ ] Los 5 principios SOLID han sido analizados individualmente contra TODO el código (26 archivos .cs + Program.cs)
- [ ] Cada análisis individual tiene al menos 3 hallazgos con referencia exacta a archivo y línea
- [ ] Los hallazgos que declaran "cumple" incluyen la evidencia de por qué cumple

### Inventario de hallazgos
- [ ] La tabla consolidada tiene al menos 15 hallazgos en total (mínimo 3 por principio)
- [ ] Cada hallazgo tiene las 6 columnas completas: ID, Ubicación, Síntoma, Principio, Impacto negocio, Severidad
- [ ] La columna "Impacto en el negocio" traduce el problema técnico a costo, riesgo o tiempo de cambio — no usa jerga técnica pura
- [ ] La columna "Ubicación" incluye archivo, clase Y número de línea

### Diagrama UML AS-IS
- [ ] El diagrama es Mermaid válido que renderiza correctamente
- [ ] Incluye TODAS las clases del proyecto (Persona, Cliente, Usuario, Laboratorio, Producto, Medicamento, MedicamentoCapsula, MedicamentoLiquido, Movimiento, y todas las interfaces y servicios)
- [ ] Muestra relaciones de herencia, composición y dependencia reales (no inventadas)
- [ ] Incluye atributos y métodos con visibilidad

### Mapa de dependencias
- [ ] Identifica claramente módulos de alto nivel vs bajo nivel
- [ ] Señala dónde hay inversión de dependencias y dónde falta
- [ ] Muestra las dependencias concretas (new, static calls, File.ReadAllLines, Console.WriteLine)

### Puntos de dolor
- [ ] Exactamente 3 puntos de dolor identificados y numerados
- [ ] Criterio de priorización explícito y defendible
- [ ] Cada punto incluye análisis de impacto contra las 3 solicitudes de cambio (SC-1, SC-2, SC-3)
- [ ] Para cada SC, se indica cuántas clases se tocarían y qué se rompería

### Organización de archivos
- [ ] Todos los archivos están en la carpeta `01-diagnostico/`
- [ ] Cada archivo de análisis individual (srp, ocp, lsp, isp, dip) existe y tiene contenido sustancial
</USER_REQUEST>
