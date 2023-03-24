using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Media.Imaging;
using AForge.Video;
using AForge.Video.DirectShow;
using System.Windows.Media;
using MOverlay;
using System.Collections.Generic;
using System.Windows.Threading;
using System.Threading;
using System.Windows.Input;
using System.Xml.Linq;
using System.IO;

namespace MOverlay
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private IntPtr gameWindowHandle;
        private List<VideoStreamRectangle> Elements;
        BitmapSource currFrame;
        ImageBrush testimg;
        public MainWindow()
        {
            InitializeComponent();
            Elements = CreateInitialElements();
            currFrame = Imaging.CreateBitmapSourceFromHBitmap(new Bitmap(24, 24, System.Drawing.Imaging.PixelFormat.Format24bppRgb).GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            MainCanvas.Background = System.Windows.Media.Brushes.White;
            MainCanvas.Opacity = .5;
            foreach (var element in Elements)
            {
                element.VideoStreamBrush.ImageSource = currFrame;
                MainCanvas.Children.Add(element);
            }

            

            // Get the handle of the game window
            Process[] processes = Process.GetProcessesByName("Gw2-64");
            if (processes.Length > 0)
            {
                Debug.WriteLine("Found processes: " + processes.Length);
                gameWindowHandle = processes[0].MainWindowHandle;
                // Make the window topmost
                Topmost = true;

                // Get the position and size of the game window
                RECT gameWindowRect = new RECT();
                GetWindowRect(gameWindowHandle, ref gameWindowRect);
                Width = MainCanvas.Width = gameWindowRect.Right - gameWindowRect.Left;
                Height = MainCanvas.Height = gameWindowRect.Bottom - gameWindowRect.Top;
                Left = gameWindowRect.Left;
                Top = gameWindowRect.Top;
                Debug.WriteLine("Width: " + (gameWindowRect.Right - gameWindowRect.Left));
                Debug.WriteLine("Height: " + (gameWindowRect.Bottom - gameWindowRect.Top));

                // Set the position and size of the window
                var helper = new WindowInteropHelper(this);
                SetWindowPos(helper.Handle, gameWindowHandle, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_SHOWWINDOW);

                // Capture the screen region that corresponds to the game window
                var gameWindowCapture = new ScreenCaptureStream(new Rectangle(gameWindowRect.Left, gameWindowRect.Top, gameWindowRect.Right - gameWindowRect.Left, gameWindowRect.Bottom - gameWindowRect.Top));

                // Create a video stream from the captured image
                gameWindowCapture.NewFrame += new NewFrameEventHandler(video_NewFrame);


                gameWindowCapture.Start();

            }
            else
            {
                Debug.WriteLine("Found no Processes");
            }



            
        }

        private void video_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            Debug.WriteLine("New Frame");
            try
            {
                Dispatcher.BeginInvoke(() =>
                {
                    Bitmap bitmap = (Bitmap)eventArgs.Frame.Clone();
                    BitmapSource mcurrFrame = Imaging.CreateBitmapSourceFromHBitmap(bitmap.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                    currFrame = mcurrFrame;
                    mcurrFrame.Freeze();
                    SaveImageToFile(mcurrFrame, "mybfimage.png");
                    UpdateAllElements(mcurrFrame);

                    Debug.WriteLine("Updated Frames");
                });
                
                // 
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Exception thrown in video_NewFrame: {ex.Message}");
                Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            }
            
        }

        private void UpdateAllElements(BitmapSource bms)
        {

            foreach (var child in MainCanvas.Children)
            {
                if (child is VideoStreamRectangle videoStreamRectangle)
                {
                    Debug.WriteLine("Updating ImageSource: "+ currFrame.Width+"  |  "+currFrame.Height);
                    videoStreamRectangle.VideoStreamBrush.ImageSource = bms;
                    TestImgBrush.ImageSource = bms;

                }
                MainCanvas.UpdateLayout();


            }

        }



        private List<VideoStreamRectangle> CreateInitialElements()
        {
            List<VideoStreamRectangle> elements = new List<VideoStreamRectangle>();
            elements.Add(new VideoStreamRectangle(MainCanvas, 100, 100, 50, 50));
            elements.Add(new VideoStreamRectangle(MainCanvas, 80, 80, 100, 100));
            elements.Add(new VideoStreamRectangle(MainCanvas, 60, 60, 100, 200));
            elements.Add(new VideoStreamRectangle(MainCanvas, 40, 40, 100, 400));
            elements.Add(new VideoStreamRectangle(MainCanvas, 20, 20, 300, 100));
            return elements;
        }

        private const int SWP_NOMOVE = 0x0002;
        private const int SWP_NOSIZE = 0x0001;
        private const int SWP_SHOWWINDOW = 0x0040;

        [DllImport("user32.dll")]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);


        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetWindowRect(IntPtr hWnd, ref RECT lpRect);

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }


        public static void SaveImageToFile(BitmapSource image, string filePath)
        {
            //var image = Clipboard.GetImage();
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                BitmapEncoder encoder = new PngBitmapEncoder();
                //encoder.Frames.Add(BitmapFrame.Create(image));
                encoder.Frames.Add(BitmapFrame.Create(image));
                encoder.Save(fileStream);
            }
        }

    }
}
