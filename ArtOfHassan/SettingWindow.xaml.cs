using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

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

        private void AppLocationButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ApplyButton_Click(object sender, RoutedEventArgs e)
        {
            using (StreamWriter streamWriter = new StreamWriter($@"setting.txt", false))
            {
                System.Windows.Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
                {
                    ((MainWindow)System.Windows.Application.Current.MainWindow).AppLocationX = int.Parse(AppLocationX.Text);
                    ((MainWindow)System.Windows.Application.Current.MainWindow).AppLocationY = int.Parse(AppLocationY.Text);
                    ((MainWindow)System.Windows.Application.Current.MainWindow).AppLocationColor = AppLocationColor.Text;
                    streamWriter.WriteLine(AppLocationX.Text + "," + AppLocationY.Text + "," + AppLocationColor.Text);

                    ((MainWindow)System.Windows.Application.Current.MainWindow).HomeButtonX = int.Parse(HomeButtonX.Text);
                    ((MainWindow)System.Windows.Application.Current.MainWindow).ShopButtonX = int.Parse(ShopButtonX.Text);
                    ((MainWindow)System.Windows.Application.Current.MainWindow).ShopButtonY = int.Parse(ShopButtonY.Text);
                    ((MainWindow)System.Windows.Application.Current.MainWindow).ShopButtonColor = ShopButtonColor.Text;
                    streamWriter.WriteLine(HomeButtonX.Text + "," + ShopButtonX.Text + "," + ShopButtonY.Text + "," + ShopButtonColor.Text);

                    ((MainWindow)System.Windows.Application.Current.MainWindow).MiddleButtonX = int.Parse(MiddleButtonX.Text);
                    ((MainWindow)System.Windows.Application.Current.MainWindow).MiddleButtonY = int.Parse(MiddleButtonY.Text);
                    ((MainWindow)System.Windows.Application.Current.MainWindow).MiddleButtonColor = MiddleButtonColor.Text;
                    streamWriter.WriteLine(MiddleButtonX.Text + "," + MiddleButtonY.Text + "," + MiddleButtonColor.Text);

                    ((MainWindow)System.Windows.Application.Current.MainWindow).GoldChestBoxX = int.Parse(GoldChestBoxX.Text);
                    ((MainWindow)System.Windows.Application.Current.MainWindow).GoldChestBoxY = int.Parse(GoldChestBoxY.Text);
                    ((MainWindow)System.Windows.Application.Current.MainWindow).GoldChestBoxColor = GoldChestBoxColor.Text;
                    streamWriter.WriteLine(GoldChestBoxX.Text + "," + GoldChestBoxY.Text + "," + GoldChestBoxColor.Text);

                    ((MainWindow)System.Windows.Application.Current.MainWindow).BattleLevelButtonX = int.Parse(BattleLevelButtonX.Text);
                    ((MainWindow)System.Windows.Application.Current.MainWindow).BattleLevelButtonY = int.Parse(BattleLevelButtonY.Text);
                    ((MainWindow)System.Windows.Application.Current.MainWindow).BattleLevelButtonColor = BattleLevelButtonColor.Text;
                    streamWriter.WriteLine(BattleLevelButtonX.Text + "," + BattleLevelButtonY.Text + "," + BattleLevelButtonColor.Text);

                    ((MainWindow)System.Windows.Application.Current.MainWindow).SkillButtonX = int.Parse(SkillButtonX.Text);
                    ((MainWindow)System.Windows.Application.Current.MainWindow).SkillButtonY = int.Parse(SkillButtonY.Text);
                    ((MainWindow)System.Windows.Application.Current.MainWindow).SkillButtonColor = SkillButtonColor.Text;
                    streamWriter.WriteLine(SkillButtonX.Text + "," + SkillButtonY.Text + "," + SkillButtonColor.Text);

                    ((MainWindow)System.Windows.Application.Current.MainWindow).SpeedButtonX = int.Parse(SpeedButtonX.Text);
                    ((MainWindow)System.Windows.Application.Current.MainWindow).SpeedButtonY = int.Parse(SpeedButtonY.Text);
                    ((MainWindow)System.Windows.Application.Current.MainWindow).SpeedButtonColor = SpeedButtonColor.Text;
                    streamWriter.WriteLine(SpeedButtonX.Text + "," + SpeedButtonY.Text + "," + SpeedButtonColor.Text);

                    ((MainWindow)System.Windows.Application.Current.MainWindow).PauseButtonX = int.Parse(PauseButtonX.Text);
                    ((MainWindow)System.Windows.Application.Current.MainWindow).PauseButtonY = int.Parse(PauseButtonY.Text);
                    ((MainWindow)System.Windows.Application.Current.MainWindow).PauseButtonColor = PauseButtonColor.Text;
                    streamWriter.WriteLine(PauseButtonX.Text + "," + PauseButtonY.Text + "," + PauseButtonColor.Text);

                    ((MainWindow)System.Windows.Application.Current.MainWindow).VictoryDefeatX = int.Parse(VictoryDefeatX.Text);
                    ((MainWindow)System.Windows.Application.Current.MainWindow).VictoryDefeatY = int.Parse(VictoryDefeatY.Text);
                    ((MainWindow)System.Windows.Application.Current.MainWindow).VictoryDefeatColor = VictoryDefeatColor.Text;
                    streamWriter.WriteLine(VictoryDefeatX.Text + "," + VictoryDefeatY.Text + "," + VictoryDefeatColor.Text);

                    ((MainWindow)System.Windows.Application.Current.MainWindow).GoldButtonBackgroundX = int.Parse(GoldButtonBackgroundX.Text);
                    ((MainWindow)System.Windows.Application.Current.MainWindow).GoldButtonBackgroundY = int.Parse(GoldButtonBackgroundY.Text);
                    ((MainWindow)System.Windows.Application.Current.MainWindow).GoldButtonBackgroundColor = GoldButtonBackgroundColor.Text;
                    streamWriter.WriteLine(GoldButtonBackgroundX.Text + "," + GoldButtonBackgroundY.Text + "," + GoldButtonBackgroundColor.Text);

                    ((MainWindow)System.Windows.Application.Current.MainWindow).GoldButtonImageX = int.Parse(GoldButtonImageX.Text);
                    ((MainWindow)System.Windows.Application.Current.MainWindow).GoldButtonImageY = int.Parse(GoldButtonImageY.Text);
                    ((MainWindow)System.Windows.Application.Current.MainWindow).GoldButtonImageColor = GoldButtonImageColor.Text;
                    streamWriter.WriteLine(GoldButtonImageX.Text + "," + GoldButtonImageY.Text + "," + GoldButtonImageColor.Text);

                    ((MainWindow)System.Windows.Application.Current.MainWindow).NextButtonX = int.Parse(NextButtonX.Text);
                    ((MainWindow)System.Windows.Application.Current.MainWindow).NextButtonY = int.Parse(NextButtonY.Text);
                    ((MainWindow)System.Windows.Application.Current.MainWindow).NextButtonColor = NextButtonColor.Text;
                    streamWriter.WriteLine(NextButtonX.Text + "," + NextButtonY.Text + "," + NextButtonColor.Text);

                    ((MainWindow)System.Windows.Application.Current.MainWindow).AdsButtonX = (int.Parse(AdsButton1X.Text) + int.Parse(AdsButton2X.Text)) / 2;
                    ((MainWindow)System.Windows.Application.Current.MainWindow).AdsButton1Y = int.Parse(AdsButton1Y.Text);
                    ((MainWindow)System.Windows.Application.Current.MainWindow).AdsButton2Y = int.Parse(AdsButton2Y.Text);
                    ((MainWindow)System.Windows.Application.Current.MainWindow).AdsButtonColor = AdsButton1Color.Text + ";" + AdsButton2Color.Text;
                    streamWriter.WriteLine((int)((int.Parse(AdsButton1X.Text) + int.Parse(AdsButton2X.Text)) / 2) + "," + AdsButton1Y.Text + "," + AdsButton2Y.Text + "," + AdsButton1Color.Text + ";" + AdsButton2Color.Text);

                    ((MainWindow)System.Windows.Application.Current.MainWindow).AdsCloseButton1X = int.Parse(AdsCloseButton1X.Text);
                    ((MainWindow)System.Windows.Application.Current.MainWindow).AdsCloseButton2X = int.Parse(AdsCloseButton2X.Text);
                    ((MainWindow)System.Windows.Application.Current.MainWindow).AdsCloseButtonY = (int.Parse(AdsCloseButton1Y.Text) + int.Parse(AdsCloseButton2Y.Text)) / 2;
                    streamWriter.WriteLine(AdsCloseButton1X.Text + "," + AdsCloseButton2X.Text + "," + (int)((int.Parse(AdsCloseButton1Y.Text) + int.Parse(AdsCloseButton2Y.Text)) / 2));

                    ((MainWindow)System.Windows.Application.Current.MainWindow).NotRespondingX = int.Parse(NotRespondingX.Text);
                    ((MainWindow)System.Windows.Application.Current.MainWindow).NotRespondingY = int.Parse(NotRespondingY.Text);
                    ((MainWindow)System.Windows.Application.Current.MainWindow).NotRespondingColor = NotRespondingColor.Text;
                    streamWriter.WriteLine(NotRespondingX.Text + "," + NotRespondingY.Text + "," + NotRespondingColor.Text);
                }));
            }

            this.Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
