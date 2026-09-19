using Biblioteca.Core.Interfaces;

namespace Biblioteca.Core.Politicas;

public class RelojSistema : IReloj
{
    public DateTime Hoy => DateTime.Today;
}
