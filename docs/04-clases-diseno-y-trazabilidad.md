# 6. Descripción de las principales clases

Las clases del código tienen correspondencia directa con el diagrama de clases del capítulo 5 (RNF-11).

## 6.1 Modelos del dominio (`Biblioteca.Core/Modelos`)

| Clase | Responsabilidad | Atributos principales | Métodos principales |
|---|---|---|---|
| `EntidadBase` (abstracta) | Base de todo lo que se guarda en JSON; aporta el identificador. | `Id` (asignado por el repositorio). | `AsignarId` (interno). |
| `Persona` (abstracta) | Datos comunes de quien usa la biblioteca y reglas de préstamo que cada tipo concreta. | `Nombre`, `Documento`, `Correo`, `Telefono`, `FechaRegistro`, `Activo`. | `Actualizar`, `Coincide`; miembros abstractos `TipoUsuario`, `LimitePrestamos`, `DiasPrestamo`, `Detalle`. |
| `Lector` | Usuario que solicita libros. | `Carnet`. | Hereda de `Persona`: límite 3 préstamos, plazo 7 días. |
| `Bibliotecario` | Personal de la biblioteca (también puede pedir libros). | `Cargo`. | Hereda de `Persona`: límite 5 préstamos, plazo 14 días. |
| `Libro` | Título del catálogo con su inventario de ejemplares. | `Isbn`, `Titulo`, `Autor`, `Editorial`, `Anio`, `Categoria`, `CopiasTotales`, `CopiasDisponibles`. | `Actualizar`, `PrestarCopia`, `DevolverCopia`, `Coincide`; calculadas `Disponible`, `CopiasPrestadas`. |
| `Prestamo` | Registro de un libro prestado a un usuario. | `LibroId`, `UsuarioId`, `FechaPrestamo`, `FechaVencimiento`, `FechaDevolucion`, `Renovaciones`. | `ObtenerEstado`, `DiasAtraso`, `Renovar`, `RegistrarDevolucion`. |
| `Multa` | Cobro generado por devolver con atraso. | `PrestamoId`, `UsuarioId`, `DiasAtraso`, `Monto`, `FechaGeneracion`, `Pagada`, `FechaPago`. | `Pagar`. |
| `Validador` (estática) | Reglas de validación compartidas (ISBN, DPI, correo, teléfono, año, ejemplares). | — | `Isbn`, `Documento`, `Correo`, `Telefono`, `TextoRequerido`, `Anio`, `Copias`. |
| `CategoriaLibro`, `EstadoPrestamo` | Enumeraciones de clasificación y de estado. | — | `Texto()` (nombre legible de la categoría). |

## 6.2 Conceptos de POO aplicados

| Concepto | Dónde se aplica |
|---|---|
| **Clases y objetos** | `Libro`, `Persona`, `Prestamo`, `Multa` y los servicios. |
| **Encapsulamiento** | Todas las propiedades tienen `set` privado; el estado solo cambia mediante métodos que validan (por ejemplo `Libro.PrestarCopia()` no permite bajar de cero ejemplares). |
| **Constructores** | Los constructores públicos validan los datos; un constructor privado sin parámetros lo usa el deserializador JSON. |
| **Propiedades** | Propiedades de solo lectura desde fuera y propiedades calculadas (`Libro.Disponible`, `Prestamo.Devuelto`). |
| **Métodos** | Comportamiento de negocio en las entidades (`Renovar`, `RegistrarDevolucion`, `Pagar`) y en los servicios. |
| **Herencia** | `Persona` → `Lector` / `Bibliotecario`; `EntidadBase` → `Persona`, `Libro`, `Prestamo`, `Multa`. |
| **Polimorfismo** | `LimitePrestamos` y `DiasPrestamo` se resuelven según el tipo concreto de `Persona`; `PrestamoService` los usa sin preguntar el tipo. La colección `List<Persona>` se guarda y se recupera con su tipo real. |
| **Interfaces** | `IRepositorio<T>` (almacenamiento intercambiable), `IReloj` (fecha controlable en pruebas) e `ICalculadoraMulta` (política de multa sustituible). |
| **Genéricos** | `RepositorioJson<T>` sirve para las cuatro colecciones. |
| **Excepciones propias** | `ValidacionException` (regla de negocio incumplida) y `AlmacenamientoException` (problema con un archivo). |

## 6.3 Servicios (`Biblioteca.Core/Servicios`)

