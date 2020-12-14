using System;
using System.Windows;
using System.Windows.Threading;

namespace ArtOfHassan
{
    /// <summary>
    /// ClickPatternWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ClickPatternWindow : Window
    {
        public ClickPatternWindow()
        {
            InitializeComponent();

            string[] ClickPatterns = ((MainWindow)System.Windows.Application.Current.MainWindow).ClickPattern.Split(';');

            if (ClickPatterns[0] == "L")
            {
                FirstLeft.IsChecked = true;
                FirstRight.IsChecked = false;
            }
            else
            {
                FirstLeft.IsChecked = false;
                FirstRight.IsChecked = true;
            }

            if (ClickPatterns[1] == "L")
            {
                SecondLeft.IsChecked = true;
                SecondRight.IsChecked = false;
            }
            else
            {
                SecondLeft.IsChecked = false;
                SecondRight.IsChecked = true;
            }

            if (ClickPatterns.Length >= 3)
            {
                ThirdClickCheckBox.IsChecked = true;
                if (ClickPatterns[2] == "L")
                {
                    ThirdLeft.IsChecked = true;
                    ThirdRight.IsChecked = false;
                }
                else
                {
                    ThirdLeft.IsChecked = false;
                    ThirdRight.IsChecked = true;
                }
            }
            else
            {
                ThirdClickCheckBox.IsChecked = false;
            }

            if (ClickPatterns.Length == 4)
            {
                FourthClickCheckBox.IsChecked = true;
                if (ClickPatterns[3] == "L")
                {
                    FourthLeft.IsChecked = true;
                    FourthRight.IsChecked = false;
                }
                else
                {
                    FourthLeft.IsChecked = false;
                    FourthRight.IsChecked = true;
                }
            }
            else
            {
                FourthClickCheckBox.IsChecked = false;
            }

            if (((MainWindow)System.Windows.Application.Current.MainWindow).KoreanRadioButton.IsChecked.Value)
            {
                this.Title = "광고 닫기 클릭 패턴";
                FirstClickTextBlock.Text = "첫번째 클릭";
                SecondClickTextBox.Text = "두번째 클릭";
                ThirdClickCheckBox.Content = "세번째 클릭";
                FourthClickCheckBox.Content = "네번째 클릭";
                FirstLeft.Content = "왼쪽";
                FirstRight.Content = "오른쪽";
                SecondLeft.Content = "왼쪽";
                SecondRight.Content = "오른쪽";
                ThirdLeft.Content = "왼쪽";
                ThirdRight.Content = "오른쪽";
                FourthLeft.Content = "왼쪽";
                FourthRight.Content = "오른쪽";
                ApplyButton.Content = "적용";
                CancelButton.Content = "취소";
            }
        }

        private void ThirdClickCheckBox_Click(object sender, RoutedEventArgs e)
        {
            if (ThirdClickCheckBox.IsChecked.Value)
            {
                ThirdLeft.IsEnabled = true;
                ThirdRight.IsEnabled = true;
            }
            else
            {
                ThirdLeft.IsEnabled = false;
                ThirdRight.IsEnabled = false;

                FourthClickCheckBox.IsChecked = false;
                FourthLeft.IsEnabled = false;
                FourthRight.IsEnabled = false;
            }
        }

        private void FourthClickCheckBox_Click(object sender, RoutedEventArgs e)
        {
            if (FourthClickCheckBox.IsChecked.Value)
            {
                FourthLeft.IsEnabled = true;
                FourthRight.IsEnabled = true;
            }
            else
            {
                FourthLeft.IsEnabled = false;
                FourthRight.IsEnabled = false;
            }
        }

        private void ApplyButton_Click(object sender, RoutedEventArgs e)
        {
            string clickPattern = "";

            if (FirstLeft.IsChecked.Value)
            {
                clickPattern += "L;";
            }
            else
            {
                clickPattern += "R;";
            }

            if (SecondLeft.IsChecked.Value)
            {
                clickPattern += "L;";
            }
            else
            {
                clickPattern += "R;";
            }

            if (ThirdClickCheckBox.IsChecked.Value)
            {
                if (ThirdLeft.IsChecked.Value)
                {
                    clickPattern += "L;";
                }
                else
                {
                    clickPattern += "R;";
                }

                if (FourthClickCheckBox.IsChecked.Value)
                {
                    if (FourthLeft.IsChecked.Value)
                    {
                        clickPattern += "L";
                    }
                    else
                    {
                        clickPattern += "R";
                    }
                }
            }

            System.Windows.Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
            {
                ((MainWindow)System.Windows.Application.Current.MainWindow).ClickPattern = clickPattern;
            }));

            this.Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
