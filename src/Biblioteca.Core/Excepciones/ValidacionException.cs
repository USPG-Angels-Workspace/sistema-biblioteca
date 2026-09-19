namespace Biblioteca.Core.Excepciones;

/// <summary>Se lanza cuando un dato o una operación incumple una regla del negocio.</summary>
public class ValidacionException : Exception
{
    public ValidacionException(string mensaje) : base(mensaje) { }
}
