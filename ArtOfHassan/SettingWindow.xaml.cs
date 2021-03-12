using System;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;

namespace ArtOfHassan
{
    /// <summary>
    /// SettingWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class SettingWindow : Window
    {
        public SettingWindow()
        {
            InitializeComponent();
        }

        private void AdsCloseClickPatternButton_Click(object sender, RoutedEventArgs e)
        {
            ClickPatternWindow clickPatternWindow = new ClickPatternWindow();
            clickPatternWindow.ShowDialog();
        }

        private void EmailTestButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(EmailAddressTextBox.Text))
            {
                if (((MainWindow)System.Windows.Application.Current.MainWindow).KoreanCheckBox.IsChecked.Value)
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
                //MonitoringLog("Email Testing...");

                try
                {
                    string message;
                    if (((MainWindow)System.Windows.Application.Current.MainWindow).KoreanCheckBox.IsChecked.Value)
                    {
                        message = "이메일 송신 테스트...";
                    }
                    else
                    {
                        message = "Email Testing...";
                    }

                    MailMessage mailMessage = new MailMessage("artofwarhassan@gmail.com",
                                                              EmailAddressTextBox.Text,
                                                              $"Art of Hassan",
                                                              message);

                    SmtpClient smtpClient = new SmtpClient("smtp.gmail.com", 587);
                    smtpClient.UseDefaultCredentials = false;
                    smtpClient.EnableSsl = true;
                    smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
                    smtpClient.Credentials = new NetworkCredential("artofwarhassan@gmail.com", "Rnrmf0575!");
                    smtpClient.Send(mailMessage);
                }
                catch (Exception ex)
                {
                    if (((MainWindow)System.Windows.Application.Current.MainWindow).KoreanCheckBox.IsChecked.Value)
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

        private void DefaultButton_Click(object sender, RoutedEventArgs e)
        {
            ScreenMonitoringIntervalTextBox.Text  = "1000";
            ScreenComparisonIntervalTextBox.Text  = "5";
            ProblemMonitoringIntervalTextBox.Text = "4";
            MaximumAdsWatchingTimeTextBox.Text    = "35";
            X3GoldButtonClickDelayTextBox.Text    = "200";
            PixelDifferenceTextBox.Text           = "1";

            AdsWatchCheckBox.IsChecked  = true;
            GoldChestCheckBox.IsChecked = true;
            LogCheckBox.IsChecked       = false;

            EmailAddressTextBox.Text = "";

            SendEmailCheckBox.IsChecked        = false;
            StopHassanCheckBox.IsChecked       = false;
            ShutdownComputerCheckBox.IsChecked = false;
        }

        private void ApplyButton_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(ScreenMonitoringIntervalTextBox.Text, out int ScreenMonitoringInterval))
            {
                ScreenMonitoringInterval = 1000;
            }

            if (!int.TryParse(ScreenComparisonIntervalTextBox.Text, out int ScreenComparisonInterval))
            {
                ScreenComparisonInterval = 5;
            }

            if (!int.TryParse(ProblemMonitoringIntervalTextBox.Text, out int ProblemMonitoringInterval))
            {
                ProblemMonitoringInterval = 4;
            }

            if (!int.TryParse(MaximumAdsWatchingTimeTextBox.Text, out int MaximumAdsWatchingTime))
            {
                MaximumAdsWatchingTime = 35;
            }

            if (!int.TryParse(X3GoldButtonClickDelayTextBox.Text, out int X3GoldButtonClickDelay))
            {
                X3GoldButtonClickDelay = 200;
            }

            if (!int.TryParse(PixelDifferenceTextBox.Text, out int PixelDifference))
            {
                PixelDifference = 0;
            }

            ((MainWindow)System.Windows.Application.Current.MainWindow).ScreenMonitoringInterval  = ScreenMonitoringInterval;
            ((MainWindow)System.Windows.Application.Current.MainWindow).ScreenComparisonInterval  = ScreenComparisonInterval;
            ((MainWindow)System.Windows.Application.Current.MainWindow).ProblemMonitoringInterval = ProblemMonitoringInterval;
            ((MainWindow)System.Windows.Application.Current.MainWindow).MaximumAdsWatchingTime    = MaximumAdsWatchingTime;
            ((MainWindow)System.Windows.Application.Current.MainWindow).X3GoldButtonClickDelay    = X3GoldButtonClickDelay;
            ((MainWindow)System.Windows.Application.Current.MainWindow).PixelDifference           = PixelDifference;

            ((MainWindow)System.Windows.Application.Current.MainWindow).IsWatchAds         = AdsWatchCheckBox.IsChecked.Value;
            ((MainWindow)System.Windows.Application.Current.MainWindow).IsOpenGoldChestBox = GoldChestCheckBox.IsChecked.Value;
            ((MainWindow)System.Windows.Application.Current.MainWindow).IsLogging          = LogCheckBox.IsChecked.Value;

            ((MainWindow)System.Windows.Application.Current.MainWindow).EmailAddress       = EmailAddressTextBox.Text;

            ((MainWindow)System.Windows.Application.Current.MainWindow).IsNoGoldSendEmail  = SendEmailCheckBox.IsChecked.Value;
            ((MainWindow)System.Windows.Application.Current.MainWindow).IsNoGoldStopHassan = StopHassanCheckBox.IsChecked.Value;
            ((MainWindow)System.Windows.Application.Current.MainWindow).IsNoGoldShutdownPC = ShutdownComputerCheckBox.IsChecked.Value;

            using (StreamWriter streamWriter = new StreamWriter($@"setting.txt", false))
            {
                streamWriter.WriteLine("AppPlayerTitle," + ((MainWindow)System.Windows.Application.Current.MainWindow).AppPlayerTitleTextBox.Text);
                streamWriter.WriteLine("ScreenMonitoringInterval," + ScreenMonitoringIntervalTextBox.Text);
                streamWriter.WriteLine("ScreenComparisonInterval," + ScreenComparisonIntervalTextBox.Text);
                streamWriter.WriteLine("ProblemMonitoringInterval," + ProblemMonitoringIntervalTextBox.Text);
                streamWriter.WriteLine("MaximumAdsWatchingTime," + MaximumAdsWatchingTimeTextBox.Text);
                streamWriter.WriteLine("X3GoldButtonDelay," + X3GoldButtonClickDelayTextBox.Text);
                streamWriter.WriteLine("PixelDifference," + PixelDifferenceTextBox.Text);
                streamWriter.WriteLine("Korean," + ((MainWindow)System.Windows.Application.Current.MainWindow).KoreanCheckBox.IsChecked.Value);
                streamWriter.WriteLine("AdsCloseClickPattern," + ((MainWindow)System.Windows.Application.Current.MainWindow).GoogleAdCloseClickPattern);
                streamWriter.WriteLine("GoldChestCheck," + GoldChestCheckBox.IsChecked.Value);
                streamWriter.WriteLine("Logging," + LogCheckBox.IsChecked.Value);
                streamWriter.WriteLine("Email," + EmailAddressTextBox.Text);
                streamWriter.WriteLine("SendEmail," + SendEmailCheckBox.IsChecked.Value);
                streamWriter.WriteLine("StopHassan," + StopHassanCheckBox.IsChecked.Value);
                streamWriter.WriteLine("ShutdownPC," + ShutdownComputerCheckBox.IsChecked.Value);
            }

            this.Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void TextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            int caretIndex = (e.Source as TextBox).CaretIndex;
            string name = (e.Source as TextBox).Name;
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
    }
}
