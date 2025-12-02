using chefPro.Models;
using Newtonsoft.Json;

namespace chefPro.Views;

public partial class RecetaDetalle : ContentPage
{
    private Receta _receta;
    private int _idUsuario;
    private string _nombreUsuario;
    HttpClient client = new HttpClient();
    private const string URL = "http://192.168.0.104/wsChefPro/recetaIngrediente"; // Cambia por tu URL


    public RecetaDetalle(Receta receta, int id_usuario, string usuario)
    {
        InitializeComponent();
        NavigationPage.SetHasNavigationBar(this, false);

        _receta = receta;
        _idUsuario = id_usuario;
        _nombreUsuario = usuario;

        // Establecer el binding context con la receta
        BindingContext = _receta;

        // Cargar los ingredientes desde la API
        CargarIngredientes();
    }

    private async void CargarIngredientes()
    {
        try
        {
            var content = await client.GetStringAsync($"{URL}/ingredientesReceta?id={_receta.id_receta}");
            var ingredientes = JsonConvert.DeserializeObject<List<Ingrediente>>(content);

            if (ingredientes != null && ingredientes.Count > 0)
            {
                _receta.Ingredientes = ingredientes;

                // Refrescar el binding para que se muestren los ingredientes
                BindingContext = null;
                BindingContext = _receta;
            }
            else
            {
                await DisplayAlert("Info", "Esta receta no tiene ingredientes registrados", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"No se pudieron cargar los ingredientes: {ex.Message}", "OK");
        }
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