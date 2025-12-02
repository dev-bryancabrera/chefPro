using chefPro.Models;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.Net;

namespace chefPro.Views;

public partial class vListaIngredientes : ContentPage
{
    private const string URL = "http://192.168.0.104/wsChefPro/ingredientes";
    private readonly HttpClient client = new HttpClient();
    private ObservableCollection<Ingrediente> _ingredientes;
    private bool _noHayIngredientes;
    private int _idUsuario;

    public ObservableCollection<Ingrediente> Ingredientes
    {
        get => _ingredientes;
        set
        {
            _ingredientes = value;
            OnPropertyChanged();
            NoHayIngredientes = _ingredientes == null || _ingredientes.Count == 0;
        }
    }

    public bool NoHayIngredientes
    {
        get => _noHayIngredientes;
        set
        {
            _noHayIngredientes = value;
            OnPropertyChanged();
        }
    }

    public vListaIngredientes(int id_usuario)
    {
        InitializeComponent();
        _idUsuario = id_usuario;
        BindingContext = this;
        CargarIngredientes();
        NavigationPage.SetHasNavigationBar(this, false);
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        CargarIngredientes();
    }

    private async void CargarIngredientes()
    {
        try
        {
            var content = await client.GetStringAsync($"{URL}/ingredientesUsuario?id_usuario={_idUsuario}");
            List<Ingrediente> listaIngredientes = JsonConvert.DeserializeObject<List<Ingrediente>>(content);

            if (listaIngredientes != null)
            {
                Ingredientes = new ObservableCollection<Ingrediente>(listaIngredientes);
            }
            else
            {
                Ingredientes = new ObservableCollection<Ingrediente>();
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"No se pudieron cargar los ingredientes: {ex.Message}", "OK");
            Ingredientes = new ObservableCollection<Ingrediente>();
        }
    }


    private async void btnActualizar_Clicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is Ingrediente ingrediente)
        {
            await Navigation.PushModalAsync(new vIngredientes(_idUsuario, ingrediente));
        }
    }

    private async void btnEliminar_Clicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is Ingrediente ingrediente)
        {
            bool cancelar = await DisplayAlert(
                "Confirmar eliminación",
                $"¿Estás seguro de eliminar '{ingrediente.nombre}'?",
                "Cancelar",
                "Sí, eliminar"
            );

            if (!cancelar)
            {
                try
                {
                    WebClient cliente = new WebClient();
                    string urlDelete = $"http://192.168.0.104/wsChefPro/ingredientes/" +
                         $"?id_ingrediente={ingrediente.id_ingrediente}";

                    string respuesta = cliente.UploadString(urlDelete, "DELETE", "");

                    Ingredientes.Remove(ingrediente);
                    await DisplayAlert("✅ Éxito", "Ingrediente eliminado correctamente", "OK");
                }
                catch (Exception ex)
                {
                    await DisplayAlert("Error", $"Error al eliminar: {ex.Message}", "OK");
                }
            }
        }
    }

    private void btnAgregar_Clicked(object sender, EventArgs e)
    {
        Navigation.PushModalAsync(new vIngredientes(_idUsuario));
    }
}