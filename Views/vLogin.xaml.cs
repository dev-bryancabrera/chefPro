using chefPro.Models;
using System.Net;

namespace chefPro.Views;

public partial class vLogin : ContentPage
{
    public vLogin()
    {
        InitializeComponent();
        NavigationPage.SetHasNavigationBar(this, false);
    }

    private async void btnLogin_Clicked(object sender, EventArgs e)
    {
        try
        {
            // Validar campos
            if (string.IsNullOrWhiteSpace(txtEmail.Text) || string.IsNullOrWhiteSpace(txtPassword.Text))
            {
                await DisplayAlert("Error", "Complete todos los campos", "OK");
                return;
            }

            WebClient cliente = new WebClient();
            var parametros = new System.Collections.Specialized.NameValueCollection();
            parametros.Add("email", txtEmail.Text);
            parametros.Add("password", txtPassword.Text);

            // Hacer la petición
            byte[] respuestaBytes = cliente.UploadValues(
                "http://192.168.0.107/wsChefPro/auth/login",
                "POST",
                parametros
            );

            // Convertir la respuesta a string
            string respuesta = System.Text.Encoding.UTF8.GetString(respuestaBytes);

            // Deserializar la respuesta JSON
            var resultado = System.Text.Json.JsonSerializer.Deserialize<Auth>(respuesta);

            if (resultado != null && resultado.usuario != null)
            {
                await DisplayAlert("Éxito", $"Bienvenido {resultado.usuario.nombres}", "OK");
                await Navigation.PushAsync(new vInicio(resultado.usuario.nombres));


            }
            else
            {
                await DisplayAlert("Error", "Credenciales incorrectas", "OK");
            }
        }
        catch (WebException wex)
        {
            try
            {
                // Leer la respuesta de error del servidor
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
                await DisplayAlert("Error", "Error de conexión: " + wex.Message, "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", "Ocurrió un problema:\n" + ex.Message, "OK");
        }
    }

    private void btnRegister_Clicked(object sender, EventArgs e)
    {
        Navigation.PushAsync(new vRegistro());
    }

    private void btnLoginGoogle_Clicked(object sender, EventArgs e)
    {

    }
}