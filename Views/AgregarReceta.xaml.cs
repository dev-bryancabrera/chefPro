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
            cvIngredientes.ItemsSource = Ingredientes;
            this.BindingContext = this;
        }

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

        private void BtnEliminarIngrediente_Clicked(object sender, EventArgs e)
        {
            if (sender is Button btn && btn.CommandParameter is Ingrediente ingrediente)
            {
                Ingredientes.Remove(ingrediente);
            }
        }

        private async void BtnFoto_Clicked(object sender, EventArgs e)
        {
            try
            {
                var result = await MediaPicker.Default.PickPhotoAsync();
                if (result != null)
                {
                    var stream = await result.OpenReadAsync();
                    imgFoto.Source = ImageSource.FromStream(() => stream);
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "OK");
            }
        }

        private async void BtnGuardarReceta_Clicked(object sender, EventArgs e)
        {
            try
            {
                // --- LEER DATOS DE LOS ENTRY ---
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

                // --- CÁLCULOS AUTOMÁTICOS DEL COSTO ---
                double costoReceta = 0;
                double pesoTotal = 0;

                foreach (var ing in Ingredientes)
                {
                    double cantidadBase = ing.cantidad;

                    switch (ing.unidad.ToLower())
                    {
                        case "g":
                            cantidadBase = ing.cantidad / 1000; // gramos → kg
                            break;
                        case "ml":
                            cantidadBase = ing.cantidad / 1000; // ml → litros
                            break;
                        case "tsp":
                            cantidadBase = ing.cantidad * 5 / 1000; // cucharadita → litros (aprox 5 ml)
                            break;
                        case "tbsp":
                            cantidadBase = ing.cantidad * 15 / 1000; // cucharada → litros (aprox 15 ml)
                            break;
                        case "kg":
                        case "l":
                        case "pieza":
                            // ya está en unidad base
                            break;
                    }

                    costoReceta += cantidadBase * ing.precio;
                    pesoTotal += ing.cantidad; // cantidad original para mostrar al usuario
                }

                double pesoPorcion = porciones > 0 ? pesoTotal / porciones : 0;

                // --- CALCULO INTELIGENTE DE VALOR / PORCENTAJE ---
                if (valorVenta > 0 && porcentaje <= 0)
                {
                    porcentaje = costoReceta > 0 ? ((valorVenta - costoReceta) / costoReceta) * 100 : 0;
                }
                else if (valorVenta <= 0 && porcentaje > 0)
                {
                    valorVenta = costoReceta * (1 + porcentaje / 100);
                }
                else if (valorVenta <= 0 && porcentaje <= 0)
                {
                    porcentaje = 20; // margen por defecto
                    valorVenta = costoReceta * 1.2;
                }

                // --- CREAR OBJETO RECETA ---
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
                    foto_url = imgFoto.Source?.ToString() ?? "",
                    fecha_creacion = DateTime.Now.ToString("yyyy-MM-dd"),
                    id_usuario = 1 // reemplaza con el ID del usuario actual
                };

                // --- GUARDAR RECETA Y OBTENER ID GENERADO ---
                int idReceta = await _service.InsertRecetaAsync(receta);

                // --- GUARDAR INGREDIENTES ASOCIADOS ---
                foreach (var ing in Ingredientes)
                {
                    ing.id_receta = idReceta;
                    await _service.InsertIngredienteAsync(ing);
                }

                await DisplayAlert("Guardado", "La receta y sus ingredientes se guardaron correctamente.", "OK");

                // --- REGRESAR A PÁGINA DE INICIO O LOGIN ---
                await Navigation.PushAsync(new vInicio()); // o vLogin según tu flujo
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"No se pudo guardar la receta: {ex.Message}", "OK");
            }
        }

        private async void BtnRegresar_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new vInicio()); // o vLogin según prefieras
        }
    }
}