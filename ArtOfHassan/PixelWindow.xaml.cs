using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace ArtOfHassan
{
    /// <summary>
    /// PixelWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class PixelWindow : Window
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


        public PixelWindow()
        {
            InitializeComponent();

            if (((MainWindow)System.Windows.Application.Current.MainWindow).KoreanCheckBox.IsChecked.Value)
            {
                this.Title = "픽셀 위치 및 색상 사용자화";
                ItemTextBlock.Text = "항목";
                AppLocationTextBlock.Text = "어플 위치";
                HomeButtonTextBlock.Text = "홈버튼";
                ShopButtonTextBlock.Text = "상점버튼";
                CollectButtonTextBlock.Text = "수집버튼";
                GoldChestBoxTextBlock.Text = "시간보상";
                BattleLevelButtonTextBlock.Text = "전투레벨버튼";
                AutoSkillButtonTextBlock.Text = "자동스킬버튼";
                VIPX2ButtonTextBlock.Text = "2배속버튼";
                ContinueButtonTextBlock.Text = "계속버튼";
                VictoryDefeatTextBlock.Text = "승리패배 플래그";
                X3GoldButtonBackgroundTextBlock.Text = "골드3배버튼 배경";
                X3GoldButtonImageTextBlock.Text = "골드3배버튼 그림";
                NextButtonTextBlock.Text = "다음버튼 (패배시)";
                NoGoldTextBlock.Text = "골드벌이 없을 때";
                GoldAdCloseButtonTextBlock.Text = "골드광고 닫기버튼";
                TroopAdCloseButtonTextBlock.Text = "용병광고 닫기버튼";
                MidasAdCloseButtonTextBlock.Text = "마이더스 닫기버튼";
                LeftAdCloseButtonTextBlock.Text = "왼쪽 광고닫기버튼";
                RightAdCloseButtonTextBlock.Text = "오른쪽 광고닫기버튼";
                LatestUsedAppButtonTextBlock.Text = "최근 사용앱 버튼";
                CloseAllAppButtonTextBlock.Text = "모든앱 닫기버튼";
                NotRespondingButtonTextBlock.Text = "응답없음 닫기버튼";
                XPositionTextBlock.Text = "X 좌표";
                YPositionTextBlock.Text = "Y 좌표";
                ColorCodeTextBlock.Text = "HTML 색상코드";
                ColorPickTextBlock.Text = "색상 선택";

                LoadButton.Content = "불러오기";
                SaveButton.Content = "다른이름으로\n      저장";
                DefaultButton.Content = "초기화";
                ApplyButton.Content = "적용";
                CancelButton.Content = "취소";
            }
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

        private void CollectButton1_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
            {
                ((MainWindow)System.Windows.Application.Current.MainWindow).PixelPositionX = int.Parse(CollectButtonX.Text);
                ((MainWindow)System.Windows.Application.Current.MainWindow).PixelPositionY = int.Parse(CollectButtonY.Text);
                ((MainWindow)System.Windows.Application.Current.MainWindow).PixelColor = ColorTranslator.FromHtml(CollectButtonColor.Text);
            }));

            GetPixelPositionAndColor();

            System.Windows.Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
            {
                CollectButtonX.Text = ((MainWindow)System.Windows.Application.Current.MainWindow).PixelPositionX.ToString();
                CollectButtonY.Text = ((MainWindow)System.Windows.Application.Current.MainWindow).PixelPositionY.ToString();
                CollectButtonColor.Text = ColorTranslator.ToHtml(((MainWindow)System.Windows.Application.Current.MainWindow).PixelColor);
                CollectButton1.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(ColorTranslator.ToHtml(((MainWindow)System.Windows.Application.Current.MainWindow).PixelColor)));
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

        private void ContinueButton1_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
            {
                ((MainWindow)System.Windows.Application.Current.MainWindow).PixelPositionX = int.Parse(ContinueButtonX.Text);
                ((MainWindow)System.Windows.Application.Current.MainWindow).PixelPositionY = int.Parse(ContinueButtonY.Text);
                ((MainWindow)System.Windows.Application.Current.MainWindow).PixelColor = ColorTranslator.FromHtml(ContinueButtonColor.Text);
            }));

            GetPixelPositionAndColor();

            System.Windows.Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
            {
                ContinueButtonX.Text = ((MainWindow)System.Windows.Application.Current.MainWindow).PixelPositionX.ToString();
                ContinueButtonY.Text = ((MainWindow)System.Windows.Application.Current.MainWindow).PixelPositionY.ToString();
                ContinueButtonColor.Text = ColorTranslator.ToHtml(((MainWindow)System.Windows.Application.Current.MainWindow).PixelColor);
                ContinueButton1.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(ColorTranslator.ToHtml(((MainWindow)System.Windows.Application.Current.MainWindow).PixelColor)));
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

        private void GoldAdCloseButton1_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
            {
                ((MainWindow)System.Windows.Application.Current.MainWindow).PixelPositionX = int.Parse(GoldAdCloseButtonX.Text);
                ((MainWindow)System.Windows.Application.Current.MainWindow).PixelPositionY = int.Parse(GoldAdCloseButtonY.Text);
                ((MainWindow)System.Windows.Application.Current.MainWindow).PixelColor = ColorTranslator.FromHtml(GoldAdCloseButtonColor.Text.Split(';')[0]);
            }));

            GetPixelPositionAndColor();

            System.Windows.Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
            {
                GoldAdCloseButtonX.Text = ((MainWindow)System.Windows.Application.Current.MainWindow).PixelPositionX.ToString();
                GoldAdCloseButtonY.Text = ((MainWindow)System.Windows.Application.Current.MainWindow).PixelPositionY.ToString();
                GoldAdCloseButtonColor.Text = ColorTranslator.ToHtml(((MainWindow)System.Windows.Application.Current.MainWindow).PixelColor) + ";" + GoldAdCloseButtonColor.Text.Split(';')[1] + ";" + GoldAdCloseButtonColor.Text.Split(';')[2];
                TroopAdCloseButtonColor.Text = ColorTranslator.ToHtml(((MainWindow)System.Windows.Application.Current.MainWindow).PixelColor) + ";" + TroopAdCloseButtonColor.Text.Split(';')[1] + ";" + TroopAdCloseButtonColor.Text.Split(';')[2];
                MidasAdCloseButtonColor.Text = ColorTranslator.ToHtml(((MainWindow)System.Windows.Application.Current.MainWindow).PixelColor) + ";" + MidasAdCloseButtonColor.Text.Split(';')[1] + ";" + MidasAdCloseButtonColor.Text.Split(';')[2];
                GoldAdCloseButton1.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(ColorTranslator.ToHtml(((MainWindow)System.Windows.Application.Current.MainWindow).PixelColor)));
            }));
        }

        private void TroopAdCloseButton1_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
            {
                ((MainWindow)System.Windows.Application.Current.MainWindow).PixelPositionX = int.Parse(TroopAdCloseButtonX.Text);
                ((MainWindow)System.Windows.Application.Current.MainWindow).PixelPositionY = int.Parse(TroopAdCloseButtonY.Text);
                ((MainWindow)System.Windows.Application.Current.MainWindow).PixelColor = ColorTranslator.FromHtml(TroopAdCloseButtonColor.Text.Split(';')[1]);
            }));

            GetPixelPositionAndColor();

            System.Windows.Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
            {
                TroopAdCloseButtonX.Text = ((MainWindow)System.Windows.Application.Current.MainWindow).PixelPositionX.ToString();
                TroopAdCloseButtonY.Text = ((MainWindow)System.Windows.Application.Current.MainWindow).PixelPositionY.ToString();
                GoldAdCloseButtonColor.Text = GoldAdCloseButtonColor.Text.Split(';')[0] + ";" + ColorTranslator.ToHtml(((MainWindow)System.Windows.Application.Current.MainWindow).PixelColor) + ";" + GoldAdCloseButtonColor.Text.Split(';')[2];
                TroopAdCloseButtonColor.Text = TroopAdCloseButtonColor.Text.Split(';')[0] + ";" + ColorTranslator.ToHtml(((MainWindow)System.Windows.Application.Current.MainWindow).PixelColor) + ";" + TroopAdCloseButtonColor.Text.Split(';')[2];
                MidasAdCloseButtonColor.Text = MidasAdCloseButtonColor.Text.Split(';')[0] + ";" + ColorTranslator.ToHtml(((MainWindow)System.Windows.Application.Current.MainWindow).PixelColor) + ";" + MidasAdCloseButtonColor.Text.Split(';')[2];
                TroopAdCloseButton1.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(ColorTranslator.ToHtml(((MainWindow)System.Windows.Application.Current.MainWindow).PixelColor)));
            }));
        }

        private void MidasAdCloseButton1_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
            {
                ((MainWindow)System.Windows.Application.Current.MainWindow).PixelPositionX = int.Parse(MidasAdCloseButtonX.Text);
                ((MainWindow)System.Windows.Application.Current.MainWindow).PixelPositionY = int.Parse(MidasAdCloseButtonY.Text);
                ((MainWindow)System.Windows.Application.Current.MainWindow).PixelColor = ColorTranslator.FromHtml(MidasAdCloseButtonColor.Text.Split(';')[2]);
            }));

            GetPixelPositionAndColor();

            System.Windows.Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
            {
                MidasAdCloseButtonX.Text = ((MainWindow)System.Windows.Application.Current.MainWindow).PixelPositionX.ToString();
                MidasAdCloseButtonY.Text = ((MainWindow)System.Windows.Application.Current.MainWindow).PixelPositionY.ToString();
                GoldAdCloseButtonColor.Text = GoldAdCloseButtonColor.Text.Split(';')[0] + ";" + GoldAdCloseButtonColor.Text.Split(';')[1] + ";" + ColorTranslator.ToHtml(((MainWindow)System.Windows.Application.Current.MainWindow).PixelColor);
                TroopAdCloseButtonColor.Text = TroopAdCloseButtonColor.Text.Split(';')[0] + ";" + TroopAdCloseButtonColor.Text.Split(';')[1] + ";" + ColorTranslator.ToHtml(((MainWindow)System.Windows.Application.Current.MainWindow).PixelColor);
                MidasAdCloseButtonColor.Text = MidasAdCloseButtonColor.Text.Split(';')[0] + ";" + MidasAdCloseButtonColor.Text.Split(';')[1] + ";" + ColorTranslator.ToHtml(((MainWindow)System.Windows.Application.Current.MainWindow).PixelColor);
                MidasAdCloseButton1.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(ColorTranslator.ToHtml(((MainWindow)System.Windows.Application.Current.MainWindow).PixelColor)));
            }));
        }

        private void LatestUsedAppButton1_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
            {
                ((MainWindow)System.Windows.Application.Current.MainWindow).PixelPositionX = int.Parse(LatestUsedAppButtonX.Text);
                ((MainWindow)System.Windows.Application.Current.MainWindow).PixelPositionY = int.Parse(LatestUsedAppButtonY.Text);
                //((MainWindow)System.Windows.Application.Current.MainWindow).PixelColor = ColorTranslator.FromHtml(LatestUsedAppButtonColor.Text);
            }));

            GetPixelPositionAndColor();

            System.Windows.Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
            {
                LatestUsedAppButtonX.Text = ((MainWindow)System.Windows.Application.Current.MainWindow).PixelPositionX.ToString();
                LatestUsedAppButtonY.Text = ((MainWindow)System.Windows.Application.Current.MainWindow).PixelPositionY.ToString();
                //LatestUsedAppButtonColor.Text = ColorTranslator.ToHtml(((MainWindow)System.Windows.Application.Current.MainWindow).PixelColor);
                //LatestUsedAppButton1.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(ColorTranslator.ToHtml(((MainWindow)System.Windows.Application.Current.MainWindow).PixelColor)));
            }));
        }

        private void CloseAllAppButton1_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
            {
                ((MainWindow)System.Windows.Application.Current.MainWindow).PixelPositionX = int.Parse(CloseAllAppButtonX.Text);
                ((MainWindow)System.Windows.Application.Current.MainWindow).PixelPositionY = int.Parse(CloseAllAppButtonY.Text);
                //((MainWindow)System.Windows.Application.Current.MainWindow).PixelColor = ColorTranslator.FromHtml(CloseAllAppButtonColor.Text);
            }));

            GetPixelPositionAndColor();

            System.Windows.Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
            {
                CloseAllAppButtonX.Text = ((MainWindow)System.Windows.Application.Current.MainWindow).PixelPositionX.ToString();
                CloseAllAppButtonY.Text = ((MainWindow)System.Windows.Application.Current.MainWindow).PixelPositionY.ToString();
                //CloseAllAppButtonColor.Text = ColorTranslator.ToHtml(((MainWindow)System.Windows.Application.Current.MainWindow).PixelColor);
                //CloseAllAppButton1.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(ColorTranslator.ToHtml(((MainWindow)System.Windows.Application.Current.MainWindow).PixelColor)));
            }));
        }

        private void NotRespondingButton1_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
            {
                ((MainWindow)System.Windows.Application.Current.MainWindow).PixelPositionX = int.Parse(NotRespondingButtonX.Text);
                ((MainWindow)System.Windows.Application.Current.MainWindow).PixelPositionY = int.Parse(NotRespondingButtonY.Text);
                ((MainWindow)System.Windows.Application.Current.MainWindow).PixelColor = ColorTranslator.FromHtml(NotRespondingButtonColor.Text);
            }));

            GetPixelPositionAndColor();

            System.Windows.Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
            {
                NotRespondingButtonX.Text = ((MainWindow)System.Windows.Application.Current.MainWindow).PixelPositionX.ToString();
                NotRespondingButtonY.Text = ((MainWindow)System.Windows.Application.Current.MainWindow).PixelPositionY.ToString();
                NotRespondingButtonColor.Text = ColorTranslator.ToHtml(((MainWindow)System.Windows.Application.Current.MainWindow).PixelColor);
                NotRespondingButton1.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(ColorTranslator.ToHtml(((MainWindow)System.Windows.Application.Current.MainWindow).PixelColor)));
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

        private void LeftAdCloseButton1_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
            {
                ((MainWindow)System.Windows.Application.Current.MainWindow).PixelPositionX = int.Parse(LeftAdCloseButtonX.Text);
                ((MainWindow)System.Windows.Application.Current.MainWindow).PixelPositionY = int.Parse(LeftAdCloseButtonY.Text);
                ((MainWindow)System.Windows.Application.Current.MainWindow).PixelColor = ColorTranslator.FromHtml(LeftAdCloseButtonColor.Text.Split(';')[0]);
            }));

            GetPixelPositionAndColor();

            System.Windows.Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
            {
                LeftAdCloseButtonX.Text = ((MainWindow)System.Windows.Application.Current.MainWindow).PixelPositionX.ToString();
                LeftAdCloseButtonY.Text = ((MainWindow)System.Windows.Application.Current.MainWindow).PixelPositionY.ToString();
                LeftAdCloseButtonColor.Text = ColorTranslator.ToHtml(((MainWindow)System.Windows.Application.Current.MainWindow).PixelColor) + ";" + LeftAdCloseButtonColor.Text.Split(';')[1];
                RightAdCloseButtonColor.Text = ColorTranslator.ToHtml(((MainWindow)System.Windows.Application.Current.MainWindow).PixelColor) + ";" + RightAdCloseButtonColor.Text.Split(';')[1];
                LeftAdCloseButton1.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(ColorTranslator.ToHtml(((MainWindow)System.Windows.Application.Current.MainWindow).PixelColor)));
            }));
        }

        private void RightAdCloseButton1_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
            {
                ((MainWindow)System.Windows.Application.Current.MainWindow).PixelPositionX = int.Parse(RightAdCloseButtonX.Text);
                ((MainWindow)System.Windows.Application.Current.MainWindow).PixelPositionY = int.Parse(RightAdCloseButtonY.Text);
                ((MainWindow)System.Windows.Application.Current.MainWindow).PixelColor = ColorTranslator.FromHtml(RightAdCloseButtonColor.Text.Split(';')[1]);
            }));

            GetPixelPositionAndColor();

            System.Windows.Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
            {
                RightAdCloseButtonX.Text = ((MainWindow)System.Windows.Application.Current.MainWindow).PixelPositionX.ToString();
                RightAdCloseButtonY.Text = ((MainWindow)System.Windows.Application.Current.MainWindow).PixelPositionY.ToString();
                LeftAdCloseButtonColor.Text = LeftAdCloseButtonColor.Text.Split(';')[0] + ";" + ColorTranslator.ToHtml(((MainWindow)System.Windows.Application.Current.MainWindow).PixelColor);
                RightAdCloseButtonColor.Text = RightAdCloseButtonColor.Text.Split(';')[0] + ";" + ColorTranslator.ToHtml(((MainWindow)System.Windows.Application.Current.MainWindow).PixelColor);
                RightAdCloseButton1.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(ColorTranslator.ToHtml(((MainWindow)System.Windows.Application.Current.MainWindow).PixelColor)));
            }));
        }

        private void ChangeButtonsColors()
        {
            AppLocationButton1.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(AppLocationColor.Text.Split(';')[0]));
            ShopButton1.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(ShopButtonColor.Text));
            CollectButton1.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(CollectButtonColor.Text));
            GoldChestBox1.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(GoldChestBoxColor.Text.Split(';')[0]));
            BattleLevelButton1.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(BattleLevelButtonColor.Text.Split(';')[0]));
            SkillButton1.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(SkillButtonColor.Text));
            SpeedButton1.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(SpeedButtonColor.Text));
            ContinueButton1.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(ContinueButtonColor.Text));
            VictoryDefeat1.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(VictoryDefeatColor.Text.Split(';')[0]));
            GoldButtonBackground1.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(GoldButtonBackgroundColor.Text.Split(';')[0]));
            GoldButtonImage1.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(GoldButtonImageColor.Text.Split(';')[0]));
            NextButton1.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(NextButtonColor.Text));
            GoldAdCloseButton1.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(GoldAdCloseButtonColor.Text.Split(';')[0]));
            TroopAdCloseButton1.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(TroopAdCloseButtonColor.Text.Split(';')[1]));
            MidasAdCloseButton1.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(MidasAdCloseButtonColor.Text.Split(';')[2]));
            //LatestUsedAppButton1.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(LatestUsedAppButtonColor.Text));
            //CloseAllAppButton1.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(CloseAllAppButtonColor.Text));
            NotRespondingButton1.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(NotRespondingButtonColor.Text));
            NoGoldButton1.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(NoGoldColor.Text));

            AppLocationButton2.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(AppLocationColor.Text.Split(';')[1]));
            GoldChestBox2.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(GoldChestBoxColor.Text.Split(';')[1]));
            BattleLevelButton2.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(BattleLevelButtonColor.Text.Split(';')[1]));
            VictoryDefeat2.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(VictoryDefeatColor.Text.Split(';')[1]));
            GoldButtonBackground2.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(GoldButtonBackgroundColor.Text.Split(';')[1]));

            BattleLevelButton3.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(BattleLevelButtonColor.Text.Split(';')[2]));
            BattleLevelButton4.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(BattleLevelButtonColor.Text.Split(';')[3]));

            LeftAdCloseButton1.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(LeftAdCloseButtonColor.Text.Split(';')[0]));
            RightAdCloseButton1.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(LeftAdCloseButtonColor.Text.Split(';')[1]));
        }

        private void LoadButton_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog();
            openFileDialog.DefaultExt = "txt";
            openFileDialog.Filter = "Text Files (*.txt)|*.txt";
            openFileDialog.ShowDialog();
            if (openFileDialog.FileName.Length > 0)
            {
                string[] lines = File.ReadAllLines(openFileDialog.FileName);

                int tempnum;
                if (int.TryParse(lines[0].Split(',')[0], out tempnum)) // 구버전 - 추후 제거
                {
                    AppLocationX.Text = lines[0].Split(',')[0];
                    AppLocationY.Text = lines[0].Split(',')[1];
                    AppLocationColor.Text = lines[0].Split(',')[2];

                    HomeButtonX.Text = lines[1].Split(',')[0];
                    ShopButtonX.Text = lines[1].Split(',')[1];
                    ShopButtonY.Text = lines[1].Split(',')[2];
                    ShopButtonColor.Text = lines[1].Split(',')[3];

                    CollectButtonX.Text = lines[2].Split(',')[0];
                    CollectButtonY.Text = lines[2].Split(',')[1];
                    CollectButtonColor.Text = lines[2].Split(',')[2];

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

                    ContinueButtonX.Text = lines[7].Split(',')[0];
                    ContinueButtonY.Text = lines[7].Split(',')[1];
                    ContinueButtonColor.Text = lines[7].Split(',')[2];

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

                        GoldAdCloseButtonX.Text = lines[13].Split(',')[0];
                        GoldAdCloseButtonY.Text = lines[13].Split(',')[1];
                        TroopAdCloseButtonX.Text = lines[13].Split(',')[0];
                        TroopAdCloseButtonY.Text = lines[13].Split(',')[2];
						// 예외발생 임시처리 나중에 삭제할것
                        if (lines[13].Split(',').Length < 5)
                        {
                            GoldAdCloseButtonColor.Text = lines[13].Split(',')[3] + ";#e9e9d8".ToUpper();
                            TroopAdCloseButtonColor.Text = lines[13].Split(',')[3] + ";#e9e9d8".ToUpper();

                            MidasAdCloseButtonX.Text = lines[13].Split(',')[0];
                            MidasAdCloseButtonY.Text = 262.ToString();
                            MidasAdCloseButtonColor.Text = lines[13].Split(',')[3] + ";#e9e9d8".ToUpper();
                        }
                        else
                        {
                            MidasAdCloseButtonX.Text = lines[13].Split(',')[0];
                            MidasAdCloseButtonY.Text = lines[13].Split(',')[3];

                            GoldAdCloseButtonColor.Text = lines[13].Split(',')[4];
                            TroopAdCloseButtonColor.Text = lines[13].Split(',')[4];
                            MidasAdCloseButtonColor.Text = lines[13].Split(',')[4];
                        }

                        LeftAdCloseButtonX.Text = lines[14].Split(',')[0];
                        LeftAdCloseButtonY.Text = lines[14].Split(',')[2];
                        RightAdCloseButtonX.Text = lines[14].Split(',')[1];
                        RightAdCloseButtonY.Text = lines[14].Split(',')[2];
                        // 예외발생 임시처리 나중에 삭제할것
                        if (lines[14].Split(',').Length < 4)
                        {
                            LeftAdCloseButtonColor.Text = "#4c4c4f;#3c4043".ToUpper();
                            RightAdCloseButtonColor.Text = "#4c4c4f;#3c4043".ToUpper();
                        }
                        else
                        {
                            LeftAdCloseButtonColor.Text = lines[14].Split(',')[3];
                            RightAdCloseButtonColor.Text = lines[14].Split(',')[3];
                        }

                        LatestUsedAppButtonX.Text = lines[15].Split(',')[0];
                        LatestUsedAppButtonY.Text = lines[15].Split(',')[1];
                        //LatestUsedAppButtonColor.Text = lines[15].Split(',')[2];
                    }
                    else
                    {
                        GoldAdCloseButtonX.Text = lines[12].Split(',')[0];
                        GoldAdCloseButtonY.Text = lines[12].Split(',')[1];
                        TroopAdCloseButtonX.Text = lines[12].Split(',')[0];
                        TroopAdCloseButtonY.Text = lines[12].Split(',')[2];
                        // 예외발생 임시처리 나중에 삭제할것
                        if (lines[12].Split(',').Length < 5)
                        {
                            GoldAdCloseButtonColor.Text = lines[12].Split(',')[3] + ";#e9e9d8".ToUpper();
                            TroopAdCloseButtonColor.Text = lines[12].Split(',')[3] + ";#e9e9d8".ToUpper();

                            MidasAdCloseButtonX.Text = lines[12].Split(',')[0];
                            MidasAdCloseButtonY.Text = 262.ToString();
                            MidasAdCloseButtonColor.Text = lines[12].Split(',')[3] + ";#e9e9d8".ToUpper();
                        }
                        else
                        {
                            MidasAdCloseButtonX.Text = lines[12].Split(',')[0];
                            MidasAdCloseButtonY.Text = lines[12].Split(',')[3];

                            GoldAdCloseButtonColor.Text = lines[12].Split(',')[4];
                            TroopAdCloseButtonColor.Text = lines[12].Split(',')[4];
                            MidasAdCloseButtonColor.Text = lines[12].Split(',')[4];
                        }

                        LeftAdCloseButtonX.Text = lines[13].Split(',')[0];
                        LeftAdCloseButtonY.Text = lines[13].Split(',')[2];
                        RightAdCloseButtonX.Text = lines[13].Split(',')[1];
                        RightAdCloseButtonY.Text = lines[13].Split(',')[2];
                        // 예외발생 임시처리 나중에 삭제할것
                        if (lines[13].Split(',').Length < 4)
                        {
                            LeftAdCloseButtonColor.Text = "#4c4c4f;#3c4043".ToUpper();
                            RightAdCloseButtonColor.Text = "#4c4c4f;#3c4043".ToUpper();
                        }
                        else
                        {
                            LeftAdCloseButtonColor.Text = lines[13].Split(',')[3];
                            RightAdCloseButtonColor.Text = lines[13].Split(',')[3];
                        }

                        LatestUsedAppButtonX.Text = lines[14].Split(',')[0];
                        LatestUsedAppButtonY.Text = lines[14].Split(',')[1];
                        //LatestUsedAppButtonColor.Text = lines[14].Split(',')[2];
                    }
                }
                else // 신버전
                {
                    foreach (string line in lines)
                    {
                        string[] listitem = line.Split(',');

                        switch (listitem[0].ToLower())
                        {
                            case ("applocation"):
                                AppLocationX.Text = listitem[1];
                                AppLocationY.Text = listitem[2];
                                AppLocationColor.Text = listitem[3];
                                break;
                            case ("shopbutton"):
                                HomeButtonX.Text = listitem[1];
                                ShopButtonX.Text = listitem[2];
                                ShopButtonY.Text = listitem[3];
                                ShopButtonColor.Text = listitem[4];
                                break;
                            case ("collectbutton"):
                                CollectButtonX.Text = listitem[1];
                                CollectButtonY.Text = listitem[2];
                                CollectButtonColor.Text = listitem[3];
                                break;
                            case ("goldchestbox"):
                                GoldChestBoxX.Text = listitem[1];
                                GoldChestBoxY.Text = listitem[2];
                                GoldChestBoxColor.Text = listitem[3];
                                break;
                            case ("battlelevelbutton"):
                                BattleLevelButtonX.Text = listitem[1];
                                BattleLevelButtonY.Text = listitem[2];
                                BattleLevelButtonColor.Text = listitem[3];
                                break;
                            case ("skillbutton"):
                                SkillButtonX.Text = listitem[1];
                                SkillButtonY.Text = listitem[2];
                                SkillButtonColor.Text = listitem[3];
                                break;
                            case ("speedbutton"):
                                SpeedButtonX.Text = listitem[1];
                                SpeedButtonY.Text = listitem[2];
                                SpeedButtonColor.Text = listitem[3];
                                break;
                            case ("continuebutton"):
                                ContinueButtonX.Text = listitem[1];
                                ContinueButtonY.Text = listitem[2];
                                ContinueButtonColor.Text = listitem[3];
                                break;
                            case ("victorydefeat"):
                                VictoryDefeatX.Text = listitem[1];
                                VictoryDefeatY.Text = listitem[2];
                                VictoryDefeatColor.Text = listitem[3];
                                break;
                            case ("goldbuttonbackground"):
                                GoldButtonBackgroundX.Text = listitem[1];
                                GoldButtonBackgroundY.Text = listitem[2];
                                GoldButtonBackgroundColor.Text = listitem[3];
                                break;
                            case ("goldbuttonimage"):
                                GoldButtonImageX.Text = listitem[1];
                                GoldButtonImageY.Text = listitem[2];
                                GoldButtonImageColor.Text = listitem[3];
                                break;
                            case ("nextbutton"):
                                NextButtonX.Text = listitem[1];
                                NextButtonY.Text = listitem[2];
                                NextButtonColor.Text = listitem[3];
                                break;
                            case ("nogold"):
                                NoGoldX.Text = listitem[1];
                                NoGoldY.Text = listitem[2];
                                NoGoldColor.Text = listitem[3];
                                break;
                            case ("gameadclosebutton"):
                                GoldAdCloseButtonX.Text = listitem[1];
                                TroopAdCloseButtonX.Text = listitem[1];
                                GoldAdCloseButtonY.Text = listitem[2];
                                TroopAdCloseButtonY.Text = listitem[3];
								// 예외발생 임시처리 나중에 삭제할것
                                if (listitem.Length == 5)
                                {
                                    GoldAdCloseButtonColor.Text = listitem[4] + ";#e9e9d8".ToUpper();
                                    TroopAdCloseButtonColor.Text = listitem[4] + ";#e9e9d8".ToUpper();

                                    MidasAdCloseButtonX.Text = listitem[1];
                                    MidasAdCloseButtonY.Text = 262.ToString();
                                    MidasAdCloseButtonColor.Text = listitem[4] + ";#e9e9d8".ToUpper();
                                }
                                else
                                {
                                    MidasAdCloseButtonX.Text = listitem[1];
                                    MidasAdCloseButtonY.Text = listitem[4];

                                    GoldAdCloseButtonColor.Text = listitem[5];
                                    TroopAdCloseButtonColor.Text = listitem[5];
                                    MidasAdCloseButtonColor.Text = listitem[5];
                                }
                                break;
                            case ("googleadclosebutton"):
                                LeftAdCloseButtonX.Text = listitem[1];
                                RightAdCloseButtonX.Text = listitem[2];
                                LeftAdCloseButtonY.Text = listitem[3];
                                RightAdCloseButtonY.Text = listitem[3];
                                LeftAdCloseButtonColor.Text = listitem[4];
                                RightAdCloseButtonColor.Text = listitem[4];
                                break;
                            case ("latestusedsppbutton"):
                                LatestUsedAppButtonX.Text = listitem[1];
                                LatestUsedAppButtonY.Text = listitem[2];
                                //LatestUsedAppButtonColor.Text = listitem[3];
                                break;
                            case ("closeallappbutton"):
                                CloseAllAppButtonX.Text = listitem[1];
                                CloseAllAppButtonY.Text = listitem[2];
                                //CloseAllAppButtonColor.Text = listitem[3];
                                break;
                            case ("notrespondingbutton"):
                                NotRespondingButtonX.Text = listitem[1];
                                NotRespondingButtonY.Text = listitem[2];
                                NotRespondingButtonColor.Text = listitem[3];
                                break;
                        }
                    }
                }

                ChangeButtonsColors();
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.SaveFileDialog saveFileDialog = new System.Windows.Forms.SaveFileDialog();
            saveFileDialog.Filter = "Text File (*.txt)|*.txt";
            if (saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                using (StreamWriter streamWriter = new StreamWriter(saveFileDialog.FileName, false))
                {
                    streamWriter.WriteLine("AppLocation," + AppLocationX.Text + "," + AppLocationY.Text + "," + AppLocationColor.Text);
                    streamWriter.WriteLine("ShopButton," + HomeButtonX.Text + "," + ShopButtonX.Text + "," + ShopButtonY.Text + "," + ShopButtonColor.Text);
                    streamWriter.WriteLine("GoldChestBox," + GoldChestBoxX.Text + "," + GoldChestBoxY.Text + "," + GoldChestBoxColor.Text);
                    streamWriter.WriteLine("CollectButton," + CollectButtonX.Text + "," + CollectButtonY.Text + "," + CollectButtonColor.Text);
                    streamWriter.WriteLine("BattleLevelButton," + BattleLevelButtonX.Text + "," + BattleLevelButtonY.Text + "," + BattleLevelButtonColor.Text);
                    streamWriter.WriteLine("SkillButton," + SkillButtonX.Text + "," + SkillButtonY.Text + "," + SkillButtonColor.Text);
                    streamWriter.WriteLine("SpeedButton," + SpeedButtonX.Text + "," + SpeedButtonY.Text + "," + SpeedButtonColor.Text);
                    streamWriter.WriteLine("ContinueButton," + ContinueButtonX.Text + "," + ContinueButtonY.Text + "," + ContinueButtonColor.Text);
                    streamWriter.WriteLine("VictoryDefeat," + VictoryDefeatX.Text + "," + VictoryDefeatY.Text + "," + VictoryDefeatColor.Text);
                    streamWriter.WriteLine("GoldButtonBackground," + GoldButtonBackgroundX.Text + "," + GoldButtonBackgroundY.Text + "," + GoldButtonBackgroundColor.Text);
                    streamWriter.WriteLine("GoldButtonImage," + GoldButtonImageX.Text + "," + GoldButtonImageY.Text + "," + GoldButtonImageColor.Text);
                    streamWriter.WriteLine("NextButton," + NextButtonX.Text + "," + NextButtonY.Text + "," + NextButtonColor.Text);
                    streamWriter.WriteLine("NoGold," + NoGoldX.Text + "," + NoGoldY.Text + "," + NoGoldColor.Text);
                    streamWriter.WriteLine("GameAdCloseButton," + (int)((int.Parse(GoldAdCloseButtonX.Text) + int.Parse(TroopAdCloseButtonX.Text) + int.Parse(MidasAdCloseButtonX.Text)) / 3) + "," + GoldAdCloseButtonY.Text + "," + TroopAdCloseButtonY.Text + "," + MidasAdCloseButtonY.Text + "," + GoldAdCloseButtonColor.Text);
                    streamWriter.WriteLine("GoogleAdCloseButton," + LeftAdCloseButtonX.Text + "," + RightAdCloseButtonX.Text + "," + (int)((int.Parse(LeftAdCloseButtonY.Text) + int.Parse(RightAdCloseButtonY.Text)) / 2) + "," + LeftAdCloseButtonColor.Text);
                    streamWriter.WriteLine("LatestUsedAppButton," + LatestUsedAppButtonX.Text + "," + LatestUsedAppButtonY.Text);
                    streamWriter.WriteLine("CloseAllAppButton," + CloseAllAppButtonX.Text + "," + CloseAllAppButtonY.Text);
                    streamWriter.WriteLine("NotRespondingButton," + NotRespondingButtonX.Text + "," + NotRespondingButtonY.Text + "," + NotRespondingButtonColor.Text);
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

            CollectButtonX.Text = 195.ToString();
            CollectButtonY.Text = 680.ToString();
            CollectButtonColor.Text = "#fdbb00".ToUpper();

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

            ContinueButtonX.Text = 215.ToString();
            ContinueButtonY.Text = 455.ToString();
            ContinueButtonColor.Text = "#fdbb00".ToUpper();

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

            GoldAdCloseButtonX.Text = 496.ToString();
            TroopAdCloseButtonX.Text = 496.ToString();
            GoldAdCloseButtonY.Text = 180.ToString();
            TroopAdCloseButtonY.Text = 190.ToString();
            MidasAdCloseButtonY.Text = 262.ToString();
            GoldAdCloseButtonColor.Text = "#efe7d6;#e9e9d8;#e9e9d8".ToUpper();
            TroopAdCloseButtonColor.Text = "#efe7d6;#e9e9d8;#e9e9d8".ToUpper();
            MidasAdCloseButtonColor.Text = "#efe7d6;#e9e9d8;#e9e9d8".ToUpper();

            LeftAdCloseButtonX.Text = 45.ToString();
            RightAdCloseButtonX.Text = 513.ToString();
            LeftAdCloseButtonY.Text = 63.ToString();
            RightAdCloseButtonY.Text = 63.ToString();
            LeftAdCloseButtonColor.Text = "#4c4c4f;#3c4043".ToUpper();
            RightAdCloseButtonColor.Text = "#4c4c4f;#3c4043".ToUpper();

            LatestUsedAppButtonX.Text = 580.ToString();
            LatestUsedAppButtonY.Text = 1000.ToString();
            //LatestUsedAppButtonColor.Text = "#009688".ToUpper();

            CloseAllAppButtonX.Text = 501.ToString();
            CloseAllAppButtonY.Text = 150.ToString();
            //CloseAllAppButtonColor.Text = "#009688".ToUpper();

            NotRespondingButtonX.Text = 79.ToString();
            NotRespondingButtonY.Text = 510.ToString();
            NotRespondingButtonColor.Text = "#009688".ToUpper();

            NoGoldX.Text = 320.ToString(); ;
            NoGoldY.Text = 646.ToString(); ;
            NoGoldColor.Text = "#dfd6be".ToUpper();

            ChangeButtonsColors();
        }

        private void ApplyButton_Click(object sender, RoutedEventArgs e)
        {
            using (StreamWriter streamWriter = new StreamWriter($@"pixel.txt", false))
            {
                System.Windows.Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
                {
                    ((MainWindow)System.Windows.Application.Current.MainWindow).AppLocationX = int.Parse(AppLocationX.Text);
                    ((MainWindow)System.Windows.Application.Current.MainWindow).AppLocationY = int.Parse(AppLocationY.Text);
                    ((MainWindow)System.Windows.Application.Current.MainWindow).AppLocationColor = AppLocationColor.Text;
                    streamWriter.WriteLine("AppLocation," + AppLocationX.Text + "," + AppLocationY.Text + "," + AppLocationColor.Text);

                    ((MainWindow)System.Windows.Application.Current.MainWindow).HomeButtonX = int.Parse(HomeButtonX.Text);
                    ((MainWindow)System.Windows.Application.Current.MainWindow).ShopButtonX = int.Parse(ShopButtonX.Text);
                    ((MainWindow)System.Windows.Application.Current.MainWindow).ShopButtonY = int.Parse(ShopButtonY.Text);
                    ((MainWindow)System.Windows.Application.Current.MainWindow).ShopButtonColor = ShopButtonColor.Text;
                    streamWriter.WriteLine("ShopButton," + HomeButtonX.Text + "," + ShopButtonX.Text + "," + ShopButtonY.Text + "," + ShopButtonColor.Text);

                    ((MainWindow)System.Windows.Application.Current.MainWindow).GoldChestBoxX = int.Parse(GoldChestBoxX.Text);
                    ((MainWindow)System.Windows.Application.Current.MainWindow).GoldChestBoxY = int.Parse(GoldChestBoxY.Text);
                    ((MainWindow)System.Windows.Application.Current.MainWindow).GoldChestBoxColor = GoldChestBoxColor.Text;
                    streamWriter.WriteLine("GoldChestBox," + GoldChestBoxX.Text + "," + GoldChestBoxY.Text + "," + GoldChestBoxColor.Text);

                    ((MainWindow)System.Windows.Application.Current.MainWindow).CollectButtonX = int.Parse(CollectButtonX.Text);
                    ((MainWindow)System.Windows.Application.Current.MainWindow).CollectButtonY = int.Parse(CollectButtonY.Text);
                    ((MainWindow)System.Windows.Application.Current.MainWindow).CollectButtonColor = CollectButtonColor.Text;
                    streamWriter.WriteLine("CollectButton," + CollectButtonX.Text + "," + CollectButtonY.Text + "," + CollectButtonColor.Text);

                    ((MainWindow)System.Windows.Application.Current.MainWindow).BattleLevelButtonX = int.Parse(BattleLevelButtonX.Text);
                    ((MainWindow)System.Windows.Application.Current.MainWindow).BattleLevelButtonY = int.Parse(BattleLevelButtonY.Text);
                    ((MainWindow)System.Windows.Application.Current.MainWindow).BattleLevelButtonColor = BattleLevelButtonColor.Text;
                    streamWriter.WriteLine("BattleLevelButton," + BattleLevelButtonX.Text + "," + BattleLevelButtonY.Text + "," + BattleLevelButtonColor.Text);

                    ((MainWindow)System.Windows.Application.Current.MainWindow).SkillButtonX = int.Parse(SkillButtonX.Text);
                    ((MainWindow)System.Windows.Application.Current.MainWindow).SkillButtonY = int.Parse(SkillButtonY.Text);
                    ((MainWindow)System.Windows.Application.Current.MainWindow).SkillButtonColor = SkillButtonColor.Text;
                    streamWriter.WriteLine("SkillButton," + SkillButtonX.Text + "," + SkillButtonY.Text + "," + SkillButtonColor.Text);

                    ((MainWindow)System.Windows.Application.Current.MainWindow).SpeedButtonX = int.Parse(SpeedButtonX.Text);
                    ((MainWindow)System.Windows.Application.Current.MainWindow).SpeedButtonY = int.Parse(SpeedButtonY.Text);
                    ((MainWindow)System.Windows.Application.Current.MainWindow).SpeedButtonColor = SpeedButtonColor.Text;
                    streamWriter.WriteLine("SpeedButton," + SpeedButtonX.Text + "," + SpeedButtonY.Text + "," + SpeedButtonColor.Text);

                    ((MainWindow)System.Windows.Application.Current.MainWindow).ContinueButtonX = int.Parse(ContinueButtonX.Text);
                    ((MainWindow)System.Windows.Application.Current.MainWindow).ContinueButtonY = int.Parse(ContinueButtonY.Text);
                    ((MainWindow)System.Windows.Application.Current.MainWindow).ContinueButtonColor = ContinueButtonColor.Text;
                    streamWriter.WriteLine("ContinueButton," + ContinueButtonX.Text + "," + ContinueButtonY.Text + "," + ContinueButtonColor.Text);

                    ((MainWindow)System.Windows.Application.Current.MainWindow).VictoryDefeatX = int.Parse(VictoryDefeatX.Text);
                    ((MainWindow)System.Windows.Application.Current.MainWindow).VictoryDefeatY = int.Parse(VictoryDefeatY.Text);
                    ((MainWindow)System.Windows.Application.Current.MainWindow).VictoryDefeatColor = VictoryDefeatColor.Text;
                    streamWriter.WriteLine("VictoryDefeat," + VictoryDefeatX.Text + "," + VictoryDefeatY.Text + "," + VictoryDefeatColor.Text);

                    ((MainWindow)System.Windows.Application.Current.MainWindow).GoldButtonBackgroundX = int.Parse(GoldButtonBackgroundX.Text);
                    ((MainWindow)System.Windows.Application.Current.MainWindow).GoldButtonBackgroundY = int.Parse(GoldButtonBackgroundY.Text);
                    ((MainWindow)System.Windows.Application.Current.MainWindow).GoldButtonBackgroundColor = GoldButtonBackgroundColor.Text;
                    streamWriter.WriteLine("GoldButtonBackground," + GoldButtonBackgroundX.Text + "," + GoldButtonBackgroundY.Text + "," + GoldButtonBackgroundColor.Text);

                    ((MainWindow)System.Windows.Application.Current.MainWindow).GoldButtonImageX = int.Parse(GoldButtonImageX.Text);
                    ((MainWindow)System.Windows.Application.Current.MainWindow).GoldButtonImageY = int.Parse(GoldButtonImageY.Text);
                    ((MainWindow)System.Windows.Application.Current.MainWindow).GoldButtonImageColor = GoldButtonImageColor.Text;
                    streamWriter.WriteLine("GoldButtonImage," + GoldButtonImageX.Text + "," + GoldButtonImageY.Text + "," + GoldButtonImageColor.Text);

                    ((MainWindow)System.Windows.Application.Current.MainWindow).NextButtonX = int.Parse(NextButtonX.Text);
                    ((MainWindow)System.Windows.Application.Current.MainWindow).NextButtonY = int.Parse(NextButtonY.Text);
                    ((MainWindow)System.Windows.Application.Current.MainWindow).NextButtonColor = NextButtonColor.Text;
                    streamWriter.WriteLine("NextButton," + NextButtonX.Text + "," + NextButtonY.Text + "," + NextButtonColor.Text);

                    ((MainWindow)System.Windows.Application.Current.MainWindow).NoGoldX = int.Parse(NoGoldX.Text);
                    ((MainWindow)System.Windows.Application.Current.MainWindow).NoGoldY = int.Parse(NoGoldY.Text);
                    ((MainWindow)System.Windows.Application.Current.MainWindow).NoGoldColor = NoGoldColor.Text;
                    streamWriter.WriteLine("NoGold," + NoGoldX.Text + "," + NoGoldY.Text + "," + NoGoldColor.Text);

                    ((MainWindow)System.Windows.Application.Current.MainWindow).GameAdCloseButtonX = (int.Parse(GoldAdCloseButtonX.Text) + int.Parse(TroopAdCloseButtonX.Text) + int.Parse(MidasAdCloseButtonX.Text)) / 3;
                    ((MainWindow)System.Windows.Application.Current.MainWindow).GoldAdCloseButtonY = int.Parse(GoldAdCloseButtonY.Text);
                    ((MainWindow)System.Windows.Application.Current.MainWindow).TroopAdCloseButtonY = int.Parse(TroopAdCloseButtonY.Text);
                    ((MainWindow)System.Windows.Application.Current.MainWindow).MidasAdCloseButtonY = int.Parse(MidasAdCloseButtonY.Text);
                    ((MainWindow)System.Windows.Application.Current.MainWindow).GameAdCloseButtonColor = GoldAdCloseButtonColor.Text;
                    streamWriter.WriteLine("GameAdCloseButton," + (int)((int.Parse(GoldAdCloseButtonX.Text) + int.Parse(TroopAdCloseButtonX.Text) + int.Parse(MidasAdCloseButtonX.Text)) / 3) + "," + GoldAdCloseButtonY.Text + "," + TroopAdCloseButtonY.Text + "," + MidasAdCloseButtonY.Text + "," + GoldAdCloseButtonColor.Text);

                    ((MainWindow)System.Windows.Application.Current.MainWindow).LeftAdCloseButtonX = int.Parse(LeftAdCloseButtonX.Text);
                    ((MainWindow)System.Windows.Application.Current.MainWindow).RightAdCloseButtonX = int.Parse(RightAdCloseButtonX.Text);
                    ((MainWindow)System.Windows.Application.Current.MainWindow).GoogleAdCloseButtonY = (int.Parse(LeftAdCloseButtonY.Text) + int.Parse(RightAdCloseButtonY.Text)) / 2;
                    ((MainWindow)System.Windows.Application.Current.MainWindow).GoogleAdCloseButtonColor = LeftAdCloseButtonColor.Text;
                    streamWriter.WriteLine("GoogleAdCloseButton," + LeftAdCloseButtonX.Text + "," + RightAdCloseButtonX.Text + "," + (int)((int.Parse(LeftAdCloseButtonY.Text) + int.Parse(RightAdCloseButtonY.Text)) / 2) + "," + LeftAdCloseButtonColor.Text);

                    ((MainWindow)System.Windows.Application.Current.MainWindow).LatestUsedAppButtonX = int.Parse(LatestUsedAppButtonX.Text);
                    ((MainWindow)System.Windows.Application.Current.MainWindow).LatestUsedAppButtonY = int.Parse(LatestUsedAppButtonY.Text);
                    //((MainWindow)System.Windows.Application.Current.MainWindow).LatestUsedAppButtonColor = LatestUsedAppButtonColor.Text;
                    streamWriter.WriteLine("LatestUsedAppButton," + LatestUsedAppButtonX.Text + "," + LatestUsedAppButtonY.Text);

                    ((MainWindow)System.Windows.Application.Current.MainWindow).CloseAllAppButtonX = int.Parse(CloseAllAppButtonX.Text);
                    ((MainWindow)System.Windows.Application.Current.MainWindow).CloseAllAppButtonY = int.Parse(CloseAllAppButtonY.Text);
                    //((MainWindow)System.Windows.Application.Current.MainWindow).CloseAllAppButtonColor = CloseAllAppButtonColor.Text;
                    streamWriter.WriteLine("CloseAllAppButton," + CloseAllAppButtonX.Text + "," + CloseAllAppButtonY.Text);

                    ((MainWindow)System.Windows.Application.Current.MainWindow).NotRespondingButtonX = int.Parse(NotRespondingButtonX.Text);
                    ((MainWindow)System.Windows.Application.Current.MainWindow).NotRespondingButtonY = int.Parse(NotRespondingButtonY.Text);
                    ((MainWindow)System.Windows.Application.Current.MainWindow).NotRespondingButtonColor = NotRespondingButtonColor.Text;
                    streamWriter.WriteLine("NotRespondingButton," + NotRespondingButtonX.Text + "," + NotRespondingButtonY.Text);
                }));
            }

            this.Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void AppLocationX_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            int caretIndex = AppLocationX.CaretIndex;
            AppLocationX.Text = Regex.Replace((e.Source as TextBox).Text, RegExClass.Number, "");
            AppLocationX.Select(caretIndex, 0);
        }

        private void HomeButtonX_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            int caretIndex = HomeButtonX.CaretIndex;
            HomeButtonX.Text = Regex.Replace((e.Source as TextBox).Text, RegExClass.Number, "");
            HomeButtonX.Select(caretIndex, 0);
        }

        private void ShopButtonX_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            int caretIndex = ShopButtonX.CaretIndex;
            ShopButtonX.Text = Regex.Replace((e.Source as TextBox).Text, RegExClass.Number, "");
            ShopButtonX.Select(caretIndex, 0);
        }

        private void CollectButtonX_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            int caretIndex = CollectButtonX.CaretIndex;
            CollectButtonX.Text = Regex.Replace((e.Source as TextBox).Text, RegExClass.Number, "");
            CollectButtonX.Select(caretIndex, 0);
        }

        private void GoldChestBoxX_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            int caretIndex = GoldChestBoxX.CaretIndex;
            GoldChestBoxX.Text = Regex.Replace((e.Source as TextBox).Text, RegExClass.Number, "");
            GoldChestBoxX.Select(caretIndex, 0);
        }

        private void BattleLevelButtonX_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            int caretIndex = BattleLevelButtonX.CaretIndex;
            BattleLevelButtonX.Text = Regex.Replace((e.Source as TextBox).Text, RegExClass.Number, "");
            BattleLevelButtonX.Select(caretIndex, 0);
        }

        private void SkillButtonX_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            int caretIndex = SkillButtonX.CaretIndex;
            SkillButtonX.Text = Regex.Replace((e.Source as TextBox).Text, RegExClass.Number, "");
            SkillButtonX.Select(caretIndex, 0);
        }

        private void SpeedButtonX_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            int caretIndex = SpeedButtonX.CaretIndex;
            SpeedButtonX.Text = Regex.Replace((e.Source as TextBox).Text, RegExClass.Number, "");
            SpeedButtonX.Select(caretIndex, 0);
        }

        private void ContinueButtonX_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            int caretIndex = ContinueButtonX.CaretIndex;
            ContinueButtonX.Text = Regex.Replace((e.Source as TextBox).Text, RegExClass.Number, "");
            ContinueButtonX.Select(caretIndex, 0);
        }

        private void VictoryDefeatX_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            int caretIndex = VictoryDefeatX.CaretIndex;
            VictoryDefeatX.Text = Regex.Replace((e.Source as TextBox).Text, RegExClass.Number, "");
            VictoryDefeatX.Select(caretIndex, 0);
        }

        private void GoldButtonBackgroundX_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            int caretIndex = GoldButtonBackgroundX.CaretIndex;
            GoldButtonBackgroundX.Text = Regex.Replace((e.Source as TextBox).Text, RegExClass.Number, "");
            GoldButtonBackgroundX.Select(caretIndex, 0);
        }

        private void GoldButtonImageX_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            int caretIndex = GoldButtonImageX.CaretIndex;
            GoldButtonImageX.Text = Regex.Replace((e.Source as TextBox).Text, RegExClass.Number, "");
            GoldButtonImageX.Select(caretIndex, 0);
        }

        private void NextButtonX_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            int caretIndex = NextButtonX.CaretIndex;
            NextButtonX.Text = Regex.Replace((e.Source as TextBox).Text, RegExClass.Number, "");
            NextButtonX.Select(caretIndex, 0);
        }

        private void NoGoldX_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            int caretIndex = NoGoldX.CaretIndex;
            NoGoldX.Text = Regex.Replace((e.Source as TextBox).Text, RegExClass.Number, "");
            NoGoldX.Select(caretIndex, 0);
        }

        private void GoldAdCloseButtonX_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            int caretIndex = GoldAdCloseButtonX.CaretIndex;
            GoldAdCloseButtonX.Text = Regex.Replace((e.Source as TextBox).Text, RegExClass.Number, "");
            GoldAdCloseButtonX.Select(caretIndex, 0);
        }

        private void TroopAdCloseButtonX_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            int caretIndex = TroopAdCloseButtonX.CaretIndex;
            TroopAdCloseButtonX.Text = Regex.Replace((e.Source as TextBox).Text, RegExClass.Number, "");
            TroopAdCloseButtonX.Select(caretIndex, 0);
        }

        private void MidasAdCloseButtonX_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            int caretIndex = MidasAdCloseButtonX.CaretIndex;
            MidasAdCloseButtonX.Text = Regex.Replace((e.Source as TextBox).Text, RegExClass.Number, "");
            MidasAdCloseButtonX.Select(caretIndex, 0);
        }

        private void LeftAdCloseButtonX_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            int caretIndex = LeftAdCloseButtonX.CaretIndex;
            LeftAdCloseButtonX.Text = Regex.Replace((e.Source as TextBox).Text, RegExClass.Number, "");
            LeftAdCloseButtonX.Select(caretIndex, 0);
        }

        private void RightAdCloseButtonX_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            int caretIndex = RightAdCloseButtonX.CaretIndex;
            RightAdCloseButtonX.Text = Regex.Replace((e.Source as TextBox).Text, RegExClass.Number, "");
            RightAdCloseButtonX.Select(caretIndex, 0);
        }

        private void LatestUsedAppButtonX_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            int caretIndex = LatestUsedAppButtonX.CaretIndex;
            LatestUsedAppButtonX.Text = Regex.Replace((e.Source as TextBox).Text, RegExClass.Number, "");
            LatestUsedAppButtonX.Select(caretIndex, 0);
        }

        private void CloseAllAppButtonX_TextChanged(object sender, TextChangedEventArgs e)
        {
            int caretIndex = CloseAllAppButtonX.CaretIndex;
            CloseAllAppButtonX.Text = Regex.Replace((e.Source as TextBox).Text, RegExClass.Number, "");
            CloseAllAppButtonX.Select(caretIndex, 0);
        }

        private void NotRespondingButtonX_TextChanged(object sender, TextChangedEventArgs e)
        {
            int caretIndex = NotRespondingButtonX.CaretIndex;
            NotRespondingButtonX.Text = Regex.Replace((e.Source as TextBox).Text, RegExClass.Number, "");
            NotRespondingButtonX.Select(caretIndex, 0);
        }

        private void AppLocationY_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            int caretIndex = AppLocationY.CaretIndex;
            AppLocationY.Text = Regex.Replace((e.Source as TextBox).Text, RegExClass.Number, "");
            AppLocationY.Select(caretIndex, 0);
        }

        private void HomeButtonY_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            int caretIndex = HomeButtonY.CaretIndex;
            HomeButtonY.Text = Regex.Replace((e.Source as TextBox).Text, RegExClass.Number, "");
            HomeButtonY.Select(caretIndex, 0);
        }

        private void ShopButtonY_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            int caretIndex = ShopButtonY.CaretIndex;
            ShopButtonY.Text = Regex.Replace((e.Source as TextBox).Text, RegExClass.Number, "");
            ShopButtonY.Select(caretIndex, 0);
        }

        private void CollectButtonY_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            int caretIndex = CollectButtonY.CaretIndex;
            CollectButtonY.Text = Regex.Replace((e.Source as TextBox).Text, RegExClass.Number, "");
            CollectButtonY.Select(caretIndex, 0);
        }

        private void GoldChestBoxY_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            int caretIndex = GoldChestBoxY.CaretIndex;
            GoldChestBoxY.Text = Regex.Replace((e.Source as TextBox).Text, RegExClass.Number, "");
            GoldChestBoxY.Select(caretIndex, 0);
        }

        private void BattleLevelButtonY_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            int caretIndex = BattleLevelButtonY.CaretIndex;
            BattleLevelButtonY.Text = Regex.Replace((e.Source as TextBox).Text, RegExClass.Number, "");
            BattleLevelButtonY.Select(caretIndex, 0);
        }

        private void SkillButtonY_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            int caretIndex = SkillButtonY.CaretIndex;
            SkillButtonY.Text = Regex.Replace((e.Source as TextBox).Text, RegExClass.Number, "");
            SkillButtonY.Select(caretIndex, 0);
        }

        private void SpeedButtonY_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            int caretIndex = SpeedButtonY.CaretIndex;
            SpeedButtonY.Text = Regex.Replace((e.Source as TextBox).Text, RegExClass.Number, "");
            SpeedButtonY.Select(caretIndex, 0);
        }

        private void ContinueButtonY_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            int caretIndex = ContinueButtonY.CaretIndex;
            ContinueButtonY.Text = Regex.Replace((e.Source as TextBox).Text, RegExClass.Number, "");
            ContinueButtonY.Select(caretIndex, 0);
        }

        private void VictoryDefeatY_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            int caretIndex = VictoryDefeatY.CaretIndex;
            VictoryDefeatY.Text = Regex.Replace((e.Source as TextBox).Text, RegExClass.Number, "");
            VictoryDefeatY.Select(caretIndex, 0);
        }

        private void GoldButtonBackgroundY_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            int caretIndex = GoldButtonBackgroundY.CaretIndex;
            GoldButtonBackgroundY.Text = Regex.Replace((e.Source as TextBox).Text, RegExClass.Number, "");
            GoldButtonBackgroundY.Select(caretIndex, 0);
        }

        private void GoldButtonImageY_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            int caretIndex = GoldButtonImageY.CaretIndex;
            GoldButtonImageY.Text = Regex.Replace((e.Source as TextBox).Text, RegExClass.Number, "");
            GoldButtonImageY.Select(caretIndex, 0);
        }

        private void NextButtonY_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            int caretIndex = NextButtonY.CaretIndex;
            NextButtonY.Text = Regex.Replace((e.Source as TextBox).Text, RegExClass.Number, "");
            NextButtonY.Select(caretIndex, 0);
        }

        private void NoGoldY_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            int caretIndex = NoGoldY.CaretIndex;
            NoGoldY.Text = Regex.Replace((e.Source as TextBox).Text, RegExClass.Number, "");
            NoGoldY.Select(caretIndex, 0);
        }

        private void GoldAdCloseButtonY_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            int caretIndex = GoldAdCloseButtonY.CaretIndex;
            GoldAdCloseButtonY.Text = Regex.Replace((e.Source as TextBox).Text, RegExClass.Number, "");
            GoldAdCloseButtonY.Select(caretIndex, 0);
        }

        private void TroopAdCloseButtonY_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            int caretIndex = TroopAdCloseButtonY.CaretIndex;
            TroopAdCloseButtonY.Text = Regex.Replace((e.Source as TextBox).Text, RegExClass.Number, "");
            TroopAdCloseButtonY.Select(caretIndex, 0);
        }

        private void MidasAdCloseButtonY_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            int caretIndex = MidasAdCloseButtonY.CaretIndex;
            MidasAdCloseButtonY.Text = Regex.Replace((e.Source as TextBox).Text, RegExClass.Number, "");
            MidasAdCloseButtonY.Select(caretIndex, 0);
        }

        private void LeftAdCloseButtonY_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            int caretIndex = LeftAdCloseButtonY.CaretIndex;
            LeftAdCloseButtonY.Text = Regex.Replace((e.Source as TextBox).Text, RegExClass.Number, "");
            LeftAdCloseButtonY.Select(caretIndex, 0);
        }

        private void RightAdCloseButtonY_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            int caretIndex = RightAdCloseButtonY.CaretIndex;
            RightAdCloseButtonY.Text = Regex.Replace((e.Source as TextBox).Text, RegExClass.Number, "");
            RightAdCloseButtonY.Select(caretIndex, 0);
        }

        private void LatestUsedAppButtonY_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            int caretIndex = LatestUsedAppButtonY.CaretIndex;
            LatestUsedAppButtonY.Text = Regex.Replace((e.Source as TextBox).Text, RegExClass.Number, "");
            LatestUsedAppButtonY.Select(caretIndex, 0);
        }

        private void CloseAllAppButtonY_TextChanged(object sender, TextChangedEventArgs e)
        {
            int caretIndex = CloseAllAppButtonY.CaretIndex;
            CloseAllAppButtonY.Text = Regex.Replace((e.Source as TextBox).Text, RegExClass.Number, "");
            CloseAllAppButtonY.Select(caretIndex, 0);
        }

        private void NotRespondingButtonY_TextChanged(object sender, TextChangedEventArgs e)
        {
            int caretIndex = NotRespondingButtonY.CaretIndex;
            NotRespondingButtonY.Text = Regex.Replace((e.Source as TextBox).Text, RegExClass.Number, "");
            NotRespondingButtonY.Select(caretIndex, 0);
        }

        private void AppLocationColor_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            int caretIndex = AppLocationColor.CaretIndex;
            AppLocationColor.Text = Regex.Replace((e.Source as TextBox).Text, RegExClass.HtmlColor, "");
            AppLocationColor.Select(caretIndex, 0);
        }

        private void ShopButtonColor_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            int caretIndex = ShopButtonColor.CaretIndex;
            ShopButtonColor.Text = Regex.Replace((e.Source as TextBox).Text, RegExClass.HtmlColor, "");
            ShopButtonColor.Select(caretIndex, 0);
        }

        private void CollectButtonColor_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            int caretIndex = CollectButtonColor.CaretIndex;
            CollectButtonColor.Text = Regex.Replace((e.Source as TextBox).Text, RegExClass.HtmlColor, "");
            CollectButtonColor.Select(caretIndex, 0);
        }

        private void GoldChestBoxColor_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            int caretIndex = GoldChestBoxColor.CaretIndex;
            GoldChestBoxColor.Text = Regex.Replace((e.Source as TextBox).Text, RegExClass.HtmlColor, "");
            GoldChestBoxColor.Select(caretIndex, 0);
        }

        private void BattleLevelButtonColor_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            int caretIndex = BattleLevelButtonColor.CaretIndex;
            BattleLevelButtonColor.Text = Regex.Replace((e.Source as TextBox).Text, RegExClass.HtmlColor, "");
            BattleLevelButtonColor.Select(caretIndex, 0);
        }

        private void SkillButtonColor_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            int caretIndex = SkillButtonColor.CaretIndex;
            SkillButtonColor.Text = Regex.Replace((e.Source as TextBox).Text, RegExClass.HtmlColor, "");
            SkillButtonColor.Select(caretIndex, 0);
        }

        private void SpeedButtonColor_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            int caretIndex = SpeedButtonColor.CaretIndex;
            SpeedButtonColor.Text = Regex.Replace((e.Source as TextBox).Text, RegExClass.HtmlColor, "");
            SpeedButtonColor.Select(caretIndex, 0);
        }

        private void ContinueButtonColor_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            int caretIndex = ContinueButtonColor.CaretIndex;
            ContinueButtonColor.Text = Regex.Replace((e.Source as TextBox).Text, RegExClass.HtmlColor, "");
            ContinueButtonColor.Select(caretIndex, 0);
        }

        private void VictoryDefeatColor_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            int caretIndex = VictoryDefeatColor.CaretIndex;
            VictoryDefeatColor.Text = Regex.Replace((e.Source as TextBox).Text, RegExClass.HtmlColor, "");
            VictoryDefeatColor.Select(caretIndex, 0);
        }

        private void GoldButtonBackgroundColor_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            int caretIndex = GoldButtonBackgroundColor.CaretIndex;
            GoldButtonBackgroundColor.Text = Regex.Replace((e.Source as TextBox).Text, RegExClass.HtmlColor, "");
            GoldButtonBackgroundColor.Select(caretIndex, 0);
        }

        private void GoldButtonImageColor_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            int caretIndex = GoldButtonImageColor.CaretIndex;
            GoldButtonImageColor.Text = Regex.Replace((e.Source as TextBox).Text, RegExClass.HtmlColor, "");
            GoldButtonImageColor.Select(caretIndex, 0);
        }

        private void NextButtonColor_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            int caretIndex = NextButtonColor.CaretIndex;
            NextButtonColor.Text = Regex.Replace((e.Source as TextBox).Text, RegExClass.HtmlColor, "");
            NextButtonColor.Select(caretIndex, 0);
        }

        private void NoGoldColor_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            int caretIndex = NoGoldColor.CaretIndex;
            NoGoldColor.Text = Regex.Replace((e.Source as TextBox).Text, RegExClass.HtmlColor, "");
            NoGoldColor.Select(caretIndex, 0);
        }

        private void GoldAdCloseButtonColor_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            int caretIndex = GoldAdCloseButtonColor.CaretIndex;
            GoldAdCloseButtonColor.Text = Regex.Replace((e.Source as TextBox).Text, RegExClass.HtmlColor, "");
            GoldAdCloseButtonColor.Select(caretIndex, 0);
        }

        private void TroopAdCloseButtonColor_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            int caretIndex = TroopAdCloseButtonColor.CaretIndex;
            TroopAdCloseButtonColor.Text = Regex.Replace((e.Source as TextBox).Text, RegExClass.HtmlColor, "");
            TroopAdCloseButtonColor.Select(caretIndex, 0);
        }

        private void MidasAdCloseButtonColor_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            int caretIndex = MidasAdCloseButtonColor.CaretIndex;
            MidasAdCloseButtonColor.Text = Regex.Replace((e.Source as TextBox).Text, RegExClass.HtmlColor, "");
            MidasAdCloseButtonColor.Select(caretIndex, 0);
        }

        private void LeftAdCloseButtonColor_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            int caretIndex = LeftAdCloseButtonColor.CaretIndex;
            LeftAdCloseButtonColor.Text = Regex.Replace((e.Source as TextBox).Text, RegExClass.HtmlColor, "");
            LeftAdCloseButtonColor.Select(caretIndex, 0);
        }

        private void RightAdCloseButtonColor_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            int caretIndex = RightAdCloseButtonColor.CaretIndex;
            RightAdCloseButtonColor.Text = Regex.Replace((e.Source as TextBox).Text, RegExClass.HtmlColor, "");
            RightAdCloseButtonColor.Select(caretIndex, 0);
        }

        private void LatestUsedAppButtonColor_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            //int caretIndex = LatestUsedAppButtonColor.CaretIndex;
            //LatestUsedAppButtonColor.Text = Regex.Replace((e.Source as TextBox).Text, RegExClass.HtmlColor, "");
            //LatestUsedAppButtonColor.Select(caretIndex, 0);
        }

        private void NotRespondingButtonColor_TextChanged(object sender, TextChangedEventArgs e)
        {
            int caretIndex = NotRespondingButtonColor.CaretIndex;
            NotRespondingButtonColor.Text = Regex.Replace((e.Source as TextBox).Text, RegExClass.HtmlColor, "");
            NotRespondingButtonColor.Select(caretIndex, 0);
        }
    }
}
