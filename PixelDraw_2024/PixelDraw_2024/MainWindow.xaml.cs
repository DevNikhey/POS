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

namespace PixelDraw_2024
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static readonly int imageSize = 300;
        private static WriteableBitmap _wb;
        private static int _bytesPerPixel;
        private static int _stride;
        private static byte[] _colorArray;

        public MainWindow()
        {
            InitializeComponent();
            _wb = new WriteableBitmap(imageSize, imageSize, 96, 96, PixelFormats.Bgra32, null);
            _bytesPerPixel = (_wb.Format.BitsPerPixel + 7) / 8;
            _stride = _wb.PixelWidth * _bytesPerPixel;
            _colorArray = ConvertColor(Colors.Black);
            drawing.Source = _wb;
        }

        #region Hilfsfunktionen

        private static byte[] ConvertColor(Color color)
        {
            byte[] c = new byte[4];
            c[0] = color.B;
            c[1] = color.G;
            c[2] = color.R;
            c[3] = color.A;
            return c;
        }

        private static Color ConvertColor(byte[] color)
        {
            Color c = new Color();
            c.B = color[0];
            c.G = color[1];
            c.R = color[2];
            c.A = color[3];
            return c;
        }

        private void setPixel(Color c, int x, int y)
        {
            if (x < _wb.PixelWidth && x > 0 && y < _wb.PixelHeight && y > 0)
            {
                _wb.WritePixels(new Int32Rect(x, y, 1, 1), ConvertColor(c), _stride, 0);
            }
        }

        private void setPixel(int x, int y)
        {
            if (x < _wb.PixelWidth && x > 0 && y < _wb.PixelHeight && y > 0)
            {
                _wb.WritePixels(new Int32Rect(x, y, 1, 1), _colorArray, _stride, 0);
            }
        }

        #endregion

        private void button1_Click(object sender, RoutedEventArgs e)
        {

            List<PointF> points = new List<PointF>()
            {
                new PointF(50, 400),
                new PointF(150, 100),
                new PointF(250, 300),
                new PointF(350, 150),
                new PointF(450, 350),
                new PointF(600, 200),
                new PointF(750, 400)
            };

            DrawCurve(points);

            for (int i = 10; i <= 290; i++)
            {
                setPixel(i, 10);
                setPixel(i, 290);
                setPixel(10, i);
                setPixel(290, i);
            }
            for (int i = 10; i <= 290; i += 20)
            {
                drawLine(150, 150, 10, i);
                drawLine(150, 150, 290, i);
                drawLine(150, 150, i, 10);
                drawLine(150, 150, i, 290);
            }
        }

        private void drawLine(int x1, int y1, int x2, int y2)
        {
            //hier euren Algorithmus implementieren
            //setPixel(x, y); zeichnet einen Punkt
            int distanzX = x2 - x1;
            int distanzy = y2 - y1;

            if(distanzX < 0)
            {
                distanzX *= -1;
            }
            if(distanzy < 0)
            {
                distanzy *= -1;
            }

            int anzahl = Math.Max(distanzX, distanzy);
            float addX = distanzX / (float)anzahl;
            float addY = distanzy / (float)anzahl;
            float tempx = x1;
            float tempy = y1;
            for (int i = 0; i <= anzahl; i++)
            {
                setPixel((int)tempx, (int)tempy);
                tempx += addX;
                tempy += addY;
            }
        }

        public struct PointF
        {
            public float X;
            public float Y;
            public PointF(float x, float y) { X = x; Y = y; }
        }

        public void DrawCurve(List<PointF> points)
        {
            if (points.Count < 2)
                return;

            for (int i = 0; i < points.Count - 1; i++)
            {
                PointF p0 = (i == 0) ? points[i] : points[i - 1];
                PointF p1 = points[i];
                PointF p2 = points[i + 1];
                PointF p3 = (i + 2 < points.Count) ? points[i + 2] : points[i + 1];

                for (int step = 0; step <= 100; step++)
                {
                    float t = step / 100f;

                    PointF p = CatmullRom(p0, p1, p2, p3, t);

                    setPixel((int)Math.Round(p.X), (int)Math.Round(p.Y));
                }
            }
        }

        private PointF CatmullRom(PointF p0, PointF p1, PointF p2, PointF p3, float t)
        {
            float t2 = t * t;
            float t3 = t2 * t;

            float x = 0.5f *
                (2 * p1.X +
                (-p0.X + p2.X) * t +
                (2 * p0.X - 5 * p1.X + 4 * p2.X - p3.X) * t2 +
                (-p0.X + 3 * p1.X - 3 * p2.X + p3.X) * t3);

            float y = 0.5f *
                (2 * p1.Y +
                (-p0.Y + p2.Y) * t +
                (2 * p0.Y - 5 * p1.Y + 4 * p2.Y - p3.Y) * t2 +
                (-p0.Y + 3 * p1.Y - 3 * p2.Y + p3.Y) * t3);

            return new PointF(x, y);
        }
    }
}