using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ViKa_Ginger.ViewModel;

namespace ViKa_Ginger
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        MainViewModel viewModel;
        

        public MainWindow()
        {
            InitializeComponent();

            DoubleAnimation da = new DoubleAnimation();



            da.From = 0;
            da.To = 1;
            da.Duration = TimeSpan.FromSeconds(4);
            da.EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn };
            general_page_button.BeginAnimation(Button.OpacityProperty, da);
            settings_page_button.BeginAnimation(Button.OpacityProperty, da);
            da.From = 100;
            da.To = 0;
            da.Duration = TimeSpan.FromSeconds(4);
            da.EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut };
            general_page_button_translate.BeginAnimation(TranslateTransform.YProperty,da);
            settings_page_button_translate.BeginAnimation(TranslateTransform.YProperty, da);

            viewModel = new MainViewModel();
            DataContext = viewModel;

        }

    }
}