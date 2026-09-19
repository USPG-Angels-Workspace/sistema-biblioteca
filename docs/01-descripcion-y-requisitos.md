# 1. Descripción del problema

## 1.1 Situación actual

Una biblioteca pequeña o universitaria administra su acervo, sus lectores y sus préstamos de forma manual: libros anotados en cuadernos o en hojas de cálculo sueltas, fichas de papel para cada préstamo y cálculo de multas "de memoria" al momento de la devolución.

En este esquema:

- El personal debe revisar a mano si un libro tiene ejemplares disponibles antes de prestarlo.
- Las fechas de devolución se calculan y recuerdan manualmente, por lo que los vencimientos se detectan tarde o no se detectan.
- Las multas se cobran de forma inconsistente (distinta tarifa según quién atienda) y no queda un registro confiable de cuáles siguen pendientes.
- Obtener un dato agregado (qué libros están atrasados, cuánto se debe en multas) exige revisar todas las fichas una por una.

## 1.2 Problema identificado

No existe un registro único, consistente y consultable que relacione **libros**, **usuarios**, **préstamos** y **multas**. Como consecuencia:

1. Se prestan libros que ya no tienen ejemplares disponibles o a usuarios que no deberían recibirlos (con multas pendientes, con libros atrasados o por encima de su límite).
2. Los vencimientos y las multas no se controlan de forma automática ni uniforme.
3. La información se pierde o se duplica, y generar reportes es lento y propenso a errores.

## 1.3 Propuesta de solución

Desarrollar un **sistema de gestión de biblioteca** en C# (.NET 8) con interfaz gráfica de escritorio que:

- Registra y administra libros, usuarios (lectores y bibliotecarios), préstamos, devoluciones y multas.
- Aplica automáticamente las reglas del servicio (límites por tipo de usuario, plazos, renovaciones máximas, tarifa de multa, bloqueos por multa o atraso).
- Guarda toda la información en **archivos JSON** para que los datos permanezcan al cerrar y volver a abrir el programa.
- Genera los reportes que necesita el personal y permite exportarlos a CSV.

El sistema se diseñó siguiendo el proceso **análisis → UML → clases → código → interfaz → archivos → pruebas**; la trazabilidad de cada requisito se documenta en el capítulo 9.

## 1.4 Objetivo general

Desarrollar una aplicación de escritorio en C# que permita gestionar el catálogo, los usuarios, los préstamos, las devoluciones y las multas de una biblioteca, aplicando análisis orientado a objetos y modelado UML, y almacenando la información en archivos JSON.

## 1.5 Objetivos específicos

1. Analizar el problema y documentar la situación actual, los requisitos funcionales y no funcionales, los actores y los casos de uso.
2. Modelar la solución con diagramas UML de casos de uso, clases, secuencia y actividad, que sirvan de base para la implementación.
3. Implementar el dominio con programación orientada a objetos: clases, encapsulamiento, constructores, propiedades, métodos, herencia, polimorfismo e interfaces.
4. Implementar la persistencia en archivos JSON, garantizando que la información se recupere al reiniciar la aplicación.
5. Construir una interfaz gráfica clara con menú principal, formularios de registro, consulta, edición, eliminación, mensajes de confirmación y error, y validación de datos.
6. Verificar el funcionamiento con pruebas automatizadas y con datos de prueba que cubran todos los estados del sistema.
7. Utilizar GitHub para el versionamiento del proyecto con commits pequeños, ordenados y descriptivos.

## 1.6 Alcance y limitaciones

### Alcance

El sistema cubre los cinco módulos definidos en el enunciado del proyecto:

| Módulo | Funcionalidad incluida |
|---|---|
| Gestión de libros | Registro, edición, búsqueda, clasificación por categoría y control de disponibilidad. |
| Gestión de usuarios | Registro de lectores y bibliotecarios, actualización, consulta y activación/desactivación. |
| Gestión de préstamos | Registro de préstamos, asignación de libros, fechas de devolución y renovaciones. |
| Devoluciones y multas | Registro de devoluciones, control de vencimientos, cálculo y cobro de multas. |
| Reportes | Libros disponibles, préstamos activos, libros atrasados, usuarios y multas; exportación a CSV. |

### Limitaciones

