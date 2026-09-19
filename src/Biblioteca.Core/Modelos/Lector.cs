using System.Text.Json.Serialization;

namespace Biblioteca.Core.Modelos;

public class Lector : Persona
{
    [JsonInclude] public string Carnet { get; private set; } = string.Empty;

    [JsonIgnore] public override string TipoUsuario => "Lector";
    [JsonIgnore] public override string EtiquetaDetalle => "Carnet";
    [JsonIgnore] public override string Detalle => Carnet;
    [JsonIgnore] public override int LimitePrestamos => 3;
    [JsonIgnore] public override int DiasPrestamo => 7;

    [JsonConstructor]
    private Lector() { }

    public Lector(string nombre, string documento, string correo, string telefono,
                  string carnet, DateTime fechaRegistro)
        : base(nombre, documento, correo, telefono, fechaRegistro)
    {
        Carnet = ValidarDetalle(carnet);
    }

    protected override string ValidarDetalle(string? detalle) =>
        Validador.TextoRequerido(detalle, "Carnet", 20);

    protected override void AsignarDetalle(string detalle) => Carnet = detalle;
}
