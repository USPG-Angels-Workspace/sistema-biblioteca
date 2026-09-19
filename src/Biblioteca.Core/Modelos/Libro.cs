using System.Text.Json.Serialization;
using Biblioteca.Core.Excepciones;

namespace Biblioteca.Core.Modelos;

public class Libro : EntidadBase
{
    [JsonInclude] public string Isbn { get; private set; } = string.Empty;
    [JsonInclude] public string Titulo { get; private set; } = string.Empty;
    [JsonInclude] public string Autor { get; private set; } = string.Empty;
    [JsonInclude] public string Editorial { get; private set; } = string.Empty;
    [JsonInclude] public int Anio { get; private set; }
    [JsonInclude] public CategoriaLibro Categoria { get; private set; }
    [JsonInclude] public int CopiasTotales { get; private set; }
    [JsonInclude] public int CopiasDisponibles { get; private set; }

    [JsonIgnore] public bool Disponible => CopiasDisponibles > 0;
    [JsonIgnore] public int CopiasPrestadas => CopiasTotales - CopiasDisponibles;

    [JsonConstructor]
    private Libro() { }

    public Libro(string isbn, string titulo, string autor, string editorial, int anio,
                 CategoriaLibro categoria, int copias)
    {
        Isbn = Validador.Isbn(isbn);
        Titulo = Validador.TextoRequerido(titulo, "Título");
        Autor = Validador.TextoRequerido(autor, "Autor");
        Editorial = Validador.TextoRequerido(editorial, "Editorial");
        Anio = Validador.Anio(anio);
        Categoria = categoria;
        CopiasTotales = Validador.Copias(copias);
        CopiasDisponibles = CopiasTotales;
    }

    public void Actualizar(string isbn, string titulo, string autor, string editorial, int anio,
                           CategoriaLibro categoria, int copiasTotales)
    {
        var isbnValido = Validador.Isbn(isbn);
        var tituloValido = Validador.TextoRequerido(titulo, "Título");
        var autorValido = Validador.TextoRequerido(autor, "Autor");
        var editorialValida = Validador.TextoRequerido(editorial, "Editorial");
        var anioValido = Validador.Anio(anio);
        var copiasValidas = Validador.Copias(copiasTotales);

        if (copiasValidas < CopiasPrestadas)
            throw new ValidacionException(
                $"No se pueden reducir los ejemplares a {copiasValidas}: hay {CopiasPrestadas} en préstamo.");

        var prestadas = CopiasPrestadas;
        Isbn = isbnValido;
        Titulo = tituloValido;
        Autor = autorValido;
        Editorial = editorialValida;
        Anio = anioValido;
        Categoria = categoria;
        CopiasTotales = copiasValidas;
        CopiasDisponibles = copiasValidas - prestadas;
    }

    public void PrestarCopia()
    {
        if (!Disponible)
            throw new ValidacionException($"No hay ejemplares disponibles de «{Titulo}».");
        CopiasDisponibles--;
    }

    public void DevolverCopia()
    {
        if (CopiasDisponibles >= CopiasTotales)
            throw new ValidacionException($"Todos los ejemplares de «{Titulo}» ya están en la biblioteca.");
        CopiasDisponibles++;
    }

    public bool Coincide(string? texto)
    {
        if (string.IsNullOrWhiteSpace(texto))
            return true;
        var buscado = texto.Trim();
        return Contiene(Titulo, buscado) || Contiene(Autor, buscado)
            || Contiene(Editorial, buscado) || Contiene(Isbn, buscado.Replace("-", ""));
    }

    private static bool Contiene(string origen, string buscado) =>
        origen.Contains(buscado, StringComparison.CurrentCultureIgnoreCase);
}
