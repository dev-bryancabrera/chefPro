using chefPro.Models;

namespace chefPro.Views;

public partial class RecetaDetalle : ContentPage
{
    public RecetaDetalle(Receta receta)
    {
        InitializeComponent();
        BindingContext = receta;
    }
    private async void BtnRegresar_Clicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }

   

    private async void BtnComprar_Clicked(object sender, EventArgs e)
    {
        await DisplayAlert("Comprar", "Aquí puedes implementar un flujo de compra.", "OK");
    }

}