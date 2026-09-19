# 5. Diagramas UML

El diagrama de casos de uso se presentó en el capítulo 4. Este capítulo contiene los diagramas de **clases**, **secuencia**, **actividad** y **estados**, además de la vista de **arquitectura**. Los diagramas de clases, secuencia, estados y arquitectura están escritos como código Mermaid (GitHub los muestra directamente y se versionan junto con el código); los de actividad se dibujaron como imágenes SVG.

## 5.1 Diagrama de clases del dominio

Es el modelo que se implementó en `src/Biblioteca.Core/Modelos`. `EntidadBase` aporta el identificador que usa la persistencia; `Persona` es abstracta y sus subclases definen, por **polimorfismo**, cuántos libros puede llevar cada usuario y por cuántos días.

```mermaid
classDiagram
    direction TB

    class EntidadBase {
        <<abstract>>
        +int Id
    }

    class Persona {
        <<abstract>>
        +string Nombre
        +string Documento
        +string Correo
        +string Telefono
        +DateTime FechaRegistro
        +bool Activo
        +string TipoUsuario*
        +string EtiquetaDetalle*
        +string Detalle*
        +int LimitePrestamos*
        +int DiasPrestamo*
        +Actualizar(nombre, documento, correo, telefono, detalle, activo)
        +Coincide(texto) bool
    }

    class Lector {
        +string Carnet
        +TipoUsuario = "Lector"
        +LimitePrestamos = 3
        +DiasPrestamo = 7
    }

    class Bibliotecario {
        +string Cargo
        +TipoUsuario = "Bibliotecario"
        +LimitePrestamos = 5
        +DiasPrestamo = 14
    }

    class Libro {
        +string Isbn
        +string Titulo
        +string Autor
        +string Editorial
        +int Anio
        +CategoriaLibro Categoria
        +int CopiasTotales
        +int CopiasDisponibles
        +bool Disponible
        +int CopiasPrestadas
        +Actualizar(isbn, titulo, autor, editorial, anio, categoria, copiasTotales)
        +PrestarCopia()
        +DevolverCopia()
        +Coincide(texto) bool
    }

    class Prestamo {
        +int LibroId
        +int UsuarioId
        +DateTime FechaPrestamo
        +DateTime FechaVencimiento
        +DateTime? FechaDevolucion
        +int Renovaciones
        +bool Devuelto
        +int MaximoRenovaciones = 2$
        +ObtenerEstado(hoy) EstadoPrestamo
        +DiasAtraso(referencia) int
        +Renovar(dias, hoy)
        +RegistrarDevolucion(fecha)
    }

    class Multa {
        +int PrestamoId
        +int UsuarioId
        +int DiasAtraso
        +decimal Monto
        +DateTime FechaGeneracion
        +bool Pagada
        +DateTime? FechaPago
        +Pagar(fecha)
    }

    class CategoriaLibro {
        <<enumeration>>
        Ficcion
        NoFiccion
        Ciencia
        Tecnologia
        Historia
        Arte
        Infantil
        Referencia
        Otros
    }

    class EstadoPrestamo {
        <<enumeration>>
        Activo
        Atrasado
        Devuelto
    }

    class Validador {
        <<static>>
        +TextoRequerido(valor, campo) string
        +Isbn(valor) string
        +Documento(valor) string
        +Correo(valor) string
        +Telefono(valor) string
        +Anio(anio) int
        +Copias(copias) int
    }

    EntidadBase <|-- Persona
    EntidadBase <|-- Libro
    EntidadBase <|-- Prestamo
    EntidadBase <|-- Multa
    Persona <|-- Lector
    Persona <|-- Bibliotecario

    Persona "1" <-- "0..*" Prestamo : realiza
    Libro "1" <-- "0..*" Prestamo : se presta en
    Prestamo "1" <-- "0..1" Multa : genera
    Persona "1" <-- "0..*" Multa : adeuda
    Libro --> CategoriaLibro
    Prestamo ..> EstadoPrestamo
    Libro ..> Validador
    Persona ..> Validador
```

## 5.2 Diagrama de clases de servicios y persistencia

Muestra cómo se conectan la lógica y el almacenamiento. Los servicios dependen de **interfaces** (`IRepositorio<T>`, `IReloj`, `ICalculadoraMulta`), lo que permite cambiar el almacenamiento, controlar la fecha en las pruebas o sustituir la política de multas sin tocar el resto del sistema.

