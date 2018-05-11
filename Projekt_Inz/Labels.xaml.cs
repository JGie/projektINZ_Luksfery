using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
    /// Interaction logic for Labels.xaml
    /// </summary>
    public partial class Labels : Window
    {
        Label[,] labels = new Label[8, 8];
        double lblheight = 0;
        double lblwidht = 0;

        public Labels()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            FillCanvas();

        }

        private void FillCanvas()
        {
             lblwidht = lbls_canvas.Width / 8;
             lblheight = lbls_canvas.Width / 8;
            

            for (int y = 0; y < 8 ; y++)
            {                
                for (int x = 0; x < 8; x++)
                {
                    Label tmplbl = new Label() { Width = lblwidht, Height = lblwidht ,Content="W"+x+"H"+y};
                   tmplbl.Margin = new Thickness(x*lblwidht,lblheight*y,0,0);

                    labels[x, y] = tmplbl;
                    lbls_canvas.Children.Add(tmplbl);
                    lbls_canvas.UpdateLayout();
                }
            }

        }

        public void UnmarkAllBlueLux()
        {
            for (int y = 0; y < 8; y++)
            {
                for (int x = 0; x < 8; x++)
                {
                    SolidColorBrush brush = labels[x, y].Background as SolidColorBrush;
                    if (brush.Color == Colors.Blue)
                    {
                        labels[x, y].Background = new SolidColorBrush(Colors.White);
                    }
                }
            }
            lbls_canvas.UpdateLayout();
        }
       
        

        public void MarkLux(byte[,] activeLux)
        {
            for (int y = 0; y < 8; y++)
            {
                for (int x = 0; x < 8; x++)
                {

                    if (activeLux[y, x] == 1)
                    {
                        labels[y, x].Background = new SolidColorBrush(Colors.Blue);                        
                        lbls_canvas.UpdateLayout();
                    }
                    else if(activeLux[x,y]==2)
                    {
                        labels[x, y].Background = new SolidColorBrush(Colors.Green);
                        lbls_canvas.UpdateLayout();
                        int dx = x;
                        int dy = y;            

                    }
                    else if (activeLux[x, y] == 3)
                    {
                        labels[x, y].Background = new SolidColorBrush(Colors.OrangeRed);
                        lbls_canvas.UpdateLayout();
                    }
                    else if (activeLux[x, y] == 4)
                    {
                        labels[x, y].Background = new SolidColorBrush(Colors.OliveDrab);
                        lbls_canvas.UpdateLayout();
                    }

                }
            }
        }

        public void MarkSwipe(string swipe)
        {
            for (int y = 0; y < 8; y++)
            {
                for (int x = 0; x < 8; x++)
                {
                    SolidColorBrush brush = labels[x, y].Background as SolidColorBrush;
                    if (swipe.Equals("u"))
                    {
                        labels[x, y].Background = new SolidColorBrush(Colors.Gold);
                    }
                    else if (swipe.Equals("d"))
                    {
                        labels[x, y].Background = new SolidColorBrush(Colors.DarkGoldenrod);
                    }
                    else if (swipe.Equals("l"))
                    {
                        labels[x, y].Background = new SolidColorBrush(Colors.Lime);
                    }
                    else if (swipe.Equals("r"))
                    {
                        labels[x, y].Background = new SolidColorBrush(Colors.Red);
                    }
                }
            }
            lbls_canvas.UpdateLayout();
        }
    }
}
