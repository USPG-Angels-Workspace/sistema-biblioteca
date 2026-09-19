using System.Net.Mail;
using Biblioteca.Core.Excepciones;

namespace Biblioteca.Core.Modelos;

/// <summary>Reglas de validación compartidas por las entidades del dominio.</summary>
public static class Validador
{
    public static string TextoRequerido(string? valor, string campo, int maximo = 120)
    {
        var texto = (valor ?? string.Empty).Trim();
        if (texto.Length == 0)
            throw new ValidacionException($"El campo «{campo}» es obligatorio.");
        if (texto.Length > maximo)
            throw new ValidacionException($"El campo «{campo}» no puede superar {maximo} caracteres.");
        return texto;
    }

    public static string TextoOpcional(string? valor, string campo, int maximo = 120)
    {
        var texto = (valor ?? string.Empty).Trim();
        if (texto.Length > maximo)
            throw new ValidacionException($"El campo «{campo}» no puede superar {maximo} caracteres.");
        return texto;
    }

    public static string Isbn(string? valor)
    {
        var texto = TextoRequerido(valor, "ISBN", 20);
        var digitos = texto.Replace("-", "").Replace(" ", "");
        if (!(digitos.Length is 10 or 13) || !digitos.All(char.IsDigit))
            throw new ValidacionException("El ISBN debe tener 10 o 13 dígitos (se permiten guiones).");
        return digitos;
    }

    public static int Anio(int anio)
    {
        var maximo = DateTime.Today.Year + 1;
        if (anio < 1000 || anio > maximo)
            throw new ValidacionException($"El año de publicación debe estar entre 1000 y {maximo}.");
        return anio;
    }

    public static int Copias(int copias)
    {
        if (copias < 1 || copias > 999)
            throw new ValidacionException("La cantidad de ejemplares debe estar entre 1 y 999.");
        return copias;
    }

    public static string Documento(string? valor)
    {
        var texto = TextoRequerido(valor, "Documento (DPI)", 20).Replace(" ", "");
        if (texto.Length != 13 || !texto.All(char.IsDigit))
            throw new ValidacionException("El documento (DPI) debe tener exactamente 13 dígitos.");
        return texto;
    }

    public static string Correo(string? valor)
    {
        var texto = TextoRequerido(valor, "Correo electrónico", 120);
        try
        {
            var direccion = new MailAddress(texto);
            if (direccion.Address != texto || !texto.Contains('.', StringComparison.Ordinal))
                throw new FormatException();
        }
        catch (FormatException)
        {
            throw new ValidacionException("El correo electrónico no tiene un formato válido.");
        }
        return texto;
    }

    public static string Telefono(string? valor)
    {
        var texto = TextoRequerido(valor, "Teléfono", 20);
        var digitos = texto.Replace("-", "").Replace(" ", "");
        if (digitos.Length is < 8 or > 15 || !digitos.All(char.IsDigit))
            throw new ValidacionException("El teléfono debe tener entre 8 y 15 dígitos.");
        return digitos;
    }
}
