# 5. Reparto de trabajo

El proyecto se dividió entre los tres integrantes **por módulos funcionales**. Cada integrante es responsable de sus módulos de punta a punta: modelos y servicios del dominio, controladores, modelos de vista, vistas Razor, pruebas y la parte de la documentación que los describe. Angel, además, se encarga de la base compartida del sistema y de consolidar la documentación.

## 5.1 Resumen

| Integrante | Carné | Módulos | Casos de uso | Requisitos |
|---|---|---|---|---|
| Angela María Cruz Cifuentes | 2600447 | Libros y Usuarios | CU-01 a CU-08 | RF-01 a RF-11 |
| Carlos Fernando Villatoro López | 2600094 | Préstamos, Devoluciones y Multas | CU-09 a CU-16, CU-18 | RF-12 a RF-22 |
| Angel Kaled Rodriguez Soc | 2600100 | Reportes, base del sistema y documentación | CU-17 | RF-23 a RF-29 |

## 5.2 Angela María Cruz Cifuentes — Libros y Usuarios

**Casos de uso:** registrar, editar, eliminar, buscar y clasificar libros (CU-01 a CU-04); registrar, editar, desactivar, eliminar y consultar usuarios (CU-05 a CU-08).

| Capa | Archivos |
|---|---|
| Dominio | `src/Biblioteca.Core/Modelos/Libro.cs`, `CategoriaLibro.cs`, `Persona.cs`, `Lector.cs`, `Bibliotecario.cs` |
| Servicios | `src/Biblioteca.Core/Servicios/LibroService.cs`, `UsuarioService.cs` |
| Controladores | `Controllers/LibrosController.cs`, `Controllers/UsuariosController.cs` |
| Modelos de vista | `Models/ViewModels/LibroViewModels.cs`, `Models/ViewModels/UsuarioViewModels.cs` |
| Vistas | `Views/Libros/*`, `Views/Usuarios/*` |
| Datos | `Data/libros.json`, `Data/usuarios.json` |
| Pruebas | casos de libros y usuarios en `tests/Biblioteca.Tests/ModelosTests.cs` y `ServiciosTests.cs` |

**Reglas que implementa:** ISBN único sin importar guiones o espacios, clasificación por categoría, control de ejemplares disponibles, DPI de 13 dígitos y correo con formato válido, y bloqueo de eliminación cuando hay historial de préstamos o multas.

## 5.3 Carlos Fernando Villatoro López — Préstamos, Devoluciones y Multas

**Casos de uso:** registrar, renovar y consultar préstamos (CU-09 a CU-11); devolución, control de vencimientos, generación y pago de multas (CU-12 a CU-16); validación de condiciones del préstamo (CU-18).

| Capa | Archivos |
|---|---|
| Dominio | `src/Biblioteca.Core/Modelos/Prestamo.cs`, `EstadoPrestamo.cs`, `Multa.cs` |
| Políticas e interfaces | `src/Biblioteca.Core/Politicas/MultaPorDia.cs`, `src/Biblioteca.Core/Interfaces/ICalculadoraMulta.cs` |
| Servicios | `src/Biblioteca.Core/Servicios/PrestamoService.cs`, `DevolucionService.cs`, `MultaService.cs`, `PrestamoDetalle.cs`, `MultaDetalle.cs`, `ResultadoDevolucion.cs` |
| Controladores | `Controllers/PrestamosController.cs`, `DevolucionesController.cs`, `MultasController.cs` |
| Modelos de vista | `Models/ViewModels/PrestamoViewModels.cs` |
| Vistas | `Views/Prestamos/*`, `Views/Devoluciones/*`, `Views/Multas/*` |
| Datos | `Data/prestamos.json`, `Data/multas.json` |
| Pruebas | casos de préstamos, devoluciones y multas en `tests/Biblioteca.Tests/ServiciosTests.cs` |

**Reglas que implementa:** 3 préstamos por 7 días para lectores y 5 por 14 días para bibliotecarios, máximo 2 renovaciones, multa de Q2.00 por día con tope de Q100.00, y bloqueo de nuevos préstamos con multas pendientes o libros atrasados.

## 5.4 Angel Kaled Rodriguez Soc — Reportes, base del sistema y documentación

**Casos de uso:** generar y exportar reportes (CU-17).

| Capa | Archivos |
|---|---|
| Base del dominio | `src/Biblioteca.Core/Modelos/EntidadBase.cs`, `Validador.cs`; `Interfaces/IRepositorio.cs`, `IReloj.cs`; `Excepciones/*`; `Politicas/RelojSistema.cs`; `SistemaBiblioteca.cs` |
| Persistencia | `src/Biblioteca.Core/Persistencia/RepositorioJson.cs` |
| Reportes | `src/Biblioteca.Core/Servicios/ReporteService.cs`, `ReporteTabular.cs` |
| Aplicación web | `Program.cs`, `Controllers/BaseController.cs`, `HomeController.cs`, `ReportesController.cs` |
| Modelos de vista | `Models/ViewModels/PanelViewModels.cs`, `Models/Estilos.cs`, `Listas.cs`, `Textos.cs`, `ErrorViewModel.cs` |
| Vistas | `Views/Reportes/*`, `Views/Home/*`, `Views/Shared/*`, `Views/_ViewImports.cshtml`, `_ViewStart.cshtml` |
| Pruebas | `tests/Biblioteca.Tests/PersistenciaTests.cs`, `WebTests.cs`, `Escenario.cs` |
| Documentación | portada, resumen, presentación, documento de análisis (PDF) y `README.md` |

**Reglas que implementa:** reportes de libros disponibles, préstamos activos, libros atrasados, usuarios y multas con exportación a CSV; persistencia en archivos JSON; configuración de la aplicación e inyección de `SistemaBiblioteca`.

## 5.5 Documentación

Cada integrante redactó en `docs/` los requisitos, casos de uso, diagramas de actividad y capturas de pantalla de **sus** módulos. Angel unificó el estilo, armó la portada, el resumen y la presentación, y generó el documento de análisis final.

## 5.6 Puntos de integración

| Quién depende | De quién | Qué usa |
|---|---|---|
| Carlos (Préstamos) | Angela | `Libro` y `LibroService` (disponibilidad), `Lector`/`Bibliotecario` y `UsuarioService` (límites y estado activo) |
| Angela (eliminación) | Carlos | historial de préstamos y multas para bloquear la eliminación de libros y usuarios |
| Angel (Reportes) | Angela y Carlos | lee los servicios de libros, usuarios, préstamos y multas |
| Todos | Angel | `IRepositorio`/`RepositorioJson`, `Validador`, `SistemaBiblioteca`, `_Layout` y estilos compartidos |

## 5.7 Orden de trabajo

1. **Angel:** estructura del proyecto, base del dominio y persistencia JSON.
2. **Angela:** libros y usuarios (dominio, servicios, controladores y vistas).
3. **Carlos:** préstamos, devoluciones y multas sobre los módulos anteriores.
4. **Angel:** reportes, panel de inicio e integración final en `SistemaBiblioteca` y `Program.cs`.
5. **Todos:** pruebas de integración, documentación y presentación.
