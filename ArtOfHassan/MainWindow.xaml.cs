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
    #region Internal

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

    #endregion

    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Dll Import

        [DllImport("user32.dll")]
        static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint dwData, int dwExtraInfo);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern IntPtr FindWindow(string strClassName, string strWindowName);

        [DllImport("user32.dll")]
        internal static extern bool GetWindowPlacement(IntPtr handle, ref WINDOWPLACEMENT placement);

        #endregion

        #region Private Variable

        private const uint MOUSEMOVE    = 0x0001;  // 마우스 이동
        private const uint ABSOLUTEMOVE = 0x8000;  // 전역 위치
        private const uint LBUTTONDOWN  = 0x0002;  // 왼쪽 마우스 버튼 눌림
        private const uint LBUTTONUP    = 0x0004;  // 왼쪽 마우스 버튼 떼어짐
        private const uint RBUTTONDOWN  = 0x0008;  // 오른쪽 마우스 버튼 눌림
        private const uint RBUTTONUP    = 0x00010; // 오른쪽 마우스 버튼 떼어짐

        private static System.Timers.Timer NoxMonitoringTimer      = new System.Timers.Timer();
        private static System.Timers.Timer ArtOfWarMonitoringTimer = new System.Timers.Timer();
        private static System.Timers.Timer ProblemMonitoringTimer  = new System.Timers.Timer();

        #endregion

        #region Public Variable

        public System.Drawing.Color PixelColor;
        public int PixelPositionX;
        public int PixelPositionY;

        public string ClickPattern = "L;R;L";

        #endregion

        #region Variable

        Stopwatch AdsCloseStopwatch          = new Stopwatch();
        Stopwatch BattleButtonInRedStopwatch = new Stopwatch();

        double NoxPointX = 0;
        double NoxPointY = 0;
        double NoxWidth  = 0;
        double NoxHeight = 0;

        bool VictoryFlag  = false;
        bool DefeatFlag   = false;
        bool AdsWatchFlag = false;

        bool IsNoGoldMailSent  = false;
        bool IsProblemOccurred = false;
        bool IsStopHassan      = false;

        int NumOfWar     = 0;
        int NumOfVictory = 0;
        int NumOfDefeat  = 0;
        int NumOfAds     = 0;

        int ProblemMailSent            = 0;
        int TimerCountForScreenCompare = 1;
        int X3GoldButtonClickDelay     = 0;

        System.Drawing.Bitmap LastBitmap;
        System.Drawing.Bitmap CurrentBitmap;

        #endregion


        public MainWindow()
        {
            InitializeComponent();

            LoadSettingTxt();
            LoadPixelTxt();

            ProblemMonitoringTimer.Interval = 3 * 60 * 1000; // 3분
            ProblemMonitoringTimer.Elapsed += ProblemMonitoringTimerFunction;

            ArtOfWarMonitoringTimer.Interval = 1000; // 1초
            ArtOfWarMonitoringTimer.Elapsed += ArtOfWarMonitoringTimerFunction;

            NoxMonitoringTimer.Interval = 200;
            NoxMonitoringTimer.Elapsed += NoxMonitoringTimerFunction;
            NoxMonitoringTimer.Enabled = true;
        }


        #region Private Event Mothod

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            ProblemMonitoringTimer.Enabled  = false;
            ArtOfWarMonitoringTimer.Enabled = false;
            NoxMonitoringTimer.Enabled      = false;
        }

        bool IsLatest = true;

        private void LatestRadioButton_Click(object sender, RoutedEventArgs e)
        {
            MonitoringLog("Working for the latest version");
            IsLatest = true;
            GoldChestBoxX = 150;
            GoldChestBoxY = 410;
            GoldButtonBackgroundY = 780;
        }

        private void OldRadioButton_Click(object sender, RoutedEventArgs e)
        {
            MonitoringLog("Working for the version 3.0.8");
            IsLatest = false;
            GoldChestBoxX = 165;
            GoldChestBoxY = 420;
            GoldButtonBackgroundY = 710;
        }

        private void AdsCloseClickPatternButton_Click(object sender, RoutedEventArgs e)
        {
            ClickPatternWindow clickPatternWindow = new ClickPatternWindow();
            clickPatternWindow.ShowDialog();
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            MonitoringLog("StartButton: " + StartButton.Content.ToString());

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

                PixelCustomizeButton.IsEnabled      = false;
                MonitoringIntervalTextBox.IsEnabled = false;

                ProblemMonitoringTimer.Interval  = int.Parse(ScreenComparisonIntervalTextBox.Text) * 60 * 1000;
                ArtOfWarMonitoringTimer.Interval = int.Parse(MonitoringIntervalTextBox.Text);
                ArtOfWarMonitoringTimer.Enabled  = true;
                ProblemMonitoringTimer.Enabled   = true;

                IsStopHassan     = false;
                IsNoGoldMailSent = false;
                ProblemMailSent  = 0;
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
                PixelCustomizeButton.IsEnabled      = true;
                MonitoringIntervalTextBox.IsEnabled = true;

                ArtOfWarMonitoringTimer.Enabled = false;
                ProblemMonitoringTimer.Enabled  = false;
            }
        }

        private void EmailTestButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(EmailAddressTextBox.Text))
            {
                if (KoreanCheckBox.IsChecked.Value)
                {
                    MessageBox.Show("이메일을 입력해주세요.");
                }
                else
                {
                    MessageBox.Show("Please input email.");
                }
            }
            else
            {
                MonitoringLog("Email Testing...");

                try
                {
                    MailMessage mailMessage = new MailMessage("artofwarhassan@gmail.com",
                                                              EmailAddressTextBox.Text,
                                                              $"Art of Hassan v{System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString()}",
                                                              "Email Testing...");

                    if ((CurrentBitmap != null) && (CurrentBitmap.Width != 0) && (CurrentBitmap.Height != 0))
                    {
                        DirectoryInfo directoryInfo = new DirectoryInfo("screenshot");
                        if (!directoryInfo.Exists)
                        {
                            directoryInfo.Create();
                        }

                        string filename = @"screenshot\Screenshot_" + DateTime.Now.ToString("yyMMdd_HHmmss_fff") + ".png";
                        CurrentBitmap.Save(filename, System.Drawing.Imaging.ImageFormat.Png);
                        mailMessage.Attachments.Add(new System.Net.Mail.Attachment(filename));
                    }

                    SmtpClient smtpClient = new SmtpClient("smtp.gmail.com", 587);
                    smtpClient.UseDefaultCredentials = false;
                    smtpClient.EnableSsl = true;
                    smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
                    smtpClient.Credentials = new NetworkCredential("artofwarhassan@gmail.com", "Rnrmf0575!");
                    smtpClient.Send(mailMessage);
                }
                catch (Exception ex)
                {
                    if (KoreanCheckBox.IsChecked.Value)
                    {
                        MessageBox.Show("이메일 송신 실패.");
                    }
                    else
                    {
                        MessageBox.Show("Email Delivery Failed.");
                    }
                }
            }
        }

        #endregion


        #region Private Method

        private void MonitoringLog(string log)
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

                using (StreamWriter streamWriter = new StreamWriter($@"log\Monitoring_{DateTime.Today.ToString("yyyyMMdd")}.log", true))
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

        private void MousePointClick(int PositionX, int PositionY)
        {
            System.Windows.Forms.Cursor.Position = new System.Drawing.Point((int)NoxPointX + PositionX, (int)NoxPointY + PositionY);
            mouse_event(LBUTTONDOWN, 0, 0, 0, 0);
            System.Threading.Thread.Sleep(50);
            mouse_event(LBUTTONUP, 0, 0, 0, 0);
        }

        #endregion


        #region Private Timer Function

        private void ProblemMonitoringTimerFunction(object sender, System.Timers.ElapsedEventArgs e)
        {
            // 전투 횟수가 올라가지 않거나 핫산 중지 상태가 아닐 때만 체크
            if (((NumOfVictory + NumOfDefeat) == NumOfWar) && !IsStopHassan)
            {
                MonitoringLog("Problem Occured...");

                IsProblemOccurred = true;

                bool isKorean          = false;
                bool isShareProblem    = true; // 무늬만 사용
                string minute          = "3";
                string emailaddress    = "";
                int pixelDifference    = 0;
                int monitoringInterval = 1000;
                System.Windows.Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
                {
                    isKorean       = KoreanCheckBox.IsChecked.Value;
                    isShareProblem = ShareProblemCheckBox.IsChecked.Value;
                    minute         = ScreenComparisonIntervalTextBox.Text;
                    emailaddress   = EmailAddressTextBox.Text;
                    if (!int.TryParse(PixelDifferenceTextBox.Text, out pixelDifference))
                    {
                        pixelDifference = 0;
                    }
                    if (int.TryParse(MonitoringIntervalTextBox.Text, out monitoringInterval))
                    {
                        if (monitoringInterval < 1000)
                        {
                            monitoringInterval = 1000;
                        }
                    }
                    else
                    {
                        monitoringInterval = 1000;
                    }
                }));

                // 이메일 전송 부분
                try
                {
                    DirectoryInfo directoryInfo = new DirectoryInfo("screenshot");
                    if (!directoryInfo.Exists)
                    {
                        directoryInfo.Create();
                    }

                    string filename = @"screenshot\Screenshot_" + DateTime.Now.ToString("yyMMdd_HHmmssfff") + ".png";
                    CurrentBitmap.Save(filename, System.Drawing.Imaging.ImageFormat.Png);

                    SmtpClient smtpClient = new SmtpClient("smtp.gmail.com", 587);
                    smtpClient.UseDefaultCredentials = false;
                    smtpClient.EnableSsl = true;
                    smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
                    smtpClient.Credentials = new NetworkCredential("artofwarhassan@gmail.com", "Rnrmf0575!");

                    if (ProblemMailSent < 3)
                    {
                        if (string.IsNullOrWhiteSpace(emailaddress))
                        {
                            MailMessage mailMessage = new MailMessage("artofwarhassan@gmail.com",
                                                                      "artofwarhassan@gmail.com",
                                                                      $"Art of Hassan v{System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString()}",
                                                                      "No Email.\nProblem reported.\nShare = " + isShareProblem);
                            mailMessage.Attachments.Add(new System.Net.Mail.Attachment(filename));
                            smtpClient.Send(mailMessage);
                        }
                        else
                        {
                            MailMessage mailMessage = new MailMessage("artofwarhassan@gmail.com",
                                                          "artofwarhassan@gmail.com",
                                                          $"Art of Hassan v{System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString()}",
                                                          $"From {emailaddress},\nProblem reported.\nShare = " + isShareProblem);
                            mailMessage.Attachments.Add(new System.Net.Mail.Attachment(filename));
                            smtpClient.Send(mailMessage);

                            // 아래 내용은 추후 제거해도 될 것 같음
                            string message;
                            if (isKorean)
                            {
                                message = $"{minute}분동안 전투횟수가 증가하지않아 Art of War 를 재시작 합니다...";
                            }
                            else
                            {
                                message = $"Number of war did not increase during {minute} minute(s).\nRestarting Art of War...";
                            }

                            mailMessage = new MailMessage("artofwarhassan@gmail.com",
                                                          emailaddress,
                                                          $"Art of Hassan v{System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString()}",
                                                          message);
                            mailMessage.Attachments.Add(new System.Net.Mail.Attachment(filename));
                            smtpClient.Send(mailMessage);
                        }
                    }
                    else if (ProblemMailSent < 5)
                    {
                        if (!string.IsNullOrWhiteSpace(emailaddress))
                        {
                            string message;
                            if (isKorean)
                            {
                                message = "Art of War 재시작이 되지 않고 있으니 확인바랍니다.";
                            }
                            else
                            {
                                message = "Restarting Art of War seems not working...";
                            }

                            MailMessage mailMessage = new MailMessage("artofwarhassan@gmail.com",
                                                          emailaddress,
                                                          $"Art of Hassan v{System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString()}",
                                                          message);
                            mailMessage.Attachments.Add(new System.Net.Mail.Attachment(filename));
                            smtpClient.Send(mailMessage);
                        }
                    }
                }
                catch (Exception ex)
                {
                    //MessageBox.Show("Email Delivery Failed");
                }

                ProblemMailSent++;


                // Latest Used App Button
                MonitoringLog("Latest Used App Button");
                MousePointClick(LatestUsedAppButtonX, LatestUsedAppButtonY);

                System.Threading.Thread.Sleep(monitoringInterval * 2);

                for (int i = 0; i < 10; i++)
                {
                    // Close All Apps Button
                    MonitoringLog("Close All Apps Button");
                    MousePointClick(RightTopAppCloseButtonX, RightTopAppCloseButtonY);

                    System.Threading.Thread.Sleep(500);
                }

                // Close Not Responding Button
                System.Drawing.Color color = CurrentBitmap.GetPixel(NotRespondAppCloseButtonX, NotRespondAppCloseButtonY);
                TimerLog("Close Not Responding Button Color: " + color.R + "," + color.G + "," + color.B);
                System.Drawing.Color color1 = ColorTranslator.FromHtml(NotRespondAppCloseButtonColor);
                if (((color.R >= color1.R - pixelDifference) && (color.G >= color1.G - pixelDifference) && (color.B >= color1.B - pixelDifference)) &&
                    ((color.R <= color1.R + pixelDifference) && (color.G <= color1.G + pixelDifference) && (color.B <= color1.B + pixelDifference)))
                {
                    MonitoringLog("Close Not Responding Button");
                    MousePointClick(NotRespondAppCloseButtonX, NotRespondAppCloseButtonY);

                    System.Threading.Thread.Sleep(monitoringInterval);
                }

                // Close System Error Button
                color = CurrentBitmap.GetPixel(NotRespondAppCloseButtonX, NotRespondAppCloseButtonY + 15);
                TimerLog("Close System Error Button Color: " + color.R + "," + color.G + "," + color.B);
                color1 = ColorTranslator.FromHtml(NotRespondAppCloseButtonColor);
                if (((color.R >= color1.R - pixelDifference) && (color.G >= color1.G - pixelDifference) && (color.B >= color1.B - pixelDifference)) &&
                    ((color.R <= color1.R + pixelDifference) && (color.G <= color1.G + pixelDifference) && (color.B <= color1.B + pixelDifference)))
                {
                    MonitoringLog("Close System Error Button");
                    MousePointClick(NotRespondAppCloseButtonX, NotRespondAppCloseButtonY + 15);

                    System.Threading.Thread.Sleep(monitoringInterval);
                }

                // Close System Error Button
                color = CurrentBitmap.GetPixel(NotRespondAppCloseButtonX, NotRespondAppCloseButtonY + 30);
                TimerLog("Close System Error Button Color: " + color.R + "," + color.G + "," + color.B);
                color1 = ColorTranslator.FromHtml(NotRespondAppCloseButtonColor);
                if (((color.R >= color1.R - pixelDifference) && (color.G >= color1.G - pixelDifference) && (color.B >= color1.B - pixelDifference)) &&
                    ((color.R <= color1.R + pixelDifference) && (color.G <= color1.G + pixelDifference) && (color.B <= color1.B + pixelDifference)))
                {
                    MonitoringLog("Close System Error Button");
                    MousePointClick(NotRespondAppCloseButtonX, NotRespondAppCloseButtonY + 30);

                    System.Threading.Thread.Sleep(monitoringInterval);
                }

                IsProblemOccurred = false;
            }
            else
            {
                NumOfWar = NumOfVictory + NumOfDefeat;
                ProblemMailSent = 0;
            }
        }

        #endregion



















        private void SaveSettingButton_Click(object sender, RoutedEventArgs e)
        {
            using (StreamWriter streamWriter = new StreamWriter($@"setting.txt", false))
            {
                System.Windows.Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
                {
                    streamWriter.WriteLine("WindowTitle," + WindowTitleTextBox.Text);
                    streamWriter.WriteLine("MonitoringInterval," + MonitoringIntervalTextBox.Text);
                    streamWriter.WriteLine("ScreenComparisonInterval," + ScreenComparisonIntervalTextBox.Text);
                    streamWriter.WriteLine("X3GoldButtonDelay," + X3GoldButtonClickDelayTextBox.Text);
                    streamWriter.WriteLine("PixelDifference," + PixelDifferenceTextBox.Text);
                    streamWriter.WriteLine("Korean," + KoreanCheckBox.IsChecked.Value);
                    streamWriter.WriteLine("AdsCloseClickPattern," + ClickPattern);
                    streamWriter.WriteLine("GoldChestCheck," + GoldChestCheckBox.IsChecked.Value);
                    streamWriter.WriteLine("Pausability," + PausabilityCheckBox.IsChecked.Value);
                    streamWriter.WriteLine("Logging," + LogCheckBox.IsChecked.Value);
                    streamWriter.WriteLine("Email," + EmailAddressTextBox.Text);
                    streamWriter.WriteLine("SendEmail," + SendEmailCheckBox.IsChecked.Value);
                    streamWriter.WriteLine("StopHassan," + StopHassanCheckBox.IsChecked.Value);
                    streamWriter.WriteLine("ShutdownPC," + ShutdownComputerCheckBox.IsChecked.Value);
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
                X3GoldButtonClickDelayTextBlock.Text = " 골드 광고\n딜레이 (ms)";
                PixelDifferenceTextBlock.Text = "픽셀 차이";
                VersionTextBlock.Text = "버전";
                LatestRadioButton.Content = "최신";
                AdsCloseTextBlock.Text = "광고 닫기";
                AdsCloseColorCheckBox.Content = "색상 확인";
                AdsCloseClickPatternButton.Content = "광고 닫기\n클릭 패턴";
                OptionTextBlock.Text = "옵션";
                GoldChestCheckBox.Content = "골드 상자";
                PausabilityCheckBox.Content = "멈춤 가능";
                LogCheckBox.Content = "로그";
                EmailAlarmTextBlock.Text = "이메일 알림";
                EmailTestButton.Content = "테스트";
                NoGoldTextBlock.Text = "골드 벌이\n  없을때";
                SendEmailCheckBox.Content = "이메일";
                StopHassanCheckBox.Content = "핫산 중지";
                ShutdownComputerCheckBox.Content = "PC 종료";
                PixelCustomizeButton.Content = "픽셀 커스텀";
                SaveSettingButton.Content = "설정 저장";
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
                X3GoldButtonClickDelayTextBlock.Text = " X3 Gold\n  Button\nDelay (ms)";
                PixelDifferenceTextBlock.Text = "   Pixel\nDifference";
                VersionTextBlock.Text = "Version";
                LatestRadioButton.Content = "Latest";
                AdsCloseTextBlock.Text = "Ads Close";
                AdsCloseColorCheckBox.Content = "Check Color";
                AdsCloseClickPatternButton.Content = "  Ads Close\nClick Pattern";
                OptionTextBlock.Text = "Option";
                GoldChestCheckBox.Content = "Gold Chest";
                PausabilityCheckBox.Content = "Pausable";
                LogCheckBox.Content = "Logging";
                EmailAlarmTextBlock.Text = "Email Alarm";
                EmailTestButton.Content = "Test";
                NoGoldTextBlock.Text = "No Gold";
                SendEmailCheckBox.Content = "Send\nEmail";
                StopHassanCheckBox.Content = " Stop\nHassan";
                ShutdownComputerCheckBox.Content = "Shutdown\nComputer";
                PixelCustomizeButton.Content = "Customize Pixel";
                SaveSettingButton.Content = "Save Setting";
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

        public int LatestUsedAppButtonX   = 580;
        public int LatestUsedAppButtonY   = 1000;
        //public string LatestUsedAppButtonColor = "#009688".ToUpper();

        public int RightTopAppCloseButtonX = 501;
        public int RightTopAppCloseButtonY = 150;

        public int NotRespondAppCloseButtonX = 79;
        public int NotRespondAppCloseButtonY = 510;
        public string NotRespondAppCloseButtonColor = "#009688".ToUpper();

        public int NoGoldX = 320;
        public int NoGoldY = 646;
        public string NoGoldColor = "#dfd6be".ToUpper();


        private void NoxMonitoringTimerFunction(object sender, System.Timers.ElapsedEventArgs e)
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
                    ProblemMonitoringTimer.Enabled  = false;
                    ArtOfWarMonitoringTimer.Enabled   = false;
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

        private void ArtOfWarMonitoringTimerFunction(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (!IsProblemOccurred)
            {
                int pixelDifference = 0;
                int monitoringInterval = 1000;
                System.Windows.Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
                {
                    if (!int.TryParse(PixelDifferenceTextBox.Text, out pixelDifference))
                    {
                        pixelDifference = 0;
                    }

                    if (!int.TryParse(MonitoringIntervalTextBox.Text, out monitoringInterval))
                    {
                        monitoringInterval = 1000;
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
                        MonitoringLog("Off Check");
                        MousePointClick(AppLocationX, AppLocationY);
                    }


                    // Shop Check
                    color = CurrentBitmap.GetPixel(ShopButtonX, ShopButtonY);
                    TimerLog("Shop Check Color: " + color.R + "," + color.G + "," + color.B);
                    color1 = ColorTranslator.FromHtml(ShopButtonColor);
                    if (((color.R >= color1.R - pixelDifference) && (color.G >= color1.G - pixelDifference) && (color.B >= color1.B - pixelDifference)) &&
                        ((color.R <= color1.R + pixelDifference) && (color.G <= color1.G + pixelDifference) && (color.B <= color1.B + pixelDifference)))
                    {
                        MonitoringLog("Shop Check");
                        MousePointClick(HomeButtonX, ShopButtonY);
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
                            MonitoringLog("Gold Chest Box");
                            MousePointClick(GoldChestBoxX, GoldChestBoxY);

                            System.Threading.Thread.Sleep((int)(monitoringInterval * 4 / 5));

                            return;
                        }
                    }


                    // Collect Button
                    color = CurrentBitmap.GetPixel(CollectButtonX, CollectButtonY);
                    TimerLog("Collect Button Color: " + color.R + "," + color.G + "," + color.B);
                    color1 = ColorTranslator.FromHtml(CollectButtonColor);
                    if (((color.R >= color1.R - pixelDifference) && (color.G >= color1.G - pixelDifference) && (color.B >= color1.B - pixelDifference)) &&
                        ((color.R <= color1.R + pixelDifference) && (color.G <= color1.G + pixelDifference) && (color.B <= color1.B + pixelDifference)))
                    {
                        MonitoringLog("Collect Button");
                        MousePointClick(CollectButtonX, CollectButtonY);
                    }


                    // Battle and Level Button
                    color = CurrentBitmap.GetPixel(BattleLevelButtonX, BattleLevelButtonY);
                    TimerLog("Battle Level Button Color: " + color.R + "," + color.G + "," + color.B);
                    color1 = ColorTranslator.FromHtml(BattleLevelButtonColor.Split(';')[0]);
                    color2 = ColorTranslator.FromHtml(BattleLevelButtonColor.Split(';')[1]);
                    System.Drawing.Color color3 = ColorTranslator.FromHtml(BattleLevelButtonColor.Split(';')[2]);
                    System.Drawing.Color color4 = ColorTranslator.FromHtml(BattleLevelButtonColor.Split(';')[3]);
                    if ((!IsStopHassan &&
                        ((color.R >= color1.R - pixelDifference) && (color.G >= color1.G - pixelDifference) && (color.B >= color1.B - pixelDifference)) &&
                        ((color.R <= color1.R + pixelDifference) && (color.G <= color1.G + pixelDifference) && (color.B <= color1.B + pixelDifference))) || // 황금색
                       (((color.R >= color2.R - pixelDifference) && (color.G >= color2.G - pixelDifference) && (color.B >= color2.B - pixelDifference)) &&
                        ((color.R <= color2.R + pixelDifference) && (color.G <= color2.G + pixelDifference) && (color.B <= color2.B + pixelDifference))))  // + 메뉴 음영
                    {
                        MonitoringLog("Battle Level Button");
                        AdsCloseStopwatch.Reset();
                        AdsCloseStopwatch.Stop();
                        BattleButtonInRedStopwatch.Reset();
                        BattleButtonInRedStopwatch.Stop();

                        MousePointClick(BattleLevelButtonX, BattleLevelButtonY);

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

                        if (AdsWatchFlag)
                        {
                            AdsWatchFlag = false;
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
                        MonitoringLog("Battle Level Button is Red");

                        if (BattleButtonInRedStopwatch.IsRunning)
                        {
                            if (BattleButtonInRedStopwatch.ElapsedMilliseconds > 30000)
                            {
                                MonitoringLog("Battle Level Cancel Button");
                                BattleButtonInRedStopwatch.Reset();
                                BattleButtonInRedStopwatch.Stop();

                                MousePointClick(BattleLevelButtonX, BattleLevelButtonY);
                            }
                        }
                        else
                        {
                            BattleButtonInRedStopwatch.Restart();
                        }

                        return;
                    }
                    else if ((color.R == color4.R) && (color.G == color4.G) && (color.B == color4.B)) // 바탕색
                    {
                        MonitoringLog("Battle Level Button is disappered");
                        return;
                    }


                    // Skill Button
                    color = CurrentBitmap.GetPixel(SkillButtonX, SkillButtonY);
                    TimerLog("Skill Button Color: " + color.R + "," + color.G + "," + color.B);
                    color1 = ColorTranslator.FromHtml(SkillButtonColor);
                    if (((color.R >= color1.R - pixelDifference) && (color.G >= color1.G - pixelDifference) && (color.B >= color1.B - pixelDifference)) &&
                        ((color.R <= color1.R + pixelDifference) && (color.G <= color1.G + pixelDifference) && (color.B <= color1.B + pixelDifference)))
                    {
                        MonitoringLog("Skill Button");
                        MousePointClick(SkillButtonX, SkillButtonY);

                        System.Threading.Thread.Sleep((int)(monitoringInterval * 3 / 4));
                        return;
                    }


                    // Speed Button
                    color = CurrentBitmap.GetPixel(SpeedButtonX, SpeedButtonY);
                    TimerLog("Speed Button Color: " + color.R + "," + color.G + "," + color.B);
                    color1 = ColorTranslator.FromHtml(SpeedButtonColor);
                    if (((color.R >= color1.R - pixelDifference) && (color.G >= color1.G - pixelDifference) && (color.B >= color1.B - pixelDifference)) &&
                        ((color.R <= color1.R + pixelDifference) && (color.G <= color1.G + pixelDifference) && (color.B <= color1.B + pixelDifference)))
                    {
                        MonitoringLog("Speed Button");
                        MousePointClick(SpeedButtonX, SpeedButtonY);

                        System.Threading.Thread.Sleep((int)(monitoringInterval * 3 / 4));
                        return;
                    }


                    // Continue Button
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
                            MonitoringLog("Continue Button");
                            MousePointClick(ContinueButtonX, ContinueButtonY);
                        }
                    }


                    // Check Victory or Defeat
                    if (!VictoryFlag && !DefeatFlag)
                    {
                        color = CurrentBitmap.GetPixel(VictoryDefeatX, VictoryDefeatY);
                        TimerLog("Victory or Defeat Color: " + color.R + "," + color.G + "," + color.B);
                        color1 = ColorTranslator.FromHtml(VictoryDefeatColor.Split(';')[0]);
                        color2 = ColorTranslator.FromHtml(VictoryDefeatColor.Split(';')[1]);
                        if (((color.R >= color1.R - pixelDifference) && (color.G >= color1.G - pixelDifference) && (color.B >= color1.B - pixelDifference)) &&
                            ((color.R <= color1.R + pixelDifference) && (color.G <= color1.G + pixelDifference) && (color.B <= color1.B + pixelDifference)))
                        {
                            MonitoringLog("Victory Checked");
                            VictoryFlag = true;
                            System.Threading.Thread.Sleep((int)(monitoringInterval * 4 / 5));

                            // X3 Gold Button Click Delay
                            X3GoldButtonClickDelay = 0;
                            System.Windows.Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
                            {
                                if (!int.TryParse(X3GoldButtonClickDelayTextBox.Text, out X3GoldButtonClickDelay))
                                {
                                    X3GoldButtonClickDelay = 0;
                                }
                            }));
                            return;
                        }
                        else if (((color.R >= color2.R - pixelDifference) && (color.G >= color2.G - pixelDifference) && (color.B >= color2.B - pixelDifference)) &&
                                 ((color.R <= color2.R + pixelDifference) && (color.G <= color2.G + pixelDifference) && (color.B <= color2.B + pixelDifference)))
                        {
                            MonitoringLog("Defeat Checked");
                            DefeatFlag = true;
                            System.Threading.Thread.Sleep((int)(monitoringInterval * 4 / 5));

                            // X3 Gold Button Click Delay
                            X3GoldButtonClickDelay = 0;
                            System.Windows.Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
                            {
                                if (!int.TryParse(X3GoldButtonClickDelayTextBox.Text, out X3GoldButtonClickDelay))
                                {
                                    X3GoldButtonClickDelay = 0;
                                }
                            }));
                            return;
                        }
                    }


                    // No Gold
                    color = CurrentBitmap.GetPixel(NoGoldX, NoGoldY);
                    TimerLog("No Gold Color: " + color.R + "," + color.G + "," + color.B);
                    color1 = ColorTranslator.FromHtml(NoGoldColor);
                    if (((color.R >= color1.R - pixelDifference) && (color.G >= color1.G - pixelDifference) && (color.B >= color1.B - pixelDifference)) &&
                        ((color.R <= color1.R + pixelDifference) && (color.G <= color1.G + pixelDifference) && (color.B <= color1.B + pixelDifference)))
                    {
                        MonitoringLog("No Gold");

                        bool isSendEmail = false;
                        IsStopHassan = false;
                        bool isShutdownPC = false;
                        System.Windows.Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
                        {
                            isSendEmail = SendEmailCheckBox.IsChecked.Value;
                            IsStopHassan = StopHassanCheckBox.IsChecked.Value;
                            isShutdownPC = ShutdownComputerCheckBox.IsChecked.Value;
                        }));

                        if (isSendEmail && !IsNoGoldMailSent)
                        {
                            MonitoringLog("Sending Email...");
                            IsNoGoldMailSent = true;

                            try
                            {
                                string emailaddress = "";
                                System.Windows.Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
                                {
                                    emailaddress = EmailAddressTextBox.Text;
                                }));
                                if (!string.IsNullOrWhiteSpace(emailaddress))
                                {
                                    MailMessage mailMessage = new MailMessage("artofwarhassan@gmail.com",
                                                                              emailaddress,
                                                                              $"Art of Hassan v{System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString()}",
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

                        if (IsStopHassan)
                        {
                            MonitoringLog("Stop Hassan...");
                        }

                        if (isShutdownPC)
                        {
                            MonitoringLog("Shutting Down PC...");

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
                                PixelCustomizeButton.IsEnabled      = true;
                                MonitoringIntervalTextBox.IsEnabled = true;

                                ArtOfWarMonitoringTimer.Enabled = false;
                                ProblemMonitoringTimer.Enabled  = false;
                                NoxMonitoringTimer.Enabled      = false;
                            }));

                            System.Diagnostics.Process.Start("shutdown.exe", "-s -f -t 0");
                        }
                    }


                    // X3 Gold Button
                    if (VictoryFlag || DefeatFlag)
                    {
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
                            if (!IsLatest ||
                               (((color.R >= color1.R - pixelDifference) && (color.G >= color1.G - pixelDifference) && (color.B >= color1.B - pixelDifference)) &&
                                ((color.R <= color1.R + pixelDifference) && (color.G <= color1.G + pixelDifference) && (color.B <= color1.B + pixelDifference)))) // Yellow
                            {
                                if (X3GoldButtonClickDelay < monitoringInterval)
                                {
                                    System.Threading.Thread.Sleep(X3GoldButtonClickDelay);
                                }
                                else
                                {
                                    X3GoldButtonClickDelay -= monitoringInterval;
                                    return;
                                }

                                MonitoringLog("Gold Button");
                                AdsWatchFlag = true;
                                AdsCloseStopwatch.Restart();

                                MousePointClick(GoldButtonBackgroundX, GoldButtonBackgroundY);
                            }
                            else // Green such as Retry
                            {
                                MonitoringLog("Next Button");

                                MousePointClick(NextButtonX, GoldButtonBackgroundY);
                            }
                        }
                        else if (((color.R >= color2.R - pixelDifference) && (color.G >= color2.G - pixelDifference) && (color.B >= color2.B - pixelDifference)) &&
                                 ((color.R <= color2.R + pixelDifference) && (color.G <= color2.G + pixelDifference) && (color.B <= color2.B + pixelDifference))) // Gray
                        {
                            if (X3GoldButtonClickDelay < monitoringInterval)
                            {
                                System.Threading.Thread.Sleep(X3GoldButtonClickDelay);
                            }
                            else
                            {
                                X3GoldButtonClickDelay -= monitoringInterval;
                                return;
                            }

                            MonitoringLog("Gold Button is Gray");

                            if (IsLatest)
                            {
                                MousePointClick(NextButtonX, GoldButtonBackgroundY);
                            }
                            else
                            {
                                MousePointClick(NextButtonX, NextButtonY);
                            }
                        }
                    }


                    // Next Button (Defeat or Old)
                    color = CurrentBitmap.GetPixel(NextButtonX, NextButtonY);
                    TimerLog("Next Button Color: " + color.R + "," + color.G + "," + color.B);
                    color1 = ColorTranslator.FromHtml(NextButtonColor);
                    if (IsLatest &&
                       (((color.R >= color1.R - pixelDifference) && (color.G >= color1.G - pixelDifference) && (color.B >= color1.B - pixelDifference)) &&
                        ((color.R <= color1.R + pixelDifference) && (color.G <= color1.G + pixelDifference) && (color.B <= color1.B + pixelDifference))))
                    {
                        MonitoringLog("Next Button");
                        MousePointClick(NextButtonX, NextButtonY);
                    }


                    // Gold Ad Close Button
                    color = CurrentBitmap.GetPixel(GameAdCloseButtonX, GoldAdCloseButtonY);
                    TimerLog("Gold Ad Close Button Color: " + color.R + "," + color.G + "," + color.B);
                    color1 = ColorTranslator.FromHtml(GameAdCloseButtonColor.Split(';')[0]);
                    color2 = ColorTranslator.FromHtml(GameAdCloseButtonColor.Split(';')[1]);
                    color3 = ColorTranslator.FromHtml(GameAdCloseButtonColor.Split(';')[2]);
                    if ((((color.R >= color1.R - pixelDifference) && (color.G >= color1.G - pixelDifference) && (color.B >= color1.B - pixelDifference)) &&
                         ((color.R <= color1.R + pixelDifference) && (color.G <= color1.G + pixelDifference) && (color.B <= color1.B + pixelDifference))) ||
                        (((color.R >= color2.R - pixelDifference) && (color.G >= color2.G - pixelDifference) && (color.B >= color2.B - pixelDifference)) &&
                         ((color.R <= color2.R + pixelDifference) && (color.G <= color2.G + pixelDifference) && (color.B <= color2.B + pixelDifference))) ||
                        (((color.R >= color3.R - pixelDifference) && (color.G >= color3.G - pixelDifference) && (color.B >= color3.B - pixelDifference)) &&
                         ((color.R <= color3.R + pixelDifference) && (color.G <= color3.G + pixelDifference) && (color.B <= color3.B + pixelDifference))))
                    {
                        MonitoringLog("Gold Ad Close Button");
                        MousePointClick(GameAdCloseButtonX, GoldAdCloseButtonY);

                        return;
                    }


                    // Troop Ad Close Button
                    color = CurrentBitmap.GetPixel(GameAdCloseButtonX, TroopAdCloseButtonY);
                    TimerLog("Troop Ad Close Button: " + color.R + "," + color.G + "," + color.B);
                    color1 = ColorTranslator.FromHtml(GameAdCloseButtonColor.Split(';')[0]);
                    color2 = ColorTranslator.FromHtml(GameAdCloseButtonColor.Split(';')[1]);
                    color3 = ColorTranslator.FromHtml(GameAdCloseButtonColor.Split(';')[2]);
                    if ((((color.R >= color1.R - pixelDifference) && (color.G >= color1.G - pixelDifference) && (color.B >= color1.B - pixelDifference)) &&
                         ((color.R <= color1.R + pixelDifference) && (color.G <= color1.G + pixelDifference) && (color.B <= color1.B + pixelDifference))) ||
                        (((color.R >= color2.R - pixelDifference) && (color.G >= color2.G - pixelDifference) && (color.B >= color2.B - pixelDifference)) &&
                         ((color.R <= color2.R + pixelDifference) && (color.G <= color2.G + pixelDifference) && (color.B <= color2.B + pixelDifference))) ||
                        (((color.R >= color3.R - pixelDifference) && (color.G >= color3.G - pixelDifference) && (color.B >= color3.B - pixelDifference)) &&
                         ((color.R <= color3.R + pixelDifference) && (color.G <= color3.G + pixelDifference) && (color.B <= color3.B + pixelDifference))))
                    {
                        MonitoringLog("Troop Ad Close Button");
                        MousePointClick(GameAdCloseButtonX, TroopAdCloseButtonY);

                        return;
                    }


                    // Midas Ad Close Button
                    color = CurrentBitmap.GetPixel(GameAdCloseButtonX, MidasAdCloseButtonY);
                    TimerLog("Midas Ad Close Button: " + color.R + "," + color.G + "," + color.B);
                    color1 = ColorTranslator.FromHtml(GameAdCloseButtonColor.Split(';')[0]);
                    color2 = ColorTranslator.FromHtml(GameAdCloseButtonColor.Split(';')[1]);
                    color3 = ColorTranslator.FromHtml(GameAdCloseButtonColor.Split(';')[2]);
                    if ((((color.R >= color1.R - pixelDifference) && (color.G >= color1.G - pixelDifference) && (color.B >= color1.B - pixelDifference)) &&
                         ((color.R <= color1.R + pixelDifference) && (color.G <= color1.G + pixelDifference) && (color.B <= color1.B + pixelDifference))) ||
                        (((color.R >= color2.R - pixelDifference) && (color.G >= color2.G - pixelDifference) && (color.B >= color2.B - pixelDifference)) &&
                         ((color.R <= color2.R + pixelDifference) && (color.G <= color2.G + pixelDifference) && (color.B <= color2.B + pixelDifference))) ||
                        (((color.R >= color3.R - pixelDifference) && (color.G >= color3.G - pixelDifference) && (color.B >= color3.B - pixelDifference)) &&
                         ((color.R <= color3.R + pixelDifference) && (color.G <= color3.G + pixelDifference) && (color.B <= color3.B + pixelDifference))))
                    {
                        MonitoringLog("Midas Ad Close Button");
                        MousePointClick(GameAdCloseButtonX, MidasAdCloseButtonY);

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
                                System.Drawing.Color lastColor = LastBitmap.GetPixel(col, row);
                                System.Drawing.Color currentColor = CurrentBitmap.GetPixel(col, row);

                                if ((lastColor.R != currentColor.R) && (lastColor.G != currentColor.G) && (lastColor.B != currentColor.B))
                                {
                                    isDifferent = true;
                                    break;
                                }
                            }
                        }

                        TimerLog("Screen is changed: " + isDifferent + ", AdsCloseStopwatchElapsed: " + AdsCloseStopwatch.ElapsedMilliseconds);

                        if (!isDifferent || (AdsCloseStopwatch.ElapsedMilliseconds > 34000))
                        {
                            MonitoringLog("Ads Close Button");
                            AdsCloseStopwatch.Reset();
                            AdsCloseStopwatch.Stop();

                            int AdsClickInterval = 333;
                            bool isGoogleAdCloseButtonColorCheck = false;
                            string[] ClickPatterns = ClickPattern.Split(';');

                            System.Windows.Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
                            {
                                AdsClickInterval = (int)(monitoringInterval / (ClickPatterns.Length - 0.5));
                                isGoogleAdCloseButtonColorCheck = AdsCloseColorCheckBox.IsChecked.Value;
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
                                        MonitoringLog("Left Ad Close Button");
                                        MousePointClick(LeftAdCloseButtonX, GoogleAdCloseButtonY);
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
                                        MonitoringLog("Right Ad Close Button");
                                        MousePointClick(RightAdCloseButtonX, GoogleAdCloseButtonY);
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
                                        MonitoringLog("Left Ad Close Button");
                                        MousePointClick(LeftAdCloseButtonX, GoogleAdCloseButtonY);
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
                                        MonitoringLog("Right Ad Close Button");
                                        MousePointClick(RightAdCloseButtonX, GoogleAdCloseButtonY);
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
                                            MonitoringLog("Left Ad Close Button");
                                            MousePointClick(LeftAdCloseButtonX, GoogleAdCloseButtonY);
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
                                            MonitoringLog("Right Ad Close Button");
                                            MousePointClick(RightAdCloseButtonX, GoogleAdCloseButtonY);
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
                                            MonitoringLog("Left Ad Close Button");
                                            MousePointClick(LeftAdCloseButtonX, GoogleAdCloseButtonY);
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
                                            MonitoringLog("Right Ad Close Button");
                                            MousePointClick(RightAdCloseButtonX, GoogleAdCloseButtonY);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                if (ClickPatterns[0] == "L")
                                {
                                    MonitoringLog("Left Ad Close Button");
                                    MousePointClick(LeftAdCloseButtonX, GoogleAdCloseButtonY);
                                }
                                else
                                {
                                    MonitoringLog("Right Ad Close Button");
                                    MousePointClick(RightAdCloseButtonX, GoogleAdCloseButtonY);
                                }

                                System.Threading.Thread.Sleep(AdsClickInterval);

                                if (ClickPatterns[1] == "L")
                                {
                                    MonitoringLog("Left Ad Close Button");
                                    MousePointClick(LeftAdCloseButtonX, GoogleAdCloseButtonY);
                                }
                                else
                                {
                                    MonitoringLog("Right Ad Close Button");
                                    MousePointClick(RightAdCloseButtonX, GoogleAdCloseButtonY);
                                }

                                if (ClickPatterns.Length == 3)
                                {
                                    System.Threading.Thread.Sleep(AdsClickInterval);

                                    if (ClickPatterns[2] == "L")
                                    {
                                        MonitoringLog("Left Ad Close Button");
                                        MousePointClick(LeftAdCloseButtonX, GoogleAdCloseButtonY);
                                    }
                                    else
                                    {
                                        MonitoringLog("Right Ad Close Button");
                                        MousePointClick(RightAdCloseButtonX, GoogleAdCloseButtonY);
                                    }
                                }

                                if (ClickPatterns.Length == 4)
                                {
                                    System.Threading.Thread.Sleep(AdsClickInterval);

                                    if (ClickPatterns[3] == "L")
                                    {
                                        MonitoringLog("Left Ad Close Button");
                                        MousePointClick(LeftAdCloseButtonX, GoogleAdCloseButtonY);
                                    }
                                    else
                                    {
                                        MonitoringLog("Right Ad Close Button");
                                        MousePointClick(RightAdCloseButtonX, GoogleAdCloseButtonY);
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

            pixelWindow.LatestUsedAppButtonX.Text = LatestUsedAppButtonX.ToString();
            pixelWindow.LatestUsedAppButtonY.Text = LatestUsedAppButtonY.ToString();
            //pixelWindow.LatestUsedAppButtonColor.Text = LatestUsedAppButtonColor.ToString();
            //pixelWindow.LatestUsedAppButton1.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(LatestUsedAppButtonColor));

            pixelWindow.RightTopAppCloseButtonX.Text = RightTopAppCloseButtonX.ToString();
            pixelWindow.RightTopAppCloseButtonY.Text = RightTopAppCloseButtonY.ToString();

            pixelWindow.NotRespondAppCloseButtonX.Text = NotRespondAppCloseButtonX.ToString();
            pixelWindow.NotRespondAppCloseButtonY.Text = NotRespondAppCloseButtonY.ToString();
            pixelWindow.NotRespondAppCloseButtonColor.Text = NotRespondAppCloseButtonColor.ToString();
            pixelWindow.NotRespondAppCloseButton1.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(NotRespondAppCloseButtonColor));

            pixelWindow.NoGoldX.Text = NoGoldX.ToString();
            pixelWindow.NoGoldY.Text = NoGoldY.ToString();
            pixelWindow.NoGoldColor.Text = NoGoldColor.ToString();
            pixelWindow.NoGoldButton1.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(NoGoldColor));

            pixelWindow.ShowDialog();
        }

        private void TextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            int caretIndex = (e.Source as TextBox).CaretIndex;
            string name = (e.Source as TextBox).Name;
            if (name.Contains("WindowTitle"))
            {
                (e.Source as TextBox).Text = Regex.Replace((e.Source as TextBox).Text, RegExClass.Location, "");
            }
            else if (name.Contains("Email"))
            {
                (e.Source as TextBox).Text = Regex.Replace((e.Source as TextBox).Text, RegExClass.Email, "");
            }
            else
            {
                (e.Source as TextBox).Text = Regex.Replace((e.Source as TextBox).Text, RegExClass.Number, "");
            }
            (e.Source as TextBox).Select(caretIndex, 0);
        }

        private void LoadSettingTxt()
        {
            FileInfo fileInfo = new FileInfo("setting.txt");
            if (fileInfo.Exists)
            {
                MonitoringLog("Load Setting...");

                string[] lines = File.ReadAllLines("setting.txt");

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
                            X3GoldButtonClickDelayTextBox.Text = listitem[1];
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
                            EmailAddressTextBox.Text = listitem[1];
                            break;
                        case ("sendemail"):
                            SendEmailCheckBox.IsChecked = bool.Parse(listitem[1]);
                            break;
                        case ("stophassan"):
                            StopHassanCheckBox.IsChecked = bool.Parse(listitem[1]);
                            break;
                        case ("shutdownpc"):
                            ShutdownComputerCheckBox.IsChecked = bool.Parse(listitem[1]);
                            break;
                    }
                }

                MonitoringLog("Load Setting Done");
            }
        }

        private void LoadPixelTxt()
        {
            FileInfo fileInfo = new FileInfo("pixel.txt");
            if (fileInfo.Exists)
            {
                MonitoringLog("Load Pixel...");

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
                        case ("collectbutton"):
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
                        case ("continuebutton"):
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
                            MidasAdCloseButtonY = int.Parse(listitem[4]);
                            GameAdCloseButtonColor = listitem[5];
                            break;
                        case ("googleadclosebutton"):
                            LeftAdCloseButtonX = int.Parse(listitem[1]);
                            RightAdCloseButtonX = int.Parse(listitem[2]);
                            GoogleAdCloseButtonY = int.Parse(listitem[3]);
                            GoogleAdCloseButtonColor = listitem[4];
                            break;
                        case ("latestusedsppbutton"):
                            LatestUsedAppButtonX = int.Parse(listitem[1]);
                            LatestUsedAppButtonY = int.Parse(listitem[2]);
                            //LatestUsedAppButtonColor = listitem[3];
                            break;
                        case ("righttopappclosebutton"):
                            RightTopAppCloseButtonX = int.Parse(listitem[1]);
                            RightTopAppCloseButtonY = int.Parse(listitem[2]);
                            //RightTopAppCloseButtonColor.Text = listitem[3];
                            break;
                        case ("notrespondappclosebutton"):
                            NotRespondAppCloseButtonX = int.Parse(listitem[1]);
                            NotRespondAppCloseButtonY = int.Parse(listitem[2]);
                            NotRespondAppCloseButtonColor = listitem[3];
                            break;
                    }
                }

                MonitoringLog("Load Pixel Done");
            }
        }
    }
}
