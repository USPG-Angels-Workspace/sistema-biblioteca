using System.Text.Json.Serialization;

namespace Biblioteca.Core.Modelos;

public class Bibliotecario : Persona
{
    [JsonInclude] public string Cargo { get; private set; } = string.Empty;

    public override string TipoUsuario => "Bibliotecario";
    public override string EtiquetaDetalle => "Cargo";
    public override string Detalle => Cargo;
    public override int LimitePrestamos => 5;
    public override int DiasPrestamo => 14;

    [JsonConstructor]
    private Bibliotecario() { }

    public Bibliotecario(string nombre, string documento, string correo, string telefono,
                         string cargo, DateTime fechaRegistro)
        : base(nombre, documento, correo, telefono, fechaRegistro)
    {
        Cargo = ValidarDetalle(cargo);
    }

    protected override string ValidarDetalle(string? detalle) =>
        Validador.TextoRequerido(detalle, "Cargo", 60);

    protected override void AsignarDetalle(string detalle) => Cargo = detalle;
}
