Documento de Arquitectura y Rediseño: Solución Farmacia
1. Estructura general de la solución
La solución adoptará una Arquitectura Limpia (Clean Architecture) orientada al dominio, dividida en capas lógicas para aislar las reglas de negocio de los detalles de infraestructura (como bases de datos o interfaces de usuario).
La estructura general será:
•	Core / Dominio: Contendrá las entidades de negocio, interfaces de dominio y reglas fundamentales. Sin dependencias externas.
•	Casos de Uso / Aplicación: Orquestará los flujos de negocio (ej. realizar una venta, aplicar un convenio) utilizando las interfaces del dominio.
•	Infraestructura: Implementación de acceso a datos, servicios externos y repositorios.
•	Presentación / API: Punto de entrada de la aplicación (Controladores o UI).
2. Responsabilidades por Módulo
•	Módulo de Catálogo y Comercialización: Gestionar de forma agnóstica cualquier elemento vendible (SC-1 y SC-2).
•	Módulo de Inventario: Exclusivo para gestionar el stock físico (afecta a SC-1, pero no a SC-2).
•	Módulo de Ventas y Facturación: Calcular totales, procesar transacciones y emitir comprobantes.
•	Módulo de Fidelización y Convenios: Gestionar las alianzas, calcular descuentos y validar las reglas de crédito según la entidad (SC-3).
3. Nuevas clases necesarias
•	ServicioMedico: Representará servicios como inyectología o curaciones (SC-2).
•	ArticuloRetail: Para productos no farmacéuticos (SC-1).
•	Medicamento: Específico para productos con regulaciones (lotes, fechas de caducidad).
•	CalculadoraDeTotal: Únicamente encargada de aplicar reglas de suma, impuestos y descuentos.
•	GestorDeConvenios: Encargado de identificar a qué entidad pertenece un cliente.
•	ReglaDescuentoUniversidad, ReglaDescuentoBanco, etc.: Implementaciones específicas de cálculo de descuentos.
•	PagoCreditoInstitucional: Para manejar la venta a crédito mediante convenios (SC-3).
4. Las interfaces necesarias
•	IVendible (o ISellable): Interfaz unificadora para todo lo que se pueda facturar (Productos y Servicios). Expondrá propiedades como Precio, Codigo, y AplicaImpuesto.
•	IControlableEnInventario: Solo implementada por productos físicos (SC-1), ignorada por los servicios (SC-2).
•	IEstrategiaDescuento: Para englobar la lógica de los diferentes convenios institucionales (SC-3).
•	IMetodoDePago: Para abstraer el pago al contado frente al pago por crédito de convenio.
•	IRepositorioVentas y IRepositorioCatalogo: Para aislar el acceso a la base de datos.
5. Las clases abstractas necesarias
•	ProductoBase: Clase abstracta que implementa IVendible y centraliza propiedades comunes de los productos físicos (nombre, código de barras, fabricante), dejando que Medicamento o ArticuloRetail añadan sus especificidades.
6. Qué clases actuales permanecen
•	Cliente: Permanece, pero se le añadirá una relación al convenio si aplica.
•	Usuario / Empleado: Quienes operan el sistema.
•	Factura (o Recibo): Permanece como entidad de solo lectura que representa la transacción consolidada.
7. Qué clases deben eliminarse
•	Diseños monolíticos como GestorFarmacia o SistemaVentas (clases "Dios" que concentraban toda la lógica).
•	Enumeradores condicionales como TipoProductoEnum si se utilizaban para definir la lógica de negocio mediante sentencias switch gigantes.
8. Qué clases deben dividirse
•	La clase actual Venta debe dividirse. Actualmente suele mezclar: el carrito de compras, el procesamiento del pago, el descuento y la actualización de inventario. Se dividirá en: CestaDeCompra, ProcesadorDePago, y un servicio de aplicación ProcesarVentaUseCase.
•	La clase actual Producto asume que todo es un medicamento. Debe dejar de ser una clase concreta y convertirse en la abstracción ProductoBase o simplemente dividirse en los subtipos mencionados anteriormente.
9. Nuevas relaciones entre los componentes
•	Una CestaDeCompra ya no dependerá de Producto, sino de una lista de IVendible. Esto permite que un cliente pague en la misma cuenta unas pastillas, un helado (SC-1) y un servicio de inyectología (SC-2).
•	La Venta (o ProcesadorDePago) se relacionará a través de inyección de dependencias con IEstrategiaDescuento. El cálculo del precio final delegará la responsabilidad a este componente según el convenio del cliente (SC-3).
10. Patrones de Diseño a utilizar y por qué
•	Strategy (Estrategia): Vital para el SC-3. Permite intercambiar los algoritmos de descuentos (Banco, Universidad, Cooperativa) sin tocar la lógica de venta general, evitando bloques gigantes de if-else.
•	Factory Method (Método de Fábrica) / Abstract Factory: Para instanciar correctamente la tipología correcta de IVendible según su familia (Medicamento, Servicio, Miscelánea) desde la base de datos.
•	Decorator (Decorador): Para añadir comportamientos durante la venta (por ejemplo, recargos por embalaje especial o sumatoria de impuestos sin modificar el objeto original base).
 