- **Un solo puesto de trabajo:** los archivos JSON se cargan en memoria al iniciar; el sistema no está diseñado para que dos instancias escriban a la vez sobre los mismos archivos.
- **Sin autenticación:** el programa lo opera el personal de la biblioteca; no hay inicio de sesión ni roles con permisos diferenciados.
- **Sin reservas de libros ni notificaciones** (correo o SMS) de vencimiento.
- **Un catálogo por título:** los ejemplares de un mismo libro se manejan como una cantidad (total y disponibles), no como copias individuales con código propio.
- **Multas con una única política** (tarifa lineal por día con tope); la política es intercambiable por código (interfaz `ICalculadoraMulta`), pero no es configurable desde la interfaz.
- **Consistencia entre archivos:** cada operación escribe los archivos afectados de forma secuencial; una falla de disco entre dos escrituras podría dejar los archivos desincronizados (no se implementan transacciones).
- **Moneda:** las multas se expresan en quetzales (Q).

---

# 2. Requisitos

## 2.1 Requisitos funcionales

| ID | Requisito | Módulo |
|---|---|---|
| RF-01 | Registrar libros con ISBN, título, autor, editorial, año de publicación, categoría y cantidad de ejemplares. | Libros |
| RF-02 | Editar los datos de un libro; la cantidad de ejemplares no puede ser menor que los ejemplares actualmente en préstamo. | Libros |
| RF-03 | Buscar libros por título, autor, editorial o ISBN, y filtrarlos por categoría y por disponibilidad. | Libros |
| RF-04 | Clasificar los libros en categorías (Ficción, No ficción, Ciencia, Tecnología, Historia, Arte, Infantil, Referencia, Otros). | Libros |
| RF-05 | Controlar la disponibilidad: los ejemplares disponibles se descuentan al prestar y se recuperan al devolver. | Libros |
| RF-06 | Eliminar libros que no tengan préstamos en su historial. | Libros |
| RF-07 | No permitir dos libros con el mismo ISBN (sin importar guiones o espacios). | Libros |
| RF-08 | Registrar lectores (con carnet) y bibliotecarios (con cargo), con nombre, DPI, correo y teléfono. | Usuarios |
| RF-09 | Actualizar y consultar los datos de un usuario; activarlo o desactivarlo. | Usuarios |
| RF-10 | Validar los datos del usuario: DPI de 13 dígitos, correo con formato válido, teléfono de 8 a 15 dígitos; DPI único y carnet único. | Usuarios |
| RF-11 | Eliminar usuarios sin préstamos ni multas en su historial; no permitir desactivar a un usuario con préstamos activos. | Usuarios |
| RF-12 | Registrar un préstamo asignando un libro disponible a un usuario activo. | Préstamos |
| RF-13 | Calcular automáticamente la fecha de devolución: 7 días para lectores y 14 días para bibliotecarios. | Préstamos |
| RF-14 | Limitar los préstamos simultáneos: 3 para lectores y 5 para bibliotecarios. | Préstamos |
| RF-15 | Impedir un préstamo si el usuario tiene multas pendientes, libros atrasados o ya tiene un ejemplar del mismo libro. | Préstamos |
| RF-16 | Renovar un préstamo vigente hasta 2 veces; no se puede renovar un préstamo vencido o ya devuelto. | Préstamos |
| RF-17 | Consultar los préstamos y filtrarlos por estado (Activo, Atrasado, Devuelto). | Préstamos |
| RF-18 | Registrar la devolución de un libro y liberar el ejemplar. | Devoluciones |
| RF-19 | Controlar los vencimientos: un préstamo pasa a "Atrasado" al día siguiente de su fecha de devolución y se muestran los días de atraso. | Devoluciones |
| RF-20 | Calcular y generar una multa al devolver con atraso: Q2.00 por día, con un tope de Q100.00. | Multas |
| RF-21 | Registrar el pago de una multa. | Multas |
| RF-22 | Consultar las multas y filtrarlas por estado (Pendiente, Pagada). | Multas |
| RF-23 | Reporte de libros disponibles. | Reportes |
| RF-24 | Reporte de préstamos activos. | Reportes |
| RF-25 | Reporte de libros atrasados con los días de atraso y la multa estimada. | Reportes |
| RF-26 | Reporte de usuarios con sus préstamos activos y multas pendientes. | Reportes |
| RF-27 | Reporte de multas con el total pendiente y el total cobrado. | Reportes |
| RF-28 | Exportar cualquier reporte a un archivo CSV. | Reportes |
| RF-29 | Guardar libros, usuarios, préstamos y multas en archivos JSON después de cada cambio. | Persistencia |
| RF-30 | Recuperar toda la información desde los archivos JSON al iniciar la aplicación. | Persistencia |
| RF-31 | Informar con un mensaje claro cuando un archivo de datos esté dañado o no se pueda leer o escribir. | Persistencia |
| RF-32 | Ofrecer un menú principal con botones de navegación entre los módulos. | Interfaz |
| RF-33 | Ofrecer formularios de registro y edición con validación de datos y mensajes de error. | Interfaz |
| RF-34 | Solicitar confirmación antes de eliminar, renovar, devolver o cobrar, y mostrar mensajes de resultado. | Interfaz |

