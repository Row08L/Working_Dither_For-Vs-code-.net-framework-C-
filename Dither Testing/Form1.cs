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
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace Dither_Testing
{
    public partial class Form1 : Form
    {
        Rectangle player;
        bool wKeyDown = false;
        bool aKeyDown = false;
        bool sKeyDown = false;
        bool dKeyDown = false;
        Pen whitePen = new Pen(Color.White, 50);
        Graphics preFilter;
        Bitmap form1Bitmap;

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
        private  Bitmap Dither( Bitmap originalImage,  double spreadValue, int numberOfColors)
        {


            spreadValue = 255 / spreadValue;

            // Downscale the original image
            int scaledWidth = (int)(originalImage.Width / 4); // Adjust the scaling factor as needed
            int scaledHeight = (int)(originalImage.Height / 4);
            Bitmap bitmap = new Bitmap(originalImage, scaledWidth, scaledHeight);
            bitmap = new Bitmap(bitmap, originalImage.Width, originalImage.Height);

            // must be the same number as the bayer array
            int dementions = 16;



            for (int y = 0; y < bitmap.Height; y++)
            {
                for (int x = 0; x < bitmap.Width; x++)
                {
                    #region dither test
                    Color bitmapColor = bitmap.GetPixel(x, y);
                    // change the bayer to 2, 4, 8, and 16
                    double threshhold = bayer16[x % dementions, y % dementions];

                    threshhold = threshhold / Math.Pow(dementions, 2) - 0.5;

                    double LimitR = Math.Floor((numberOfColors - 1) * bitmapColor.R / 255.0) * (255 / (numberOfColors - 1));
                    double LimitG = Math.Floor((numberOfColors - 1) * bitmapColor.G / 255.0) * (255 / (numberOfColors - 1));
                    double LimitB = Math.Floor((numberOfColors - 1) * bitmapColor.B / 255.0) * (255 / (numberOfColors - 1));

                    int newR = Clamp(Convert.ToInt32(LimitR * (bitmapColor.R + spreadValue * (threshhold - 0.5))), 0, 255);
                    int newG = Clamp(Convert.ToInt32(LimitG * (bitmapColor.G + spreadValue * (threshhold - 0.5))), 0, 255);
                    int newB = Clamp(Convert.ToInt32(LimitB * (bitmapColor.B + spreadValue * (threshhold - 0.5))), 0, 255);

                    bitmap.SetPixel(x, y, Color.FromArgb(newR, newG, newB));
                    #endregion
                }
            }
            // Upscale the downscaled image to whatever size you want
            Bitmap upscaledImage = new Bitmap(bitmap, originalImage.Width / 1, originalImage.Height/ 1);
            return upscaledImage;

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
            player = new Rectangle(0, 0, 1, 3);
            form1Bitmap = new Bitmap(this.Width, this.Height);
            preFilter = Graphics.FromImage(form1Bitmap);
        }

        private void  Form1_Paint(object sender, PaintEventArgs e)
        {
            //// Load an image from file
            //Image image = Properties.Resources.GTTOD;
            //
            //// Convert the image to a base64 string
            //string base64Image = ImageToBase64(image);
            //
            //// Create an XML document
            //XmlDocument xmlDocument = new XmlDocument();
            //
            //// Create the root element
            //XmlElement root = xmlDocument.CreateElement("ImageData");
            //xmlDocument.AppendChild(root);
            //
            //// Add the base64 image data as a child element
            //XmlElement imageElement = xmlDocument.CreateElement("Image");
            //imageElement.InnerText = base64Image;
            //root.AppendChild(imageElement);
            //
            //// Save the XML document to a file
            //xmlDocument.Save("C:/Users/Rowan Locke/source/repos/Dither Testing/Dither Testing/Image/imageData.xml");
            //
            //XmlDocument xmlDocument1 = new XmlDocument();
            //xmlDocument1.Load("C:/Users/Rowan Locke/source/repos/Dither Testing/Dither Testing/Image/imageData.xml");
            //
            //// Get the base64 image data from the XML
            //XmlNode imageNode = xmlDocument1.SelectSingleNode("/ImageData/Image");
            //string base64Image1 = imageNode.InnerText;
            //
            //// Convert the base64 string back to a byte array
            //byte[] imageBytes = Convert.FromBase64String(base64Image1);
            //
            //Image image2;
            //// Create a memory stream from the byte array
            //using (MemoryStream ms = new MemoryStream(imageBytes))
            //{
            //    // Create an image from the memory stream
            //    image2 = Image.FromStream(ms);
            //
            //}
            //
            //
            //
            form1Bitmap = new Bitmap(this.Width, this.Height);
            preFilter = Graphics.FromImage(form1Bitmap);
           ///preFilter.DrawImage(image2, new PointF(0, 0));
            preFilter.DrawImage(Properties.Resources.tenor_2728146430, new PointF(0, 0));

            // first veriable is the color spread nad the second one limits the amount of 8 bit colors
            Bitmap ditheredBitmap = Dither(form1Bitmap, 1.1, 50);

            // Draw the dithered bitmap onto the form
            e.Graphics.DrawImage(ditheredBitmap, new PointF(0, 0));

            string currentDirectory = AppDomain.CurrentDomain.BaseDirectory;

            string parent1 = Directory.GetParent(currentDirectory).FullName;
            string parent2 = Directory.GetParent(parent1).FullName;
            string parent3 = Directory.GetParent(parent2).FullName;

            string fullPath = Path.Combine(parent3, "Image", "image.png");

            ditheredBitmap.Save(fullPath, System.Drawing.Imaging.ImageFormat.Png);
            // Dispose of the bitmaps
            form1Bitmap.Dispose();
            ditheredBitmap.Dispose();
        }

        #region Movement Code
        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.W:
                    wKeyDown = true;
                    break;
                case Keys.S:
                    sKeyDown = true;
                    break;
                case Keys.A:
                    aKeyDown = true;
                    break;
                case Keys.D:
                    dKeyDown = true;
                    break;

            }
        }

        private void Form1_KeyUp(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.W:
                    wKeyDown = false;
                    break;
                case Keys.S:
                    sKeyDown = false;
                    break;
                case Keys.A:
                    aKeyDown = false;
                    break;
                case Keys.D:
                    dKeyDown = false;
                    break;
            }
        }
        #endregion

        private void GameTick_Tick(object sender, EventArgs e)
        {
            if (wKeyDown == true)
            {
                player.Y-= 4;
            }
            if (aKeyDown == true)
            {
                player.X-= 4;
            }
            if (sKeyDown == true)
            {
                player.Y+= 4;
            }
            if (dKeyDown == true)
            {
                player.X+= 4;
            }
            //Refresh();
        }

        static string ImageToBase64(Image image)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                // Convert the image to a byte array
                image.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                byte[] imageBytes = ms.ToArray();

                // Convert the byte array to a base64 string
                string base64String = Convert.ToBase64String(imageBytes);
                return base64String;
            }
        }
    }
}
