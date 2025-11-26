using chefPro.Models;
using chefPro.Services;
using SQLite;
using System.Collections.ObjectModel;

namespace chefPro.Views;

public partial class BuscarReceta : ContentPage
{
    private RecetaService _service;
    private ObservableCollection<Receta> recetas;

    public BuscarReceta()
    {
        InitializeComponent();

        _service = new RecetaService();
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

        var resultados = await _service.BuscarRecetasPorTituloAsync(texto);

        foreach (var r in resultados)
        {
            r.Ingredientes = await _service.ObtenerIngredientesPorRecetaAsync(r.id_receta);
        }

        recetas.Clear();
        foreach (var r in resultados)
            recetas.Add(r);

        ResultadosList.IsVisible = resultados.Count > 0;

        if (resultados.Count == 0)
        {
            await DisplayAlert("Aviso", "No se encontró la receta.", "OK");
        }
    }

    private async void Receta_Tap(object sender, EventArgs e)
    {
        var frame = sender as Frame;
        var receta = frame.BindingContext as Receta;

        if (receta != null)
        {
            await Navigation.PushAsync(new RecetaDetalle(receta));
        }
    }
    private async void BtnRegresar_Clicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new vInicio());
    }


}