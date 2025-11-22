using chefPro.Models;
using chefPro.Services;
using System.Collections.ObjectModel;


namespace chefPro.Views
{
    public partial class AgregarReceta : ContentPage
    {
        // Lista observable para el CollectionView
        public ObservableCollection<Ingrediente> Ingredientes { get; set; }
            = new ObservableCollection<Ingrediente>();

        public AgregarReceta()
        {
            InitializeComponent();
            cvIngredientes.ItemsSource = Ingredientes;
        }

   
        // BOTÓN: AGREGAR INGREDIENTE
        
        private void BtnAgregarIngrediente_Clicked(object sender, EventArgs e)
        {
            Ingredientes.Add(new Ingrediente
            {
                nombre = "",
                cantidad = 0,
                unidad = "",
                precio = 0
            });
        }

     
        // BOTÓN: ELIMINAR INGREDIENTE
     
        private void BtnEliminarIngrediente_Clicked(object sender, EventArgs e)
        {
            if (sender is Button btn && btn.CommandParameter is Ingrediente ing)
            {
                Ingredientes.Remove(ing);
            }
        }

        // BOTÓN: TOMAR O SELECCIONAR FOTO
     
        private async void BtnFoto_Clicked(object sender, EventArgs e)
        {
            try
            {
                string opcion = await DisplayActionSheet(
                    "Seleccionar imagen",
                    "Cancelar",
                    null,
                    "Tomar foto",
                    "Elegir de la galería"
                );

                FileResult file = null;

                if (opcion == "Tomar foto")
                {
                    file = await MediaPicker.CapturePhotoAsync();
                }
                else if (opcion == "Elegir de la galería")
                {
                    file = await MediaPicker.PickPhotoAsync();
                }

                if (file == null)
                    return;

                var stream = await file.OpenReadAsync();
                imgFoto.Source = ImageSource.FromStream(() => stream);
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "OK");
            }
        }

        // BOTÓN: GUARDAR RECETA
  
        private async void BtnGuardar_Clicked(object sender, EventArgs e)
        {
            string titulo = txtTitulo.Text?.Trim();
            string descripcion = txtDescripcion.Text?.Trim();
            string instrucciones = txtInstrucciones.Text?.Trim();

            // Validaciones
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

            if (descripcion.Length > 150)
            {
                await DisplayAlert("Error", "La descripción no puede superar 150 caracteres.", "OK");
                return;
            }

            // Convertir campos numéricos
            double.TryParse(txtTiempo.Text, out double tiempo);
            double.TryParse(txtPorciones.Text, out double porciones);
            double.TryParse(txtValorVenta.Text, out double valorVenta);
            double.TryParse(txtPorcentaje.Text, out double porcentaje);

            // Validación mínima opcional
            if (porciones <= 0)
            {
                await DisplayAlert("Error", "Las porciones deben ser mayor a 0.", "OK");
                return;
            }

            // Convertir ingredientes
            var listaIngredientes = Ingredientes.ToList();

            // Ejemplo de guardado (aquí iría BD o API real)
            await DisplayAlert("Guardado", "La receta se guardó correctamente.", "OK");
        }

   
        // BOTÓN: REGRESAR
       
        private async void BtnRegresar_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new vInicio());
        }
    }
}
