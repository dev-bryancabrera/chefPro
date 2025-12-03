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
    private HttpClient client = new HttpClient();
    private WebClient cliente = new WebClient();
    private const string URL_BASE = AppConfig.URL_BASE;

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
            var content = await client.GetStringAsync($"{URL_BASE}/recetas/listarRecetas");
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

        // Solo cargar si hay un usuario válido
        if (id_usuario > 0)
        {
            CargarRecetas();
        }
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
        await Navigation.PushAsync(new AgregarReceta(id_usuario, nombreUsuario));
    }

    private async void BuscarReceta_Clicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new BuscarReceta(id_usuario));
    }


    private void btnEstadisticas_Clicked(object sender, EventArgs e)
    {
        Navigation.PushAsync(new vEstadistica(id_usuario));

    }

    private void btnIngredientes_Clicked(object sender, EventArgs e)
    {
        Navigation.PushAsync(new vListaIngredientes(id_usuario));
    }

    private async void OnRecetaSeleccionada(object sender, EventArgs e)
    {
        try
        {
            var tappedEventArgs = (TappedEventArgs)e;
            var receta = (Receta)tappedEventArgs.Parameter;

            if (receta.id_usuario != id_usuario)
            {
                await RegistrarVistaReceta(receta.id_receta);
            }

            // Navegar a la página de detalle
            await Navigation.PushAsync(new RecetaDetalle(receta, id_usuario, nombreUsuario));
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Error al abrir receta: {ex.Message}", "OK");
        }
    }

    private async Task RegistrarVistaReceta(int idReceta)
    {
        try
        {
            var parametros = new System.Collections.Specialized.NameValueCollection();
            parametros.Add("id_receta", idReceta.ToString());
            parametros.Add("id_usuario", id_usuario.ToString());

            byte[] respuestaBytes = null;
            try
            {
                respuestaBytes = cliente.UploadValues($"{URL_BASE}/estadisticas/registrar_vista_receta", "POST", parametros);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al conectar con el servidor de recetas: {ex.Message}");
            }

            string respuestaReceta = System.Text.Encoding.UTF8.GetString(respuestaBytes);

            if (string.IsNullOrWhiteSpace(respuestaReceta))
            {
                throw new Exception("El servidor no devolvió ninguna respuesta al crear el evento capturado");
            }
        }
        catch (Exception ex)
        {
            // No mostramos error al usuario para no interrumpir la navegación
            System.Diagnostics.Debug.WriteLine($"Error al registrar vista de receta: {ex.Message}");
        }
    }

    private async void OnEditarReceta(object sender, EventArgs e)
    {
        // Con TapGestureRecognizer, el parameter viene en TappedEventArgs
        if (e is TappedEventArgs tappedEventArgs && tappedEventArgs.Parameter is Receta receta)
        {
            await Navigation.PushAsync(new AgregarReceta(receta, id_usuario, nombreUsuario));
        }
    }

    private async void OnEliminarReceta(object sender, EventArgs e)
    {
        if (e is TappedEventArgs tappedEventArgs && tappedEventArgs.Parameter is Receta receta)
        {
            bool confirmacion = await DisplayAlert(
                "Confirmar eliminación",
                $"¿Estás seguro de eliminar '{receta.titulo}'?\n\nEsta acción no se puede deshacer.",
                "Sí, eliminar",
                "Cancelar"
            );

            if (confirmacion)
            {
                await EliminarReceta(receta.id_receta);
            }
        }
    }

    private async Task EliminarReceta(int idReceta)
    {
        try
        {
            // Mostrar indicador de carga
            IsBusy = true;

            var response = await client.DeleteAsync(
                $"{URL_BASE}/recetas/eliminar?id={idReceta}"
            );

            if (response.IsSuccessStatusCode)
            {
                await DisplayAlert("Éxito", "Receta eliminada correctamente", "OK");
                CargarRecetas(); // Recargar la lista
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                await DisplayAlert("Error", $"No se pudo eliminar la receta: {errorContent}", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Error al eliminar: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }
}