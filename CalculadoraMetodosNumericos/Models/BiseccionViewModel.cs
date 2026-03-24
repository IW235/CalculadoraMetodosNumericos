using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CalculadoraMetodosNumericos.Models
{
    public class BiseccionViewModel
    {
        // ==========================================
        // 1. Datos de entrada que el usuario debe de ingresar
        // ==========================================

        [Required(ErrorMessage = "La función es obligatoria.")]
        public string Funcion { get; set; }

        [Required(ErrorMessage = "Debes ingresar un límite inferior.")]
        //public double A { get; set; }
        public string A { get; set; }

        [Required(ErrorMessage = "Debes ingresar un límite superior.")]
        // public double B { get; set; }
        public string B { get; set; }

        [Required(ErrorMessage = "La tolerancia es necesaria para detener el ciclo.")]
        //public double Tolerancia { get; set; }
        public string Tolerancia { get; set; }

        // ==========================================
        // 2. Datos de salida, los cuales seran calculados por el controlador 
        // ==========================================
        public bool RaizEncontrada { get; set; }
        public double Raiz { get; set; }
        public int Iteraciones { get; set; }
        public string Error { get; set; } // Para manejar si f(a) y f(b) no tienen signos opuestos

        // ==========================================
        // 3. Datos para la gráfica, se utiliza  Chart.js
        // ==========================================

        // Se alamcenan  los puntos (x, y) para dibujar la curva de f(x)
        public List<double> ValoresX { get; set; }
        public List<double> ValoresY { get; set; }

        // Constructor para inicializar las listas y evitar errores de referencia nula
        public BiseccionViewModel()
        {
            ValoresX = new List<double>();
            ValoresY = new List<double>();
            Funcion = "x^2 - 4"; // Valor por defecto
            Tolerancia = "0.01";   // Valor por defecto
            IteracionesDetalle = new List<IteracionBiseccion>();
        }

        //Modelo para la iteración
        public class IteracionBiseccion
        {
            public int Iteracion { get; set; }
            public double A { get; set; }
            public double B { get; set; }
            public double C { get; set; }
            public double Fa { get; set; }
            public double Fb { get; set; }
            public double Fc { get; set; }
            public double Error { get; set; }
        }

        public List<IteracionBiseccion> IteracionesDetalle { get; set; }

    }
}