```mermaid
classDiagram
    direction LR

    class IRepositorio~T~ {
        <<interface>>
        +ObtenerTodos() IReadOnlyList~T~
        +ObtenerPorId(id) T
        +Agregar(entidad)
        +Actualizar(entidad)
        +Eliminar(id)
    }

    class RepositorioJson~T~ {
        -string _ruta
        -List~T~ _elementos
        +Agregar(entidad)
        +Actualizar(entidad)
        +Eliminar(id)
        -Cargar() List~T~
        -Guardar()
    }

    class IReloj {
        <<interface>>
        +DateTime Hoy
    }

    class RelojSistema {
        +DateTime Hoy
    }

    class ICalculadoraMulta {
        <<interface>>
        +Calcular(diasAtraso) decimal
    }

    class MultaPorDia {
        +decimal TarifaDiaria = 2.00
        +decimal TopeMaximo = 100.00
        +Calcular(diasAtraso) decimal
    }

    class LibroService {
        +Registrar(...) Libro
        +Editar(id, ...) Libro
        +Eliminar(id)
        +Buscar(texto, categoria, soloDisponibles)
        +Listar()
    }

    class UsuarioService {
        +RegistrarLector(...) Lector
        +RegistrarBibliotecario(...) Bibliotecario
        +Editar(id, ...) Persona
        +Eliminar(id)
        +Buscar(texto, tipo)
    }

    class PrestamoService {
        +Prestar(libroId, usuarioId) Prestamo
        +Renovar(prestamoId) Prestamo
        +Listar(soloPendientes, texto, estado)
    }

    class DevolucionService {
        +Registrar(prestamoId) ResultadoDevolucion
    }

    class MultaService {
        +CalcularMonto(diasAtraso) decimal
        +GenerarPorDevolucion(prestamo) Multa
        +Pagar(multaId) Multa
        +TotalPendienteDe(usuarioId) decimal
        +Listar(pagadas, texto)
    }

    class ReporteService {
        +LibrosDisponibles() ReporteTabular
        +PrestamosActivos() ReporteTabular
        +LibrosAtrasados() ReporteTabular
        +Usuarios() ReporteTabular
        +Multas() ReporteTabular
    }

    class ReporteTabular {
        +string Titulo
        +Columnas
        +Filas
        +string Resumen
        +ACsv() string
    }

    class SistemaBiblioteca {
        +LibroService Libros
        +UsuarioService Usuarios
        +PrestamoService Prestamos
        +DevolucionService Devoluciones
        +MultaService Multas
        +ReporteService Reportes
    }

    IRepositorio~T~ <|.. RepositorioJson~T~
    IReloj <|.. RelojSistema
    ICalculadoraMulta <|.. MultaPorDia

    SistemaBiblioteca *-- LibroService
    SistemaBiblioteca *-- UsuarioService
    SistemaBiblioteca *-- PrestamoService
    SistemaBiblioteca *-- DevolucionService
    SistemaBiblioteca *-- MultaService
    SistemaBiblioteca *-- ReporteService

    DevolucionService --> MultaService
    ReporteService ..> ReporteTabular

    LibroService ..> IRepositorio~T~
    UsuarioService ..> IRepositorio~T~
    PrestamoService ..> IRepositorio~T~
    DevolucionService ..> IRepositorio~T~
    MultaService ..> IRepositorio~T~
    ReporteService ..> IRepositorio~T~
    PrestamoService ..> IReloj
    MultaService ..> IReloj
    MultaService ..> ICalculadoraMulta
```

## 5.3 Diagramas de secuencia

### Registrar un préstamo (CU-09)

```mermaid
sequenceDiagram
    autonumber
    actor B as Bibliotecario
    participant UI as PrestamoFormWindow
    participant PS as PrestamoService
    participant LR as Repositorio de libros
    participant UR as Repositorio de usuarios
    participant MR as Repositorio de multas
    participant PR as Repositorio de préstamos
    participant J as Archivos JSON

    B->>UI: elige usuario y libro, pulsa "Registrar préstamo"
    UI->>PS: Prestar(libroId, usuarioId)
    PS->>LR: ObtenerPorId(libroId)
    PS->>UR: ObtenerPorId(usuarioId)
    PS->>MR: ObtenerTodos() para multas pendientes
    PS->>PR: ObtenerTodos() para préstamos del usuario
    alt incumple una regla (inactivo, multa, atraso, límite, duplicado o sin ejemplares)
        PS-->>UI: ValidacionException(mensaje)
        UI-->>B: muestra el mensaje de error
    else cumple todas las reglas
        PS->>PS: crea Prestamo(hoy + días del tipo de usuario)
        PS->>LR: libro.PrestarCopia()
        PS->>PR: Agregar(prestamo)
        PR->>J: escribe prestamos.json
        PS->>LR: Actualizar(libro)
        LR->>J: escribe libros.json
        PS-->>UI: Prestamo
        UI-->>B: confirma con la fecha de devolución
    end
```

