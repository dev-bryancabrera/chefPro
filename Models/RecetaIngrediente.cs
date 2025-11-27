using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace chefPro.Models
{
    public class RecetaIngrediente : INotifyPropertyChanged
    {
        private int _idIngrediente;
        private string _nombreIngrediente;
        private double _cantidad;
        private double _costoUnitario;
        private Ingrediente _ingredienteSeleccionado;

        public int IdIngrediente
        {
            get => _idIngrediente;
            set
            {
                _idIngrediente = value;
                OnPropertyChanged();
            }
        }

        public string NombreIngrediente
        {
            get => _nombreIngrediente;
            set
            {
                _nombreIngrediente = value;
                OnPropertyChanged();
            }
        }

        public double Cantidad
        {
            get => _cantidad;
            set
            {
                _cantidad = value;
                OnPropertyChanged();
            }
        }

        public double CostoUnitario
        {
            get => _costoUnitario;
            set
            {
                _costoUnitario = value;
                OnPropertyChanged();
            }
        }

        public Ingrediente IngredienteSeleccionado
        {
            get => _ingredienteSeleccionado;
            set
            {
                _ingredienteSeleccionado = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
