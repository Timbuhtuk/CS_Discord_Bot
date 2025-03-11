using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using System.Windows.Media.Animation;
using System.Windows.Media;

namespace ViKa_Ginger.ViewModel
{
    public class MainViewModel : INotifyPropertyChanged
    {
        
        public MainViewModel() {
            ViewWidth = 800;
            ViewHeight = 800;
        }


        public event PropertyChangedEventHandler? PropertyChanged;


        int viewWidth;
        public int ViewWidth { get { return viewWidth; } set { viewWidth = value; NotifyProperyChanged(); } }

        int viewHeight;
        public int ViewHeight { get { return viewHeight; } set { viewHeight = value; NotifyProperyChanged(); } }



        private ICommand? сhangeScreenToGeneral;
        public ICommand? ChangeScreenToGeneral => сhangeScreenToGeneral ??= new BaseCommand(
                param =>
                {
                    DoubleAnimation da = new DoubleAnimation();
                    var targetElement = Application.Current.MainWindow.FindName("TopMenuSlideBarX") as TranslateTransform;

                    da.From = targetElement.X;
                    da.To = -180;
                    da.Duration = TimeSpan.FromSeconds(0.7);
                    da.EasingFunction = new CubicEase { EasingMode = EasingMode.EaseInOut };
                    targetElement.BeginAnimation(TranslateTransform.XProperty, da);

                }
            );
        private ICommand? сhangeScreenToSettings;
        public ICommand? ChangeScreenToSettings => сhangeScreenToSettings ??= new BaseCommand(
                param =>
                {
                    DoubleAnimation da = new DoubleAnimation();
                    var targetElement = Application.Current.MainWindow.FindName("TopMenuSlideBarX") as TranslateTransform;

                    da.From = targetElement.X;
                    da.To = -90;
                    da.Duration = TimeSpan.FromSeconds(0.7);
                    da.EasingFunction = new CubicEase { EasingMode = EasingMode.EaseInOut };
                    targetElement.BeginAnimation(TranslateTransform.XProperty, da);

                }
            );

        public void NotifyProperyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
    internal class BaseCommand : ICommand
    {
        Predicate<object>? canExecuteMethod;
        Action<object>? executeMethod;
        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter)
        {
            return canExecuteMethod == null || canExecuteMethod(parameter);
        }
        public BaseCommand(Action<object>? executeMethod, Predicate<object>? canExecuteMethod = null)
        {
            this.canExecuteMethod = canExecuteMethod;
            this.executeMethod = executeMethod;
        }


        public void Execute(object? parameter)
        {
            executeMethod?.Invoke(parameter);
        }
    }
}
