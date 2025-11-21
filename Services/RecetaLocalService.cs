using chefPro.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace chefPro.Services
{
    public class RecetaLocalService
    {
        private readonly string filePath =
             Path.Combine(FileSystem.AppDataDirectory, "recetas.json");

        public RecetaLocalService()
        {
            InicializarArchivoAsync().Wait();
        }

        // Inicializa el JSON si no existe
        private async Task InicializarArchivoAsync()
        {
            if (!File.Exists(filePath))
            {
#if ANDROID
                var assembly = typeof(RecetaLocalService).Assembly;
                using var stream = assembly.GetManifestResourceStream("chefPro.Assets.recetas.json");
                using var reader = new StreamReader(stream);
                var json = await reader.ReadToEndAsync();
                await File.WriteAllTextAsync(filePath, json);
#elif WINDOWS
                var source = Path.Combine(AppContext.BaseDirectory, "Assets", "recetas.json");
                File.Copy(source, filePath, true);
#endif
            }
        }

        public async Task<List<Receta>> ObtenerRecetasAsync()
        {
            if (!File.Exists(filePath))
                return new List<Receta>();

            string json = await File.ReadAllTextAsync(filePath);
            return JsonSerializer.Deserialize<List<Receta>>(json) ?? new List<Receta>();
        }

        public async Task<List<Receta>> BuscarRecetasPorTituloAsync(string titulo)
        {
            var recetas = await ObtenerRecetasAsync();
            return recetas
                .Where(r => r.titulo.Contains(titulo, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        public async Task GuardarRecetaAsync(Receta receta)
        {
            var recetas = await ObtenerRecetasAsync();
            receta.id_receta = recetas.Count + 1;
            receta.fecha_creacion = DateTime.Now.ToString("yyyy-MM-dd");
            recetas.Add(receta);
            string json = JsonSerializer.Serialize(recetas, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(filePath, json);
        }
    }
}