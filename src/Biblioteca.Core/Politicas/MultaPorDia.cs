using Biblioteca.Core.Interfaces;

namespace Biblioteca.Core.Politicas;

/// <summary>Multa lineal: una tarifa fija por cada día de atraso, con un tope máximo.</summary>
public class MultaPorDia : ICalculadoraMulta
{
    public const decimal TarifaDiaria = 2.00m;
    public const decimal TopeMaximo = 100.00m;

    public decimal Calcular(int diasAtraso)
    {
        if (diasAtraso <= 0)
            return 0m;
        return Math.Min(diasAtraso * TarifaDiaria, TopeMaximo);
    }
}