| Servicio | Responsabilidad |
|---|---|
| `LibroService` | Registrar, editar, eliminar, listar y buscar libros; garantiza ISBN único y bloquea la eliminación de libros con historial. |
| `UsuarioService` | Registrar lectores y bibliotecarios, editar, desactivar, eliminar y buscar; garantiza DPI y carnet únicos. |
| `PrestamoService` | Registrar préstamos aplicando todas las reglas del servicio, renovarlos y listarlos con nombres y estado resueltos (`PrestamoDetalle`). |
| `DevolucionService` | Registrar la devolución, liberar el ejemplar y disparar la generación de la multa. |
| `MultaService` | Calcular y generar multas, registrar pagos, consultar multas y saldos pendientes. |
| `ReporteService` | Construir los cinco reportes (`ReporteTabular`) y exportarlos a CSV. |
| `SistemaBiblioteca` | Punto de entrada: crea los repositorios JSON y los servicios; es lo único que la interfaz necesita conocer. |

---

# 7. Diseño de la persistencia en archivos JSON

## 7.1 Archivos

| Archivo | Contenido | Entidad |
|---|---|---|
| `libros.json` | Catálogo de libros con sus ejemplares. | `Libro` |
| `usuarios.json` | Lectores y bibliotecarios; cada objeto lleva `"tipo": "lector"` o `"bibliotecario"`. | `Persona` |
| `prestamos.json` | Historial completo de préstamos. | `Prestamo` |
| `multas.json` | Multas generadas y su estado de pago. | `Multa` |

Los archivos se guardan en la carpeta `datos/` junto al ejecutable (o en la ruta indicada por la variable de entorno `BIBLIOTECA_DATOS`). El repositorio incluye los **datos de prueba** en `datos/` y el proyecto los copia automáticamente a la carpeta de salida.

## 7.2 Ejemplo de formato

```json
{
  "tipo": "lector",
  "carnet": "2026-0101",
  "nombre": "María Fernanda López",
  "documento": "2456789010101",
  "correo": "maria.lopez@ejemplo.com",
  "telefono": "55123401",
  "fechaRegistro": "2026-08-03T00:00:00",
  "activo": true,
  "id": 1
}
```

Solo se guardan los datos primarios: las propiedades calculadas (`LimitePrestamos`, `Disponible`, estado del préstamo, etc.) se recalculan al cargar, de modo que no puedan quedar desincronizadas.

## 7.3 Estrategia de lectura y escritura

1. **Al iniciar**, `RepositorioJson<T>` lee el archivo completo y lo deserializa a una lista en memoria. Si no existe o está vacío, comienza con una colección vacía.
2. **En cada cambio** (agregar, actualizar o eliminar) serializa la lista completa a `archivo.json.tmp` y luego reemplaza al archivo original. Así, un fallo a mitad de escritura no deja un archivo truncado.
3. **Identificadores:** `Id = máximo actual + 1`. Los identificadores de registros eliminados no se reutilizan mientras exista un registro con un identificador mayor.
4. **Errores:** un JSON dañado o un problema de disco se convierte en `AlmacenamientoException` con el nombre del archivo; al iniciar, la aplicación lo muestra y no abre con datos incompletos.
5. **Codificación:** UTF-8, con sangría, sin escapar tildes ni «ñ», para que el archivo sea legible.

---

# 8. Interfaz gráfica

