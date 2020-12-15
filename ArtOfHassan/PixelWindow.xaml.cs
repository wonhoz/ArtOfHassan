using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;
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

        #endregion

        #region Variable

        double NoxPointX = 0;
        double NoxPointY = 0;
        double NoxWidth  = 0;
        double NoxHeight = 0;

        #endregion


        public PixelWindow()
        {
            InitializeComponent();

            SetUiLanguage();
        }


        #region Private Event Method

        private void PositionColorButton_Click(object sender, RoutedEventArgs e)
        {
            string buttonName = (e.Source as Button).Name;

            int buttonIndex;

            FieldInfo fieldInfoX;
            FieldInfo fieldInfoY;
            FieldInfo fieldInfoColor;

            if (buttonName.Contains("1"))
            {
                buttonIndex    = 0;
                fieldInfoX     = this.GetType().GetField(buttonName.Replace("1", "X"),     BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                fieldInfoY     = this.GetType().GetField(buttonName.Replace("1", "Y"),     BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                fieldInfoColor = this.GetType().GetField(buttonName.Replace("1", "Color"), BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            }
            else if (buttonName.Contains("2"))
            {
                buttonIndex    = 1;
                fieldInfoX     = this.GetType().GetField(buttonName.Replace("2", "X"),     BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                fieldInfoY     = this.GetType().GetField(buttonName.Replace("2", "Y"),     BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                fieldInfoColor = this.GetType().GetField(buttonName.Replace("2", "Color"), BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            }
            else if (buttonName.Contains("3"))
            {
                buttonIndex    = 2;
                fieldInfoX     = this.GetType().GetField(buttonName.Replace("3", "X"),     BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                fieldInfoY     = this.GetType().GetField(buttonName.Replace("3", "Y"),     BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                fieldInfoColor = this.GetType().GetField(buttonName.Replace("3", "Color"), BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            }
            else
            {
                buttonIndex    = 3;
                fieldInfoX     = this.GetType().GetField(buttonName.Replace("4", "X"),     BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                fieldInfoY     = this.GetType().GetField(buttonName.Replace("4", "Y"),     BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                fieldInfoColor = this.GetType().GetField(buttonName.Replace("4", "Color"), BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            }

            TextBox TargetTextBox = new TextBox();
            string[] colorset = { };
            if (fieldInfoColor != null)
            {
                TargetTextBox = fieldInfoColor.GetValue(this) as TextBox;
                colorset      = TargetTextBox.Text.Split(';');
            }

            System.Windows.Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
            {
                // X Position
                if ((fieldInfoX.GetValue(this) as TextBox).Text.Contains(";"))
                {
                    ((MainWindow)System.Windows.Application.Current.MainWindow).PixelPositionX = int.Parse((fieldInfoX.GetValue(this) as TextBox).Text.Split(';')[buttonIndex]);
                }
                else
                {
                    ((MainWindow)System.Windows.Application.Current.MainWindow).PixelPositionX = int.Parse((fieldInfoX.GetValue(this) as TextBox).Text);
                }

                // Y Position
                if ((fieldInfoY.GetValue(this) as TextBox).Text.Contains(";"))
                {
                    ((MainWindow)System.Windows.Application.Current.MainWindow).PixelPositionY = int.Parse((fieldInfoY.GetValue(this) as TextBox).Text.Split(';')[buttonIndex]);
                }
                else
                {
                    ((MainWindow)System.Windows.Application.Current.MainWindow).PixelPositionY = int.Parse((fieldInfoY.GetValue(this) as TextBox).Text);
                }

                // Color
                if (fieldInfoColor != null)
                {
                    if (buttonName.Contains("NotRespondAppCloseButton") || (buttonName == "HomeShopButton2"))
                    {
                        ((MainWindow)System.Windows.Application.Current.MainWindow).PixelColor = ColorTranslator.FromHtml(colorset[0]);
                    }
                    else if (buttonName != "HomeShopButton1")
                    {
                        ((MainWindow)System.Windows.Application.Current.MainWindow).PixelColor = ColorTranslator.FromHtml(colorset[buttonIndex]);
                    }
                }
            }));

            GetPixelPositionAndColor();

            System.Windows.Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
            {
                // X Position
                if ((fieldInfoX.GetValue(this) as TextBox).Text.Contains(";"))
                {
                    string resultText = "";
                    string[] originalTexts = (fieldInfoX.GetValue(this) as TextBox).Text.Split(';');
                    for (int i = 0; i < originalTexts.Length; i++)
                    {
                        if (i == buttonIndex)
                        {
                            resultText += ((MainWindow)System.Windows.Application.Current.MainWindow).PixelPositionX.ToString();
                        }
                        else
                        {
                            resultText += originalTexts[i];
                        }

                        if (i != (originalTexts.Length - 1))
                        {
                            resultText += ";";
                        }
                    }
                    (fieldInfoX.GetValue(this) as TextBox).Text = resultText;
                }
                else
                {
                    (fieldInfoX.GetValue(this) as TextBox).Text = ((MainWindow)System.Windows.Application.Current.MainWindow).PixelPositionX.ToString();
                }

                // Y Position
                if ((fieldInfoY.GetValue(this) as TextBox).Text.Contains(";"))
                {
                    string resultText = "";
                    string[] originalTexts = (fieldInfoY.GetValue(this) as TextBox).Text.Split(';');
                    for (int i = 0; i < originalTexts.Length; i++)
                    {
                        if (i == buttonIndex)
                        {
                            resultText += ((MainWindow)System.Windows.Application.Current.MainWindow).PixelPositionY.ToString();
                        }
                        else
                        {
                            resultText += originalTexts[i];
                        }

                        if (i != (originalTexts.Length - 1))
                        {
                            resultText += ";";
                        }
                    }
                    (fieldInfoY.GetValue(this) as TextBox).Text = resultText;
                }
                else
                {
                    (fieldInfoY.GetValue(this) as TextBox).Text = ((MainWindow)System.Windows.Application.Current.MainWindow).PixelPositionY.ToString();
                }

                // Color
                if (fieldInfoColor != null)
                {
                    if (buttonName.Contains("NotRespondAppCloseButton"))
                    {
                        NotRespondAppCloseButton1.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(ColorTranslator.ToHtml(((MainWindow)System.Windows.Application.Current.MainWindow).PixelColor)));
                        NotRespondAppCloseButton2.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(ColorTranslator.ToHtml(((MainWindow)System.Windows.Application.Current.MainWindow).PixelColor)));
                        NotRespondAppCloseButton3.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(ColorTranslator.ToHtml(((MainWindow)System.Windows.Application.Current.MainWindow).PixelColor)));
                    }
                    else if (buttonName != "HomeShopButton1")
                    {
                        (e.Source as Button).Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(ColorTranslator.ToHtml(((MainWindow)System.Windows.Application.Current.MainWindow).PixelColor)));
                    }

                    switch (colorset.Length)
                    {
                        case 1:
                            {
                                if (buttonName != "HomeShopButton1")
                                {
                                    TargetTextBox.Text = ColorTranslator.ToHtml(((MainWindow)System.Windows.Application.Current.MainWindow).PixelColor);
                                }
                            }
                            break;
                        case 2:
                            {
                                switch (buttonIndex)
                                {
                                    case 0:
                                        TargetTextBox.Text = ColorTranslator.ToHtml(((MainWindow)System.Windows.Application.Current.MainWindow).PixelColor) + ";" + colorset[1];
                                        break;
                                    case 1:
                                        TargetTextBox.Text = colorset[0] + ";" + ColorTranslator.ToHtml(((MainWindow)System.Windows.Application.Current.MainWindow).PixelColor);
                                        break;
                                }
                            }
                            break;
                        case 3:
                            {
                                switch (buttonIndex)
                                {
                                    case 0:
                                        TargetTextBox.Text = ColorTranslator.ToHtml(((MainWindow)System.Windows.Application.Current.MainWindow).PixelColor) + ";" + colorset[1] + ";" + colorset[2];
                                        break;
                                    case 1:
                                        TargetTextBox.Text = colorset[0] + ";" + ColorTranslator.ToHtml(((MainWindow)System.Windows.Application.Current.MainWindow).PixelColor) + ";" + colorset[2];
                                        break;
                                    case 2:
                                        TargetTextBox.Text = colorset[0] + ";" + colorset[1] + ";" + ColorTranslator.ToHtml(((MainWindow)System.Windows.Application.Current.MainWindow).PixelColor);
                                        break;
                                }
                            }
                            break;
                        case 4:
                            {
                                switch (buttonIndex)
                                {
                                    case 0:
                                        TargetTextBox.Text = ColorTranslator.ToHtml(((MainWindow)System.Windows.Application.Current.MainWindow).PixelColor) + ";" + colorset[1] + ";" + colorset[2] + ";" + colorset[3];
                                        break;
                                    case 1:
                                        TargetTextBox.Text = colorset[0] + ";" + ColorTranslator.ToHtml(((MainWindow)System.Windows.Application.Current.MainWindow).PixelColor) + ";" + colorset[2] + ";" + colorset[3];
                                        break;
                                    case 2:
                                        TargetTextBox.Text = colorset[0] + ";" + colorset[1] + ";" + ColorTranslator.ToHtml(((MainWindow)System.Windows.Application.Current.MainWindow).PixelColor) + ";" + colorset[3];
                                        break;
                                    case 3:
                                        TargetTextBox.Text = colorset[0] + ";" + colorset[1] + ";" + colorset[2] + ";" + ColorTranslator.ToHtml(((MainWindow)System.Windows.Application.Current.MainWindow).PixelColor);
                                        break;
                                }
                            }
                            break;
                    }

                    string targetTextBoxText = TargetTextBox.Text;
                }
            }));
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
                        case ("homeshopbutton"):
                            HomeShopButtonX.Text = listitem[1];
                            HomeShopButtonY.Text = listitem[2];
                            HomeShopButtonColor.Text = listitem[3];
                            break;
                        case ("goldchestbox"):
                            GoldChestBoxX.Text = listitem[1];
                            GoldChestBoxY.Text = listitem[2];
                            GoldChestBoxColor.Text = listitem[3];
                            break;
                        case ("collectbutton"):
                            CollectButtonX.Text = listitem[1];
                            CollectButtonY.Text = listitem[2];
                            CollectButtonColor.Text = listitem[3];
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
                        case ("nogold"):
                            NoGoldX.Text = listitem[1];
                            NoGoldY.Text = listitem[2];
                            NoGoldColor.Text = listitem[3];
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
                        case ("gameadclosebutton"):
                            GameAdCloseButtonX.Text = listitem[1];
                            GameAdCloseButtonY.Text = listitem[2];
                            GameAdCloseButtonColor.Text = listitem[3];
                            break;
                        case ("googleadclosebutton"):
                            GoogleAdCloseButtonX.Text = listitem[1];
                            GoogleAdCloseButtonY.Text = listitem[2];
                            GoogleAdCloseButtonColor.Text = listitem[3];
                            break;
                        case ("latestusedsppbutton"):
                            LatestUsedAppButtonX.Text = listitem[1];
                            LatestUsedAppButtonY.Text = listitem[2];
                            break;
                        case ("righttopappclosebutton"):
                            RightTopAppCloseButtonX.Text = listitem[1];
                            RightTopAppCloseButtonY.Text = listitem[2];
                            break;
                        case ("notrespondappclosebutton"):
                            NotRespondAppCloseButtonX.Text = listitem[1];
                            NotRespondAppCloseButtonY.Text = listitem[2];
                            NotRespondAppCloseButtonColor.Text = listitem[3];
                            break;
                    }
                }

                ChangeButtonsColors();
            }
        }

        private void SavePixelButton_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.SaveFileDialog saveFileDialog = new System.Windows.Forms.SaveFileDialog();
            saveFileDialog.Filter = "Text File (*.txt)|*.txt";
            if (saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                using (StreamWriter streamWriter = new StreamWriter(saveFileDialog.FileName, false))
                {
                    streamWriter.WriteLine("AppLocation," + AppLocationX.Text + "," + AppLocationY.Text + "," + AppLocationColor.Text);
                    streamWriter.WriteLine("HomeShopButton," + HomeShopButtonX.Text + "," + HomeShopButtonY.Text + "," + HomeShopButtonColor.Text);
                    streamWriter.WriteLine("GoldChestBox," + GoldChestBoxX.Text + "," + GoldChestBoxY.Text + "," + GoldChestBoxColor.Text);
                    streamWriter.WriteLine("CollectButton," + CollectButtonX.Text + "," + CollectButtonY.Text + "," + CollectButtonColor.Text);
                    streamWriter.WriteLine("BattleLevelButton," + BattleLevelButtonX.Text + "," + BattleLevelButtonY.Text + "," + BattleLevelButtonColor.Text);
                    streamWriter.WriteLine("SkillButton," + SkillButtonX.Text + "," + SkillButtonY.Text + "," + SkillButtonColor.Text);
                    streamWriter.WriteLine("SpeedButton," + SpeedButtonX.Text + "," + SpeedButtonY.Text + "," + SpeedButtonColor.Text);
                    streamWriter.WriteLine("ContinueButton," + ContinueButtonX.Text + "," + ContinueButtonY.Text + "," + ContinueButtonColor.Text);
                    streamWriter.WriteLine("VictoryDefeat," + VictoryDefeatX.Text + "," + VictoryDefeatY.Text + "," + VictoryDefeatColor.Text);
                    streamWriter.WriteLine("NoGold," + NoGoldX.Text + "," + NoGoldY.Text + "," + NoGoldColor.Text);
                    streamWriter.WriteLine("GoldButtonBackground," + GoldButtonBackgroundX.Text + "," + GoldButtonBackgroundY.Text + "," + GoldButtonBackgroundColor.Text);
                    streamWriter.WriteLine("GoldButtonImage," + GoldButtonImageX.Text + "," + GoldButtonImageY.Text + "," + GoldButtonImageColor.Text);
                    streamWriter.WriteLine("NextButton," + NextButtonX.Text + "," + NextButtonY.Text + "," + NextButtonColor.Text);
                    streamWriter.WriteLine("GameAdCloseButton," + GameAdCloseButtonX.Text + "," + GameAdCloseButtonY.Text + "," + GameAdCloseButtonColor.Text);
                    streamWriter.WriteLine("GoogleAdCloseButton," + GoogleAdCloseButtonX.Text + "," + GoogleAdCloseButtonY.Text + "," + GoogleAdCloseButtonColor.Text);
                    streamWriter.WriteLine("LatestUsedAppButton," + LatestUsedAppButtonX.Text + "," + LatestUsedAppButtonY.Text);
                    streamWriter.WriteLine("RightTopAppCloseButton," + RightTopAppCloseButtonX.Text + "," + RightTopAppCloseButtonY.Text);
                    streamWriter.WriteLine("NotRespondAppCloseButton," + NotRespondAppCloseButtonX.Text + "," + NotRespondAppCloseButtonY.Text + "," + NotRespondAppCloseButtonColor.Text);
                }
            }
        }

        private void DefaultButton_Click(object sender, RoutedEventArgs e)
        {
            AppLocationX.Text = 60.ToString();
            AppLocationY.Text = 500.ToString();
            AppLocationColor.Text = "#a9d8ff;#97c601".ToUpper();

            HomeShopButtonX.Text = 290.ToString() + ";" + 65.ToString();
            HomeShopButtonY.Text = 980.ToString();
            HomeShopButtonColor.Text = "#ea3d34".ToUpper();

            GoldChestBoxX.Text = 150.ToString();
            GoldChestBoxY.Text = 410.ToString();
            GoldChestBoxColor.Text = "#fff102;#eabf2f".ToUpper();

            CollectButtonX.Text = 195.ToString();
            CollectButtonY.Text = 680.ToString();
            CollectButtonColor.Text = "#fdbb00".ToUpper();

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

            NoGoldX.Text = 321.ToString(); ;
            NoGoldY.Text = 646.ToString(); ;
            NoGoldColor.Text = "#dfd6be".ToUpper();

            GoldButtonBackgroundX.Text = 115.ToString();
            GoldButtonBackgroundY.Text = 780.ToString();
            GoldButtonBackgroundColor.Text = "#7da70a;#8e8e8e".ToUpper();

            GoldButtonImageX.Text = 133.ToString();
            GoldButtonImageY.Text = 755.ToString();
            GoldButtonImageColor.Text = "#ffea90".ToUpper();

            NextButtonX.Text = 450.ToString();
            NextButtonY.Text = 710.ToString();
            NextButtonColor.Text = "#fdbb00".ToUpper();

            GameAdCloseButtonX.Text = 496.ToString();
            GameAdCloseButtonY.Text = 180.ToString() + ";" + 190.ToString() + ";" + 262.ToString();
            GameAdCloseButtonColor.Text = "#efe7d6;#e9e9d8;#e9e9d8".ToUpper();

            GoogleAdCloseButtonX.Text = 45.ToString() + ";" + 513.ToString();
            GoogleAdCloseButtonY.Text = 63.ToString();
            GoogleAdCloseButtonColor.Text = "#4c4c4f;#3c4043".ToUpper();

            LatestUsedAppButtonX.Text = 580.ToString();
            LatestUsedAppButtonY.Text = 1000.ToString();

            RightTopAppCloseButtonX.Text = 501.ToString();
            RightTopAppCloseButtonY.Text = 150.ToString();

            NotRespondAppCloseButtonX.Text = 79.ToString();
            NotRespondAppCloseButtonY.Text = 510.ToString() + ";" + 525.ToString() + ";" + 540.ToString();
            NotRespondAppCloseButtonColor.Text = "#009688".ToUpper();

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

                    ((MainWindow)System.Windows.Application.Current.MainWindow).HomeButtonX = int.Parse(HomeShopButtonX.Text.Split(';')[0]);
                    ((MainWindow)System.Windows.Application.Current.MainWindow).ShopButtonX = int.Parse(HomeShopButtonX.Text.Split(';')[1]);
                    ((MainWindow)System.Windows.Application.Current.MainWindow).ShopButtonY = int.Parse(HomeShopButtonY.Text);
                    ((MainWindow)System.Windows.Application.Current.MainWindow).ShopButtonColor = HomeShopButtonColor.Text;
                    streamWriter.WriteLine("HomeShopButton," + HomeShopButtonX.Text + "," + HomeShopButtonY.Text + "," + HomeShopButtonColor.Text);

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

                    ((MainWindow)System.Windows.Application.Current.MainWindow).NoGoldX = int.Parse(NoGoldX.Text);
                    ((MainWindow)System.Windows.Application.Current.MainWindow).NoGoldY = int.Parse(NoGoldY.Text);
                    ((MainWindow)System.Windows.Application.Current.MainWindow).NoGoldColor = NoGoldColor.Text;
                    streamWriter.WriteLine("NoGold," + NoGoldX.Text + "," + NoGoldY.Text + "," + NoGoldColor.Text);

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

                    ((MainWindow)System.Windows.Application.Current.MainWindow).GameAdCloseButtonX = int.Parse(GameAdCloseButtonX.Text);
                    ((MainWindow)System.Windows.Application.Current.MainWindow).GoldAdCloseButtonY = int.Parse(GameAdCloseButtonY.Text.Split(';')[0]);
                    ((MainWindow)System.Windows.Application.Current.MainWindow).TroopAdCloseButtonY = int.Parse(GameAdCloseButtonY.Text.Split(';')[1]);
                    ((MainWindow)System.Windows.Application.Current.MainWindow).MidasAdCloseButtonY = int.Parse(GameAdCloseButtonY.Text.Split(';')[2]);
                    ((MainWindow)System.Windows.Application.Current.MainWindow).GameAdCloseButtonColor = GameAdCloseButtonColor.Text;
                    streamWriter.WriteLine("GameAdCloseButton," + GameAdCloseButtonX.Text + "," + GameAdCloseButtonY.Text + "," + GameAdCloseButtonColor.Text);

                    ((MainWindow)System.Windows.Application.Current.MainWindow).LeftAdCloseButtonX = int.Parse(GoogleAdCloseButtonX.Text.Split(';')[0]);
                    ((MainWindow)System.Windows.Application.Current.MainWindow).RightAdCloseButtonX = int.Parse(GoogleAdCloseButtonX.Text.Split(';')[1]);
                    ((MainWindow)System.Windows.Application.Current.MainWindow).GoogleAdCloseButtonY = int.Parse(GoogleAdCloseButtonY.Text);
                    ((MainWindow)System.Windows.Application.Current.MainWindow).GoogleAdCloseButtonColor = GoogleAdCloseButtonColor.Text;
                    streamWriter.WriteLine("GoogleAdCloseButton," + GoogleAdCloseButtonX.Text + "," + GoogleAdCloseButtonY.Text + "," + GoogleAdCloseButtonColor.Text);

                    ((MainWindow)System.Windows.Application.Current.MainWindow).LatestUsedAppButtonX = int.Parse(LatestUsedAppButtonX.Text);
                    ((MainWindow)System.Windows.Application.Current.MainWindow).LatestUsedAppButtonY = int.Parse(LatestUsedAppButtonY.Text);
                    streamWriter.WriteLine("LatestUsedAppButton," + LatestUsedAppButtonX.Text + "," + LatestUsedAppButtonY.Text);

                    ((MainWindow)System.Windows.Application.Current.MainWindow).RightTopAppCloseButtonX = int.Parse(RightTopAppCloseButtonX.Text);
                    ((MainWindow)System.Windows.Application.Current.MainWindow).RightTopAppCloseButtonY = int.Parse(RightTopAppCloseButtonY.Text);
                    streamWriter.WriteLine("RightTopAppCloseButton," + RightTopAppCloseButtonX.Text + "," + RightTopAppCloseButtonY.Text);

                    ((MainWindow)System.Windows.Application.Current.MainWindow).NotRespondAppCloseButtonX = int.Parse(NotRespondAppCloseButtonX.Text);
                    ((MainWindow)System.Windows.Application.Current.MainWindow).NotRespondAppCloseButtonY1 = int.Parse(NotRespondAppCloseButtonY.Text.Split(';')[0]);
                    ((MainWindow)System.Windows.Application.Current.MainWindow).NotRespondAppCloseButtonY2 = int.Parse(NotRespondAppCloseButtonY.Text.Split(';')[1]);
                    ((MainWindow)System.Windows.Application.Current.MainWindow).NotRespondAppCloseButtonY3 = int.Parse(NotRespondAppCloseButtonY.Text.Split(';')[2]);
                    ((MainWindow)System.Windows.Application.Current.MainWindow).NotRespondAppCloseButtonColor = NotRespondAppCloseButtonColor.Text;
                    streamWriter.WriteLine("NotRespondAppCloseButton," + NotRespondAppCloseButtonX.Text + "," + NotRespondAppCloseButtonY.Text + "," + NotRespondAppCloseButtonColor.Text);
                }));
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
            if (name.Contains("Color"))
            {
                (e.Source as TextBox).Text = Regex.Replace((e.Source as TextBox).Text, RegExClass.HtmlColor, "");
            }
            else
            {
                (e.Source as TextBox).Text = Regex.Replace((e.Source as TextBox).Text, RegExClass.Numbers, "");
            }
            (e.Source as TextBox).Select(caretIndex, 0);
        }

        #endregion


        #region Private Method

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

            size  = new System.Windows.Size(placement.normal_position.Right - (placement.normal_position.Left * 2), placement.normal_position.Bottom - (placement.normal_position.Top * 2));
            point = new System.Windows.Point(placement.normal_position.Left, placement.normal_position.Top);
        }

        private void GetPixelPositionAndColor()
        {
            System.Windows.Point point = new System.Windows.Point();
            System.Windows.Size size   = new System.Windows.Size();

            GetWindowPos(GetWinAscHandle(), ref point, ref size);

            if ((size.Width != 0) && (size.Height != 0))
            {
                NoxPointX = point.X;
                NoxPointY = point.Y;
                NoxWidth  = size.Width;
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
                    bitmapImage.CacheOption  = BitmapCacheOption.OnLoad;
                    bitmapImage.EndInit();

                    Screenshot screenshot = new Screenshot();
                    screenshot.Width  = NoxWidth;
                    screenshot.Height = NoxHeight;
                    screenshot.MainImage.Source = bitmapImage;
                    screenshot.CurrentBitmap    = CurrentBitmap;
                    screenshot.ShowDialog();
                }
            }
        }

        private void SetUiLanguage()
        {
            if (((MainWindow)System.Windows.Application.Current.MainWindow).KoreanCheckBox.IsChecked.Value)
            {
                this.Title = "픽셀 위치 및 색상 사용자화";
                ItemTextBlock.Text = "항목";
                AppLocationTextBlock.Text = "어플 위치";
                HomeShopButtonTextBlock.Text = "홈 및 상점버튼";
                GoldChestBoxTextBlock.Text = "시간보상";
                CollectButtonTextBlock.Text = "수집버튼";
                BattleLevelButtonTextBlock.Text = "전투레벨버튼";
                AutoSkillButtonTextBlock.Text = "자동스킬버튼";
                VIPX2ButtonTextBlock.Text = "2배속버튼";
                ContinueButtonTextBlock.Text = "계속버튼";
                VictoryDefeatTextBlock.Text = "승리패배 플래그";
                X3GoldButtonBackgroundTextBlock.Text = "골드3배버튼 배경";
                X3GoldButtonImageTextBlock.Text = "골드3배버튼 그림";
                NextButtonTextBlock.Text = "다음버튼 (패배시)";
                NoGoldTextBlock.Text = "골드벌이 없을 때";
                GameAdCloseButtonTextBlock.Text = "골드 | 용병 | 마이더스\n    광고 닫기버튼";
                GoogleAdCloseButtonTextBlock.Text = "구글 광고닫기버튼";
                LatestUsedAppButtonTextBlock.Text = "최근 사용앱 버튼";
                RightTopAppCloseButtonTextBlock.Text = "우상단 앱 닫기버튼";
                NotRespondAppCloseButtonTextBlock.Text = "응답없음 닫기버튼";
                XPositionTextBlock.Text = "X 좌표";
                YPositionTextBlock.Text = "Y 좌표";
                ColorCodeTextBlock.Text = "HTML 색상코드";
                ColorPickTextBlock.Text = "색상 선택";

                LoadButton.Content = "불러오기";
                SavePixelButton.Content = "다른이름으로\n      저장";
                DefaultButton.Content = "초기화";
                ApplyButton.Content = "적용";
                CancelButton.Content = "취소";
            }
        }

        private void ChangeButtonsColors()
        {
            AppLocation1.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(AppLocationColor.Text.Split(';')[0]));
            AppLocation2.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(AppLocationColor.Text.Split(';')[1]));
            HomeShopButton2.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(HomeShopButtonColor.Text));
            GoldChestBox1.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(GoldChestBoxColor.Text.Split(';')[0]));
            GoldChestBox2.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(GoldChestBoxColor.Text.Split(';')[1]));
            CollectButton1.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(CollectButtonColor.Text));
            BattleLevelButton1.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(BattleLevelButtonColor.Text.Split(';')[0]));
            BattleLevelButton2.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(BattleLevelButtonColor.Text.Split(';')[1]));
            BattleLevelButton3.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(BattleLevelButtonColor.Text.Split(';')[2]));
            BattleLevelButton4.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(BattleLevelButtonColor.Text.Split(';')[3]));
            SkillButton1.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(SkillButtonColor.Text));
            SpeedButton1.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(SpeedButtonColor.Text));
            ContinueButton1.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(ContinueButtonColor.Text));
            VictoryDefeat1.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(VictoryDefeatColor.Text.Split(';')[0]));
            VictoryDefeat2.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(VictoryDefeatColor.Text.Split(';')[1]));
            NoGold1.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(NoGoldColor.Text));
            GoldButtonBackground1.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(GoldButtonBackgroundColor.Text.Split(';')[0]));
            GoldButtonBackground2.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(GoldButtonBackgroundColor.Text.Split(';')[1]));
            GoldButtonImage1.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(GoldButtonImageColor.Text.Split(';')[0]));
            NextButton1.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(NextButtonColor.Text));
            GameAdCloseButton1.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(GameAdCloseButtonColor.Text.Split(';')[0]));
            GameAdCloseButton2.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(GameAdCloseButtonColor.Text.Split(';')[1]));
            GameAdCloseButton3.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(GameAdCloseButtonColor.Text.Split(';')[2]));
            GoogleAdCloseButton1.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(GoogleAdCloseButtonColor.Text.Split(';')[0]));
            GoogleAdCloseButton2.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(GoogleAdCloseButtonColor.Text.Split(';')[1]));
            NotRespondAppCloseButton1.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(NotRespondAppCloseButtonColor.Text));
        }

        #endregion
    }
}
