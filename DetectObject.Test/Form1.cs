using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Alturos.Yolo;

namespace DetectObject.Test
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // open
            using (OpenFileDialog ofd = new OpenFileDialog() {Filter = "PNG|*.png|JPEG|*.jpg"})
            {
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    pictureBox1.Image = Image.FromFile(ofd.FileName);
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            var configurationDetector = new ConfigurationDetector();
            var config = configurationDetector.Detect();
            using (var yoloWrapper = new YoloWrapper(config))
            {
                using (var ms = new MemoryStream())
                {
                    pictureBox1.Image.Save(ms, ImageFormat.Png);
                    var items = yoloWrapper.Detect(ms.ToArray());
                    yoloItemBindingSource.DataSource = items;

                    // draw rectangles
                    var image = pictureBox1.Image;
                    var graphic = Graphics.FromImage(image);
                    var rectangleFs = items.Select(x => new RectangleF(x.X, x.Y, x.Width, x.Height)).ToArray();
                    graphic.DrawRectangles(new Pen(Brushes.Red, 5), rectangleFs);

                    pictureBox1.Image = image;
                }
            }
        }
        

        public static byte[] ImageToByte2(Image img)
        {
            using (var stream = new MemoryStream())
            {
                img.Save(stream, ImageFormat.Png);
                return stream.ToArray();
            }
        }

        private void btnOpenVideo_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog() { Filter = "mp4|*.mp4" })
            {
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    pictureBox1.Image = Image.FromFile(ofd.FileName);
                }
            }
        }
    }
}
