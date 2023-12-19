using System.Windows;

namespace WizMes_HanYoung.PopUp
{
    /// <summary>
    /// ScreenShot.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ScreenShot : Window
    {
        public ScreenShot()
        {
            InitializeComponent();
        }

        private void ScreenShot_Loaded(object sender, RoutedEventArgs e)
        {

            //품명별 불량분석 화면에서 보낸 image소스를 받는다.
            if (MainWindow.ScreenCapture != null && MainWindow.ScreenCapture.Count > 0)
            {
                //받아서 IMAGEDATA에 넣었는데 과연 나올까??
                ImageData.Source = MainWindow.ScreenCapture[0].Source;
            }
        }
    }
}
