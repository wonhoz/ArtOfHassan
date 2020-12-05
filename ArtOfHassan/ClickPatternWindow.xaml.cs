using System;
using System.Collections.Generic;
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
    /// ClickPatternWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ClickPatternWindow : Window
    {
        public ClickPatternWindow()
        {
            InitializeComponent();
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
