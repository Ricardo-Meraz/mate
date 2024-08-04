using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using OxyPlot;
using OxyPlot.Series;
using OxyPlot.WindowsForms;


namespace ProgramaCalculo
{
    public partial class CalculadoraCa : Form
    {
        public CalculadoraCa()
        {
            InitializeComponent();
        }
        private List<Func<float, float>> funcionesOriginales = new List<Func<float, float>>();
        private List<Func<float, float>> funcionesDerivadas = new List<Func<float, float>>();
        private void btnDerivar_Click_1(object sender, EventArgs e)
{
    if (string.IsNullOrWhiteSpace(txtFuncion.Text))
    {
        txtResultado.Text = "ERROR: Ingresa una función válida";
        return;
    }

    string funcion = txtFuncion.Text;
    Regex regex = new Regex(@"(?<terminos>(?<constantes>-?([a-eA-E]+)?[0-9]*\.?[0-9]*[0-9/]*)[xX]\^(?<exponente>-?([a-eA-E0-9]+)?\.?[0-9]*[0-9/]*))");
    MatchCollection matches = regex.Matches(funcion);

    List<string> derivadas = new List<string>();
    funcionesOriginales.Clear();
    funcionesDerivadas.Clear();

    foreach (Match match in matches)
    {
        string Const = match.Groups["constantes"].Value;
        string Expo = match.Groups["exponente"].Value;

        double coef = ParseCoefficientOrFraction(Const);
        double expo = ParseCoefficientOrFraction(Expo);

        double resCoef = coef * expo;
        double resExp = expo - 1;

                string coefOutput = Const.Any(char.IsLetter) ? $"{expo}{Const}" : resCoef.ToString();
                string expOutput = resExp.ToString();

        string derivada = $"{coefOutput}x^{expOutput}";

        if (expo == 0)
        {
            derivada = "0";
        }

        derivadas.Add(derivada);

        funcionesOriginales.Add(x => (float)(coef * Math.Pow(x, expo)));
        funcionesDerivadas.Add(x => (float)(resCoef * Math.Pow(x, resExp)));
    }

    txtResultado.Text = string.Join(" + ", derivadas);
    DibujarGrafica(funcionesOriginales, funcionesDerivadas);
}

        private double ParseCoefficientOrFraction(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return 1;
            }

            input = input.Replace("a", "1").Replace("b", "2").Replace("c", "3").Replace("d", "4").Replace("e", "5");
            return ParseFraction(input);
        }
        private double ParseFraction(string fraction)
        {
            if (string.IsNullOrEmpty(fraction))
                return 1.0;

            string[] parts = fraction.Split('/');
            if (parts.Length == 2)
            {
                double numerator = double.Parse(parts[0]);
                double denominator = double.Parse(parts[1]);
                return numerator / denominator;
            }

            return double.Parse(fraction);
        }

        private void btnLimpiar_Click(object sender, EventArgs e)
        {
            txtFuncion.Text = string.Empty;
            txtResultado.Text = string.Empty;
        }
        private void btnSalir_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
        private void DibujarGrafica(List<Func<float, float>> funcionesOriginales, List<Func<float, float>> funcionesDerivadas)
        {
            Bitmap bmp = new Bitmap(PictureBoxGrafica.Width, PictureBoxGrafica.Height);
            Graphics g = Graphics.FromImage(bmp);

            int width = PictureBoxGrafica.Width;
            int height = PictureBoxGrafica.Height;

            g.Clear(Color.White);

            Pen penEjes = new Pen(Color.Black);
            g.DrawLine(penEjes, 0, height / 2, width, height / 2); // Eje X
            g.DrawLine(penEjes, width / 2, 0, width / 2, height); // Eje Y

            Font font = new Font("Arial", 8);
            Brush brush = Brushes.Black;

            float escalaX = 50; // Puntos por unidad en el eje X
            float escalaY = 50; // Puntos por unidad en el eje Y
            int unidadesX = width / (int)escalaX / 2; // Número de unidades en el eje X
            int unidadesY = height / (int)escalaY / 2; // Número de unidades en el eje Y

            for (int i = -unidadesX; i <= unidadesX; i++)
            {
                int x = i * (int)escalaX + width / 2;
                g.DrawString(i.ToString(), font, brush, x, height / 2 + 5);
                g.DrawLine(penEjes, x, height / 2 - 2, x, height / 2 + 2);
            }

            for (int i = -unidadesY; i <= unidadesY; i++)
            {
                int y = height / 2 - i * (int)escalaY;
                g.DrawString(i.ToString(), font, brush, width / 2 + 5, y - 5);
                g.DrawLine(penEjes, width / 2 - 2, y, width / 2 + 2, y);
            }

            DibujarFunciones(g, funcionesOriginales, Color.Red, width, height, escalaX, escalaY);
            DibujarFunciones(g, funcionesDerivadas, Color.Blue, width, height, escalaX, escalaY);

            PictureBoxGrafica.Image = bmp;
        }
        private void DibujarFunciones(Graphics g, List<Func<float, float>> funciones, Color color, int width, int height, float escalaX, float escalaY)
        {
            foreach (var funcion in funciones)
            {
                Point? puntoAnterior = null;
                Pen penFuncion = new Pen(color);
                for (int x = -width / 2; x < width / 2; x++)
                {
                    float realX = x / escalaX;
                    float realY = funcion(realX);

                    if (float.IsInfinity(realY) || float.IsNaN(realY))
                    {
                        puntoAnterior = null;
                        continue;
                    }

                    int graficaX = x + width / 2;
                    int graficaY = height / 2 - (int)(realY * escalaY);

                    if (graficaY < 0 || graficaY >= height)
                    {
                        puntoAnterior = null;
                        continue;
                    }

                    if (puntoAnterior.HasValue)
                    {
                        g.DrawLine(penFuncion, puntoAnterior.Value.X, puntoAnterior.Value.Y, graficaX, graficaY);
                    }

                    puntoAnterior = new Point(graficaX, graficaY);
                }
            }
        }
    }
 
}