Aplicación de Principios SOLID en el Nuevo Diseño
•	S - Single Responsibility Principle (SRP): La lógica de fijación de precios y descuentos se separa en CalculadoraDeTotal y las estrategias de descuento. El carrito no resta del inventario; eso lo hace el módulo de Inventario escuchando los eventos de la venta procesada.
•	O - Open/Closed Principle (OCP): Soportamos el SC-1, SC-2 y SC-3 sin alterar código fundamental. Para agregar el nuevo servicio de "Nebulización" (SC-2) o el convenio "Sindicato de Maestros" (SC-3), solo se crearán nuevas clases que implementen IVendible o IEstrategiaDescuento. El núcleo de ventas se mantiene intacto.
•	L - Liskov Substitution Principle (LSP): Al usar la abstracción IVendible, la caja registradora puede procesar cualquier objeto que la implemente (servicios médicos o dulces) de manera intercambiable sin que el sistema falle esperando "lotes" de un servicio médico.
•	I - Interface Segregation Principle (ISP): Separación de IVendible e IControlableEnInventario. A un servicio de inyectología no se le obligará a implementar métodos como ActualizarStock(), previniendo lanzar excepciones de "No Implementado".
•	D - Dependency Inversion Principle (DIP): Los Casos de Uso de la farmacia dependerán completamente de interfaces (IEstrategiaDescuento, IRepositorioVentas), en lugar de instanciar clases directamente (new BaseDeDatos() o new DescuentoUniversidad()). Esto facilita las pruebas unitarias y el recambio de tecnología.
 
Arquitectura Objetivo
Al finalizar la refactorización, el sistema quedará organizado como un conjunto de componentes desacoplados alrededor de un Dominio rico y puro.
En el centro del sistema residirán interfaces abstractas (ej. IVendible) y modelos de negocio. Alrededor de ellas, las reglas de aplicación orquestarán que, cuando un cajero registre un artículo, el sistema evalúe dinámicamente si es un producto físico o un servicio médico. Seguidamente, una fábrica instanciará la estrategia de cálculo correspondiente inyectando las lógicas de convenio del cliente. Si el pago procede con crédito corporativo, el sistema aplicará un descuento basado en la estrategia configurada.
Finalmente, la orquestación despachará un evento, provocando que solo los componentes que implementen la interfaz de inventario deduzcan el stock (ignorando servicios), para concluir almacenando la transacción usando controladores de infraestructura aislados que implementan los contratos de persistencia.
 
Impacto de la Implementación (Modificaciones y Riesgos)
Cuántas clases y archivos habría que modificar
Asumiendo la estructura típica de un monolito legado de este tipo:
•	Clases / Archivos a Modificar Profundamente: Aprox. 15 a 20 archivos núcleo (Modelos antiguos de Producto, Venta, Factura, Cliente y sus controladores/servicios asociados).
•	Clases / Archivos Nuevos a Crear: Entre 25 a 35 archivos nuevos (interfaces, divisiones por responsabilidad, clases de Estrategia para cada convenio, factories y tests unitarios).
Riesgos Técnicos (Comportamiento existente que podría romperse)
1.	Deducción de Inventario Aleatoria: Debido a que ahora los "Servicios" no tendrán inventario (SC-2), existe el riesgo de que la lógica heredada que itera sobre la factura intente buscar stock de servicios médicos y bloquee las ventas lanzando errores de "Producto sin stock".
2.	Reportes Históricos de Rentabilidad y Auditoría: Al cambiar el concepto de 'Producto' a una abstracción que incluye gaseosas y consultas, las consultas a base de datos de los reportes heredados que asumen que todo tiene un laboratorio o lote pueden romperse.
3.	Cuadres de Caja y Medios de Pago: Integrar los pagos a crédito (SC-3) modificará la forma en la que la caja registradora suma el efectivo del día. Podría generar descuadres contables en la UI existente si asume que toda venta fue un ingreso en efectivo.
4.	Integridad Referencial en DB: Eliminar o migrar el enumerador TipoProducto hacia tablas o estrategias polimórficas requerirá un script de migración de datos que podría generar datos huérfanos.
