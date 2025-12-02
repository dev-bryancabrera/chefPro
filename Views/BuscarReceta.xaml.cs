using chefPro.Models;
using SQLite;
using System.Collections.ObjectModel;
using System.Net;
using System.Text.Json;

namespace chefPro.Views;

public partial class BuscarReceta : ContentPage
{
    private ObservableCollection<Receta> recetas;
    private int _idUsuario;

    // Constructor recibe el ID del usuario
    public BuscarReceta(int idUsuario)
    {
        InitializeComponent();
        _idUsuario = idUsuario;
        NavigationPage.SetHasNavigationBar(this, false);

        recetas = new ObservableCollection<Receta>();
        ResultadosList.ItemsSource = recetas;
    }

    private async void BtnBuscar_Clicked(object sender, EventArgs e)
    {
        string texto = txtBusqueda.Text?.Trim();

        if (string.IsNullOrEmpty(texto))
        {
            await DisplayAlert("Aviso", "Ingrese un título para buscar.", "OK");
            return;
        }

        try
        {
            using (WebClient cliente = new WebClient())
            {
                string url = $"http://192.168.0.102/wsChefPro/receta?titulo={Uri.EscapeDataString(texto)}";
                string respuesta = await cliente.DownloadStringTaskAsync(url);

                var resultados = JsonSerializer.Deserialize<List<Receta>>(respuesta);
                recetas.Clear();

                if (resultados != null)
                {
                    foreach (var r in resultados)
                        recetas.Add(r);
                }

                ResultadosList.IsVisible = recetas.Count > 0;

                if (recetas.Count == 0)
                    await DisplayAlert("Aviso", "No se encontró la receta.", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", "No se pudo conectar al servidor:\n" + ex.Message, "OK");
        }
    }

    private async void Receta_Tap(object sender, EventArgs e)
    {
        var frame = sender as Frame;
        var receta = frame?.BindingContext as Receta;

        if (receta != null)
        {
            string valorString = "valorEjemplo"; // Cambia esto según lo que tu constructor necesite
            await Navigation.PushAsync(new RecetaDetalle(receta, _idUsuario, valorString));
        }
    }

    private async void BtnRegresar_Clicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new vInicio()); // Si tu vInicio también requiere id_usuario, pásalo aquí
    }
}