namespace chefPro.Views;

public partial class vInicio : ContentPage
{
    private string nombreUsuario;
      public vInicio()
    {
        InitializeComponent();
        // Valores por defecto
        NombreUsuarioLabel.Text = "Chef invitado cocinando";
        RecetasCreadasLabel.Text = "Recetas creadas: 0";
        RecetasCompradasLabel.Text = "Recetas compradas: 0";
    }
    public vInicio(string nombreUsuarioRegistrado)
    {
        InitializeComponent();

        nombreUsuario = nombreUsuarioRegistrado;

        // Asignar texto al Label
        NombreUsuarioLabel.Text = $"Chef {nombreUsuario} cocinando";

        // Contadores iniciales
        RecetasCreadasLabel.Text = "Recetas creadas: 12";
        RecetasCompradasLabel.Text = "Recetas compradas: 8";
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        Random rnd = new Random();
        NotaMotivacional.Text = notas[rnd.Next(notas.Length)];
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
        await Navigation.PushAsync(new AgregarReceta());
    }

    private async void BuscarReceta_Clicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new BuscarReceta());
    }

    private async void VerGanancias_Clicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new Ganancias());
    }
}