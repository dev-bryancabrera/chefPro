using chefPro.Models;
using Newtonsoft.Json;

namespace chefPro.Views;

public partial class vEstadistica : ContentPage
{
    private HttpClient client = new HttpClient();
    private int _idUsuario;
    private const string URL_BASE = "http://192.168.0.102/wsChefPro/estadisticas";

    public vEstadistica(int idUsuario)
    {
        InitializeComponent();
        NavigationPage.SetHasNavigationBar(this, false);
        _idUsuario = idUsuario;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await CargarEstadisticas();
    }

    private async void btnActualizar_Clicked(object sender, EventArgs e)
    {
        await CargarEstadisticas();
    }

    private async Task CargarEstadisticas()
    {
        try
        {
            IsBusy = true;
            btnActualizar.IsEnabled = false;

            // Cargar todas las estadísticas en paralelo para mejor rendimiento
            await Task.WhenAll(
                CargarEstadisticasGenerales(),
                CargarRecetasMasVistas(),
                CargarIngredientesMasUsados(),
                CargarTecnicasMasUsadas(),
                CargarEstadisticasTiempo(),
                CargarActividadReciente(),
                CargarEstadisticasFinancieras(),
                CargarRecetasMasRentables(),
                CargarRecetasTendencia(),
                CargarIngredientesCostosos(),
                CargarResumenPorciones()
            );

            await DisplayAlert("✅ Éxito", "Estadísticas actualizadas correctamente", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("❌ Error", $"Error al cargar estadísticas: {ex.Message}", "OK");
            System.Diagnostics.Debug.WriteLine($"Error completo: {ex}");
        }
        finally
        {
            IsBusy = false;
            btnActualizar.IsEnabled = true;
        }
    }

    private async Task CargarEstadisticasGenerales()
    {
        try
        {
            var url = $"{URL_BASE}/estadisticas_generales_mejoradas?id_usuario={_idUsuario}";
            var response = await client.GetStringAsync(url);
            var stats = JsonConvert.DeserializeObject<EstadisticasGenerales>(response);

            if (stats != null)
            {
                lblTotalVistas.Text = stats.total_vistas.ToString();
                lblPromedioIngredientes.Text = stats.promedio_ingredientes.ToString("F1");
                lblTotalRecetas.Text = stats.total_recetas.ToString();
                lblVistasPorReceta.Text = stats.vistas_por_receta.ToString("F1");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error al cargar estadísticas generales: {ex.Message}");
        }
    }

    private async Task CargarRecetasMasVistas()
    {
        try
        {
            var url = $"{URL_BASE}/recetas_mas_vistas?id_usuario={_idUsuario}&limite=10";
            var response = await client.GetStringAsync(url);
            var recetas = JsonConvert.DeserializeObject<List<RecetasEst>>(response);

            if (recetas != null && recetas.Count > 0)
            {
                foreach (var receta in recetas)
                {
                    if (!string.IsNullOrEmpty(receta.imagen) && !receta.imagen.StartsWith("http"))
                    {
                        receta.imagen = $"http://192.168.0.102/wsChefPro/uploads/recetas/{receta.imagen}";
                    }
                    else if (string.IsNullOrEmpty(receta.imagen))
                    {
                        receta.imagen = "fondo_cocina.jpg";
                    }
                }
                cvRecetasTop.ItemsSource = recetas;
            }
            else
            {
                cvRecetasTop.ItemsSource = new List<RecetasEst>();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error al cargar recetas más vistas: {ex.Message}");
            cvRecetasTop.ItemsSource = new List<RecetasEst>();
        }
    }

    private async Task CargarIngredientesMasUsados()
    {
        try
        {
            var url = $"{URL_BASE}/ingredientes_mas_usados?id_usuario={_idUsuario}&limite=10";
            var response = await client.GetStringAsync(url);
            var ingredientes = JsonConvert.DeserializeObject<List<IngredientesEst>>(response);

            cvIngredientesTop.ItemsSource = ingredientes ?? new List<IngredientesEst>();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error al cargar ingredientes más usados: {ex.Message}");
            cvIngredientesTop.ItemsSource = new List<IngredientesEst>();
        }
    }

    private async Task CargarTecnicasMasUsadas()
    {
        try
        {
            var url = $"{URL_BASE}/tecnicas_mas_usadas?id_usuario={_idUsuario}&limite=10";
            var response = await client.GetStringAsync(url);
            var tecnicas = JsonConvert.DeserializeObject<List<TecnicasEst>>(response);

            cvTecnicasTop.ItemsSource = tecnicas ?? new List<TecnicasEst>();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error al cargar técnicas más usadas: {ex.Message}");
            cvTecnicasTop.ItemsSource = new List<TecnicasEst>();
        }
    }

    private async Task CargarEstadisticasTiempo()
    {
        try
        {
            var url = $"{URL_BASE}/estadisticas_tiempo?id_usuario={_idUsuario}";
            var response = await client.GetStringAsync(url);
            var tiempos = JsonConvert.DeserializeObject<TiemposEst>(response);

            if (tiempos != null)
            {
                lblRecetasRapidas.Text = tiempos.recetas_rapidas.ToString();
                lblTiempoPromedio.Text = tiempos.tiempo_promedio.ToString();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error al cargar estadísticas de tiempo: {ex.Message}");
            lblRecetasRapidas.Text = "0";
            lblTiempoPromedio.Text = "0";
        }
    }

    private async Task CargarActividadReciente()
    {
        try
        {
            var url = $"{URL_BASE}/actividad_reciente?id_usuario={_idUsuario}";
            var response = await client.GetStringAsync(url);
            var actividad = JsonConvert.DeserializeObject<ActividadEst>(response);

            if (actividad != null)
            {
                lblRecetasSemana.Text = actividad.recetas_semana.ToString();
                lblVistasSemana.Text = actividad.vistas_semana.ToString();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error al cargar actividad reciente: {ex.Message}");
            lblRecetasSemana.Text = "0";
            lblVistasSemana.Text = "0";
        }
    }

    private async Task CargarEstadisticasFinancieras()
    {
        try
        {
            var url = $"{URL_BASE}/estadisticas_financieras?id_usuario={_idUsuario}";
            var response = await client.GetStringAsync(url);
            var financiero = JsonConvert.DeserializeObject<FinancieroEst>(response);

            if (financiero != null)
            {
                lblCostoPromedio.Text = $"${financiero.costo_promedio:F2}";
                lblVentaPromedio.Text = $"${financiero.venta_promedio:F2}";
                lblGananciaPromedio.Text = $"{financiero.ganancia_promedio:F1}%";
                lblRecetasCosteadas.Text = financiero.total_recetas_costeadas.ToString();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error al cargar estadísticas financieras: {ex.Message}");
            lblCostoPromedio.Text = "$0.00";
            lblVentaPromedio.Text = "$0.00";
            lblGananciaPromedio.Text = "0%";
            lblRecetasCosteadas.Text = "0";
        }
    }

    private async Task CargarRecetasMasRentables()
    {
        try
        {
            var url = $"{URL_BASE}/recetas_mas_rentables?id_usuario={_idUsuario}&limite=5";
            var response = await client.GetStringAsync(url);
            var recetas = JsonConvert.DeserializeObject<List<RecetasRentablesEst>>(response);

            if (recetas != null && recetas.Count > 0)
            {
                foreach (var receta in recetas)
                {
                    if (!string.IsNullOrEmpty(receta.imagen) && !receta.imagen.StartsWith("http"))
                    {
                        receta.imagen = $"http://192.168.0.102/wsChefPro/uploads/recetas/{receta.imagen}";
                    }
                    else if (string.IsNullOrEmpty(receta.imagen))
                    {
                        receta.imagen = "fondo_cocina.jpg";
                    }
                }
                cvRecetasRentables.ItemsSource = recetas;
            }
            else
            {
                cvRecetasRentables.ItemsSource = new List<RecetasRentablesEst>();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error al cargar recetas rentables: {ex.Message}");
            cvRecetasRentables.ItemsSource = new List<RecetasRentablesEst>();
        }
    }

    private async Task CargarRecetasTendencia()
    {
        try
        {
            var url = $"{URL_BASE}/recetas_tendencia?id_usuario={_idUsuario}&limite=5";
            var response = await client.GetStringAsync(url);
            var recetas = JsonConvert.DeserializeObject<List<RecetasTendenciaEst>>(response);

            if (recetas != null && recetas.Count > 0)
            {
                foreach (var receta in recetas)
                {
                    if (!string.IsNullOrEmpty(receta.imagen) && !receta.imagen.StartsWith("http"))
                    {
                        receta.imagen = $"http://192.168.0.102/wsChefPro/uploads/recetas/{receta.imagen}";
                    }
                    else if (string.IsNullOrEmpty(receta.imagen))
                    {
                        receta.imagen = "fondo_cocina.jpg";
                    }
                }
                cvRecetasTendencia.ItemsSource = recetas;
            }
            else
            {
                cvRecetasTendencia.ItemsSource = new List<RecetasTendenciaEst>();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error al cargar recetas tendencia: {ex.Message}");
            cvRecetasTendencia.ItemsSource = new List<RecetasTendenciaEst>();
        }
    }

    private async Task CargarIngredientesCostosos()
    {
        try
        {
            var url = $"{URL_BASE}/ingredientes_costosos?id_usuario={_idUsuario}&limite=8";
            var response = await client.GetStringAsync(url);
            var ingredientes = JsonConvert.DeserializeObject<List<IngredientesCostososEst>>(response);

            cvIngredientesCostosos.ItemsSource = ingredientes ?? new List<IngredientesCostososEst>();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error al cargar ingredientes costosos: {ex.Message}");
            cvIngredientesCostosos.ItemsSource = new List<IngredientesCostososEst>();
        }
    }

    private async Task CargarResumenPorciones()
    {
        try
        {
            var url = $"{URL_BASE}/resumen_porciones?id_usuario={_idUsuario}";
            var response = await client.GetStringAsync(url);
            var porciones = JsonConvert.DeserializeObject<PorcionesEst>(response);

            if (porciones != null)
            {
                lblPromedioPorciones.Text = porciones.promedio_porciones.ToString("F1");
                lblTotalPorciones.Text = porciones.total_porciones.ToString();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error al cargar resumen de porciones: {ex.Message}");
            lblPromedioPorciones.Text = "0.0";
            lblTotalPorciones.Text = "0";
        }
    }
}