using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
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
    /// SettingWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class SettingWindow : Window
    {
        [DllImport("user32.dll")]
        static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint dwData, int dwExtraInfo);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern IntPtr FindWindow(string strClassName, string strWindowName);

        [DllImport("user32.dll")]
        internal static extern bool GetWindowPlacement(IntPtr handle, ref WINDOWPLACEMENT placement);

        private const uint MOUSEMOVE = 0x0001;  // 마우스 이동
        private const uint ABSOLUTEMOVE = 0x8000;  // 전역 위치
        private const uint LBUTTONDOWN = 0x0002;  // 왼쪽 마우스 버튼 눌림
        private const uint LBUTTONUP = 0x0004;  // 왼쪽 마우스 버튼 떼어짐
        private const uint RBUTTONDOWN = 0x0008;  // 오른쪽 마우스 버튼 눌림
        private const uint RBUTTONUP = 0x00010; // 오른쪽 마우스 버튼 떼어짐

        double NoxPointX = 0;
        double NoxPointY = 0;
        double NoxWidth  = 0;
        double NoxHeight = 0;


        public SettingWindow()
        {
            InitializeComponent();
        }

        private IntPtr GetWinAscHandle()
        {
            string windowTitle = "NoxPlayer";
            System.Windows.Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
            {
                windowTitle = ((MainWindow)System.Windows.Application.Current.MainWindow).WindowTitleTextBox.Text;
            }));
            return FindWindow(null, windowTitle);
        }

        private void GetWindowPos(IntPtr hwnd, ref System.Windows.Point point, ref System.Windows.Size size)
        {
            WINDOWPLACEMENT placement = new WINDOWPLACEMENT();
            placement.length = System.Runtime.InteropServices.Marshal.SizeOf(placement);

            GetWindowPlacement(hwnd, ref placement);

            size = new System.Windows.Size(placement.normal_position.Right - (placement.normal_position.Left * 2), placement.normal_position.Bottom - (placement.normal_position.Top * 2));
            point = new System.Windows.Point(placement.normal_position.Left, placement.normal_position.Top);
        }

        private void GetPixelPositionAndColor()
        {
            System.Windows.Point point = new System.Windows.Point();
            System.Windows.Size size = new System.Windows.Size();

            GetWindowPos(GetWinAscHandle(), ref point, ref size);

            if ((size.Width != 0) && (size.Height != 0))
            {
                NoxPointX = point.X;
                NoxPointY = point.Y;
                NoxWidth = size.Width;
                NoxHeight = size.Height;

                // 화면 크기만큼의 Bitmap 생성
                System.Drawing.Bitmap CurrentBitmap = new System.Drawing.Bitmap((int)NoxWidth, (int)NoxHeight, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                // Bitmap 이미지 변경을 위해 Graphics 객체 생성
                using (System.Drawing.Graphics graphics = System.Drawing.Graphics.FromImage(CurrentBitmap))
                {
                    // 화면을 그대로 카피해서 Bitmap 메모리에 저장
                    graphics.CopyFromScreen((int)NoxPointX, (int)NoxPointY, 0, 0, CurrentBitmap.Size);
                    // Bitmap 데이타를 파일로 저장
                    //CurrentBitmap.Save("screenshot.png", System.Drawing.Imaging.ImageFormat.Png);
                }

                using (MemoryStream memory = new MemoryStream())
                {
                    CurrentBitmap.Save(memory, ImageFormat.Png);
                    memory.Position = 0;
                    BitmapImage bitmapImage = new BitmapImage();
                    bitmapImage.BeginInit();
                    bitmapImage.StreamSource = memory;
                    bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                    bitmapImage.EndInit();

                    Screenshot screenshot = new Screenshot();
                    screenshot.Width = NoxWidth;
                    screenshot.Height = NoxHeight;
                    screenshot.MainImage.Source = bitmapImage;
                    screenshot.CurrentBitmap = CurrentBitmap;
                    screenshot.ShowDialog();
                }
            }
        }

        private void AppLocationButton1_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
            {
                ((MainWindow)System.Windows.Application.Current.MainWindow).PixelPositionX = int.Parse(AppLocationX.Text);
                ((MainWindow)System.Windows.Application.Current.MainWindow).PixelPositionY = int.Parse(AppLocationY.Text);
                ((MainWindow)System.Windows.Application.Current.MainWindow).PixelColor = ColorTranslator.FromHtml(AppLocationColor.Text.Split(';')[0]);
            }));

            GetPixelPositionAndColor();

            System.Windows.Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
            {
                AppLocationX.Text = ((MainWindow)System.Windows.Application.Current.MainWindow).PixelPositionX.ToString();
                AppLocationY.Text = ((MainWindow)System.Windows.Application.Current.MainWindow).PixelPositionY.ToString();
                AppLocationColor.Text = ColorTranslator.ToHtml(((MainWindow)System.Windows.Application.Current.MainWindow).PixelColor) + ";" + AppLocationColor.Text.Split(';')[1];
                AppLocationButton1.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(ColorTranslator.ToHtml(((MainWindow)System.Windows.Application.Current.MainWindow).PixelColor)));
            }));
        }

        private void ShopButton1_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
            {
                ((MainWindow)System.Windows.Application.Current.MainWindow).PixelPositionX = int.Parse(ShopButtonX.Text);
                ((MainWindow)System.Windows.Application.Current.MainWindow).PixelPositionY = int.Parse(ShopButtonY.Text);
                ((MainWindow)System.Windows.Application.Current.MainWindow).PixelColor = ColorTranslator.FromHtml(ShopButtonColor.Text);
            }));

            GetPixelPositionAndColor();

            System.Windows.Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
            {
                ShopButtonX.Text = ((MainWindow)System.Windows.Application.Current.MainWindow).PixelPositionX.ToString();
                ShopButtonY.Text = ((MainWindow)System.Windows.Application.Current.MainWindow).PixelPositionY.ToString();
                ShopButtonColor.Text = ColorTranslator.ToHtml(((MainWindow)System.Windows.Application.Current.MainWindow).PixelColor);
                ShopButton1.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(ColorTranslator.ToHtml(((MainWindow)System.Windows.Application.Current.MainWindow).PixelColor)));
            }));
        }

        private void MiddleButton1_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
            {
                ((MainWindow)System.Windows.Application.Current.MainWindow).PixelPositionX = int.Parse(MiddleButtonX.Text);
                ((MainWindow)System.Windows.Application.Current.MainWindow).PixelPositionY = int.Parse(MiddleButtonY.Text);
                ((MainWindow)System.Windows.Application.Current.MainWindow).PixelColor = ColorTranslator.FromHtml(MiddleButtonColor.Text);
            }));

            GetPixelPositionAndColor();

            System.Windows.Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
            {
                MiddleButtonX.Text = ((MainWindow)System.Windows.Application.Current.MainWindow).PixelPositionX.ToString();
                MiddleButtonY.Text = ((MainWindow)System.Windows.Application.Current.MainWindow).PixelPositionY.ToString();
                MiddleButtonColor.Text = ColorTranslator.ToHtml(((MainWindow)System.Windows.Application.Current.MainWindow).PixelColor);
                MiddleButton1.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(ColorTranslator.ToHtml(((MainWindow)System.Windows.Application.Current.MainWindow).PixelColor)));
            }));
        }

        private void GoldChestBox1_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
            {
                ((MainWindow)System.Windows.Application.Current.MainWindow).PixelPositionX = int.Parse(GoldChestBoxX.Text);
                ((MainWindow)System.Windows.Application.Current.MainWindow).PixelPositionY = int.Parse(GoldChestBoxY.Text);
                ((MainWindow)System.Windows.Application.Current.MainWindow).PixelColor = ColorTranslator.FromHtml(GoldChestBoxColor.Text.Split(';')[0]);
            }));

            GetPixelPositionAndColor();

            System.Windows.Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
            {
                GoldChestBoxX.Text = ((MainWindow)System.Windows.Application.Current.MainWindow).PixelPositionX.ToString();
                GoldChestBoxY.Text = ((MainWindow)System.Windows.Application.Current.MainWindow).PixelPositionY.ToString();
                GoldChestBoxColor.Text = ColorTranslator.ToHtml(((MainWindow)System.Windows.Application.Current.MainWindow).PixelColor) + ";" + GoldChestBoxColor.Text.Split(';')[1];
                GoldChestBox1.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(ColorTranslator.ToHtml(((MainWindow)System.Windows.Application.Current.MainWindow).PixelColor)));
            }));
        }

        private void BattleLevelButton1_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
            {
                ((MainWindow)System.Windows.Application.Current.MainWindow).PixelPositionX = int.Parse(BattleLevelButtonX.Text);
                ((MainWindow)System.Windows.Application.Current.MainWindow).PixelPositionY = int.Parse(BattleLevelButtonY.Text);
                ((MainWindow)System.Windows.Application.Current.MainWindow).PixelColor = ColorTranslator.FromHtml(BattleLevelButtonColor.Text.Split(';')[0]);
            }));

            GetPixelPositionAndColor();

            System.Windows.Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
            {
                BattleLevelButtonX.Text = ((MainWindow)System.Windows.Application.Current.MainWindow).PixelPositionX.ToString();
                BattleLevelButtonY.Text = ((MainWindow)System.Windows.Application.Current.MainWindow).PixelPositionY.ToString();
                BattleLevelButtonColor.Text = ColorTranslator.ToHtml(((MainWindow)System.Windows.Application.Current.MainWindow).PixelColor) + ";" + BattleLevelButtonColor.Text.Split(';')[1] + ";" + BattleLevelButtonColor.Text.Split(';')[2] + ";" + BattleLevelButtonColor.Text.Split(';')[3];
                BattleLevelButton1.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(ColorTranslator.ToHtml(((MainWindow)System.Windows.Application.Current.MainWindow).PixelColor)));
            }));
        }

        private void SkillButton1_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
            {
                ((MainWindow)System.Windows.Application.Current.MainWindow).PixelPositionX = int.Parse(SkillButtonX.Text);
                ((MainWindow)System.Windows.Application.Current.MainWindow).PixelPositionY = int.Parse(SkillButtonY.Text);
                ((MainWindow)System.Windows.Application.Current.MainWindow).PixelColor = ColorTranslator.FromHtml(SkillButtonColor.Text);
            }));

            GetPixelPositionAndColor();

            System.Windows.Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
            {
                SkillButtonX.Text = ((MainWindow)System.Windows.Application.Current.MainWindow).PixelPositionX.ToString();
                SkillButtonY.Text = ((MainWindow)System.Windows.Application.Current.MainWindow).PixelPositionY.ToString();
                SkillButtonColor.Text = ColorTranslator.ToHtml(((MainWindow)System.Windows.Application.Current.MainWindow).PixelColor);
                SkillButton1.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(ColorTranslator.ToHtml(((MainWindow)System.Windows.Application.Current.MainWindow).PixelColor)));
            }));
        }

        private void SpeedButton1_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
            {
                ((MainWindow)System.Windows.Application.Current.MainWindow).PixelPositionX = int.Parse(SpeedButtonX.Text);
                ((MainWindow)System.Windows.Application.Current.MainWindow).PixelPositionY = int.Parse(SpeedButtonY.Text);
                ((MainWindow)System.Windows.Application.Current.MainWindow).PixelColor = ColorTranslator.FromHtml(SpeedButtonColor.Text);
            }));

            GetPixelPositionAndColor();

            System.Windows.Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
            {
                SpeedButtonX.Text = ((MainWindow)System.Windows.Application.Current.MainWindow).PixelPositionX.ToString();
                SpeedButtonY.Text = ((MainWindow)System.Windows.Application.Current.MainWindow).PixelPositionY.ToString();
                SpeedButtonColor.Text = ColorTranslator.ToHtml(((MainWindow)System.Windows.Application.Current.MainWindow).PixelColor);
                SpeedButton1.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(ColorTranslator.ToHtml(((MainWindow)System.Windows.Application.Current.MainWindow).PixelColor)));
            }));
        }

        private void PauseButton1_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
            {
                ((MainWindow)System.Windows.Application.Current.MainWindow).PixelPositionX = int.Parse(PauseButtonX.Text);
                ((MainWindow)System.Windows.Application.Current.MainWindow).PixelPositionY = int.Parse(PauseButtonY.Text);
                ((MainWindow)System.Windows.Application.Current.MainWindow).PixelColor = ColorTranslator.FromHtml(PauseButtonColor.Text);
            }));

            GetPixelPositionAndColor();

            System.Windows.Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
            {
                PauseButtonX.Text = ((MainWindow)System.Windows.Application.Current.MainWindow).PixelPositionX.ToString();
                PauseButtonY.Text = ((MainWindow)System.Windows.Application.Current.MainWindow).PixelPositionY.ToString();
                PauseButtonColor.Text = ColorTranslator.ToHtml(((MainWindow)System.Windows.Application.Current.MainWindow).PixelColor);
                PauseButton1.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(ColorTranslator.ToHtml(((MainWindow)System.Windows.Application.Current.MainWindow).PixelColor)));
            }));
        }

        private void VictoryDefeat1_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
            {
                ((MainWindow)System.Windows.Application.Current.MainWindow).PixelPositionX = int.Parse(VictoryDefeatX.Text);
                ((MainWindow)System.Windows.Application.Current.MainWindow).PixelPositionY = int.Parse(VictoryDefeatY.Text);
                ((MainWindow)System.Windows.Application.Current.MainWindow).PixelColor = ColorTranslator.FromHtml(VictoryDefeatColor.Text.Split(';')[0]);
            }));

            GetPixelPositionAndColor();

            System.Windows.Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
            {
                VictoryDefeatX.Text = ((MainWindow)System.Windows.Application.Current.MainWindow).PixelPositionX.ToString();
                VictoryDefeatY.Text = ((MainWindow)System.Windows.Application.Current.MainWindow).PixelPositionY.ToString();
                VictoryDefeatColor.Text = ColorTranslator.ToHtml(((MainWindow)System.Windows.Application.Current.MainWindow).PixelColor) + ";" + VictoryDefeatColor.Text.Split(';')[1];
                VictoryDefeat1.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(ColorTranslator.ToHtml(((MainWindow)System.Windows.Application.Current.MainWindow).PixelColor)));
            }));
        }

        private void GoldButtonBackground1_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
            {
                ((MainWindow)System.Windows.Application.Current.MainWindow).PixelPositionX = int.Parse(GoldButtonBackgroundX.Text);
                ((MainWindow)System.Windows.Application.Current.MainWindow).PixelPositionY = int.Parse(GoldButtonBackgroundY.Text);
                ((MainWindow)System.Windows.Application.Current.MainWindow).PixelColor = ColorTranslator.FromHtml(GoldButtonBackgroundColor.Text.Split(';')[0]);
            }));

            GetPixelPositionAndColor();

            System.Windows.Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
            {
                GoldButtonBackgroundX.Text = ((MainWindow)System.Windows.Application.Current.MainWindow).PixelPositionX.ToString();
                GoldButtonBackgroundY.Text = ((MainWindow)System.Windows.Application.Current.MainWindow).PixelPositionY.ToString();
                GoldButtonBackgroundColor.Text = ColorTranslator.ToHtml(((MainWindow)System.Windows.Application.Current.MainWindow).PixelColor) + ";" + GoldButtonBackgroundColor.Text.Split(';')[1];
                GoldButtonBackground1.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(ColorTranslator.ToHtml(((MainWindow)System.Windows.Application.Current.MainWindow).PixelColor)));
            }));
        }

        private void GoldButtonImage1_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
            {
                ((MainWindow)System.Windows.Application.Current.MainWindow).PixelPositionX = int.Parse(GoldButtonImageX.Text);
                ((MainWindow)System.Windows.Application.Current.MainWindow).PixelPositionY = int.Parse(GoldButtonImageY.Text);
                ((MainWindow)System.Windows.Application.Current.MainWindow).PixelColor = ColorTranslator.FromHtml(GoldButtonImageColor.Text);
            }));

            GetPixelPositionAndColor();

            System.Windows.Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
            {
                GoldButtonImageX.Text = ((MainWindow)System.Windows.Application.Current.MainWindow).PixelPositionX.ToString();
                GoldButtonImageY.Text = ((MainWindow)System.Windows.Application.Current.MainWindow).PixelPositionY.ToString();
                GoldButtonImageColor.Text = ColorTranslator.ToHtml(((MainWindow)System.Windows.Application.Current.MainWindow).PixelColor);
                GoldButtonImage1.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(ColorTranslator.ToHtml(((MainWindow)System.Windows.Application.Current.MainWindow).PixelColor)));
            }));
        }

        private void NextButton1_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
            {
                ((MainWindow)System.Windows.Application.Current.MainWindow).PixelPositionX = int.Parse(NextButtonX.Text);
                ((MainWindow)System.Windows.Application.Current.MainWindow).PixelPositionY = int.Parse(NextButtonY.Text);
                ((MainWindow)System.Windows.Application.Current.MainWindow).PixelColor = ColorTranslator.FromHtml(NextButtonColor.Text);
            }));

            GetPixelPositionAndColor();

            System.Windows.Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
            {
                NextButtonX.Text = ((MainWindow)System.Windows.Application.Current.MainWindow).PixelPositionX.ToString();
                NextButtonY.Text = ((MainWindow)System.Windows.Application.Current.MainWindow).PixelPositionY.ToString();
                NextButtonColor.Text = ColorTranslator.ToHtml(((MainWindow)System.Windows.Application.Current.MainWindow).PixelColor);
                NextButton1.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(ColorTranslator.ToHtml(((MainWindow)System.Windows.Application.Current.MainWindow).PixelColor)));
            }));
        }

        private void AdsButton1_1_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
            {
                ((MainWindow)System.Windows.Application.Current.MainWindow).PixelPositionX = int.Parse(AdsButton1X.Text);
                ((MainWindow)System.Windows.Application.Current.MainWindow).PixelPositionY = int.Parse(AdsButton1Y.Text);
                ((MainWindow)System.Windows.Application.Current.MainWindow).PixelColor = ColorTranslator.FromHtml(AdsButton1Color.Text.Split(';')[0]);
            }));

            GetPixelPositionAndColor();

            System.Windows.Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
            {
                AdsButton1X.Text = ((MainWindow)System.Windows.Application.Current.MainWindow).PixelPositionX.ToString();
                AdsButton1Y.Text = ((MainWindow)System.Windows.Application.Current.MainWindow).PixelPositionY.ToString();
                AdsButton1Color.Text = ColorTranslator.ToHtml(((MainWindow)System.Windows.Application.Current.MainWindow).PixelColor) + ";" + AdsButton1Color.Text.Split(';')[1];
                AdsButton2Color.Text = ColorTranslator.ToHtml(((MainWindow)System.Windows.Application.Current.MainWindow).PixelColor) + ";" + AdsButton2Color.Text.Split(';')[1];
                AdsButton1_1.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(ColorTranslator.ToHtml(((MainWindow)System.Windows.Application.Current.MainWindow).PixelColor)));
            }));
        }

        private void AdsButton2_1_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
            {
                ((MainWindow)System.Windows.Application.Current.MainWindow).PixelPositionX = int.Parse(AdsButton2X.Text);
                ((MainWindow)System.Windows.Application.Current.MainWindow).PixelPositionY = int.Parse(AdsButton2Y.Text);
                ((MainWindow)System.Windows.Application.Current.MainWindow).PixelColor = ColorTranslator.FromHtml(AdsButton2Color.Text.Split(';')[1]);
            }));

            GetPixelPositionAndColor();

            System.Windows.Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
            {
                AdsButton2X.Text = ((MainWindow)System.Windows.Application.Current.MainWindow).PixelPositionX.ToString();
                AdsButton2Y.Text = ((MainWindow)System.Windows.Application.Current.MainWindow).PixelPositionY.ToString();
                AdsButton1Color.Text = AdsButton1Color.Text.Split(';')[0] + ";" + ColorTranslator.ToHtml(((MainWindow)System.Windows.Application.Current.MainWindow).PixelColor);
                AdsButton2Color.Text = AdsButton2Color.Text.Split(';')[0] + ";" + ColorTranslator.ToHtml(((MainWindow)System.Windows.Application.Current.MainWindow).PixelColor);
                AdsButton2_1.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(ColorTranslator.ToHtml(((MainWindow)System.Windows.Application.Current.MainWindow).PixelColor)));
            }));
        }

        private void NotResponding1_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
            {
                ((MainWindow)System.Windows.Application.Current.MainWindow).PixelPositionX = int.Parse(NotRespondingX.Text);
                ((MainWindow)System.Windows.Application.Current.MainWindow).PixelPositionY = int.Parse(NotRespondingY.Text);
                ((MainWindow)System.Windows.Application.Current.MainWindow).PixelColor = ColorTranslator.FromHtml(NotRespondingColor.Text);
            }));

            GetPixelPositionAndColor();

            System.Windows.Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
            {
                NotRespondingX.Text = ((MainWindow)System.Windows.Application.Current.MainWindow).PixelPositionX.ToString();
                NotRespondingY.Text = ((MainWindow)System.Windows.Application.Current.MainWindow).PixelPositionY.ToString();
                NotRespondingColor.Text = ColorTranslator.ToHtml(((MainWindow)System.Windows.Application.Current.MainWindow).PixelColor);
                NotResponding1.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(ColorTranslator.ToHtml(((MainWindow)System.Windows.Application.Current.MainWindow).PixelColor)));
            }));
        }

        private void AppLocationButton2_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
            {
                ((MainWindow)System.Windows.Application.Current.MainWindow).PixelPositionX = int.Parse(AppLocationX.Text);
                ((MainWindow)System.Windows.Application.Current.MainWindow).PixelPositionY = int.Parse(AppLocationY.Text);
                ((MainWindow)System.Windows.Application.Current.MainWindow).PixelColor = ColorTranslator.FromHtml(AppLocationColor.Text.Split(';')[1]);
            }));

            GetPixelPositionAndColor();

            System.Windows.Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
            {
                AppLocationX.Text = ((MainWindow)System.Windows.Application.Current.MainWindow).PixelPositionX.ToString();
                AppLocationY.Text = ((MainWindow)System.Windows.Application.Current.MainWindow).PixelPositionY.ToString();
                AppLocationColor.Text = AppLocationColor.Text.Split(';')[0] + ";" + ColorTranslator.ToHtml(((MainWindow)System.Windows.Application.Current.MainWindow).PixelColor);
                AppLocationButton2.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(ColorTranslator.ToHtml(((MainWindow)System.Windows.Application.Current.MainWindow).PixelColor)));
            }));
        }

        private void GoldChestBox2_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
            {
                ((MainWindow)System.Windows.Application.Current.MainWindow).PixelPositionX = int.Parse(GoldChestBoxX.Text);
                ((MainWindow)System.Windows.Application.Current.MainWindow).PixelPositionY = int.Parse(GoldChestBoxY.Text);
                ((MainWindow)System.Windows.Application.Current.MainWindow).PixelColor = ColorTranslator.FromHtml(GoldChestBoxColor.Text.Split(';')[1]);
            }));

            GetPixelPositionAndColor();

            System.Windows.Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
            {
                GoldChestBoxX.Text = ((MainWindow)System.Windows.Application.Current.MainWindow).PixelPositionX.ToString();
                GoldChestBoxY.Text = ((MainWindow)System.Windows.Application.Current.MainWindow).PixelPositionY.ToString();
                GoldChestBoxColor.Text = GoldChestBoxColor.Text.Split(';')[0] + ";" + ColorTranslator.ToHtml(((MainWindow)System.Windows.Application.Current.MainWindow).PixelColor);
                GoldChestBox2.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(ColorTranslator.ToHtml(((MainWindow)System.Windows.Application.Current.MainWindow).PixelColor)));
            }));
        }

        private void BattleLevelButton2_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
            {
                ((MainWindow)System.Windows.Application.Current.MainWindow).PixelPositionX = int.Parse(BattleLevelButtonX.Text);
                ((MainWindow)System.Windows.Application.Current.MainWindow).PixelPositionY = int.Parse(BattleLevelButtonY.Text);
                ((MainWindow)System.Windows.Application.Current.MainWindow).PixelColor = ColorTranslator.FromHtml(BattleLevelButtonColor.Text.Split(';')[1]);
            }));

            GetPixelPositionAndColor();

            System.Windows.Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
            {
                BattleLevelButtonX.Text = ((MainWindow)System.Windows.Application.Current.MainWindow).PixelPositionX.ToString();
                BattleLevelButtonY.Text = ((MainWindow)System.Windows.Application.Current.MainWindow).PixelPositionY.ToString();
                BattleLevelButtonColor.Text = BattleLevelButtonColor.Text.Split(';')[0] + ";" + ColorTranslator.ToHtml(((MainWindow)System.Windows.Application.Current.MainWindow).PixelColor) + ";" + BattleLevelButtonColor.Text.Split(';')[2] + ";" + BattleLevelButtonColor.Text.Split(';')[3];
                BattleLevelButton2.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(ColorTranslator.ToHtml(((MainWindow)System.Windows.Application.Current.MainWindow).PixelColor)));
            }));
        }

        private void VictoryDefeat2_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
            {
                ((MainWindow)System.Windows.Application.Current.MainWindow).PixelPositionX = int.Parse(VictoryDefeatX.Text);
                ((MainWindow)System.Windows.Application.Current.MainWindow).PixelPositionY = int.Parse(VictoryDefeatY.Text);
                ((MainWindow)System.Windows.Application.Current.MainWindow).PixelColor = ColorTranslator.FromHtml(VictoryDefeatColor.Text.Split(';')[1]);
            }));

            GetPixelPositionAndColor();

            System.Windows.Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
            {
                VictoryDefeatX.Text = ((MainWindow)System.Windows.Application.Current.MainWindow).PixelPositionX.ToString();
                VictoryDefeatY.Text = ((MainWindow)System.Windows.Application.Current.MainWindow).PixelPositionY.ToString();
                VictoryDefeatColor.Text = VictoryDefeatColor.Text.Split(';')[0] + ";" + ColorTranslator.ToHtml(((MainWindow)System.Windows.Application.Current.MainWindow).PixelColor);
                VictoryDefeat2.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(ColorTranslator.ToHtml(((MainWindow)System.Windows.Application.Current.MainWindow).PixelColor)));
            }));
        }

        private void GoldButtonBackground2_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
            {
                ((MainWindow)System.Windows.Application.Current.MainWindow).PixelPositionX = int.Parse(GoldButtonBackgroundX.Text);
                ((MainWindow)System.Windows.Application.Current.MainWindow).PixelPositionY = int.Parse(GoldButtonBackgroundY.Text);
                ((MainWindow)System.Windows.Application.Current.MainWindow).PixelColor = ColorTranslator.FromHtml(GoldButtonBackgroundColor.Text.Split(';')[1]);
            }));

            GetPixelPositionAndColor();

            System.Windows.Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
            {
                GoldButtonBackgroundX.Text = ((MainWindow)System.Windows.Application.Current.MainWindow).PixelPositionX.ToString();
                GoldButtonBackgroundY.Text = ((MainWindow)System.Windows.Application.Current.MainWindow).PixelPositionY.ToString();
                GoldButtonBackgroundColor.Text = GoldButtonBackgroundColor.Text.Split(';')[0] + ";" + ColorTranslator.ToHtml(((MainWindow)System.Windows.Application.Current.MainWindow).PixelColor);
                GoldButtonBackground2.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(ColorTranslator.ToHtml(((MainWindow)System.Windows.Application.Current.MainWindow).PixelColor)));
            }));
        }

        private void BattleLevelButton3_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
            {
                ((MainWindow)System.Windows.Application.Current.MainWindow).PixelPositionX = int.Parse(BattleLevelButtonX.Text);
                ((MainWindow)System.Windows.Application.Current.MainWindow).PixelPositionY = int.Parse(BattleLevelButtonY.Text);
                ((MainWindow)System.Windows.Application.Current.MainWindow).PixelColor = ColorTranslator.FromHtml(BattleLevelButtonColor.Text.Split(';')[2]);
            }));

            GetPixelPositionAndColor();

            System.Windows.Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
            {
                BattleLevelButtonX.Text = ((MainWindow)System.Windows.Application.Current.MainWindow).PixelPositionX.ToString();
                BattleLevelButtonY.Text = ((MainWindow)System.Windows.Application.Current.MainWindow).PixelPositionY.ToString();
                BattleLevelButtonColor.Text = BattleLevelButtonColor.Text.Split(';')[0] + ";" + BattleLevelButtonColor.Text.Split(';')[1] + ";" + ColorTranslator.ToHtml(((MainWindow)System.Windows.Application.Current.MainWindow).PixelColor) + ";" + BattleLevelButtonColor.Text.Split(';')[3];
                BattleLevelButton3.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(ColorTranslator.ToHtml(((MainWindow)System.Windows.Application.Current.MainWindow).PixelColor)));
            }));
        }

        private void BattleLevelButton4_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
            {
                ((MainWindow)System.Windows.Application.Current.MainWindow).PixelPositionX = int.Parse(BattleLevelButtonX.Text);
                ((MainWindow)System.Windows.Application.Current.MainWindow).PixelPositionY = int.Parse(BattleLevelButtonY.Text);
                ((MainWindow)System.Windows.Application.Current.MainWindow).PixelColor = ColorTranslator.FromHtml(BattleLevelButtonColor.Text.Split(';')[3]);
            }));

            GetPixelPositionAndColor();

            System.Windows.Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
            {
                BattleLevelButtonX.Text = ((MainWindow)System.Windows.Application.Current.MainWindow).PixelPositionX.ToString();
                BattleLevelButtonY.Text = ((MainWindow)System.Windows.Application.Current.MainWindow).PixelPositionY.ToString();
                BattleLevelButtonColor.Text = BattleLevelButtonColor.Text.Split(';')[0] + ";" + BattleLevelButtonColor.Text.Split(';')[1] + ";" + BattleLevelButtonColor.Text.Split(';')[2] + ";" + ColorTranslator.ToHtml(((MainWindow)System.Windows.Application.Current.MainWindow).PixelColor);
                BattleLevelButton4.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(ColorTranslator.ToHtml(((MainWindow)System.Windows.Application.Current.MainWindow).PixelColor)));
            }));
        }

        private void ApplyButton_Click(object sender, RoutedEventArgs e)
        {
            using (StreamWriter streamWriter = new StreamWriter($@"setting.txt", false))
            {
                System.Windows.Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
                {
                    ((MainWindow)System.Windows.Application.Current.MainWindow).AppLocationX = int.Parse(AppLocationX.Text);
                    ((MainWindow)System.Windows.Application.Current.MainWindow).AppLocationY = int.Parse(AppLocationY.Text);
                    ((MainWindow)System.Windows.Application.Current.MainWindow).AppLocationColor = AppLocationColor.Text;
                    streamWriter.WriteLine(AppLocationX.Text + "," + AppLocationY.Text + "," + AppLocationColor.Text);

                    ((MainWindow)System.Windows.Application.Current.MainWindow).HomeButtonX = int.Parse(HomeButtonX.Text);
                    ((MainWindow)System.Windows.Application.Current.MainWindow).ShopButtonX = int.Parse(ShopButtonX.Text);
                    ((MainWindow)System.Windows.Application.Current.MainWindow).ShopButtonY = int.Parse(ShopButtonY.Text);
                    ((MainWindow)System.Windows.Application.Current.MainWindow).ShopButtonColor = ShopButtonColor.Text;
                    streamWriter.WriteLine(HomeButtonX.Text + "," + ShopButtonX.Text + "," + ShopButtonY.Text + "," + ShopButtonColor.Text);

                    ((MainWindow)System.Windows.Application.Current.MainWindow).MiddleButtonX = int.Parse(MiddleButtonX.Text);
                    ((MainWindow)System.Windows.Application.Current.MainWindow).MiddleButtonY = int.Parse(MiddleButtonY.Text);
                    ((MainWindow)System.Windows.Application.Current.MainWindow).MiddleButtonColor = MiddleButtonColor.Text;
                    streamWriter.WriteLine(MiddleButtonX.Text + "," + MiddleButtonY.Text + "," + MiddleButtonColor.Text);

                    ((MainWindow)System.Windows.Application.Current.MainWindow).GoldChestBoxX = int.Parse(GoldChestBoxX.Text);
                    ((MainWindow)System.Windows.Application.Current.MainWindow).GoldChestBoxY = int.Parse(GoldChestBoxY.Text);
                    ((MainWindow)System.Windows.Application.Current.MainWindow).GoldChestBoxColor = GoldChestBoxColor.Text;
                    streamWriter.WriteLine(GoldChestBoxX.Text + "," + GoldChestBoxY.Text + "," + GoldChestBoxColor.Text);

                    ((MainWindow)System.Windows.Application.Current.MainWindow).BattleLevelButtonX = int.Parse(BattleLevelButtonX.Text);
                    ((MainWindow)System.Windows.Application.Current.MainWindow).BattleLevelButtonY = int.Parse(BattleLevelButtonY.Text);
                    ((MainWindow)System.Windows.Application.Current.MainWindow).BattleLevelButtonColor = BattleLevelButtonColor.Text;
                    streamWriter.WriteLine(BattleLevelButtonX.Text + "," + BattleLevelButtonY.Text + "," + BattleLevelButtonColor.Text);

                    ((MainWindow)System.Windows.Application.Current.MainWindow).SkillButtonX = int.Parse(SkillButtonX.Text);
                    ((MainWindow)System.Windows.Application.Current.MainWindow).SkillButtonY = int.Parse(SkillButtonY.Text);
                    ((MainWindow)System.Windows.Application.Current.MainWindow).SkillButtonColor = SkillButtonColor.Text;
                    streamWriter.WriteLine(SkillButtonX.Text + "," + SkillButtonY.Text + "," + SkillButtonColor.Text);

                    ((MainWindow)System.Windows.Application.Current.MainWindow).SpeedButtonX = int.Parse(SpeedButtonX.Text);
                    ((MainWindow)System.Windows.Application.Current.MainWindow).SpeedButtonY = int.Parse(SpeedButtonY.Text);
                    ((MainWindow)System.Windows.Application.Current.MainWindow).SpeedButtonColor = SpeedButtonColor.Text;
                    streamWriter.WriteLine(SpeedButtonX.Text + "," + SpeedButtonY.Text + "," + SpeedButtonColor.Text);

                    ((MainWindow)System.Windows.Application.Current.MainWindow).PauseButtonX = int.Parse(PauseButtonX.Text);
                    ((MainWindow)System.Windows.Application.Current.MainWindow).PauseButtonY = int.Parse(PauseButtonY.Text);
                    ((MainWindow)System.Windows.Application.Current.MainWindow).PauseButtonColor = PauseButtonColor.Text;
                    streamWriter.WriteLine(PauseButtonX.Text + "," + PauseButtonY.Text + "," + PauseButtonColor.Text);

                    ((MainWindow)System.Windows.Application.Current.MainWindow).VictoryDefeatX = int.Parse(VictoryDefeatX.Text);
                    ((MainWindow)System.Windows.Application.Current.MainWindow).VictoryDefeatY = int.Parse(VictoryDefeatY.Text);
                    ((MainWindow)System.Windows.Application.Current.MainWindow).VictoryDefeatColor = VictoryDefeatColor.Text;
                    streamWriter.WriteLine(VictoryDefeatX.Text + "," + VictoryDefeatY.Text + "," + VictoryDefeatColor.Text);

                    ((MainWindow)System.Windows.Application.Current.MainWindow).GoldButtonBackgroundX = int.Parse(GoldButtonBackgroundX.Text);
                    ((MainWindow)System.Windows.Application.Current.MainWindow).GoldButtonBackgroundY = int.Parse(GoldButtonBackgroundY.Text);
                    ((MainWindow)System.Windows.Application.Current.MainWindow).GoldButtonBackgroundColor = GoldButtonBackgroundColor.Text;
                    streamWriter.WriteLine(GoldButtonBackgroundX.Text + "," + GoldButtonBackgroundY.Text + "," + GoldButtonBackgroundColor.Text);

                    ((MainWindow)System.Windows.Application.Current.MainWindow).GoldButtonImageX = int.Parse(GoldButtonImageX.Text);
                    ((MainWindow)System.Windows.Application.Current.MainWindow).GoldButtonImageY = int.Parse(GoldButtonImageY.Text);
                    ((MainWindow)System.Windows.Application.Current.MainWindow).GoldButtonImageColor = GoldButtonImageColor.Text;
                    streamWriter.WriteLine(GoldButtonImageX.Text + "," + GoldButtonImageY.Text + "," + GoldButtonImageColor.Text);

                    ((MainWindow)System.Windows.Application.Current.MainWindow).NextButtonX = int.Parse(NextButtonX.Text);
                    ((MainWindow)System.Windows.Application.Current.MainWindow).NextButtonY = int.Parse(NextButtonY.Text);
                    ((MainWindow)System.Windows.Application.Current.MainWindow).NextButtonColor = NextButtonColor.Text;
                    streamWriter.WriteLine(NextButtonX.Text + "," + NextButtonY.Text + "," + NextButtonColor.Text);

                    ((MainWindow)System.Windows.Application.Current.MainWindow).AdsButtonX = (int.Parse(AdsButton1X.Text) + int.Parse(AdsButton2X.Text)) / 2;
                    ((MainWindow)System.Windows.Application.Current.MainWindow).AdsButton1Y = int.Parse(AdsButton1Y.Text);
                    ((MainWindow)System.Windows.Application.Current.MainWindow).AdsButton2Y = int.Parse(AdsButton2Y.Text);
                    ((MainWindow)System.Windows.Application.Current.MainWindow).AdsButtonColor = AdsButton1Color.Text + ";" + AdsButton2Color.Text;
                    streamWriter.WriteLine((int)((int.Parse(AdsButton1X.Text) + int.Parse(AdsButton2X.Text)) / 2) + "," + AdsButton1Y.Text + "," + AdsButton2Y.Text + "," + AdsButton1Color.Text + ";" + AdsButton2Color.Text);

                    ((MainWindow)System.Windows.Application.Current.MainWindow).AdsCloseButton1X = int.Parse(AdsCloseButton1X.Text);
                    ((MainWindow)System.Windows.Application.Current.MainWindow).AdsCloseButton2X = int.Parse(AdsCloseButton2X.Text);
                    ((MainWindow)System.Windows.Application.Current.MainWindow).AdsCloseButtonY = (int.Parse(AdsCloseButton1Y.Text) + int.Parse(AdsCloseButton2Y.Text)) / 2;
                    streamWriter.WriteLine(AdsCloseButton1X.Text + "," + AdsCloseButton2X.Text + "," + (int)((int.Parse(AdsCloseButton1Y.Text) + int.Parse(AdsCloseButton2Y.Text)) / 2));

                    ((MainWindow)System.Windows.Application.Current.MainWindow).NotRespondingX = int.Parse(NotRespondingX.Text);
                    ((MainWindow)System.Windows.Application.Current.MainWindow).NotRespondingY = int.Parse(NotRespondingY.Text);
                    ((MainWindow)System.Windows.Application.Current.MainWindow).NotRespondingColor = NotRespondingColor.Text;
                    streamWriter.WriteLine(NotRespondingX.Text + "," + NotRespondingY.Text + "," + NotRespondingColor.Text);
                }));
            }

            this.Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
