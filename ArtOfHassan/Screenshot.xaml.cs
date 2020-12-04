using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace ArtOfHassan
{
    /// <summary>
    /// Screenshot.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Screenshot : Window
    {
        public System.Drawing.Bitmap CurrentBitmap;

        public Screenshot()
        {
            InitializeComponent();
        }

        private void MainImage_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Point ClickPos = e.GetPosition((IInputElement)sender);

            int ClickX = (int)ClickPos.X;
            int ClickY = (int)ClickPos.Y;

            System.Windows.Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
            {
                ((MainWindow)System.Windows.Application.Current.MainWindow).PixelPositionX = ClickX;
                ((MainWindow)System.Windows.Application.Current.MainWindow).PixelPositionY = ClickY;
                ((MainWindow)System.Windows.Application.Current.MainWindow).PixelColor  = CurrentBitmap.GetPixel(ClickX, ClickY);
            }));

            this.Close();
        }

        private void MainImage_PreviewMouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            this.Close();
        }

        private void MainImage_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            System.Windows.Point ClickPos = e.GetPosition((IInputElement)sender);

            int ClickX = (int)ClickPos.X;
            int ClickY = (int)ClickPos.Y;

            if (ClickX < 140)
            {
                if (ClickY < 70)
                {
                    PosColorNW.Visibility = Visibility.Hidden;
                    PosColorNE.Visibility = Visibility.Hidden;
                    PosColorSW.Visibility = Visibility.Hidden;
                    PosColorSE.Visibility = Visibility.Visible;

                    PosColorNWout.Visibility = Visibility.Hidden;
                    PosColorNEout.Visibility = Visibility.Hidden;
                    PosColorSWout.Visibility = Visibility.Hidden;
                    PosColorSEout.Visibility = Visibility.Visible;

                    PosColorSE.Text = $"X: {ClickX}, Y: {ClickY}\nColor: {ColorTranslator.ToHtml(CurrentBitmap.GetPixel(ClickX, ClickY))}";
                }
                //else
                //{
                //    PosColorNW.Visibility = Visibility.Hidden;
                //    PosColorNE.Visibility = Visibility.Visible;
                //    PosColorSW.Visibility = Visibility.Hidden;
                //    PosColorSE.Visibility = Visibility.Hidden;

                //    PosColorNWout.Visibility = Visibility.Hidden;
                //    PosColorNEout.Visibility = Visibility.Visible;
                //    PosColorSWout.Visibility = Visibility.Hidden;
                //    PosColorSEout.Visibility = Visibility.Hidden;

                //    PosColorNE.Text = $"X: {ClickX}, Y: {ClickY}\nColor: {ColorTranslator.ToHtml(CurrentBitmap.GetPixel(ClickX, ClickY))}";
                //}
                else
                {
                    PosColorNW.Visibility = Visibility.Visible;
                    PosColorNE.Visibility = Visibility.Hidden;
                    PosColorSW.Visibility = Visibility.Hidden;
                    PosColorSE.Visibility = Visibility.Hidden;

                    PosColorNWout.Visibility = Visibility.Visible;
                    PosColorNEout.Visibility = Visibility.Hidden;
                    PosColorSWout.Visibility = Visibility.Hidden;
                    PosColorSEout.Visibility = Visibility.Hidden;

                    PosColorNW.Text = $"X: {ClickX}, Y: {ClickY}\nColor: {ColorTranslator.ToHtml(CurrentBitmap.GetPixel(ClickX, ClickY))}";
                }
            }
            else
            {
                //if (ClickY < this.Height / 2)
                //{
                //    PosColorNW.Visibility = Visibility.Hidden;
                //    PosColorNE.Visibility = Visibility.Hidden;
                //    PosColorSW.Visibility = Visibility.Visible;
                //    PosColorSE.Visibility = Visibility.Hidden;

                //    PosColorNWout.Visibility = Visibility.Hidden;
                //    PosColorNEout.Visibility = Visibility.Hidden;
                //    PosColorSWout.Visibility = Visibility.Visible;
                //    PosColorSEout.Visibility = Visibility.Hidden;

                //    PosColorSW.Text = $"X: {ClickX}, Y: {ClickY}\nColor: {ColorTranslator.ToHtml(CurrentBitmap.GetPixel(ClickX, ClickY))}";
                //}
                //else
                {
                    PosColorNW.Visibility = Visibility.Visible;
                    PosColorNE.Visibility = Visibility.Hidden;
                    PosColorSW.Visibility = Visibility.Hidden;
                    PosColorSE.Visibility = Visibility.Hidden;

                    PosColorNWout.Visibility = Visibility.Visible;
                    PosColorNEout.Visibility = Visibility.Hidden;
                    PosColorSWout.Visibility = Visibility.Hidden;
                    PosColorSEout.Visibility = Visibility.Hidden;

                    PosColorNW.Text = $"X: {ClickX}, Y: {ClickY}\nColor: {ColorTranslator.ToHtml(CurrentBitmap.GetPixel(ClickX, ClickY))}";
                }
            }
        }
    }
}
