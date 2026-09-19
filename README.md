# Sistema de biblioteca

[2do Semestre] Sistema de gestión de biblioteca - Programación II

Proyecto Integrador de Programación II: aplicación de escritorio en **C# (.NET 8)** con interfaz gráfica (Avalonia), programación orientada a objetos y persistencia en **archivos JSON**. Fecha de presentación: 10 de octubre de 2026.

![Pantalla de inicio](docs/img/pantalla-inicio.png)

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
dotnet build Biblioteca.sln
dotnet test tests/Biblioteca.Tests
dotnet run --project src/Biblioteca.App
```

Al compilar, los archivos de `datos/` (datos de prueba) se copian junto al ejecutable. La aplicación guarda ahí los cambios; para usar otra carpeta se define la variable de entorno `BIBLIOTECA_DATOS`.

## Estructura

```
├── Biblioteca.sln
├── datos/                 libros.json, usuarios.json, prestamos.json, multas.json (datos de prueba)
├── docs/                  documento de análisis (PDF y fuentes), diagramas UML, imágenes y presentación
├── src/
│   ├── Biblioteca.Core/   modelos, servicios, políticas y repositorio JSON
│   └── Biblioteca.App/    interfaz gráfica
└── tests/
    └── Biblioteca.Tests/  70 pruebas unitarias (xUnit)
```

## Documentación

- [Documento de análisis (PDF)](docs/Documento-de-Analisis.pdf)
- Fuentes en Markdown con diagramas Mermaid y SVG:
  [descripción y requisitos](docs/01-descripcion-y-requisitos.md) ·
  [casos de uso](docs/02-casos-de-uso.md) ·
  [diagramas UML](docs/03-diagramas-uml.md) ·
  [clases, persistencia, interfaz y trazabilidad](docs/04-clases-diseno-y-trazabilidad.md)
- [Presentación](docs/Presentacion.pptx)

## Trazabilidad

Problema → Requisitos → UML → Clases → Código C# → Interfaz → Archivos → Pruebas. La matriz completa está en el capítulo 9 del documento de análisis.

## Convención de commits

[Conventional Commits](https://www.conventionalcommits.org/) con la descripción en español (`feat`, `fix`, `docs`, `chore`, `test`…), un commit por cambio lógico.
