using FFImageLoading;
using System;
using System.Timers;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Spherical360Viewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        int i = 3;
        public MainWindow()
        {
            InitializeComponent();

            inertiaTimer = new Timer(10);
            inertiaTimer.Elapsed += InertiaHandler;

            var ScreenWidth = SystemParameters.PrimaryScreenWidth;
            var ScreenHeight = SystemParameters.PrimaryScreenHeight;

            Left = Convert.ToInt32(ScreenWidth * 0.05);
            Top = Convert.ToInt32(ScreenHeight * 0.05);

            Width = Convert.ToInt32(ScreenWidth * 0.9);
            Height = Convert.ToInt32(ScreenHeight * 0.9);
            process_image();
        }

        private void process_image()
        {
            BitmapImage Image = new BitmapImage();
            var imageReady = (Action)(() =>
            {
                DownloadProgressContainer.Visibility = Visibility.Hidden;

                var ImageTransform = new TransformedBitmap();
                ImageTransform.BeginInit();
                ImageTransform.Source = Image;
                var transform = new ScaleTransform(-1, 1, 0, 0);
                ImageTransform.Transform = transform;
                ImageTransform.EndInit();
                WallImage.ImageSource = ImageTransform;
            });

            Image.BeginInit();
            string caminho = "C:\\Users\\pedro\\Pictures\\teste\\" + i.ToString() + ".jpg";
            Image.UriSource = new Uri(caminho, UriKind.Relative);
            Image.EndInit();
            if (!Image.IsDownloading)
            {
                imageReady();
            }
        }



        private Timer inertiaTimer;

        private Point PreviousMousePosition;
        private double horizontalDelta;
        private double verticalDelta;

        private void Window_TouchDown(object sender, TouchEventArgs e)
        {
            DownAction(e.GetTouchPoint(this).Position);
        }
        private void MouseDownHandler(object sender, MouseButtonEventArgs e)
        {
            DownAction(e.GetPosition(this));
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.W)
            {
                i++;
                process_image();
            }
            if (e.Key == Key.S)
            {
                i--;
                process_image();
            }


            if (e.Key == Key.Escape)
            {
               this.Close();
            }


        }


        private void DownAction(Point position)
        {
            if (Mouse.Captured == Viewer)
            {
                return;
            }
            stopInertia();
            PreviousMousePosition = position;
            Viewer.Cursor = Cursors.None;
            Mouse.Capture(Viewer);
        }

        private void stopInertia()
        {
            Viewer.Cursor = Cursors.Hand;
            inertiaTimer.Stop();
            horizontalDelta = 0;
            verticalDelta = 0;
        }

        private double relevantDelta = 0.05;
        private double relevantInertalDelta = 0.001;

        private double friction = 0.02;

        private void MouseUpHandler(object sender, MouseButtonEventArgs e)
        {
            Mouse.Capture(null);
            if ((Math.Abs(horizontalDelta) > relevantDelta) || (Math.Abs(verticalDelta) > relevantDelta))
            {
                Viewer.Cursor = Cursors.Cross;
                inertiaTimer.Start();
            }
            else
            {
                Viewer.Cursor = Cursors.Hand;
            }
        }

        private void MouseMoveHandler(object sender, MouseEventArgs e)
        {
            if (Mouse.Captured == Viewer)
            {
                var CurrentMousePosition = e.GetPosition(this);
                horizontalDelta = (PreviousMousePosition.X - CurrentMousePosition.X) / 25;
                verticalDelta = (CurrentMousePosition.Y - PreviousMousePosition.Y) / 25;
                move();
                PreviousMousePosition = CurrentMousePosition;
            }
        }

        private void MouseWheelHandler(object sender, MouseWheelEventArgs e)
        {
            var HalfPitchAngle = e.Delta / 50;
            var newFieldOfView = CameraPerspective.FieldOfView - HalfPitchAngle;
            if(newFieldOfView > 135)
            {
                newFieldOfView = 135;
            }
            else
            {
                if(newFieldOfView < 45)
                {
                    newFieldOfView = 45;
                }
            }
            CameraPerspective.FieldOfView = newFieldOfView;
        }

        private void move()
        {
            var newPanAngle = Pan.Angle + horizontalDelta;
            if (newPanAngle > 180)
            {
                newPanAngle = newPanAngle - 360;
            }
            else
            {
                if (newPanAngle < -180)
                {
                    newPanAngle = newPanAngle + 360;
                }
            }

            Pan.Angle = newPanAngle;

            var newPitchAngle = Pitch.Angle + verticalDelta;
            if (newPitchAngle > 90)
            {
                newPitchAngle = 90;
            }
            else
            {
                if (newPitchAngle < -90)
                {
                    newPitchAngle = -90;
                }
            }
            Pitch.Angle = newPitchAngle;

        }

        private void InertiaHandler(object sender, ElapsedEventArgs e)
        {
            Dispatcher.Invoke((Action)(() =>
            {
                move();
                horizontalDelta -= horizontalDelta * friction;
                verticalDelta -= verticalDelta * friction;
                if ((Math.Abs(horizontalDelta) < relevantInertalDelta) || (Math.Abs(verticalDelta) < relevantInertalDelta))
                {
                    stopInertia();
                }
            }));
        }
    }
}