La interfaz se construyó con Avalonia (XAML + C#), sin lógica de negocio: cada vista solo llama a los servicios y muestra el resultado. Cumple con lo mínimo solicitado:

| Requisito de interfaz | Cómo se cumple |
|---|---|
| Menú principal | Barra lateral con **Inicio, Libros, Usuarios, Préstamos, Devoluciones y multas, Reportes y Salir**. |
| Botones de navegación | El menú lateral y los accesos rápidos de la pantalla de inicio. |
| Formularios de registro | Formularios modales de libro, usuario y préstamo. |
| Consulta de información | Tablas con búsqueda, filtros y ordenamiento por columna. |
| Edición de información | Botón **Editar** o doble clic sobre una fila. |
| Eliminación | Botón **Eliminar** en libros y usuarios, permitido solo sin historial. |
| Mensajes de confirmación | Diálogo «¿Está seguro?» antes de eliminar, renovar, devolver o cobrar; mensaje de éxito después de cada operación. |
| Mensajes de error | Errores de validación dentro del formulario (en rojo) y diálogo de error para operaciones rechazadas. |
| Validación de datos | Campos obligatorios, formatos (ISBN, DPI, correo, teléfono), números, unicidad y reglas de negocio. |

### Pantalla de inicio
![Pantalla de inicio](img/pantalla-inicio.png)

### Libros
![Módulo de libros](img/pantalla-libros.png)

| Formulario | Validación |
|---|---|
| ![Formulario de libro](img/form-libro.png) | ![Error de validación](img/form-libro-error.png) |

### Usuarios
![Módulo de usuarios](img/pantalla-usuarios.png)

| Formulario | Validación |
|---|---|
| ![Formulario de usuario](img/form-usuario.png) | ![Error de validación](img/form-usuario-error.png) |

### Préstamos
Los préstamos atrasados se resaltan en rojo y los devueltos en gris.

![Módulo de préstamos](img/pantalla-prestamos.png)

| Nuevo préstamo | Préstamo rechazado por regla de negocio |
|---|---|
| ![Formulario de préstamo](img/form-prestamo.png) | ![Préstamo rechazado](img/form-prestamo-error.png) |

### Devoluciones y multas
![Devoluciones pendientes](img/pantalla-devoluciones.png)

![Multas](img/pantalla-multas.png)

### Reportes
![Reporte de libros disponibles](img/pantalla-reportes.png)

![Reporte de libros atrasados](img/pantalla-reporte-atrasados.png)

### Diálogos de mensajes

| Confirmación | Error |
|---|---|
| ![Diálogo de confirmación](img/dialogo-confirmar.png) | ![Diálogo de error](img/dialogo-error.png) |

---

# 9. Trazabilidad

El proyecto demuestra la cadena **Problema → Requisitos → UML → Clases → Código C# → Interfaz → Archivos → Pruebas**.

## 9.1 Del problema a las clases

| Problema identificado | Requisitos | Casos de uso y diagramas UML | Clases |
|---|---|---|---|
| Catálogo sin control de disponibilidad | RF-01 a RF-07 | CU-01 a CU-04; diagrama de clases | `Libro`, `LibroService`, `Validador` |
| Usuarios sin registro ni límites | RF-08 a RF-11 | CU-05 a CU-08; herencia y polimorfismo | `Persona`, `Lector`, `Bibliotecario`, `UsuarioService` |
| Préstamos a quien no debe recibirlos | RF-12 a RF-17 | CU-09, CU-10, CU-11, CU-18; secuencia, actividad y estados del préstamo | `Prestamo`, `PrestamoService` |
| Vencimientos y multas sin control | RF-18 a RF-22 | CU-12 a CU-16; secuencia y actividad de devolución | `DevolucionService`, `MultaService`, `Multa`, `ICalculadoraMulta`, `MultaPorDia` |
| Reportes lentos y propensos a error | RF-23 a RF-28 | CU-17 | `ReporteService`, `ReporteTabular` |
| Información que se pierde | RF-29 a RF-31 | Secuencia de persistencia | `IRepositorio<T>`, `RepositorioJson<T>`, `AlmacenamientoException` |
| Uso poco claro del sistema | RF-32 a RF-34 | Especificación de casos de uso | `MainWindow`, `Dialogos`, formularios |

## 9.2 De las clases a la interfaz, los archivos y las pruebas

| Requisitos | Código C# | Interfaz | Archivo | Pruebas |
|---|---|---|---|---|
| RF-01 a RF-07 | `LibroService.Registrar/Editar/Eliminar/Buscar`, `Libro.PrestarCopia/DevolverCopia` | `LibrosView`, `LibroFormWindow` | `libros.json` | `LibroTests`, `LibroServiceTests` |
| RF-08 a RF-11 | `Persona.Actualizar`, `LimitePrestamos`, `DiasPrestamo` | `UsuariosView`, `UsuarioFormWindow` | `usuarios.json` | `PersonaTests`, `UsuarioServiceTests` |
| RF-12 a RF-17 | `PrestamoService.Prestar/Renovar`, `Prestamo.Renovar/ObtenerEstado` | `PrestamosView`, `PrestamoFormWindow` | `prestamos.json` | `PrestamoTests`, `PrestamoFlujoTests` |
| RF-18 a RF-22 | `DevolucionService.Registrar`, `MultaService.GenerarPorDevolucion/Pagar` | `DevolucionesView` | `multas.json`, `prestamos.json` | `MultaTests`, `PrestamoFlujoTests` |
| RF-23 a RF-28 | `ReporteService.*`, `ReporteTabular.ACsv` | `ReportesView` | (lee los cuatro archivos) | `ReporteServiceTests` |
| RF-29 a RF-31 | `RepositorioJson.Cargar/Guardar` | Mensaje de error al iniciar | Los cuatro archivos JSON | `PersistenciaTests`, `DatosDePruebaTests` |
| RF-32 a RF-34 | `MainWindow.Navegar`, `Dialogos.Confirmar/Error/Informacion` | Toda la aplicación | — | Verificación manual de la interfaz |

---

# 10. Pruebas

## 10.1 Pruebas automatizadas

El proyecto `tests/Biblioteca.Tests` contiene **70 casos de prueba** (xUnit) que se ejecutan con `dotnet test` sin necesidad de abrir la interfaz:

| Grupo | Qué verifica |
|---|---|
| `LibroTests` | Validación de ISBN, año y ejemplares; disponibilidad; edición sin bajar de los ejemplares prestados; búsqueda. |
| `PersonaTests` | Polimorfismo (límites y plazos por tipo de usuario); validación de DPI, correo y teléfono; actualización. |
| `PrestamoTests` | Fecha de vencimiento; estados; días de atraso; renovaciones y su máximo; devolución única. |
| `MultaTests` | Cálculo de la multa (tarifa diaria, tope, casos límite); pago único. |
| `LibroServiceTests`, `UsuarioServiceTests` | Duplicados, edición, eliminación con y sin historial, búsqueda y filtros. |
| `PrestamoFlujoTests` | Flujo completo: préstamo, límite, bloqueos por multa o atraso, renovación, devolución a tiempo y con atraso, pago de la multa. |
| `ReporteServiceTests` | Contenido de los reportes y escape de comas y comillas en el CSV. |
| `PersistenciaTests` | Los datos sobreviven al cerrar y reabrir; se conserva el polimorfismo; los identificadores continúan; no quedan temporales; archivo dañado o vacío. |
| `DatosDePruebaTests` | Los datos de prueba cargan, son consistentes (ejemplares vs. préstamos) y cubren todos los estados. |

## 10.2 Datos de prueba

La carpeta `datos/` incluye 16 libros, 8 usuarios (6 lectores y 2 bibliotecarios), 12 préstamos y 2 multas. Se generaron con la propia lógica del sistema para que sean consistentes y cubren:

- Préstamos **activos**, **atrasados** y **devueltos** (a tiempo y con atraso).
- Un préstamo **renovado**.
- Un libro **sin ejemplares disponibles** (`Rayuela`).
- Una multa **pagada** y otra **pendiente** (que bloquea al usuario para nuevos préstamos).
- Un usuario con préstamos **atrasados** (`Luis Eduardo Morales`).
- Libros que se pueden **eliminar** (sin historial) y otros que no.

## 10.3 Pruebas manuales de interfaz recomendadas

1. Registrar un libro con un ISBN repetido y comprobar el mensaje de error.
2. Registrar un préstamo a un usuario con multa pendiente y comprobar el rechazo.
3. Devolver un préstamo atrasado y comprobar que se genera la multa correcta.
4. Pagar la multa y volver a prestar al mismo usuario.
5. Cerrar la aplicación, abrirla de nuevo y comprobar que los datos siguen ahí.
6. Exportar un reporte a CSV y abrirlo en una hoja de cálculo.

---

# 11. Uso de GitHub y ejecución del proyecto

## 11.1 Repositorio

Repositorio: <https://github.com/USPG-Angels-Workspace/sistema-biblioteca>

```
sistema-biblioteca/
├── Biblioteca.sln
├── datos/                 archivos JSON con los datos de prueba
├── docs/                  documento de análisis, diagramas, imágenes y presentación
├── src/
│   ├── Biblioteca.Core/   dominio, servicios y persistencia
│   └── Biblioteca.App/    interfaz gráfica (Avalonia)
└── tests/
    └── Biblioteca.Tests/  pruebas unitarias (xUnit)
```

## 11.2 Flujo de trabajo

- Los commits siguen **Conventional Commits** con la descripción en español (`feat`, `fix`, `docs`, `chore`, `test`…).
- Cada commit contiene un solo cambio lógico y el historial se construyó por capas: estructura base → dominio → persistencia → servicios → pruebas → interfaz → documentación.

## 11.3 Cómo ejecutar

Requisitos: .NET SDK 8 o superior.

```bash
dotnet build Biblioteca.sln
dotnet test tests/Biblioteca.Tests
dotnet run --project src/Biblioteca.App
```
