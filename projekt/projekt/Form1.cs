using Emgu.CV;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace projekt
{
    public partial class Form1 : Form
    {
        Image<Bgr, byte> obrazWczytany, obrazPoSegmentacji;//przechowywanie obrazu
        VideoCapture kamera;//do pobrania obrazu z kamery

        private Size wymaganyRozmiar;//do skalowania

        private byte[] LUTprog = new byte[256];
        private byte prog;

        private Point start;
        private Point stop;
        private Point srodekPilki;
        private int promienPilki;
        private int dlugoscWektoraPrzesuwania;

        private enum wektory { E, NE, N, NW, W, SW, S, SE };
        private Point[] wektoryRuchow = new Point[8];//wektory po jakich moze poruszac sie pilka:E,NE,N,NW,W,SW,S,SE

        public Form1()
        {
            InitializeComponent();

            wymaganyRozmiar = pictureBoxWczytanyObraz.Size;
            obrazWczytany = new Image<Bgr, byte>(wymaganyRozmiar);

            /*kamera = new VideoCapture(0);//inicjalizacja i start kamery
            kamera.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.FrameWidth, wymaganyRozmiar.Width);
            kamera.Start();*/

            //inicjalizacja wektorów
            wektoryRuchow[(int)wektory.E] = new Point(1, 0);
            wektoryRuchow[(int)wektory.SE] = new Point(1, 1);
            wektoryRuchow[(int)wektory.S] = new Point(0, 1);
            wektoryRuchow[(int)wektory.SW] = new Point(-1, 1);
            wektoryRuchow[(int)wektory.W] = new Point(-1, 0);
            wektoryRuchow[(int)wektory.NW] = new Point(-1, -1);
            wektoryRuchow[(int)wektory.N] = new Point(0, -1);
            wektoryRuchow[(int)wektory.NE] = new Point(1, -1);

        }

        private void buttonZPliku_Click(object sender, EventArgs e)
        {
            string path = Directory.GetCurrentDirectory();

            Mat zPliku;//plik przechowywujacy png/jpg
            zPliku = CvInvoke.Imread(@"..\\..\\..\\..\\..\\labirynt_paint.png");
            CvInvoke.Resize(zPliku, zPliku, pictureBoxWczytanyObraz.Size);//zmien rozmiar(skąd,dokąd,rozmiar docelowy)
            obrazWczytany = zPliku.ToImage<Bgr, byte>();//skopiowanie wczytanego obrazu do pamięci
            pictureBoxWczytanyObraz.Image = obrazWczytany.Bitmap;//wyświetlenie obrazu
        }

        private void buttonZKamery_Click(object sender, EventArgs e)
        {
            Mat zKamery = new Mat();
            kamera.Read(zKamery);//podbranie obrazu z kamery
            CvInvoke.Resize(zKamery, zKamery, pictureBoxWczytanyObraz.Size);
            obrazWczytany = zKamery.ToImage<Bgr, byte>();
            pictureBoxWczytanyObraz.Image = obrazWczytany.Bitmap;
        }

        private void progowanie()
        {
            prog = (byte)numericUpDownProg.Value;
            for (int i = 0; i < 256; i++)
            {
                if (i < prog)
                {
                    LUTprog[i] = 0;
                }
                else if (i >= prog)
                {
                    LUTprog[i] = 255;
                }
            }

            byte[,,] temp = obrazWczytany.Data;
            int wysokosc = obrazWczytany.Height;
            int szerokosc = obrazWczytany.Width;

            for (int wys = 0; wys < wysokosc; wys++)
            {
                for (int szer = 0; szer < szerokosc; szer++)
                {
                    temp[wys, szer, 0] = LUTprog[temp[wys, szer, 0]];
                    temp[wys, szer, 1] = LUTprog[temp[wys, szer, 1]];
                    temp[wys, szer, 2] = LUTprog[temp[wys, szer, 2]];
                }
            }

            obrazWczytany.Data = temp;
            pictureBoxWczytanyObraz.Image = obrazWczytany.Bitmap;
        }

        //Segmentacja
        Queue<Point> pix_tlace = new Queue<Point>();
        Queue<Point> pix_palace = new Queue<Point>();
        Queue<Point> pix_nadpalone = new Queue<Point>();
        Queue<Point> pix_wypalone = new Queue<Point>();

        private MCvScalar cecha_nadpalenia = new MCvScalar(0, 0, 0);

        private MCvScalar kolor_tlenia = new MCvScalar(51, 153, 255);
        private MCvScalar kolor_palenia = new MCvScalar(0, 0, 204);
        private MCvScalar kolor_nadpalenia = new MCvScalar(51, 204, 51);
        private MCvScalar aktualny_kolor_wypalenia = new MCvScalar(100, 100, 100);
        //kolor wypalenai
        private MCvScalar kolorSciezkiWypalanie = new MCvScalar(60, 60, 60);
        private MCvScalar kolorPilkiWypalanie = new MCvScalar(45, 45, 45);
        private MCvScalar kolorScianWypalanie = new MCvScalar(30, 30, 30);
        //kolor nadpalenia
        private MCvScalar kolorSciezkiNadpalanie = new MCvScalar(60, 60, 128);
        private MCvScalar kolorPilkiNadpalanie = new MCvScalar(45, 45, 128);
        private MCvScalar kolorScianNadpalanie = new MCvScalar(30, 30, 128);
        //jaki wyswietlic w kopiowaniu
        private MCvScalar kolorSciezkiKopiowanie = new MCvScalar(255, 255, 255);
        private MCvScalar kolorScianKopiowanie = new MCvScalar(255, 0, 0);
        private MCvScalar kolorPilkiKopiowanie = new MCvScalar(0, 255, 255);
        //kolor po progowaniu
        private MCvScalar kolorSciezki = new MCvScalar(255, 255, 255);
        private MCvScalar kolorScian = new MCvScalar(255, 0, 0);
        private MCvScalar kolorScian2 = new MCvScalar(255, 255, 0);
        private MCvScalar kolorPilki = new MCvScalar(0, 255, 255);
        private MCvScalar kolorStart = new MCvScalar(0, 255, 0);
        private MCvScalar kolorStop = new MCvScalar(0, 0, 255);

        private bool skos = false;
        private bool cecha_dowolna = false;

        private void Wyczysc_dane_pozaru()
        {
            pix_nadpalone.Clear();
            pix_palace.Clear();
            pix_tlace.Clear();
            pix_wypalone.Clear();
        }

        private void Tlace_do_palacych(byte[,,] temp)
        {
            while (pix_tlace.Count > 0)
            {
                Point p = pix_tlace.Dequeue();
                pix_palace.Enqueue(p);
                temp[p.Y, p.X, 0] = (byte)kolor_palenia.V0;
                temp[p.Y, p.X, 1] = (byte)kolor_palenia.V1;
                temp[p.Y, p.X, 2] = (byte)kolor_palenia.V2;
            }
        }

        private void Tlenie_od_palacego(byte[,,] temp, Point pix_in)
        {
            if (Czy_piksel_w_zakresie(pix_in))
            {
                Point[] sasiedzi = Wylicz_wspolrzedne_sasiednich_pikseli(pix_in);
                foreach (Point p in sasiedzi)
                {
                    if (Sprawdz_czy_cecha_palnosci(temp[p.Y, p.X, 0], temp[p.Y, p.X, 1], temp[p.Y, p.X, 2]))
                    {
                        pix_tlace.Enqueue(new Point(p.X, p.Y));
                        temp[p.Y, p.X, 0] = (byte)kolor_tlenia.V0;
                        temp[p.Y, p.X, 1] = (byte)kolor_tlenia.V1;
                        temp[p.Y, p.X, 2] = (byte)kolor_tlenia.V2;
                    }
                }
            }
        }

        private void Nadpalenie_palacego(byte[,,] temp, Point pix_in)
        {
            if (Czy_piksel_w_zakresie(pix_in))
            {
                Point[] sasiedzi = Wylicz_wspolrzedne_sasiednich_pikseli(pix_in);
                bool nalezy_nadpalic = false;
                foreach (Point p in sasiedzi)
                {
                    if (cecha_dowolna)
                        nalezy_nadpalic = Sprawdz_czy_jakiekolwiek_nadpalenie(temp[p.Y, p.X, 0], temp[p.Y, p.X, 1], temp[p.Y, p.X, 2]);
                    else
                        nalezy_nadpalic = Sprawdz_czy_cecha_nadpalenia(temp[p.Y, p.X, 0], temp[p.Y, p.X, 1], temp[p.Y, p.X, 2]);
                    if (nalezy_nadpalic)
                    {
                        pix_nadpalone.Enqueue(new Point(p.X, p.Y));
                        temp[p.Y, p.X, 0] = (byte)(kolor_nadpalenia.V0);
                        temp[p.Y, p.X, 1] = (byte)(kolor_nadpalenia.V1);
                        temp[p.Y, p.X, 2] = (byte)(kolor_nadpalenia.V2);
                    }
                }
            }
        }

        private void Wypalenie_palacego(byte[,,] temp)
        {
            while (pix_palace.Count > 0)
            {
                Point p = pix_palace.Dequeue();
                pix_wypalone.Enqueue(p);
                temp[p.Y, p.X, 0] = (byte)(aktualny_kolor_wypalenia.V0);
                temp[p.Y, p.X, 1] = (byte)(aktualny_kolor_wypalenia.V1);
                temp[p.Y, p.X, 2] = (byte)(aktualny_kolor_wypalenia.V2);
            }
        }

        private Point[] Wylicz_wspolrzedne_sasiednich_pikseli(Point pix_in)
        {
            List<Point> sasiedzi = new List<Point>();
            sasiedzi.Add(new Point(pix_in.X - 1, pix_in.Y));
            sasiedzi.Add(new Point(pix_in.X + 1, pix_in.Y));
            sasiedzi.Add(new Point(pix_in.X, pix_in.Y - 1));
            sasiedzi.Add(new Point(pix_in.X, pix_in.Y + 1));

            if (skos)
            {
                sasiedzi.Add(new Point(pix_in.X - 1, pix_in.Y - 1));
                sasiedzi.Add(new Point(pix_in.X + 1, pix_in.Y + 1));
                sasiedzi.Add(new Point(pix_in.X - 1, pix_in.Y + 1));
                sasiedzi.Add(new Point(pix_in.X + 1, pix_in.Y - 1));
            }

            return sasiedzi.ToArray();
        }

        private bool Czy_piksel_w_zakresie(Point pix_in)
        {
            int max_W, max_H;
            max_W = wymaganyRozmiar.Width - 1;
            max_H = wymaganyRozmiar.Height - 1;
            if (pix_in.X > 0 && pix_in.X < max_W && pix_in.Y > 0 && pix_in.Y < max_H)
                return true;
            else
                return false;
        }

        private bool Sprawdz_czy_cecha_palnosci(byte B, byte G, byte R)
        {
            if (B == kolorSciezki.V0 && G == kolorSciezki.V1 && R == kolorSciezki.V2)//||
                //B == kolorScian.V0 && G == kolorScian.V1 && R == kolorScian.V2 ||
                //B == kolorPilki.V0 && G == kolorPilki.V1 && R == kolorPilki.V2 )
                return true;
            else
                return false;
        }

        private bool Sprawdz_czy_cecha_nadpalenia(byte B, byte G, byte R)
        {
            if (B == cecha_nadpalenia.V0 && G == cecha_nadpalenia.V1 && R == cecha_nadpalenia.V2)
                return true;
            else
                return false;
        }

        private bool Sprawdz_czy_jakiekolwiek_nadpalenie(byte B, byte G, byte R)
        {
            if (B == kolorPilki.V0 && G == kolorPilki.V1 && R == kolorPilki.V2 ||
                B == kolorScian.V0 && G == kolorScian.V1 && R == kolorScian.V2 ||
                B == kolorSciezki.V0 && G == kolorSciezki.V1 && R == kolorSciezki.V2)
                return false;
            else if (B == cecha_nadpalenia.V0 && G == cecha_nadpalenia.V1 && R == cecha_nadpalenia.V2)
                return true;
            else if (B == kolor_tlenia.V0 && G == kolor_tlenia.V1 && R == kolor_tlenia.V2)
                return false;
            else if (B == kolor_nadpalenia.V0 && G == kolor_nadpalenia.V1 && R == kolor_nadpalenia.V2)
                return false;
            else if (B == kolor_palenia.V0 && G == kolor_palenia.V1 && R == kolor_palenia.V2)
                return false;
            else if (B == aktualny_kolor_wypalenia.V0 && G == aktualny_kolor_wypalenia.V1 && R == aktualny_kolor_wypalenia.V2)
                return false;
            else
                return true;
        }

        private void Pozar_Calosci()
        {
            int width = obrazWczytany.Width;
            int height = obrazWczytany.Height;
            byte[,,] temp = obrazWczytany.Data;

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (temp[y, x, 0] == kolorSciezki.V0 && temp[y, x, 1] == kolorSciezki.V1 && temp[y, x, 2] == kolorSciezki.V2 ||
                        temp[y, x, 0] == kolorStart.V0 && temp[y, x, 1] == kolorStart.V1 && temp[y, x, 2] == kolorStart.V2 ||
                        temp[y, x, 0] == kolorStop.V0 && temp[y, x, 1] == kolorStop.V1 && temp[y, x, 2] == kolorStop.V2)
                    {
                        kolor_nadpalenia.V0 = kolorSciezkiNadpalanie.V0;
                        kolor_nadpalenia.V1 = kolorSciezkiNadpalanie.V1;
                        kolor_nadpalenia.V2 = kolorSciezkiNadpalanie.V2;
                        aktualny_kolor_wypalenia.V0 = kolorSciezkiWypalanie.V0;
                        aktualny_kolor_wypalenia.V1 = kolorSciezkiWypalanie.V1;
                        aktualny_kolor_wypalenia.V2 = kolorSciezkiWypalanie.V2;

                        pix_tlace.Enqueue(new Point(x, y));
                        temp[y, x, 0] = (byte)kolor_tlenia.V0;
                        temp[y, x, 1] = (byte)kolor_tlenia.V1;
                        temp[y, x, 2] = (byte)kolor_tlenia.V2;

                        while (pix_tlace.Count() != 0)
                        {
                            Tlace_do_palacych(temp);

                            foreach (Point pix in pix_palace)
                            {
                                Tlenie_od_palacego(temp, pix);
                            }

                            foreach (Point pix in pix_palace)
                            {
                                Nadpalenie_palacego(temp, pix);
                            }

                            Wypalenie_palacego(temp);

                            obrazWczytany.Data = temp;
                            pictureBoxWczytanyObraz.Image = obrazWczytany.Bitmap;
                            Application.DoEvents();
                        }
                    }
                }
            }
            Wyczysc_dane_pozaru();
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (temp[y, x, 0] == kolorPilki.V0 && temp[y, x, 1] == kolorPilki.V1 && temp[y, x, 2] == kolorPilki.V2)
                    {
                        kolor_nadpalenia.V0 = kolorPilkiNadpalanie.V0;
                        kolor_nadpalenia.V1 = kolorPilkiNadpalanie.V1;
                        kolor_nadpalenia.V2 = kolorPilkiNadpalanie.V2;
                        aktualny_kolor_wypalenia.V0 = kolorPilkiWypalanie.V0;
                        aktualny_kolor_wypalenia.V1 = kolorPilkiWypalanie.V1;
                        aktualny_kolor_wypalenia.V2 = kolorPilkiWypalanie.V2;

                        pix_tlace.Enqueue(new Point(x, y));
                        temp[y, x, 0] = (byte)kolor_tlenia.V0;
                        temp[y, x, 1] = (byte)kolor_tlenia.V1;
                        temp[y, x, 2] = (byte)kolor_tlenia.V2;

                        while (pix_tlace.Count() != 0)
                        {
                            Tlace_do_palacych(temp);

                            foreach (Point pix in pix_palace)
                            {
                                Tlenie_od_palacego(temp, pix);
                            }

                            foreach (Point pix in pix_palace)
                            {
                                Nadpalenie_palacego(temp, pix);
                            }

                            Wypalenie_palacego(temp);

                            obrazWczytany.Data = temp;
                            pictureBoxWczytanyObraz.Image = obrazWczytany.Bitmap;
                            Application.DoEvents();
                        }
                    }
                }
            }
            Wyczysc_dane_pozaru();
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (temp[y, x, 0] == kolorScian.V0 && temp[y, x, 1] == kolorScian.V1 && temp[y, x, 2] == kolorScian.V2
                        || temp[y, x, 0] == kolorScian2.V0 && temp[y, x, 1] == kolorScian2.V1 && temp[y, x, 2] == kolorScian2.V2)
                    {
                        kolor_nadpalenia.V0 = kolorScianNadpalanie.V0;
                        kolor_nadpalenia.V1 = kolorScianNadpalanie.V1;
                        kolor_nadpalenia.V2 = kolorScianNadpalanie.V2;
                        aktualny_kolor_wypalenia.V0 = kolorScianWypalanie.V0;
                        aktualny_kolor_wypalenia.V1 = kolorScianWypalanie.V1;
                        aktualny_kolor_wypalenia.V2 = kolorScianWypalanie.V2;

                        pix_tlace.Enqueue(new Point(x, y));
                        temp[y, x, 0] = (byte)kolor_tlenia.V0;
                        temp[y, x, 1] = (byte)kolor_tlenia.V1;
                        temp[y, x, 2] = (byte)kolor_tlenia.V2;

                        while (pix_tlace.Count() != 0)
                        {
                            Tlace_do_palacych(temp);

                            foreach (Point pix in pix_palace)
                            {
                                Tlenie_od_palacego(temp, pix);
                            }

                            foreach (Point pix in pix_palace)
                            {
                                Nadpalenie_palacego(temp, pix);
                            }

                            Wypalenie_palacego(temp);

                            obrazWczytany.Data = temp;
                            pictureBoxWczytanyObraz.Image = obrazWczytany.Bitmap;
                            Application.DoEvents();
                        }
                    }
                }
            }
        }

        //Mechanika
        private Point znajdzSrodekCiezkosciKoloru(MCvScalar kolor)
        {
            Point tempPoint;
            double F, Sx, Sy, x0, y0;

            F = Sx = Sy = x0 = y0 = 0;

            byte[,,] temp = obrazWczytany.Data;
            int wysokosc = obrazWczytany.Height;
            int szerokosc = obrazWczytany.Width;
            for (int X = 0; X < szerokosc; X++)
            {
                for (int Y = 0; Y < wysokosc; Y++)
                {
                    if (temp[Y, X, 0] == kolor.V0 && temp[Y, X, 1] == kolor.V1 && temp[Y, X, 2] == kolor.V2)
                    {
                        F = F + 1;
                        Sx = Sx + Y;
                        Sy = Sy + X;
                    }
                }
            }
            //Obliczenie środka cieżkości
            if (F > 0)
            {
                x0 = Sy / F;
                y0 = Sx / F;
            }

            tempPoint = new Point((int)x0, (int)y0);

            return tempPoint;
        }

        private int obliczaniePromieniaPilki()//sprawdzanie odlelosci pion/poziom do kranca pilki od srodka
        {
            int[] odleglosciOdSrodka = new int[4];

            byte[,,] obraz = obrazWczytany.Data;
            int wysokosc = obrazWczytany.Height;
            int szerokosc = obrazWczytany.Width;


            for (int wys = srodekPilki.Y; wys < wysokosc; wys++)
            {
                if (obraz[wys, srodekPilki.X, 0] != kolorPilki.V0)
                {
                    odleglosciOdSrodka[0] = wys - srodekPilki.Y;
                    break;
                }
            }
            for (int wys = srodekPilki.Y; wys > -1; wys--)
            {
                if (obraz[wys, srodekPilki.X, 0] != kolorPilki.V0)
                {
                    odleglosciOdSrodka[1] = srodekPilki.Y - wys;
                    break;
                }
            }
            for (int szer = srodekPilki.X; szer < szerokosc; szer++)
            {
                if (obraz[srodekPilki.Y, szer, 0] != kolorPilki.V0)
                {
                    odleglosciOdSrodka[2] = szer - srodekPilki.X;
                    break;
                }
            }
            for (int szer = srodekPilki.X; szer > -1; szer--)
            {
                if (obraz[srodekPilki.Y, szer, 0] != kolorPilki.V0)
                {
                    odleglosciOdSrodka[3] = srodekPilki.X - szer;
                    break;
                }
            }

            Array.Sort(odleglosciOdSrodka);
            return ((odleglosciOdSrodka[3] + odleglosciOdSrodka[0]) / 2);//usredniony promien z najwiekszej i najmniejszej odleglosci
        }


        //Przyciski
        private void buttonRozpocznijSegmentacje_Click(object sender, EventArgs e)
        {
            start = znajdzSrodekCiezkosciKoloru(kolorStart);
            stop = znajdzSrodekCiezkosciKoloru(kolorStop);
            srodekPilki = znajdzSrodekCiezkosciKoloru(kolorPilki);
            promienPilki = obliczaniePromieniaPilki();
            Pozar_Calosci();
        }

        private bool czyNalezyDoElementu(byte B, byte G, byte R, int nr_el)
        {
            //nr el: 1-droga, 2- sciana, 3- pilka
            if (nr_el == 1)
            {
                if (B == kolorSciezkiNadpalanie.V0 && G == kolorSciezkiNadpalanie.V1 && R == kolorSciezkiNadpalanie.V2 ||
                    B == kolorSciezkiWypalanie.V0 && G == kolorSciezkiWypalanie.V1 && R == kolorSciezkiWypalanie.V2 ||
                    B == kolorPilkiNadpalanie.V0 && G == kolorPilkiNadpalanie.V1 && R == kolorPilkiNadpalanie.V2 ||
                    B == kolorPilkiWypalanie.V0 && G == kolorPilkiWypalanie.V1 && R == kolorPilkiWypalanie.V2)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else if (nr_el == 2)
            {
                if (B == kolorScianNadpalanie.V0 && G == kolorScianNadpalanie.V1 && R == kolorScianNadpalanie.V2 ||
                    B == kolorScianWypalanie.V0 && G == kolorScianWypalanie.V1 && R == kolorScianWypalanie.V2)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                if (B == kolorPilkiNadpalanie.V0 && G == kolorPilkiNadpalanie.V1 && R == kolorPilkiNadpalanie.V2 ||
                    B == kolorPilkiWypalanie.V0 && G == kolorPilkiWypalanie.V1 && R == kolorPilkiWypalanie.V2)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

        }

        private void buttonKopiujElement_Click(object sender, EventArgs e)
        {

            byte[,,] temp = obrazWczytany.Data;
            int wysokosc = obrazWczytany.Height;
            int szerokosc = obrazWczytany.Width;
            byte[,,] element = new byte[wysokosc, szerokosc, 3];

            int nr_elementu = (int)numericUpDownWyborElementu.Value;
            for (int wys = 0; wys < wysokosc; wys++)
            {
                for (int szer = 0; szer < szerokosc; szer++)
                {
                    if (czyNalezyDoElementu(temp[wys, szer, 0], temp[wys, szer, 1], temp[wys, szer, 2], nr_elementu))
                    {
                        //1 - droga, 2 - sciana, 3 - pilka
                        if (nr_elementu == 1)
                        {
                            element[wys, szer, 0] = (byte)kolorSciezkiKopiowanie.V0;
                            element[wys, szer, 1] = (byte)kolorSciezkiKopiowanie.V1;
                            element[wys, szer, 2] = (byte)kolorSciezkiKopiowanie.V2;
                        }
                        else if (nr_elementu == 2)
                        {
                            element[wys, szer, 0] = (byte)kolorScianKopiowanie.V0;
                            element[wys, szer, 1] = (byte)kolorScianKopiowanie.V1;
                            element[wys, szer, 2] = (byte)kolorScianKopiowanie.V2;
                        }
                        else
                        {
                            element[wys, szer, 0] = (byte)kolorPilkiKopiowanie.V0;
                            element[wys, szer, 1] = (byte)kolorPilkiKopiowanie.V1;
                            element[wys, szer, 2] = (byte)kolorPilkiKopiowanie.V2;
                        }

                    }
                }
            }

            obrazPoSegmentacji = new Image<Bgr, byte>(element);
            pictureBoxPoSegmentacji.Image = obrazPoSegmentacji.Bitmap;
        }

        private void narysujDozwolonyWektor(Image<Bgr, byte> obraz, Point wektor)
        {
            byte[,,] temp = obraz.Data;

            int przesuniecieX;
            int przesuniecieY;
            if (Math.Abs(wektor.X) == Math.Abs(wektor.Y))
            {
                przesuniecieX = (int)((double)wektor.X / Math.Sqrt(2.0));
                przesuniecieY = (int)((double)wektor.Y / Math.Sqrt(2.0));
            }
            else
            {
                przesuniecieX = wektor.X;
                przesuniecieY = wektor.Y;
            }

            przesuniecieX *= dlugoscWektoraPrzesuwania;
            przesuniecieY *= dlugoscWektoraPrzesuwania;

            Point sprawdzanyPunkt = new Point(srodekPilki.X + przesuniecieX, srodekPilki.Y + przesuniecieY);

            if (temp[sprawdzanyPunkt.Y, sprawdzanyPunkt.X, 0] == kolorSciezkiKopiowanie.V0
                && temp[sprawdzanyPunkt.Y, sprawdzanyPunkt.X, 1] == kolorSciezkiKopiowanie.V1
                && temp[sprawdzanyPunkt.Y, sprawdzanyPunkt.X, 2] == kolorSciezkiKopiowanie.V2)
            {
                CvInvoke.Line(obraz, srodekPilki, sprawdzanyPunkt, new MCvScalar(0, 0, 255), 4);
            }
        }

        private void buttonPokazWektory_Click(object sender, EventArgs e)
        {
            listViewListaWektorow.Clear();
            dlugoscWektoraPrzesuwania = promienPilki * 2;//nede przesuwac o promien dlatego sprawdzam co jest 2*promien od srodka

            byte[,,] temp = obrazWczytany.Data;
            int wysokosc = obrazWczytany.Height;
            int szerokosc = obrazWczytany.Width;
            byte[,,] element = new byte[wysokosc, szerokosc, 3];

            int nr_elementu = 1;//droga
            for (int wys = 0; wys < wysokosc; wys++)
            {
                for (int szer = 0; szer < szerokosc; szer++)
                {
                    if (czyNalezyDoElementu(temp[wys, szer, 0], temp[wys, szer, 1], temp[wys, szer, 2], nr_elementu))
                    {
                        element[wys, szer, 0] = (byte)kolorSciezkiKopiowanie.V0;
                        element[wys, szer, 1] = (byte)kolorSciezkiKopiowanie.V1;
                        element[wys, szer, 2] = (byte)kolorSciezkiKopiowanie.V2;
                    }
                }
            }

            obrazPoSegmentacji = new Image<Bgr, byte>(element);

            narysujDozwolonyWektor(obrazPoSegmentacji, wektoryRuchow[(int)wektory.E]);
            narysujDozwolonyWektor(obrazPoSegmentacji, wektoryRuchow[(int)wektory.NE]);
            narysujDozwolonyWektor(obrazPoSegmentacji, wektoryRuchow[(int)wektory.N]);
            narysujDozwolonyWektor(obrazPoSegmentacji, wektoryRuchow[(int)wektory.NW]);
            narysujDozwolonyWektor(obrazPoSegmentacji, wektoryRuchow[(int)wektory.W]);
            narysujDozwolonyWektor(obrazPoSegmentacji, wektoryRuchow[(int)wektory.SW]);
            narysujDozwolonyWektor(obrazPoSegmentacji, wektoryRuchow[(int)wektory.S]);
            narysujDozwolonyWektor(obrazPoSegmentacji, wektoryRuchow[(int)wektory.SE]);
            CvInvoke.Circle(obrazPoSegmentacji, srodekPilki, promienPilki, kolorPilki, -1);
            pictureBoxPoSegmentacji.Image = obrazPoSegmentacji.Bitmap;
        }

        private bool pilkaWObszarzeKonca(Point pilka, Point stop, int zasieg)
        {
            if (stop.X >= pilka.X - zasieg && stop.X <= pilka.X + zasieg && stop.Y >= pilka.Y - zasieg && stop.Y <= pilka.Y + zasieg)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private string przygotujWektorKierunku(Point wektor)
        {
            string wektorString = "";

            if(wektor.X==1&&wektor.Y==0)//E
            {
                wektorString = "E,";
            }
            if (wektor.X == 0 && wektor.Y == 1)//S
            {
                wektorString = "S,";
            }
            if (wektor.X == -1 && wektor.Y == 0)//W
            {
                wektorString = "W,";
            }
            if (wektor.X == 0 && wektor.Y == -1)//N
            {
                wektorString = "N,";
            }
            if (wektor.X == 1 && wektor.Y == 1)//SE
            {
                wektorString = "SE,";
            }
            if (wektor.X == -1 && wektor.Y == 1)//SW
            {
                wektorString = "SW,";
            }
            if (wektor.X == -1 && wektor.Y == -1)////NW
            {
                wektorString = "NW,";
            }
            if (wektor.X == 1 && wektor.Y == -1)//NE
            {
                wektorString = "NE,";
            }

            return wektorString;
        }

        private void przesunPilke(ref Point pilka, Point wektor, ref Point wektorPoprzedni, ref Point zakazanyWektor)
        {
            byte[,,] temp = obrazPoSegmentacji.Data;
            if (!(wektor.X == zakazanyWektor.X && wektor.Y == zakazanyWektor.Y)
                && (Math.Abs(wektor.X) + Math.Abs(wektor.Y)) != 0)
            {
                int przesuniecieX;
                int przesuniecieY;
                if (Math.Abs(wektor.X) == Math.Abs(wektor.Y))
                {
                    przesuniecieX = (int)((double)wektor.X * (double)dlugoscWektoraPrzesuwania / Math.Sqrt(2.0));
                    przesuniecieY = (int)((double)wektor.Y * (double)dlugoscWektoraPrzesuwania / Math.Sqrt(2.0));
                }
                else
                {
                    przesuniecieX = wektor.X * dlugoscWektoraPrzesuwania;
                    przesuniecieY = wektor.Y * dlugoscWektoraPrzesuwania;
                }

                Point sprawdzanyPunkt = new Point((pilka.X + przesuniecieX), (pilka.Y + przesuniecieY));

                if (temp[sprawdzanyPunkt.Y, sprawdzanyPunkt.X, 0] == kolorSciezkiKopiowanie.V0
                    && temp[sprawdzanyPunkt.Y, sprawdzanyPunkt.X, 1] == kolorSciezkiKopiowanie.V1
                    && temp[sprawdzanyPunkt.Y, sprawdzanyPunkt.X, 2] == kolorSciezkiKopiowanie.V2)
                {
                    pilka.X += przesuniecieX / 2;
                    pilka.Y += przesuniecieY / 2;
                    zakazanyWektor.X = -wektor.X;
                    zakazanyWektor.Y = -wektor.Y;
                    CvInvoke.Rectangle(obrazPoSegmentacji, new Rectangle(pilka.X - promienPilki,
                        pilka.Y - promienPilki, 2 * promienPilki, 2 * promienPilki),
                        new MCvScalar(0, 0, 255), -1);
                    pictureBoxPoSegmentacji.Image = obrazPoSegmentacji.Bitmap;
                    Application.DoEvents();
                    if (wektor.X != wektorPoprzedni.X || wektor.Y != wektorPoprzedni.Y)
                    {
                        wektorPoprzedni.X = wektor.X;
                        wektorPoprzedni.Y = wektor.Y;

                        listViewListaWektorow.Items.Add(przygotujWektorKierunku(wektor));
                    }
                }
            }
        }

        private void buttonPokazSciezke_Click(object sender, EventArgs e)
        {
            double magicznyWspolczynnik = 1.5;
            dlugoscWektoraPrzesuwania = (int)(magicznyWspolczynnik * (double)promienPilki);
            Point pilka = new Point(start.X, start.Y);
            Point kierunekZabroniony = new Point(0, 0);
            Point kierunekPoprzedni = new Point(0, 0);

            byte[,,] temp = obrazPoSegmentacji.Data;
            Point sprawdzanyPunkt;
            int B = temp[160, 136, 0];
            int G = temp[160, 136, 1];
            int R = temp[160, 136, 2];
            CvInvoke.Rectangle(obrazPoSegmentacji, new Rectangle(pilka.X - promienPilki,
                        pilka.Y - promienPilki, 2 * promienPilki, 2 * promienPilki),
                        new MCvScalar(0, 0, 255), -1);
            for (int i = 0; i < wektoryRuchow.Length; i++)
            {
                sprawdzanyPunkt = new Point(pilka.X + wektoryRuchow[i].X * dlugoscWektoraPrzesuwania, pilka.Y + wektoryRuchow[i].Y * dlugoscWektoraPrzesuwania);
                if (temp[sprawdzanyPunkt.Y, sprawdzanyPunkt.X, 0] == kolorSciezkiKopiowanie.V0
                    && temp[sprawdzanyPunkt.Y, sprawdzanyPunkt.X, 1] == kolorSciezkiKopiowanie.V1
                    && temp[sprawdzanyPunkt.Y, sprawdzanyPunkt.X, 2] == kolorSciezkiKopiowanie.V2)
                {
                    pilka.X = sprawdzanyPunkt.X;
                    pilka.Y = sprawdzanyPunkt.Y;
                    kierunekPoprzedni.X = wektoryRuchow[i].X;
                    kierunekPoprzedni.Y = wektoryRuchow[i].Y;
                    listViewListaWektorow.Items.Add(przygotujWektorKierunku(wektoryRuchow[i]));
                }
            }
            CvInvoke.Rectangle(obrazPoSegmentacji, new Rectangle(pilka.X - promienPilki,
                        pilka.Y - promienPilki, 2 * promienPilki, 2 * promienPilki),
                        new MCvScalar(0, 0, 255), -1);
            while (!pilkaWObszarzeKonca(pilka, stop, dlugoscWektoraPrzesuwania))
            {
                sprawdzanyPunkt = new Point((pilka.X + kierunekPoprzedni.X * dlugoscWektoraPrzesuwania), (pilka.Y + kierunekPoprzedni.Y * dlugoscWektoraPrzesuwania));

                if (temp[sprawdzanyPunkt.Y, sprawdzanyPunkt.X, 0] == kolorSciezkiKopiowanie.V0
                    && temp[sprawdzanyPunkt.Y, sprawdzanyPunkt.X, 1] == kolorSciezkiKopiowanie.V1
                    && temp[sprawdzanyPunkt.Y, sprawdzanyPunkt.X, 2] == kolorSciezkiKopiowanie.V2)
                {
                    przesunPilke(ref pilka, kierunekPoprzedni, ref kierunekPoprzedni, ref kierunekZabroniony);
                }
                else
                {
                    przesunPilke(ref pilka, wektoryRuchow[(int)wektory.E], ref kierunekPoprzedni, ref kierunekZabroniony);
                    przesunPilke(ref pilka, wektoryRuchow[(int)wektory.N], ref kierunekPoprzedni, ref kierunekZabroniony);
                    przesunPilke(ref pilka, wektoryRuchow[(int)wektory.S], ref kierunekPoprzedni, ref kierunekZabroniony);
                    przesunPilke(ref pilka, wektoryRuchow[(int)wektory.W], ref kierunekPoprzedni, ref kierunekZabroniony);
                    przesunPilke(ref pilka, wektoryRuchow[(int)wektory.NE], ref kierunekPoprzedni, ref kierunekZabroniony);
                    przesunPilke(ref pilka, wektoryRuchow[(int)wektory.NW], ref kierunekPoprzedni, ref kierunekZabroniony);
                    przesunPilke(ref pilka, wektoryRuchow[(int)wektory.SE], ref kierunekPoprzedni, ref kierunekZabroniony);
                    przesunPilke(ref pilka, wektoryRuchow[(int)wektory.SW], ref kierunekPoprzedni, ref kierunekZabroniony);
                }

            }
        }

        private void buttonProgowanie_Click(object sender, EventArgs e)
        {
            progowanie();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
           // kamera.Stop();//zatrzymanie kamery po wylączeniu aplikacji
        }
    }
}
