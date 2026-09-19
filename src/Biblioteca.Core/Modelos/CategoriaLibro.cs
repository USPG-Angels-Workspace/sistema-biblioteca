namespace Biblioteca.Core.Modelos;

/// <summary>Clasificación temática de un libro.</summary>
public enum CategoriaLibro
{
    Ficcion,
    NoFiccion,
    Ciencia,
    Tecnologia,
    Historia,
    Arte,
    Infantil,
    Referencia,
    Otros
}

public static class CategoriaLibroExtensiones
{
    public static string Texto(this CategoriaLibro categoria) => categoria switch
    {
        CategoriaLibro.Ficcion => "Ficción",
        CategoriaLibro.NoFiccion => "No ficción",
        CategoriaLibro.Ciencia => "Ciencia",
        CategoriaLibro.Tecnologia => "Tecnología",
        CategoriaLibro.Historia => "Historia",
        CategoriaLibro.Arte => "Arte",
        CategoriaLibro.Infantil => "Infantil",
        CategoriaLibro.Referencia => "Referencia",
        _ => "Otros"
    };
}
