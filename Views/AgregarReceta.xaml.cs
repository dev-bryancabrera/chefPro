using chefPro.Models;
using chefPro.Services;
using System.Collections.ObjectModel;


namespace chefPro.Views
{
    public partial class AgregarReceta : ContentPage
    {
        private readonly RecetaService _service = new RecetaService();

        public ObservableCollection<Ingrediente> Ingredientes { get; set; } = new ObservableCollection<Ingrediente>();

        public ObservableCollection<string> Unidades { get; set; } = new ObservableCollection<string>
        {
            "kg", "g", "ml", "l", "pieza", "tsp", "tbsp"
        };

        public AgregarReceta()
        {
            InitializeComponent();
            NavigationPage.SetHasNavigationBar(this, false);
            cvIngredientes.ItemsSource = Ingredientes;

            BindingContext = this;
        }

        // AGREGAR INGREDIENTE
        private void BtnAgregarIngrediente_Clicked(object sender, EventArgs e)
        {
            Ingredientes.Add(new Ingrediente
            {
                nombre = "",
                cantidad = 0,
                unidad = "kg",
                precio = 0
            });
        }

        // ELIMINAR INGREDIENTE
        private void BtnEliminarIngrediente_Clicked(object sender, EventArgs e)
        {
            if (sender is Button btn && btn.CommandParameter is Ingrediente ingrediente)
            {
                Ingredientes.Remove(ingrediente);
            }
        }

        // FOTO (CÁMARA O GALERÍA)
        private async void BtnFoto_Clicked(object sender, EventArgs e)
        {
            try
            {
                string opcion = await DisplayActionSheet(
                    "Seleccionar opción",
                    "Cancelar",
                    null,
                    "Tomar foto",
                    "Elegir de galería");

                if (opcion == "Tomar foto")
                {
#if ANDROID
                    var photo = await MediaPicker.CapturePhotoAsync();
                    if (photo != null)
                    {
                        string ruta = await GuardarImagenLocal(photo);
                        imgFoto.Source = ruta;
                    }
#else
            await DisplayAlert("Aviso", "La cámara no está disponible en Windows.", "OK");
#endif
                }
                else if (opcion == "Elegir de galería")
                {
                    var result = await MediaPicker.PickPhotoAsync();
                    if (result != null)
                    {
                        string ruta = await GuardarImagenLocal(result);
                        imgFoto.Source = ruta;
                    }
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "OK");
            }
        }


        // GUARDAR RECETA
        private async void BtnGuardarReceta_Clicked(object sender, EventArgs e)
        {
            try
            {
                string titulo = txtTitulo.Text?.Trim();
                string descripcion = txtDescripcion.Text?.Trim();
                string instrucciones = txtInstrucciones.Text?.Trim();

                if (string.IsNullOrWhiteSpace(titulo))
                {
                    await DisplayAlert("Error", "El título es obligatorio.", "OK");
                    return;
                }

                if (string.IsNullOrWhiteSpace(descripcion))
                {
                    await DisplayAlert("Error", "La descripción es obligatoria.", "OK");
                    return;
                }

                double.TryParse(txtTiempo.Text, out double tiempo);
                double.TryParse(txtPorciones.Text, out double porciones);
                double.TryParse(txtValorVenta.Text, out double valorVenta);
                double.TryParse(txtPorcentaje.Text, out double porcentaje);

                if (porciones <= 0)
                {
                    await DisplayAlert("Error", "Las porciones deben ser mayor a 0.", "OK");
                    return;
                }

                double costoReceta = 0;
                double pesoTotal = 0;

                foreach (var ing in Ingredientes)
                {
                    double cantidadBase = ing.cantidad;

                    switch (ing.unidad.ToLower())
                    {
                        case "g":
                        case "ml":
                            cantidadBase = ing.cantidad / 1000;
                            break;

                        case "tsp":
                            cantidadBase = ing.cantidad * 5 / 1000;
                            break;

                        case "tbsp":
                            cantidadBase = ing.cantidad * 15 / 1000;
                            break;
                    }

                    costoReceta += cantidadBase * ing.precio;
                    pesoTotal += ing.cantidad;
                }

                double pesoPorcion = pesoTotal / porciones;

                // AUTOCÁLCULOS
                if (valorVenta > 0 && porcentaje <= 0)
                    porcentaje = ((valorVenta - costoReceta) / costoReceta) * 100;

                else if (valorVenta <= 0 && porcentaje > 0)
                    valorVenta = costoReceta * (1 + porcentaje / 100);

                else if (valorVenta <= 0 && porcentaje <= 0)
                {
                    porcentaje = 20;
                    valorVenta = costoReceta * 1.2;
                }

                // CREAR OBJETO RECETA
                var receta = new Receta
                {
                    titulo = titulo,
                    descripcion = descripcion,
                    instrucciones = instrucciones,
                    tiempo_preparacion = tiempo,
                    porciones = porciones,
                    valor_venta = valorVenta,
                    porcentaje_ganancia = porcentaje,
                    costo_receta = costoReceta,
                    peso_total = pesoTotal,
                    peso_porcion = pesoPorcion,
                    foto_url = (imgFoto.Source as FileImageSource)?.File ?? "",

                    fecha_creacion = DateTime.Now.ToString("yyyy-MM-dd"),
                    id_usuario = 1
                };

                int idReceta = await _service.InsertRecetaAsync(receta);

                foreach (var ing in Ingredientes)
                {
                    ing.id_receta = idReceta;
                    await _service.GuardarIngredienteAsync(ing);
                }

                await DisplayAlert("Guardado", "La receta y sus ingredientes se guardaron correctamente.", "OK");
                await Navigation.PushAsync(new vInicio());
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"No se pudo guardar la receta: {ex.Message}", "OK");
            }
        }

        // REGRESAR
        private async void BtnRegresar_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new vInicio());
        }
        private async Task<string> GuardarImagenLocal(FileResult file)
            {
                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
                var ruta = Path.Combine(FileSystem.AppDataDirectory, fileName);

                using var streamOriginal = await file.OpenReadAsync();
                using var streamDestino = File.OpenWrite(ruta);

                await streamOriginal.CopyToAsync(streamDestino);

                return ruta;
            }

    }
}