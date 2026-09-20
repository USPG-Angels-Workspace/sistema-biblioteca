# Sistema de biblioteca

[2do Semestre] Sistema de gestión de biblioteca - Programación II

Proyecto Integrador de Programación II (Universidad San Pablo de Guatemala): aplicación web en **C# con ASP.NET Core MVC**, interfaz con **Razor + Tailwind CSS**, programación orientada a objetos y persistencia en **archivos JSON** (sin base de datos).

![Panel de inicio](docs/img/pantalla-inicio.png)

## Integrantes

| Integrante | Carné |
|---|---|
| Angela María Cruz Cifuentes | 2600447 |
| Carlos Fernando Villatoro López | 2600094 |
| Angel Kaled Rodriguez Soc | 2600100 |

Catedrático: Erik Arnulfo Santizo Bardales · Fecha de presentación: 10 de octubre de 2026.

## Funcionalidad

| Módulo | Qué permite |
|---|---|
| Libros | Registro, edición, eliminación, búsqueda, clasificación por categoría y control de disponibilidad. |
| Usuarios | Lectores y bibliotecarios: registro, actualización, consulta y desactivación. |
| Préstamos | Préstamo con validación de reglas, fecha de devolución automática y renovaciones. |
| Devoluciones y multas | Devolución, control de vencimientos, cálculo automático de multas (Q2.00 por día, tope Q100.00) y cobro. |
| Reportes | Libros disponibles, préstamos activos, libros atrasados, usuarios y multas, con exportación a CSV. |

Reglas principales: un lector puede tener 3 préstamos por 7 días y un bibliotecario 5 por 14 días; máximo 2 renovaciones; con multas pendientes o libros atrasados no se pueden hacer nuevos préstamos.

## Ejecutar

Requisitos: [.NET SDK 8](https://dotnet.microsoft.com/download) o superior (Windows, Linux o macOS).

```bash
dotnet run
```

Abrir en el navegador la dirección que muestra la consola (por defecto `http://localhost:5110`). **Se necesita conexión a internet**: los estilos (Tailwind CSS) y la tipografía se cargan por CDN. Otros comandos:

```bash
dotnet build Biblioteca.sln      # compilar todo
dotnet test tests/Biblioteca.Tests   # 94 pruebas
```

La aplicación guarda los cambios en los archivos de `Data/`. Para volver a los datos de prueba originales: `git checkout Data`. Para usar otra carpeta se define `BIBLIOTECA_DATOS` (o la configuración `DataPath`).

## Arquitectura (MVC)

```
Biblioteca.Web.csproj      aplicación web ASP.NET Core MVC (creada con "dotnet new mvc")
├── Program.cs             configuración: MVC, inyección de SistemaBiblioteca, cultura es-GT
├── Controllers/           C: un controlador por módulo (Libros, Usuarios, Préstamos, Devoluciones, Multas, Reportes)
├── Models/ViewModels/     modelos de vista: formularios con validación y listados
├── Views/                 V: vistas Razor (.cshtml) con Tailwind CSS
├── wwwroot/               archivos estáticos (favicon)
├── Data/                  libros.json, usuarios.json, prestamos.json, multas.json (datos de prueba)
├── src/Biblioteca.Core/   M: dominio, servicios, políticas y repositorio JSON
├── tests/Biblioteca.Tests pruebas unitarias y de integración (xUnit)
└── docs/                  documento de análisis (PDF y fuentes), diagramas UML, imágenes y presentación
```

El **Modelo** vive en `Biblioteca.Core` (entidades como `Libro`, `Persona` → `Lector`/`Bibliotecario`, `Prestamo` y `Multa`, con sus servicios y el acceso a JSON detrás de `IRepositorio<T>`). Los **controladores** solo reciben la petición, llaman a los servicios y eligen la vista; las reglas de negocio no están en la interfaz.

## Documentación

- [Documento de análisis (PDF, formato APA 7, 10 páginas)](docs/Documento-de-Analisis.pdf) · fuente: [analisis-apa.md](docs/analisis-apa.md) · [versión resumida sin formato APA](docs/resumen.md)
- Anexo con el detalle completo (especificación de cada caso de uso, todos los diagramas y capturas), en Markdown con diagramas Mermaid y SVG:
  [descripción y requisitos](docs/01-descripcion-y-requisitos.md) ·
  [casos de uso](docs/02-casos-de-uso.md) ·
  [diagramas UML](docs/03-diagramas-uml.md) ·
  [clases, persistencia, interfaz y trazabilidad](docs/04-clases-diseno-y-trazabilidad.md)
- [Presentación](docs/Presentacion.pptx)

## Trazabilidad

Problema → Requisitos → UML → Clases → Código C# → Interfaz → Archivos → Pruebas. La matriz completa está en la Tabla 5 del documento de análisis.

## Convención de commits

[Conventional Commits](https://www.conventionalcommits.org/) con la descripción en español (`feat`, `fix`, `docs`, `chore`, `test`…), un commit por cambio lógico.
