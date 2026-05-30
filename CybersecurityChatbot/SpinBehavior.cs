using System;
using System.Windows;
using System.Windows.Media.Animation;
using System.Windows.Media;
using System.Windows.Threading;

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
            new PropertyMetadata(false, OnBehaviorChanged));

        public static readonly DependencyProperty BlinkIntervalSecondsProperty = DependencyProperty.RegisterAttached(
            "BlinkIntervalSeconds",
            typeof(double),
            typeof(SpinBehavior),
            new PropertyMetadata(2.0, OnTimingChanged));

        public static readonly DependencyProperty SpinIntervalSecondsProperty = DependencyProperty.RegisterAttached(
            "SpinIntervalSeconds",
            typeof(double),
            typeof(SpinBehavior),
            new PropertyMetadata(3.0, OnTimingChanged));

        public static readonly DependencyProperty SpinDurationSecondsProperty = DependencyProperty.RegisterAttached(
            "SpinDurationSeconds",
            typeof(double),
            typeof(SpinBehavior),
            new PropertyMetadata(0.8, OnTimingChanged));

        private static readonly DependencyProperty StateProperty = DependencyProperty.RegisterAttached(
            "State",
            typeof(SpinState),
            typeof(SpinBehavior),
            new PropertyMetadata(null));

        public static bool GetEnableSpin(DependencyObject obj) => (bool)obj.GetValue(EnableSpinProperty);
        public static void SetEnableSpin(DependencyObject obj, bool value) => obj.SetValue(EnableSpinProperty, value);

        public static double GetBlinkIntervalSeconds(DependencyObject obj) => (double)obj.GetValue(BlinkIntervalSecondsProperty);
        public static void SetBlinkIntervalSeconds(DependencyObject obj, double value) => obj.SetValue(BlinkIntervalSecondsProperty, value);

        public static double GetSpinIntervalSeconds(DependencyObject obj) => (double)obj.GetValue(SpinIntervalSecondsProperty);
        public static void SetSpinIntervalSeconds(DependencyObject obj, double value) => obj.SetValue(SpinIntervalSecondsProperty, value);

        public static double GetSpinDurationSeconds(DependencyObject obj) => (double)obj.GetValue(SpinDurationSecondsProperty);
        public static void SetSpinDurationSeconds(DependencyObject obj, double value) => obj.SetValue(SpinDurationSecondsProperty, value);

        private static void OnBehaviorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not FrameworkElement element)
                return;

            if ((bool)e.NewValue)
            {
                element.Loaded += Element_Loaded;
                element.Unloaded += Element_Unloaded;

                if (element.IsLoaded)
                    Start(element);
            }
            else
            {
                element.Loaded -= Element_Loaded;
                element.Unloaded -= Element_Unloaded;
                Stop(element);
            }
        }

        private static void OnTimingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is FrameworkElement element && GetEnableSpin(element) && element.IsLoaded)
            {
                Stop(element);
                Start(element);
            }
        }

        private static void Element_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement element && GetEnableSpin(element))
                Start(element);
        }

        private static void Element_Unloaded(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement element)
                Stop(element);
        }

        private static void Start(FrameworkElement element)
        {
            if (GetState(element) != null)
                return;

            if (element.RenderTransform is not RotateTransform)
                element.RenderTransform = new RotateTransform(0);

            element.RenderTransformOrigin = new Point(0.5, 0.5);

            var state = new SpinState(element)
            {
                BlinkInterval = TimeSpan.FromSeconds(GetBlinkIntervalSeconds(element)),
                SpinInterval = TimeSpan.FromSeconds(GetSpinIntervalSeconds(element)),
                SpinDuration = TimeSpan.FromSeconds(GetSpinDurationSeconds(element))
            };

            state.BlinkTimer.Interval = state.BlinkInterval;
            state.SpinTimer.Interval = state.SpinInterval;

            state.BlinkTimer.Tick += (_, __) => Blink(element);
            state.SpinTimer.Tick += (_, __) => Spin(element, state.SpinDuration);

            SetState(element, state);
            state.BlinkTimer.Start();
            state.SpinTimer.Start();
        }

        private static void Stop(FrameworkElement element)
        {
            if (GetState(element) is not SpinState state)
                return;

            state.BlinkTimer.Stop();
            state.SpinTimer.Stop();
            SetState(element, null);
        }

        private static void Blink(FrameworkElement element)
        {
            AnimateEye(element.FindName("LeftEye") as FrameworkElement);
            AnimateEye(element.FindName("RightEye") as FrameworkElement);
        }

        private static void AnimateEye(FrameworkElement eye)
        {
            if (eye == null)
                return;

            var blink = new DoubleAnimation
            {
                To = 2,
                Duration = TimeSpan.FromMilliseconds(120),
                AutoReverse = true,
                FillBehavior = FillBehavior.Stop
            };

            eye.BeginAnimation(FrameworkElement.HeightProperty, blink);
        }

        private static void Spin(FrameworkElement element, TimeSpan duration)
        {
            if (element.RenderTransform is not RotateTransform rotateTransform)
            {
                rotateTransform = new RotateTransform(0);
                element.RenderTransform = rotateTransform;
            }

            var spin = new DoubleAnimation
            {
                From = 0,
                To = 360,
                Duration = duration,
                FillBehavior = FillBehavior.Stop
            };

            rotateTransform.BeginAnimation(RotateTransform.AngleProperty, spin);
        }

        private static SpinState GetState(DependencyObject obj) => (SpinState)obj.GetValue(StateProperty);
        private static void SetState(DependencyObject obj, SpinState value) => obj.SetValue(StateProperty, value);

        private sealed class SpinState
        {
            public SpinState(FrameworkElement element)
            {
                BlinkTimer = new DispatcherTimer();
                SpinTimer = new DispatcherTimer();
            }

            public DispatcherTimer BlinkTimer { get; }
            public DispatcherTimer SpinTimer { get; }
            public TimeSpan BlinkInterval { get; set; }
            public TimeSpan SpinInterval { get; set; }
            public TimeSpan SpinDuration { get; set; }
        }
    }
}
