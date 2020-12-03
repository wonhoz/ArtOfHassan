using System;
using System.Collections.Generic;
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
            Point ClickPos = e.GetPosition((IInputElement)sender);

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
    }
}
