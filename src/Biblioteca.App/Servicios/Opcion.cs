namespace Biblioteca.App.Servicios;

/// <summary>Elemento de un ComboBox: un valor asociado y el texto que se muestra.</summary>
public record Opcion<T>(T Valor, string Texto)
{
    public override string ToString() => Texto;
}
