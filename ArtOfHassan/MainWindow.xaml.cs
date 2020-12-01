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

        private static System.Timers.Timer ButtonTimer = new System.Timers.Timer();

        double NoxPointX = 0;
        double NoxPointY = 0;
        double NoxWidth  = 0;
        double NoxHeight = 0;

        int TimerCount = 1;
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
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            ButtonTimer.Enabled = false;
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
            return FindWindow(null, WindowTextBox.Text);
        }

        private void GetWindowPos(IntPtr hwnd, ref System.Windows.Point point, ref System.Windows.Size size)
        {
            WINDOWPLACEMENT placement = new WINDOWPLACEMENT();
            placement.length = System.Runtime.InteropServices.Marshal.SizeOf(placement);

            GetWindowPlacement(hwnd, ref placement);

            size = new System.Windows.Size(placement.normal_position.Right - (placement.normal_position.Left * 2), placement.normal_position.Bottom - (placement.normal_position.Top * 2));
            point = new System.Windows.Point(placement.normal_position.Left, placement.normal_position.Top);
        }

        private void FindButton_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Point point = new System.Windows.Point();
            System.Windows.Size size = new System.Windows.Size();

            GetWindowPos(GetWinAscHandle(), ref point, ref size);

            if ((size.Width != 0) && (size.Height != 0))
            {
                NoxPointX = point.X;
                NoxPointY = point.Y;
                NoxWidth  = size.Width;
                NoxHeight = size.Height;

                StartButton.IsEnabled = true;
            }

            ClickLog("NoxPointX: " + NoxPointX);
            ClickLog("NoxPointY: " + NoxPointY);
            ClickLog("NoxWidth: "  + NoxWidth);
            ClickLog("NoxHeight: " + NoxHeight);
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


        int NoxWallpaperX   = 100;
        int NoxWallpaperY   = 865;
        int AppLocationX    = 60;
        int AppLocationY    = 500;

        int BattleLevelButtonX = 180;
        int BattleLevelButtonY = 855;

        int SkillButtonX    = 475;
        int SkillButtonY    = 920;
        int SpeedButtonX    = 514;
        int SpeedButtonY    = 989;

        int VictoryButton1X = 115;
        int VictoryButton1Y = 780;
        int VictoryButton2X = 133;
        int VictoryButton2Y = 755;
        int DefeatButtonX   = 450;
        int DefeatButtonY   = 710;

        int AdsButtonX      = 496;
        int AdsButton1Y     = 180;
        int AdsButton2Y     = 190;
        int GoogleButton1X  = 39;
        int GoogleButton2X  = 519;
        int GoogleButtonY   = 68;

        int ProblemButtonX  = 195;
        int ProblemButtonY  = 680;


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
                System.Drawing.Color color = CurrentBitmap.GetPixel(NoxWallpaperX, NoxWallpaperY);
                TimerLog("Off Check Color: " + color.R + "," + color.G + "," + color.B);
                if ((color.R == 47) && (color.G == 46) && (color.B == 72))
                {
                    ClickLog("Off Check");
                    System.Windows.Forms.Cursor.Position = new System.Drawing.Point((int)NoxPointX + AppLocationX, (int)NoxPointY + AppLocationY);
                    mouse_event(LBUTTONDOWN, 0, 0, 0, 0);
                    System.Threading.Thread.Sleep(50);
                    mouse_event(LBUTTONUP, 0, 0, 0, 0);
                }


                // Battle and Level Button
                color = CurrentBitmap.GetPixel(BattleLevelButtonX, BattleLevelButtonY);
                TimerLog("Battle Level Button Color: " + color.R + "," + color.G + "," + color.B);
                if ((color.R == 253) && (color.G == 187) && (color.B == 0)) // 황금색
                {
                    ClickLog("Battle Level Button");
                    System.Windows.Forms.Cursor.Position = new System.Drawing.Point((int)NoxPointX + BattleLevelButtonX, (int)NoxPointY + BattleLevelButtonY);
                    mouse_event(LBUTTONDOWN, 0, 0, 0, 0);
                    System.Threading.Thread.Sleep(50);
                    mouse_event(LBUTTONUP, 0, 0, 0, 0);
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


                // Victory Coin Button
                color = CurrentBitmap.GetPixel(VictoryButton1X, VictoryButton1Y);
                TimerLog("Victory Coin Button1 Color: " + color.R + "," + color.G + "," + color.B);
                if ((color.R == 125) && (color.G == 167) && (color.B == 10)) // Green
                {
                    color = CurrentBitmap.GetPixel(VictoryButton2X, VictoryButton2Y);
                    TimerLog("Victory Coin Button2 Color: " + color.R + "," + color.G + "," + color.B);
                    if ((color.R == 255) && (color.G == 234) && (color.B == 144)) // Yellow
                    {
                        ClickLog("Victory Coin Button");

                        System.Windows.Forms.Cursor.Position = new System.Drawing.Point((int)NoxPointX + VictoryButton1X, (int)NoxPointY + VictoryButton1Y);
                        mouse_event(LBUTTONDOWN, 0, 0, 0, 0);
                        System.Threading.Thread.Sleep(50);
                        mouse_event(LBUTTONUP, 0, 0, 0, 0);
                    }
                    else // Green such as Retry
                    {
                        ClickLog("Victory Retry Button");

                        System.Windows.Forms.Cursor.Position = new System.Drawing.Point((int)NoxPointX + DefeatButtonX, (int)NoxPointY + VictoryButton1Y);
                        mouse_event(LBUTTONDOWN, 0, 0, 0, 0);
                        System.Threading.Thread.Sleep(50);
                        mouse_event(LBUTTONUP, 0, 0, 0, 0);
                    }
                }
                else if ((color.R == 142) && (color.G == 142) && (color.B == 142)) // Gray
                {
                    ClickLog("Victory Coin Button is Gray");

                    System.Windows.Forms.Cursor.Position = new System.Drawing.Point((int)NoxPointX + DefeatButtonX, (int)NoxPointY + VictoryButton1Y);
                    mouse_event(LBUTTONDOWN, 0, 0, 0, 0);
                    System.Threading.Thread.Sleep(50);
                    mouse_event(LBUTTONUP, 0, 0, 0, 0);
                }


                // Defeat Retry Button
                color = CurrentBitmap.GetPixel(DefeatButtonX, DefeatButtonY);
                TimerLog("Defeat Retry Button Color: " + color.R + "," + color.G + "," + color.B);
                if ((color.R == 253) && (color.G == 187) && (color.B == 0))
                {
                    ClickLog("Defeat Retry Button");
                    System.Windows.Forms.Cursor.Position = new System.Drawing.Point((int)NoxPointX + DefeatButtonX, (int)NoxPointY + DefeatButtonY);
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
                }


                // Google Button
                int googleTime = 3;
                System.Windows.Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
                {
                    if (!int.TryParse(GoogleTime.Text, out googleTime))
                    {
                        googleTime = 3;
                    }
                }));
                TimerLog("GoogleTime: " + googleTime);
                if ((TimerCount % googleTime) == 0)
                {
                    TimerCount = 1;

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
                        ClickLog("Google Button");

                        System.Windows.Forms.Cursor.Position = new System.Drawing.Point((int)NoxPointX + GoogleButton1X, (int)NoxPointY + GoogleButtonY);
                        mouse_event(LBUTTONDOWN, 0, 0, 0, 0);
                        System.Threading.Thread.Sleep(50);
                        mouse_event(LBUTTONUP, 0, 0, 0, 0);

                        System.Threading.Thread.Sleep(500);

                        System.Windows.Forms.Cursor.Position = new System.Drawing.Point((int)NoxPointX + GoogleButton2X, (int)NoxPointY + GoogleButtonY);
                        mouse_event(LBUTTONDOWN, 0, 0, 0, 0);
                        System.Threading.Thread.Sleep(50);
                        mouse_event(LBUTTONUP, 0, 0, 0, 0);

                        return;
                    }
                }


                // Problem Button
                color = CurrentBitmap.GetPixel(ProblemButtonX, ProblemButtonY);
                TimerLog("Problem Button Color: " + color.R + "," + color.G + "," + color.B);
                if ((color.R == 253) && (color.G == 187) && (color.B == 0))
                {
                    ClickLog("Problem Button");
                    System.Windows.Forms.Cursor.Position = new System.Drawing.Point((int)NoxPointX + ProblemButtonX, (int)NoxPointY + ProblemButtonY);
                    mouse_event(LBUTTONDOWN, 0, 0, 0, 0);
                    System.Threading.Thread.Sleep(50);
                    mouse_event(LBUTTONUP, 0, 0, 0, 0);
                }


                //CurrentBitmap.Save(Stage + ".png", System.Drawing.Imaging.ImageFormat.Png);
                LastBitmap = CurrentBitmap;
                TimerCount++;
            }
        }
    }
}
