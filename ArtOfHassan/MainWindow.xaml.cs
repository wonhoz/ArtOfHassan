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

        [DllImport("user32.dll")] // 커서 위치 제어
        static extern int SetCursorPos(int x, int y);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern IntPtr FindWindow(string strClassName, string strWindowName);

        [DllImport("user32.dll")]
        internal static extern bool GetWindowPlacement(IntPtr handle, ref WINDOWPLACEMENT placement);

        #endregion

        #region Private Variable

        private const uint MOUSEMOVE    = 0x0001;   // 마우스 이동
        private const uint ABSOLUTEMOVE = 0x8000;   // 전역 위치
        private const uint LBUTTONDOWN  = 0x0002;   // 왼쪽 마우스 버튼 눌림
        private const uint LBUTTONUP    = 0x0004;   // 왼쪽 마우스 버튼 떼어짐
        private const uint RBUTTONDOWN  = 0x0008;   // 오른쪽 마우스 버튼 눌림
        private const uint RBUTTONUP    = 0x00010;  // 오른쪽 마우스 버튼 떼어짐
        private const uint WBUTTONDOWN  = 0x00020;  // 휠 버튼 눌림
        private const uint WBUTTONUP    = 0x000040; // 휠 버튼 떼어짐
        private const uint WBUTTONWHEEL = 0x00800;  // 휠 스크롤

        private static System.Timers.Timer NoxMonitoringTimer      = new System.Timers.Timer();
        private static System.Timers.Timer ArtOfWarMonitoringTimer = new System.Timers.Timer();
        private static System.Timers.Timer ProblemMonitoringTimer  = new System.Timers.Timer();

        #endregion

        #region Public Variable

        public System.Drawing.Color PixelColor;
        public int PixelPositionX;
        public int PixelPositionY;

        public string GoogleAdCloseClickPattern = "L;R;L";

        #endregion

        #region Point Variable

        int AppLocationX = 60;
        int AppLocationY = 500;
        string AppLocationOrangeBlueColor = "#a9d8ff".ToUpper();
        string AppLocationGreenColor      = "#97c601".ToUpper();

        int HomeButtonX = 278;
        int ShopButtonX = 65;
        int ShopButtonY = 980;
        string ShopButtonColor = "#ea3d34".ToUpper();

        int GoldChestBoxX = 190;
        int GoldChestBoxY = 370;
        int GoldChestBoxCoinLatestX = 150;
        int GoldChestBoxCoinLatestY = 410;
        int GoldChestBoxCoinV308X   = 165;
        int GoldChestBoxCoinV308Y   = 420;
        string GoldChestBoxCoinLatestColor = "#fff102".ToUpper();
        string GoldChestBoxCoinV308Color   = "#eabf2f".ToUpper();

        int CollectButtonX = 190;
        int CollectButtonY = 690;
        string CollectButtonColor = "#fdbb00".ToUpper();

        int BattleLevelButtonX = 175;
        int BattleLevelButtonY = 855;
        string BattleLevelButtonYellowColor = "#fdbb00".ToUpper();
        string BattleLevelButtonShadedColor = "#ca9600".ToUpper();
        string BattleLevelButtonRedColor    = "#bd1808".ToUpper();
        string BattleLevelButtonBackColor   = "#0d677a".ToUpper();

        int SkillButtonX = 475;
        int SkillButtonY = 920;
        string SkillButtonColor = "#fdbb00".ToUpper();

        int SpeedButtonX = 514;
        int SpeedButtonY = 989;
        string SpeedButtonColor = "#eda500".ToUpper();

        int ContinueButtonX = 215;
        int ContinueButtonY = 455;
        string ContinueButtonColor = "#fdbb00".ToUpper();

        int VictoryDefeatX = 120;
        int VictoryDefeatY = 355;
        string VictoryDefeatVictoryColor = "#d91c13".ToUpper();
        string VictoryDefeatDefeatColor  = "#12a7d8".ToUpper();

        int NoGoldX = 321;
        int NoGoldY = 646;
        string NoGoldColor = "#dfd6be".ToUpper();

        int GoldButtonBackgroundX = 115;
        int GoldButtonBackground3StarY  = 780;
        int GoldButtonBackgroundNoStarY = 685;
        string GoldButtonBackgroundGreenColor = "#7da70a".ToUpper();
        string GoldButtonBackgroundGrayColor  = "#8e8e8e".ToUpper();

        int GoldButtonImageX = 133;
        int GoldButtonImageY = 755;
        string GoldButtonImageColor = "#ffea90".ToUpper();

        int NextButtonX = 450;
        int NextButtonY = 710;
        string NextButtonColor = "#fdbb00".ToUpper();

        int GameAdCloseButtonX = 496;
        int GoldAdCloseButtonY = 180;
        int TroopAdCloseButtonY = 190;
        int MidasAdCloseButtonY = 262;
        string GameAdCloseButtonLatestColor = "#e9e9d8".ToUpper();
        string GameAdCloseButtonV308Color   = "#efe7d6".ToUpper();

        int LeftAdCloseButtonX = 45;
        int RightAdCloseButtonX = 513;
        int GoogleAdCloseButtonY = 63;

        int LatestUsedAppButtonX = 580;
        int LatestUsedAppButtonY = 1000;

        int RightTopAppCloseButtonX = 501;
        int RightTopAppCloseButtonY = 150;

        int NotRespondAppCloseButtonX = 79;
        int NotRespondAppCloseButtonY1 = 510;
        int NotRespondAppCloseButtonY2 = 525;
        int NotRespondAppCloseButtonY3 = 540;
        string NotRespondAppCloseButtonColor = "#009688".ToUpper();

        // Headhunt
        int HeadhuntButtonX = 515;
        int HeadhuntButtonY = 380;
        string HeadhuntButtonYellowColor = "#fdbb00".ToUpper();
        string HeadhuntButtonGrayColor   = "#572f17".ToUpper();

        // Troop
        int TroopButtonX = 470;
        int TroopButtonY = 860;
        string TroopButtonColor = "#fac91c".ToUpper();

        int TroopOpenButtonX = 384;
        int TroopOpenButtonY = 894;
        string TroopOpenButtonCenterColor = "#f6a408".ToUpper();
        string TroopOpenButtonRightColor  = "#d08a0f".ToUpper();

        int TroopCloseButtonX = 239;
        int TroopCloseButtonY = 868;
        string TroopCloseButtonColor = "#ffffff".ToUpper();

        // Honor
        int HonorChallengeButtonX = 425;
        int HonorChallengeButtonY = 895;
        string HonorChallengeButtonColor = "#fdbb00".ToUpper();

        int HonorFightButtonX = 175;
        int HonorFightButtonY = 820;
        string HonorFightButtonColor = "#fdcc00".ToUpper();

        int HonorSkillButtonX = 35;
        int HonorSkillButtonY = 902;

        int HonorPauseButtonX = 43;
        int HonorPauseButtonY = 68;

        int HonorQuitButtonX = 177;
        int HonorQuitButtonY = 533;
        string HonorQuitButtonColor = "#93bd20".ToUpper();

        int HonorHeroPositionX = 490;
        int HonorHeroPositionY = 600;

        int HonorReplaceButtonX = 477;
        int HonorReplaceButtonY = 477;
        string HonorReplaceButtonColor = "#1ca813".ToUpper();

        int HonorHeroWindowX = 390;
        int HonorHeroWindowY = 295;
        string HonorHeroWindowColor = "#215b84".ToUpper();

        int HonorHeroWindowCloseButtonX = 467;
        int HonorHeroWindowCloseButtonY = 295;

        #endregion

        #region Variable

        Stopwatch AdsCloseStopwatch          = new Stopwatch();
        Stopwatch GoldChestBoxStopwatch      = new Stopwatch();
        Stopwatch BattleButtonInRedStopwatch = new Stopwatch();

        double NoxPointX = 0;
        double NoxPointY = 0;
        double NoxWidth  = 0;
        double NoxHeight = 0;

        bool VictoryFlag  = false;
        bool DefeatFlag   = false;
        bool AdsWatchFlag = false;

        bool IsProblemOccurred = false;
        bool IsNoGoldStatus    = false;
        bool IsNoGoldMailSent  = false;
        bool IsStopHassan      = false;

        int NumOfWar     = 0;
        int NumOfVictory = 0;
        int NumOfDefeat  = 0;
        int NumOfAds     = 0;

        int ProblemMailSent            = 0;
        int TimerCountForScreenCompare = 1;
        int X3GoldButtonClickDelay     = 0; // UI 설정값이지만 가변적

        System.Drawing.Bitmap LastBitmap;
        System.Drawing.Bitmap CurrentBitmap;

        // Honor
        bool IsHonorFightButtonClicked  = false;
        bool IsGetHonorSkillButtonColor = false;
        bool IsHonorPauseButtonClicked  = false;

        int HonorSkillChangeCount = 0;
        int HonorHeroWindowCount  = 0;

        System.Drawing.Color HonorSkillColor;

        #endregion

        #region UI Variable

        // UI 설정값
        int MonitoringInterval       = 1000;
        int ScreenComparisonInterval = 3;
        int PixelDifference          = 1;
        int HonorHeroChangeTime      = 2000;

        bool IsKorean = false;

        bool     IsWatchAds                      = true;
        int      GoogleAdCloseClickInterval      = 333;
        string[] GoogleAdCloseClickPatterns;

        bool IsOpenGoldChestBox = false;
        bool IsPausable         = false;

        string EmailAddress;

        bool IsNoGoldSendEmail  = false;
        bool IsNoGoldShutdownPC = false;

        bool IsOpenHonorHeroWindow = true;

        bool IsShareProblem = true; // 무늬만 사용

        string Version;

        #endregion


        public MainWindow()
        {
            InitializeComponent();

            Version = "v" +
                      System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.Major.ToString() + "." +
                      System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.Minor.ToString() + "." +
                      System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.Build.ToString();

            LoadSettingCsv();
            LoadPositionCsv("position.csv");
            LoadColorCsv("color.csv");

            SavePositionCsv();
            SaveColorCsv();

            ProblemMonitoringTimer.Interval = 3 * 60 * 1000; // 3분
            ProblemMonitoringTimer.Elapsed += ProblemMonitoringTimerFunction;

            ArtOfWarMonitoringTimer.Interval = 1000; // 1초
            ArtOfWarMonitoringTimer.Elapsed += ArtOfWarMonitoringTimerFunction;

            NoxMonitoringTimer.Interval = 200;
            NoxMonitoringTimer.Elapsed += NoxMonitoringTimerFunction;
            NoxMonitoringTimer.Enabled = true;
        }


        #region Private Timer Function

        private void NoxMonitoringTimerFunction(object sender, System.Timers.ElapsedEventArgs e)
        {
            System.Windows.Point point = new System.Windows.Point();
            System.Windows.Size size   = new System.Windows.Size();

            GetWindowPos(GetWinAscHandle(), ref point, ref size);

            System.Windows.Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
            {
                if (!int.TryParse(MonitoringIntervalTextBox.Text, out MonitoringInterval))
                {
                    MonitoringInterval = 1000;
                }

                if (!int.TryParse(ScreenComparisonIntervalTextBox.Text, out ScreenComparisonInterval))
                {
                    ScreenComparisonInterval = 3;
                }

                if (!int.TryParse(PixelDifferenceTextBox.Text, out PixelDifference))
                {
                    PixelDifference = 0;
                }

                if (!int.TryParse(HonorHeroChangeTimeTextBox.Text, out HonorHeroChangeTime))
                {
                    HonorHeroChangeTime = 2000;
                }

                IsKorean           = KoreanCheckBox.IsChecked.Value;
                IsWatchAds         = AdsWatchCheckBox.IsChecked.Value;
                IsOpenGoldChestBox = GoldChestCheckBox.IsChecked.Value;
                IsPausable         = PausabilityCheckBox.IsChecked.Value;

                EmailAddress = EmailAddressTextBox.Text;

                IsNoGoldSendEmail  = SendEmailCheckBox.IsChecked.Value;
                IsNoGoldShutdownPC = ShutdownComputerCheckBox.IsChecked.Value;
                IsShareProblem     = ShareProblemCheckBox.IsChecked.Value;

                GoogleAdCloseClickPatterns      = GoogleAdCloseClickPattern.Split(';');
                GoogleAdCloseClickInterval      = (int)(MonitoringInterval / (GoogleAdCloseClickPatterns.Length - 0.5));

                IsOpenHonorHeroWindow = HonorHeroChangeCheckBox.IsChecked.Value;


                if ((size.Width == 0) || (size.Height == 0))
                {
                    ProblemMonitoringTimer.Enabled  = false;
                    ArtOfWarMonitoringTimer.Enabled = false;
                    StartButton.IsEnabled = false;
                    if (IsKorean)
                    {
                        StartButton.Content = "시작";
                    }
                    else
                    {
                        StartButton.Content = "Start";
                    }
                }
                else
                {
                    NoxPointX = point.X;
                    NoxPointY = point.Y;
                    NoxWidth  = size.Width;
                    NoxHeight = size.Height;

                    StartButton.IsEnabled = true;
                }
            }));
        }

        private void ProblemMonitoringTimerFunction(object sender, System.Timers.ElapsedEventArgs e)
        {
            // 전투 횟수가 올라가지 않거나 핫산 중지 상태가 아닐 때만 체크
            if (!IsTroopMode && !IsStopHassan && ((NumOfVictory + NumOfDefeat) == NumOfWar))
            {
                MonitoringLog("Problem Occured...");

                IsProblemOccurred = true;

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

                    if (ProblemMailSent == 2)
                    {
                        if (string.IsNullOrWhiteSpace(EmailAddress))
                        {
                            MailMessage mailMessage = new MailMessage("artofwarhassan@gmail.com",
                                                                      "artofwarhassan@gmail.com",
                                                                      $"Art of Hassan {Version}",
                                                                      "No Email.\nProblem reported.\nShare = " + IsShareProblem);
                            mailMessage.Attachments.Add(new System.Net.Mail.Attachment(filename));
                            smtpClient.Send(mailMessage);
                        }
                        else
                        {
                            MailMessage mailMessage = new MailMessage("artofwarhassan@gmail.com",
                                                          "artofwarhassan@gmail.com",
                                                          $"Art of Hassan {Version}",
                                                          $"From {EmailAddress},\nProblem reported.\nShare = " + IsShareProblem);
                            mailMessage.Attachments.Add(new System.Net.Mail.Attachment(filename));
                            smtpClient.Send(mailMessage);

                            string message;
                            if (IsKorean)
                            {
                                message = "Art of War 재시작이 되지 않고 있으니 확인바랍니다.";
                            }
                            else
                            {
                                message = "Restarting Art of War seems not working...";
                            }

                            mailMessage = new MailMessage("artofwarhassan@gmail.com",
                                                          EmailAddress,
                                                          $"Art of Hassan {Version}",
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


                // NotRespondAppCloseButton
                if (MousePointColorCheck(NotRespondAppCloseButtonX, NotRespondAppCloseButtonY1, NotRespondAppCloseButtonColor))
                {
                    MonitoringLog("NotRespondAppCloseButton");
                    MousePointClick(NotRespondAppCloseButtonX, NotRespondAppCloseButtonY1);

                    System.Threading.Thread.Sleep(MonitoringInterval);
                }

                // NotRespondAppCloseButton
                if (MousePointColorCheck(NotRespondAppCloseButtonX, NotRespondAppCloseButtonY2, NotRespondAppCloseButtonColor))
                {
                    MonitoringLog("NotRespondAppCloseButton15");
                    MousePointClick(NotRespondAppCloseButtonX, NotRespondAppCloseButtonY2);

                    System.Threading.Thread.Sleep(MonitoringInterval);
                }

                // NotRespondAppCloseButton
                if (MousePointColorCheck(NotRespondAppCloseButtonX, NotRespondAppCloseButtonY3, NotRespondAppCloseButtonColor))
                {
                    MonitoringLog("NotRespondAppCloseButton30");
                    MousePointClick(NotRespondAppCloseButtonX, NotRespondAppCloseButtonY3);

                    System.Threading.Thread.Sleep(MonitoringInterval);
                }

                if (!(MousePointColorCheck(AppLocationX, AppLocationY, AppLocationOrangeBlueColor) ||
                      MousePointColorCheck(AppLocationX, AppLocationY, AppLocationGreenColor)))
                {
                    // LatestUsedAppButton
                    MonitoringLog("LatestUsedAppButton");
                    MousePointClick(LatestUsedAppButtonX, LatestUsedAppButtonY);

                    System.Threading.Thread.Sleep(MonitoringInterval * 2);

                    for (int i = 0; i < 10; i++)
                    {
                        // RightTopAppCloseButton
                        MonitoringLog("RightTopAppCloseButton");
                        MousePointClick(RightTopAppCloseButtonX, RightTopAppCloseButtonY);

                        System.Threading.Thread.Sleep(500);
                    }
                }

                IsProblemOccurred = false;
            }
            else
            {
                NumOfWar = NumOfVictory + NumOfDefeat;
                ProblemMailSent = 0;
            }
        }

        private void ArtOfWarMonitoringTimerFunction(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (!IsProblemOccurred)
            {
                //MonitoringLog("Monitoring...");

                // 화면 크기만큼의 Bitmap 생성
                CurrentBitmap = new System.Drawing.Bitmap((int)NoxWidth, (int)NoxHeight, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                if (LastBitmap == null)
                {
                    LastBitmap = CurrentBitmap;
                }

                // Bitmap 이미지 변경을 위해 Graphics 객체 생성
                using (System.Drawing.Graphics graphics = System.Drawing.Graphics.FromImage(CurrentBitmap))
                {
                    // 화면을 그대로 카피해서 Bitmap 메모리에 저장
                    graphics.CopyFromScreen((int)NoxPointX, (int)NoxPointY, 0, 0, CurrentBitmap.Size);
                    // Bitmap 데이타를 파일로 저장
                    //bitmap.Save(imageName + ".png", System.Drawing.Imaging.ImageFormat.Png);


                    // Honor Mode
                    if (IsHonorMode)
                    {
                        // HonorChallengeButton
                        if (MousePointColorCheck(HonorChallengeButtonX, HonorChallengeButtonY, HonorChallengeButtonColor))
                        {
                            MonitoringLog("HonorChallengeButton");
                            MousePointClick(HonorChallengeButtonX, HonorChallengeButtonY);

                            System.Threading.Thread.Sleep((int)(MonitoringInterval * 3 / 4));

                            return;
                        }

                        // HonorFightButton
                        if (!IsHonorFightButtonClicked &&
                            MousePointColorCheck(HonorFightButtonX, HonorFightButtonY, HonorFightButtonColor))
                        {
                            MonitoringLog("HonorFightButton");
                            MousePointClick(HonorFightButtonX, HonorFightButtonY);

                            System.Threading.Thread.Sleep((int)(MonitoringInterval * 3 / 4));

                            IsHonorFightButtonClicked = true;
                            return;
                        }

                        // HonorSkillColor
                        if (IsHonorFightButtonClicked &&
                            !IsGetHonorSkillButtonColor &&
                            !MousePointColorCheck(HonorChallengeButtonX, HonorChallengeButtonY, HonorChallengeButtonColor) &&
                            !MousePointColorCheck(HonorFightButtonX, HonorFightButtonY, HonorFightButtonColor) &&
                            !MousePointColorCheck(HonorQuitButtonX, HonorQuitButtonY, HonorQuitButtonColor) &&
                            !MousePointColorCheck(HonorHeroWindowX, HonorHeroWindowY, HonorHeroWindowColor))
                        {
                            MonitoringLog("HonorSkillColor");
                            HonorSkillColor = CurrentBitmap.GetPixel(HonorSkillButtonX, HonorSkillButtonY);
                            //CurrentBitmap.Save("HonorSkillColor.png", System.Drawing.Imaging.ImageFormat.Png);
                            IsGetHonorSkillButtonColor = true;
                            return;
                        }

                        // HonorSkillColor Change
                        if (IsGetHonorSkillButtonColor &&
                            !MousePointColorCheck(HonorChallengeButtonX, HonorChallengeButtonY, HonorChallengeButtonColor) &&
                            !MousePointColorCheck(HonorFightButtonX, HonorFightButtonY, HonorFightButtonColor) &&
                            !MousePointColorCheck(HonorQuitButtonX, HonorQuitButtonY, HonorQuitButtonColor) &&
                            !MousePointColorCheck(HonorHeroWindowX, HonorHeroWindowY, HonorHeroWindowColor))
                        {
                            System.Drawing.Color currentHonorSkillColor = CurrentBitmap.GetPixel(HonorSkillButtonX, HonorSkillButtonY);
                            if (!(((currentHonorSkillColor.R >= HonorSkillColor.R - PixelDifference) && (currentHonorSkillColor.G >= HonorSkillColor.G - PixelDifference) && (currentHonorSkillColor.B >= HonorSkillColor.B - PixelDifference)) &&
                                  ((currentHonorSkillColor.R <= HonorSkillColor.R + PixelDifference) && (currentHonorSkillColor.G <= HonorSkillColor.G + PixelDifference) && (currentHonorSkillColor.B <= HonorSkillColor.B + PixelDifference))))
                            {
                                if (HonorSkillChangeCount == 0)
                                {
                                    MonitoringLog($"HonorSkillColor Change Phase {HonorSkillChangeCount}: Initial");
                                    //CurrentBitmap.Save($"HonorSkillColorChanged_{HonorSkillChangeCount}.png", System.Drawing.Imaging.ImageFormat.Png);
                                    HonorSkillChangeCount++;
                                }

                                if (HonorSkillChangeCount == 1)
                                {
                                    if ((HonorSkillColor.R + HonorSkillColor.G + HonorSkillColor.B) <
                                        (currentHonorSkillColor.R + currentHonorSkillColor.G + currentHonorSkillColor.B))
                                    {
                                        HonorSkillChangeCount++;
                                        MonitoringLog($"HonorSkillColor Change Phase {HonorSkillChangeCount}: First Bright");
                                        //CurrentBitmap.Save($"HonorSkillColorChanged_{HonorSkillChangeCount}.png", System.Drawing.Imaging.ImageFormat.Png);
                                    }
                                    else
                                    {
                                        MonitoringLog($"HonorSkillColor Change Phase {HonorSkillChangeCount}: First Dark");
                                        //CurrentBitmap.Save($"HonorSkillColorChanged_{HonorSkillChangeCount}.png", System.Drawing.Imaging.ImageFormat.Png);
                                    }
                                }

                                if ((HonorSkillChangeCount == 2) &&
                                   ((HonorSkillColor.R + HonorSkillColor.G + HonorSkillColor.B) >
                                    (currentHonorSkillColor.R + currentHonorSkillColor.G + currentHonorSkillColor.B)))
                                {
                                    HonorSkillChangeCount++;
                                    MonitoringLog($"HonorSkillColor Change Phase {HonorSkillChangeCount}: Second Dark");
                                    //CurrentBitmap.Save($"HonorSkillColorChanged_{HonorSkillChangeCount}.png", System.Drawing.Imaging.ImageFormat.Png);
                                }

                                if ((HonorSkillChangeCount == 3) &&
                                    ((HonorSkillColor.R + HonorSkillColor.G + HonorSkillColor.B) <
                                     (currentHonorSkillColor.R + currentHonorSkillColor.G + currentHonorSkillColor.B)))
                                {
                                    HonorSkillChangeCount++;
                                    MonitoringLog($"HonorSkillColor Change Phase {HonorSkillChangeCount}: Second Bright");
                                    //CurrentBitmap.Save($"HonorSkillColorChanged_{HonorSkillChangeCount}.png", System.Drawing.Imaging.ImageFormat.Png);
                                }

                                HonorSkillColor = currentHonorSkillColor;
                            }
                        }

                        // HonorSkillColor Changes 4 - Time to Quit
                        if (!IsHonorPauseButtonClicked && (HonorSkillChangeCount == 4))
                        {
                            HonorSkillChangeCount++;

                            MonitoringLog("HonorPauseButton");
                            MousePointClick(HonorPauseButtonX, HonorPauseButtonY);

                            System.Threading.Thread.Sleep((int)(MonitoringInterval * 3 / 4));

                            IsHonorPauseButtonClicked = true;
                            return;
                        }

                        // HonorQuitButton
                        if (MousePointColorCheck(HonorQuitButtonX, HonorQuitButtonY, HonorQuitButtonColor))
                        {
                            MonitoringLog("HonorQuitButton");
                            MousePointClick(HonorQuitButtonX, HonorQuitButtonY);

                            System.Threading.Thread.Sleep((int)(MonitoringInterval * 3 / 4));

                            return;
                        }

                        if (IsOpenHonorHeroWindow)
                        {
                            // HonorHeroPosition
                            if (IsHonorPauseButtonClicked &&
                               (HonorHeroWindowCount < (HonorHeroChangeTime / MonitoringInterval)) &&
                                MousePointColorCheck(HonorFightButtonX, HonorFightButtonY, HonorFightButtonColor) &&
                                !MousePointColorCheck(HonorReplaceButtonX, HonorReplaceButtonY, HonorReplaceButtonColor))
                            {
                                MonitoringLog("HonorHeroPosition");
                                MousePointClick(HonorHeroPositionX, HonorHeroPositionY);

                                System.Threading.Thread.Sleep((int)(MonitoringInterval * 3 / 4));
                                return;
                            }

                            // HonorReplaceButton
                            if (IsHonorPauseButtonClicked &&
                                MousePointColorCheck(HonorFightButtonX, HonorFightButtonY, HonorFightButtonColor) &&
                                MousePointColorCheck(HonorReplaceButtonX, HonorReplaceButtonY, HonorReplaceButtonColor))
                            {
                                MonitoringLog("HonorReplaceButton");
                                MousePointClick(HonorReplaceButtonX, HonorReplaceButtonY);

                                System.Threading.Thread.Sleep((int)(MonitoringInterval * 3 / 4));
                                return;
                            }

                            // HonorHeroWindow
                            if (MousePointColorCheck(HonorHeroWindowX, HonorHeroWindowY, HonorHeroWindowColor))
                            {
                                HonorHeroWindowCount++;
                                MonitoringLog("HonorHeroWindow: " + HonorHeroWindowCount);
                            }

                            // HonorHeroWindow Close
                            if ((HonorHeroWindowCount == (HonorHeroChangeTime / MonitoringInterval)) &&
                                MousePointColorCheck(HonorHeroWindowX, HonorHeroWindowY, HonorHeroWindowColor))
                            {
                                MonitoringLog("HonorHeroWindowCloseButton");
                                MousePointClick(HonorHeroWindowCloseButtonX, HonorHeroWindowCloseButtonY);

                                System.Threading.Thread.Sleep((int)(MonitoringInterval * 3 / 4));

                                IsHonorFightButtonClicked = false;
                                return;
                            }
                        }
                        else
                        {
                            IsHonorFightButtonClicked = false;
                            return;
                        }

                        // Honor Mode Finish
                        if (IsHonorFightButtonClicked &&
                            IsHonorPauseButtonClicked &&
                            (HonorSkillChangeCount > 4) &&
                            (HonorHeroWindowCount >= (HonorHeroChangeTime / MonitoringInterval)) &&
                            !MousePointColorCheck(HonorChallengeButtonX, HonorChallengeButtonY, HonorChallengeButtonColor) &&
                            !MousePointColorCheck(HonorFightButtonX, HonorFightButtonY, HonorFightButtonColor) &&
                            !MousePointColorCheck(HonorQuitButtonX, HonorQuitButtonY, HonorQuitButtonColor) &&
                            !MousePointColorCheck(HonorHeroWindowX, HonorHeroWindowY, HonorHeroWindowColor))
                        {
                            MonitoringLog("Honor Mode Finish...");

                            System.Windows.Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
                            {
                                if (IsKorean)
                                {
                                    StartButton.Content = "시작";
                                }
                                else
                                {
                                    StartButton.Content = "Start";
                                }
                                MonitoringIntervalTextBox.IsEnabled = true;
                                ModeGrid.IsEnabled                  = true;

                                ArtOfWarMonitoringTimer.Enabled = false;
                                ProblemMonitoringTimer.Enabled  = false;
                                NoxMonitoringTimer.Enabled      = false;
                            }));
                        }

                        return;
                    }


                    // Troop Mode
                    if (IsTroopMode)
                    {
                        // TroopButton
                        if (MousePointColorCheck(TroopButtonX, TroopButtonY, TroopButtonColor))
                        {
                            MonitoringLog("TroopButton");
                            MousePointClick(TroopButtonX, TroopButtonY);
                        }

                        // TroopOpenButton
                        if (MousePointColorCheck(TroopOpenButtonX, TroopOpenButtonY, TroopOpenButtonCenterColor) ||
                            MousePointColorCheck(TroopOpenButtonX, TroopOpenButtonY, TroopOpenButtonRightColor))
                        {
                            MonitoringLog("TroopOpenButton");
                            MousePointClick(TroopOpenButtonX, TroopOpenButtonY);
                        }

                        // TroopCloseButton
                        if (MousePointColorCheck(TroopCloseButtonX, TroopCloseButtonY, TroopCloseButtonColor))
                        {
                            MonitoringLog("TroopCloseButton");
                            MousePointClick(TroopButtonX, TroopButtonY);
                        }

                        return;
                    }


                    // AppLocation
                    if (MousePointColorCheck(AppLocationX, AppLocationY, AppLocationOrangeBlueColor) ||
                        MousePointColorCheck(AppLocationX, AppLocationY, AppLocationGreenColor))
                    {
                        MonitoringLog("AppLocation");
                        MousePointClick(AppLocationX, AppLocationY);
                    }


                    // HomeButton
                    if (MousePointColorCheck(ShopButtonX, ShopButtonY, ShopButtonColor))
                    {
                        MonitoringLog("HomeButton");
                        MousePointClick(HomeButtonX, ShopButtonY);
                    }


                    // GoldChestBox
                    if (IsOpenGoldChestBox)
                    {
                        if (MousePointColorCheck(GoldChestBoxCoinLatestX, GoldChestBoxCoinLatestY, GoldChestBoxCoinLatestColor) ||
                            MousePointColorCheck(GoldChestBoxCoinV308X,   GoldChestBoxCoinV308Y,   GoldChestBoxCoinV308Color) ||
                            (GoldChestBoxStopwatch.ElapsedMilliseconds > 4 * 60 * 60 * 1000)) // 4시간
                        {
                            MonitoringLog("GoldChestBox");
                            GoldChestBoxStopwatch.Restart();
                            MousePointClick(GoldChestBoxX, GoldChestBoxY);

                            System.Threading.Thread.Sleep((int)(MonitoringInterval * 4 / 5));

                            return;
                        }
                    }


                    // CollectButton
                    if (MousePointColorCheck(CollectButtonX, CollectButtonY, CollectButtonColor))
                    {
                        MonitoringLog("CollectButton");
                        MousePointClick(CollectButtonX, CollectButtonY);
                    }


                    // HeadhuntButton
                    if (MousePointColorCheck(HeadhuntButtonX, HeadhuntButtonY, HeadhuntButtonYellowColor))
                    {
                        MonitoringLog("HeadhuntButton");
                        MousePointClick(HeadhuntButtonX, HeadhuntButtonY);
                    }
                    else if (MousePointColorCheck(HeadhuntButtonX, HeadhuntButtonY, HeadhuntButtonGrayColor))
                    {
                        MonitoringLog("Headhunt Finished");

                        System.Windows.Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
                        {
                            StageRadioButton.IsChecked    = true;
                            HeadhuntRadioButton.IsChecked = false;
                            AdsWatchCheckBox.IsChecked    = PrevAdsWatch;
                        }));

                        MousePointClick(LeftAdCloseButtonX, GoogleAdCloseButtonY);

                        System.Threading.Thread.Sleep((int)(MonitoringInterval * 4 / 5));

                        MousePointClick(HomeButtonX, ShopButtonY);
                    }


                    // BattleLevelButton
                    if ((!IsStopHassan &&
                         MousePointColorCheck(BattleLevelButtonX, BattleLevelButtonY, BattleLevelButtonYellowColor)) ||
                         MousePointColorCheck(BattleLevelButtonX, BattleLevelButtonY, BattleLevelButtonShadedColor))
                    {
                        MonitoringLog("BattleLevelButton");
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
                            if (IsKorean)
                            {
                                MessageBar.Text = $"전투: {NumOfVictory + NumOfDefeat}  |  승리: {NumOfVictory}  |  패배: {NumOfDefeat}  |  광고: {NumOfAds}";
                            }
                            else
                            {
                                MessageBar.Text = $"War: {NumOfVictory + NumOfDefeat}  |  Victory: {NumOfVictory}  |  Defeat: {NumOfDefeat}  |  Ads: {NumOfAds}";
                            }
                        }));
                    }
                    else if (MousePointColorCheck(BattleLevelButtonX, BattleLevelButtonY, BattleLevelButtonRedColor)) // 빨간색
                    {
                        MonitoringLog("BattleLevelButton is Red");

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
                    else if (MousePointColorCheck(BattleLevelButtonX, BattleLevelButtonY, BattleLevelButtonBackColor)) // 바탕색
                    {
                        MonitoringLog("BattleLevelButton is disappered");
                        return;
                    }


                    // SkillButton
                    if (MousePointColorCheck(SkillButtonX, SkillButtonY, SkillButtonColor))
                    {
                        MonitoringLog("SkillButton");
                        MousePointClick(SkillButtonX, SkillButtonY);

                        System.Threading.Thread.Sleep((int)(MonitoringInterval * 3 / 4));
                        return;
                    }


                    // SpeedButton
                    if (MousePointColorCheck(SpeedButtonX, SpeedButtonY, SpeedButtonColor))
                    {
                        MonitoringLog("SpeedButton");
                        MousePointClick(SpeedButtonX, SpeedButtonY);

                        System.Threading.Thread.Sleep((int)(MonitoringInterval * 3 / 4));
                        return;
                    }


                    // ContinueButton
                    if (!IsPausable)
                    {
                        if (MousePointColorCheck(ContinueButtonX, ContinueButtonY, ContinueButtonColor))
                        {
                            MonitoringLog("ContinueButton");
                            MousePointClick(ContinueButtonX, ContinueButtonY);
                        }
                    }


                    // VictoryDefeat
                    if (!VictoryFlag && !DefeatFlag)
                    {
                        if (MousePointColorCheck(VictoryDefeatX, VictoryDefeatY, VictoryDefeatVictoryColor))
                        {
                            MonitoringLog("Victory Checked");
                            VictoryFlag = true;
                            System.Threading.Thread.Sleep((int)(MonitoringInterval * 4 / 5));

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
                        else if (MousePointColorCheck(VictoryDefeatX, VictoryDefeatY, VictoryDefeatDefeatColor))
                        {
                            MonitoringLog("Defeat Checked");
                            DefeatFlag = true;
                            System.Threading.Thread.Sleep((int)(MonitoringInterval * 4 / 5));

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


                    // NoGold
                    if (MousePointColorCheck(NoGoldX, NoGoldY, NoGoldColor))
                    {
                        MonitoringLog("NoGold");

                        IsNoGoldStatus = true;
                        IsStopHassan   = false;
                        
                        System.Windows.Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
                        {
                            IsStopHassan = StopHassanCheckBox.IsChecked.Value;
                        }));

                        if (!IsNoGoldMailSent)
                        {
                            IsNoGoldMailSent = true;

                            try
                            {
                                string filename = @"screenshot\Screenshot_" + DateTime.Now.ToString("yyMMdd_HHmmssfff") + ".png";
                                CurrentBitmap.Save(filename, System.Drawing.Imaging.ImageFormat.Png);

                                SmtpClient smtpClient = new SmtpClient("smtp.gmail.com", 587);
                                smtpClient.UseDefaultCredentials = false;
                                smtpClient.EnableSsl = true;
                                smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
                                smtpClient.Credentials = new NetworkCredential("artofwarhassan@gmail.com", "Rnrmf0575!");

                                if (string.IsNullOrWhiteSpace(EmailAddress))
                                {
                                    MailMessage mailMessage = new MailMessage("artofwarhassan@gmail.com",
                                                                              "artofwarhassan@gmail.com",
                                                                              $"Art of Hassan {Version}",
                                                                              "No Email.\nNo Gold.\nShare = " + IsShareProblem);
                                    mailMessage.Attachments.Add(new System.Net.Mail.Attachment(filename));
                                    smtpClient.Send(mailMessage);
                                }
                                else
                                {
                                    MailMessage mailMessage = new MailMessage("artofwarhassan@gmail.com",
                                                                  "artofwarhassan@gmail.com",
                                                                  $"Art of Hassan {Version}",
                                                                  $"From {EmailAddress},\nNo Gold.\nShare = " + IsShareProblem);
                                    mailMessage.Attachments.Add(new System.Net.Mail.Attachment(filename));
                                    smtpClient.Send(mailMessage);

                                    if (IsNoGoldSendEmail)
                                    {
                                        MonitoringLog("Sending Email...");

                                        string message;
                                        if (IsKorean)
                                        {
                                            message = "골드 벌이가 끝났습니다.";
                                        }
                                        else
                                        {
                                            message = "No Gold.";
                                        }

                                        mailMessage = new MailMessage("artofwarhassan@gmail.com",
                                                                      EmailAddress,
                                                                      $"Art of Hassan {Version}",
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
                        }

                        if (IsStopHassan)
                        {
                            MonitoringLog("Stop Hassan...");
                        }

                        if (IsNoGoldShutdownPC)
                        {
                            MonitoringLog("Shutting Down PC...");

                            System.Windows.Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
                            {
                                if (IsKorean)
                                {
                                    StartButton.Content = "시작";
                                }
                                else
                                {
                                    StartButton.Content = "Start";
                                }
                                MonitoringIntervalTextBox.IsEnabled = true;
                                ModeGrid.IsEnabled                  = true;

                                ArtOfWarMonitoringTimer.Enabled = false;
                                ProblemMonitoringTimer.Enabled  = false;
                                NoxMonitoringTimer.Enabled      = false;
                            }));

                            System.Diagnostics.Process.Start("shutdown.exe", "-s -f -t 0");

                            return;
                        }
                    }


                    // X3 Gold Button and Next Button
                    if (VictoryFlag)
                    {
                        if (IsWatchAds && !IsNoGoldStatus) // 광고 보기
                        {
                            // 3별 시스템 - 재시도가 있는 것
                            if (MousePointColorCheck(GoldButtonBackgroundX, GoldButtonBackground3StarY, GoldButtonBackgroundGreenColor)) // Green
                            {
                                if (MousePointColorCheck(GoldButtonImageX, GoldButtonImageY, GoldButtonImageColor)) // Yellow Coin
                                {
                                    if (X3GoldButtonClickDelay < MonitoringInterval)
                                    {
                                        System.Threading.Thread.Sleep(X3GoldButtonClickDelay);
                                    }
                                    else
                                    {
                                        X3GoldButtonClickDelay -= MonitoringInterval;
                                        return;
                                    }

                                    MonitoringLog("Gold Button - Star");
                                    AdsWatchFlag = true;
                                    AdsCloseStopwatch.Restart();

                                    MousePointClick(GoldButtonBackgroundX, GoldButtonBackground3StarY);
                                }
                                else // Green but no coin such as Retry
                                {
                                    MonitoringLog("Next Button - Retry of Star");
                                    MousePointClick(NextButtonX, GoldButtonBackground3StarY);
                                }
                            }
                            else if (MousePointColorCheck(GoldButtonBackgroundX, GoldButtonBackground3StarY, GoldButtonBackgroundGrayColor)) // Gray
                            {
                                if (X3GoldButtonClickDelay < MonitoringInterval)
                                {
                                    System.Threading.Thread.Sleep(X3GoldButtonClickDelay);
                                }
                                else
                                {
                                    X3GoldButtonClickDelay -= MonitoringInterval;
                                    return;
                                }

                                MonitoringLog("Next Button - Gold Button is Gray in Star");
                                MousePointClick(NextButtonX, GoldButtonBackground3StarY);
                            }
                            // 비 3별 시스템 - 현상금
                            else if (MousePointColorCheck(GoldButtonBackgroundX, GoldButtonBackgroundNoStarY, GoldButtonBackgroundGreenColor)) // Green
                            {
                                if (X3GoldButtonClickDelay < MonitoringInterval)
                                {
                                    System.Threading.Thread.Sleep(X3GoldButtonClickDelay);
                                }
                                else
                                {
                                    X3GoldButtonClickDelay -= MonitoringInterval;
                                    return;
                                }

                                MonitoringLog("Gold Button - Headhunt");
                                AdsWatchFlag = true;
                                AdsCloseStopwatch.Restart();

                                MousePointClick(GoldButtonBackgroundX, GoldButtonBackgroundNoStarY);
                            }
                            else if (MousePointColorCheck(GoldButtonBackgroundX, GoldButtonBackgroundNoStarY, GoldButtonBackgroundGrayColor)) // Gray
                            {
                                if (X3GoldButtonClickDelay < MonitoringInterval)
                                {
                                    System.Threading.Thread.Sleep(X3GoldButtonClickDelay);
                                }
                                else
                                {
                                    X3GoldButtonClickDelay -= MonitoringInterval;
                                    return;
                                }

                                MonitoringLog("Next Button - Gold Button is Gray in Headhunt");
                                MousePointClick(NextButtonX, GoldButtonBackgroundNoStarY);
                            }
                            // 구버전
                            else if (MousePointColorCheck(GoldButtonBackgroundX, NextButtonY, GoldButtonBackgroundGreenColor)) // Green
                            {
                                if (X3GoldButtonClickDelay < MonitoringInterval)
                                {
                                    System.Threading.Thread.Sleep(X3GoldButtonClickDelay);
                                }
                                else
                                {
                                    X3GoldButtonClickDelay -= MonitoringInterval;
                                    return;
                                }

                                MonitoringLog("Gold Button - v3.0.8");
                                AdsWatchFlag = true;
                                AdsCloseStopwatch.Restart();

                                MousePointClick(GoldButtonBackgroundX, NextButtonY);
                            }
                            else if (MousePointColorCheck(GoldButtonBackgroundX, NextButtonY, GoldButtonBackgroundGrayColor)) // Gray
                            {
                                if (X3GoldButtonClickDelay < MonitoringInterval)
                                {
                                    System.Threading.Thread.Sleep(X3GoldButtonClickDelay);
                                }
                                else
                                {
                                    X3GoldButtonClickDelay -= MonitoringInterval;
                                    return;
                                }

                                MonitoringLog("Next Button - Gold Button is Gray in v3.0.8");
                                MousePointClick(NextButtonX, NextButtonY);
                            }
                        }
                        else if (!IsWatchAds || IsNoGoldStatus) // 광고 안보기 - Next Button
                        {
                            // 3별 시스템
                            if (MousePointColorCheck(NextButtonX, GoldButtonBackground3StarY, NextButtonColor))
                            {
                                MonitoringLog("Next Button - No Ads in Star");
                                MousePointClick(NextButtonX, GoldButtonBackground3StarY);
                            }
                            // 비 3별 시스템
                            else if (MousePointColorCheck(NextButtonX, GoldButtonBackgroundNoStarY, NextButtonColor))
                            {
                                MonitoringLog("Next Button - No Ads in Headhunt");
                                MousePointClick(NextButtonX, GoldButtonBackgroundNoStarY);
                            }
                            // 구버전
                            else if (MousePointColorCheck(NextButtonX, NextButtonY, NextButtonColor))
                            {
                                MonitoringLog("Next Button - No Ads in v3.0.8");
                                MousePointClick(NextButtonX, NextButtonY);
                            }
                        }
                    }
                    else if (DefeatFlag)
                    {
                        // Next Button - Defeat
                        if (MousePointColorCheck(NextButtonX, NextButtonY, NextButtonColor))
                        {
                            MonitoringLog("Next Button - Defeat");
                            MousePointClick(NextButtonX, NextButtonY);
                        }
                    }


                    // GoldAdCloseButton
                    if (MousePointColorCheck(GameAdCloseButtonX, GoldAdCloseButtonY, GameAdCloseButtonLatestColor) ||
                        MousePointColorCheck(GameAdCloseButtonX, GoldAdCloseButtonY, GameAdCloseButtonV308Color))
                    {
                        MonitoringLog("GoldAdCloseButton");
                        MousePointClick(GameAdCloseButtonX, GoldAdCloseButtonY);

                        return;
                    }


                    // TroopAdCloseButton
                    if (MousePointColorCheck(GameAdCloseButtonX, TroopAdCloseButtonY, GameAdCloseButtonLatestColor) ||
                        MousePointColorCheck(GameAdCloseButtonX, TroopAdCloseButtonY, GameAdCloseButtonV308Color))
                    {
                        MonitoringLog("TroopAdCloseButton");
                        MousePointClick(GameAdCloseButtonX, TroopAdCloseButtonY);

                        return;
                    }


                    // MidasAdCloseButton
                    if (MousePointColorCheck(GameAdCloseButtonX, MidasAdCloseButtonY, GameAdCloseButtonLatestColor) ||
                        MousePointColorCheck(GameAdCloseButtonX, MidasAdCloseButtonY, GameAdCloseButtonV308Color))
                    {
                        MonitoringLog("MidasAdCloseButton");
                        MousePointClick(GameAdCloseButtonX, MidasAdCloseButtonY);

                        return;
                    }


                    // Google Ad Close Button
                    if ((TimerCountForScreenCompare % ScreenComparisonInterval) == 0)
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

                        if (!isDifferent || (AdsCloseStopwatch.ElapsedMilliseconds > 34000))
                        {
                            MonitoringLog("GoogleAdCloseButton");
                            AdsCloseStopwatch.Reset();
                            AdsCloseStopwatch.Stop();

                            if (GoogleAdCloseClickPatterns[0] == "L")
                            {
                                MonitoringLog("Left Ad Close Button");
                                MousePointClick(LeftAdCloseButtonX, GoogleAdCloseButtonY);
                            }
                            else
                            {
                                MonitoringLog("Right Ad Close Button");
                                MousePointClick(RightAdCloseButtonX, GoogleAdCloseButtonY);
                            }

                            System.Threading.Thread.Sleep(GoogleAdCloseClickInterval);

                            if (GoogleAdCloseClickPatterns[1] == "L")
                            {
                                MonitoringLog("Left Ad Close Button");
                                MousePointClick(LeftAdCloseButtonX, GoogleAdCloseButtonY);
                            }
                            else
                            {
                                MonitoringLog("Right Ad Close Button");
                                MousePointClick(RightAdCloseButtonX, GoogleAdCloseButtonY);
                            }

                            if (GoogleAdCloseClickPatterns.Length == 3)
                            {
                                System.Threading.Thread.Sleep(GoogleAdCloseClickInterval);

                                if (GoogleAdCloseClickPatterns[2] == "L")
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

                            if (GoogleAdCloseClickPatterns.Length == 4)
                            {
                                System.Threading.Thread.Sleep(GoogleAdCloseClickInterval);

                                if (GoogleAdCloseClickPatterns[3] == "L")
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

                            return;
                        }
                    }


                    //CurrentBitmap.Save(Stage + ".png", System.Drawing.Imaging.ImageFormat.Png);
                    LastBitmap = CurrentBitmap;
                    TimerCountForScreenCompare++;
                }
            }
        }

        #endregion


        #region Private Event Mothod

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            ProblemMonitoringTimer.Enabled  = false;
            ArtOfWarMonitoringTimer.Enabled = false;
            NoxMonitoringTimer.Enabled      = false;
        }

        string PrevMonitoringInterval;

        bool PrevAdsWatch;

        bool IsHonorMode    = false;
        bool IsTroopMode    = false;

        private void StageRadioButton_Click(object sender, RoutedEventArgs e)
        {
            MonitoringLog("Stage Mode");

            if (IsHonorMode || IsTroopMode)
            {
                MonitoringIntervalTextBox.Text = PrevMonitoringInterval;

                X3GoldButtonClickDelayTextBlock.Visibility = Visibility.Visible;
                X3GoldButtonClickDelayTextBox.Visibility   = Visibility.Visible;
                HonorHeroChangeTimeTextBlock.Visibility    = Visibility.Hidden;
                HonorHeroChangeTimeTextBox.Visibility      = Visibility.Hidden;

                AdsTextBlock.Visibility = Visibility.Visible;
                AdsGrid.Visibility      = Visibility.Visible;
                HonorHeroChangeTextBlock.Visibility = Visibility.Hidden;
                HonorHeroChangeCheckBox.Visibility  = Visibility.Hidden;
            }

            // 신버전 3별 시스템이 기준
            IsHonorMode    = false;
            IsTroopMode    = false;
            AdsWatchCheckBox.IsChecked = true;
        }

        private void HeadhuntRadioButton_Click(object sender, RoutedEventArgs e)
        {
            MonitoringLog("Headhunt Mode");

            PrevAdsWatch = AdsWatchCheckBox.IsChecked.Value;

            if (IsHonorMode || IsTroopMode)
            {
                MonitoringIntervalTextBox.Text = PrevMonitoringInterval;

                X3GoldButtonClickDelayTextBlock.Visibility = Visibility.Visible;
                X3GoldButtonClickDelayTextBox.Visibility   = Visibility.Visible;
                HonorHeroChangeTimeTextBlock.Visibility    = Visibility.Hidden;
                HonorHeroChangeTimeTextBox.Visibility      = Visibility.Hidden;

                AdsTextBlock.Visibility = Visibility.Visible;
                AdsGrid.Visibility      = Visibility.Visible;
                HonorHeroChangeTextBlock.Visibility = Visibility.Hidden;
                HonorHeroChangeCheckBox.Visibility  = Visibility.Hidden;
            }

            // 현상금은 비 3별 시스템
            IsHonorMode    = false;
            IsTroopMode    = false;
            AdsWatchCheckBox.IsChecked = false;
        }

        private void HonorRadioButton_Click(object sender, RoutedEventArgs e)
        {
            MonitoringLog("Honor Hunting Mode");

            if (!IsTroopMode)
            {
                PrevMonitoringInterval = MonitoringIntervalTextBox.Text;
            }

            IsHonorMode    = true;
            IsTroopMode    = false;

            MonitoringIntervalTextBox.Text = "200";

            X3GoldButtonClickDelayTextBlock.Visibility = Visibility.Hidden;
            X3GoldButtonClickDelayTextBox.Visibility   = Visibility.Hidden;
            HonorHeroChangeTimeTextBlock.Visibility    = Visibility.Visible;
            HonorHeroChangeTimeTextBox.Visibility      = Visibility.Visible;

            AdsTextBlock.Visibility = Visibility.Hidden;
            AdsGrid.Visibility      = Visibility.Hidden;
            HonorHeroChangeTextBlock.Visibility = Visibility.Visible;
            HonorHeroChangeCheckBox.Visibility  = Visibility.Visible;
        }

        private void TroopRadioButton_Click(object sender, RoutedEventArgs e)
        {
            MonitoringLog("Troop Mode");

            if (IsHonorMode)
            {
                X3GoldButtonClickDelayTextBlock.Visibility = Visibility.Visible;
                X3GoldButtonClickDelayTextBox.Visibility   = Visibility.Visible;
                HonorHeroChangeTimeTextBlock.Visibility    = Visibility.Hidden;
                HonorHeroChangeTimeTextBox.Visibility      = Visibility.Hidden;

                AdsTextBlock.Visibility = Visibility.Visible;
                AdsGrid.Visibility      = Visibility.Visible;
                HonorHeroChangeTextBlock.Visibility = Visibility.Hidden;
                HonorHeroChangeCheckBox.Visibility  = Visibility.Hidden;
            }
            else
            {
                PrevMonitoringInterval = MonitoringIntervalTextBox.Text;
            }

            IsHonorMode    = false;
            IsTroopMode    = true;

            MonitoringIntervalTextBox.Text = "300";
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

                MonitoringIntervalTextBox.IsEnabled = false;
                ModeGrid.IsEnabled                  = false;

                IsStopHassan     = false;
                IsNoGoldStatus   = false;
                IsNoGoldMailSent = false;
                ProblemMailSent  = 0;

                IsHonorFightButtonClicked  = false;
                IsGetHonorSkillButtonColor = false;
                IsHonorPauseButtonClicked  = false;

                HonorSkillChangeCount = 0;
                HonorHeroWindowCount  = 0;

                GoldChestBoxStopwatch.Restart();

                if (!IsHonorMode && !IsTroopMode && (int.Parse(MonitoringIntervalTextBox.Text) < 1000))
                {
                    MonitoringIntervalTextBox.Text = "1000";
                    MonitoringInterval             = 1000;
                }
                ArtOfWarMonitoringTimer.Interval = int.Parse(MonitoringIntervalTextBox.Text);
                ProblemMonitoringTimer.Interval  = int.Parse(ScreenComparisonIntervalTextBox.Text) * 60 * 1000;
                ArtOfWarMonitoringTimer.Enabled  = true;
                ProblemMonitoringTimer.Enabled   = true;
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
                MonitoringIntervalTextBox.IsEnabled = true;
                ModeGrid.IsEnabled                  = true;

                GoldChestBoxStopwatch.Reset();
                GoldChestBoxStopwatch.Stop();

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
                                                              $"Art of Hassan {Version}",
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

        private void LoadPixelPositionColorButton_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog();
            openFileDialog.DefaultExt = "csv";
            openFileDialog.Filter = "CSV Files (*.csv)|*.csv";
            openFileDialog.ShowDialog();
            if (openFileDialog.FileName.Length > 0)
            {
                LoadPositionCsv(openFileDialog.FileName);
                LoadColorCsv(openFileDialog.FileName);

                SavePositionCsv();
                SaveColorCsv();
            }
        }

        private void SaveSettingButton_Click(object sender, RoutedEventArgs e)
        {
            using (StreamWriter streamWriter = new StreamWriter($@"setting.csv", false))
            {
                streamWriter.WriteLine("AppPlayerTitle," + AppPlayerTitleTextBox.Text);
                streamWriter.WriteLine("MonitoringInterval," + MonitoringIntervalTextBox.Text);
                streamWriter.WriteLine("ScreenComparisonInterval," + ScreenComparisonIntervalTextBox.Text);
                streamWriter.WriteLine("X3GoldButtonDelay," + X3GoldButtonClickDelayTextBox.Text);
                streamWriter.WriteLine("PixelDifference," + PixelDifferenceTextBox.Text);
                streamWriter.WriteLine("Korean," + KoreanCheckBox.IsChecked.Value);
                streamWriter.WriteLine("AdsCloseClickPattern," + GoogleAdCloseClickPattern);
                streamWriter.WriteLine("GoldChestCheck," + GoldChestCheckBox.IsChecked.Value);
                streamWriter.WriteLine("Pausability," + PausabilityCheckBox.IsChecked.Value);
                streamWriter.WriteLine("Logging," + LogCheckBox.IsChecked.Value);
                streamWriter.WriteLine("Email," + EmailAddressTextBox.Text);
                streamWriter.WriteLine("SendEmail," + SendEmailCheckBox.IsChecked.Value);
                streamWriter.WriteLine("StopHassan," + StopHassanCheckBox.IsChecked.Value);
                streamWriter.WriteLine("ShutdownPC," + ShutdownComputerCheckBox.IsChecked.Value);
            }
        }

        private void KoreanCheckBox_Click(object sender, RoutedEventArgs e)
        {
            if (KoreanCheckBox.IsChecked.Value)
            {
                this.Title = $"아트 오브 핫산 {Version}";
                AppPlayerTitleTextBlock.Text = "앱플레이어\n    이름";
                MonitoringIntervalTextBlock.Text = "모니터링\n주기 (ms)";
                ScreenComparisonIntervalTextBlock.Text = " 화면 비교\n주기 (횟수)";
                X3GoldButtonClickDelayTextBlock.Text = " 골드 광고\n딜레이 (ms)";
                HonorHeroChangeTimeTextBlock.Text = "영웅 선택\n시간 (ms)";
                PixelDifferenceTextBlock.Text = "픽셀 차이";
                HonorHeroChangeTextBlock.Text = "영웅 변경";
                HonorHeroChangeCheckBox.Content = "영웅 선택창 열기";
                AdsTextBlock.Text = "광고";
                AdsWatchCheckBox.Content = "광고 보기";
                AdsCloseClickPatternButton.Content = "광고 닫기\n클릭 패턴";
                ModeTextBlock.Text = "모드";
                StageRadioButton.Content = "스테이지";
                StageColumn.Width = new GridLength(1.4, GridUnitType.Star);
                HeadhuntRadioButton.Content = "현상금";
                HeadhuntColumn.Width = new GridLength(1.25, GridUnitType.Star);
                HonorRadioButton.Content = "영광";
                TroopRadioButton.Content = "용병";
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
                LoadPixelPositionColorButton.Content = " 픽셀 위치 및\n색상 불러오기";
                SaveSettingButton.Content = "설정 저장";
                StartButton.Content = "시작";
                MessageBar.Text = $"전투: {NumOfWar}  |  승리: {NumOfVictory}  |  패배: {NumOfDefeat}  |  광고: {NumOfAds}";
                ShareProblemCheckBox.Content = "우리 핫산 개선을 위해 문제 발생 스샷 공유 :)";
            }
            else
            {
                this.Title = $"Art of Hassan {Version}";
                AppPlayerTitleTextBlock.Text = "AppPlayer Title";
                MonitoringIntervalTextBlock.Text = "Monitoring\nInterval (ms)";
                ScreenComparisonIntervalTextBlock.Text = "   Screen\nComparison\n Interval (#)";
                X3GoldButtonClickDelayTextBlock.Text = " X3 Gold\n  Button\nDelay (ms)";
                HonorHeroChangeTimeTextBlock.Text = "   Hero\n Change\nTime (ms)";
                PixelDifferenceTextBlock.Text = "   Pixel\nDifference";
                HonorHeroChangeTextBlock.Text = "  Hero\nChange";
                HonorHeroChangeCheckBox.Content = "Open Heros Window";
                AdsTextBlock.Text = "Ads";
                AdsWatchCheckBox.Content = "Watch Ads ";
                AdsCloseClickPatternButton.Content = "  Ads Close\nClick Pattern";
                ModeTextBlock.Text = "Mode";
                StageRadioButton.Content = "Stage";
                StageColumn.Width = new GridLength(1, GridUnitType.Star);
                HeadhuntRadioButton.Content = "Headhunt";
                HeadhuntColumn.Width = new GridLength(1.35, GridUnitType.Star);
                HonorRadioButton.Content = "Honor";
                TroopRadioButton.Content = "Troop";
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
                LoadPixelPositionColorButton.Content = "    Load Pixel\nPosition or Color";
                SaveSettingButton.Content = "Save Setting";
                StartButton.Content = "Start";
                MessageBar.Text = $"War: {NumOfWar}  |  Victory: {NumOfVictory}  |  Defeat: {NumOfDefeat}  |  Ads: {NumOfAds}";
                ShareProblemCheckBox.Content = "Share screenshot of problem to improve our Hassan :)";
            }
        }

        #endregion


        #region Private Method

        private string InsertSpaceBeforeUpper(string input)
        {
            string output = input[0].ToString();
            for (int i = 1; i < input.Length - 1; i++)
            {
                if (char.IsUpper(input[i]) &&
                   (input[i - 1] != ' ') &&
                   (char.IsLower(input[i - 1]) ||
                    char.IsLower(input[i + 1])))
                {
                    output += " " + input[i].ToString();
                }
                else
                {
                    output += input[i].ToString();
                }
            }
            output += input[input.Length - 1].ToString();
            return output;
        }

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
                    streamWriter.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff ") + InsertSpaceBeforeUpper(log));
                }
            }
        }

        private IntPtr GetWinAscHandle()
        {
            string appPlayerTitle = "NoxPlayer";
            System.Windows.Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
            {
                appPlayerTitle = AppPlayerTitleTextBox.Text;
            }));
            return FindWindow(null, appPlayerTitle);
        }

        private void GetWindowPos(IntPtr hwnd, ref System.Windows.Point point, ref System.Windows.Size size)
        {
            WINDOWPLACEMENT placement = new WINDOWPLACEMENT();
            placement.length = System.Runtime.InteropServices.Marshal.SizeOf(placement);

            GetWindowPlacement(hwnd, ref placement);

            size  = new System.Windows.Size(placement.normal_position.Right - (placement.normal_position.Left * 2), placement.normal_position.Bottom - (placement.normal_position.Top * 2));
            point = new System.Windows.Point(placement.normal_position.Left, placement.normal_position.Top);
        }

        private void MousePointClick(int PositionX, int PositionY)
        {
            System.Windows.Forms.Cursor.Position = new System.Drawing.Point((int)NoxPointX + PositionX, (int)NoxPointY + PositionY);
            mouse_event(LBUTTONDOWN, 0, 0, 0, 0);
            System.Threading.Thread.Sleep(50);
            mouse_event(LBUTTONUP, 0, 0, 0, 0);
        }

        private bool MousePointColorCheck(int PositionX, int PositionY, string Color)
        {
            System.Drawing.Color TargetColor  = ColorTranslator.FromHtml(Color);
            System.Drawing.Color CurrentColor = CurrentBitmap.GetPixel(PositionX, PositionY);

            return (((CurrentColor.R >= TargetColor.R - PixelDifference) && (CurrentColor.G >= TargetColor.G - PixelDifference) && (CurrentColor.B >= TargetColor.B - PixelDifference)) &&
                    ((CurrentColor.R <= TargetColor.R + PixelDifference) && (CurrentColor.G <= TargetColor.G + PixelDifference) && (CurrentColor.B <= TargetColor.B + PixelDifference)));
        }

        private void TextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            int caretIndex = (e.Source as TextBox).CaretIndex;
            string name    = (e.Source as TextBox).Name;
            if (name.Contains("AppPlayerTitle"))
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

        private void SavePositionCsv()
        {
            using (StreamWriter streamWriter = new StreamWriter($@"position.csv", false))
            {
                streamWriter.WriteLine("AppLocation," + AppLocationX + "," + AppLocationY);
                streamWriter.WriteLine("HomeButton," + HomeButtonX);
                streamWriter.WriteLine("ShopButton," + ShopButtonX + "," + ShopButtonY);
                streamWriter.WriteLine("GoldChestBox," + GoldChestBoxX + "," + GoldChestBoxY);
                streamWriter.WriteLine("GoldChestBoxCoinLatest," + GoldChestBoxCoinLatestX + "," + GoldChestBoxCoinLatestY);
                streamWriter.WriteLine("GoldChestBoxCoinV308," + GoldChestBoxCoinV308X + "," + GoldChestBoxCoinV308Y);
                streamWriter.WriteLine("CollectButton," + CollectButtonX + "," + CollectButtonY);
                streamWriter.WriteLine("BattleLevelButton," + BattleLevelButtonX + "," + BattleLevelButtonY);
                streamWriter.WriteLine("SkillButton," + SkillButtonX + "," + SkillButtonY);
                streamWriter.WriteLine("SpeedButton," + SpeedButtonX + "," + SpeedButtonY);
                streamWriter.WriteLine("ContinueButton," + ContinueButtonX + "," + ContinueButtonY);
                streamWriter.WriteLine("VictoryDefeat," + VictoryDefeatX + "," + VictoryDefeatY);
                streamWriter.WriteLine("NoGold," + NoGoldX + "," + NoGoldY);
                streamWriter.WriteLine("GoldButtonBackground," + GoldButtonBackgroundX + "," + GoldButtonBackground3StarY + "," + GoldButtonBackgroundNoStarY);
                streamWriter.WriteLine("GoldButtonImage," + GoldButtonImageX + "," + GoldButtonImageY);
                streamWriter.WriteLine("NextButton," + NextButtonX + "," + NextButtonY);
                streamWriter.WriteLine("GameAdCloseButton," + GameAdCloseButtonX + "," + GoldAdCloseButtonY + "," + TroopAdCloseButtonY + "," + MidasAdCloseButtonY);
                streamWriter.WriteLine("GoogleAdCloseButton," + LeftAdCloseButtonX + "," + RightAdCloseButtonX + "," + GoogleAdCloseButtonY);
                streamWriter.WriteLine("LatestUsedAppButton," + LatestUsedAppButtonX + "," + LatestUsedAppButtonY);
                streamWriter.WriteLine("RightTopAppCloseButton," + RightTopAppCloseButtonX + "," + RightTopAppCloseButtonY);
                streamWriter.WriteLine("NotRespondAppCloseButton," + NotRespondAppCloseButtonX + "," + NotRespondAppCloseButtonY1 + "," + NotRespondAppCloseButtonY2 + "," + NotRespondAppCloseButtonY3);
                streamWriter.WriteLine("HeadhuntButton," + HeadhuntButtonX + "," + HeadhuntButtonY);
                streamWriter.WriteLine("TroopButton," + TroopButtonX + "," + TroopButtonY);
                streamWriter.WriteLine("TroopOpenButton," + TroopOpenButtonX + "," + TroopOpenButtonY);
                streamWriter.WriteLine("TroopCloseButton," + TroopCloseButtonX + "," + TroopCloseButtonY);
                streamWriter.WriteLine("HonorChallengeButton," + HonorChallengeButtonX + "," + HonorChallengeButtonY);
                streamWriter.WriteLine("HonorFightButton," + HonorFightButtonX + "," + HonorFightButtonY);
                streamWriter.WriteLine("HonorSkillButton," + HonorSkillButtonX + "," + HonorSkillButtonY);
                streamWriter.WriteLine("HonorPauseButton," + HonorPauseButtonX + "," + HonorPauseButtonY);
                streamWriter.WriteLine("HonorQuitButton," + HonorQuitButtonX + "," + HonorQuitButtonY);
                streamWriter.WriteLine("HonorHeroPosition," + HonorHeroPositionX + "," + HonorHeroPositionY);
                streamWriter.WriteLine("HonorReplaceButton," + HonorReplaceButtonX + "," + HonorReplaceButtonY);
                streamWriter.WriteLine("HonorHeroWindow," + HonorHeroWindowX + "," + HonorHeroWindowY);
                streamWriter.WriteLine("HonorHeroWindowCloseButton," + HonorHeroWindowCloseButtonX + "," + HonorHeroWindowCloseButtonY);
            }
        }

        private void SaveColorCsv()
        {
            using (StreamWriter streamWriter = new StreamWriter($@"color.csv", false))
            {
                streamWriter.WriteLine("AppLocationOrangeBlueColor," + AppLocationOrangeBlueColor);
                streamWriter.WriteLine("AppLocationGreenColor," + AppLocationGreenColor);
                streamWriter.WriteLine("ShopButtonColor," + ShopButtonColor);
                streamWriter.WriteLine("GoldChestBoxCoinLatestColor," + GoldChestBoxCoinLatestColor);
                streamWriter.WriteLine("GoldChestBoxCoinV308Color," + GoldChestBoxCoinV308Color);
                streamWriter.WriteLine("CollectButtonColor," + CollectButtonColor);
                streamWriter.WriteLine("BattleLevelButtonYellowColor," + BattleLevelButtonYellowColor);
                streamWriter.WriteLine("BattleLevelButtonShadedColor," + BattleLevelButtonShadedColor);
                streamWriter.WriteLine("BattleLevelButtonRedColor," + BattleLevelButtonRedColor);
                streamWriter.WriteLine("BattleLevelButtonBackColor," + BattleLevelButtonBackColor);
                streamWriter.WriteLine("SkillButtonColor," + SkillButtonColor);
                streamWriter.WriteLine("SpeedButtonColor," + SpeedButtonColor);
                streamWriter.WriteLine("ContinueButtonColor," + ContinueButtonColor);
                streamWriter.WriteLine("VictoryDefeatVictoryColor," + VictoryDefeatVictoryColor);
                streamWriter.WriteLine("VictoryDefeatDefeatColor," + VictoryDefeatDefeatColor);
                streamWriter.WriteLine("NoGoldColor," + NoGoldColor);
                streamWriter.WriteLine("GoldButtonBackgroundGreenColor," + GoldButtonBackgroundGreenColor);
                streamWriter.WriteLine("GoldButtonBackgroundGrayColor," + GoldButtonBackgroundGrayColor);
                streamWriter.WriteLine("GoldButtonImageColor," + GoldButtonImageColor);
                streamWriter.WriteLine("NextButtonColor," + NextButtonColor);
                streamWriter.WriteLine("GameAdCloseButtonLatestColor," + GameAdCloseButtonLatestColor);
                streamWriter.WriteLine("GameAdCloseButtonV308Color," + GameAdCloseButtonV308Color);
                streamWriter.WriteLine("NotRespondAppCloseButtonColor," + NotRespondAppCloseButtonColor);
                streamWriter.WriteLine("HeadhuntButtonYellowColor," + HeadhuntButtonYellowColor);
                streamWriter.WriteLine("HeadhuntButtonGrayColor," + HeadhuntButtonGrayColor);
                streamWriter.WriteLine("TroopButtonColor," + TroopButtonColor);
                streamWriter.WriteLine("TroopOpenButtonCenterColor," + TroopOpenButtonCenterColor);
                streamWriter.WriteLine("TroopOpenButtonRightColor," + TroopOpenButtonRightColor);
                streamWriter.WriteLine("TroopCloseButtonColor," + TroopCloseButtonColor);
                streamWriter.WriteLine("HonorChallengeButtonColor," + HonorChallengeButtonColor);
                streamWriter.WriteLine("HonorFightButtonColor," + HonorFightButtonColor);
                streamWriter.WriteLine("HonorQuitButtonColor," + HonorQuitButtonColor);
                streamWriter.WriteLine("HonorReplaceButtonColor," + HonorReplaceButtonColor);
                streamWriter.WriteLine("HonorHeroWindowColor," + HonorHeroWindowColor);
            }
        }

        private void LoadSettingCsv()
        {
            FileInfo fileInfo = new FileInfo("setting.csv");
            if (fileInfo.Exists)
            {
                MonitoringLog("Load Setting...");

                string[] lines = File.ReadAllLines("setting.csv");

                foreach (string line in lines)
                {
                    string[] listitem = line.Split(',');

                    switch (listitem[0].ToLower())
                    {
                        case ("appplayertitle"):
                            AppPlayerTitleTextBox.Text = listitem[1];
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
                            KoreanCheckBox.IsChecked  =  bool.Parse(listitem[1]);
                            break;
                        case ("adscloseclickpattern"):
                            GoogleAdCloseClickPattern = listitem[1];
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

        private void LoadPositionCsv(string filename)
        {
            FileInfo fileInfo = new FileInfo(filename);
            if (fileInfo.Exists)
            {
                MonitoringLog("Load Pixel Position...");

                string[] lines = File.ReadAllLines(filename);

                foreach (string line in lines)
                {
                    bool   isCanParse = true;
                    string[] listitem = line.Split(',');

                    int temp;
                    switch (listitem.Length)
                    {
                        case 2:
                            if (!int.TryParse(listitem[1], out temp)) isCanParse = false;
                            if (isCanParse)
                            {
                                switch (listitem[0].ToLower())
                                {
                                    case ("homebutton"):
                                        HomeButtonX = int.Parse(listitem[1]);
                                        break;
                                }
                            }
                            break;
                        case 3:
                            if (!int.TryParse(listitem[1], out temp)) isCanParse = false;
                            if (!int.TryParse(listitem[2], out temp)) isCanParse = false;
                            if (isCanParse)
                            {
                                switch (listitem[0].ToLower())
                                {
                                    case ("applocation"):
                                        AppLocationX = int.Parse(listitem[1]);
                                        AppLocationY = int.Parse(listitem[2]);
                                        break;
                                    case ("shopbutton"):
                                        ShopButtonX = int.Parse(listitem[1]);
                                        ShopButtonY = int.Parse(listitem[2]);
                                        break;
                                    case ("goldchestbox"):
                                        GoldChestBoxX = int.Parse(listitem[1]);
                                        GoldChestBoxY = int.Parse(listitem[2]);
                                        break;
                                    case ("goldchestboxcoinlatest"):
                                        GoldChestBoxCoinLatestX = int.Parse(listitem[1]);
                                        GoldChestBoxCoinLatestY = int.Parse(listitem[2]);
                                        break;
                                    case ("goldchestboxcoinv308"):
                                        GoldChestBoxCoinV308X = int.Parse(listitem[1]);
                                        GoldChestBoxCoinV308Y = int.Parse(listitem[2]);
                                        break;
                                    case ("collectbutton"):
                                        CollectButtonX = int.Parse(listitem[1]);
                                        CollectButtonY = int.Parse(listitem[2]);
                                        break;
                                    case ("battlelevelbutton"):
                                        BattleLevelButtonX = int.Parse(listitem[1]);
                                        BattleLevelButtonY = int.Parse(listitem[2]);
                                        break;
                                    case ("skillbutton"):
                                        SkillButtonX = int.Parse(listitem[1]);
                                        SkillButtonY = int.Parse(listitem[2]);
                                        break;
                                    case ("speedbutton"):
                                        SpeedButtonX = int.Parse(listitem[1]);
                                        SpeedButtonY = int.Parse(listitem[2]);
                                        break;
                                    case ("continuebutton"):
                                        ContinueButtonX = int.Parse(listitem[1]);
                                        ContinueButtonY = int.Parse(listitem[2]);
                                        break;
                                    case ("victorydefeat"):
                                        VictoryDefeatX = int.Parse(listitem[1]);
                                        VictoryDefeatY = int.Parse(listitem[2]);
                                        break;
                                    case ("nogold"):
                                        NoGoldX = int.Parse(listitem[1]);
                                        NoGoldY = int.Parse(listitem[2]);
                                        break;
                                    case ("goldbuttonimage"):
                                        GoldButtonImageX = int.Parse(listitem[1]);
                                        GoldButtonImageY = int.Parse(listitem[2]);
                                        break;
                                    case ("nextbutton"):
                                        NextButtonX = int.Parse(listitem[1]);
                                        NextButtonY = int.Parse(listitem[2]);
                                        break;
                                    case ("latestusedappbutton"):
                                        LatestUsedAppButtonX = int.Parse(listitem[1]);
                                        LatestUsedAppButtonY = int.Parse(listitem[2]);
                                        break;
                                    case ("righttopappclosebutton"):
                                        RightTopAppCloseButtonX = int.Parse(listitem[1]);
                                        RightTopAppCloseButtonY = int.Parse(listitem[2]);
                                        break;
                                    case ("headhuntbutton"):
                                        HeadhuntButtonX = int.Parse(listitem[1]);
                                        HeadhuntButtonY = int.Parse(listitem[2]);
                                        break;
                                    case ("troopbutton"):
                                        TroopButtonX = int.Parse(listitem[1]);
                                        TroopButtonY = int.Parse(listitem[2]);
                                        break;
                                    case ("troopopenbutton"):
                                        TroopOpenButtonX = int.Parse(listitem[1]);
                                        TroopOpenButtonY = int.Parse(listitem[2]);
                                        break;
                                    case ("troopclosebutton"):
                                        TroopCloseButtonX = int.Parse(listitem[1]);
                                        TroopCloseButtonY = int.Parse(listitem[2]);
                                        break;
                                    case ("honorchallengebutton"):
                                        HonorChallengeButtonX = int.Parse(listitem[1]);
                                        HonorChallengeButtonY = int.Parse(listitem[2]);
                                        break;
                                    case ("honorfightbutton"):
                                        HonorFightButtonX = int.Parse(listitem[1]);
                                        HonorFightButtonY = int.Parse(listitem[2]);
                                        break;
                                    case ("honorskillbutton"):
                                        HonorSkillButtonX = int.Parse(listitem[1]);
                                        HonorSkillButtonY = int.Parse(listitem[2]);
                                        break;
                                    case ("honorpausebutton"):
                                        HonorPauseButtonX = int.Parse(listitem[1]);
                                        HonorPauseButtonY = int.Parse(listitem[2]);
                                        break;
                                    case ("honorquitbutton"):
                                        HonorQuitButtonX = int.Parse(listitem[1]);
                                        HonorQuitButtonY = int.Parse(listitem[2]);
                                        break;
                                    case ("honorheroposition"):
                                        HonorHeroPositionX = int.Parse(listitem[1]);
                                        HonorHeroPositionY = int.Parse(listitem[2]);
                                        break;
                                    case ("honorreplacebutton"):
                                        HonorReplaceButtonX = int.Parse(listitem[1]);
                                        HonorReplaceButtonY = int.Parse(listitem[2]);
                                        break;
                                    case ("honorherowindow"):
                                        HonorHeroWindowX = int.Parse(listitem[1]);
                                        HonorHeroWindowY = int.Parse(listitem[2]);
                                        break;
                                    case ("honorherowindowclosebutton"):
                                        HonorHeroWindowCloseButtonX = int.Parse(listitem[1]);
                                        HonorHeroWindowCloseButtonY = int.Parse(listitem[2]);
                                        break;
                                }
                            }
                            break;
                        case 4:
                            if (!int.TryParse(listitem[1], out temp)) isCanParse = false;
                            if (!int.TryParse(listitem[2], out temp)) isCanParse = false;
                            if (!int.TryParse(listitem[3], out temp)) isCanParse = false;
                            if (isCanParse)
                            {
                                switch (listitem[0].ToLower())
                                {
                                    case ("goldbuttonbackground"):
                                        GoldButtonBackgroundX = int.Parse(listitem[1]);
                                        GoldButtonBackground3StarY = int.Parse(listitem[2]);
                                        GoldButtonBackgroundNoStarY = int.Parse(listitem[3]);
                                        break;
                                    case ("googleadclosebutton"):
                                        LeftAdCloseButtonX = int.Parse(listitem[1]);
                                        RightAdCloseButtonX = int.Parse(listitem[2]);
                                        GoogleAdCloseButtonY = int.Parse(listitem[3]);
                                        break;
                                }
                            }
                            break;
                        case 5:
                            if (!int.TryParse(listitem[1], out temp)) isCanParse = false;
                            if (!int.TryParse(listitem[2], out temp)) isCanParse = false;
                            if (!int.TryParse(listitem[3], out temp)) isCanParse = false;
                            if (!int.TryParse(listitem[4], out temp)) isCanParse = false;
                            if (isCanParse)
                            {
                                switch (listitem[0].ToLower())
                                {
                                    case ("gameadclosebutton"):
                                        GameAdCloseButtonX = int.Parse(listitem[1]);
                                        GoldAdCloseButtonY = int.Parse(listitem[2]);
                                        TroopAdCloseButtonY = int.Parse(listitem[3]);
                                        MidasAdCloseButtonY = int.Parse(listitem[4]);
                                        break;
                                    case ("notrespondappclosebutton"):
                                        NotRespondAppCloseButtonX = int.Parse(listitem[1]);
                                        NotRespondAppCloseButtonY1 = int.Parse(listitem[2]);
                                        NotRespondAppCloseButtonY2 = int.Parse(listitem[3]);
                                        NotRespondAppCloseButtonY3 = int.Parse(listitem[4]);
                                        break;
                                }
                            }
                            break;
                    }
                }

                MonitoringLog("Load Pixel Position Done");
            }
        }

        private void LoadColorCsv(string filename)
        {
            FileInfo fileInfo = new FileInfo(filename);
            if (fileInfo.Exists)
            {
                MonitoringLog("Load Pixel Color...");

                string[] lines = File.ReadAllLines(filename);

                foreach (string line in lines)
                {
                    string[] listitem = line.Split(',');

                    if ((listitem.Length == 2) && listitem[1].Contains("#") && (listitem[1].Length == 7))
                    {
                        switch (listitem[0].ToLower())
                        {
                            case ("applocationorangebluecolor"):
                                AppLocationOrangeBlueColor = listitem[1];
                                break;
                            case ("applocationgreencolor"):
                                AppLocationGreenColor = listitem[1];
                                break;
                            case ("shopbuttoncolor"):
                                ShopButtonColor = listitem[1];
                                break;
                            case ("goldchestboxcoinlatestcolor"):
                                GoldChestBoxCoinLatestColor = listitem[1];
                                break;
                            case ("goldchestboxcoinv308color"):
                                GoldChestBoxCoinV308Color = listitem[1];
                                break;
                            case ("collectbuttoncolor"):
                                CollectButtonColor = listitem[1];
                                break;
                            case ("battlelevelbuttonyellowcolor"):
                                BattleLevelButtonYellowColor = listitem[1];
                                break;
                            case ("battlelevelbuttonshadedcolor"):
                                BattleLevelButtonShadedColor = listitem[1];
                                break;
                            case ("battlelevelbuttonredcolor"):
                                BattleLevelButtonRedColor = listitem[1];
                                break;
                            case ("battlelevelbuttonbackcolor"):
                                BattleLevelButtonBackColor = listitem[1];
                                break;
                            case ("skillbuttoncolor"):
                                SkillButtonColor = listitem[1];
                                break;
                            case ("speedbuttoncolor"):
                                SpeedButtonColor = listitem[1];
                                break;
                            case ("continuebuttoncolor"):
                                ContinueButtonColor = listitem[1];
                                break;
                            case ("victorydefeatvictorycolor"):
                                VictoryDefeatVictoryColor = listitem[1];
                                break;
                            case ("victorydefeatdefeatcolor"):
                                VictoryDefeatDefeatColor = listitem[1];
                                break;
                            case ("nogoldcolor"):
                                NoGoldColor = listitem[1];
                                break;
                            case ("goldbuttonbackgroundgreencolor"):
                                GoldButtonBackgroundGreenColor = listitem[1];
                                break;
                            case ("goldbuttonbackgroundgraycolor"):
                                GoldButtonBackgroundGrayColor = listitem[1];
                                break;
                            case ("goldbuttonimagecolor"):
                                GoldButtonImageColor = listitem[1];
                                break;
                            case ("nextbuttoncolor"):
                                NextButtonColor = listitem[1];
                                break;
                            case ("gameadclosebuttonlatestcolor"):
                                GameAdCloseButtonLatestColor = listitem[1];
                                break;
                            case ("gameadclosebuttonv308color"):
                                GameAdCloseButtonV308Color = listitem[1];
                                break;
                            case ("notrespondappclosebuttoncolor"):
                                NotRespondAppCloseButtonColor = listitem[1];
                                break;
                            case ("headhuntbuttonyellowcolor"):
                                HeadhuntButtonYellowColor = listitem[1];
                                break;
                            case ("headhuntbuttongraycolor"):
                                HeadhuntButtonGrayColor = listitem[1];
                                break;
                            case ("troopbuttoncolor"):
                                TroopButtonColor = listitem[1];
                                break;
                            case ("troopopenbuttoncentercolor"):
                                TroopOpenButtonCenterColor = listitem[1];
                                break;
                            case ("troopopenbuttonrightcolor"):
                                TroopOpenButtonRightColor = listitem[1];
                                break;
                            case ("troopclosebuttoncolor"):
                                TroopCloseButtonColor = listitem[1];
                                break;
                            case ("honorchallengebuttoncolor"):
                                HonorChallengeButtonColor = listitem[1];
                                break;
                            case ("honorfightbuttoncolor"):
                                HonorFightButtonColor = listitem[1];
                                break;
                            case ("honorquitbuttoncolor"):
                                HonorQuitButtonColor = listitem[1];
                                break;
                            case ("honorreplacebuttoncolor"):
                                HonorReplaceButtonColor = listitem[1];
                                break;
                            case ("honorherowindowcolor"):
                                HonorHeroWindowColor = listitem[1];
                                break;
                        }
                    }
                }

                MonitoringLog("Load Pixel Color Done");
            }
        }

        #endregion
    }
}
