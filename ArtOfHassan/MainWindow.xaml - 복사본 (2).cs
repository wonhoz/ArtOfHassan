using System;
using System.Runtime.InteropServices;
using System.Windows;

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

        int Stage = 0;
        System.Drawing.Bitmap LastBitmap;
        System.Drawing.Bitmap CurrentBitmap;


        public MainWindow()
        {
            InitializeComponent();

            ButtonTimer.Interval = 1000; // 1초
            ButtonTimer.Elapsed += ButtonTimerFunction;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            ButtonTimer.Enabled = false;
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
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
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

        private void ButtonTimerFunction(object sender, System.Timers.ElapsedEventArgs e)
        {
            CurrentBitmap = Screenshot();

            // Battle Button
            System.Drawing.Color color = CurrentBitmap.GetPixel(180, 855);
            if ((color.R == 253) && (color.G == 187) && (color.B == 0))
            {
                Stage = 1;
                System.Windows.Forms.Cursor.Position = new System.Drawing.Point((int)NoxPointX + 180, (int)NoxPointY + 855);
                mouse_event(LBUTTONDOWN, 0, 0, 0, 0);
                System.Threading.Thread.Sleep(50);
                mouse_event(LBUTTONUP, 0, 0, 0, 0);
            }

            // Level Button
            color = CurrentBitmap.GetPixel(180, 870);
            if ((color.R == 253) && (color.G == 187) && (color.B == 0))
            {
                Stage = 2;
                System.Windows.Forms.Cursor.Position = new System.Drawing.Point((int)NoxPointX + 180, (int)NoxPointY + 870);
                mouse_event(LBUTTONDOWN, 0, 0, 0, 0);
                System.Threading.Thread.Sleep(50);
                mouse_event(LBUTTONUP, 0, 0, 0, 0);
            }

            // Three Times Coin Button
            color = CurrentBitmap.GetPixel(115, 685);
            if ((color.R == 124) && (color.G == 155) && (color.B == 5)) // Green
            {
                Stage = 3;
                System.Windows.Forms.Cursor.Position = new System.Drawing.Point((int)NoxPointX + 115, (int)NoxPointY + 685);
                mouse_event(LBUTTONDOWN, 0, 0, 0, 0);
                System.Threading.Thread.Sleep(50);
                mouse_event(LBUTTONUP, 0, 0, 0, 0);
            }
            else if ((color.R == 142) && (color.G == 142) && (color.B == 142)) // Gray
            {
                Stage = 4;
                System.Windows.Forms.Cursor.Position = new System.Drawing.Point((int)NoxPointX + 280, (int)NoxPointY + 685);
                mouse_event(LBUTTONDOWN, 0, 0, 0, 0);
                System.Threading.Thread.Sleep(50);
                mouse_event(LBUTTONUP, 0, 0, 0, 0);
            }

            //CurrentBitmap.Save(Stage + ".png", System.Drawing.Imaging.ImageFormat.Png);
            LastBitmap = CurrentBitmap;
        }

        private System.Drawing.Bitmap Screenshot()
        {
            // 화면 크기만큼의 Bitmap 생성
            System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap((int)NoxWidth, (int)NoxHeight, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            // Bitmap 이미지 변경을 위해 Graphics 객체 생성
            System.Drawing.Graphics graphics = System.Drawing.Graphics.FromImage(bitmap);
            // 화면을 그대로 카피해서 Bitmap 메모리에 저장
            graphics.CopyFromScreen((int)NoxPointX, (int)NoxPointY, 0, 0, bitmap.Size);
            // Bitmap 데이타를 파일로 저장
            //bitmap.Save(imageName + ".png", System.Drawing.Imaging.ImageFormat.Png);
            return bitmap;
        }
    }
}
