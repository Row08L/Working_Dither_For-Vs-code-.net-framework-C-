using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using System.Drawing.Imaging;

namespace Dither_Testing
{
    public partial class Form1 : Form
    {
        int[,] bayer2 = new int[2, 2]
        {
            {0, 2},
            {3, 1}
        };

        int[,] bayer4 = new int[4, 4]
        {
            {0, 8, 2, 10},
            {12, 4, 14, 6},
            {3, 11, 1, 9 },
            {15, 7, 13, 5},
        };

        int[,] bayer8 = {
    {  0, 48, 12, 60,  3, 51, 15, 63 },
    { 32, 16, 44, 28, 35, 19, 47, 31 },
    {  8, 56,  4, 52, 11, 59,  7, 55 },
    { 40, 24, 36, 20, 43, 27, 39, 23 },
    {  2, 50, 14, 62,  1, 49, 13, 61 },
    { 34, 18, 46, 30, 33, 17, 45, 29 },
    { 10, 58,  6, 54,  9, 57,  5, 53 },
    { 42, 26, 38, 22, 41, 25, 37, 21 }
};

        int[,] bayer16 = {
    {  0, 192,  48, 240,  12, 204,  60, 252,   3, 195,  51, 243,  15, 207,  63, 255 },
    { 128,  64, 176, 112, 140,  76, 188, 124, 131,  67, 179, 115, 143,  79, 191, 127 },
    {  32, 224,  16, 208,  44, 236,  28, 220,  35, 227,  19, 211,  47, 239,  31, 223 },
    { 160,  96, 144,  80, 172, 108, 156,  92, 163,  99, 147,  83, 175, 111, 159,  95 },
    {   8, 200,  56, 248,   4, 196,  52, 244,  11, 203,  59, 251,   7, 199,  55, 247 },
    { 136,  72, 184, 120, 132,  68, 180, 116, 139,  75, 187, 123, 135,  71, 183, 119 },
    {  40, 232,  24, 216,  36, 228,  20, 212,  43, 235,  27, 219,  39, 231,  23, 215 },
    { 168, 104, 152,  88, 164, 100, 148,  84, 171, 107, 155,  91, 167, 103, 151,  87 },
    {   2, 194,  50, 242,  14, 206,  62, 254,   1, 193,  49, 241,  13, 205,  61, 253 },
    { 130,  66, 178, 114, 142,  78, 190, 126, 129,  65, 177, 113, 141,  77, 189, 125 },
    {  34, 226,  18, 210,  46, 238,  30, 222,  33, 225,  17, 209,  45, 237,  29, 221 },
    { 162,  98, 146,  82, 174, 110, 158,  94, 161,  97, 145,  81, 173, 109, 157,  93 },
    {  10, 202,  58, 250,   6, 198,  54, 246,   9, 201,  57, 249,   5, 197,  53, 245 },
    { 138,  74, 186, 122, 134,  70, 182, 118, 137,  73, 185, 121, 133,  69, 181, 117 },
    {  42, 234,  26, 218,  38, 230,  22, 214,  41, 233,  25, 217,  37, 229,  21, 213 },
    { 170, 106, 154,  90, 166, 102, 150,  86, 169, 105, 153,  89, 165, 101, 149,  85 }
};

        int[,] bayer3 = {
    { 0, 7, 3 },
    { 6, 5, 2 },
    { 4, 1, 8 }
};

        #region Dithering Test
        private Bitmap Dither(PaintEventArgs e, double spreadValue)
        {
            Bitmap bitmap = Properties.Resources.Capture;
            int width = bitmap.Width;
            int height = bitmap.Height;
            int dementions = 8;
            int n = 100;
            Color[] neonPalet = new Color[5] { Color.Black, Color.White, Color.Red, Color.Green, Color.Blue };

            // Draw the original image onto the lower quality image with stretching
            e.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.Low;


            for (int y = 0; y < bitmap.Height; y++)
            {
                for (int x = 0; x < bitmap.Width; x++)
                {
                    #region dither test
                    Color bitmapColor = bitmap.GetPixel(x, y);
                    int rX = x % dementions;
                    int rY = y % dementions;
                    double offset = dementions * (dementions / 2) - 0.5;

                    double threshhold = bayer8[rX, rY];

                    threshhold = threshhold * (1 / Math.Pow(dementions, 2)) - 0.5;
                    Color nearestColor = FindNearestColor(bitmapColor, neonPalet);



                    
                    int newLimitR = Convert.ToInt32((Math.Floor((n - 1) * bitmapColor.R / 255.0) * (255 / (n - 1))));
                    int newLimitG = Convert.ToInt32((Math.Floor((n - 1) * bitmapColor.G / 255.0) * (255 / (n - 1))));
                    int newLimitB = Convert.ToInt32((Math.Floor((n - 1) * bitmapColor.B / 255.0) * (255 / (n - 1))));

                    int test1 = Clamp(Convert.ToInt32(newLimitR * (bitmapColor.R+ spreadValue * (threshhold - 0.5))), 0, 255);
                    int test2 = Clamp(Convert.ToInt32(newLimitG * (bitmapColor.G+ spreadValue * (threshhold - 0.5))), 0, 255);
                    int test3 = Clamp(Convert.ToInt32(newLimitB * (bitmapColor.B+ spreadValue * (threshhold - 0.5))), 0, 255);



                    bitmap.SetPixel(x, y, Color.FromArgb(test1, test2, test3));
                    #endregion



                }
            }

            return bitmap;
        }



        static Color FindNearestColor(Color desiredColor, Color[] palette)
        {
            Color nearestColor = palette[0];
            double minDistance = ColorDistance(desiredColor, palette[0]);

            foreach (Color color in palette)
            {
                double distance = ColorDistance(desiredColor, color);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearestColor = color;
                }
            }

            return nearestColor;
        }

        static double ColorDistance(Color color1, Color color2)
        {
            int rDiff = color1.R - color2.R;
            int gDiff = color1.G - color2.G;
            int bDiff = color1.B - color2.B;
            return Math.Sqrt(rDiff * rDiff + gDiff * gDiff + bDiff * bDiff);
        }

        public static Color AddNoiseToPixel(Color originalColor, double noiseValue)
        {
            // Get the individual components of the original color
            int red = originalColor.R;
            int green = originalColor.G;
            int blue = originalColor.B;

            // Scale and add noise to each color component
            red = Clamp((int)(red + noiseValue * 255), 0, 255);
            green = Clamp((int)(green + noiseValue * 255), 0, 255);
            blue = Clamp((int)(blue + noiseValue * 255), 0, 255);

            // Create and return the new color with noise added
            return Color.FromArgb(red, green, blue);
        }

        // Clamp "Clamps" a value between two others
        // if input value is outside of clamp range values it will be turned into the closest clamping value
        private static int Clamp(double value, int min, int max)
        {
            return Convert.ToInt32(Math.Max(min, Math.Min(max, value)));
        }

        #endregion
        public Form1()
        {
            InitializeComponent();
            Refresh();
        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.DrawImage(Dither(e, 255 / 0.99), new PointF(0,0));
        }
    }
}
