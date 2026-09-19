using System.Text;

namespace Biblioteca.Core.Servicios;

/// <summary>Reporte genérico: un título, columnas, filas de texto y un resumen. Se muestra en pantalla y se exporta a CSV.</summary>
public record ReporteTabular(
    string Titulo,
    IReadOnlyList<string> Columnas,
    IReadOnlyList<IReadOnlyList<string>> Filas,
    string Resumen)
{
    public string ACsv()
    {
        var sb = new StringBuilder();
        sb.AppendLine(string.Join(",", Columnas.Select(Escapar)));
        foreach (var fila in Filas)
            sb.AppendLine(string.Join(",", fila.Select(Escapar)));
        return sb.ToString();
    }

    private static string Escapar(string valor) =>
        valor.IndexOfAny(new[] { ',', '"', '\n', '\r' }) >= 0
            ? "\"" + valor.Replace("\"", "\"\"") + "\""
            : valor;
}
