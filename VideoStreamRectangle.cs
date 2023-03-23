using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace MOverlay
{
    public class VideoStreamRectangle : ContentControl
    {
        static VideoStreamRectangle()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(VideoStreamRectangle), new FrameworkPropertyMetadata(typeof(VideoStreamRectangle)));
        }
        private System.Windows.Point _lastMousePosition;
        private Canvas canvas;

        public VideoStreamRectangle(Canvas canvas, int X, int Y, int W, int H)
        {
            this.canvas = canvas;
            Width = W;
            Height = H;
            MouseDown += OnVideoStreamRectangleMouseDown;
            Opacity = 1;
            this.VideoStreamBrush = new ImageBrush() { Viewport = new Rect(X, Y, W, H) };
        }

        public static readonly DependencyProperty VideoStreamBrushProperty = DependencyProperty.Register(
            "VideoStreamBrush", typeof(ImageBrush), typeof(VideoStreamRectangle), new PropertyMetadata(null));

        public ImageBrush VideoStreamBrush
        {
            get { return (ImageBrush)GetValue(VideoStreamBrushProperty); }
            set { SetValue(VideoStreamBrushProperty, value); }
        }

        public void SetPosition(int X, int Y)
        {
            this.VideoStreamBrush.Viewport = new Rect(X, Y, this.Width, this.Height);
        }

        private void OnVideoStreamRectangleMouseDown(object sender, MouseButtonEventArgs e)
        {
            // Save the current mouse position
            _lastMousePosition = e.GetPosition(canvas);

            // Add a mouse move event handler
            canvas.MouseMove += OnVideoStreamRectangleMouseMove;

            // Add a mouse up event handler
            canvas.MouseUp += OnVideoStreamRectangleMouseUp;

            // Set the mouse capture to the canvas
            canvas.CaptureMouse();
        }

        private void OnVideoStreamRectangleMouseMove(object sender, MouseEventArgs e)
        {
            // Calculate the distance moved by the mouse
            var currentMousePosition = e.GetPosition(canvas);
            var deltaX = currentMousePosition.X - _lastMousePosition.X;
            var deltaY = currentMousePosition.Y - _lastMousePosition.Y;

            // Get the VideoStreamRectangle that raised the event
            var videoStreamRectangle = (VideoStreamRectangle)sender;

            // Move the VideoStreamRectangle by the distance moved by the mouse
            Canvas.SetLeft(videoStreamRectangle, Canvas.GetLeft(videoStreamRectangle) + deltaX);
            Canvas.SetTop(videoStreamRectangle, Canvas.GetTop(videoStreamRectangle) + deltaY);
        }
        private void OnVideoStreamRectangleMouseUp(object sender, MouseEventArgs e)
        {
            // Reset the last mouse position
            _lastMousePosition = new System.Windows.Point();
        }
    }
}
