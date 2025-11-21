
using chefPro.Models;
using chefPro.Services;
using System.Collections.ObjectModel;


namespace chefPro.Views;

public partial class AgregarReceta : ContentPage
{
    private RecetaService _service;
    private ObservableCollection<Ingrediente> ingredientes;
    private string fotoPath;

    private int idUsuarioLogueado = 1; // Cambiar según usuario logueado

    public AgregarReceta()
    {
        InitializeComponent();
        _service = new RecetaService();
        ingredientes = new ObservableCollection<Ingrediente>();
        cvIngredientes.ItemsSource = ingredientes;

        // Puedes habilitar mostrar nombre de usuario si implementas login
        // string nombreUsuario = ObtenerNombreUsuario(idUsuarioLogueado);
        // lblUsuarioActivo.Text = $"Chef {nombreUsuario} está cocinando.";
    }

    private void BtnAgregarIngrediente_Clicked(object sender, EventArgs e)
    {
        ingredientes.Add(new Ingrediente());
    }

    private void BtnEliminarIngrediente_Clicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is Ingrediente ing)
        {
            ingredientes.Remove(ing);
        }
    }

    private async void BtnFoto_Clicked(object sender, EventArgs e)
    {
        try
        {
            string opcion = await DisplayActionSheet("Selecciona una opción", "Cancelar", null, "Tomar Foto", "Seleccionar de Galería");

            if (opcion == "Tomar Foto")
            {
                var photo = await MediaPicker.CapturePhotoAsync();
                if (photo != null)
                {
                    var stream = await photo.OpenReadAsync();
                    imgFoto.Source = ImageSource.FromStream(() => stream);
                    fotoPath = photo.FullPath;
                }
            }
            else if (opcion == "Seleccionar de Galería")
            {
                var photo = await MediaPicker.PickPhotoAsync();
                if (photo != null)
                {
                    var stream = await photo.OpenReadAsync();
                    imgFoto.Source = ImageSource.FromStream(() => stream);
                    fotoPath = photo.FullPath;
                }
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }

    private async void BtnGuardar_Clicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(txtTitulo.Text))
        {
            await DisplayAlert("Aviso", "Ingrese el título de la receta.", "OK");
            return;
        }

        if (ingredientes.Count == 0)
        {
            await DisplayAlert("Aviso", "Agregue al menos un ingrediente.", "OK");
            return;
        }

        var receta = new Receta
        {
            titulo = txtTitulo.Text.Trim(),
            descripcion = txtDescripcion.Text?.Trim(),
            instrucciones = txtInstrucciones.Text?.Trim(),
            tiempo_preparacion = double.TryParse(txtTiempo.Text, out double t) ? t : 0,
            peso_porcion = double.TryParse(txtPesoPorcion.Text, out double pp) ? pp : 0,
            costo_receta = double.TryParse(txtCosto.Text, out double c) ? c : 0,
            fecha_creacion = DateTime.Now.ToString("yyyy-MM-dd"),
            id_usuario = idUsuarioLogueado,
            foto_url = fotoPath
        };

        // Calcular peso total sumando ingredientes
        receta.peso_total = ingredientes.Sum(i => i.cantidad);

        // Guardar receta y obtener id
        receta.id_receta = await _service.InsertRecetaAsync(receta);

        // Guardar ingredientes
        foreach (var ing in ingredientes)
        {
            ing.id_receta = receta.id_receta;
            await _service.InsertIngredienteAsync(ing);
        }

        await DisplayAlert("Éxito", "Receta guardada correctamente.", "OK");

        // Ir a login reemplazando la pila de navegación
        Application.Current.MainPage = new NavigationPage(new vInicio());
    }

    private async void BtnRegresar_Clicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new vInicio());
    }
}