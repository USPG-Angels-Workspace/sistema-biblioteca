namespace Biblioteca.Core.Excepciones;

/// <summary>Se lanza cuando no se puede leer o escribir un archivo de datos.</summary>
public class AlmacenamientoException : Exception
{
    public AlmacenamientoException(string mensaje, Exception? causa = null) : base(mensaje, causa) { }
}
