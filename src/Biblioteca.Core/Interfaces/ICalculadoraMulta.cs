namespace Biblioteca.Core.Interfaces;

/// <summary>Estrategia para calcular el monto de una multa a partir de los días de atraso.</summary>
public interface ICalculadoraMulta
{
    decimal Calcular(int diasAtraso);
}
