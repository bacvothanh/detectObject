using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using AForge.Video;
using AForge.Video.DirectShow;
using Alturos.Yolo;

namespace DetectObject.Test
{
    public partial class MainForm : Form
    {
        private readonly YoloWrapper yoloWrapper;

        public MainForm()
        {
            InitializeComponent();

            // default: tiny version 2
            //var configurationDetector = new ConfigurationDetector();
            //var config = configurationDetector.Detect();
            //yoloWrapper = new YoloWrapper(config);

            // version 3
            yoloWrapper = new YoloWrapper("yolov3-tiny.cfg", "yolov3-tiny_9000.weights", "yolo.names");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            pictureBox1.Visible = true;
            videoSourcePlayer.Visible = false;
            // open
            using (OpenFileDialog ofd = new OpenFileDialog(){Filter = "image files (*.jpg, *.jpeg, *.png) | *.jpg; *.jpeg; *.png" })
            {
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    pictureBox1.Image = Image.FromFile(ofd.FileName);
                    DetectAndDraw(ofd.FileName);
                }
            }
        }
        
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            lbDetectTime.Text = string.Empty;
            lbCounting.Text = string.Empty;
        }

        private void DetectAndDraw(string path)
        {
            var items = yoloWrapper.Detect(path);
            yoloItemBindingSource.DataSource = items;
            if (items != null && items.Any())
            {
                // draw rectangles
                var image = pictureBox1.Image;
                var graphic = Graphics.FromImage(image);
                var rectangleFs = items.Select(x => new RectangleF(x.X, x.Y, x.Width, x.Height)).ToArray();
                graphic.DrawRectangles(new Pen(Brushes.Red, 5), rectangleFs);

                pictureBox1.Image = image;
                ThreadHelper.SetText(this, lbDetectTime, $"{items.Count()} result");
            }
            else
            {
                ThreadHelper.SetText(this, lbDetectTime, $"No result");
            }
        }
        
    }
}