### Registrar una devolución con multa (CU-12 y CU-14)

```mermaid
sequenceDiagram
    autonumber
    actor B as Bibliotecario
    participant UI as DevolucionesView
    participant DS as DevolucionService
    participant P as Prestamo
    participant L as Libro
    participant MS as MultaService
    participant C as ICalculadoraMulta
    participant J as Archivos JSON

    B->>UI: selecciona el préstamo y pulsa "Registrar devolución"
    UI-->>B: pide confirmación (muestra la multa estimada)
    B->>UI: confirma
    UI->>DS: Registrar(prestamoId)
    DS->>P: RegistrarDevolucion(hoy)
    DS->>L: DevolverCopia()
    DS->>J: guarda prestamos.json y libros.json
    DS->>MS: GenerarPorDevolucion(prestamo)
    MS->>P: DiasAtraso(hoy)
    opt días de atraso mayores que cero
        MS->>C: Calcular(dias)
        C-->>MS: monto (Q2.00 por día, máximo Q100.00)
        MS->>J: guarda la multa en multas.json
    end
    MS-->>DS: Multa o nulo
    DS-->>UI: ResultadoDevolucion
    UI-->>B: informa la devolución y la multa generada
```

### Guardar y recuperar información (persistencia)

```mermaid
sequenceDiagram
    autonumber
    participant App as Aplicación
    participant SB as SistemaBiblioteca
    participant R as RepositorioJson
    participant F as libros.json

    Note over App,F: Al iniciar la aplicación
    App->>SB: new SistemaBiblioteca(carpetaDatos)
    SB->>R: new RepositorioJson(ruta)
    R->>F: lee y deserializa el archivo
    alt archivo dañado
        R-->>App: AlmacenamientoException
        App-->>App: muestra el error y no abre el sistema
    else archivo válido o inexistente
        R-->>SB: colección en memoria
    end

    Note over App,F: Al registrar, editar o eliminar
    App->>R: Agregar / Actualizar / Eliminar
    R->>F: escribe libros.json.tmp
    R->>F: reemplaza libros.json por el temporal
```

## 5.4 Diagramas de actividad

### Registrar un préstamo

![Diagrama de actividad: registrar un préstamo](img/diagrama-actividad-prestamo.svg)

### Registrar una devolución y generar la multa

![Diagrama de actividad: registrar una devolución y generar la multa](img/diagrama-actividad-devolucion.svg)

### Registrar o editar un libro (validación de datos)

![Diagrama de actividad: registrar o editar un libro](img/diagrama-actividad-libro.svg)

## 5.5 Diagrama de estados del préstamo

```mermaid
stateDiagram-v2
    direction LR
    [*] --> Activo: prestar
    Activo --> Activo: renovar (máx. 2)
    Activo --> Atrasado: vence el plazo
    Activo --> Devuelto: devolver
    Atrasado --> Devuelto: devolver y multar
    Devuelto --> [*]
```

## 5.6 Arquitectura de la solución

La solución está dividida en tres proyectos con dependencias en un solo sentido (interfaz → lógica). La lógica no conoce la interfaz, por eso se puede probar de forma automática.

```mermaid
flowchart TB
    subgraph APP["Biblioteca.App · Presentación (Avalonia)"]
        direction LR
        V1[MainWindow]
        V2[Vistas: Libros, Usuarios, Préstamos, Devoluciones, Reportes]
        V3[Formularios y diálogos]
    end

    subgraph CORE["Biblioteca.Core · Lógica y dominio"]
        direction LR
        S[Servicios]
        M[Modelos]
        P[Políticas: multa y reloj]
        R[RepositorioJson]
    end

    subgraph DATA["Archivos JSON · carpeta datos"]
        direction LR
        F1[(libros.json)]
        F2[(usuarios.json)]
        F3[(prestamos.json)]
        F4[(multas.json)]
    end

    T["Biblioteca.Tests · pruebas xUnit"]

    APP -->|usa SistemaBiblioteca| S
    S --> M
    S --> P
    S --> R
    R --> F1 & F2 & F3 & F4
    T -.->|prueba| CORE
```
