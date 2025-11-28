using chefPro.Models;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.Net;
using System.Net.Http;

namespace chefPro.Views;

public partial class vInicio : ContentPage
{
    private string nombreUsuario;
    private int id_usuario;

    /* Obtener todas las recetas para mostrar como un dashboard */
    private const string URL = "http://192.168.0.106/wsChefPro/recetas";
    private HttpClient client = new HttpClient();
    private int _idUsuario;

    public ObservableCollection<Receta> ListaRecetas { get; set; }

    public vInicio()
    {
        InitializeComponent();
        // Valores por defecto
        NombreUsuarioLabel.Text = "Chef invitado cocinando";
        ListaRecetas = new ObservableCollection<Receta>();
        BindingContext = this;
        /*RecetasCreadasLabel.Text = "Recetas creadas: 0";
        RecetasCompradasLabel.Text = "Recetas compradas: 0";*/

        NavigationPage.SetHasNavigationBar(this, false);
    }
    public vInicio(string nombreUsuarioRegistrado, int usuario)
    {
        InitializeComponent();

        nombreUsuario = nombreUsuarioRegistrado;
        id_usuario = usuario;

        ListaRecetas = new ObservableCollection<Receta>();
        BindingContext = this;
        // Asignar texto al Label
        NombreUsuarioLabel.Text = $"Chef {nombreUsuario} cocinando";

        // Contadores iniciales
        /*RecetasCreadasLabel.Text = "Recetas creadas: 12";
        RecetasCompradasLabel.Text = "Recetas compradas: 8";*/

        NavigationPage.SetHasNavigationBar(this, false);

        CargarRecetas();
    }

    private async void CargarRecetas()
    {
        try
        {
            var content = await client.GetStringAsync($"{URL}/listarRecetas");
            List<Receta> recetas = JsonConvert.DeserializeObject<List<Receta>>(content);

            ListaRecetas.Clear();

            if (recetas != null && recetas.Count > 0)
            {
                foreach (var receta in recetas)
                {
                    receta.UsuarioActual = (receta.id_usuario == id_usuario);

                    ListaRecetas.Add(receta);
                }
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"No se pudieron cargar las recetas: {ex.Message}", "OK");
        }
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        Random rnd = new Random();
        /* NotaMotivacional.Text = notas[rnd.Next(notas.Length)];*/
    }

    private readonly string[] notas = new string[]
    {
        "La cocina es el arte de compartir.",
        "Cada receta es una nueva aventura.",
        "Cocinar es amar a quienes alimentarás.",
        "Un chef feliz hace platos felices."
    };

    private async void CerrarSesion_Clicked(object sender, EventArgs e)
    {
        bool answer = await DisplayAlert("Confirmación", "¿Deseas cerrar sesión?", "Sí", "No");
        if (answer)
            await Navigation.PopToRootAsync();
    }

    private async void CrearReceta_Clicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new AgregarReceta(id_usuario));
    }

    private async void BuscarReceta_Clicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new BuscarReceta(id_usuario));
    }


    private void btnEstadisticas_Clicked(object sender, EventArgs e)
    {
        Navigation.PushAsync(new vEstadistica());

    }

    private void btnIngredientes_Clicked(object sender, EventArgs e)
    {
        Navigation.PushAsync(new vListaIngredientes(id_usuario));
    }

    private async void OnRecetaSeleccionada(object sender, EventArgs e)
    {
        // El sender es el Frame, el TapGestureRecognizer está en los EventArgs
        var tappedEventArgs = (TappedEventArgs)e;
        var receta = (Receta)tappedEventArgs.Parameter;

        // Navegar a la página de detalle
        await Navigation.PushAsync(new RecetaDetalle(receta, id_usuario, nombreUsuario));
    }

    private async void OnEditarReceta(object sender, EventArgs e)
    {
        var tappedEventArgs = (TappedEventArgs)e;
        var receta = (Receta)tappedEventArgs.Parameter;

        await DisplayAlert("Editar", $"Editar receta: {receta.titulo}", "OK");
        // await Navigation.PushAsync(new EditarReceta(receta));
    }

    private async void OnEliminarReceta(object sender, EventArgs e)
    {
        var tappedEventArgs = (TappedEventArgs)e;
        var receta = (Receta)tappedEventArgs.Parameter;

        bool confirmacion = await DisplayAlert(
            "Confirmar eliminación",
            $"¿Estás seguro de eliminar '{receta.titulo}'?",
            "Sí",
            "No"
        );

        if (confirmacion)
        {
            await EliminarReceta(receta.id_receta);
        }
    }

    private async Task EliminarReceta(int idReceta)
    {
        try
        {
            var response = await client.DeleteAsync(
                $"https://tudominio.com/chefPro/receta.php?id={idReceta}"
            );

            if (response.IsSuccessStatusCode)
            {
                await DisplayAlert("Éxito", "Receta eliminada correctamente", "OK");
                CargarRecetas(); // Recargar la lista
            }
            else
            {
                await DisplayAlert("Error", "No se pudo eliminar la receta", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Error al eliminar: {ex.Message}", "OK");
        }
    }

}