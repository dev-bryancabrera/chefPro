using chefPro.Models;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        // Insertar receta y devolver ID generado
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
        {
            return _database.InsertAsync(ingrediente);
        }

        public Task<int> InsertIngredienteAsync(Ingrediente ingrediente)
        {
            return _database.InsertAsync(ingrediente);
        }

        public async Task<List<string>> GetIngredientesBaseAsync()
        {
            var lista = await _database.Table<Ingrediente>().ToListAsync();

            return lista
                .Select(i => i.nombre.Trim())
                .Distinct()
                .OrderBy(n => n)
                .ToList();
        }

        // ================== INICIALIZACIÓN ===================

        public async Task InicializarRecetasAsync()
        {
            var count = await _database.Table<Receta>().CountAsync();
            if (count > 0) return;

            var recetas = new List<Receta>
            {
                new Receta
                {
                    titulo = "Pan Tradicional",
                    descripcion = "Receta básica de pan artesanal",
                    instrucciones = "1. Mezclar los ingredientes.\n2. Amasar.\n3. Hornear.",
                    tiempo_preparacion = 120,
                    peso_total = 1000,
                    porciones = 10,
                    valor_venta = 5,
                    costo_receta = 2.5,
                    porcentaje_ganancia = 100,
                    foto_url = "pan_tradicional.jpg",
                    fecha_creacion = DateTime.Now.ToString("yyyy-MM-dd"),
                    id_usuario = 1
                },
                new Receta
                {
                    titulo = "Galletas de Mantequilla",
                    descripcion = "Galletas dulces clásicas y crujientes",
                    instrucciones = "1. Batir mantequilla.\n2. Agregar harina.\n3. Hornear.",
                    tiempo_preparacion = 45,
                    peso_total = 500,
                    porciones = 20,
                    valor_venta = 4,
                    costo_receta = 1.8,
                    porcentaje_ganancia = 122,
                    foto_url = "gm.png",
                    fecha_creacion = DateTime.Now.ToString("yyyy-MM-dd"),
                    id_usuario = 1
                },
                new Receta
                {
                    titulo = "Brownies de Chocolate",
                    descripcion = "Brownies húmedos y suaves",
                    instrucciones = "1. Derretir chocolate.\n2. Mezclar.\n3. Hornear.",
                    tiempo_preparacion = 60,
                    peso_total = 600,
                    porciones = 12,
                    valor_venta = 7.5,
                    costo_receta = 3.5,
                    porcentaje_ganancia = 114,
                    foto_url = "brownies_chocolate.jpg",
                    fecha_creacion = DateTime.Now.ToString("yyyy-MM-dd"),
                    id_usuario = 1
                }
            };

            foreach (var r in recetas)
            {
                await GuardarRecetaAsync(r);

                if (r.titulo == "Pan Tradicional")
                {
                    await GuardarIngredienteAsync(new Ingrediente { id_receta = r.id_receta, nombre = "Harina", cantidad = 500, unidad = "g" });
                    await GuardarIngredienteAsync(new Ingrediente { id_receta = r.id_receta, nombre = "Agua", cantidad = 300, unidad = "ml" });
                    await GuardarIngredienteAsync(new Ingrediente { id_receta = r.id_receta, nombre = "Levadura", cantidad = 10, unidad = "g" });
                }
                else if (r.titulo == "Galletas de Mantequilla")
                {
                    await GuardarIngredienteAsync(new Ingrediente { id_receta = r.id_receta, nombre = "Mantequilla", cantidad = 200, unidad = "g" });
                    await GuardarIngredienteAsync(new Ingrediente { id_receta = r.id_receta, nombre = "Azúcar", cantidad = 100, unidad = "g" });
                    await GuardarIngredienteAsync(new Ingrediente { id_receta = r.id_receta, nombre = "Harina", cantidad = 250, unidad = "g" });
                }
                else if (r.titulo == "Brownies de Chocolate")
                {
                    await GuardarIngredienteAsync(new Ingrediente { id_receta = r.id_receta, nombre = "Chocolate", cantidad = 200, unidad = "g" });
                    await GuardarIngredienteAsync(new Ingrediente { id_receta = r.id_receta, nombre = "Mantequilla", cantidad = 150, unidad = "g" });
                    await GuardarIngredienteAsync(new Ingrediente { id_receta = r.id_receta, nombre = "Azúcar", cantidad = 150, unidad = "g" });
                }
            }
        }
    }
}