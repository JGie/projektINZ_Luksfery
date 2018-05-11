using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Projekt_Inz
{
    /// <summary>
    /// Interaction logic for Luxfery.xaml
    /// </summary>
    public partial class Luxfery : Window
    {
        Color[,] c = new Color[16, 12];
        Label[,] l = new Label[16, 12];




        public Luxfery()
        {
            InitializeComponent();

        }

        public void drawLine(int[] points, int[] xpoints)
        {


            Line myLine1 = new Line();
            Line myLine2 = new Line();

            myLine1.Stroke = Brushes.Green;
            myLine1.StrokeThickness = 2;
            myLine2.Stroke = Brushes.Green;
            myLine2.StrokeThickness = 2;

            myLine1.X1 = xpoints[0];
            myLine1.Y1 = (points[0] / 4) - (900 / 4);
            myLine1.X2 = xpoints[1];
            myLine1.Y2 = (points[1] / 4) - (900 / 4);
            canvas.Children.Add(myLine1);

            myLine2.X1 = xpoints[1];
            myLine2.Y1 = (points[1] / 4) - (900 / 4);
            myLine2.X2 = xpoints[2];
            myLine2.Y2 = (points[2] / 4) - (900 / 4);
            canvas.Children.Add(myLine2);

            canvas.UpdateLayout();
        }

        public void drawLine(int[,] points, int[,] xpoints, int i, int y)
        {


            Line myLine1;

            for (int x = 0; x < i - 1; x++)
            {
                myLine1 = new Line();
                myLine1.Stroke = Brushes.Green;
                myLine1.StrokeThickness = 2;

                myLine1.Y1 = (xpoints[x, y] / 4) - (900 / 4);
                myLine1.Y2 = (xpoints[x + 1, y] / 4) - (900 / 4);
                myLine1.X1 = points[x, y];
                myLine1.X2 = points[x + 1, y];
                canvas.Children.Add(myLine1);
                canvas.UpdateLayout();

            }


            canvas.UpdateLayout();
        }



        private void Canvas_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void canvas_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {

        }

        internal void LiniaXY(int x, int y)
        {

            Line liniaY = new Line();
            liniaY.Stroke = Brushes.Green;
            liniaY.StrokeThickness = 2;

            liniaY.X1 = canvas.Width / 2;
            liniaY.Y1 = 0;
            liniaY.X2 = x;
            liniaY.Y2 = (y - 900) / 4;
            canvas.Children.Add(liniaY);

            canvas.UpdateLayout();
        }

        internal void LiniaXY(int poprzedniX, int poprzedniY, int x, int y, int iteracjaKalibracjiY)
        {
            Line liniaX = new Line();
            liniaX.Stroke = Brushes.Green;
            liniaX.StrokeThickness = 2;

            Line liniaY = new Line();
            liniaY.Stroke = Brushes.Green;
            liniaY.StrokeThickness = 2;

            if (iteracjaKalibracjiY == 0)
            {
                liniaX.X1 = poprzedniX;
                liniaX.Y1 = (poprzedniY - 900) / 4;
                liniaX.X2 = x;
                liniaX.Y2 = (y - 900) / 4;
                canvas.Children.Add(liniaX);

                liniaY.X1 = canvas.Width / 2;
                liniaY.Y1 = 0;
                liniaY.X2 = x;
                liniaY.Y2 = (y - 900) / 4;
                canvas.Children.Add(liniaY);
            }
            else
            {
                liniaY.X1 = poprzedniX;
                liniaY.Y1 = (poprzedniY - 900) / 4;
                liniaY.X2 = x;
                liniaY.Y2 = (y - 900) / 4;
                canvas.Children.Add(liniaY);
            }

            canvas.UpdateLayout();
        }

        internal void LiniaXY(int poprzedniPoziomX, int poprzedniPoziomY, int poprzedniX, int poprzedniY, int x, int y)
        {
            Line liniaX = new Line();
            liniaX.Stroke = Brushes.Green;
            liniaX.StrokeThickness = 2;

            Line liniaY = new Line();
            liniaY.Stroke = Brushes.Green;
            liniaY.StrokeThickness = 2;

            liniaX.X1 = poprzedniX;
            liniaX.Y1 = (poprzedniY - 900) / 4;
            liniaX.X2 = x;
            liniaX.Y2 = (y - 900) / 4;
            canvas.Children.Add(liniaX);

            liniaY.X1 = poprzedniPoziomX;
            liniaY.Y1 = (poprzedniPoziomY - 900) / 4;
            liniaY.X2 = x;
            liniaY.Y2 = (y - 900) / 4;
            canvas.Children.Add(liniaY);

            canvas.UpdateLayout();
        }
    }
}
