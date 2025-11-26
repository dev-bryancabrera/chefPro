using chefPro.Models;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace chefPro.Services
{
    public class RecetaService
    {

        private readonly SQLiteAsyncConnection _database;

        public RecetaService()
        {
            string dbPath = Path.Combine(FileSystem.AppDataDirectory, "chefPro.db3");
            _database = new SQLiteAsyncConnection(dbPath);

            _database.CreateTableAsync<Receta>().Wait();
            _database.CreateTableAsync<Ingrediente>().Wait();
        }

        // ================== RECETAS ===================
        public Task<List<Receta>> ObtenerRecetasAsync()
            => _database.Table<Receta>().ToListAsync();

        public Task<int> GuardarRecetaAsync(Receta receta)
            => _database.InsertAsync(receta);

        public async Task<int> InsertRecetaAsync(Receta receta)
        {
            await _database.InsertAsync(receta);
            return receta.id_receta;
        }

        public Task<List<Receta>> BuscarRecetasPorTituloAsync(string titulo)
        {
            return _database.Table<Receta>()
                .Where(r => r.titulo.ToLower().Contains(titulo.ToLower()))
                .ToListAsync();
        }

        // ================== INGREDIENTES ===================
        public Task<List<Ingrediente>> ObtenerIngredientesPorRecetaAsync(int idReceta)
        {
            return _database.Table<Ingrediente>()
                .Where(i => i.id_receta == idReceta)
                .ToListAsync();
        }

        public Task<int> GuardarIngredienteAsync(Ingrediente ingrediente)
            => _database.InsertAsync(ingrediente);
    }
}