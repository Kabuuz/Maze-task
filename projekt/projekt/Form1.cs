using Emgu.CV;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace projekt
{
    public partial class Form1 : Form
    {
        Image<Bgr, byte> obrazWczytany, obrazPoSegmentacji;//przechowywanie obrazu
        VideoCapture kamera;//do pobrania obrazu z kamery

        private Size wymaganyRozmiar;//do skalowania

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
            zPliku = CvInvoke.Imread(@"D:\Studia\7sem\Systemy wizyjne\Projekt\test.png");
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


        //Segmentacja
        Queue<Point> pix_tlace = new Queue<Point>();
        Queue<Point> pix_palace = new Queue<Point>();
        Queue<Point> pix_nadpalone = new Queue<Point>();
        Queue<Point> pix_wypalone = new Queue<Point>();

        private MCvScalar cecha_palnosci = new MCvScalar(0xFF, 0xFF, 0xFF);
        private MCvScalar cecha_nadpalenia = new MCvScalar(0, 0, 0);

        private MCvScalar kolor_tlenia = new MCvScalar(51, 153, 255);
        private MCvScalar kolor_palenia = new MCvScalar(0, 0, 204);
        private MCvScalar kolor_nadpalenia = new MCvScalar(51, 204, 51);
        private MCvScalar aktualny_kolor_wypalenia = new MCvScalar(100, 100, 100);

        private MCvScalar kolorSciezki = new MCvScalar(255, 255, 255);

        private int nr_pozaru = 0;
        private bool skos = false;
        private bool cecha_dowolna = false;

        private void Wyczysc_dane_pozaru()
        {
            nr_pozaru = 0;
            pix_nadpalone.Clear();
            pix_palace.Clear();
            pix_tlace.Clear();
            pix_wypalone.Clear();
        }

        private void resetujKolory()
        {
            kolor_nadpalenia.V0 = 51;
            kolor_nadpalenia.V1 = 204;
            kolor_nadpalenia.V2 = 51;
            aktualny_kolor_wypalenia.V0 = 100;
            aktualny_kolor_wypalenia.V1 = 100;
            aktualny_kolor_wypalenia.V2 = 100;
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
            //Należy zobaczyć co się stanie z rysunkiem innym niż *.bmp i/lub takim na którym została wywołana metoda
            //resize zarówno dla cechy dowolnej (jakiejkolwiek) jak i konkretnej
            //Należy zwrócic uwagę na nieoczekiwane zmiany kolorów na modyfikowanych lub kompresowanych obrazach
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
            if (B == cecha_palnosci.V0 && G == cecha_palnosci.V1 && R == cecha_palnosci.V2)
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
            if (B == cecha_palnosci.V0 && G == cecha_palnosci.V1 && R == cecha_palnosci.V2)
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
            Wyczysc_dane_pozaru();

            int width = obrazWczytany.Width;
            int height = obrazWczytany.Height;
            byte[,,] temp = obrazWczytany.Data;

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (Sprawdz_czy_cecha_palnosci(temp[y, x, 0], temp[y, x, 1], temp[y, x, 2]))
                    {
                        pix_tlace.Enqueue(new Point(x, y));
                        temp[y, x, 0] = (byte)kolor_tlenia.V0;
                        temp[y, x, 1] = (byte)kolor_tlenia.V1;
                        temp[y, x, 2] = (byte)kolor_tlenia.V2;
                        nr_pozaru++;
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
                        kolor_nadpalenia.V0++;
                        kolor_nadpalenia.V1++;
                        kolor_nadpalenia.V2++;
                        aktualny_kolor_wypalenia.V0++;
                        aktualny_kolor_wypalenia.V1++;
                        aktualny_kolor_wypalenia.V2++;
                    }
                }
            }
        }

        private void buttonRozpocznijSegmentacje_Click(object sender, EventArgs e)
        {
            Pozar_Calosci();
        }

        private bool sprawdzCzyNalezyDoElementu(byte B, byte G, byte R, int nr_elementu)
        {
            resetujKolory();
            if ((B == (byte)(aktualny_kolor_wypalenia.V0 + nr_elementu-1) &&
                G == (byte)(aktualny_kolor_wypalenia.V1 + nr_elementu - 1) &&
                R == (byte)(aktualny_kolor_wypalenia.V2 + nr_elementu - 1)) ||
                (B == (byte)(kolor_nadpalenia.V0 + nr_elementu - 1) &&
                G == (byte)(kolor_nadpalenia.V1 + nr_elementu - 1) &&
                R == (byte)(kolor_nadpalenia.V2 + nr_elementu - 1)))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private void buttonKopiujElement_Click(object sender, EventArgs e)
        {
            byte[,,] temp = obrazWczytany.Data;
            int wysokosc = obrazWczytany.Height;
            int szerokosc = obrazWczytany.Width;
            byte[,,] element = new byte[wysokosc, szerokosc, 3];

            int nr_elementu = (int)numericUpDownWyborElementu.Value;

            for (int i = 0; i < wysokosc; i++)
            {
                for (int j = 0; j < szerokosc; j++)
                {
                    if (sprawdzCzyNalezyDoElementu(temp[i, j, 0], temp[i, j, 1], temp[i, j, 2], nr_elementu))
                    {
                        element[i, j, 0] = (byte)kolorSciezki.V0;
                        element[i, j, 1] = (byte)kolorSciezki.V1;
                        element[i, j, 2] = (byte)kolorSciezki.V2;
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
