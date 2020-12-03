using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media;
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

        Stopwatch AdsStopwatch = new Stopwatch();

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

        public System.Drawing.Color PixelColor;
        public int PixelPositionX;
        public int PixelPositionY;


        public MainWindow()
        {
            InitializeComponent();

            DirectoryInfo directoryInfo = new DirectoryInfo("log");
            if (directoryInfo.Exists == false)
            {
                directoryInfo.Create();
            }

            FileInfo fileInfo = new FileInfo("setting.txt");
            if (fileInfo.Exists)
            {
                ClickLog("Load Setting...");

                string[] lines = File.ReadAllLines("setting.txt");
                AppLocationX = int.Parse(lines[0].Split(',')[0]);
                AppLocationY = int.Parse(lines[0].Split(',')[1]);
                AppLocationColor = lines[0].Split(',')[2];

                HomeButtonX = int.Parse(lines[1].Split(',')[0]);
                ShopButtonX = int.Parse(lines[1].Split(',')[1]);
                ShopButtonY = int.Parse(lines[1].Split(',')[2]);
                ShopButtonColor = lines[1].Split(',')[3];

                MiddleButtonX = int.Parse(lines[2].Split(',')[0]);
                MiddleButtonY = int.Parse(lines[2].Split(',')[1]);
                MiddleButtonColor = lines[2].Split(',')[2];

                GoldChestBoxX = int.Parse(lines[3].Split(',')[0]);
                GoldChestBoxY = int.Parse(lines[3].Split(',')[1]);
                GoldChestBoxColor = lines[3].Split(',')[2];

                BattleLevelButtonX = int.Parse(lines[4].Split(',')[0]);
                BattleLevelButtonY = int.Parse(lines[4].Split(',')[1]);
                BattleLevelButtonColor = lines[4].Split(',')[2];

                SkillButtonX = int.Parse(lines[5].Split(',')[0]);
                SkillButtonY = int.Parse(lines[5].Split(',')[1]);
                SkillButtonColor = lines[5].Split(',')[2];

                SpeedButtonX = int.Parse(lines[6].Split(',')[0]);
                SpeedButtonY = int.Parse(lines[6].Split(',')[1]);
                SpeedButtonColor = lines[6].Split(',')[2];

                PauseButtonX = int.Parse(lines[7].Split(',')[0]);
                PauseButtonY = int.Parse(lines[7].Split(',')[1]);
                PauseButtonColor = lines[7].Split(',')[2];

                VictoryDefeatX = int.Parse(lines[8].Split(',')[0]);
                VictoryDefeatY = int.Parse(lines[8].Split(',')[1]);
                VictoryDefeatColor = lines[8].Split(',')[2];

                GoldButtonBackgroundX = int.Parse(lines[9].Split(',')[0]);
                GoldButtonBackgroundY = int.Parse(lines[9].Split(',')[1]);
                GoldButtonBackgroundColor = lines[9].Split(',')[2];

                GoldButtonImageX = int.Parse(lines[10].Split(',')[0]);
                GoldButtonImageY = int.Parse(lines[10].Split(',')[1]);
                GoldButtonImageColor = lines[10].Split(',')[2];

                NextButtonX = int.Parse(lines[11].Split(',')[0]);
                NextButtonY = int.Parse(lines[11].Split(',')[1]);
                NextButtonColor = lines[11].Split(',')[2];

                AdsButtonX = int.Parse(lines[12].Split(',')[0]);
                AdsButton1Y = int.Parse(lines[12].Split(',')[1]);
                AdsButton2Y = int.Parse(lines[12].Split(',')[2]);
                AdsButtonColor = lines[12].Split(',')[3];

                AdsCloseButton1X = int.Parse(lines[13].Split(',')[0]);
                AdsCloseButton2X = int.Parse(lines[13].Split(',')[1]);
                AdsCloseButtonY = int.Parse(lines[13].Split(',')[2]);

                NotRespondingX = int.Parse(lines[14].Split(',')[0]);
                NotRespondingY = int.Parse(lines[14].Split(',')[1]);
                NotRespondingColor = lines[14].Split(',')[2];

                ClickLog("Load Done");
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
                SettingButton.IsEnabled = false;

                ButtonTimer.Enabled = true;
            }
            else
            {
                StartButton.Content = "Start";
                SettingButton.IsEnabled = true;

                ButtonTimer.Enabled = false;
            }
        }


        public int AppLocationX        = 60;
        public int AppLocationY        = 500;
        public string AppLocationColor = "#a9d8ff;#97c601".ToUpper();

        public int HomeButtonX        = 290;
        public int ShopButtonX        = 65;
        public int ShopButtonY        = 980;
        public string ShopButtonColor = "#ea3d34".ToUpper();

        public int MiddleButtonX        = 195;
        public int MiddleButtonY        = 680;
        public string MiddleButtonColor = "#fdbb00".ToUpper();

        public int GoldChestBoxX      = 150;
        public int GoldChestBoxY      = 410;
        public string GoldChestBoxColor = "#fff102;#eabf2f".ToUpper();

        public int BattleLevelButtonX = 180;
        public int BattleLevelButtonY = 855;
        public string BattleLevelButtonColor = "#fdbb00;#ca9600;#bd1808;#0d677a".ToUpper();

        public int SkillButtonX     = 475;
        public int SkillButtonY     = 920;
        public string SkillButtonColor = "#fdbb00".ToUpper();

        public int SpeedButtonX     = 514;
        public int SpeedButtonY     = 989;
        public string SpeedButtonColor = "#eda500".ToUpper();

        public int PauseButtonX     = 215;
        public int PauseButtonY     = 455;
        public string PauseButtonColor = "#fdbb00".ToUpper();

        public int VictoryDefeatX   = 120;
        public int VictoryDefeatY   = 355;
        public string VictoryDefeatColor = "#d91c13;#12a7d8".ToUpper();

        public int GoldButtonBackgroundX = 115;
        public int GoldButtonBackgroundY = 780;
        public string GoldButtonBackgroundColor = "#7da70a;#8e8e8e".ToUpper();

        public int GoldButtonImageX      = 133;
        public int GoldButtonImageY      = 755;
        public string GoldButtonImageColor = "#ffea90".ToUpper();

        public int NextButtonX           = 450;
        public int NextButtonY           = 710;
        public string NextButtonColor = "#fdbb00".ToUpper();

        public int AdsButtonX       = 496;
        public int AdsButton1Y      = 180;
        public int AdsButton2Y      = 190;
        public string AdsButtonColor = "#e9e9d8;#efe7d6".ToUpper();

        public int AdsCloseButton1X = 39;
        public int AdsCloseButton2X = 519;
        public int AdsCloseButtonY  = 68;

        public int NotRespondingX   = 79;
        public int NotRespondingY   = 540;
        public string NotRespondingColor = "#009688".ToUpper();


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
                System.Drawing.Color color1 = ColorTranslator.FromHtml(AppLocationColor.Split(';')[0]);
                System.Drawing.Color color2 = ColorTranslator.FromHtml(AppLocationColor.Split(';')[1]);
                if (((color.R == color1.R) && (color.G == color1.G) && (color.B == color1.B)) ||
                    ((color.R == color2.R) && (color.G == color2.G) && (color.B == color2.B)))
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
                color1 = ColorTranslator.FromHtml(ShopButtonColor);
                if ((color.R == color1.R) && (color.G == color1.G) && (color.B == color1.B))
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
                color1 = ColorTranslator.FromHtml(MiddleButtonColor);
                if ((color.R == color1.R) && (color.G == color1.G) && (color.B == color1.B))
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
                    color1 = ColorTranslator.FromHtml(GoldChestBoxColor.Split(';')[0]);
                    color2 = ColorTranslator.FromHtml(GoldChestBoxColor.Split(';')[1]);
                    if (((color.R == color1.R) && (color.G == color1.G) && (color.B == color1.B)) || // Latest
                        ((color.R == color2.R) && (color.G == color2.G) && (color.B == color2.B)))  // 3.0.8
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
                color1 = ColorTranslator.FromHtml(BattleLevelButtonColor.Split(';')[0]);
                color2 = ColorTranslator.FromHtml(BattleLevelButtonColor.Split(';')[1]);
                System.Drawing.Color color3 = ColorTranslator.FromHtml(BattleLevelButtonColor.Split(';')[2]);
                System.Drawing.Color color4 = ColorTranslator.FromHtml(BattleLevelButtonColor.Split(';')[3]);
                if (((color.R == color1.R) && (color.G == color1.G) && (color.B == color1.B)) || // 황금색
                    ((color.R == color2.R) && (color.G == color2.G) && (color.B == color2.B)))   // + 메뉴 음영
                {
                    ClickLog("Battle Level Button");
                    AdsStopwatch.Reset();
                    AdsStopwatch.Stop();

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
                else if ((color.R == color3.R) && (color.G == color3.G) && (color.B == color3.B)) // 빨간색
                {
                    ClickLog("Battle Level Button is Red");
                    return;
                }
                else if ((color.R == color4.R) && (color.G == color4.G) && (color.B == color4.B)) // 바탕색
                {
                    ClickLog("Battle Level Button is disappered");
                    return;
                }


                // Skill Button
                color = CurrentBitmap.GetPixel(SkillButtonX, SkillButtonY);
                TimerLog("Skill Button Color: " + color.R + "," + color.G + "," + color.B);
                color1 = ColorTranslator.FromHtml(SkillButtonColor);
                if ((color.R == color1.R) && (color.G == color1.G) && (color.B == color1.B))
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
                color1 = ColorTranslator.FromHtml(SpeedButtonColor);
                if ((color.R == color1.R) && (color.G == color1.G) && (color.B == color1.B))
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
                    color1 = ColorTranslator.FromHtml(PauseButtonColor);
                    if ((color.R == color1.R) && (color.G == color1.G) && (color.B == color1.B))
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
                color1 = ColorTranslator.FromHtml(VictoryDefeatColor.Split(';')[0]);
                color2 = ColorTranslator.FromHtml(VictoryDefeatColor.Split(';')[1]);
                if ((color.R == color1.R) && (color.G == color1.G) && (color.B == color1.B))
                {
                    ClickLog("Victory Checked");
                    VictoryFlag = true;
                }
                else if ((color.R == color2.R) && (color.G == color2.G) && (color.B == color2.B))
                {
                    ClickLog("Defeat Checked");
                    DefeatFlag = true;
                }


                // Gold Button
                color = CurrentBitmap.GetPixel(GoldButtonBackgroundX, GoldButtonBackgroundY);
                TimerLog("Gold Button Background Color: " + color.R + "," + color.G + "," + color.B);
                color1 = ColorTranslator.FromHtml(GoldButtonBackgroundColor.Split(';')[0]);
                color2 = ColorTranslator.FromHtml(GoldButtonBackgroundColor.Split(';')[1]);
                if ((color.R == color1.R) && (color.G == color1.G) && (color.B == color1.B)) // Green
                {
                    color = CurrentBitmap.GetPixel(GoldButtonImageX, GoldButtonImageY);
                    TimerLog("Gold Button Image Color: " + color.R + "," + color.G + "," + color.B);
                    color1 = ColorTranslator.FromHtml(GoldButtonImageColor);
                    if (!IsLatest || ((color.R == color1.R) && (color.G == color1.G) && (color.B == color1.B))) // Yellow
                    {
                        ClickLog("Gold Button");
                        AdsFlag = true;
                        AdsStopwatch.Restart();

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
                else if ((color.R == color2.R) && (color.G == color2.G) && (color.B == color2.B)) // Gray
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
                color1 = ColorTranslator.FromHtml(NextButtonColor);
                if (IsLatest && ((color.R == color1.R) && (color.G == color1.G) && (color.B == color1.B)))
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
                color1 = ColorTranslator.FromHtml(NotRespondingColor);
                if ((color.R == color1.R) && (color.G == color1.G) && (color.B == color1.B))
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
                color1 = ColorTranslator.FromHtml(AdsButtonColor.Split(';')[0]);
                color2 = ColorTranslator.FromHtml(AdsButtonColor.Split(';')[1]);
                if (((color.R == color1.R) && (color.G == color1.G) && (color.B == color1.B)) ||
                    ((color.R == color2.R) && (color.G == color2.G) && (color.B == color2.B)))
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
                color1 = ColorTranslator.FromHtml(AdsButtonColor.Split(';')[0]);
                color2 = ColorTranslator.FromHtml(AdsButtonColor.Split(';')[1]);
                if (((color.R == color1.R) && (color.G == color1.G) && (color.B == color1.B)) ||
                    ((color.R == color2.R) && (color.G == color2.G) && (color.B == color2.B)))
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

                    TimerLog("Screen is changed: " + isDifferent + ", AdsStopwatchElapsed: " + AdsStopwatch.ElapsedMilliseconds);

                    if (!isDifferent || (AdsStopwatch.ElapsedMilliseconds > 32000))
                    {
                        ClickLog("Ads Close Button");
                        AdsStopwatch.Reset();
                        AdsStopwatch.Stop();

                        bool isLeftFirst  = true;

                        System.Windows.Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
                        {
                            if (RightAdsRadioButton.IsChecked.Value)
                            {
                                isLeftFirst  = false;
                            }
                        }));

                        if (!isLeftFirst)
                        {
                            System.Windows.Forms.Cursor.Position = new System.Drawing.Point((int)NoxPointX + AdsCloseButton2X, (int)NoxPointY + AdsCloseButtonY);
                            mouse_event(LBUTTONDOWN, 0, 0, 0, 0);
                            System.Threading.Thread.Sleep(50);
                            mouse_event(LBUTTONUP, 0, 0, 0, 0);

                            System.Threading.Thread.Sleep(500);
                        }

                        System.Windows.Forms.Cursor.Position = new System.Drawing.Point((int)NoxPointX + AdsCloseButton1X, (int)NoxPointY + AdsCloseButtonY);
                        mouse_event(LBUTTONDOWN, 0, 0, 0, 0);
                        System.Threading.Thread.Sleep(50);
                        mouse_event(LBUTTONUP, 0, 0, 0, 0);

                        System.Threading.Thread.Sleep(500);

                        if (isLeftFirst)
                        {
                            System.Windows.Forms.Cursor.Position = new System.Drawing.Point((int)NoxPointX + AdsCloseButton2X, (int)NoxPointY + AdsCloseButtonY);
                            mouse_event(LBUTTONDOWN, 0, 0, 0, 0);
                            System.Threading.Thread.Sleep(50);
                            mouse_event(LBUTTONUP, 0, 0, 0, 0);
                        }

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
            SettingWindow settingWindow = new SettingWindow();
            settingWindow.AppLocationX.Text = AppLocationX.ToString();
            settingWindow.AppLocationY.Text = AppLocationY.ToString();
            settingWindow.AppLocationColor.Text = AppLocationColor;
            settingWindow.AppLocationButton1.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(AppLocationColor.Split(';')[0]));
            settingWindow.AppLocationButton2.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(AppLocationColor.Split(';')[1]));

            settingWindow.HomeButtonX.Text = HomeButtonX.ToString();
            settingWindow.HomeButtonY.Text = ShopButtonY.ToString();

            settingWindow.ShopButtonX.Text = ShopButtonX.ToString();
            settingWindow.ShopButtonY.Text = ShopButtonY.ToString();
            settingWindow.ShopButtonColor.Text = ShopButtonColor;
            settingWindow.ShopButton1.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(ShopButtonColor));

            settingWindow.MiddleButtonX.Text = MiddleButtonX.ToString();
            settingWindow.MiddleButtonY.Text = MiddleButtonY.ToString();
            settingWindow.MiddleButtonColor.Text = MiddleButtonColor;
            settingWindow.MiddleButton1.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(MiddleButtonColor));

            settingWindow.GoldChestBoxX.Text = GoldChestBoxX.ToString();
            settingWindow.GoldChestBoxY.Text = GoldChestBoxY.ToString();
            settingWindow.GoldChestBoxColor.Text = GoldChestBoxColor;
            settingWindow.GoldChestBox1.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(GoldChestBoxColor.Split(';')[0]));
            settingWindow.GoldChestBox2.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(GoldChestBoxColor.Split(';')[1]));

            settingWindow.BattleLevelButtonX.Text = BattleLevelButtonX.ToString();
            settingWindow.BattleLevelButtonY.Text = BattleLevelButtonY.ToString();
            settingWindow.BattleLevelButtonColor.Text = BattleLevelButtonColor;
            settingWindow.BattleLevelButton1.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(BattleLevelButtonColor.Split(';')[0]));
            settingWindow.BattleLevelButton2.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(BattleLevelButtonColor.Split(';')[1]));
            settingWindow.BattleLevelButton3.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(BattleLevelButtonColor.Split(';')[2]));
            settingWindow.BattleLevelButton4.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(BattleLevelButtonColor.Split(';')[3]));

            settingWindow.SkillButtonX.Text = SkillButtonX.ToString();
            settingWindow.SkillButtonY.Text = SkillButtonY.ToString();
            settingWindow.SkillButtonColor.Text = SkillButtonColor;
            settingWindow.SkillButton1.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(SkillButtonColor));

            settingWindow.SpeedButtonX.Text = SpeedButtonX.ToString();
            settingWindow.SpeedButtonY.Text = SpeedButtonY.ToString();
            settingWindow.SpeedButtonColor.Text = SpeedButtonColor;
            settingWindow.SpeedButton1.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(SpeedButtonColor));

            settingWindow.PauseButtonX.Text = PauseButtonX.ToString();
            settingWindow.PauseButtonY.Text = PauseButtonY.ToString();
            settingWindow.PauseButtonColor.Text = PauseButtonColor;
            settingWindow.PauseButton1.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(PauseButtonColor));

            settingWindow.VictoryDefeatX.Text = VictoryDefeatX.ToString();
            settingWindow.VictoryDefeatY.Text = VictoryDefeatY.ToString();
            settingWindow.VictoryDefeatColor.Text = VictoryDefeatColor;
            settingWindow.VictoryDefeat1.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(VictoryDefeatColor.Split(';')[0]));
            settingWindow.VictoryDefeat2.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(VictoryDefeatColor.Split(';')[1]));

            settingWindow.GoldButtonBackgroundX.Text = GoldButtonBackgroundX.ToString();
            settingWindow.GoldButtonBackgroundY.Text = GoldButtonBackgroundY.ToString();
            settingWindow.GoldButtonBackgroundColor.Text = GoldButtonBackgroundColor;
            settingWindow.GoldButtonBackground1.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(GoldButtonBackgroundColor.Split(';')[0]));
            settingWindow.GoldButtonBackground2.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(GoldButtonBackgroundColor.Split(';')[1]));

            settingWindow.GoldButtonImageX.Text = GoldButtonImageX.ToString();
            settingWindow.GoldButtonImageY.Text = GoldButtonImageY.ToString();
            settingWindow.GoldButtonImageColor.Text = GoldButtonImageColor;
            settingWindow.GoldButtonImage1.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(GoldButtonImageColor));

            settingWindow.NextButtonX.Text = NextButtonX.ToString();
            settingWindow.NextButtonY.Text = NextButtonY.ToString();
            settingWindow.NextButtonColor.Text = NextButtonColor;
            settingWindow.NextButton1.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(NextButtonColor));

            settingWindow.AdsButton1X.Text = AdsButtonX.ToString();
            settingWindow.AdsButton1Y.Text = AdsButton1Y.ToString();
            settingWindow.AdsButton1Color.Text = AdsButtonColor;
            settingWindow.AdsButton1_1.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(AdsButtonColor.Split(';')[0]));

            settingWindow.AdsButton2X.Text = AdsButtonX.ToString();
            settingWindow.AdsButton2Y.Text = AdsButton2Y.ToString();
            settingWindow.AdsButton2Color.Text = AdsButtonColor;
            settingWindow.AdsButton2_1.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(AdsButtonColor.Split(';')[1]));

            settingWindow.AdsCloseButton1X.Text = AdsCloseButton1X.ToString();
            settingWindow.AdsCloseButton1Y.Text = AdsCloseButtonY.ToString();

            settingWindow.AdsCloseButton2X.Text = AdsCloseButton2X.ToString();
            settingWindow.AdsCloseButton2Y.Text = AdsCloseButtonY.ToString();

            settingWindow.NotRespondingX.Text = NotRespondingX.ToString();
            settingWindow.NotRespondingY.Text = NotRespondingY.ToString();
            settingWindow.NotRespondingColor.Text = NotRespondingColor.ToString();
            settingWindow.NotResponding1.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(NotRespondingColor));

            settingWindow.ShowDialog();
        }
    }
}
