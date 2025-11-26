using chefPro.Models;

using chefPro.Services;

namespace chefPro.Views;

public partial class RecetaDetalle : ContentPage
{
    private readonly RecetaService _recetaService;
    private readonly Receta _receta;

    public RecetaDetalle(Receta receta)
    {
        InitializeComponent();
        _recetaService = new RecetaService();
        _receta = receta;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // Cargar ingredientes desde la base de datos usando el id correcto
        _receta.Ingredientes = await _recetaService.ObtenerIngredientesPorRecetaAsync(_receta.id_receta);

        // Asignar BindingContext después de cargar los ingredientes
        BindingContext = _receta;
    }

    private async void BtnRegresar_Clicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }

    private async void BtnComprar_Clicked(object sender, EventArgs e)
    {
        await DisplayAlert("Comprar", "Funcionalidad de compra no implementada.", "OK");
    }
}