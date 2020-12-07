using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
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

            FileInfo fileInfo = new FileInfo("pixel.txt");
            if (fileInfo.Exists)
            {
                ClickLog("Load Pixel...");

                string[] lines = File.ReadAllLines("pixel.txt");

                foreach (string line in lines)
                {
                    string[] listitem = line.Split(',');

                    switch (listitem[0].ToLower())
                    {
                        case ("applocation"):
                            AppLocationX = int.Parse(listitem[1]);
                            AppLocationY = int.Parse(listitem[2]);
                            AppLocationColor = listitem[3];
                            break;
                        case ("shopbutton"):
                            HomeButtonX = int.Parse(listitem[1]);
                            ShopButtonX = int.Parse(listitem[2]);
                            ShopButtonY = int.Parse(listitem[3]);
                            ShopButtonColor = listitem[4];
                            break;
                        case ("middlebutton"):
                            CollectButtonX = int.Parse(listitem[1]);
                            CollectButtonY = int.Parse(listitem[2]);
                            CollectButtonColor = listitem[3];
                            break;
                        case ("goldchestbox"):
                            GoldChestBoxX = int.Parse(listitem[1]);
                            GoldChestBoxY = int.Parse(listitem[2]);
                            GoldChestBoxColor = listitem[3];
                            break;
                        case ("battlelevelbutton"):
                            BattleLevelButtonX = int.Parse(listitem[1]);
                            BattleLevelButtonY = int.Parse(listitem[2]);
                            BattleLevelButtonColor = listitem[3];
                            break;
                        case ("skillbutton"):
                            SkillButtonX = int.Parse(listitem[1]);
                            SkillButtonY = int.Parse(listitem[2]);
                            SkillButtonColor = listitem[3];
                            break;
                        case ("speedbutton"):
                            SpeedButtonX = int.Parse(listitem[1]);
                            SpeedButtonY = int.Parse(listitem[2]);
                            SpeedButtonColor = listitem[3];
                            break;
                        case ("pausebutton"):
                            ContinueButtonX = int.Parse(listitem[1]);
                            ContinueButtonY = int.Parse(listitem[2]);
                            ContinueButtonColor = listitem[3];
                            break;
                        case ("victorydefeat"):
                            VictoryDefeatX = int.Parse(listitem[1]);
                            VictoryDefeatY = int.Parse(listitem[2]);
                            VictoryDefeatColor = listitem[3];
                            break;
                        case ("goldbuttonbackground"):
                            GoldButtonBackgroundX = int.Parse(listitem[1]);
                            GoldButtonBackgroundY = int.Parse(listitem[2]);
                            GoldButtonBackgroundColor = listitem[3];
                            break;
                        case ("goldbuttonimage"):
                            GoldButtonImageX = int.Parse(listitem[1]);
                            GoldButtonImageY = int.Parse(listitem[2]);
                            GoldButtonImageColor = listitem[3];
                            break;
                        case ("nextbutton"):
                            NextButtonX = int.Parse(listitem[1]);
                            NextButtonY = int.Parse(listitem[2]);
                            NextButtonColor = listitem[3];
                            break;
                        case ("nogold"):
                            NoGoldX = int.Parse(listitem[1]);
                            NoGoldY = int.Parse(listitem[2]);
                            NoGoldColor = listitem[3];
                            break;
                        case ("gameadclosebutton"):
                            GameAdCloseButtonX = int.Parse(listitem[1]);
                            GoldAdCloseButtonY = int.Parse(listitem[2]);
                            TroopAdCloseButtonY = int.Parse(listitem[3]);
                            // 예외발생 임시처리 나중에 삭제할것
                            if (listitem.Length < 6)
                            {
                                GameAdCloseButtonColor = listitem[4] + ";#e9e9d8".ToUpper();
                            }
                            else
                            {
                                MidasAdCloseButtonY = int.Parse(listitem[4]);
                                GameAdCloseButtonColor = listitem[5];
                            }
                            break;
                        case ("googleadclosebutton"):
                            LeftAdCloseButtonX = int.Parse(listitem[1]);
                            RightAdCloseButtonX = int.Parse(listitem[2]);
                            GoogleAdCloseButtonY = int.Parse(listitem[3]);
                            GoogleAdCloseButtonColor = listitem[4];
                            break;
                        case ("notresponding"):
                            NotRespondingX = int.Parse(listitem[1]);
                            NotRespondingY = int.Parse(listitem[2]);
                            NotRespondingColor = listitem[3];
                            break;
                    }
                }
            }

            fileInfo = new FileInfo("setting.txt");
            if (fileInfo.Exists)
            {
                ClickLog("Load Setting...");

                string[] lines = File.ReadAllLines("setting.txt");

                int tempnum;
                if (int.TryParse(lines[0].Split(',')[0], out tempnum)) // 구버전 - 추후 제거
                {
                    AppLocationX = int.Parse(lines[0].Split(',')[0]);
                    AppLocationY = int.Parse(lines[0].Split(',')[1]);
                    AppLocationColor = lines[0].Split(',')[2];

                    HomeButtonX = int.Parse(lines[1].Split(',')[0]);
                    ShopButtonX = int.Parse(lines[1].Split(',')[1]);
                    ShopButtonY = int.Parse(lines[1].Split(',')[2]);
                    ShopButtonColor = lines[1].Split(',')[3];

                    CollectButtonX = int.Parse(lines[2].Split(',')[0]);
                    CollectButtonY = int.Parse(lines[2].Split(',')[1]);
                    CollectButtonColor = lines[2].Split(',')[2];

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

                    ContinueButtonX = int.Parse(lines[7].Split(',')[0]);
                    ContinueButtonY = int.Parse(lines[7].Split(',')[1]);
                    ContinueButtonColor = lines[7].Split(',')[2];

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
						// 예외발생 임시처리 나중에 삭제할것
                        if (lines[13].Split(',').Length < 5)
                        {
                            GameAdCloseButtonColor = lines[13].Split(',')[3] + ";#e9e9d8".ToUpper();
                        }
                        else
                        {
                            MidasAdCloseButtonY = int.Parse(lines[13].Split(',')[3]);
                            GameAdCloseButtonColor = lines[13].Split(',')[4];
                        }

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
						// 예외발생 임시처리 나중에 삭제할것
                        if (lines[12].Split(',').Length < 5)
                        {
                            GameAdCloseButtonColor = lines[12].Split(',')[3] + ";#e9e9d8".ToUpper();
                        }
                        else
                        {
                            MidasAdCloseButtonY = int.Parse(lines[12].Split(',')[3]);
                            GameAdCloseButtonColor = lines[12].Split(',')[4];
                        }

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

                    using (StreamWriter streamWriter = new StreamWriter("pixel.txt", false))
                    {
                        streamWriter.WriteLine("AppLocation," + AppLocationX + "," + AppLocationY + "," + AppLocationColor);
                        streamWriter.WriteLine("ShopButton," + HomeButtonX + "," + ShopButtonX + "," + ShopButtonY + "," + ShopButtonColor);
                        streamWriter.WriteLine("CollectButton," + CollectButtonX + "," + CollectButtonY + "," + CollectButtonColor);
                        streamWriter.WriteLine("GoldChestBox," + GoldChestBoxX + "," + GoldChestBoxY + "," + GoldChestBoxColor);
                        streamWriter.WriteLine("BattleLevelButton," + BattleLevelButtonX + "," + BattleLevelButtonY + "," + BattleLevelButtonColor);
                        streamWriter.WriteLine("SkillButton," + SkillButtonX + "," + SkillButtonY + "," + SkillButtonColor);
                        streamWriter.WriteLine("SpeedButton," + SpeedButtonX + "," + SpeedButtonY + "," + SpeedButtonColor);
                        streamWriter.WriteLine("ContinueButton," + ContinueButtonX + "," + ContinueButtonY + "," + ContinueButtonColor);
                        streamWriter.WriteLine("VictoryDefeat," + VictoryDefeatX + "," + VictoryDefeatY + "," + VictoryDefeatColor);
                        streamWriter.WriteLine("GoldButtonBackground," + GoldButtonBackgroundX + "," + GoldButtonBackgroundY + "," + GoldButtonBackgroundColor);
                        streamWriter.WriteLine("GoldButtonImage," + GoldButtonImageX + "," + GoldButtonImageY + "," + GoldButtonImageColor);
                        streamWriter.WriteLine("NextButton," + NextButtonX + "," + NextButtonY + "," + NextButtonColor);
                        streamWriter.WriteLine("NoGold," + NoGoldX + "," + NoGoldY + "," + NoGoldColor);
                        streamWriter.WriteLine("GameAdCloseButton," + GameAdCloseButtonX + "," + GoldAdCloseButtonY + "," + TroopAdCloseButtonY + "," + MidasAdCloseButtonY + "," + GameAdCloseButtonColor);
                        streamWriter.WriteLine("GoogleAdCloseButton," + LeftAdCloseButtonX + "," + RightAdCloseButtonX + "," + GoogleAdCloseButtonY + "," + GoogleAdCloseButtonColor);
                        streamWriter.WriteLine("NotResponding," + NotRespondingX + "," + NotRespondingY + "," + NotRespondingColor);
                    }
                }
                else // 신버전
                {
                    foreach (string line in lines)
                    {
                        string[] listitem = line.Split(',');

                        switch (listitem[0].ToLower())
                        {
                            case ("windowtitle"):
                                WindowTitleTextBox.Text = listitem[1];
                                break;
                            case ("monitoringinterval"):
                                MonitoringIntervalTextBox.Text = listitem[1];
                                break;
                            case ("screencomparisoninterval"):
                                ScreenComparisonIntervalTextBox.Text = listitem[1];
                                break;
                            case ("x3goldbuttondelay"):
                                DelayTextBox.Text = listitem[1];
                                break;
                            case ("pixeldifference"):
                                PixelDifferenceTextBox.Text = listitem[1];
                                break;
                            case ("korean"):
                                KoreanCheckBox.IsChecked = bool.Parse(listitem[1]);
                                break;
                            case ("adscloseclickpattern"):
                                ClickPattern = listitem[1];
                                break;
                            case ("goldchestcheck"):
                                GoldChestCheckBox.IsChecked = bool.Parse(listitem[1]);
                                break;
                            case ("pausability"):
                                PausabilityCheckBox.IsChecked = bool.Parse(listitem[1]);
                                break;
                            case ("logging"):
                                LogCheckBox.IsChecked = bool.Parse(listitem[1]);
                                break;
                            case ("email"):
                                EmailTextBox.Text = listitem[1];
                                break;
                            case ("sendemail"):
                                SendEmailCheckBox.IsChecked = bool.Parse(listitem[1]);
                                break;
                            case ("stophassan"):
                                StopWorkingCheckBox.IsChecked = bool.Parse(listitem[1]);
                                break;
                            case ("shutdownpc"):
                                ShutdownPCCheckBox.IsChecked = bool.Parse(listitem[1]);
                                break;
                        }
                    }
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

            if ((StartButton.Content.ToString() == "Start") ||
                (StartButton.Content.ToString() == "시작"))
            {
                if (KoreanCheckBox.IsChecked.Value)
                {
                    StartButton.Content = "중지";
                }
                else
                {
                    StartButton.Content = "Stop";
                }

                PixelCustomizeButton.IsEnabled = false;
                MonitoringIntervalTextBox.IsEnabled = false;

                ButtonTimer.Interval = int.Parse(MonitoringIntervalTextBox.Text);
                ButtonTimer.Enabled = true;
                ProblemTimer.Enabled = true;

                IsNoGoldMailSent = false;
                ProblemMailSent = 0;
            }
            else
            {
                if (KoreanCheckBox.IsChecked.Value)
                {
                    StartButton.Content = "시작";
                }
                else
                {
                    StartButton.Content = "Start";
                }
                PixelCustomizeButton.IsEnabled = true;
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
                    smtpClient.Credentials = new NetworkCredential("artofwarhassan@gmail.com", "Rnrmf0575!");
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

                if (ProblemMailSent < 3)
                {
                    try
                    {
                        CurrentBitmap.Save("Problem.png", System.Drawing.Imaging.ImageFormat.Png);

                        bool isShareEmail = true; // 무늬만 사용
                        string emailaddress = "";
                        System.Windows.Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
                        {
                            isShareEmail = ShareProblemCheckBox.IsChecked.Value;
                            emailaddress = EmailTextBox.Text;
                        }));

                        SmtpClient smtpClient = new SmtpClient("smtp.gmail.com", 587);
                        smtpClient.UseDefaultCredentials = false;
                        smtpClient.EnableSsl = true;
                        smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
                        smtpClient.Credentials = new NetworkCredential("artofwarhassan@gmail.com", "Rnrmf0575!");

                        if (string.IsNullOrWhiteSpace(emailaddress))
                        {
                            MailMessage mailMessage = new MailMessage("artofwarhassan@gmail.com",
                                                          emailaddress,
                                                          "Art of Hassan",
                                                          "Problem occured.\nPlease check.");
                            mailMessage.Attachments.Add(new System.Net.Mail.Attachment("Problem.png"));
                            smtpClient.Send(mailMessage);

                            mailMessage = new MailMessage("artofwarhassan@gmail.com",
                                                          "artofwarhassan@gmail.com",
                                                          "Art of Hassan",
                                                          $"From {emailaddress},\nProblem occured.\nPlease check.");
                            mailMessage.Attachments.Add(new System.Net.Mail.Attachment("Problem.png"));
                            smtpClient.Send(mailMessage);
                        }
                        else
                        {
                            MailMessage mailMessage = new MailMessage("artofwarhassan@gmail.com",
                                                                      "artofwarhassan@gmail.com",
                                                                      "Art of Hassan",
                                                                      "Problem occured.\nPlease check.");
                            mailMessage.Attachments.Add(new System.Net.Mail.Attachment("Problem.png"));
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

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            using (StreamWriter streamWriter = new StreamWriter($@"setting.txt", false))
            {
                System.Windows.Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
                {
                    streamWriter.WriteLine("WindowTitle," + WindowTitleTextBox.Text);
                    streamWriter.WriteLine("MonitoringInterval," + MonitoringIntervalTextBox.Text);
                    streamWriter.WriteLine("ScreenComparisonInterval," + ScreenComparisonIntervalTextBox.Text);
                    streamWriter.WriteLine("X3GoldButtonDelay," + DelayTextBox.Text);
                    streamWriter.WriteLine("PixelDifference," + PixelDifferenceTextBox.Text);
                    streamWriter.WriteLine("korean," + KoreanCheckBox.IsChecked.Value);
                    streamWriter.WriteLine("AdsCloseClickPattern," + ClickPattern);
                    streamWriter.WriteLine("GoldChestCheck," + GoldChestCheckBox.IsChecked.Value);
                    streamWriter.WriteLine("Pausability," + PausabilityCheckBox.IsChecked.Value);
                    streamWriter.WriteLine("Logging," + LogCheckBox.IsChecked.Value);
                    streamWriter.WriteLine("Email," + EmailTextBox.Text);
                    streamWriter.WriteLine("SendEmail," + SendEmailCheckBox.IsChecked.Value);
                    streamWriter.WriteLine("StopHassan," + StopWorkingCheckBox.IsChecked.Value);
                    streamWriter.WriteLine("ShutdownPC," + ShutdownPCCheckBox.IsChecked.Value);
                }));
            }
        }

        private void KoreanCheckBox_Click(object sender, RoutedEventArgs e)
        {
            if (KoreanCheckBox.IsChecked.Value)
            {
                this.Title = "아트 오브 핫산";
                WindowTitleTextBlock.Text = "앱플레이어\n    이름";
                MonitoringIntervalTextBlock.Text = "모니터링\n주기 (ms)";
                ScreenComparisonIntervalTextBlock.Text = " 화면 비교\n주기 (횟수)";
                DelayTextBlock.Text = " 골드 광고\n딜레이 (ms)";
                PixelDifferenceTextBlock.Text = "픽셀 차이";
                VersionTextBlock.Text = "버전";
                LatestRadioButton.Content = "최신";
                AdsCloseTextBlock.Text = "광고 닫기";
                AdsCloseCheckBox.Content = "색상 확인";
                AdsClosePatternButton.Content = "광고 닫기\n클릭 패턴";
                OptionTextBlock.Text = "옵션";
                GoldChestCheckBox.Content = "골드 상자";
                PausabilityCheckBox.Content = "멈춤 가능";
                LogCheckBox.Content = "로그";
                EmailAlarmTextBlock.Text = "이메일 알림";
                EmailTestButton.Content = "테스트";
                NoGoldTextBlock.Text = "골드 벌이\n  없을때";
                SendEmailCheckBox.Content = "이메일";
                StopWorkingCheckBox.Content = "핫산 중지";
                ShutdownPCCheckBox.Content = "PC 종료";
                PixelCustomizeButton.Content = "픽셀 커스텀";
                SaveButton.Content = "설정 저장";
                StartButton.Content = "시작";
                MessageBar.Text = $"전투: {NumOfWar}  |  승리: {NumOfVictory}  |  패배: {NumOfDefeat}  |  광고: {NumOfAds}";
                ShareProblemCheckBox.Content = "우리 핫산 개선을 위해 문제 발생 스샷 공유 :)";
            }
            else
            {
                this.Title = "Art of Hassan";
                WindowTitleTextBlock.Text = "Window Title";
                MonitoringIntervalTextBlock.Text = "Monitoring\nInterval (ms)";
                ScreenComparisonIntervalTextBlock.Text = "   Screen\nComparison\n Interval (#)";
                DelayTextBlock.Text = " X3 Gold\n  Button\nDelay (ms)";
                PixelDifferenceTextBlock.Text = "   Pixel\nDifference";
                VersionTextBlock.Text = "Version";
                LatestRadioButton.Content = "Latest";
                AdsCloseTextBlock.Text = "Ads Close";
                AdsCloseCheckBox.Content = "Check Color";
                AdsClosePatternButton.Content = "  Ads Close\nClick Pattern";
                OptionTextBlock.Text = "Option";
                GoldChestCheckBox.Content = "Gold Chest";
                PausabilityCheckBox.Content = "Pausable";
                LogCheckBox.Content = "Logging";
                EmailAlarmTextBlock.Text = "Email Alarm";
                EmailTestButton.Content = "Test";
                NoGoldTextBlock.Text = "No Gold";
                SendEmailCheckBox.Content = "Send\nEmail";
                StopWorkingCheckBox.Content = " Stop\nHassan";
                ShutdownPCCheckBox.Content = "Shutdown\nComputer";
                PixelCustomizeButton.Content = "Customize Pixel";
                SaveButton.Content = "Save Setting";
                StartButton.Content = "Start";
                MessageBar.Text = $"War: {NumOfWar}  |  Victory: {NumOfVictory}  |  Defeat: {NumOfDefeat}  |  Ads: {NumOfAds}";
                ShareProblemCheckBox.Content = "Share screenshot of problem to improve our Hassan :)";
            }
        }


        public int AppLocationX        = 60;
        public int AppLocationY        = 500;
        public string AppLocationColor = "#a9d8ff;#97c601".ToUpper();

        public int HomeButtonX        = 290;
        public int ShopButtonX        = 65;
        public int ShopButtonY        = 980;
        public string ShopButtonColor = "#ea3d34".ToUpper();

        public int CollectButtonX        = 195;
        public int CollectButtonY        = 680;
        public string CollectButtonColor = "#fdbb00".ToUpper();

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

        public int ContinueButtonX     = 215;
        public int ContinueButtonY     = 455;
        public string ContinueButtonColor = "#fdbb00".ToUpper();

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
        public int MidasAdCloseButtonY = 262;
        public string GameAdCloseButtonColor = "#efe7d6;#e9e9d8;#e9e9d8".ToUpper();

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
                    if (KoreanCheckBox.IsChecked.Value)
                    {
                        StartButton.Content = "시작";
                    }
                    else
                    {
                        StartButton.Content = "Start";
                    }
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
                color = CurrentBitmap.GetPixel(CollectButtonX, CollectButtonY);
                TimerLog("Middle Button Color: " + color.R + "," + color.G + "," + color.B);
                color1 = ColorTranslator.FromHtml(CollectButtonColor);
                if (((color.R >= color1.R - pixelDifference) && (color.G >= color1.G - pixelDifference) && (color.B >= color1.B - pixelDifference)) &&
                    ((color.R <= color1.R + pixelDifference) && (color.G <= color1.G + pixelDifference) && (color.B <= color1.B + pixelDifference)))
                {
                    ClickLog("Middle Button");
                    System.Windows.Forms.Cursor.Position = new System.Drawing.Point((int)NoxPointX + CollectButtonX, (int)NoxPointY + CollectButtonY);
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
                        if (KoreanCheckBox.IsChecked.Value)
                        {
                            MessageBar.Text = $"전투: {NumOfVictory + NumOfDefeat}  |  승리: {NumOfVictory}  |  패배: {NumOfDefeat}  |  광고: {NumOfAds}";
                        }
                        else
                        {
                            MessageBar.Text = $"War: {NumOfVictory + NumOfDefeat}  |  Victory: {NumOfVictory}  |  Defeat: {NumOfDefeat}  |  Ads: {NumOfAds}";
                        }
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
                    color = CurrentBitmap.GetPixel(ContinueButtonX, ContinueButtonY);
                    TimerLog("Continue Button Color: " + color.R + "," + color.G + "," + color.B);
                    color1 = ColorTranslator.FromHtml(ContinueButtonColor);
                    if (((color.R >= color1.R - pixelDifference) && (color.G >= color1.G - pixelDifference) && (color.B >= color1.B - pixelDifference)) &&
                        ((color.R <= color1.R + pixelDifference) && (color.G <= color1.G + pixelDifference) && (color.B <= color1.B + pixelDifference)))
                    {
                        ClickLog("Continue Button");
                        System.Windows.Forms.Cursor.Position = new System.Drawing.Point((int)NoxPointX + ContinueButtonX, (int)NoxPointY + ContinueButtonY);
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
                                smtpClient.Credentials = new NetworkCredential("artofwarhassan@gmail.com", "Rnrmf0575!");
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
                            if (KoreanCheckBox.IsChecked.Value)
                            {
                                StartButton.Content = "시작";
                            }
                            else
                            {
                                StartButton.Content = "Start";
                            }
                            PixelCustomizeButton.IsEnabled = true;
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


                // Midas Ad Close Button
                color = CurrentBitmap.GetPixel(GameAdCloseButtonX, MidasAdCloseButtonY);
                TimerLog("Midas Ad Close Button: " + color.R + "," + color.G + "," + color.B);
                color1 = ColorTranslator.FromHtml(GameAdCloseButtonColor.Split(';')[0]);
                color2 = ColorTranslator.FromHtml(GameAdCloseButtonColor.Split(';')[1]);
                if ((((color.R >= color1.R - pixelDifference) && (color.G >= color1.G - pixelDifference) && (color.B >= color1.B - pixelDifference)) &&
                     ((color.R <= color1.R + pixelDifference) && (color.G <= color1.G + pixelDifference) && (color.B <= color1.B + pixelDifference))) ||
                    (((color.R >= color2.R - pixelDifference) && (color.G >= color2.G - pixelDifference) && (color.B >= color2.B - pixelDifference)) &&
                     ((color.R <= color2.R + pixelDifference) && (color.G <= color2.G + pixelDifference) && (color.B <= color2.B + pixelDifference))))
                {
                    ClickLog("Midas Ad Close Button");
                    System.Windows.Forms.Cursor.Position = new System.Drawing.Point((int)NoxPointX + GameAdCloseButtonX, (int)NoxPointY + MidasAdCloseButtonY);
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

        private void PixelCustomizeButton_Click(object sender, RoutedEventArgs e)
        {
            PixelWindow pixelWindow = new PixelWindow();
            pixelWindow.AppLocationX.Text = AppLocationX.ToString();
            pixelWindow.AppLocationY.Text = AppLocationY.ToString();
            pixelWindow.AppLocationColor.Text = AppLocationColor;
            pixelWindow.AppLocationButton1.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(AppLocationColor.Split(';')[0]));
            pixelWindow.AppLocationButton2.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(AppLocationColor.Split(';')[1]));

            pixelWindow.HomeButtonX.Text = HomeButtonX.ToString();
            pixelWindow.HomeButtonY.Text = ShopButtonY.ToString();

            pixelWindow.ShopButtonX.Text = ShopButtonX.ToString();
            pixelWindow.ShopButtonY.Text = ShopButtonY.ToString();
            pixelWindow.ShopButtonColor.Text = ShopButtonColor;
            pixelWindow.ShopButton1.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(ShopButtonColor));

            pixelWindow.CollectButtonX.Text = CollectButtonX.ToString();
            pixelWindow.CollectButtonY.Text = CollectButtonY.ToString();
            pixelWindow.CollectButtonColor.Text = CollectButtonColor;
            pixelWindow.CollectButton1.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(CollectButtonColor));

            pixelWindow.GoldChestBoxX.Text = GoldChestBoxX.ToString();
            pixelWindow.GoldChestBoxY.Text = GoldChestBoxY.ToString();
            pixelWindow.GoldChestBoxColor.Text = GoldChestBoxColor;
            pixelWindow.GoldChestBox1.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(GoldChestBoxColor.Split(';')[0]));
            pixelWindow.GoldChestBox2.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(GoldChestBoxColor.Split(';')[1]));

            pixelWindow.BattleLevelButtonX.Text = BattleLevelButtonX.ToString();
            pixelWindow.BattleLevelButtonY.Text = BattleLevelButtonY.ToString();
            pixelWindow.BattleLevelButtonColor.Text = BattleLevelButtonColor;
            pixelWindow.BattleLevelButton1.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(BattleLevelButtonColor.Split(';')[0]));
            pixelWindow.BattleLevelButton2.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(BattleLevelButtonColor.Split(';')[1]));
            pixelWindow.BattleLevelButton3.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(BattleLevelButtonColor.Split(';')[2]));
            pixelWindow.BattleLevelButton4.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(BattleLevelButtonColor.Split(';')[3]));

            pixelWindow.SkillButtonX.Text = SkillButtonX.ToString();
            pixelWindow.SkillButtonY.Text = SkillButtonY.ToString();
            pixelWindow.SkillButtonColor.Text = SkillButtonColor;
            pixelWindow.SkillButton1.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(SkillButtonColor));

            pixelWindow.SpeedButtonX.Text = SpeedButtonX.ToString();
            pixelWindow.SpeedButtonY.Text = SpeedButtonY.ToString();
            pixelWindow.SpeedButtonColor.Text = SpeedButtonColor;
            pixelWindow.SpeedButton1.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(SpeedButtonColor));

            pixelWindow.ContinueButtonX.Text = ContinueButtonX.ToString();
            pixelWindow.ContinueButtonY.Text = ContinueButtonY.ToString();
            pixelWindow.ContinueButtonColor.Text = ContinueButtonColor;
            pixelWindow.ContinueButton1.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(ContinueButtonColor));

            pixelWindow.VictoryDefeatX.Text = VictoryDefeatX.ToString();
            pixelWindow.VictoryDefeatY.Text = VictoryDefeatY.ToString();
            pixelWindow.VictoryDefeatColor.Text = VictoryDefeatColor;
            pixelWindow.VictoryDefeat1.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(VictoryDefeatColor.Split(';')[0]));
            pixelWindow.VictoryDefeat2.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(VictoryDefeatColor.Split(';')[1]));

            pixelWindow.GoldButtonBackgroundX.Text = GoldButtonBackgroundX.ToString();
            pixelWindow.GoldButtonBackgroundY.Text = GoldButtonBackgroundY.ToString();
            pixelWindow.GoldButtonBackgroundColor.Text = GoldButtonBackgroundColor;
            pixelWindow.GoldButtonBackground1.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(GoldButtonBackgroundColor.Split(';')[0]));
            pixelWindow.GoldButtonBackground2.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(GoldButtonBackgroundColor.Split(';')[1]));

            pixelWindow.GoldButtonImageX.Text = GoldButtonImageX.ToString();
            pixelWindow.GoldButtonImageY.Text = GoldButtonImageY.ToString();
            pixelWindow.GoldButtonImageColor.Text = GoldButtonImageColor;
            pixelWindow.GoldButtonImage1.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(GoldButtonImageColor));

            pixelWindow.NextButtonX.Text = NextButtonX.ToString();
            pixelWindow.NextButtonY.Text = NextButtonY.ToString();
            pixelWindow.NextButtonColor.Text = NextButtonColor;
            pixelWindow.NextButton1.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(NextButtonColor));

            pixelWindow.GoldAdCloseButtonX.Text = GameAdCloseButtonX.ToString();
            pixelWindow.GoldAdCloseButtonY.Text = GoldAdCloseButtonY.ToString();
            pixelWindow.GoldAdCloseButtonColor.Text = GameAdCloseButtonColor;
            pixelWindow.GoldAdCloseButton1.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(GameAdCloseButtonColor.Split(';')[0]));

            pixelWindow.TroopAdCloseButtonX.Text = GameAdCloseButtonX.ToString();
            pixelWindow.TroopAdCloseButtonY.Text = TroopAdCloseButtonY.ToString();
            pixelWindow.TroopAdCloseButtonColor.Text = GameAdCloseButtonColor;
            pixelWindow.TroopAdCloseButton1.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(GameAdCloseButtonColor.Split(';')[1]));

            pixelWindow.MidasAdCloseButtonX.Text = GameAdCloseButtonX.ToString();
            pixelWindow.MidasAdCloseButtonY.Text = MidasAdCloseButtonY.ToString();
            pixelWindow.MidasAdCloseButtonColor.Text = GameAdCloseButtonColor;
            pixelWindow.MidasAdCloseButton1.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(GameAdCloseButtonColor.Split(';')[2]));

            pixelWindow.LeftAdCloseButtonX.Text = LeftAdCloseButtonX.ToString();
            pixelWindow.LeftAdCloseButtonY.Text = GoogleAdCloseButtonY.ToString();
            pixelWindow.LeftAdCloseButtonColor.Text = GoogleAdCloseButtonColor;
            pixelWindow.LeftAdCloseButton1.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(GoogleAdCloseButtonColor.Split(';')[0]));

            pixelWindow.RightAdCloseButtonX.Text = RightAdCloseButtonX.ToString();
            pixelWindow.RightAdCloseButtonY.Text = GoogleAdCloseButtonY.ToString();
            pixelWindow.RightAdCloseButtonColor.Text = GoogleAdCloseButtonColor;
            pixelWindow.RightAdCloseButton1.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(GoogleAdCloseButtonColor.Split(';')[1]));

            pixelWindow.NotRespondingX.Text = NotRespondingX.ToString();
            pixelWindow.NotRespondingY.Text = NotRespondingY.ToString();
            //pixelWindow.NotRespondingColor.Text = NotRespondingColor.ToString();
            //pixelWindow.NotResponding1.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(NotRespondingColor));

            pixelWindow.NoGoldX.Text = NoGoldX.ToString();
            pixelWindow.NoGoldY.Text = NoGoldY.ToString();
            pixelWindow.NoGoldColor.Text = NoGoldColor.ToString();
            pixelWindow.NoGoldButton1.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(NoGoldColor));

            pixelWindow.ShowDialog();
        }

        private void WindowTitleTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            int caretIndex = WindowTitleTextBox.CaretIndex;
            WindowTitleTextBox.Text = Regex.Replace((e.Source as TextBox).Text, RegExClass.Location, "");
            WindowTitleTextBox.Select(caretIndex, 0);
        }

        private void MonitoringIntervalTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            int caretIndex = MonitoringIntervalTextBox.CaretIndex;
            MonitoringIntervalTextBox.Text = Regex.Replace((e.Source as TextBox).Text, RegExClass.Number, "");
            MonitoringIntervalTextBox.Select(caretIndex, 0);
        }

        private void ScreenComparisonIntervalTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            int caretIndex = ScreenComparisonIntervalTextBox.CaretIndex;
            ScreenComparisonIntervalTextBox.Text = Regex.Replace((e.Source as TextBox).Text, RegExClass.Number, "");
            ScreenComparisonIntervalTextBox.Select(caretIndex, 0);
        }

        private void DelayTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            int caretIndex = DelayTextBox.CaretIndex;
            DelayTextBox.Text = Regex.Replace((e.Source as TextBox).Text, RegExClass.Number, "");
            DelayTextBox.Select(caretIndex, 0);
        }

        private void PixelDifferenceTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            int caretIndex = PixelDifferenceTextBox.CaretIndex;
            PixelDifferenceTextBox.Text = Regex.Replace((e.Source as TextBox).Text, RegExClass.Number, "");
            PixelDifferenceTextBox.Select(caretIndex, 0);
        }

        private void EmailTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            int caretIndex = EmailTextBox.CaretIndex;
            EmailTextBox.Text = Regex.Replace((e.Source as TextBox).Text, RegExClass.Email, "");
            EmailTextBox.Select(caretIndex, 0);
        }
    }
}
