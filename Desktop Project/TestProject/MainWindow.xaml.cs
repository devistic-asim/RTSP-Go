using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

using FFmpeg.AutoGen;

namespace TestProject
{
    /// <summary>
    /// main window
    /// </summary>
    public partial class MainWindow : Window
    {
        //////////////////////////////////////////////////////////////////////////////////////////////////// Field
        ////////////////////////////////////////////////////////////////////////////////////////// Private

        #region Field

        /// <summary>
        /// dispatcher
        /// </summary>
        private Dispatcher dispatcher = Application.Current.Dispatcher;

        /// <summary>
        /// thread
        /// </summary>
        private Thread thread;

        /// <summary>
        /// Whether the thread is running
        /// </summary>
        private bool isThreadRunning;

        #endregion

        //////////////////////////////////////////////////////////////////////////////////////////////////// Constructor
        ////////////////////////////////////////////////////////////////////////////////////////// Public

        #region constructor - MainWindow()

        /// <summary>
        /// constructor
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();

            FFMpegHelper.Register();

            Closing               += Window_Closing;
            this.playButton.Click += playButton_Click;
        }

        #endregion

        //////////////////////////////////////////////////////////////////////////////////////////////////// Method
        ////////////////////////////////////////////////////////////////////////////////////////// Private
        //////////////////////////////////////////////////////////////////////////////// Event

        #region Handling when the window is closed - Window_Closing(sender, e)

        /// <summary>
        /// Handling when the window is closed
        /// </summary>
        /// <param name="sender">event generator</param>
        /// <param name="e">event argument</param>
        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if(this.thread.IsAlive)
            {
                this.isThreadRunning = false;

                this.thread.Join();
            }
        }

        #endregion
        #region Handling when the play button is clicked - playButton_Click(sender, e)

        /// <summary>
        /// Handling when the play button is clicked
        /// </summary>
        /// <param name="sender">event generator</param>
        /// <param name="e">event argument</param>
        private void playButton_Click(object sender, RoutedEventArgs e)
        {
            if(this.isThreadRunning)
            {
                this.isThreadRunning = false;
            }
            else
            {
                this.isThreadRunning = true;

                this.thread = new Thread(ProcessThread);

                this.thread.Start();
            }
        }

        #endregion

        //////////////////////////////////////////////////////////////////////////////// Function

        #region Setting up log callbacks - SetLogCallback()

        /// <summary>
        /// Setting up log callbacks
        /// </summary>
        private static unsafe void SetLogCallback()
        {
            ffmpeg.av_log_set_level(ffmpeg.AV_LOG_VERBOSE);

            av_log_set_callback_callback callback = (p0, level, format, vl) =>
            {
                if(level > ffmpeg.av_log_get_level())
                {
                    return;
                }

                int lineSize = 1024;

                byte* lineBuffer = stackalloc byte[lineSize];

                int printPrefix = 1;

                ffmpeg.av_log_format_line(p0, level, format, vl, lineBuffer, lineSize, &printPrefix);

                string line = Marshal.PtrToStringAnsi((IntPtr)lineBuffer);
            };

            ffmpeg.av_log_set_callback(callback);
        }

        #endregion

        #region Setting the image source - SetImageSource(bitmap)

        /// <summary>
        /// Setting the image source
        /// </summary>
        /// <param name="bitmap">bitmap</param>
        private void SetImageSource(System.Drawing.Bitmap bitmap)
        {
            this.dispatcher.BeginInvoke((Action)(() =>
            {
                using(MemoryStream memoryStream = new MemoryStream())
                {
                    if(this.thread.IsAlive)
                    {
                        bitmap.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Bmp);

                        memoryStream.Position = 0;

                        BitmapImage bitmapimage = new BitmapImage();

                        bitmapimage.BeginInit();

                        bitmapimage.CacheOption  = BitmapCacheOption.OnLoad;
                        bitmapimage.StreamSource = memoryStream;

                        bitmapimage.EndInit();

                        this.image.Source = bitmapimage;
                    }
                }
            }));
        }

        #endregion
        #region handling threads - ProcessThread()

        /// <summary>
        /// handling threads
        /// </summary>
        private unsafe void ProcessThread()
        {
            //stream path
            //string url = $"rtsp://asim:pass@192.168.43.1:1935/";
            //string url = $"rtsp://asim:pass@192.168.68.86:1935/";
            //string url = $"rtsp://wowzaec2demo.streamlock.net/vod/mp4:BigBuckBunny_115k.mp4";
            string url = $"http://assets.appcelerator.com.s3.amazonaws.com/video/media.m4v";

            using (VideoStreamDecoder decoder = new VideoStreamDecoder(url))
            {
                IReadOnlyDictionary<string, string> contextInfoDictionary = decoder.GetContextInfoDictionary();

                contextInfoDictionary.ToList().ForEach(x => Console.WriteLine($"{x.Key} = {x.Value}"));

                Size sourceSize = decoder.FrameSize;
                AVPixelFormat sourcePixelFormat = decoder.PixelFormat;
                Size targetSize = sourceSize;
                AVPixelFormat targetPixelFormat = AVPixelFormat.AV_PIX_FMT_BGR24;

                using(VideoFrameConverter converter = new VideoFrameConverter(sourceSize, sourcePixelFormat, targetSize, targetPixelFormat))
                {
                    int frameNumber = 0;

                    while(decoder.TryDecodeNextFrame(out AVFrame sourceFrame) && isThreadRunning)
                    {
                        AVFrame targetFrame = converter.Convert(sourceFrame);

                        System.Drawing.Bitmap bitmap;

                        bitmap = new System.Drawing.Bitmap
                        (
                            targetFrame.width,
                            targetFrame.height,
                            targetFrame.linesize[0],
                            System.Drawing.Imaging.PixelFormat.Format24bppRgb,
                            (IntPtr)targetFrame.data[0]
                        );

                        SetImageSource(bitmap);

                        frameNumber++;
                    }
                }
            }
        }

        #endregion
    }
}