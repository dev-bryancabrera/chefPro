using chefPro.Models;
using chefPro.Services;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.Net;


namespace chefPro.Views
{
    public partial class AgregarReceta : ContentPage
    {
        private const string URL = "http://192.168.0.106/wsChefPro/ingredientes";
        private HttpClient client = new HttpClient();
        private WebClient cliente = new WebClient();
        private int _idUsuario;

        private string fotoUrlServidor = "";
        private FileResult fotoSeleccionada;

        public ObservableCollection<Ingrediente> ListaIngredientes { get; set; }

        // Lista de ingredientes agregados a la receta
        public ObservableCollection<RecetaIngrediente> IngredientesReceta { get; set; }

        public AgregarReceta(int id_usuario)
        {
            InitializeComponent();

            _idUsuario = id_usuario;

            NavigationPage.SetHasNavigationBar(this, false);
            IngredientesReceta = new ObservableCollection<RecetaIngrediente>();
            ListaIngredientes = new ObservableCollection<Ingrediente>();

            BindingContext = this;

            CargarIngredientes();
        }

        private async void CargarIngredientes()
        {
            try
            {
                var content = await client.GetStringAsync($"{URL}?id_usuario={_idUsuario}");
                List<Ingrediente> listaIngredientes = JsonConvert.DeserializeObject<List<Ingrediente>>(content);

                ListaIngredientes.Clear();
                if (listaIngredientes != null && listaIngredientes.Count > 0)
                {
                    foreach (var ingrediente in listaIngredientes)
                    {
                        ListaIngredientes.Add(ingrediente);
                    }
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"No se pudieron cargar los ingredientes: {ex.Message}", "OK");
                ListaIngredientes.Clear();
            }
        }

        // AGREGAR INGREDIENTE
        private void BtnAgregarIngrediente_Clicked(object sender, EventArgs e)
        {
            IngredientesReceta.Add(new RecetaIngrediente()
            {
                IdIngrediente = 0,
                NombreIngrediente = "",
                Cantidad = 0,
                CostoUnitario = 0,
                IngredienteSeleccionado = null
            });
        }

        private void OnIngredienteSeleccionado(object sender, EventArgs e)
        {
            var picker = sender as Picker;
            if (picker?.BindingContext is RecetaIngrediente recetaIng && picker.SelectedItem is Ingrediente ingrediente)
            {
                recetaIng.IdIngrediente = ingrediente.id_ingrediente;
                recetaIng.NombreIngrediente = ingrediente.nombre;
                recetaIng.CostoUnitario = ingrediente.costo_unidad;
                recetaIng.IngredienteSeleccionado = ingrediente;

                ActualizarCamposCalculados();
            }
        }

