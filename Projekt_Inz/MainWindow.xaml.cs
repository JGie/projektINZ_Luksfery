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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Kinect;
using System.Net.Sockets;
using System.Net;
using System.Threading;


namespace Projekt_Inz
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Window luxfery;
        Window labels;
        KinectSensor sensor;
        int depth = 0;
        byte[] piksele;
        short[] rawDepthData;

        int[,] xAndDepths = new int[3, 12];
        int[,] xLux = new int[8, 8];
        int[,] yLux = new int[8, 8];

        string[,] luksferaXY = new string[8, 8];


        int[,] previousPixelsTab = new int[641, 49];


        bool czyKalibracjaGotowa = false;

        int[,] pixelsTab = new int[641, 481];

        int maksymalnaOdlegloscY = 3000;
        int iteracjaKalibracjiX = 0;
        int iteracjaKalibracjiY = 0;
        double gorneOgraniczenieDanych = 0.8;
        double dolneOgraniczenieDanych = 0.85;
        int startIteracjaGlebi = 0;
        int startIteracjaKoloru = 0;
        byte[,] aktywnaLuksfera = new byte[8, 8];
        byte[,] poprzedniaAktywnaLuksfera = new byte[8, 8];
        Byte[] tabPiksele;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (KinectSensor.KinectSensors.Count > 0)
            {
                this.sensor = KinectSensor.KinectSensors[0];
                this.sensor.Start();

                //mapa dystorsji
                luxfery = new Luxfery();
                luxfery.Owner = this;
                luxfery.Show();

                if (!this.sensor.DepthStream.IsEnabled)
                {
                    this.sensor.DepthStream.Enable(DepthImageFormat.Resolution640x480Fps30);

                    this.sensor.DepthFrameReady +=
                        new EventHandler<DepthImageFrameReadyEventArgs>(sensor_DepthFrameReady);
                }
            }
            else
            {
                MessageBox.Show("Nie wykryto sensorow");
                this.Close();
            }
        }

        private void sensor_DepthFrameReady(object sender, DepthImageFrameReadyEventArgs e)
        {

            using (DepthImageFrame depthFrame = e.OpenDepthImageFrame())
            {
                if (depthFrame == null)
                {
                    return;
                }

                piksele = WygenerujKolory(depthFrame);

                BitmapSource.Create(depthFrame.Width, depthFrame.Height, 96, 96, PixelFormats.Bgr32, null, piksele, depthFrame.Width * 4);

            }
        }



        private byte[] WygenerujKolory(DepthImageFrame obrazKlatkiGlebi)
        {
            rawDepthData = new short[obrazKlatkiGlebi.PixelDataLength];
            obrazKlatkiGlebi.CopyPixelDataTo(rawDepthData);
            tabPiksele = new byte[obrazKlatkiGlebi.Height * obrazKlatkiGlebi.Width * 4];
            startIteracjaGlebi = Convert.ToInt32(rawDepthData.Length * gorneOgraniczenieDanych);
            startIteracjaKoloru = Convert.ToInt32(tabPiksele.Length * gorneOgraniczenieDanych);

            const int blueIndex = 0, greenIndex = 1, redIndex = 2;
            int x = 0, y = 0;

            for (int indeksGlebi = startIteracjaGlebi, indeksKoloru = startIteracjaKoloru;
                indeksGlebi < rawDepthData.Length * dolneOgraniczenieDanych &&
                indeksKoloru < tabPiksele.Length * dolneOgraniczenieDanych;
                indeksGlebi++, indeksKoloru += 4)
            {
                depth = rawDepthData[indeksGlebi] >> DepthImageFrame.PlayerIndexBitmaskWidth;
                if (depth >= 900 && depth <= maksymalnaOdlegloscY)
                {
                    y = indeksGlebi / 640;
                    x = indeksGlebi % 640;
                    pixelsTab[x, y] = rawDepthData[indeksGlebi] >> DepthImageFrame.PlayerIndexBitmaskWidth;

                    byte nasycenie = (byte)(depth >= 900 && depth <= maksymalnaOdlegloscY ? depth : 0);
                    tabPiksele[indeksKoloru + blueIndex] = nasycenie;
                    tabPiksele[indeksKoloru + greenIndex] = nasycenie;
                    tabPiksele[indeksKoloru + redIndex] = nasycenie;
                }
            }

            #region gdyKalibracjaGotowa
            if (czyKalibracjaGotowa == true)
            {

                HashSet<string> interakcjaXY = ZnajdzSrodekZacmieniaNacisku(pixelsTab);
                //    List<string> poprzedniaInterakcjaLuksferXY = ZnajdzSrodekZacmieniaNacisku(previousPixelsTab);

                aktywnaLuksfera = PrzypiszInterakcjeDoLuksfer(interakcjaXY);
                //       poprzedniaAktywnaLuksfera = PrzypiszInterakcjeDoLuksfer(poprzedniaInterakcjaLuksferXY);

                ZaznaczLuksfery(aktywnaLuksfera);

                previousPixelsTab = pixelsTab;

                if (!AreTheSame(previousPixelsTab, pixelsTab))
                {


                    if (!TabIsEmpty(pixelsTab))
                    {

                    }
                }
            }
            #endregion gdyKalibracjaGotowa

            return tabPiksele;
        }



        private void zapiszPxelsTabDoPliku(int[,] pixelsTab)
        {
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(@"D:\pixelsTab.txt"))
            {
                for (int i = 0; i < 49; i++)
                {
                    for (int j = 0; j < 641; j++)
                    {
                        file.Write(pixelsTab[j, i] + " ");
                    }
                    file.WriteLine();
                }
            }
        }

        private void ZaznaczLuksfery(byte[,] aktywnaLuksfera)
        {
            Labels labelsLux = (Labels)labels;
            labelsLux.MarkLux(aktywnaLuksfera);

            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    if (aktywnaLuksfera[i, j] == 1)
                    {
                        WyslijDaneDoSerwera("p" + i + "" + j);
                    }
                }
            }

        }

        private byte[,] PrzypiszInterakcjeDoLuksfer(HashSet<string> interakcjaXY)
        {
            int interakcjaX = 0, interakcjaY = 0, granicaY = 0, granicaNastepnaY = 0,
                granicaX = 0, granicaNastepnaX = 0, luksferaX = 0, luksferaY = 0;
            byte[,] aktywnaLuksfera = new byte[8, 8];

            foreach (string xy in interakcjaXY)
            {
                interakcjaX = int.Parse(xy.Split('|')[0]);
                interakcjaY = int.Parse(xy.Split('|')[1]);

                for (int i = 0; i < 7; i++)
                {
                    for (int j = 0; j < 8; j++)
                    {
                        if (!string.IsNullOrEmpty(luksferaXY[i, j])
                            && !string.IsNullOrEmpty(luksferaXY[i + 1, j]))
                        {
                            granicaY = int.Parse(luksferaXY[i, j].Split('|')[1]);
                            granicaNastepnaY = int.Parse(luksferaXY[i + 1, j].Split('|')[1]);

                            if (interakcjaY >= granicaY && interakcjaY <= granicaNastepnaY)
                            {
                                luksferaY = i;

                                for (int k = 0; k < 7; k++)
                                {
                                    if (!string.IsNullOrEmpty(luksferaXY[luksferaY, k])
                                        && !string.IsNullOrEmpty(luksferaXY[luksferaY, k + 1]))
                                    {
                                        granicaX = int.Parse(luksferaXY[luksferaY, k].Split('|')[1]);
                                        granicaNastepnaX = int.Parse(luksferaXY[luksferaY, k + 1].Split('|')[1]);

                                        if (interakcjaX > granicaX && interakcjaX <= granicaNastepnaX)
                                        {
                                            luksferaX = k;
                                            aktywnaLuksfera[luksferaY, luksferaX] = 1;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return aktywnaLuksfera;
        }

        private HashSet<string> ZnajdzSrodekZacmieniaNacisku(int[,] pixelsTab)
        {
            int yInterakcja = startIteracjaGlebi / 640;
            bool czyOkreslonoPoczatekZacmienia = false;
            int minimalnaRozpietosc = 20;
            HashSet<string> miejscaInterakcji = new HashSet<string>();
            int poczatek = 0, srodek = 0, koniec = 0;

            for (int x = 0; x < 641; x++)
            {
                if (czyOkreslonoPoczatekZacmienia == false &&
                    pixelsTab[x, yInterakcja] > 0)
                {
                    poczatek = x;
                    czyOkreslonoPoczatekZacmienia = true;
                }
                else if (czyOkreslonoPoczatekZacmienia == true &&
                    (pixelsTab[x, yInterakcja] == 0 || x == 641))
                {
                    koniec = x - 1;
                    if (koniec - poczatek >= minimalnaRozpietosc)
                    {
                        srodek = ((koniec - poczatek) / 2) + poczatek;
                        miejscaInterakcji.Add
                            (srodek.ToString() + "|" + pixelsTab[srodek, yInterakcja].ToString());
                    }
                    czyOkreslonoPoczatekZacmienia = false;
                }
            }
            return miejscaInterakcji;
        }


        private void WyslijDaneDoSerwera(string s)
        {
            if (polaczony)
            {
                byte[] data = Encoding.ASCII.GetBytes(s);

                if (data.Count() != 0)
                {
                    try
                    {
                        socket.Send(data);
                    }
                    catch (Exception ex)
                    {
                        Console.Write(ex);
                    }
                }
            }
        }

        private void WyslijSerwer(string s)
        {
            if (polaczony)
            {
                byte[] data = Encoding.ASCII.GetBytes(s);
                wyslijDoSerwera(data);
            }

        }
        private void wyslijDoSerwera(byte[] data)
        {
            if (data.Count() != 0)
            {
                try
                {

                    socket.Send(data);
                    Thread.Sleep(100);
                    socket.Disconnect(false);
                }
                catch (Exception ex)
                {
                    socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Parse("172.21.9.4"), 1701);
                    socket.Connect(localEndPoint);

                    socket.Send(data);
                    Thread.Sleep(100);
                    socket.Disconnect(false);
                }

            }
        }


        private bool TabIsEmpty(int[,] pixelsTab)
        {
            foreach (int p in pixelsTab)
            {
                if (p != 0)
                {
                    return false;
                }
            }
            return true;
        }

        private bool AreTheSame(int[,] previousPixelsTab, int[,] pixelsTab)
        {
            for (int x = 0; x < 641; x++)
            {
                for (int y = 0; y < 49; y++)
                {
                    if (previousPixelsTab[x, y] != pixelsTab[x, y])
                    {
                        return false;
                    }
                }
            }
            return true;
        }


        private int ZnajdzMaksymalnaZapisanaOdlegloscY()
        {
            int max = 0;
            foreach (string luksfera in luksferaXY)
            {
                if (!string.IsNullOrEmpty(luksfera))
                {
                    int granicaY = int.Parse(luksfera.Split('|')[1]);
                    if (granicaY > max)
                    {
                        max = granicaY;
                    }
                }
            }
            return max;
        }


        private void DepthImage_MouseLeftButtonDown_1(object sender, MouseButtonEventArgs e)
        {
            Point currentPoint = e.GetPosition(Obraz_Glebi);
            int pixelIndex = (int)(currentPoint.X + ((int)currentPoint.Y * this.Obraz_Glebi.Width));
        }

        private void point_add_btn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void line_skip_btn_Click(object sender, RoutedEventArgs e)
        {

        }


        private void done_btn_Click(object sender, RoutedEventArgs e)
        {
            PowolajOknoPodgladu();
            czyKalibracjaGotowa = true;
            //SaveTab();
        }

        private void SaveTab()
        {
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(@"D:\luksferyXY.txt"))
            {
                for (int i = 0; i < 8; i++)
                {
                    for (int j = 0; j < 8; j++)
                    {
                        file.Write(luksferaXY[i, j] + " ");
                    }
                    file.WriteLine();
                }
            }
        }

        private void LoadTab()
        {
            string line;
            int i = 0;
            using (System.IO.StreamReader reader = new System.IO.StreamReader(@"D:\luksferyXY.txt"))
            {
                while (true)
                {
                    line = reader.ReadLine();
                    if (line == null)
                    {
                        break;
                    }
                    string[] bits = line.Split(' ');
                    for (int j = 0; j < bits.Length; j++)
                    {
                        if (!string.IsNullOrEmpty(bits[j]))
                        {
                            luksferaXY[i, j] = bits[j];
                        }
                    }
                    i++;
                }
            }
            maksymalnaOdlegloscY = ZnajdzMaksymalnaZapisanaOdlegloscY();
        }

        private void PowolajOknoPodgladu()
        {
            labels = new Labels();
            labels.Owner = this;
            labels.Show();

            Zapamietaj.IsEnabled = false;
            Nastepny_poziom.IsEnabled = false;
        }

        private void luksfera_Click(object sender, RoutedEventArgs e)
        {
        }


        private void poziom_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            LoadTab();
            //    openWindow();
            //     isCalibrationDone = true;
        }

        bool polaczony = false;
        Socket socket;

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Parse("172.21.9.4"), 1701);
            socket.NoDelay = true;
            try
            {
                socket.Connect(localEndPoint);
                polaczony = true;
            }
            catch
            {
                Console.Write("Nie można połączyć.");
            }
        }



        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            ZapiszWspolrzedneLuksfery();
        }

        private void ZapiszWspolrzedneLuksfery()
        {
            int minOdleglosc = 900;
            int maxOdleglosc = ZnajdzMaksymalnaZapisanaOdlegloscY();
            int przedmiotOdlegloscCzujnik = 0;
            int przedmiotOdlegloscX = 0;
            iteracjaKalibracjiY = int.Parse(levels_lbl.Content.ToString());
            int poprzedniX = 0;
            int antyZaklocenieBrzegiX = 0;
            int odlegloscX = 0;
            startIteracjaGlebi = Convert.ToInt32(rawDepthData.Length * gorneOgraniczenieDanych);

            if (iteracjaKalibracjiX > 0)
            {
                //X poprzedni z tego poziomu X
                antyZaklocenieBrzegiX =
                    int.Parse(luksferaXY[iteracjaKalibracjiY, iteracjaKalibracjiX - 1].Split('|')[0]);
            }
            else if (iteracjaKalibracjiY > 0)
            {
                //poprzedni poziom X
                antyZaklocenieBrzegiX =
                    int.Parse(luksferaXY[iteracjaKalibracjiY - 1, iteracjaKalibracjiX].Split('|')[0]);
            }

            for (int indeksGlebi = startIteracjaGlebi;
                indeksGlebi < rawDepthData.Length * dolneOgraniczenieDanych;
                indeksGlebi++)
            {
                przedmiotOdlegloscCzujnik =
                    rawDepthData[indeksGlebi] >> DepthImageFrame.PlayerIndexBitmaskWidth;
                odlegloscX = indeksGlebi % 640;

                if (przedmiotOdlegloscCzujnik > minOdleglosc &&
                    przedmiotOdlegloscCzujnik <= maksymalnaOdlegloscY &&
                    odlegloscX > antyZaklocenieBrzegiX)
                {
                    przedmiotOdlegloscX = indeksGlebi % 640;
                    maxOdleglosc = przedmiotOdlegloscCzujnik;
                }

            }

            if (przedmiotOdlegloscX > poprzedniX)
            {
                luksferaXY[iteracjaKalibracjiY, iteracjaKalibracjiX] =
                    przedmiotOdlegloscX + "|" + maxOdleglosc.ToString();

                iteracjaKalibracjiX++;
                points_lbl.Content = iteracjaKalibracjiX;
            }
            else
            {
                MessageBox.Show("Brak danych o X.");
            }
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            WyrysujLinie(luksferaXY, iteracjaKalibracjiY);
            iteracjaKalibracjiX = 0;
            iteracjaKalibracjiY++;
            levels_lbl.Content = iteracjaKalibracjiY;
        }

        private void WyrysujLinie(string[,] luksferaXY, int iteracjaKalibracjiY)
        {
            int poprzedniX = 0;
            int poprzedniY = 0;
            int poprzedniPoziomX = 0;
            int poprzedniPoziomY = 0;

            Luxfery a = (Luxfery)luxfery;

            for (int i = 0; i < 8; i++)
            {
                if (!string.IsNullOrEmpty(luksferaXY[iteracjaKalibracjiY, i]))
                {
                    int x = int.Parse(luksferaXY[iteracjaKalibracjiY, i].Split('|')[0]);
                    int y = int.Parse(luksferaXY[iteracjaKalibracjiY, i].Split('|')[1]);


                    if (i == 0 && iteracjaKalibracjiY == 0)
                    {
                        a.LiniaXY(x, y);
                    }
                    else if (i != 0 && iteracjaKalibracjiY == 0)
                    {
                        poprzedniX = int.Parse(luksferaXY[iteracjaKalibracjiY, i - 1].Split('|')[0]);
                        poprzedniY = int.Parse(luksferaXY[iteracjaKalibracjiY, i - 1].Split('|')[1]);
                        a.LiniaXY(poprzedniX, poprzedniY, x, y, iteracjaKalibracjiY);
                    }
                    else if (i == 0 && iteracjaKalibracjiY > 0)
                    {
                        poprzedniPoziomX = int.Parse(luksferaXY[iteracjaKalibracjiY - 1, i].Split('|')[0]);
                        poprzedniPoziomY = int.Parse(luksferaXY[iteracjaKalibracjiY - 1, i].Split('|')[1]);
                        a.LiniaXY(poprzedniPoziomX, poprzedniPoziomY, x, y, iteracjaKalibracjiY);
                    }
                    else
                    {
                        poprzedniPoziomX = int.Parse(luksferaXY[iteracjaKalibracjiY - 1, i].Split('|')[0]);
                        poprzedniPoziomY = int.Parse(luksferaXY[iteracjaKalibracjiY - 1, i].Split('|')[1]);
                        poprzedniX = int.Parse(luksferaXY[iteracjaKalibracjiY, i - 1].Split('|')[0]);
                        poprzedniY = int.Parse(luksferaXY[iteracjaKalibracjiY, i - 1].Split('|')[1]);
                        a.LiniaXY(poprzedniPoziomX, poprzedniPoziomY, poprzedniX, poprzedniY, x, y);
                    }
                }
            }
        }

        private void Poprzedni_poziom_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Poprzedni_poziom_Click_1(object sender, RoutedEventArgs e)
        {

        }

        private void Poprzedni_poziom_Click_2(object sender, RoutedEventArgs e)
        {
            SaveTab();
        }
    }
}
