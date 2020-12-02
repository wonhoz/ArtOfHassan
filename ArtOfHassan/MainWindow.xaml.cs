using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Threading;

namespace ArtOfHassan
{
    internal enum SHOW_WINDOW_COMMANDS : int
    {
        HIDE      = 0,
        NORMAL    = 1,
        MINIMIZED = 2,
        MAXIMIZED = 3,
    }

    internal struct WINDOWPLACEMENT
    {
        public int length;
        public int flags;
        public SHOW_WINDOW_COMMANDS     showc_cmd;
        public System.Drawing.Point     min_position;
        public System.Drawing.Point     max_position;
        public System.Drawing.Rectangle normal_position;
    }

    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        [DllImport("user32.dll")]
        static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint dwData, int dwExtraInfo);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern IntPtr FindWindow(string strClassName, string strWindowName);

        [DllImport("user32.dll")]
        internal static extern bool GetWindowPlacement(IntPtr handle, ref WINDOWPLACEMENT placement);

        private const uint MOUSEMOVE    = 0x0001;  // 마우스 이동
        private const uint ABSOLUTEMOVE = 0x8000;  // 전역 위치
        private const uint LBUTTONDOWN  = 0x0002;  // 왼쪽 마우스 버튼 눌림
        private const uint LBUTTONUP    = 0x0004;  // 왼쪽 마우스 버튼 떼어짐
        private const uint RBUTTONDOWN  = 0x0008;  // 오른쪽 마우스 버튼 눌림
        private const uint RBUTTONUP    = 0x00010; // 오른쪽 마우스 버튼 떼어짐

        private static System.Timers.Timer NoxTimer    = new System.Timers.Timer();
        private static System.Timers.Timer ButtonTimer = new System.Timers.Timer();

        double NoxPointX = 0;
        double NoxPointY = 0;
        double NoxWidth  = 0;
        double NoxHeight = 0;

        bool VictoryFlag = false;
        bool DefeatFlag  = false;
        bool AdsFlag     = false;

        int NumOfVictory = 0;
        int NumOfDefeat  = 0;
        int NumOfAds     = 0;

        int TimerCountForScreenCompare = 1;

        System.Drawing.Bitmap LastBitmap;
        System.Drawing.Bitmap CurrentBitmap;


        public MainWindow()
        {
            InitializeComponent();

            DirectoryInfo directoryInfo = new DirectoryInfo("log");
            if (directoryInfo.Exists == false)
            {
                directoryInfo.Create();
            }

            ButtonTimer.Interval = 1000; // 1초
            ButtonTimer.Elapsed += ButtonTimerFunction;

            NoxTimer.Interval += 200;
            NoxTimer.Elapsed += NoxTimerFunction;
            NoxTimer.Enabled = true;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            ButtonTimer.Enabled = false;
            NoxTimer.Enabled    = false;
        }

        private void ClickLog(string log)
        {
            using (StreamWriter streamWriter = new StreamWriter($@"log\Click_{DateTime.Today.ToString("yyyyMMdd")}.log", true))
            {
                streamWriter.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff ") + log);
            }
        }

        private void TimerLog(string log)
        {
            using (StreamWriter streamWriter = new StreamWriter($@"log\Timer_{DateTime.Today.ToString("yyyyMMdd")}.log", true))
            {
                streamWriter.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff ") + log);
            }
        }

        private IntPtr GetWinAscHandle()
        {
            string windowTitle = "NoxPlayer";
            System.Windows.Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
            {
                windowTitle = WindowTitleTextBox.Text;
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

        bool IsLatest = true;

        private void LatestRadioButton_Click(object sender, RoutedEventArgs e)
        {
            ClickLog("Working for the latest version");
            IsLatest = true;
            GoldChestBoxX = 150;
            GoldChestBoxY = 410;
            GoldButtonBackgroundY = 780;
        }

        private void OldRadioButton_Click(object sender, RoutedEventArgs e)
        {
            ClickLog("Working for the version 3.0.8");
            IsLatest = false;
            GoldChestBoxX = 165;
            GoldChestBoxY = 420;
            GoldButtonBackgroundY = 710;
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            ClickLog("StartButton: " + StartButton.Content.ToString());

            if (StartButton.Content.ToString() == "Start")
            {
                StartButton.Content = "Stop";

                ButtonTimer.Enabled = true;
            }
            else
            {
                StartButton.Content = "Start";

                ButtonTimer.Enabled = false;
            }
        }


        int AppLocationX    = 60;
        int AppLocationY    = 500;

        int HomeButtonX     = 290;
        int ShopButtonX     = 65;
        int ShopButtonY     = 980;

        int MiddleButtonX   = 195;
        int MiddleButtonY   = 680;

        int GoldChestBoxX      = 150;
        int GoldChestBoxY      = 410;
        int BattleLevelButtonX = 180;
        int BattleLevelButtonY = 855;

        int SkillButtonX     = 475;
        int SkillButtonY     = 920;
        int SpeedButtonX     = 514;
        int SpeedButtonY     = 989;

        int PauseButtonX     = 215;
        int PauseButtonY     = 455;
        int VictoryDefeatX   = 120;
        int VictoryDefeatY   = 355;

        int GoldButtonBackgroundX = 115;
        int GoldButtonBackgroundY = 780;
        int GoldButtonImageX      = 133;
        int GoldButtonImageY      = 755;
        int NextButtonX           = 450;
        int NextButtonY           = 710;

        int AdsButtonX       = 496;
        int AdsButton1Y      = 180;
        int AdsButton2Y      = 190;

        int AdsCloseButton1X = 39;
        int AdsCloseButton2X = 519;
        int AdsCloseButtonY  = 68;

        int NotRespondingX   = 79;
        int NotRespondingY   = 540;


        private void NoxTimerFunction(object sender, System.Timers.ElapsedEventArgs e)
        {
            System.Windows.Point point = new System.Windows.Point();
            System.Windows.Size size = new System.Windows.Size();

            GetWindowPos(GetWinAscHandle(), ref point, ref size);
            //TimerLog("point.X: "     + point.X);
            //TimerLog("point.Y: "     + point.Y);
            //TimerLog("size.Width: "  + size.Width);
            //TimerLog("size.Height: " + size.Height);

            if ((size.Width == 0) || (size.Height == 0))
            {
                System.Windows.Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
                {
                    ButtonTimer.Enabled   = false;
                    StartButton.IsEnabled = false;
                    StartButton.Content   = "Start";
                }));
            }
            else
            {
                NoxPointX = point.X;
                NoxPointY = point.Y;
                NoxWidth  = size.Width;
                NoxHeight = size.Height;

                System.Windows.Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
                {
                    StartButton.IsEnabled = true;
                }));
            }

            //TimerLog("NoxPointX: " + NoxPointX);
            //TimerLog("NoxPointY: " + NoxPointY);
            //TimerLog("NoxWidth: "  + NoxWidth);
            //TimerLog("NoxHeight: " + NoxHeight);
        }

        private void ButtonTimerFunction(object sender, System.Timers.ElapsedEventArgs e)
        {
            TimerLog("Allocate CurrentBitmap");
            // 화면 크기만큼의 Bitmap 생성
            CurrentBitmap = new System.Drawing.Bitmap((int)NoxWidth, (int)NoxHeight, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            // Bitmap 이미지 변경을 위해 Graphics 객체 생성
            TimerLog("Allocate Graphics from CurrentBitmap");
            using (System.Drawing.Graphics graphics = System.Drawing.Graphics.FromImage(CurrentBitmap))
            {
                TimerLog("CopyFromScreen");
                // 화면을 그대로 카피해서 Bitmap 메모리에 저장
                graphics.CopyFromScreen((int)NoxPointX, (int)NoxPointY, 0, 0, CurrentBitmap.Size);
                // Bitmap 데이타를 파일로 저장
                //bitmap.Save(imageName + ".png", System.Drawing.Imaging.ImageFormat.Png);


                // Off Check
                System.Drawing.Color color = CurrentBitmap.GetPixel(AppLocationX, AppLocationY);
                TimerLog("Off Check Color: " + color.R + "," + color.G + "," + color.B);
                if (((color.R == 169) && (color.G == 216) && (color.B == 255)) ||
                    ((color.R == 151) && (color.G == 198) && (color.B == 1)))
                {
                    ClickLog("Off Check");
                    System.Windows.Forms.Cursor.Position = new System.Drawing.Point((int)NoxPointX + AppLocationX, (int)NoxPointY + AppLocationY);
                    mouse_event(LBUTTONDOWN, 0, 0, 0, 0);
                    System.Threading.Thread.Sleep(50);
                    mouse_event(LBUTTONUP, 0, 0, 0, 0);
                }


                // Shop Check
                color = CurrentBitmap.GetPixel(ShopButtonX, ShopButtonY);
                TimerLog("Shop Check Color: " + color.R + "," + color.G + "," + color.B);
                if ((color.R == 234) && (color.G == 61) && (color.B == 52))
                {
                    ClickLog("Shop Check");
                    System.Windows.Forms.Cursor.Position = new System.Drawing.Point((int)NoxPointX + HomeButtonX, (int)NoxPointY + ShopButtonY);
                    mouse_event(LBUTTONDOWN, 0, 0, 0, 0);
                    System.Threading.Thread.Sleep(50);
                    mouse_event(LBUTTONUP, 0, 0, 0, 0);
                }


                // Middle Button
                color = CurrentBitmap.GetPixel(MiddleButtonX, MiddleButtonY);
                TimerLog("Middle Button Color: " + color.R + "," + color.G + "," + color.B);
                if ((color.R == 253) && (color.G == 187) && (color.B == 0))
                {
                    ClickLog("Middle Button");
                    System.Windows.Forms.Cursor.Position = new System.Drawing.Point((int)NoxPointX + MiddleButtonX, (int)NoxPointY + MiddleButtonY);
                    mouse_event(LBUTTONDOWN, 0, 0, 0, 0);
                    System.Threading.Thread.Sleep(50);
                    mouse_event(LBUTTONUP, 0, 0, 0, 0);
                }


                // Gold Chest Box Button
                bool IsOpenGoldChestBox = false;
                System.Windows.Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
                {
                    IsOpenGoldChestBox = GoldChestCheckBox.IsChecked.Value;
                }));
                if (IsOpenGoldChestBox)
                {
                    color = CurrentBitmap.GetPixel(GoldChestBoxX, GoldChestBoxY);
                    TimerLog("Gold Chest Box Color: " + color.R + "," + color.G + "," + color.B);
                    if (((color.R == 255) && (color.G == 241) && (color.B == 2)) || // Latest
                        ((color.R == 234) && (color.G == 191) && (color.B == 47)))  // 3.0.8
                    {
                        ClickLog("Gold Chest Box");
                        System.Windows.Forms.Cursor.Position = new System.Drawing.Point((int)NoxPointX + GoldChestBoxX, (int)NoxPointY + GoldChestBoxY);
                        mouse_event(LBUTTONDOWN, 0, 0, 0, 0);
                        System.Threading.Thread.Sleep(50);
                        mouse_event(LBUTTONUP, 0, 0, 0, 0);

                        System.Threading.Thread.Sleep(500);

                        return;
                    }
                }


                // Battle and Level Button
                color = CurrentBitmap.GetPixel(BattleLevelButtonX, BattleLevelButtonY);
                TimerLog("Battle Level Button Color: " + color.R + "," + color.G + "," + color.B);
                if (((color.R == 253) && (color.G == 187) && (color.B == 0)) || // 황금색
                    ((color.R == 202) && (color.G == 150) && (color.B == 0)))   // + 메뉴 음영
                {
                    ClickLog("Battle Level Button");
                    System.Windows.Forms.Cursor.Position = new System.Drawing.Point((int)NoxPointX + BattleLevelButtonX, (int)NoxPointY + BattleLevelButtonY);
                    mouse_event(LBUTTONDOWN, 0, 0, 0, 0);
                    System.Threading.Thread.Sleep(50);
                    mouse_event(LBUTTONUP, 0, 0, 0, 0);

                    if (VictoryFlag)
                    {
                        VictoryFlag = false;
                        NumOfVictory++;
                    }

                    if (DefeatFlag)
                    {
                        DefeatFlag = false;
                        NumOfDefeat++;
                    }

                    if (AdsFlag)
                    {
                        AdsFlag = false;
                        NumOfAds++;
                    }

                    System.Windows.Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
                    {
                        MessageBar.Text = $"War: {NumOfVictory + NumOfDefeat} | Victory: {NumOfVictory} | Defeat: {NumOfDefeat} | Ads: {NumOfAds}";
                    }));
                }
                else if ((color.R == 189) && (color.G == 24) && (color.B == 8)) // 빨간색
                {
                    ClickLog("Battle Level Button is Red");
                    return;
                }
                else if ((color.R == 13) && (color.G == 103) && (color.B == 122)) // 바탕색
                {
                    ClickLog("Battle Level Button is disappered");
                    return;
                }


                // Skill Button
                color = CurrentBitmap.GetPixel(SkillButtonX, SkillButtonY);
                TimerLog("Skill Button Color: " + color.R + "," + color.G + "," + color.B);
                if ((color.R == 253) && (color.G == 187) && (color.B == 0))
                {
                    ClickLog("Skill Button");
                    System.Windows.Forms.Cursor.Position = new System.Drawing.Point((int)NoxPointX + SkillButtonX, (int)NoxPointY + SkillButtonY);
                    mouse_event(LBUTTONDOWN, 0, 0, 0, 0);
                    System.Threading.Thread.Sleep(50);
                    mouse_event(LBUTTONUP, 0, 0, 0, 0);

                    System.Threading.Thread.Sleep(500);
                }


                // Speed Button
                color = CurrentBitmap.GetPixel(SpeedButtonX, SpeedButtonY);
                TimerLog("Speed Button Color: " + color.R + "," + color.G + "," + color.B);
                if ((color.R == 237) && (color.G == 165) && (color.B == 0))
                {
                    ClickLog("Speed Button");
                    System.Windows.Forms.Cursor.Position = new System.Drawing.Point((int)NoxPointX + SpeedButtonX, (int)NoxPointY + SpeedButtonY);
                    mouse_event(LBUTTONDOWN, 0, 0, 0, 0);
                    System.Threading.Thread.Sleep(50);
                    mouse_event(LBUTTONUP, 0, 0, 0, 0);
                }


                // Pause
                bool IsPausable = false;
                System.Windows.Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
                {
                    IsPausable = PausabilityCheckBox.IsChecked.Value;
                }));
                if (!IsPausable)
                {
                    color = CurrentBitmap.GetPixel(PauseButtonX, PauseButtonY);
                    TimerLog("Pause Button Color: " + color.R + "," + color.G + "," + color.B);
                    if ((color.R == 253) && (color.G == 187) && (color.B == 0))
                    {
                        ClickLog("Pause Button");
                        System.Windows.Forms.Cursor.Position = new System.Drawing.Point((int)NoxPointX + PauseButtonX, (int)NoxPointY + PauseButtonY);
                        mouse_event(LBUTTONDOWN, 0, 0, 0, 0);
                        System.Threading.Thread.Sleep(50);
                        mouse_event(LBUTTONUP, 0, 0, 0, 0);
                    }
                }


                // Check Victory or Defeat
                color = CurrentBitmap.GetPixel(VictoryDefeatX, VictoryDefeatY);
                TimerLog("Victory or Defeat Color: " + color.R + "," + color.G + "," + color.B);
                if ((color.R == 217) && (color.G == 28) && (color.B == 19))
                {
                    ClickLog("Victory Checked");
                    VictoryFlag = true;
                }
                else if ((color.R == 18) && (color.G == 167) && (color.B == 216))
                {
                    ClickLog("Defeat Checked");
                    DefeatFlag = true;
                }


                // Gold Button
                color = CurrentBitmap.GetPixel(GoldButtonBackgroundX, GoldButtonBackgroundY);
                TimerLog("Gold Button Background Color: " + color.R + "," + color.G + "," + color.B);
                if ((color.R == 125) && (color.G == 167) && (color.B == 10)) // Green
                {
                    color = CurrentBitmap.GetPixel(GoldButtonImageX, GoldButtonImageY);
                    TimerLog("Gold Button Image Color: " + color.R + "," + color.G + "," + color.B);
                    if (!IsLatest || ((color.R == 255) && (color.G == 234) && (color.B == 144))) // Yellow
                    {
                        ClickLog("Gold Button");
                        AdsFlag = true;

                        System.Windows.Forms.Cursor.Position = new System.Drawing.Point((int)NoxPointX + GoldButtonBackgroundX, (int)NoxPointY + GoldButtonBackgroundY);
                        mouse_event(LBUTTONDOWN, 0, 0, 0, 0);
                        System.Threading.Thread.Sleep(50);
                        mouse_event(LBUTTONUP, 0, 0, 0, 0);
                    }
                    else // Green such as Retry
                    {
                        ClickLog("Next Button");

                        System.Windows.Forms.Cursor.Position = new System.Drawing.Point((int)NoxPointX + NextButtonX, (int)NoxPointY + GoldButtonBackgroundY);
                        mouse_event(LBUTTONDOWN, 0, 0, 0, 0);
                        System.Threading.Thread.Sleep(50);
                        mouse_event(LBUTTONUP, 0, 0, 0, 0);
                    }
                }
                else if ((color.R == 142) && (color.G == 142) && (color.B == 142)) // Gray
                {
                    ClickLog("Gold Button is Gray");

                    if (IsLatest)
                    {
                        System.Windows.Forms.Cursor.Position = new System.Drawing.Point((int)NoxPointX + NextButtonX, (int)NoxPointY + GoldButtonBackgroundY);
                    }
                    else
                    {
                        System.Windows.Forms.Cursor.Position = new System.Drawing.Point((int)NoxPointX + NextButtonX, (int)NoxPointY + NextButtonY);
                    }
                    mouse_event(LBUTTONDOWN, 0, 0, 0, 0);
                    System.Threading.Thread.Sleep(50);
                    mouse_event(LBUTTONUP, 0, 0, 0, 0);
                }


                // Next Button
                color = CurrentBitmap.GetPixel(NextButtonX, NextButtonY);
                TimerLog("Next Button Color: " + color.R + "," + color.G + "," + color.B);
                if (IsLatest && ((color.R == 253) && (color.G == 187) && (color.B == 0)))
                {
                    ClickLog("Next Button");
                    System.Windows.Forms.Cursor.Position = new System.Drawing.Point((int)NoxPointX + NextButtonX, (int)NoxPointY + NextButtonY);
                    mouse_event(LBUTTONDOWN, 0, 0, 0, 0);
                    System.Threading.Thread.Sleep(50);
                    mouse_event(LBUTTONUP, 0, 0, 0, 0);
                }


                // Not Responding Button
                color = CurrentBitmap.GetPixel(NotRespondingX, NotRespondingY);
                TimerLog("Not Responding Button Color: " + color.R + "," + color.G + "," + color.B);
                if ((color.R == 0) && (color.G == 150) && (color.B == 136))
                {
                    ClickLog("Not Responding Button");
                    System.Windows.Forms.Cursor.Position = new System.Drawing.Point((int)NoxPointX + NotRespondingX, (int)NoxPointY + NotRespondingY);
                    mouse_event(LBUTTONDOWN, 0, 0, 0, 0);
                    System.Threading.Thread.Sleep(50);
                    mouse_event(LBUTTONUP, 0, 0, 0, 0);
                }


                // Ads Button1
                color = CurrentBitmap.GetPixel(AdsButtonX, AdsButton1Y);
                TimerLog("Ads Button Color: " + color.R + "," + color.G + "," + color.B);
                if ((color.R == 233) && (color.G == 233) && (color.B == 216))
                {
                    ClickLog("Ads Button 1");
                    System.Windows.Forms.Cursor.Position = new System.Drawing.Point((int)NoxPointX + AdsButtonX, (int)NoxPointY + AdsButton1Y);
                    mouse_event(LBUTTONDOWN, 0, 0, 0, 0);
                    System.Threading.Thread.Sleep(50);
                    mouse_event(LBUTTONUP, 0, 0, 0, 0);

                    return;
                }


                // Ads Button2
                color = CurrentBitmap.GetPixel(AdsButtonX, AdsButton2Y);
                TimerLog("Ads Button Color: " + color.R + "," + color.G + "," + color.B);
                if ((color.R == 233) && (color.G == 233) && (color.B == 216))
                {
                    ClickLog("Ads Button 2");
                    System.Windows.Forms.Cursor.Position = new System.Drawing.Point((int)NoxPointX + AdsButtonX, (int)NoxPointY + AdsButton2Y);
                    mouse_event(LBUTTONDOWN, 0, 0, 0, 0);
                    System.Threading.Thread.Sleep(50);
                    mouse_event(LBUTTONUP, 0, 0, 0, 0);

                    return;
                }


                // Ads Close Button
                int screenCompareInterval = 3;
                System.Windows.Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
                {
                    if (!int.TryParse(ScreenComparisonIntervalTextBox.Text, out screenCompareInterval))
                    {
                        screenCompareInterval = 3;
                    }
                }));
                TimerLog("Screen Compare Interval: " + screenCompareInterval);
                if ((TimerCountForScreenCompare % screenCompareInterval) == 0)
                {
                    TimerCountForScreenCompare = 1;

                    bool isDifferent = false;

                    for (int row = 0; row < NoxHeight; row++)
                    {
                        if (isDifferent)
                        {
                            break;
                        }

                        for (int col = 0; col < NoxWidth; col++)
                        {
                            System.Drawing.Color lastColor    = LastBitmap.GetPixel(col, row);
                            System.Drawing.Color currentColor = CurrentBitmap.GetPixel(col, row);

                            if ((lastColor.R != currentColor.R) && (lastColor.G != currentColor.G) && (lastColor.B != currentColor.B))
                            {
                                isDifferent = true;
                                break;
                            }
                        }
                    }

                    TimerLog("Screen is changed: " + isDifferent);

                    if (!isDifferent)
                    {
                        ClickLog("Ads Close Button");

                        System.Windows.Forms.Cursor.Position = new System.Drawing.Point((int)NoxPointX + AdsCloseButton1X, (int)NoxPointY + AdsCloseButtonY);
                        mouse_event(LBUTTONDOWN, 0, 0, 0, 0);
                        System.Threading.Thread.Sleep(50);
                        mouse_event(LBUTTONUP, 0, 0, 0, 0);

                        System.Threading.Thread.Sleep(500);

                        System.Windows.Forms.Cursor.Position = new System.Drawing.Point((int)NoxPointX + AdsCloseButton2X, (int)NoxPointY + AdsCloseButtonY);
                        mouse_event(LBUTTONDOWN, 0, 0, 0, 0);
                        System.Threading.Thread.Sleep(50);
                        mouse_event(LBUTTONUP, 0, 0, 0, 0);

                        return;
                    }
                }


                //CurrentBitmap.Save(Stage + ".png", System.Drawing.Imaging.ImageFormat.Png);
                LastBitmap = CurrentBitmap;
                TimerCountForScreenCompare++;
            }
        }

        private void SettingButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
