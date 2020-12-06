﻿using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Mail;
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

        private static System.Timers.Timer NoxTimer     = new System.Timers.Timer();
        private static System.Timers.Timer ButtonTimer  = new System.Timers.Timer();
        private static System.Timers.Timer ProblemTimer = new System.Timers.Timer();

        Stopwatch AdsStopwatch    = new Stopwatch();
        Stopwatch DelayStopwatch  = new Stopwatch();
        Stopwatch BattleStopwatch = new Stopwatch();

        double NoxPointX = 0;
        double NoxPointY = 0;
        double NoxWidth  = 0;
        double NoxHeight = 0;

        bool VictoryFlag = false;
        bool DefeatFlag  = false;
        bool AdsFlag     = false;

        bool IsNoGoldMailSent = false;

        int NumOfWar     = 0;
        int NumOfVictory = 0;
        int NumOfDefeat  = 0;
        int NumOfAds     = 0;

        int ProblemMailSent = 0;
        int TimerCountForScreenCompare = 1;

        System.Drawing.Bitmap LastBitmap;
        System.Drawing.Bitmap CurrentBitmap;

        public System.Drawing.Color PixelColor;
        public int PixelPositionX;
        public int PixelPositionY;

        public string ClickPattern = "L;R;L";


        public MainWindow()
        {
            InitializeComponent();

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

                // 예외발생 임시처리 나중에 삭제할것
                if (lines.Length == 16)
                {
                    NoGoldX = int.Parse(lines[12].Split(',')[0]);
                    NoGoldY = int.Parse(lines[12].Split(',')[1]);
                    NoGoldColor = lines[12].Split(',')[2];

                    GameAdCloseButtonX = int.Parse(lines[13].Split(',')[0]);
                    GoldAdCloseButtonY = int.Parse(lines[13].Split(',')[1]);
                    TroopAdCloseButtonY = int.Parse(lines[13].Split(',')[2]);
                    GameAdCloseButtonColor = lines[13].Split(',')[3];

                    LeftAdCloseButtonX = int.Parse(lines[14].Split(',')[0]);
                    RightAdCloseButtonX = int.Parse(lines[14].Split(',')[1]);
                    GoogleAdCloseButtonY = int.Parse(lines[14].Split(',')[2]);
                    // 예외발생 임시처리 나중에 삭제할것
                    if (lines[14].Split(',').Length < 4)
                    {
                        GoogleAdCloseButtonColor = "#4c4c4f;#3c4043".ToUpper();
                    }
                    else
                    {
                        GoogleAdCloseButtonColor = lines[14].Split(',')[3];
                    }

                    NotRespondingX = int.Parse(lines[15].Split(',')[0]);
                    NotRespondingY = int.Parse(lines[15].Split(',')[1]);
                    NotRespondingColor = lines[15].Split(',')[2];
                }
                else
                {
                    GameAdCloseButtonX = int.Parse(lines[12].Split(',')[0]);
                    GoldAdCloseButtonY = int.Parse(lines[12].Split(',')[1]);
                    TroopAdCloseButtonY = int.Parse(lines[12].Split(',')[2]);
                    GameAdCloseButtonColor = lines[12].Split(',')[3];

                    LeftAdCloseButtonX = int.Parse(lines[13].Split(',')[0]);
                    RightAdCloseButtonX = int.Parse(lines[13].Split(',')[1]);
                    GoogleAdCloseButtonY = int.Parse(lines[13].Split(',')[2]);
                    // 예외발생 임시처리 나중에 삭제할것
                    if (lines[13].Split(',').Length < 4)
                    {
                        GoogleAdCloseButtonColor = "#4c4c4f;#3c4043".ToUpper();
                    }
                    else
                    {
                        GoogleAdCloseButtonColor = lines[13].Split(',')[3];
                    }

                    NotRespondingX = int.Parse(lines[14].Split(',')[0]);
                    NotRespondingY = int.Parse(lines[14].Split(',')[1]);
                    NotRespondingColor = lines[14].Split(',')[2];
                }

                ClickLog("Load Done");
            }

            ProblemTimer.Interval = 5 * 60 * 1000; // 5분
            ProblemTimer.Elapsed += ProblemTimerFunction;

            ButtonTimer.Interval = 1000; // 1초
            ButtonTimer.Elapsed += ButtonTimerFunction;

            NoxTimer.Interval = 200;
            NoxTimer.Elapsed += NoxTimerFunction;
            NoxTimer.Enabled = true;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            ProblemTimer.Enabled = false;
            ButtonTimer.Enabled = false;
            NoxTimer.Enabled    = false;
        }

        private void ClickLog(string log)
        {
            bool isLogging = false;
            System.Windows.Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
            {
                isLogging = LogCheckBox.IsChecked.Value;
            }));

            if (isLogging)
            {
                DirectoryInfo directoryInfo = new DirectoryInfo("log");
                if (!directoryInfo.Exists)
                {
                    directoryInfo.Create();
                }

                using (StreamWriter streamWriter = new StreamWriter($@"log\Click_{DateTime.Today.ToString("yyyyMMdd")}.log", true))
                {
                    streamWriter.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff ") + log);
                }
            }
        }

        private void TimerLog(string log)
        {
            bool isLogging = false;
            System.Windows.Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
            {
                isLogging = LogCheckBox.IsChecked.Value;
            }));

            if (isLogging)
            {
                DirectoryInfo directoryInfo = new DirectoryInfo("log");
                if (!directoryInfo.Exists)
                {
                    directoryInfo.Create();
                }

                using (StreamWriter streamWriter = new StreamWriter($@"log\Timer_{DateTime.Today.ToString("yyyyMMdd")}.log", true))
                {
                    streamWriter.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff ") + log);
                }
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
                MonitoringIntervalTextBox.IsEnabled = false;

                ButtonTimer.Interval = int.Parse(MonitoringIntervalTextBox.Text);
                ButtonTimer.Enabled = true;
                ProblemTimer.Enabled = true;

                IsNoGoldMailSent = false;
                ProblemMailSent = 0;
            }
            else
            {
                StartButton.Content = "Start";
                SettingButton.IsEnabled = true;
                MonitoringIntervalTextBox.IsEnabled = true;

                ButtonTimer.Enabled = false;
                ProblemTimer.Enabled = false;
            }
        }

        private void AdsClosePatternButton_Click(object sender, RoutedEventArgs e)
        {
            ClickPatternWindow clickPatternWindow = new ClickPatternWindow();
            clickPatternWindow.ShowDialog();
        }

        private void EmailTestButton_Click(object sender, RoutedEventArgs e)
        {
            ClickLog("Email Testing...");

            try
            {
                if (!string.IsNullOrWhiteSpace(EmailTextBox.Text))
                {
                    MailMessage mailMessage = new MailMessage("artofwarhassan@gmail.com",
                                                              EmailTextBox.Text,
                                                              "Art of Hassan",
                                                              "Email Testing...\nPlease check.");

                    SmtpClient smtpClient = new SmtpClient("smtp.gmail.com", 587);
                    smtpClient.UseDefaultCredentials = false;
                    smtpClient.EnableSsl = true;
                    smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
                    smtpClient.Credentials = new NetworkCredential("artofwarhassan@gmail.com",
                                                                   "Rnrmf0575!");
                    smtpClient.Send(mailMessage);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Email Delivery Failed");
            }
        }

        private void ProblemTimerFunction(object sender, System.Timers.ElapsedEventArgs e)
        {
            if ((NumOfVictory + NumOfDefeat) == NumOfWar)
            {
                ClickLog("Problem Occured...");

                if (ProblemMailSent < 10)
                {
                    CurrentBitmap.Save("Problem.png", System.Drawing.Imaging.ImageFormat.Png);

                    MailMessage mailMessage = new MailMessage("artofwarhassan@gmail.com",
                                                              "artofwarhassan@gmail.com",
                                                              "Art of Hassan",
                                                              "Problem occured.\nPlease check.");
                    mailMessage.Attachments.Add(new System.Net.Mail.Attachment("Problem.png"));

                    SmtpClient smtpClient = new SmtpClient("smtp.gmail.com", 587);
                    smtpClient.UseDefaultCredentials = false;
                    smtpClient.EnableSsl = true;
                    smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
                    smtpClient.Credentials = new NetworkCredential("artofwarhassan@gmail.com",
                                                                   "Rnrmf0575!");
                    smtpClient.Send(mailMessage);

                    try
                    {
                        string emailaddress = "";
                        System.Windows.Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
                        {
                            emailaddress = EmailTextBox.Text;
                        }));
                        if (!string.IsNullOrWhiteSpace(emailaddress))
                        {
                            mailMessage = new MailMessage("artofwarhassan@gmail.com",
                                                          emailaddress,
                                                          "Art of Hassan",
                                                          "Problem occured.\nPlease check.");
                            mailMessage.Attachments.Add(new System.Net.Mail.Attachment("Problem.png"));

                            smtpClient = new SmtpClient("smtp.gmail.com", 587);
                            smtpClient.UseDefaultCredentials = false;
                            smtpClient.EnableSsl = true;
                            smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
                            smtpClient.Credentials = new NetworkCredential("artofwarhassan@gmail.com",
                                                                           "Rnrmf0575!");
                            smtpClient.Send(mailMessage);
                        }
                    }
                    catch (Exception ex)
                    {
                        //MessageBox.Show("Email Delivery Failed");
                    }
                }

                ProblemMailSent++;
            }
            else
            {
                NumOfWar = NumOfVictory + NumOfDefeat;
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

        public int GameAdCloseButtonX       = 496;
        public int GoldAdCloseButtonY      = 180;
        public int TroopAdCloseButtonY      = 190;
        public string GameAdCloseButtonColor = "#e9e9d8;#efe7d6".ToUpper();

        public int LeftAdCloseButtonX = 45;
        public int RightAdCloseButtonX = 513;
        public int GoogleAdCloseButtonY  = 63;
        public string GoogleAdCloseButtonColor = "#4c4c4f;#3c4043".ToUpper();

        public int NotRespondingX   = 79;
        public int NotRespondingY   = 540;
        public string NotRespondingColor = "#009688".ToUpper();

        public int NoGoldX = 320;
        public int NoGoldY = 646;
        public string NoGoldColor = "#dfd6be".ToUpper();


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
                    ProblemTimer.Enabled  = false;
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
            int pixelDifference = 0;
            System.Windows.Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
            {
                if (!int.TryParse(PixelDifferenceTextBox.Text, out pixelDifference))
                {
                    pixelDifference = 0;
                }
            }));

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
                if ((((color.R >= color1.R - pixelDifference) && (color.G >= color1.G - pixelDifference) && (color.B >= color1.B - pixelDifference)) &&
                     ((color.R <= color1.R + pixelDifference) && (color.G <= color1.G + pixelDifference) && (color.B <= color1.B + pixelDifference))) ||
                    (((color.R >= color2.R - pixelDifference) && (color.G >= color2.G - pixelDifference) && (color.B >= color2.B - pixelDifference)) &&
                     ((color.R <= color2.R + pixelDifference) && (color.G <= color2.G + pixelDifference) && (color.B <= color2.B + pixelDifference))))
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
                if (((color.R >= color1.R - pixelDifference) && (color.G >= color1.G - pixelDifference) && (color.B >= color1.B - pixelDifference)) &&
                    ((color.R <= color1.R + pixelDifference) && (color.G <= color1.G + pixelDifference) && (color.B <= color1.B + pixelDifference)))
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
                if (((color.R >= color1.R - pixelDifference) && (color.G >= color1.G - pixelDifference) && (color.B >= color1.B - pixelDifference)) &&
                    ((color.R <= color1.R + pixelDifference) && (color.G <= color1.G + pixelDifference) && (color.B <= color1.B + pixelDifference)))
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
                    if ((((color.R >= color1.R - pixelDifference) && (color.G >= color1.G - pixelDifference) && (color.B >= color1.B - pixelDifference)) &&
                         ((color.R <= color1.R + pixelDifference) && (color.G <= color1.G + pixelDifference) && (color.B <= color1.B + pixelDifference))) || // Latest
                        (((color.R >= color2.R - pixelDifference) && (color.G >= color2.G - pixelDifference) && (color.B >= color2.B - pixelDifference)) &&
                         ((color.R <= color2.R + pixelDifference) && (color.G <= color2.G + pixelDifference) && (color.B <= color2.B + pixelDifference))))   // 3.0.8
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
                if ((((color.R >= color1.R - pixelDifference) && (color.G >= color1.G - pixelDifference) && (color.B >= color1.B - pixelDifference)) &&
                     ((color.R <= color1.R + pixelDifference) && (color.G <= color1.G + pixelDifference) && (color.B <= color1.B + pixelDifference))) || // 황금색
                    (((color.R >= color2.R - pixelDifference) && (color.G >= color2.G - pixelDifference) && (color.B >= color2.B - pixelDifference)) &&
                     ((color.R <= color2.R + pixelDifference) && (color.G <= color2.G + pixelDifference) && (color.B <= color2.B + pixelDifference))))   // + 메뉴 음영
                {
                    ClickLog("Battle Level Button");
                    AdsStopwatch.Reset();
                    AdsStopwatch.Stop();
                    DelayStopwatch.Reset();
                    DelayStopwatch.Stop();
                    BattleStopwatch.Reset();
                    BattleStopwatch.Stop();

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

                    if (BattleStopwatch.IsRunning)
                    {
                        if (BattleStopwatch.ElapsedMilliseconds > 30000)
                        {
                            ClickLog("Battle Level Cancel Button");
                            BattleStopwatch.Reset();
                            BattleStopwatch.Stop();

                            System.Windows.Forms.Cursor.Position = new System.Drawing.Point((int)NoxPointX + BattleLevelButtonX, (int)NoxPointY + BattleLevelButtonY);
                            mouse_event(LBUTTONDOWN, 0, 0, 0, 0);
                            System.Threading.Thread.Sleep(50);
                            mouse_event(LBUTTONUP, 0, 0, 0, 0);
                        }
                    }
                    else
                    {
                        BattleStopwatch.Restart();
                    }

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
                if (((color.R >= color1.R - pixelDifference) && (color.G >= color1.G - pixelDifference) && (color.B >= color1.B - pixelDifference)) &&
                    ((color.R <= color1.R + pixelDifference) && (color.G <= color1.G + pixelDifference) && (color.B <= color1.B + pixelDifference)))
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
                if (((color.R >= color1.R - pixelDifference) && (color.G >= color1.G - pixelDifference) && (color.B >= color1.B - pixelDifference)) &&
                    ((color.R <= color1.R + pixelDifference) && (color.G <= color1.G + pixelDifference) && (color.B <= color1.B + pixelDifference)))
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
                    if (((color.R >= color1.R - pixelDifference) && (color.G >= color1.G - pixelDifference) && (color.B >= color1.B - pixelDifference)) &&
                        ((color.R <= color1.R + pixelDifference) && (color.G <= color1.G + pixelDifference) && (color.B <= color1.B + pixelDifference)))
                    {
                        ClickLog("Pause Button");
                        System.Windows.Forms.Cursor.Position = new System.Drawing.Point((int)NoxPointX + PauseButtonX, (int)NoxPointY + PauseButtonY);
                        mouse_event(LBUTTONDOWN, 0, 0, 0, 0);
                        System.Threading.Thread.Sleep(50);
                        mouse_event(LBUTTONUP, 0, 0, 0, 0);
                    }
                }


                int DelayCriteria = 0; // X3 Gold Button Delay


                // Check Victory or Defeat
                color = CurrentBitmap.GetPixel(VictoryDefeatX, VictoryDefeatY);
                TimerLog("Victory or Defeat Color: " + color.R + "," + color.G + "," + color.B);
                color1 = ColorTranslator.FromHtml(VictoryDefeatColor.Split(';')[0]);
                color2 = ColorTranslator.FromHtml(VictoryDefeatColor.Split(';')[1]);
                if (((color.R >= color1.R - pixelDifference) && (color.G >= color1.G - pixelDifference) && (color.B >= color1.B - pixelDifference)) &&
                    ((color.R <= color1.R + pixelDifference) && (color.G <= color1.G + pixelDifference) && (color.B <= color1.B + pixelDifference)))
                {
                    ClickLog("Victory Checked");
                    VictoryFlag = true;

                    System.Windows.Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
                    {
                        if (!int.TryParse(DelayTextBox.Text, out DelayCriteria))
                        {
                            DelayCriteria = 0;
                        }

                        if (DelayCriteria < int.Parse(MonitoringIntervalTextBox.Text))
                        {
                            DelayCriteria = 0;
                            System.Threading.Thread.Sleep(DelayCriteria);
                        }
                    }));
                }
                else if (((color.R >= color2.R - pixelDifference) && (color.G >= color2.G - pixelDifference) && (color.B >= color2.B - pixelDifference)) &&
                         ((color.R <= color2.R + pixelDifference) && (color.G <= color2.G + pixelDifference) && (color.B <= color2.B + pixelDifference)))
                {
                    ClickLog("Defeat Checked");
                    DefeatFlag = true;
                }


                // No Gold
                color = CurrentBitmap.GetPixel(NoGoldX, NoGoldY);
                TimerLog("No Gold Color: " + color.R + "," + color.G + "," + color.B);
                color1 = ColorTranslator.FromHtml(NoGoldColor);
                if (((color.R >= color1.R - pixelDifference) && (color.G >= color1.G - pixelDifference) && (color.B >= color1.B - pixelDifference)) &&
                    ((color.R <= color1.R + pixelDifference) && (color.G <= color1.G + pixelDifference) && (color.B <= color1.B + pixelDifference)))
                {
                    ClickLog("No Gold");

                    bool isSendEmail = false;
                    bool isStopHassan = false;
                    bool isShutdownPC = false;
                    System.Windows.Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
                    {
                        isSendEmail  = SendEmailCheckBox.IsChecked.Value;
                        isStopHassan = StopWorkingCheckBox.IsChecked.Value;
                        isShutdownPC = ShutdownPCCheckBox.IsChecked.Value;
                    }));

                    if (isSendEmail && !IsNoGoldMailSent)
                    {
                        ClickLog("Sending Email...");
                        IsNoGoldMailSent = true;

                        try
                        {
                            string emailaddress = "";
                            System.Windows.Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
                            {
                                emailaddress = EmailTextBox.Text;
                            }));
                            if (!string.IsNullOrWhiteSpace(emailaddress))
                            {
                                MailMessage mailMessage = new MailMessage("artofwarhassan@gmail.com",
                                                                          emailaddress,
                                                                          "Art of Hassan",
                                                                          "No Gold.\nPlease check.");
                                SmtpClient smtpClient = new SmtpClient("smtp.gmail.com", 587);
                                smtpClient.UseDefaultCredentials = false;
                                smtpClient.EnableSsl = true;
                                smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
                                smtpClient.Credentials = new NetworkCredential("artofwarhassan@gmail.com",
                                                                               "Rnrmf0575!");
                                smtpClient.Send(mailMessage);
                            }
                        }
                        catch (Exception ex)
                        {
                            //MessageBox.Show("Email Delivery Failed");
                        }
                    }

                    if (isStopHassan)
                    {
                        ClickLog("Stop Hassan...");

                        System.Windows.Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
                        {
                            StartButton.Content = "Start";
                            SettingButton.IsEnabled = true;
                            MonitoringIntervalTextBox.IsEnabled = true;

                            ButtonTimer.Enabled  = false;
                            ProblemTimer.Enabled = false;
                        }));
                    }

                    if (isShutdownPC)
                    {
                        ClickLog("Shutting Down PC...");

                        System.Diagnostics.Process.Start("shutdown.exe", "-s -f -t 30");
                    }

                    return;
                }


                // Gold Button
                color = CurrentBitmap.GetPixel(GoldButtonBackgroundX, GoldButtonBackgroundY);
                TimerLog("Gold Button Background Color: " + color.R + "," + color.G + "," + color.B);
                color1 = ColorTranslator.FromHtml(GoldButtonBackgroundColor.Split(';')[0]);
                color2 = ColorTranslator.FromHtml(GoldButtonBackgroundColor.Split(';')[1]);
                if (((color.R >= color1.R - pixelDifference) && (color.G >= color1.G - pixelDifference) && (color.B >= color1.B - pixelDifference)) &&
                    ((color.R <= color1.R + pixelDifference) && (color.G <= color1.G + pixelDifference) && (color.B <= color1.B + pixelDifference))) // Green
                {
                    color = CurrentBitmap.GetPixel(GoldButtonImageX, GoldButtonImageY);
                    TimerLog("Gold Button Image Color: " + color.R + "," + color.G + "," + color.B);
                    color1 = ColorTranslator.FromHtml(GoldButtonImageColor);
                    if (!IsLatest || (((color.R >= color1.R - pixelDifference) && (color.G >= color1.G - pixelDifference) && (color.B >= color1.B - pixelDifference)) &&
                                      ((color.R <= color1.R + pixelDifference) && (color.G <= color1.G + pixelDifference) && (color.B <= color1.B + pixelDifference)))) // Yellow
                    {
                        if (DelayCriteria == 0)
                        {
                            ClickLog("Gold Button");
                            AdsFlag = true;
                            AdsStopwatch.Restart();
                            DelayStopwatch.Reset();
                            DelayStopwatch.Stop();

                            System.Windows.Forms.Cursor.Position = new System.Drawing.Point((int)NoxPointX + GoldButtonBackgroundX, (int)NoxPointY + GoldButtonBackgroundY);
                            mouse_event(LBUTTONDOWN, 0, 0, 0, 0);
                            System.Threading.Thread.Sleep(50);
                            mouse_event(LBUTTONUP, 0, 0, 0, 0);
                        }
                        else
                        {
                            if (DelayStopwatch.IsRunning)
                            {
                                if (DelayStopwatch.ElapsedMilliseconds > DelayCriteria)
                                {
                                    ClickLog("Gold Button");
                                    AdsFlag = true;
                                    AdsStopwatch.Restart();
                                    DelayStopwatch.Reset();
                                    DelayStopwatch.Stop();

                                    System.Windows.Forms.Cursor.Position = new System.Drawing.Point((int)NoxPointX + GoldButtonBackgroundX, (int)NoxPointY + GoldButtonBackgroundY);
                                    mouse_event(LBUTTONDOWN, 0, 0, 0, 0);
                                    System.Threading.Thread.Sleep(50);
                                    mouse_event(LBUTTONUP, 0, 0, 0, 0);
                                }
                            }
                            else
                            {
                                DelayStopwatch.Restart();
                            }
                        }
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
                else if (((color.R >= color2.R - pixelDifference) && (color.G >= color2.G - pixelDifference) && (color.B >= color2.B - pixelDifference)) &&
                         ((color.R <= color2.R + pixelDifference) && (color.G <= color2.G + pixelDifference) && (color.B <= color2.B + pixelDifference))) // Gray
                {
                    if (DelayCriteria == 0)
                    {
                        ClickLog("Gold Button is Gray");
                        DelayStopwatch.Reset();
                        DelayStopwatch.Stop();

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
                    else
                    {
                        if (DelayStopwatch.IsRunning)
                        {
                            if (DelayStopwatch.ElapsedMilliseconds > DelayCriteria)
                            {
                                ClickLog("Gold Button is Gray");
                                DelayStopwatch.Reset();
                                DelayStopwatch.Stop();

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
                        }
                        else
                        {
                            DelayStopwatch.Restart();
                        }
                    }
                }


                // Next Button
                color = CurrentBitmap.GetPixel(NextButtonX, NextButtonY);
                TimerLog("Next Button Color: " + color.R + "," + color.G + "," + color.B);
                color1 = ColorTranslator.FromHtml(NextButtonColor);
                if (IsLatest && (((color.R >= color1.R - pixelDifference) && (color.G >= color1.G - pixelDifference) && (color.B >= color1.B - pixelDifference)) &&
                                 ((color.R <= color1.R + pixelDifference) && (color.G <= color1.G + pixelDifference) && (color.B <= color1.B + pixelDifference))))
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
                if (((color.R >= color1.R - pixelDifference) && (color.G >= color1.G - pixelDifference) && (color.B >= color1.B - pixelDifference)) &&
                    ((color.R <= color1.R + pixelDifference) && (color.G <= color1.G + pixelDifference) && (color.B <= color1.B + pixelDifference)))
                {
                    ClickLog("Not Responding Button");
                    System.Windows.Forms.Cursor.Position = new System.Drawing.Point((int)NoxPointX + NotRespondingX, (int)NoxPointY + NotRespondingY);
                    mouse_event(LBUTTONDOWN, 0, 0, 0, 0);
                    System.Threading.Thread.Sleep(50);
                    mouse_event(LBUTTONUP, 0, 0, 0, 0);
                }


                // Gold Ad Close Button
                color = CurrentBitmap.GetPixel(GameAdCloseButtonX, GoldAdCloseButtonY);
                TimerLog("Gold Ad Close Button Color: " + color.R + "," + color.G + "," + color.B);
                color1 = ColorTranslator.FromHtml(GameAdCloseButtonColor.Split(';')[0]);
                color2 = ColorTranslator.FromHtml(GameAdCloseButtonColor.Split(';')[1]);
                if ((((color.R >= color1.R - pixelDifference) && (color.G >= color1.G - pixelDifference) && (color.B >= color1.B - pixelDifference)) &&
                     ((color.R <= color1.R + pixelDifference) && (color.G <= color1.G + pixelDifference) && (color.B <= color1.B + pixelDifference))) ||
                    (((color.R >= color2.R - pixelDifference) && (color.G >= color2.G - pixelDifference) && (color.B >= color2.B - pixelDifference)) &&
                     ((color.R <= color2.R + pixelDifference) && (color.G <= color2.G + pixelDifference) && (color.B <= color2.B + pixelDifference))))
                {
                    ClickLog("Gold Ad Close Button");
                    System.Windows.Forms.Cursor.Position = new System.Drawing.Point((int)NoxPointX + GameAdCloseButtonX, (int)NoxPointY + GoldAdCloseButtonY);
                    mouse_event(LBUTTONDOWN, 0, 0, 0, 0);
                    System.Threading.Thread.Sleep(50);
                    mouse_event(LBUTTONUP, 0, 0, 0, 0);

                    return;
                }


                // Troop Ad Close Button
                color = CurrentBitmap.GetPixel(GameAdCloseButtonX, TroopAdCloseButtonY);
                TimerLog("Troop Ad Close Button: " + color.R + "," + color.G + "," + color.B);
                color1 = ColorTranslator.FromHtml(GameAdCloseButtonColor.Split(';')[0]);
                color2 = ColorTranslator.FromHtml(GameAdCloseButtonColor.Split(';')[1]);
                if ((((color.R >= color1.R - pixelDifference) && (color.G >= color1.G - pixelDifference) && (color.B >= color1.B - pixelDifference)) &&
                     ((color.R <= color1.R + pixelDifference) && (color.G <= color1.G + pixelDifference) && (color.B <= color1.B + pixelDifference))) ||
                    (((color.R >= color2.R - pixelDifference) && (color.G >= color2.G - pixelDifference) && (color.B >= color2.B - pixelDifference)) &&
                     ((color.R <= color2.R + pixelDifference) && (color.G <= color2.G + pixelDifference) && (color.B <= color2.B + pixelDifference))))
                {
                    ClickLog("Troop Ad Close Button");
                    System.Windows.Forms.Cursor.Position = new System.Drawing.Point((int)NoxPointX + GameAdCloseButtonX, (int)NoxPointY + TroopAdCloseButtonY);
                    mouse_event(LBUTTONDOWN, 0, 0, 0, 0);
                    System.Threading.Thread.Sleep(50);
                    mouse_event(LBUTTONUP, 0, 0, 0, 0);

                    return;
                }


                // Google Ad Close Button
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

                    if (!isDifferent || (AdsStopwatch.ElapsedMilliseconds > 34000))
                    {
                        ClickLog("Ads Close Button");
                        AdsStopwatch.Reset();
                        AdsStopwatch.Stop();

                        int AdsClickInterval = 333;
                        bool isGoogleAdCloseButtonColorCheck = false;
                        string[] ClickPatterns = ClickPattern.Split(';');

                        System.Windows.Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
                        {
                            AdsClickInterval = int.Parse(MonitoringIntervalTextBox.Text) / ClickPatterns.Length;
                            isGoogleAdCloseButtonColorCheck = AdsCloseCheckBox.IsChecked.Value;
                        }));

                        if (isGoogleAdCloseButtonColorCheck)
                        {
                            if (ClickPatterns[0] == "L")
                            {
                                color = CurrentBitmap.GetPixel(LeftAdCloseButtonX, GoogleAdCloseButtonY);
                                TimerLog("Left Ad Close Button Color: " + color.R + "," + color.G + "," + color.B);
                                color1 = ColorTranslator.FromHtml(GoogleAdCloseButtonColor.Split(';')[0]);
                                color2 = ColorTranslator.FromHtml(GoogleAdCloseButtonColor.Split(';')[1]);
                                if ((((color.R >= color1.R - pixelDifference) && (color.G >= color1.G - pixelDifference) && (color.B >= color1.B - pixelDifference)) &&
                                     ((color.R <= color1.R + pixelDifference) && (color.G <= color1.G + pixelDifference) && (color.B <= color1.B + pixelDifference))) ||
                                    (((color.R >= color2.R - pixelDifference) && (color.G >= color2.G - pixelDifference) && (color.B >= color2.B - pixelDifference)) &&
                                     ((color.R <= color2.R + pixelDifference) && (color.G <= color2.G + pixelDifference) && (color.B <= color2.B + pixelDifference))))
                                {
                                    ClickLog("Left Ad Close Button");
                                    System.Windows.Forms.Cursor.Position = new System.Drawing.Point((int)NoxPointX + LeftAdCloseButtonX, (int)NoxPointY + GoogleAdCloseButtonY);
                                    mouse_event(LBUTTONDOWN, 0, 0, 0, 0);
                                    System.Threading.Thread.Sleep(50);
                                    mouse_event(LBUTTONUP, 0, 0, 0, 0);
                                }
                            }
                            else
                            {
                                color = CurrentBitmap.GetPixel(RightAdCloseButtonX, GoogleAdCloseButtonY);
                                TimerLog("Right Ad Close Button Color: " + color.R + "," + color.G + "," + color.B);
                                color1 = ColorTranslator.FromHtml(GoogleAdCloseButtonColor.Split(';')[0]);
                                color2 = ColorTranslator.FromHtml(GoogleAdCloseButtonColor.Split(';')[1]);
                                if ((((color.R >= color1.R - pixelDifference) && (color.G >= color1.G - pixelDifference) && (color.B >= color1.B - pixelDifference)) &&
                                     ((color.R <= color1.R + pixelDifference) && (color.G <= color1.G + pixelDifference) && (color.B <= color1.B + pixelDifference))) ||
                                    (((color.R >= color2.R - pixelDifference) && (color.G >= color2.G - pixelDifference) && (color.B >= color2.B - pixelDifference)) &&
                                     ((color.R <= color2.R + pixelDifference) && (color.G <= color2.G + pixelDifference) && (color.B <= color2.B + pixelDifference))))
                                {
                                    ClickLog("Right Ad Close Button");
                                    System.Windows.Forms.Cursor.Position = new System.Drawing.Point((int)NoxPointX + RightAdCloseButtonX, (int)NoxPointY + GoogleAdCloseButtonY);
                                    mouse_event(LBUTTONDOWN, 0, 0, 0, 0);
                                    System.Threading.Thread.Sleep(50);
                                    mouse_event(LBUTTONUP, 0, 0, 0, 0);
                                }
                            }

                            System.Threading.Thread.Sleep(AdsClickInterval);

                            if (ClickPatterns[1] == "L")
                            {
                                color = CurrentBitmap.GetPixel(LeftAdCloseButtonX, GoogleAdCloseButtonY);
                                TimerLog("Left Ad Close Button Color: " + color.R + "," + color.G + "," + color.B);
                                color1 = ColorTranslator.FromHtml(GoogleAdCloseButtonColor.Split(';')[0]);
                                color2 = ColorTranslator.FromHtml(GoogleAdCloseButtonColor.Split(';')[1]);
                                if ((((color.R >= color1.R - pixelDifference) && (color.G >= color1.G - pixelDifference) && (color.B >= color1.B - pixelDifference)) &&
                                     ((color.R <= color1.R + pixelDifference) && (color.G <= color1.G + pixelDifference) && (color.B <= color1.B + pixelDifference))) ||
                                    (((color.R >= color2.R - pixelDifference) && (color.G >= color2.G - pixelDifference) && (color.B >= color2.B - pixelDifference)) &&
                                     ((color.R <= color2.R + pixelDifference) && (color.G <= color2.G + pixelDifference) && (color.B <= color2.B + pixelDifference))))
                                {
                                    ClickLog("Left Ad Close Button");
                                    System.Windows.Forms.Cursor.Position = new System.Drawing.Point((int)NoxPointX + LeftAdCloseButtonX, (int)NoxPointY + GoogleAdCloseButtonY);
                                    mouse_event(LBUTTONDOWN, 0, 0, 0, 0);
                                    System.Threading.Thread.Sleep(50);
                                    mouse_event(LBUTTONUP, 0, 0, 0, 0);
                                }
                            }
                            else
                            {
                                color = CurrentBitmap.GetPixel(RightAdCloseButtonX, GoogleAdCloseButtonY);
                                TimerLog("Right Ad Close Button Color: " + color.R + "," + color.G + "," + color.B);
                                color1 = ColorTranslator.FromHtml(GoogleAdCloseButtonColor.Split(';')[0]);
                                color2 = ColorTranslator.FromHtml(GoogleAdCloseButtonColor.Split(';')[1]);
                                if ((((color.R >= color1.R - pixelDifference) && (color.G >= color1.G - pixelDifference) && (color.B >= color1.B - pixelDifference)) &&
                                     ((color.R <= color1.R + pixelDifference) && (color.G <= color1.G + pixelDifference) && (color.B <= color1.B + pixelDifference))) ||
                                    (((color.R >= color2.R - pixelDifference) && (color.G >= color2.G - pixelDifference) && (color.B >= color2.B - pixelDifference)) &&
                                     ((color.R <= color2.R + pixelDifference) && (color.G <= color2.G + pixelDifference) && (color.B <= color2.B + pixelDifference))))
                                {
                                    ClickLog("Right Ad Close Button");
                                    System.Windows.Forms.Cursor.Position = new System.Drawing.Point((int)NoxPointX + RightAdCloseButtonX, (int)NoxPointY + GoogleAdCloseButtonY);
                                    mouse_event(LBUTTONDOWN, 0, 0, 0, 0);
                                    System.Threading.Thread.Sleep(50);
                                    mouse_event(LBUTTONUP, 0, 0, 0, 0);
                                }
                            }

                            if (ClickPatterns.Length == 3)
                            {
                                System.Threading.Thread.Sleep(AdsClickInterval);

                                if (ClickPatterns[2] == "L")
                                {
                                    color = CurrentBitmap.GetPixel(LeftAdCloseButtonX, GoogleAdCloseButtonY);
                                    TimerLog("Left Ad Close Button Color: " + color.R + "," + color.G + "," + color.B);
                                    color1 = ColorTranslator.FromHtml(GoogleAdCloseButtonColor.Split(';')[0]);
                                    color2 = ColorTranslator.FromHtml(GoogleAdCloseButtonColor.Split(';')[1]);
                                    if ((((color.R >= color1.R - pixelDifference) && (color.G >= color1.G - pixelDifference) && (color.B >= color1.B - pixelDifference)) &&
                                         ((color.R <= color1.R + pixelDifference) && (color.G <= color1.G + pixelDifference) && (color.B <= color1.B + pixelDifference))) ||
                                        (((color.R >= color2.R - pixelDifference) && (color.G >= color2.G - pixelDifference) && (color.B >= color2.B - pixelDifference)) &&
                                         ((color.R <= color2.R + pixelDifference) && (color.G <= color2.G + pixelDifference) && (color.B <= color2.B + pixelDifference))))
                                    {
                                        ClickLog("Left Ad Close Button");
                                        System.Windows.Forms.Cursor.Position = new System.Drawing.Point((int)NoxPointX + LeftAdCloseButtonX, (int)NoxPointY + GoogleAdCloseButtonY);
                                        mouse_event(LBUTTONDOWN, 0, 0, 0, 0);
                                        System.Threading.Thread.Sleep(50);
                                        mouse_event(LBUTTONUP, 0, 0, 0, 0);
                                    }
                                }
                                else
                                {
                                    color = CurrentBitmap.GetPixel(RightAdCloseButtonX, GoogleAdCloseButtonY);
                                    TimerLog("Right Ad Close Button Color: " + color.R + "," + color.G + "," + color.B);
                                    color1 = ColorTranslator.FromHtml(GoogleAdCloseButtonColor.Split(';')[0]);
                                    color2 = ColorTranslator.FromHtml(GoogleAdCloseButtonColor.Split(';')[1]);
                                    if ((((color.R >= color1.R - pixelDifference) && (color.G >= color1.G - pixelDifference) && (color.B >= color1.B - pixelDifference)) &&
                                         ((color.R <= color1.R + pixelDifference) && (color.G <= color1.G + pixelDifference) && (color.B <= color1.B + pixelDifference))) ||
                                        (((color.R >= color2.R - pixelDifference) && (color.G >= color2.G - pixelDifference) && (color.B >= color2.B - pixelDifference)) &&
                                         ((color.R <= color2.R + pixelDifference) && (color.G <= color2.G + pixelDifference) && (color.B <= color2.B + pixelDifference))))
                                    {
                                        ClickLog("Right Ad Close Button");
                                        System.Windows.Forms.Cursor.Position = new System.Drawing.Point((int)NoxPointX + RightAdCloseButtonX, (int)NoxPointY + GoogleAdCloseButtonY);
                                        mouse_event(LBUTTONDOWN, 0, 0, 0, 0);
                                        System.Threading.Thread.Sleep(50);
                                        mouse_event(LBUTTONUP, 0, 0, 0, 0);
                                    }
                                }
                            }

                            if (ClickPatterns.Length == 4)
                            {
                                System.Threading.Thread.Sleep(AdsClickInterval);

                                if (ClickPatterns[3] == "L")
                                {
                                    color = CurrentBitmap.GetPixel(LeftAdCloseButtonX, GoogleAdCloseButtonY);
                                    TimerLog("Left Ad Close Button Color: " + color.R + "," + color.G + "," + color.B);
                                    color1 = ColorTranslator.FromHtml(GoogleAdCloseButtonColor.Split(';')[0]);
                                    color2 = ColorTranslator.FromHtml(GoogleAdCloseButtonColor.Split(';')[1]);
                                    if ((((color.R >= color1.R - pixelDifference) && (color.G >= color1.G - pixelDifference) && (color.B >= color1.B - pixelDifference)) &&
                                         ((color.R <= color1.R + pixelDifference) && (color.G <= color1.G + pixelDifference) && (color.B <= color1.B + pixelDifference))) ||
                                        (((color.R >= color2.R - pixelDifference) && (color.G >= color2.G - pixelDifference) && (color.B >= color2.B - pixelDifference)) &&
                                         ((color.R <= color2.R + pixelDifference) && (color.G <= color2.G + pixelDifference) && (color.B <= color2.B + pixelDifference))))
                                    {
                                        ClickLog("Left Ad Close Button");
                                        System.Windows.Forms.Cursor.Position = new System.Drawing.Point((int)NoxPointX + LeftAdCloseButtonX, (int)NoxPointY + GoogleAdCloseButtonY);
                                        mouse_event(LBUTTONDOWN, 0, 0, 0, 0);
                                        System.Threading.Thread.Sleep(50);
                                        mouse_event(LBUTTONUP, 0, 0, 0, 0);
                                    }
                                }
                                else
                                {
                                    color = CurrentBitmap.GetPixel(RightAdCloseButtonX, GoogleAdCloseButtonY);
                                    TimerLog("Right Ad Close Button Color: " + color.R + "," + color.G + "," + color.B);
                                    color1 = ColorTranslator.FromHtml(GoogleAdCloseButtonColor.Split(';')[0]);
                                    color2 = ColorTranslator.FromHtml(GoogleAdCloseButtonColor.Split(';')[1]);
                                    if ((((color.R >= color1.R - pixelDifference) && (color.G >= color1.G - pixelDifference) && (color.B >= color1.B - pixelDifference)) &&
                                         ((color.R <= color1.R + pixelDifference) && (color.G <= color1.G + pixelDifference) && (color.B <= color1.B + pixelDifference))) ||
                                        (((color.R >= color2.R - pixelDifference) && (color.G >= color2.G - pixelDifference) && (color.B >= color2.B - pixelDifference)) &&
                                         ((color.R <= color2.R + pixelDifference) && (color.G <= color2.G + pixelDifference) && (color.B <= color2.B + pixelDifference))))
                                    {
                                        ClickLog("Right Ad Close Button");
                                        System.Windows.Forms.Cursor.Position = new System.Drawing.Point((int)NoxPointX + RightAdCloseButtonX, (int)NoxPointY + GoogleAdCloseButtonY);
                                        mouse_event(LBUTTONDOWN, 0, 0, 0, 0);
                                        System.Threading.Thread.Sleep(50);
                                        mouse_event(LBUTTONUP, 0, 0, 0, 0);
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (ClickPatterns[0] == "L")
                            {
                                ClickLog("Left Ad Close Button");
                                System.Windows.Forms.Cursor.Position = new System.Drawing.Point((int)NoxPointX + LeftAdCloseButtonX, (int)NoxPointY + GoogleAdCloseButtonY);
                                mouse_event(LBUTTONDOWN, 0, 0, 0, 0);
                                System.Threading.Thread.Sleep(50);
                                mouse_event(LBUTTONUP, 0, 0, 0, 0);
                            }
                            else
                            {
                                ClickLog("Right Ad Close Button");
                                System.Windows.Forms.Cursor.Position = new System.Drawing.Point((int)NoxPointX + RightAdCloseButtonX, (int)NoxPointY + GoogleAdCloseButtonY);
                                mouse_event(LBUTTONDOWN, 0, 0, 0, 0);
                                System.Threading.Thread.Sleep(50);
                                mouse_event(LBUTTONUP, 0, 0, 0, 0);
                            }

                            System.Threading.Thread.Sleep(AdsClickInterval);

                            if (ClickPatterns[1] == "L")
                            {
                                ClickLog("Left Ad Close Button");
                                System.Windows.Forms.Cursor.Position = new System.Drawing.Point((int)NoxPointX + LeftAdCloseButtonX, (int)NoxPointY + GoogleAdCloseButtonY);
                                mouse_event(LBUTTONDOWN, 0, 0, 0, 0);
                                System.Threading.Thread.Sleep(50);
                                mouse_event(LBUTTONUP, 0, 0, 0, 0);
                            }
                            else
                            {
                                ClickLog("Right Ad Close Button");
                                System.Windows.Forms.Cursor.Position = new System.Drawing.Point((int)NoxPointX + RightAdCloseButtonX, (int)NoxPointY + GoogleAdCloseButtonY);
                                mouse_event(LBUTTONDOWN, 0, 0, 0, 0);
                                System.Threading.Thread.Sleep(50);
                                mouse_event(LBUTTONUP, 0, 0, 0, 0);
                            }

                            if (ClickPatterns.Length == 3)
                            {
                                System.Threading.Thread.Sleep(AdsClickInterval);

                                if (ClickPatterns[2] == "L")
                                {
                                    ClickLog("Left Ad Close Button");
                                    System.Windows.Forms.Cursor.Position = new System.Drawing.Point((int)NoxPointX + LeftAdCloseButtonX, (int)NoxPointY + GoogleAdCloseButtonY);
                                    mouse_event(LBUTTONDOWN, 0, 0, 0, 0);
                                    System.Threading.Thread.Sleep(50);
                                    mouse_event(LBUTTONUP, 0, 0, 0, 0);
                                }
                                else
                                {
                                    ClickLog("Right Ad Close Button");
                                    System.Windows.Forms.Cursor.Position = new System.Drawing.Point((int)NoxPointX + RightAdCloseButtonX, (int)NoxPointY + GoogleAdCloseButtonY);
                                    mouse_event(LBUTTONDOWN, 0, 0, 0, 0);
                                    System.Threading.Thread.Sleep(50);
                                    mouse_event(LBUTTONUP, 0, 0, 0, 0);
                                }
                            }

                            if (ClickPatterns.Length == 4)
                            {
                                System.Threading.Thread.Sleep(AdsClickInterval);

                                if (ClickPatterns[3] == "L")
                                {
                                    ClickLog("Left Ad Close Button");
                                    System.Windows.Forms.Cursor.Position = new System.Drawing.Point((int)NoxPointX + LeftAdCloseButtonX, (int)NoxPointY + GoogleAdCloseButtonY);
                                    mouse_event(LBUTTONDOWN, 0, 0, 0, 0);
                                    System.Threading.Thread.Sleep(50);
                                    mouse_event(LBUTTONUP, 0, 0, 0, 0);
                                }
                                else
                                {
                                    ClickLog("Right Ad Close Button");
                                    System.Windows.Forms.Cursor.Position = new System.Drawing.Point((int)NoxPointX + RightAdCloseButtonX, (int)NoxPointY + GoogleAdCloseButtonY);
                                    mouse_event(LBUTTONDOWN, 0, 0, 0, 0);
                                    System.Threading.Thread.Sleep(50);
                                    mouse_event(LBUTTONUP, 0, 0, 0, 0);
                                }
                            }
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

            settingWindow.GoldAdCloseButtonX.Text = GameAdCloseButtonX.ToString();
            settingWindow.GoldAdCloseButtonY.Text = GoldAdCloseButtonY.ToString();
            settingWindow.GoldAdCloseButtonColor.Text = GameAdCloseButtonColor;
            settingWindow.GoldAdCloseButton1.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(GameAdCloseButtonColor.Split(';')[0]));

            settingWindow.TroopAdCloseButtonX.Text = GameAdCloseButtonX.ToString();
            settingWindow.TroopAdCloseButtonY.Text = TroopAdCloseButtonY.ToString();
            settingWindow.TroopAdCloseButtonColor.Text = GameAdCloseButtonColor;
            settingWindow.TroopAdCloseButton1.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(GameAdCloseButtonColor.Split(';')[1]));

            settingWindow.LeftAdCloseButtonX.Text = LeftAdCloseButtonX.ToString();
            settingWindow.LeftAdCloseButtonY.Text = GoogleAdCloseButtonY.ToString();
            settingWindow.LeftAdCloseButtonColor.Text = GoogleAdCloseButtonColor;
            settingWindow.LeftAdCloseButton1.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(GoogleAdCloseButtonColor.Split(';')[0]));

            settingWindow.RightAdCloseButtonX.Text = RightAdCloseButtonX.ToString();
            settingWindow.RightAdCloseButtonY.Text = GoogleAdCloseButtonY.ToString();
            settingWindow.RightAdCloseButtonColor.Text = GoogleAdCloseButtonColor;
            settingWindow.RightAdCloseButton1.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(GoogleAdCloseButtonColor.Split(';')[1]));

            settingWindow.NotRespondingX.Text = NotRespondingX.ToString();
            settingWindow.NotRespondingY.Text = NotRespondingY.ToString();
            settingWindow.NotRespondingColor.Text = NotRespondingColor.ToString();
            settingWindow.NotResponding1.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(NotRespondingColor));

            settingWindow.NoGoldX.Text = NoGoldX.ToString();
            settingWindow.NoGoldY.Text = NoGoldY.ToString();
            settingWindow.NoGoldColor.Text = NoGoldColor.ToString();
            settingWindow.NoGoldButton1.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(NoGoldColor));

            settingWindow.ShowDialog();
        }
    }
}