        // ELIMINAR INGREDIENTE
        private void BtnEliminarIngrediente_Clicked(object sender, EventArgs e)
        {
            var button = sender as Button;
            var ingrediente = button?.CommandParameter as RecetaIngrediente;

            if (ingrediente != null)
            {
                IngredientesReceta.Remove(ingrediente);
                ActualizarCamposCalculados();
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
                fotoSeleccionada = photo; // ✅ Asignar aquí
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
                        fotoSeleccionada = result; // ✅ Asignar aquí
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

        private void ActualizarCamposCalculados()
        {
            // Calcular costo total de la receta
            double costoReceta = 0;
            double pesoTotal = 0;

            foreach (var ing in IngredientesReceta)
            {
                if (ing.Cantidad > 0 && ing.CostoUnitario > 0)
                {
                    costoReceta += ing.Cantidad * ing.CostoUnitario;
                    pesoTotal += ing.Cantidad;
                }
            }

            // Mostrar valores calculados
            txtCostoReceta.Text = costoReceta.ToString("F2");
            txtPesoTotal.Text = pesoTotal.ToString("F2");

            // Calcular peso por porción
            if (!string.IsNullOrWhiteSpace(txtPorciones.Text))
            {
                if (int.TryParse(txtPorciones.Text, out int porciones) && porciones > 0)
                {
                    double pesoPorcion = pesoTotal / porciones;
                    txtPesoPorcion.Text = pesoPorcion.ToString("F2");
                }
            }
        }

        private void OnCampoNumericoChanged(object sender, TextChangedEventArgs e)
        {
            ActualizarCamposCalculados();
        }

        private async Task<string> SubirFotoAlServidor()
        {
            if (fotoSeleccionada == null)
                return "";

            try
            {
                using (var httpClient = new HttpClient())
                using (var content = new MultipartFormDataContent())
                {
                    // Leer el stream de la foto
                    var stream = await fotoSeleccionada.OpenReadAsync();
                    var streamContent = new StreamContent(stream);
                    streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");

                    // Agregar la foto al contenido
                    content.Add(streamContent, "foto", fotoSeleccionada.FileName);

                    // Agregar el título de la receta
                    content.Add(new StringContent(txtTitulo.Text.Trim()), "titulo");

                    // Enviar al servidor
                    var response = await httpClient.PostAsync(
                        "http://192.168.0.106/wsChefPro/recetas/subir_foto",
                        content
                    );

                    if (response.IsSuccessStatusCode)
                    {
                        var jsonResponse = await response.Content.ReadAsStringAsync();
                        var resultado = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonResponse);

                        if (resultado.ContainsKey("nombre_archivo"))
                        {
                            return resultado["nombre_archivo"].ToString();
                        }
                    }
                    else
                    {
                        var errorContent = await response.Content.ReadAsStringAsync();
                        throw new Exception($"Error del servidor: {errorContent}");
                    }

                    return "";
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Error al subir foto: {ex.Message}", "OK");
                return "";
            }
        }

        // GUARDAR RECETA
        private async void BtnGuardarReceta_Clicked(object sender, EventArgs e)
        {
            try
            {
                // Validaciones
                if (string.IsNullOrWhiteSpace(txtTitulo.Text))
                {
                    await DisplayAlert("Error", "Debe ingresar un título para la receta", "OK");
                    return;
                }

                if (IngredientesReceta.Count == 0)
                {
                    await DisplayAlert("Error", "Debe agregar al menos un ingrediente", "OK");
                    return;
                }

                foreach (var ing in IngredientesReceta)
                {
                    if (ing.IdIngrediente == 0 || ing.Cantidad <= 0)
                    {
                        await DisplayAlert("Error", "Complete todos los datos de los ingredientes", "OK");
                        return;
                    }
                }

                // Mostrar indicador de carga
                IsBusy = true;
                /*var loadingTask = DisplayAlert("Procesando", "Guardando receta, por favor espere...", "OK");*/

                try
                {
                    // PASO 1: Subir foto al servidor (si hay)
                    if (fotoSeleccionada != null)
                    {
                        fotoUrlServidor = await SubirFotoAlServidor();
                    }

                    // PASO 2: Calcular valores
                    double costoReceta = 0;
                    double pesoTotal = 0;

                    foreach (var ing in IngredientesReceta)
                    {
                        costoReceta += ing.Cantidad * ing.CostoUnitario;
                        pesoTotal += ing.Cantidad;
                    }

                    int porciones = 1;
                    if (!string.IsNullOrWhiteSpace(txtPorciones.Text))
                    {
                        int.TryParse(txtPorciones.Text, out porciones);
                        if (porciones <= 0) porciones = 1;
                    }

                    double pesoPorcion = pesoTotal / porciones;
                    double precioUnidad = costoReceta / porciones;

                    double valorVenta = 0;
                    if (!string.IsNullOrWhiteSpace(txtValorVenta.Text))
                    {
                        double.TryParse(txtValorVenta.Text, out valorVenta);
                    }

                    double porcentajeGanancia = 0;
                    if (!string.IsNullOrWhiteSpace(txtPorcentaje.Text))
                    {
                        double.TryParse(txtPorcentaje.Text, out porcentajeGanancia);
                    }
                    else if (valorVenta > 0 && costoReceta > 0)
                    {
                        porcentajeGanancia = ((valorVenta - costoReceta) / costoReceta) * 100;
                    }

                    // PASO 3: Registrar la receta
                    var parametrosReceta = new System.Collections.Specialized.NameValueCollection();
                    parametrosReceta.Add("id_usuario", _idUsuario.ToString());
                    parametrosReceta.Add("titulo", txtTitulo.Text.Trim());
                    parametrosReceta.Add("descripcion", txtDescripcion.Text?.Trim() ?? "");
                    parametrosReceta.Add("preparacion", txtInstrucciones.Text?.Trim() ?? "");
                    parametrosReceta.Add("tiempo_preparacion", txtTiempo.Text?.Trim() ?? "0");
                    parametrosReceta.Add("peso_total", pesoTotal.ToString("F2"));
                    parametrosReceta.Add("porciones", porciones.ToString());
                    parametrosReceta.Add("peso_porcion", pesoPorcion.ToString("F2"));
                    parametrosReceta.Add("valor_venta", valorVenta.ToString("F2"));
                    parametrosReceta.Add("costo_receta", costoReceta.ToString("F2"));
                    parametrosReceta.Add("precio_unidad", precioUnidad.ToString("F2"));
                    parametrosReceta.Add("porcentaje_ganancia", porcentajeGanancia.ToString("F2"));
                    parametrosReceta.Add("foto_url", fotoUrlServidor ?? "");

                    string urlReceta = "http://192.168.0.106/wsChefPro/recetas/registrar";

                    byte[] respuestaBytes = null;
                    try
                    {
                        respuestaBytes = cliente.UploadValues(urlReceta, "POST", parametrosReceta);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception($"Error al conectar con el servidor de recetas: {ex.Message}");
                    }

                    string respuestaReceta = System.Text.Encoding.UTF8.GetString(respuestaBytes);

                    // Validar que la respuesta no esté vacía
                    if (string.IsNullOrWhiteSpace(respuestaReceta))
                    {
                        throw new Exception("El servidor no devolvió ninguna respuesta al crear la receta");
                    }

                    // Deserializar la respuesta para obtener el ID de la receta
                    Dictionary<string, object> recetaCreada = null;
                    try
                    {
                        recetaCreada = JsonConvert.DeserializeObject<Dictionary<string, object>>(respuestaReceta);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception($"Error al procesar respuesta del servidor: {ex.Message}\nRespuesta: {respuestaReceta}");
                    }

                    if (recetaCreada == null || !recetaCreada.ContainsKey("id_receta"))
                    {
                        throw new Exception($"La respuesta del servidor no contiene el ID de la receta creada. Respuesta: {respuestaReceta}");
                    }

                    int idRecetaCreada = Convert.ToInt32(recetaCreada["id_receta"]);

                    // PASO 4: Registrar los ingredientes de la receta
                    string urlRecetaIngrediente = "http://192.168.0.106/wsChefPro/recetaIngrediente/registrar";
                    int ingredientesGuardados = 0;

                    foreach (var ingrediente in IngredientesReceta)
                    {
                        try
                        {
                            var parametrosIngrediente = new System.Collections.Specialized.NameValueCollection();
                            parametrosIngrediente.Add("id_receta", idRecetaCreada.ToString());
                            parametrosIngrediente.Add("id_ingrediente", ingrediente.IdIngrediente.ToString());
                            parametrosIngrediente.Add("cantidad", ingrediente.Cantidad.ToString("F2"));
                            parametrosIngrediente.Add("costo_unitario", ingrediente.CostoUnitario.ToString("F2"));

                            await Task.Run(() =>
                            {
                                cliente.UploadValues(urlRecetaIngrediente, "POST", parametrosIngrediente);
                            });

                            ingredientesGuardados++;
                        }
                        catch (Exception ex)
                        {
                            throw new Exception($"Error al guardar ingrediente '{ingrediente.NombreIngrediente}': {ex.Message}");
                        }
                    }

                    IsBusy = false;

                    await DisplayAlert("Éxito",
                        $"✅ Receta guardada correctamente\n\n" +
                        $"📝 Ingredientes guardados: {ingredientesGuardados}/{IngredientesReceta.Count}\n" +
                        $"💰 Costo total: ${costoReceta:F2}\n" +
                        $"🍽️ Precio/porción: ${precioUnidad:F2}\n" +
                        $"📈 Ganancia: {porcentajeGanancia:F2}%",
                        "OK");

                    await Navigation.PopAsync();
                }
                finally
                {
                    IsBusy = false;
                }
            }
            catch (WebException webEx)
            {
                IsBusy = false;
                string errorMessage = "Error de conexión con el servidor";

                if (webEx.Response != null)
                {
                    using (var stream = webEx.Response.GetResponseStream())
                    using (var reader = new StreamReader(stream))
                    {
                        errorMessage = await reader.ReadToEndAsync();
                    }
                }

                await DisplayAlert("Error de Conexión",
                    $"No se pudo conectar con el servidor:\n{errorMessage}\n\nDetalles técnicos: {webEx.Message}",
                    "OK");
            }
            catch (Exception ex)
            {
                IsBusy = false;
                await DisplayAlert("Error",
                    $"No se pudo guardar la receta:\n\n{ex.Message}\n\nTipo: {ex.GetType().Name}",
                    "OK");

                // Log para debugging
                System.Diagnostics.Debug.WriteLine($"Error completo: {ex}");
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