using chefPro.Models;
using Newtonsoft.Json;
using System.Net;

namespace chefPro.Views;

public partial class vIngredientes : ContentPage
{
    private Ingrediente _ingredienteActual;
    private bool _modoEdicion;
    private int _idUsuario;

    private const string URL_BASE = AppConfig.URL_BASE;

    public vIngredientes(int id_usuario)
    {
        InitializeComponent();
        _idUsuario = id_usuario;
        _modoEdicion = false;
        LblTitulo.Text = "Agregar Ingrediente";
        EventoCalcularTotal();

        NavigationPage.SetHasNavigationBar(this, false);
    }

    public vIngredientes(int id_usuario, Ingrediente ingrediente)
    {
        InitializeComponent();
        _idUsuario = id_usuario;
        _modoEdicion = true;
        _ingredienteActual = ingrediente;
        LblTitulo.Text = "Editar Ingrediente";
        CargarDatos();
        EventoCalcularTotal();
        CalcularCostoTotalActualizar();
    }

    private void CalcularCostoTotalActualizar()
    {
        if (decimal.TryParse(EntryPeso.Text, out decimal peso) &&
            decimal.TryParse(EntryCosto.Text, out decimal costo))
        {
            decimal total = peso * costo;
            LblPreview.Text = $"Costo total: ${total:F2}";
        }
        else
        {
            LblPreview.Text = "Costo total: $0.00";
        }
    }

    private void EventoCalcularTotal()
    {
        EntryPeso.TextChanged += EventoPrecioCosto;
        EntryCosto.TextChanged += EventoPrecioCosto;
    }

    private void CargarDatos()
    {
        EntryNombre.Text = _ingredienteActual.nombre;
        EntryPeso.Text = _ingredienteActual.peso.ToString();
        EntryCosto.Text = _ingredienteActual.costo_unidad.ToString();

        string unidadCompleta = MapearUnidadBDaPicker(_ingredienteActual.unidad_medida);
        PickerUnidad.SelectedItem = unidadCompleta;
    }

    private string MapearUnidadBDaPicker(string unidadBD)
    {
        // Normalizar a minúsculas para comparar
        string unidad = unidadBD.ToLower().Trim();

        switch (unidad)
        {
            case "gramos":
                return "Gramos (g)";
            case "kilogramos":
                return "Kilogramos (kg)";
            case "mililitros":
                return "Mililitros (ml)";
            case "litros":
                return "Litros (L)";
            case "unidad":
                return "Unidad (u)";
            case "taza":
                return "Taza (cup)";
            case "cucharada":
                return "Cucharada (tbsp)";
            case "cucharadita":
                return "Cucharadita (tsp)";
            case "pizca":
                return "Pizca";
            default:
                return "Gramos (g)"; // Valor por defecto
        }
    }

    private void EventoPrecioCosto(object sender, TextChangedEventArgs e)
    {
        if (decimal.TryParse(EntryPeso.Text, out decimal peso) &&
            decimal.TryParse(EntryCosto.Text, out decimal costo))
        {
            decimal total = peso * costo;
            LblPreview.Text = $"Costo total: ${total:F2}";
        }
        else
        {
            LblPreview.Text = "Costo total: $0.00";
        }
    }

    private async void BtnCancelar_Clicked(object sender, EventArgs e)
    {
        bool confirmar = await DisplayAlert(
            "Cancelar",
            "¿Estás seguro de cancelar? Los cambios no guardados se perderán.",
            "Sí, cancelar",
            "No"
        );

        if (confirmar)
        {
            await Navigation.PopModalAsync();
        }
    }

    private async void BtnGuardar_Clicked(object sender, EventArgs e)
    {
        try
        {
            if (!ValidarCampos())
                return;

            BtnGuardar.IsEnabled = false;
            BtnGuardar.Text = "Guardando...";

            WebClient cliente = new WebClient();
            byte[] respuestaBytes;
            string url;
            string mensaje;

            // Crear nuevo ingrediente — POST
            if (!_modoEdicion)
            {
                var parametros = new System.Collections.Specialized.NameValueCollection();
                parametros.Add("nombre", EntryNombre.Text.Trim());
                parametros.Add("peso", EntryPeso.Text.Trim());
                parametros.Add("unidad_medida", PickerUnidad.SelectedItem.ToString());
                parametros.Add("costo_unidad", EntryCosto.Text.Trim());
                parametros.Add("id_usuario", _idUsuario.ToString());

                url = $"{URL_BASE}/ingredientes/registrar";
                respuestaBytes = cliente.UploadValues(url, "POST", parametros);
                mensaje = "agregado";

                string respuesta = System.Text.Encoding.UTF8.GetString(respuestaBytes);
                await DisplayAlert("Éxito", $"Ingrediente {mensaje} correctamente", "OK");
            }
            else
            {
                // Editar ingrediente existente — PUT
                url = $"{URL_BASE}/ingredientes/{_ingredienteActual.id_ingrediente}";

                // Crear el objeto JSON para enviar en el body
                var datos = new
                {
                    nombre = EntryNombre.Text.Trim(),
                    peso = EntryPeso.Text.Trim(),
                    unidad_medida = PickerUnidad.SelectedItem.ToString(),
                    costo_unidad = EntryCosto.Text.Trim()
                };

                string jsonData = Newtonsoft.Json.JsonConvert.SerializeObject(datos);

                cliente.Headers[HttpRequestHeader.ContentType] = "application/json";
                string respuesta = cliente.UploadString(url, "PUT", jsonData);

                await DisplayAlert("Éxito", "Ingrediente actualizado correctamente", "OK");
            }

            await Navigation.PopModalAsync();
        }
        catch (WebException wex)
        {
            try
            {
                using (var stream = wex.Response.GetResponseStream())
                using (var reader = new System.IO.StreamReader(stream))
                {
                    string errorResponse = reader.ReadToEnd();
                    var error = System.Text.Json.JsonSerializer.Deserialize<ErrorResponse>(errorResponse);
                    await DisplayAlert("Error", error.error, "OK");
                }
            }
            catch
            {
                await DisplayAlert("Error", "Error de conexión", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", "Ocurrió un problema:\n" + ex.Message, "OK");
        }
        finally
        {
            BtnGuardar.IsEnabled = true;
            BtnGuardar.Text = "Guardar";
        }
    }

    private bool ValidarCampos()
    {
        if (string.IsNullOrWhiteSpace(EntryNombre.Text))
        {
            DisplayAlert("Validación", "Ingresa el nombre del ingrediente", "OK");
            return false;
        }

        if (string.IsNullOrWhiteSpace(EntryPeso.Text) || !decimal.TryParse(EntryPeso.Text, out decimal peso) || peso <= 0)
        {
            DisplayAlert("Validación", "Ingresa un peso válido mayor a 0", "OK");
            return false;
        }

        if (PickerUnidad.SelectedItem == null)
        {
            DisplayAlert("Validación", "Selecciona una unidad de medida", "OK");
            return false;
        }

        if (string.IsNullOrWhiteSpace(EntryCosto.Text) || !decimal.TryParse(EntryCosto.Text, out decimal costo) || costo <= 0)
        {
            DisplayAlert("Validación", "Ingresa un costo válido mayor a 0", "OK");
            return false;
        }

        return true;
    }
}