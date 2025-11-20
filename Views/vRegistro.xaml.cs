using chefPro.Models;
using System.Net;

namespace chefPro.Views;

public partial class vRegistro : ContentPage
{
    public vRegistro()
    {
        InitializeComponent();
        NavigationPage.SetHasNavigationBar(this, false);
    }

    private async void btnRegister_Clicked(object sender, EventArgs e)
    {
        try
        {
            // Validar campos
            if (string.IsNullOrWhiteSpace(txtEmail.Text) || string.IsNullOrWhiteSpace(txtPassword.Text) || string.IsNullOrWhiteSpace(txtNombre.Text))
            {
                await DisplayAlert("Error", "Complete todos los campos", "OK");
                return;
            }

            if (string.IsNullOrWhiteSpace(txtPassword.Text) || txtPassword.Text.Length < 6)
            {
                await DisplayAlert("Error", "La contraseña debe tener al menos 6 caracteres", "OK");
                return;
            }

            if (txtPassword.Text != txtConfirmPassword.Text)
            {
                await DisplayAlert("Error", "Las contraseñas no coinciden", "OK");
                return;
            }

            WebClient cliente = new WebClient();
            var parametros = new System.Collections.Specialized.NameValueCollection();
            parametros.Add("nombres", txtNombre.Text);
            parametros.Add("email", txtEmail.Text);
            parametros.Add("password", txtPassword.Text);
            parametros.Add("tipo_login", "1");

            // Hacer la petición
            byte[] respuestaBytes = cliente.UploadValues(
                "http://192.168.0.107/wsChefPro/usuarios/registrar",
                "POST",
                parametros
            );

            // Convertir la respuesta a string
            string respuesta = System.Text.Encoding.UTF8.GetString(respuestaBytes);

            // Deserializar la respuesta JSON
            var resultado = System.Text.Json.JsonSerializer.Deserialize<Auth>(respuesta);

            if (resultado != null && resultado.usuario != null)
            {
                await DisplayAlert("Éxito",
                    $"Usuario creado correctamente\n¡Bienvenido {resultado.usuario.nombres}!", "OK");
                await Navigation.PushAsync(new vLogin());
            }
            else
            {
                await DisplayAlert("Error", "No se pudo crear el usuario", "OK");
            }
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
    }

    private void btnGoLogin_Clicked(object sender, EventArgs e)
    {
        Navigation.PushAsync(new vLogin());
    }

    private void btnRegisterGoogle_Clicked(object sender, EventArgs e)
    {

    }
}