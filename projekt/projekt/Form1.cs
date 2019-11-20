using Emgu.CV;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.Drawing;
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

        public Form1()
        {
            InitializeComponent();

            wymaganyRozmiar = pictureBoxWczytanyObraz.Size;
            obrazWczytany = new Image<Bgr, byte>(wymaganyRozmiar);

            kamera = new VideoCapture(0);//inicjalizacja i start kamery
            kamera.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.FrameWidth, wymaganyRozmiar.Width);
            kamera.Start();
        }

        private void buttonZPliku_Click(object sender, EventArgs e)
        {
            Mat zPliku;//plik przechowywujacy png/jpg
            zPliku = CvInvoke.Imread(@"D:\Studia\7sem\Systemy wizyjne\Projekt\labirynt_paint.png");
            //zPliku = CvInvoke.Imread(@"D:\Studia\7sem\Systemy wizyjne\Projekt\labirynt_paint.png");//wczytaj plik
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

        private MCvScalar kolorSciezkiWypalanie = new MCvScalar(60, 60, 60);
        private MCvScalar kolorPilkiWypalanie = new MCvScalar(45, 45, 45);
        private MCvScalar kolorScianWypalanie = new MCvScalar(30, 30, 30);

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
        private MCvScalar kolorPilki = new MCvScalar(0, 255, 255);

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
                    if (temp[y, x, 0] == kolorSciezki.V0 && temp[y, x, 1] == kolorSciezki.V1 && temp[y, x, 2] == kolorSciezki.V2)
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
                    if (temp[y, x, 0] == kolorScian.V0 && temp[y, x, 1] == kolorScian.V1 && temp[y, x, 2] == kolorScian.V2)
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
        }

        private void buttonRozpocznijSegmentacje_Click(object sender, EventArgs e)
        {
            progowanie();
            Pozar_Calosci();
        }

        private bool czyNalezyDoElementu(byte B, byte G, byte R, int nr_el)
        {
            //nr el: 1-droga, 2- sciana, 3- pilka
            if (nr_el == 1)
            {
                if (B == kolorSciezkiNadpalanie.V0 && G == kolorSciezkiNadpalanie.V1 && R == kolorSciezkiNadpalanie.V2 ||
                    B == kolorSciezkiWypalanie.V0 && G == kolorSciezkiWypalanie.V1 && R == kolorSciezkiWypalanie.V2)
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
                    if(czyNalezyDoElementu(temp[wys, szer, 0], temp[wys, szer, 1], temp[wys, szer, 2], nr_elementu))
                    {
                        //1 - droga, 2 - sciana, 3 - pilka
                        if (nr_elementu == 1)
                        {
                            element[wys, szer, 0] =(byte)kolorSciezkiKopiowanie.V0;
                            element[wys, szer, 1] = (byte)kolorSciezkiKopiowanie.V1;
                            element[wys, szer, 2] = (byte)kolorSciezkiKopiowanie.V2;
                        }
                        else if(nr_elementu==2)
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

        private void buttonPokazWektory_Click(object sender, EventArgs e)
        {

        }

        private void buttonPokazSciezke_Click(object sender, EventArgs e)
        {

        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            kamera.Stop();//zatrzymanie kamery po wylączeniu aplikacji
        }
    }
}
