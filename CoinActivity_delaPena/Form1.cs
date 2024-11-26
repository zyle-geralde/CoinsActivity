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
            AnalyzeCoinsPixels(processed,ref TotalCoins,ref TotalAmount,ref Total5P,ref Total1P,ref Total25C,ref Total10C,ref Total5C);
            assignTotals();
        }

        private void BlackAndWhiteBinaryConversion()
        {
            if (loaded == null)
                return;
            Bitmap bImage = new Bitmap(loaded.Width,loaded.Height);
            int thres = 180;
            for (int x= 0; x<loaded.Width; ++x)
            {
                for (int y =0; y< loaded.Height; ++y)
                {
                    Color pixel = loaded.GetPixel(x, y);
                    int grayscale = (pixel.R +pixel.G +pixel.B)/3;
                    bImage.SetPixel(x,y,grayscale <thres?Color.Black:Color.White);
                }
            }
            processed = bImage;
            pictureBox2.Image = processed;
        }

        public static void AnalyzeCoinsPixels(Bitmap img,ref int totalCount,ref float totalAmount,ref int p5Count,ref int p1Count,ref int c25Count,ref int c10Count,ref int c5Count)
        {
            bool[,] seen= VisitedMatrix(img.Width,img.Height);
            List<int>regionSizes=IdentifyRegions(img,seen);

            ProcessRegions(regionSizes,ref totalCount,ref totalAmount, ref p5Count,ref p1Count,ref c25Count,ref c10Count,ref c5Count);


        }
        private static bool[,] VisitedMatrix(int w,int h)
        {
            return new bool[w,h];
        }

        private static List<int> IdentifyRegions(Bitmap img, bool[,] visited)
        {
            List<int> regSizes = new List<int>();

            for (int x=0;x<img.Width;x++)
            {
                for (int y =0; y<img.Height; y++)
                {
                    if (visited[x,y] ||img.GetPixel(x,y).R != 0)
                        continue;

                    int cregSize = CalculateRegSize(img, x,y,visited);
                    if (cregSize >= 800)
                    {
                        regSizes.Add(cregSize);
                    }
                }
            }
            return regSizes;
        }

        private static int CalculateRegSize(Bitmap img, int strtX, int strtY, bool[,] visited)
        {
            Stack<(int x,int y)> stack =new Stack<(int x,int y)>();
            stack.Push((strtX, strtY));
            int regSize = 0;

            while (stack.Count >0)
            {
                var (x, y) = stack.Pop();
                if (x <0 ||y<0||x>=img.Width||y >= img.Height||visited[x,y])
                    continue;
                visited[x,y] = true;
                if (img.GetPixel(x, y).R != 0)
                    continue;

                regSize++;
                stack.Push((x - 1, y));
                stack.Push((x + 1, y));
                stack.Push((x, y - 1));
                stack.Push((x, y + 1));
            }
            return regSize;
        }

        private static void ProcessRegions(List<int> regSizes,ref int totalCount,ref float totalAmount,ref int p5Count,ref int p1Count,ref int c25Count,ref int c10Count,ref int c5Count)
        {
            int localPeso5Count = p5Count;
            int localPeso1Count = p1Count;
            int localCent25Count = c25Count;
            int localCent10Count = c10Count;
            int localCent5Count = c5Count;

            
            var coinClassification = new List<(int minsize,float value,Action)>
            {
                (6800,5.0f,new Action(()=>localPeso5Count++)),   
                (4800,1.0f,new Action(()=>localPeso1Count++)),  
                (3800,0.25f,new Action(()=>localCent25Count++)),
                (2800,0.10f,new Action(()=>localCent10Count++)),
                (800,0.05f,new Action(()=>localCent5Count++))   
            };

            foreach (int size in regSizes)
            {
                foreach (var(minsize,value,Increment) in coinClassification)
                {
                    if (size >minsize)
                    {
                        Increment();             
                        totalAmount+= value;     
                        totalCount++;  
                        break;                   
                    }
                }
            }
            
            p5Count = localPeso5Count;
            p1Count = localPeso1Count;
            c25Count = localCent25Count;
            c10Count = localCent10Count;
            c5Count = localCent5Count;

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
