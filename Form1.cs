using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CvTest
{
    using System.Diagnostics;
    using System.Drawing.Imaging;
    using System.Runtime.InteropServices;

    using Emgu.CV;
    using Emgu.CV.CvEnum;
    using Emgu.CV.OCR;
    using Emgu.CV.Structure;
    using ZXing;

    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var image = new Image<Bgr, byte>(@"E:\测试图片\2018-09-05_133455.png");
            this.pictureBox1.Image = image.ToBitmap();
            var grayimage = image.Convert<Gray, byte>();
            this.pictureBox2.Image = grayimage.ToBitmap();
            
           
            //var threshImage = grayimage.ThresholdAdaptive(new Gray(255), AdaptiveThresholdType.MeanC, ThresholdType.Binary, 9, new Gray(5));
            //CvInvoke.NamedWindow("ThresholdAdaptive");
            //CvInvoke.Imshow("ThresholdAdaptive", threshImage);
            //CvInvoke.Threshold(grayimage, threshimg, 0, 255, ThresholdType.Otsu);

            //CvInvoke.NamedWindow("Otsu");
            //CvInvoke.Imshow("Otsu", threshImage);
            //CvInvoke.Line(image, new Point(10, 10), new Point(150, 150), new MCvScalar(211,203,0));
            //CvInvoke.NamedWindow("line");
            //CvInvoke.Imshow("line", image);

            //均衡化
            Image<Gray, Byte> histimg = new Image<Gray, Byte>(grayimage.Width, grayimage.Height, new Gray(0.1));
            CvInvoke.EqualizeHist(grayimage, histimg);
            MIplImage histmi = (MIplImage)Marshal.PtrToStructure(histimg, typeof(MIplImage));
            Image<Gray, Byte> histimage = new Image<Gray, Byte>(histimg.Width, histimg.Height, histmi.WidthStep, histmi.ImageData);
            this.pictureBox5.Image = histimage.ToBitmap();

            //二值化
            Image<Gray, Byte> threshimg = new Image<Gray, Byte>(grayimage.Width, grayimage.Height);
            CvInvoke.Threshold(histimg, threshimg, 120, 255, ThresholdType.Binary);
            this.pictureBox3.Image = threshimg.ToBitmap();
            //BarcodeReader reader = new BarcodeReader();
            //reader.Options.CharacterSet = "utf-8";
            //var result = reader.Decode(threshimg.ToBitmap());
            //if (result != null)
            //{
            //    this.textBox1.Text = result.Text;
            //}


            //canny算子边缘检测
            Image<Gray, Byte> cannyimg = new Image<Gray, Byte>(grayimage.Width, grayimage.Height, new Gray(0.1));
            CvInvoke.Canny(threshimg, cannyimg, 10, 30);
            MIplImage cannymi = (MIplImage)Marshal.PtrToStructure(cannyimg, typeof(MIplImage));
            Image<Gray, Byte> cannyimage = new Image<Gray, Byte>(cannymi.Width, cannymi.Height, cannymi.WidthStep, cannymi.ImageData);
            this.pictureBox4.Image = cannyimage.ToBitmap();


            //circle画圈
            Image<Gray, Byte> circleimg = new Image<Gray, Byte>(grayimage.Width, grayimage.Height, new Gray(255));
            CvInvoke.Circle(circleimg, new Point(150, 150), 50, new MCvScalar(100, 100, 255));
            this.pictureBox6.Image = circleimg.ToBitmap();


            ////mix
            //Image<Gray, Byte> miximg = new Image<Gray, Byte>(grayimage.Width, grayimage.Height, new Gray(255));
            //Image<Gray, Byte> miximg1 = new Image<Gray, Byte>(grayimage.Width, grayimage.Height, new Gray(0.1));
            //Image<Gray, Byte> miximg2 = new Image<Gray, Byte>(grayimage.Width, grayimage.Height, new Gray(255));
            //CvInvoke.ColorChange(miximg1, miximg2, miximg);
            //this.pictureBox6.Image = img_gray(miximg.Bitmap);
        }

        /// <summary>
        /// 数字识别按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button2_Click(object sender, EventArgs e)
        {
            var sold = Convert.ToInt32(this.textBox2.Text);
            var image = new Image<Bgr, byte>("9502.jpg");
            this.pictureBox1.Image = image.ToBitmap();
            Image<Bgr, Byte> threshimg = new Image<Bgr, Byte>(image.Width, image.Height);
            CvInvoke.Threshold(image, threshimg, sold, 255, ThresholdType.Binary);
            Image<Bgr, Byte> ercodeimg = new Image<Bgr, Byte>(image.Width, image.Height);
            var element = CvInvoke.GetStructuringElement(ElementShape.Cross, new Size(1, 1), new Point(-1, -1));
            CvInvoke.Erode(threshimg, ercodeimg, element, new Point(-1, -1), 1, BorderType.Reflect, default(MCvScalar));
            this.pictureBox2.Image = ercodeimg.ToBitmap();
            Tesseract ocr = new Tesseract("", "chi_sim", OcrEngineMode.TesseractOnly);
            ocr.SetImage(ercodeimg);
            ocr.SetVariable("tessedit_char_whitelist", "01234567890");
            var result = ocr.GetUTF8Text();
            this.textBox1.Text = result;
        }

        /// <summary>
        /// 自己写的灰度化方法，用于性能比较
        /// </summary>
        /// <param name="curBitmap"></param>
        /// <returns></returns>
        public static unsafe Bitmap img_gray(Bitmap curBitmap)
        {
            int width = curBitmap.Width;
            int height = curBitmap.Height;
            Bitmap back = new Bitmap(width, height);
            byte temp;
            Rectangle rect = new Rectangle(0, 0, curBitmap.Width, curBitmap.Height);
            //这种速度最快
            BitmapData bmpData = curBitmap.LockBits(rect, ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);//24位rgb显示一个像素，即一个像素点3个字节，每个字节是BGR分量。Format32bppRgb是用4个字节表示一个像素
            byte* ptr = (byte*)(bmpData.Scan0);
            for (int j = 0; j < height; j++)
            {
                for (int i = 0; i < width; i++)
                {
                    //ptr[2]为r值，ptr[1]为g值，ptr[0]为b值
                    temp = (byte)(0.299 * ptr[2] + 0.587 * ptr[1] + 0.114 * ptr[0]);
                    back.SetPixel(i, j, Color.FromArgb(temp, temp, temp));
                    ptr += 3; //Format24bppRgb格式每个像素占3字节
                }
                ptr += bmpData.Stride - bmpData.Width * 3;//每行读取到最后“有用”数据时，跳过未使用空间XX
            }
            curBitmap.UnlockBits(bmpData);
            return back;
        }
    }
}