## 2.2 Requisitos no funcionales

| ID | Requisito | Categoría |
|---|---|---|
| RNF-01 | El sistema se desarrolla en C# sobre .NET 8. | Tecnología |
| RNF-02 | El diseño aplica POO: encapsulamiento, constructores, propiedades, herencia (`Persona` → `Lector`/`Bibliotecario`), polimorfismo (límite y plazo de préstamo por tipo de usuario) e interfaces (`IRepositorio<T>`, `IReloj`, `ICalculadoraMulta`). | Diseño |
| RNF-03 | La información se almacena en archivos JSON en UTF-8, legibles y editables con cualquier editor de texto. | Persistencia |
| RNF-04 | Cada escritura se hace en un archivo temporal y luego reemplaza al original, para no dejar archivos corruptos ante un fallo a medio guardado. | Confiabilidad |
| RNF-05 | Las reglas del negocio viven en la capa de lógica, no en la interfaz; ningún dato inválido debe llegar a los archivos. | Integridad |
| RNF-06 | La interfaz es clara, organizada y en español, con mensajes de error comprensibles para el usuario final. | Usabilidad |
| RNF-07 | La aplicación se ejecuta en Windows, Linux y macOS (interfaz Avalonia). | Portabilidad |
| RNF-08 | La lógica se puede probar sin la interfaz: el reloj es inyectable y las pruebas usan carpetas temporales. | Testabilidad |
| RNF-09 | Las consultas y operaciones responden de forma inmediata con volúmenes de cientos de registros (datos en memoria). | Rendimiento |
| RNF-10 | El código se versiona en GitHub con commits pequeños siguiendo Conventional Commits, con mensajes en español. | Proceso |
| RNF-11 | La estructura del código mantiene correspondencia directa con el diagrama de clases UML. | Mantenibilidad |

## 2.3 Reglas del negocio

| ID | Regla |
|---|---|
| RN-01 | Un lector puede tener hasta **3** préstamos simultáneos con plazo de **7** días. |
| RN-02 | Un bibliotecario puede tener hasta **5** préstamos simultáneos con plazo de **14** días. |
| RN-03 | Un préstamo se puede renovar como máximo **2** veces, y solo si no está vencido. Cada renovación agrega el plazo del tipo de usuario. |
| RN-04 | Un préstamo está **Atrasado** cuando la fecha actual es posterior a su fecha de vencimiento y aún no se devolvió. |
| RN-05 | La multa es de **Q2.00 por día de atraso**, con un tope de **Q100.00** por préstamo. |
| RN-06 | Un usuario con multas pendientes o con préstamos atrasados **no puede** recibir nuevos préstamos. |
| RN-07 | Un usuario no puede tener dos ejemplares del mismo libro en préstamo al mismo tiempo. |
| RN-08 | Un libro o un usuario con historial de préstamos **no se elimina**; un usuario se puede desactivar. |
| RN-09 | El ISBN, el DPI y el carnet son únicos. |

---

# 3. Actores

| Actor | Tipo | Descripción |
|---|---|---|
| **Bibliotecario** | Principal | Persona del personal que opera el sistema: registra libros y usuarios, presta, renueva, recibe devoluciones, cobra multas y consulta reportes. Un bibliotecario también puede ser beneficiario de préstamos (con reglas propias). |
| **Lector** | Secundario | Usuario de la biblioteca que solicita libros. No opera el programa; interviene en los casos de uso de préstamo, devolución y pago de multas a través del bibliotecario. |
| **Archivos JSON** | Sistema secundario | Almacenamiento persistente (`libros.json`, `usuarios.json`, `prestamos.json`, `multas.json`) que el sistema lee al iniciar y escribe con cada cambio. |
