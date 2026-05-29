using System;
using System.Windows;
using System.Windows.Media.Animation;
using System.Windows.Media;
using System.Windows.Controls;

namespace CybersecurityChatbot.Behaviors
{
    /// <summary>
    /// Attached behavior to spin a UIElement using a RotateTransform.
    /// The storyboard is created in code but the behavior is attached from XAML,
    /// keeping MainWindow.xaml.cs untouched.
    /// </summary>
    public static class SpinBehavior
    {
        public static readonly DependencyProperty EnableSpinProperty = DependencyProperty.RegisterAttached(
            "EnableSpin",
            typeof(bool),
            typeof(SpinBehavior),
            new PropertyMetadata(false, OnEnableSpinChanged));

        public static readonly DependencyProperty SpinsProperty = DependencyProperty.RegisterAttached(
            "Spins",
            typeof(int),
            typeof(SpinBehavior),
            new PropertyMetadata(1));

        public static readonly DependencyProperty DelayMinutesProperty = DependencyProperty.RegisterAttached(
            "DelayMinutes",
            typeof(double),
            typeof(SpinBehavior),
            new PropertyMetadata(0.0));

        public static readonly DependencyProperty DurationPerSpinSecondsProperty = DependencyProperty.RegisterAttached(
            "DurationPerSpinSeconds",
            typeof(double),
            typeof(SpinBehavior),
            new PropertyMetadata(1.0));

        public static bool GetEnableSpin(DependencyObject obj) => (bool)obj.GetValue(EnableSpinProperty);
        public static void SetEnableSpin(DependencyObject obj, bool value) => obj.SetValue(EnableSpinProperty, value);

        public static int GetSpins(DependencyObject obj) => (int)obj.GetValue(SpinsProperty);
        public static void SetSpins(DependencyObject obj, int value) => obj.SetValue(SpinsProperty, value);

        public static double GetDelayMinutes(DependencyObject obj) => (double)obj.GetValue(DelayMinutesProperty);
        public static void SetDelayMinutes(DependencyObject obj, double value) => obj.SetValue(DelayMinutesProperty, value);

        public static double GetDurationPerSpinSeconds(DependencyObject obj) => (double)obj.GetValue(DurationPerSpinSecondsProperty);
        public static void SetDurationPerSpinSeconds(DependencyObject obj, double value) => obj.SetValue(DurationPerSpinSecondsProperty, value);

        private static void OnEnableSpinChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is FrameworkElement fe))
                return;

            if ((bool)e.NewValue)
            {
                // Attach once the element is loaded
                RoutedEventHandler loaded = null;
                loaded = (s, ev) =>
                {
                    fe.Loaded -= loaded;
                    StartSpin(fe);
                };

                if (fe.IsLoaded)
                    StartSpin(fe);
                else
                    fe.Loaded += loaded;
            }
        }

        private static void StartSpin(FrameworkElement fe)
        {
            // Ensure RenderTransform is a RotateTransform
            if (!(fe.RenderTransform is RotateTransform))
            {
                fe.RenderTransform = new RotateTransform(0);
            }

            // Default center origin if not set
            if (fe.RenderTransformOrigin == new System.Windows.Point(0, 0))
            {
                fe.RenderTransformOrigin = new System.Windows.Point(0.5, 0.5);
            }

            int spins = GetSpins(fe);
            double delayMinutes = GetDelayMinutes(fe);
            double perSpinSeconds = GetDurationPerSpinSeconds(fe);

            var animation = new DoubleAnimation
            {
                From = 0,
                To = 360,
                Duration = TimeSpan.FromSeconds(perSpinSeconds),
                RepeatBehavior = new RepeatBehavior(spins),
                BeginTime = TimeSpan.FromMinutes(delayMinutes),
                FillBehavior = FillBehavior.Stop
            };

            var sb = new Storyboard();
            Storyboard.SetTarget(animation, fe);
            Storyboard.SetTargetProperty(animation, new PropertyPath("(UIElement.RenderTransform).(RotateTransform.Angle)"));
            sb.Children.Add(animation);

            // Start the storyboard; it will run after the specified BeginTime
            sb.Begin(fe, true);
        }
    }
}
