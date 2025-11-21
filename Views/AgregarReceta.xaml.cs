using chefPro.Models;
using chefPro.Services;

namespace chefPro.Views;

public partial class AgregarReceta : ContentPage
{
    RecetaLocalService _service;

    public AgregarReceta()
    {
        InitializeComponent();
        _service = new RecetaLocalService();
    }

    private async void BtnFoto_Clicked(object sender, EventArgs e)
    {
        var photo = await MediaPicker.CapturePhotoAsync();

        if (photo != null)
        {
            var stream = await photo.OpenReadAsync();
            imgFoto.Source = ImageSource.FromStream(() => stream);
        }
    }

    private async void BtnGuardar_Clicked(object sender, EventArgs e)
    {
        var receta = new Receta
        {
            titulo = txtTitulo.Text,
            descripcion = txtDescripcion.Text,
            tiempo_preparacion = double.Parse(txtTiempo.Text),
            peso_total = double.Parse(txtPesoTotal.Text),
            porciones = double.Parse(txtPorciones.Text),
            peso_porcion = double.Parse(txtPesoPorcion.Text),
            valor_venta = double.Parse(txtValorVenta.Text),
            costo_receta = double.Parse(txtCosto.Text),
            precio_unidad = double.Parse(txtPrecioUnidad.Text),
            porcentaje_ganancia = double.Parse(txtPorcentajeGanancia.Text),
            foto_url = "",
            id_usuario = 1,
            fecha_creacion = DateTime.Now.ToString("yyyy-MM-dd")
        };

        await _service.GuardarRecetaAsync(receta);

        await DisplayAlert("Éxito", "Receta guardada correctamente", "OK");
    }
}
