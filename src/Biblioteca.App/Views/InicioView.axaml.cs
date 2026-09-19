using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Biblioteca.Core;
using Biblioteca.Core.Modelos;
using Biblioteca.Core.Politicas;

namespace Biblioteca.App.Views;

public partial class InicioView : UserControl
{
    private readonly Action<string> _navegar;

    public InicioView(SistemaBiblioteca sistema, Action<string> navegar)
    {
        InitializeComponent();
        _navegar = navegar;

        Fecha.Text = DateTime.Today.ToString("dddd, d 'de' MMMM 'de' yyyy", new CultureInfo("es-GT"));

        var libros = sistema.Libros.Listar();
        var usuarios = sistema.Usuarios.Listar();
        var pendientes = sistema.Prestamos.Listar(soloPendientes: true);
        var atrasados = pendientes.Count(p => p.Estado == EstadoPrestamo.Atrasado);
        var multasPendientes = sistema.Multas.Listar(pagadas: false).Sum(m => m.Monto);

        Tarjetas.Children.Add(Tarjeta("Libros", libros.Count.ToString(), $"{libros.Sum(l => l.CopiasDisponibles)} de {libros.Sum(l => l.CopiasTotales)} ejemplares disponibles", "#1F4E79"));
        Tarjetas.Children.Add(Tarjeta("Usuarios", usuarios.Count.ToString(), $"{usuarios.Count(u => u.Activo)} activos", "#2E7D6B"));
        Tarjetas.Children.Add(Tarjeta("Préstamos activos", pendientes.Count.ToString(), "sin devolver", "#6A4C93"));
        Tarjetas.Children.Add(Tarjeta("Atrasados", atrasados.ToString(), "pasaron su fecha de devolución", atrasados > 0 ? "#B3261E" : "#5F6B7A"));
        Tarjetas.Children.Add(Tarjeta("Multas pendientes", "Q" + multasPendientes.ToString("N2", CultureInfo.InvariantCulture), "por cobrar", multasPendientes > 0 ? "#B26A00" : "#5F6B7A"));

        Reglas.Text =
            $"• Lector: hasta 3 libros a la vez, 7 días de préstamo.   • Bibliotecario: hasta 5 libros, 14 días.\n" +
            $"• Cada préstamo puede renovarse hasta {Prestamo.MaximoRenovaciones} veces si no está vencido.\n" +
            $"• Multa por atraso: Q{MultaPorDia.TarifaDiaria:N2} por día (máximo Q{MultaPorDia.TopeMaximo:N2}). " +
            "Con multas pendientes o libros atrasados no se pueden hacer nuevos préstamos.";
    }

    private static Border Tarjeta(string titulo, string valor, string detalle, string color) => new()
    {
        Classes = { "card" },
        Margin = new Thickness(0, 0, 12, 0),
        Child = new StackPanel
        {
            Spacing = 4,
            Children =
            {
                new TextBlock { Text = titulo, Classes = { "subtitulo" } },
                new TextBlock { Text = valor, FontSize = 32, FontWeight = FontWeight.Bold, Foreground = new SolidColorBrush(Color.Parse(color)) },
                new TextBlock { Text = detalle, Classes = { "subtitulo" }, TextWrapping = TextWrapping.Wrap }
            }
        }
    };

    private void Acceso_Click(object? sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: string destino })
            _navegar(destino);
    }
}
