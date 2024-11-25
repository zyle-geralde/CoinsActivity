using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CoinActivity_delaPena
{
    public partial class Form1 : Form
    {
        Bitmap loaded, processed;
        int TotalCoins;
        float TotalAmount;
        int Total5P;
        int Total1P;
        int Total25C;
        int Total10C;
        int Total5C;
        public Form1()
        {
            InitializeComponent();
        }

        private void label12_Click(object sender, EventArgs e)
        {
            
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openFileDialog1.ShowDialog();

        }

        private void openFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            loaded = new Bitmap(openFileDialog1.FileName);
            pictureBox1.Image = loaded;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if(loaded == null)
            {
                return;
            }
            BlackAndWhiteBinaryConversion();
            if(processed == null)
            {
                return;
            }
            initializeTotals();
            GetCoinPixels(processed,ref TotalCoins,ref TotalAmount,ref Total5P,ref Total1P,ref Total25C,ref Total10C,ref Total5C);
            assignTotals();
        }

        private void BlackAndWhiteBinaryConversion()
        {
            if (loaded == null)
                return;

            Bitmap binaryImage = new Bitmap(loaded.Width, loaded.Height);
            int threshold = 180;

            for (int x = 0; x < loaded.Width; ++x)
            {
                for (int y = 0; y < loaded.Height; ++y)
                {
                    Color pixel = loaded.GetPixel(x, y);
                    int grayscale = (pixel.R + pixel.G + pixel.B) / 3;
                    binaryImage.SetPixel(x, y, grayscale < threshold ? Color.Black : Color.White);
                }
            }

            processed = binaryImage;

            pictureBox2.Image = processed;
        }

        public static void GetCoinPixels(Bitmap image,ref int totalCount,ref float totalValue,ref int peso5Count,ref int peso1Count,ref int cent25Count,ref int cent10Count,ref int cent5Count)
        {
            bool[,] seen = InitializeSeenMatrix(image.Width, image.Height);
            List<int> regionSizes = IdentifyRegions(image, seen);

            ProcessRegions(regionSizes,ref totalCount,ref totalValue,ref peso5Count,ref peso1Count,ref cent25Count,ref cent10Count,ref cent5Count);


        }
        private static bool[,] InitializeSeenMatrix(int width, int height)
        {
            return new bool[width, height];
        }

        private static List<int> IdentifyRegions(Bitmap image, bool[,] seen)
        {
            List<int> regionSizes = new List<int>();

            for (int x = 0; x < image.Width; x++)
            {
                for (int y = 0; y < image.Height; y++)
                {
                    if (seen[x, y] || image.GetPixel(x, y).R != 0)
                        continue;

                    int regionSize = CalculateRegionSize(image, x, y, seen);

                    if (regionSize >= 800)
                    {
                        regionSizes.Add(regionSize);
                    }
                }
            }

            return regionSizes;
        }

        private static int CalculateRegionSize(Bitmap image, int startX, int startY, bool[,] visited)
        {
            Stack<(int x, int y)> stack = new Stack<(int x, int y)>();
            stack.Push((startX, startY));

            int regionSize = 0;

            while (stack.Count > 0)
            {
                var (x, y) = stack.Pop();
                if (x < 0 || y < 0 || x >= image.Width || y >= image.Height || visited[x, y])
                    continue;
                visited[x, y] = true;

                if (image.GetPixel(x, y).R != 0)
                    continue;

                regionSize++;

                stack.Push((x - 1, y));
                stack.Push((x + 1, y));
                stack.Push((x, y - 1));
                stack.Push((x, y + 1));
            }

            return regionSize;
        }

        private static void ProcessRegions(List<int> regionSizes,ref int totalCount,ref float totalValue,ref int peso5Count,ref int peso1Count,ref int cent25Count,ref int cent10Count,ref int cent5Count)
        {
            int localPeso5Count = peso5Count;
            int localPeso1Count = peso1Count;
            int localCent25Count = cent25Count;
            int localCent10Count = cent10Count;
            int localCent5Count = cent5Count;

            
            var coinClassification = new List<(int MinSize, float Value, Action)>
            {
                (6800, 5.0f, new Action(() => localPeso5Count++)),   
                (4800, 1.0f, new Action(() => localPeso1Count++)),  
                (3800, 0.25f, new Action(() => localCent25Count++)),
                (2800, 0.10f, new Action(() => localCent10Count++)),
                (800, 0.05f, new Action(() => localCent5Count++))   
            };

            foreach (int size in regionSizes)
            {
                foreach (var (MinSize, Value, Increment) in coinClassification)
                {
                    if (size > MinSize)
                    {
                        Increment();             
                        totalValue += Value;     
                        totalCount++;  
                        break;                   
                    }
                }
            }

            
            peso5Count = localPeso5Count;
            peso1Count = localPeso1Count;
            cent25Count = localCent25Count;
            cent10Count = localCent10Count;
            cent5Count = localCent5Count;

        }
        private void assignTotals()
        {
            label9.Text = TotalCoins.ToString();
            label10.Text = Total5P.ToString();
            label11.Text = Total1P.ToString();
            label12.Text = Total25C.ToString();
            label13.Text = Total10C.ToString();
            label14.Text = Total5C.ToString();
            label15.Text = TotalAmount.ToString();
        }
        private void initializeTotals()
        {
            TotalCoins = 0;
            Total5P = 0;
            Total1P = 0;
            Total25C = 0;
            Total10C= 0;
            Total5C = 0;
            TotalAmount = 0;
        }
    }
}
