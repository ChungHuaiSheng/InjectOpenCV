using CenterSpace.NMath.Analysis;
using CenterSpace.NMath.Core;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace InjectOpenCV
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
        public void imageProcess(string path)
        {
            //string fp = @"D:\TEM\03H\03H\TempImage026.png";
            string fp = path;
            var imgO = Cv2.ImRead(fp, ImreadModes.Color);
            var imgGray = Cv2.ImRead(fp, ImreadModes.Grayscale);
            //Cut Unstable image boundary
            Rect rect = new Rect(2, 2, imgO.Width - 4, imgO.Height - 4);
            imgO = imgO.Clone(rect);
            imgGray = imgGray.Clone(rect);

            //threshold
            Mat imgBinary = new Mat();
            Cv2.Threshold(imgGray, imgBinary, 60, 255, ThresholdTypes.BinaryInv);

            //DeNoise
            Mat imgOpen = new Mat();
            var kernal_Cross3 = Cv2.GetStructuringElement(MorphShapes.Cross, new OpenCvSharp.Size(3, 3));
            Cv2.MorphologyEx(imgBinary, imgOpen, MorphTypes.Open, kernal_Cross3);

            //Find all Particle
            Mat labels = new Mat();
            Mat stats = new Mat();
            Mat centroids = new Mat();
            var lableCnt = Cv2.ConnectedComponentsWithStats(imgOpen, labels, stats, centroids);

            //Find Hole: area > 10% total image
            double darkAreaSpec = imgOpen.Width * imgOpen.Height * 0.5 / 100;
            Bitmap imgOpenBitmap = BitmapConverter.ToBitmap(imgOpen);
            Mat mask = new Mat(imgOpen.Size(), MatType.CV_8UC1);
            //Mat imgAreaFilter = stats.Col(4) > darkAreaSpec;

            List<int> holeLable = new List<int>();
            Mat imgth = new Mat();
            for (int i = 1; i < lableCnt; i++)
            {
                var left = stats.At<int>(i, (int)ConnectedComponentsTypes.Left);
                var top = stats.At<int>(i, (int)ConnectedComponentsTypes.Top);
                var width = stats.At<int>(i, (int)ConnectedComponentsTypes.Width);
                var height = stats.At<int>(i, (int)ConnectedComponentsTypes.Height);
                var area = stats.At<int>(i, (int)ConnectedComponentsTypes.Area);
                //Hole Area > spec
                if (area > (int)darkAreaSpec)
                {

                    //imgO.Rectangle(new OpenCvSharp.Point(left, top), new OpenCvSharp.Point(left + width, top + height), Scalar.Red, 2);
                    holeLable.Add(i);
                    //Find Contour by different ConnectedComponent
                    imgth = selectLabelImg(labels, i);
                    imgth.ConvertTo(imgth, MatType.CV_8UC1);
                    var ConPoint = imgth.FindContoursAsArray(RetrievalModes.External, ContourApproximationModes.ApproxNone);
                    //Query the Hole Boundary
                    var query = from p in ConPoint[0]
                                where p.X != 0 && p.X != imgth.Width - 1 && p.Y != 0 && p.Y != imgth.Height - 1
                                select p;

                    List<OpenCvSharp.Point> qp = new List<OpenCvSharp.Point>();
                    foreach (var p in query)
                    {
                        qp.Add(p);
                    }
                    OpenCvSharp.Point[][] qPoint = new OpenCvSharp.Point[][] { qp.ToArray() };
                    //Check Hole Connect the Boundary
                    if (query.Count() > ConPoint[0].Length * 0.9)
                    {
                        continue;
                    }
                    //Contour moment
                    var ConM = Cv2.Moments(ConPoint[0]);
                    int cX = (int)(ConM.M10 / ConM.M00);
                    int cY = (int)(ConM.M01 / ConM.M00);
                    //Circle Fit
                    var rtn = circleFit(qp.ToArray(), new OpenCvSharp.Point() { X = cX, Y = cY });
                    //Check Hole and FitCircle in the same side
                    if (Math.Pow((Math.Pow(rtn[0] - cX, 2) + Math.Pow(rtn[1] - cY, 2)), 0.5) < rtn[2])
                    {
                        imgO.Circle((int)rtn[0], (int)rtn[1], (int)rtn[2], Scalar.Red, 2);
                    }
                }
            }

            //TODO Create Mask Image

            imgth.ConvertTo(imgth, MatType.CV_8U);
            var imgbit = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(imgO);

            pictureBox1.Image = imgbit;
        }

        private Mat selectLabelImg(Mat img, int label)
        {
            if (img.Type().ToString() != MatType.CV_64F.ToString())
            {
                img.ConvertTo(img, MatType.CV_64F);
            }
            Mat removeHigher = img.Threshold(label, 255, ThresholdTypes.TozeroInv);
            Mat removeLower = removeHigher.Threshold(label - 1, 255, ThresholdTypes.Binary);
            return removeLower;
        }

        private double[] circleFit(OpenCvSharp.Point[] p, OpenCvSharp.Point startp)
        {
            int n = p.Length;
            DoubleVector x = new DoubleVector(n);
            DoubleVector y = new DoubleVector(n);

            for (int i = 0; i < p.Length; i++)
            {
                x[i] = p[i].X;
                y[i] = p[i].Y;
            }

            CircleFitFunction f = new CircleFitFunction(x, y);
            TrustRegionMinimizer minimizer = new TrustRegionMinimizer();
            DoubleVector start = new DoubleVector(Convert.ToString(startp.X) + " " + Convert.ToString(startp.Y) + " 500");
            DoubleVector solution = minimizer.Minimize(f, start);

            return solution.Append(minimizer.FinalResidual).ToArray<double>();

        }

        private void pictureBox1_DoubleClick(object sender, EventArgs e)
        {
            openFileDialog1 = new OpenFileDialog()
            {
                FileName = "Select a Png file",
                Filter = "Image File (*.Png)|*.Png",
                Title = "Open Image File",
                InitialDirectory = @"D:\TEM\03H\03H"
            };

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                //Get the path of specified file
                string filePath = openFileDialog1.FileName;
                imageProcess(filePath);
            }
        }
    }


}
