using CalculadoraMetodosNumericos.Models;
using Microsoft.AspNetCore.Mvc;
using NCalc;
using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using static CalculadoraMetodosNumericos.Models.BiseccionViewModel;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace CalculadoraMetodosNumericos.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View(new BiseccionViewModel());
        }

        [HttpPost]
        public IActionResult Calcular(BiseccionViewModel model)
        {
            //Variables convertidas manualmente
            double a, b, tol;

            // ============================================
            // 1. Validaci�n de los datos y conversion de estos
            //Se valida que solo los datos que acepta la aplicaci�n lleguen al calculo. 
            // ===========================================

            if (!TryParseDouble(model.A, out a))
            {
                model.Error = "Error en el valor de A. Usa formato como 1.5 o 1,5";
                return View("Index", model);
            }

            if (!TryParseDouble(model.B, out b))
            {
                model.Error = "Error en el valor de B. Usa formato como 1.5 o 1,5";
                return View("Index", model);
            }

            if (!TryParseDouble(model.Tolerancia, out tol))
            {
                model.Error = "Error en la tolerancia. Usa formato como 0.01 o 0,01";
                return View("Index", model);
            }

            try
            {
                double fa = Evaluar(model.Funcion, a);
                double fb = Evaluar(model.Funcion, b);

                // ======================================
                // 2. Validaci�n del metodo de Biseccion , que todos los datos ingresados sean correctos
                // y se pueda proceder a realizar el calclulo 
                // ======================================
                /*if (fa * fb >= 0)
                {
                    model.Error = "La funci�n debe tener signos opuestos en f(a) y f(b).";
                    model.RaizEncontrada = false;
                    return View("Index", model);
                }*/

                if (!ValidarFuncion(model, a, b, out string errorValidacion))
                {
                    model.Error = errorValidacion;
                    model.RaizEncontrada = false;
                    return View("Index", model);
                }

                // ===============================
                // 3. M�todo de Bisecci�n
                // Aqui se comienza a realizar los calculos 
                // ===============================
                double c = a;
                int it = 0;
                double error = double.MaxValue;

                while ((b - a) / 2.0 > tol && it < 100)
                {
                    c = (a + b) / 2.0;

                    double fc = Evaluar(model.Funcion, c);

                    // Secci�n para guardar las iteraciones
                    model.IteracionesDetalle.Add(new IteracionBiseccion
                    {
                        Iteracion = it + 1,
                        A = a,
                        B = b,
                        C = c,
                        Fa = fa,
                        Fb = fb,
                        Fc = fc,
                        Error = error
                    });


                    if (fc == 0) break;

                    if (fa * fc < 0)
                    {
                        b = c;
                        fb = fc;
                    }
                    else
                    {
                        a = c;
                        fa = fc;
                    }

                    it++;
                }

                model.Raiz = Math.Round(c, 6);
                model.Iteraciones = it;
                model.RaizEncontrada = true;

                // ===============================
                // 4. Se genera la gr�fica resultante
                // ===============================
                GenerarPuntosGrafica(model, a, b);
            }
            catch (Exception ex)
            {
                model.Error = "Error en la expresi�n: " + ex.Message;
                model.RaizEncontrada = false;
            }

            return View("Index", model);
        }

        // ===============================
        // Parseo
        // ===============================
        private bool TryParseDouble(string input, out double result)
        {
            input = input.Replace(",", ".");

            return double.TryParse(
                input,
                NumberStyles.Any,
                CultureInfo.InvariantCulture,
                out result
            );
        }


        /*=============================
         Se valida que los datos dados por el usuario cumplan con los requisitos 
         para poder realizar los calculos 
         ===============================*/
        private bool ValidarFuncion(BiseccionViewModel model, double a, double b, out string error)
        {
            error = null;

            try
            {
                double fa = Evaluar(model.Funcion, a);
                double fb = Evaluar(model.Funcion, b);

                //Caso 1: valores no v�lidos
                if (double.IsNaN(fa) || double.IsNaN(fb) ||
                    double.IsInfinity(fa) || double.IsInfinity(fb))
                {
                    error = "La funci�n no es v�lida en el intervalo proporcionado.";
                    return false;
                }

                //Caso 2: no hay cambio de signo
                if (fa * fb > 0)
                {
                    error = $"No hay cambio de signo en el intervalo. f(a)={fa}, f(b)={fb}.";
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                error = "Error en la funci�n: " + ex.Message;
                return false;
            }
        }


        // ===============================
        //Se utiliza la biblioteca  NCalc para evaluar expresiones matem�ticas y l�gicas
        // ===============================
        private double Evaluar(string formula, double x)
        {
            //string xVal = x.ToString(CultureInfo.InvariantCulture);

            string xVal = x.ToString(System.Globalization.CultureInfo.InvariantCulture);

            //string formulaProcesada = formula.ToLower();
            string f = formula.ToLower();

            //==== Limpieza de caracteres extra�os ===== 
            f = f.Replace("?", "");

            //==== Soporte para ? ===== 
            f = f.Replace("?", "pi");

            // POTENCIAS: x^2 ? Pow(x,2)
            //f = Regex.Replace(f, @"(\w+)\^(\d+)", "Pow($1,$2)");
            f = System.Text.RegularExpressions.Regex.Replace(f, @"(\([^\)]+\)|\w+)\^(\d+)", "Pow($1,$2)");
            /*formulaProcesada = Regex.Replace(
                formulaProcesada,
                @"(\w+)\^(\d+)",
                "Pow($1,$2)"
            ); */

            //======= Multiplicaci�n impl�cita
            //  4x ? 4*x
            f = System.Text.RegularExpressions.Regex.Replace(f, @"(\d)(x)", "$1*$2");
            /*formulaProcesada = Regex.Replace(
                formulaProcesada,
                @"(\d)(x)",
                "$1*$2"
            );*/

            //2pi ? 2*pi
            f = System.Text.RegularExpressions.Regex.Replace(f, @"(\d)(pi|e)", "$1*$2");

            //pi x ? pi*x
            f = System.Text.RegularExpressions.Regex.Replace(f, @"(pi|e)(x)", "$1*$2");

            //xcos(x) ? x*cos(x)
            f = System.Text.RegularExpressions.Regex.Replace(f, @"(x)([a-z])", "$1*$2");

            // )cos(x) ? )*cos(x)
            f = System.Text.RegularExpressions.Regex.Replace(f, @"\)([a-z])", ")*$1");

            // (x+1)(x-1) ? (x+1)*(x-1)
            f = System.Text.RegularExpressions.Regex.Replace(f, @"\)\(", ")*(");

            // ===========Remplazar variable x =========================
            f = f.Replace("x", $"({xVal})");
            // formulaProcesada = formulaProcesada.Replace("x", $"({xVal})");

            // ===========Crear la expresi�n para NCalc  =========================
            Expression e = new Expression(f);

            //Funciones personalizadas
            e.EvaluateFunction += (name, args) =>
            {
                //double val = Convert.ToDouble(args.Parameters[0].Evaluate());

                switch (name.ToLower())
                {
                    case "pow":
                        double baseVal = Convert.ToDouble(args.Parameters[0].Evaluate());
                        double expVal = Convert.ToDouble(args.Parameters[1].Evaluate());
                        args.Result = Math.Pow(baseVal, expVal);
                        break;

                    case "sin":
                        args.Result = Math.Sin(Convert.ToDouble(args.Parameters[0].Evaluate()));
                        break;

                    case "cos":
                        args.Result = Math.Cos(Convert.ToDouble(args.Parameters[0].Evaluate()));
                        break;

                    case "tan":
                        args.Result = Math.Tan(Convert.ToDouble(args.Parameters[0].Evaluate()));
                        break;

                    case "log": // base 10
                        args.Result = Math.Log10(Convert.ToDouble(args.Parameters[0].Evaluate()));
                        break;

                    case "ln": // natural
                        args.Result = Math.Log(Convert.ToDouble(args.Parameters[0].Evaluate()));
                        break;

                    case "sqrt":
                        args.Result = Math.Sqrt(Convert.ToDouble(args.Parameters[0].Evaluate()));
                        break;

                    case "abs":
                        args.Result = Math.Abs(Convert.ToDouble(args.Parameters[0].Evaluate()));
                        break;

                    default:
                        throw new Exception($"Funci�n '{name}' no soportada");
                }
            };

            // =============Constasntes pi y e ==================
            e.EvaluateParameter += (name, args) =>
            {
                switch (name.ToLower())
                {
                    case "pi":
                        args.Result = Math.PI;
                        break;

                    case "e":
                        args.Result = Math.E;
                        break;
                }
            };

            // ===============Evaluar ================
            return Convert.ToDouble(
                e.Evaluate(), System.Globalization.CultureInfo.InvariantCulture);

            /*return Convert.ToDouble(
                e.Evaluate(),
                CultureInfo.InvariantCulture
            );*/
        }

        // ===============================
        //  M�todo para generar la gr�fica 
        // ===============================
        private void GenerarPuntosGrafica(BiseccionViewModel model, double a, double b)
        {
            model.ValoresX.Clear();
            model.ValoresY.Clear();

            double inicio = a - 1;
            double fin = b + 1;
            double paso = (fin - inicio) / 50;

            for (double x = inicio; x <= fin; x += paso)
            {
                try
                {
                    model.ValoresX.Add(Math.Round(x, 2));
                    model.ValoresY.Add(Math.Round(Evaluar(model.Funcion, x), 4));
                }
                catch 
                {

                }
            }
        }
    }
}