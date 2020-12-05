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
using System.Windows.Forms;
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

        private void NoGoldButton1_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
            {
                ((MainWindow)System.Windows.Application.Current.MainWindow).PixelPositionX = int.Parse(NoGoldX.Text);
                ((MainWindow)System.Windows.Application.Current.MainWindow).PixelPositionY = int.Parse(NoGoldY.Text);
                ((MainWindow)System.Windows.Application.Current.MainWindow).PixelColor = ColorTranslator.FromHtml(NoGoldColor.Text);
            }));

            GetPixelPositionAndColor();

            System.Windows.Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
            {
                NoGoldX.Text = ((MainWindow)System.Windows.Application.Current.MainWindow).PixelPositionX.ToString();
                NoGoldY.Text = ((MainWindow)System.Windows.Application.Current.MainWindow).PixelPositionY.ToString();
                NoGoldColor.Text = ColorTranslator.ToHtml(((MainWindow)System.Windows.Application.Current.MainWindow).PixelColor);
                NoGoldButton1.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(ColorTranslator.ToHtml(((MainWindow)System.Windows.Application.Current.MainWindow).PixelColor)));
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

        private void HomeButton1_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
            {
                ((MainWindow)System.Windows.Application.Current.MainWindow).PixelPositionX = int.Parse(HomeButtonX.Text);
                ((MainWindow)System.Windows.Application.Current.MainWindow).PixelPositionY = int.Parse(HomeButtonY.Text);
            }));

            GetPixelPositionAndColor();

            System.Windows.Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
            {
                HomeButtonX.Text = ((MainWindow)System.Windows.Application.Current.MainWindow).PixelPositionX.ToString();
                HomeButtonY.Text = ((MainWindow)System.Windows.Application.Current.MainWindow).PixelPositionY.ToString();
            }));
        }

        private void AdsCloseButton1_1_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
            {
                ((MainWindow)System.Windows.Application.Current.MainWindow).PixelPositionX = int.Parse(AdsCloseButton1X.Text);
                ((MainWindow)System.Windows.Application.Current.MainWindow).PixelPositionY = int.Parse(AdsCloseButton1Y.Text);
                ((MainWindow)System.Windows.Application.Current.MainWindow).PixelColor = ColorTranslator.FromHtml(AdsCloseButton1Color.Text.Split(';')[0]);
            }));

            GetPixelPositionAndColor();

            System.Windows.Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
            {
                AdsCloseButton1X.Text = ((MainWindow)System.Windows.Application.Current.MainWindow).PixelPositionX.ToString();
                AdsCloseButton1Y.Text = ((MainWindow)System.Windows.Application.Current.MainWindow).PixelPositionY.ToString();
                AdsCloseButton1Color.Text = ColorTranslator.ToHtml(((MainWindow)System.Windows.Application.Current.MainWindow).PixelColor) + ";" + AdsCloseButton1Color.Text.Split(';')[1];
                AdsCloseButton2Color.Text = ColorTranslator.ToHtml(((MainWindow)System.Windows.Application.Current.MainWindow).PixelColor) + ";" + AdsCloseButton2Color.Text.Split(';')[1];
                AdsCloseButton1_1.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(ColorTranslator.ToHtml(((MainWindow)System.Windows.Application.Current.MainWindow).PixelColor)));
            }));
        }

        private void AdsCloseButton2_1_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
            {
                ((MainWindow)System.Windows.Application.Current.MainWindow).PixelPositionX = int.Parse(AdsCloseButton2X.Text);
                ((MainWindow)System.Windows.Application.Current.MainWindow).PixelPositionY = int.Parse(AdsCloseButton2Y.Text);
                ((MainWindow)System.Windows.Application.Current.MainWindow).PixelColor = ColorTranslator.FromHtml(AdsCloseButton2Color.Text.Split(';')[1]);
            }));

            GetPixelPositionAndColor();

            System.Windows.Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
            {
                AdsCloseButton2X.Text = ((MainWindow)System.Windows.Application.Current.MainWindow).PixelPositionX.ToString();
                AdsCloseButton2Y.Text = ((MainWindow)System.Windows.Application.Current.MainWindow).PixelPositionY.ToString();
                AdsCloseButton1Color.Text = AdsCloseButton1Color.Text.Split(';')[0] + ";" + ColorTranslator.ToHtml(((MainWindow)System.Windows.Application.Current.MainWindow).PixelColor);
                AdsCloseButton2Color.Text = AdsCloseButton2Color.Text.Split(';')[0] + ";" + ColorTranslator.ToHtml(((MainWindow)System.Windows.Application.Current.MainWindow).PixelColor);
                AdsCloseButton2_1.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(ColorTranslator.ToHtml(((MainWindow)System.Windows.Application.Current.MainWindow).PixelColor)));
            }));
        }

        private void ChangeButtonsColors()
        {
            AppLocationButton1.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(AppLocationColor.Text.Split(';')[0]));
            ShopButton1.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(ShopButtonColor.Text));
            MiddleButton1.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(MiddleButtonColor.Text));
            GoldChestBox1.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(GoldChestBoxColor.Text.Split(';')[0]));
            BattleLevelButton1.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(BattleLevelButtonColor.Text.Split(';')[0]));
            SkillButton1.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(SkillButtonColor.Text));
            SpeedButton1.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(SpeedButtonColor.Text));
            PauseButton1.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(PauseButtonColor.Text));
            VictoryDefeat1.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(VictoryDefeatColor.Text.Split(';')[0]));
            GoldButtonBackground1.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(GoldButtonBackgroundColor.Text.Split(';')[0]));
            GoldButtonImage1.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(GoldButtonImageColor.Text.Split(';')[0]));
            NextButton1.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(NextButtonColor.Text));
            AdsButton1_1.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(AdsButton1Color.Text.Split(';')[0]));
            AdsButton2_1.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(AdsButton1Color.Text.Split(';')[0]));
            NotResponding1.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(NotRespondingColor.Text));
            NoGoldButton1.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(NoGoldColor.Text));

            AppLocationButton2.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(AppLocationColor.Text.Split(';')[1]));
            GoldChestBox2.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(GoldChestBoxColor.Text.Split(';')[1]));
            BattleLevelButton2.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(BattleLevelButtonColor.Text.Split(';')[1]));
            VictoryDefeat2.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(VictoryDefeatColor.Text.Split(';')[1]));
            GoldButtonBackground2.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(GoldButtonBackgroundColor.Text.Split(';')[1]));

            BattleLevelButton3.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(BattleLevelButtonColor.Text.Split(';')[2]));
            BattleLevelButton4.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(BattleLevelButtonColor.Text.Split(';')[3]));

            AdsCloseButton1_1.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(AdsCloseButton1Color.Text.Split(';')[0]));
            AdsCloseButton2_1.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(AdsCloseButton1Color.Text.Split(';')[1]));
        }

        private void LoadButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.DefaultExt = "txt";
            openFileDialog.Filter = "Text Files (*.txt)|*.txt";
            openFileDialog.ShowDialog();
            if (openFileDialog.FileName.Length > 0)
            {
                string[] lines = File.ReadAllLines(openFileDialog.FileName);
                AppLocationX.Text = lines[0].Split(',')[0];
                AppLocationY.Text = lines[0].Split(',')[1];
                AppLocationColor.Text = lines[0].Split(',')[2];

                HomeButtonX.Text = lines[1].Split(',')[0];
                ShopButtonX.Text = lines[1].Split(',')[1];
                ShopButtonY.Text = lines[1].Split(',')[2];
                ShopButtonColor.Text = lines[1].Split(',')[3];

                MiddleButtonX.Text = lines[2].Split(',')[0];
                MiddleButtonY.Text = lines[2].Split(',')[1];
                MiddleButtonColor.Text = lines[2].Split(',')[2];

                GoldChestBoxX.Text = lines[3].Split(',')[0];
                GoldChestBoxY.Text = lines[3].Split(',')[1];
                GoldChestBoxColor.Text = lines[3].Split(',')[2];

                BattleLevelButtonX.Text = lines[4].Split(',')[0];
                BattleLevelButtonY.Text = lines[4].Split(',')[1];
                BattleLevelButtonColor.Text = lines[4].Split(',')[2];

                SkillButtonX.Text = lines[5].Split(',')[0];
                SkillButtonY.Text = lines[5].Split(',')[1];
                SkillButtonColor.Text = lines[5].Split(',')[2];

                SpeedButtonX.Text = lines[6].Split(',')[0];
                SpeedButtonY.Text = lines[6].Split(',')[1];
                SpeedButtonColor.Text = lines[6].Split(',')[2];

                PauseButtonX.Text = lines[7].Split(',')[0];
                PauseButtonY.Text = lines[7].Split(',')[1];
                PauseButtonColor.Text = lines[7].Split(',')[2];

                VictoryDefeatX.Text = lines[8].Split(',')[0];
                VictoryDefeatY.Text = lines[8].Split(',')[1];
                VictoryDefeatColor.Text = lines[8].Split(',')[2];

                GoldButtonBackgroundX.Text = lines[9].Split(',')[0];
                GoldButtonBackgroundY.Text = lines[9].Split(',')[1];
                GoldButtonBackgroundColor.Text = lines[9].Split(',')[2];

                GoldButtonImageX.Text = lines[10].Split(',')[0];
                GoldButtonImageY.Text = lines[10].Split(',')[1];
                GoldButtonImageColor.Text = lines[10].Split(',')[2];

                NextButtonX.Text = lines[11].Split(',')[0];
                NextButtonY.Text = lines[11].Split(',')[1];
                NextButtonColor.Text = lines[11].Split(',')[2];

                // 예외발생 임시처리 나중에 삭제할것
                if (lines.Length == 16)
                {
                    NoGoldX.Text = lines[12].Split(',')[0];
                    NoGoldY.Text = lines[12].Split(',')[1];
                    NoGoldColor.Text = lines[12].Split(',')[2];

                    AdsButton1X.Text = lines[13].Split(',')[0];
                    AdsButton2X.Text = lines[13].Split(',')[0];
                    AdsButton1Y.Text = lines[13].Split(',')[1];
                    AdsButton2Y.Text = lines[13].Split(',')[2];
                    AdsButton1Color.Text = lines[13].Split(',')[3];
                    AdsButton2Color.Text = lines[13].Split(',')[3];

                    AdsCloseButton1X.Text = lines[14].Split(',')[0];
                    AdsCloseButton2X.Text = lines[14].Split(',')[1];
                    AdsCloseButton1Y.Text = lines[14].Split(',')[2];
                    AdsCloseButton2Y.Text = lines[14].Split(',')[2];
                    // 예외발생 임시처리 나중에 삭제할것
                    if (lines[14].Split(',').Length < 4)
                    {
                        AdsCloseButton1Color.Text = "#4c4c4f;#3c4043".ToUpper();
                        AdsCloseButton2Color.Text = "#4c4c4f;#3c4043".ToUpper();
                    }
                    else
                    {
                        AdsCloseButton1Color.Text = lines[14].Split(',')[3];
                        AdsCloseButton2Color.Text = lines[14].Split(',')[3];
                    }

                    NotRespondingX.Text = lines[15].Split(',')[0];
                    NotRespondingY.Text = lines[15].Split(',')[1];
                    NotRespondingColor.Text = lines[15].Split(',')[2];
                }
                else
                {
                    AdsButton1X.Text = lines[12].Split(',')[0];
                    AdsButton2X.Text = lines[12].Split(',')[0];
                    AdsButton1Y.Text = lines[12].Split(',')[1];
                    AdsButton2Y.Text = lines[12].Split(',')[2];
                    AdsButton1Color.Text = lines[12].Split(',')[3];
                    AdsButton2Color.Text = lines[12].Split(',')[3];

                    AdsCloseButton1X.Text = lines[13].Split(',')[0];
                    AdsCloseButton2X.Text = lines[13].Split(',')[1];
                    AdsCloseButton1Y.Text = lines[13].Split(',')[2];
                    AdsCloseButton2Y.Text = lines[13].Split(',')[2];
                    // 예외발생 임시처리 나중에 삭제할것
                    if (lines[13].Split(',').Length < 4)
                    {
                        AdsCloseButton1Color.Text = "#4c4c4f;#3c4043".ToUpper();
                        AdsCloseButton2Color.Text = "#4c4c4f;#3c4043".ToUpper();
                    }
                    else
                    {
                        AdsCloseButton1Color.Text = lines[13].Split(',')[3];
                        AdsCloseButton2Color.Text = lines[13].Split(',')[3];
                    }

                    NotRespondingX.Text = lines[14].Split(',')[0];
                    NotRespondingY.Text = lines[14].Split(',')[1];
                    NotRespondingColor.Text = lines[14].Split(',')[2];
                }

                ChangeButtonsColors();
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Text File (*.txt)|*.txt";
            if (saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                using (StreamWriter streamWriter = new StreamWriter(saveFileDialog.FileName, false))
                {
                    streamWriter.WriteLine(AppLocationX.Text + "," + AppLocationY.Text + "," + AppLocationColor.Text);
                    streamWriter.WriteLine(HomeButtonX.Text + "," + ShopButtonX.Text + "," + ShopButtonY.Text + "," + ShopButtonColor.Text);
                    streamWriter.WriteLine(MiddleButtonX.Text + "," + MiddleButtonY.Text + "," + MiddleButtonColor.Text);
                    streamWriter.WriteLine(GoldChestBoxX.Text + "," + GoldChestBoxY.Text + "," + GoldChestBoxColor.Text);
                    streamWriter.WriteLine(BattleLevelButtonX.Text + "," + BattleLevelButtonY.Text + "," + BattleLevelButtonColor.Text);
                    streamWriter.WriteLine(SkillButtonX.Text + "," + SkillButtonY.Text + "," + SkillButtonColor.Text);
                    streamWriter.WriteLine(SpeedButtonX.Text + "," + SpeedButtonY.Text + "," + SpeedButtonColor.Text);
                    streamWriter.WriteLine(PauseButtonX.Text + "," + PauseButtonY.Text + "," + PauseButtonColor.Text);
                    streamWriter.WriteLine(VictoryDefeatX.Text + "," + VictoryDefeatY.Text + "," + VictoryDefeatColor.Text);
                    streamWriter.WriteLine(GoldButtonBackgroundX.Text + "," + GoldButtonBackgroundY.Text + "," + GoldButtonBackgroundColor.Text);
                    streamWriter.WriteLine(GoldButtonImageX.Text + "," + GoldButtonImageY.Text + "," + GoldButtonImageColor.Text);
                    streamWriter.WriteLine(NextButtonX.Text + "," + NextButtonY.Text + "," + NextButtonColor.Text);
                    streamWriter.WriteLine(NoGoldX.Text + "," + NoGoldY.Text + "," + NoGoldColor.Text);
                    streamWriter.WriteLine((int)((int.Parse(AdsButton1X.Text) + int.Parse(AdsButton2X.Text)) / 2) + "," + AdsButton1Y.Text + "," + AdsButton2Y.Text + "," + AdsButton1Color.Text);
                    streamWriter.WriteLine(AdsCloseButton1X.Text + "," + AdsCloseButton2X.Text + "," + (int)((int.Parse(AdsCloseButton1Y.Text) + int.Parse(AdsCloseButton2Y.Text)) / 2) + "," + AdsCloseButton1Color.Text);
                    streamWriter.WriteLine(NotRespondingX.Text + "," + NotRespondingY.Text + "," + NotRespondingColor.Text);
                }
            }
        }

        private void DefaultButton_Click(object sender, RoutedEventArgs e)
        {
            AppLocationX.Text = 60.ToString();
            AppLocationY.Text = 500.ToString();
            AppLocationColor.Text = "#a9d8ff;#97c601".ToUpper();

            HomeButtonX.Text = 290.ToString();
            ShopButtonX.Text = 65.ToString();
            ShopButtonY.Text = 980.ToString();
            ShopButtonColor.Text = "#ea3d34".ToUpper();

            MiddleButtonX.Text = 195.ToString();
            MiddleButtonY.Text = 680.ToString();
            MiddleButtonColor.Text = "#fdbb00".ToUpper();

            GoldChestBoxX.Text = 150.ToString();
            GoldChestBoxY.Text = 410.ToString();
            GoldChestBoxColor.Text = "#fff102;#eabf2f".ToUpper();

            BattleLevelButtonX.Text = 180.ToString();
            BattleLevelButtonY.Text = 855.ToString();
            BattleLevelButtonColor.Text = "#fdbb00;#ca9600;#bd1808;#0d677a".ToUpper();

            SkillButtonX.Text = 475.ToString();
            SkillButtonY.Text = 920.ToString();
            SkillButtonColor.Text = "#fdbb00".ToUpper();

            SpeedButtonX.Text = 514.ToString();
            SpeedButtonY.Text = 989.ToString();
            SpeedButtonColor.Text = "#eda500".ToUpper();

            PauseButtonX.Text = 215.ToString();
            PauseButtonY.Text = 455.ToString();
            PauseButtonColor.Text = "#fdbb00".ToUpper();

            VictoryDefeatX.Text = 120.ToString();
            VictoryDefeatY.Text = 355.ToString();
            VictoryDefeatColor.Text = "#d91c13;#12a7d8".ToUpper();

            GoldButtonBackgroundX.Text = 115.ToString();
            GoldButtonBackgroundY.Text = 780.ToString();
            GoldButtonBackgroundColor.Text = "#7da70a;#8e8e8e".ToUpper();

            GoldButtonImageX.Text = 133.ToString();
            GoldButtonImageY.Text = 755.ToString();
            GoldButtonImageColor.Text = "#ffea90".ToUpper();

            NextButtonX.Text = 450.ToString();
            NextButtonY.Text = 710.ToString();
            NextButtonColor.Text = "#fdbb00".ToUpper();

            AdsButton1X.Text = 496.ToString();
            AdsButton2X.Text = 496.ToString();
            AdsButton1Y.Text = 180.ToString();
            AdsButton2Y.Text = 190.ToString();
            AdsButton1Color.Text = "#e9e9d8;#efe7d6".ToUpper();
            AdsButton2Color.Text = "#e9e9d8;#efe7d6".ToUpper();

            AdsCloseButton1X.Text = 45.ToString();
            AdsCloseButton2X.Text = 513.ToString();
            AdsCloseButton1Y.Text = 63.ToString();
            AdsCloseButton2Y.Text = 63.ToString();
            AdsCloseButton1Color.Text = "#4c4c4f;#3c4043".ToUpper();
            AdsCloseButton2Color.Text = "#4c4c4f;#3c4043".ToUpper();

            NotRespondingX.Text = 79.ToString();
            NotRespondingY.Text = 540.ToString();
            NotRespondingColor.Text = "#009688".ToUpper();

            NoGoldX.Text = 320.ToString(); ;
            NoGoldY.Text = 646.ToString(); ;
            NoGoldColor.Text = "dfd6be".ToUpper();

            ChangeButtonsColors();
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

                    ((MainWindow)System.Windows.Application.Current.MainWindow).NoGoldX = int.Parse(NoGoldX.Text);
                    ((MainWindow)System.Windows.Application.Current.MainWindow).NoGoldY = int.Parse(NoGoldY.Text);
                    ((MainWindow)System.Windows.Application.Current.MainWindow).NoGoldColor = NoGoldColor.Text;
                    streamWriter.WriteLine(NoGoldX.Text + "," + NoGoldY.Text + "," + NoGoldColor.Text);

                    ((MainWindow)System.Windows.Application.Current.MainWindow).AdsButtonX = (int.Parse(AdsButton1X.Text) + int.Parse(AdsButton2X.Text)) / 2;
                    ((MainWindow)System.Windows.Application.Current.MainWindow).AdsButton1Y = int.Parse(AdsButton1Y.Text);
                    ((MainWindow)System.Windows.Application.Current.MainWindow).AdsButton2Y = int.Parse(AdsButton2Y.Text);
                    ((MainWindow)System.Windows.Application.Current.MainWindow).AdsButtonColor = AdsButton1Color.Text;
                    streamWriter.WriteLine((int)((int.Parse(AdsButton1X.Text) + int.Parse(AdsButton2X.Text)) / 2) + "," + AdsButton1Y.Text + "," + AdsButton2Y.Text + "," + AdsButton1Color.Text);

                    ((MainWindow)System.Windows.Application.Current.MainWindow).AdsCloseButton1X = int.Parse(AdsCloseButton1X.Text);
                    ((MainWindow)System.Windows.Application.Current.MainWindow).AdsCloseButton2X = int.Parse(AdsCloseButton2X.Text);
                    ((MainWindow)System.Windows.Application.Current.MainWindow).AdsCloseButtonY = (int.Parse(AdsCloseButton1Y.Text) + int.Parse(AdsCloseButton2Y.Text)) / 2;
                    ((MainWindow)System.Windows.Application.Current.MainWindow).AdsCloseButtonColor = AdsButton1Color.Text;
                    streamWriter.WriteLine(AdsCloseButton1X.Text + "," + AdsCloseButton2X.Text + "," + (int)((int.Parse(AdsCloseButton1Y.Text) + int.Parse(AdsCloseButton2Y.Text)) / 2) + "," + AdsCloseButton1Color.Text);

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
