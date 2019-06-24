using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
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
        private readonly Stopwatch _detectStopwatch;

        public MainForm()
        {
            InitializeComponent();

            // default: tiny version 2
            var configurationDetector = new ConfigurationDetector();
            var config = configurationDetector.Detect();
            yoloWrapper = new YoloWrapper(config);

            // version 3
            //yoloWrapper = new YoloWrapper("yolov3.cfg", "yolov3.weights", "coco.names");
            _detectStopwatch = new Stopwatch();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            pictureBox1.Visible = true;
            videoSourcePlayer.Visible = false;
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

        private void btnOpenVideo_Click(object sender, EventArgs e)
        {
            pictureBox1.Visible = false;
            videoSourcePlayer.Visible = true;
            VideoCaptureDeviceForm form = new VideoCaptureDeviceForm();

            if (form.ShowDialog(this) == DialogResult.OK)
            {
                // create video source
                VideoCaptureDevice videoSource = form.VideoDevice;
                // open it
                OpenVideoSource(videoSource);
            }
        }

        private void OpenVideoSource(IVideoSource source)
        {
            // set busy cursor
            this.Cursor = Cursors.WaitCursor;

            // stop current video source
            CloseCurrentVideoSource();

            // start new video source
            videoSourcePlayer.VideoSource = source;
            videoSourcePlayer.Start();
            videoSourcePlayer.NewFrame += VideoSourcePlayer_NewFrame;

            this.Cursor = Cursors.Default;
        }

        private DateTime _flagTime = DateTime.Now;
        private RectangleF[] _previousRectangleFs;
        private const int delayTime = 4000;

        private void VideoSourcePlayer_NewFrame(object sender, ref Bitmap image)
        {
            if (DateTime.Now.Subtract(_flagTime).TotalMilliseconds < delayTime && _previousRectangleFs != null && _previousRectangleFs.Length > 0)
            {
                var graphic = Graphics.FromImage(image);
                graphic.DrawRectangles(new Pen(Brushes.Red, 5), _previousRectangleFs);

                return;
            }

            _detectStopwatch.Start();
            using (var ms = new MemoryStream())
            {
                _flagTime = DateTime.Now;
                image.Save(ms, ImageFormat.Png);
                var items = yoloWrapper.Detect(ms.ToArray());

                var graphic = Graphics.FromImage(image);
                _previousRectangleFs = items.Select(x => new RectangleF(x.X, x.Y, x.Width, x.Height)).ToArray();
                if (_previousRectangleFs.Length > 0)
                {
                    graphic.DrawRectangles(new Pen(Brushes.Red, 5), _previousRectangleFs);
                }
            }

            _detectStopwatch.Stop();
            ThreadHelper.SetText(this, lbDetectTime, $"Detect time: {_detectStopwatch.ElapsedMilliseconds}ms");
            _detectStopwatch.Reset();
        }

        // Close video source if it is running
        private void CloseCurrentVideoSource()
        {
            if (videoSourcePlayer.VideoSource != null)
            {
                videoSourcePlayer.SignalToStop();

                // wait ~ 3 seconds
                for (int i = 0; i < 30; i++)
                {
                    if (!videoSourcePlayer.IsRunning)
                        break;
                    System.Threading.Thread.Sleep(100);
                }

                if (videoSourcePlayer.IsRunning)
                {
                    videoSourcePlayer.Stop();
                }

                videoSourcePlayer.VideoSource = null;
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            CloseCurrentVideoSource();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            lbDetectTime.Text = string.Empty;
        }

        private void btnOpenVideoFile_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog() { Filter = "mp4|*.mp4" })
            {
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    FileVideoSource fileSource = new FileVideoSource(ofd.FileName);
                    fileSource.VideoSourceError += FileSource_VideoSourceError;
                    OpenVideoSource(fileSource);
                }
            }
        }

        private void FileSource_VideoSourceError(object sender, VideoSourceErrorEventArgs eventArgs)
        {
            var error = eventArgs.Description;
        }
    }
}
