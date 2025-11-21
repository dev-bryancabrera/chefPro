using chefPro.Models;
using chefPro.Services;
using System.Collections.ObjectModel;

namespace chefPro.Views;

public partial class BuscarReceta : ContentPage
{
    private RecetaLocalService _service;
    private ObservableCollection<Receta> recetas;

    public BuscarReceta()
    {
        InitializeComponent();
        _service = new RecetaLocalService();
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

        // Llamamos al método correcto que devuelve lista
        var resultados = await _service.BuscarRecetasPorTituloAsync(texto);

        recetas.Clear();

        if (resultados.Count > 0)
        {
            foreach (var r in resultados)
                recetas.Add(r);

            ResultadosList.IsVisible = true; // Mostrar CollectionView
        }
        else
        {
            ResultadosList.IsVisible = false;
            await DisplayAlert("Aviso", "No se encontró la receta.", "OK");
        }
    }
}