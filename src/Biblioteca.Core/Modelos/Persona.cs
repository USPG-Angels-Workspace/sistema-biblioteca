using System.Text.Json.Serialization;

namespace Biblioteca.Core.Modelos;

/// <summary>Persona registrada en la biblioteca. Las reglas de préstamo dependen del tipo concreto (polimorfismo).</summary>
[JsonPolymorphic(TypeDiscriminatorPropertyName = "tipo")]
[JsonDerivedType(typeof(Lector), "lector")]
[JsonDerivedType(typeof(Bibliotecario), "bibliotecario")]
public abstract class Persona : EntidadBase
{
    [JsonInclude] public string Nombre { get; private set; } = string.Empty;
    [JsonInclude] public string Documento { get; private set; } = string.Empty;
    [JsonInclude] public string Correo { get; private set; } = string.Empty;
    [JsonInclude] public string Telefono { get; private set; } = string.Empty;
    [JsonInclude] public DateTime FechaRegistro { get; private set; }
    [JsonInclude] public bool Activo { get; private set; } = true;

    [JsonIgnore] public abstract string TipoUsuario { get; }
    [JsonIgnore] public abstract string EtiquetaDetalle { get; }
    [JsonIgnore] public abstract string Detalle { get; }
    [JsonIgnore] public abstract int LimitePrestamos { get; }
    [JsonIgnore] public abstract int DiasPrestamo { get; }

    protected Persona() { }

    protected Persona(string nombre, string documento, string correo, string telefono, DateTime fechaRegistro)
    {
        Nombre = Validador.TextoRequerido(nombre, "Nombre");
        Documento = Validador.Documento(documento);
        Correo = Validador.Correo(correo);
        Telefono = Validador.Telefono(telefono);
        FechaRegistro = fechaRegistro.Date;
    }

    protected abstract string ValidarDetalle(string? detalle);
    protected abstract void AsignarDetalle(string detalle);

    public void Actualizar(string nombre, string documento, string correo, string telefono,
                           string? detalle, bool activo)
    {
        var nombreValido = Validador.TextoRequerido(nombre, "Nombre");
        var documentoValido = Validador.Documento(documento);
        var correoValido = Validador.Correo(correo);
        var telefonoValido = Validador.Telefono(telefono);
        var detalleValido = ValidarDetalle(detalle);

        Nombre = nombreValido;
        Documento = documentoValido;
        Correo = correoValido;
        Telefono = telefonoValido;
        AsignarDetalle(detalleValido);
        Activo = activo;
    }

    public bool Coincide(string? texto)
    {
        if (string.IsNullOrWhiteSpace(texto))
            return true;
        var buscado = texto.Trim();
        return Nombre.Contains(buscado, StringComparison.CurrentCultureIgnoreCase)
            || Documento.Contains(buscado, StringComparison.Ordinal)
            || Correo.Contains(buscado, StringComparison.CurrentCultureIgnoreCase)
            || Detalle.Contains(buscado, StringComparison.CurrentCultureIgnoreCase);
    }
}
