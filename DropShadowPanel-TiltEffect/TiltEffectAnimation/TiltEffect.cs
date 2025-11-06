using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace DropShadowPanel_TiltEffect.TiltEffectAnimation;

public static class TiltEffect
{
    #region IsPressed

    internal static bool GetIsPressed(FrameworkElement frameworkElement)
    {
        return (bool)frameworkElement.GetValue(IsPressedProperty);
    }

    internal static void SetIsPressed(FrameworkElement frameworkElement, bool value)
    {
        frameworkElement.SetValue(IsPressedProperty, value);
    }

    internal static readonly DependencyProperty IsPressedProperty =
        DependencyProperty.RegisterAttached(
            "IsPressed",
            typeof(bool),
            typeof(TiltEffect), new PropertyMetadata(false, OnIsPressedPropertyChanged));

    #endregion

    #region IsEnabled
    public static bool GetIsEnabled(FrameworkElement frameworkElement)
    {
        return (bool)frameworkElement.GetValue(IsEnabledProperty);
    }

    public static void SetIsEnabled(FrameworkElement frameworkElement, bool value)
    {
        frameworkElement.SetValue(IsEnabledProperty, value);
    }

    public static readonly DependencyProperty IsEnabledProperty =
        DependencyProperty.RegisterAttached(
            "IsEnabled",
            typeof(bool),
            typeof(TiltEffect), new PropertyMetadata(false));
    #endregion

    #region TiltFactor

    public static double GetTiltFactor(FrameworkElement frameworkElement)
    {
        return (double)frameworkElement.GetValue(TiltFactorProperty);
    }

    public static void SetTiltFactor(FrameworkElement frameworkElement, double value)
    {
        frameworkElement.SetValue(TiltFactorProperty, value);
    }

    public static readonly DependencyProperty TiltFactorProperty =
        DependencyProperty.RegisterAttached(
            "TiltFactor",
            typeof(double),
            typeof(TiltEffect), new PropertyMetadata(7.0));

    #endregion 

    private static Duration DefaultDuration = TimeSpan.FromMilliseconds(300.0);

    //private static double TiltFactor = 3.0;

    private static double Depth = 5.0;

    private static void OnIsPressedPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (DesignMode.DesignModeEnabled)
        {
            return;
        }

        var fe = d as FrameworkElement;
        if (fe == null) return;

        bool newValue = (bool)e.NewValue;
        bool oldValue = (bool)e.OldValue;

        if (oldValue && !newValue)
        {
            var RP_release = PlaneratorHelper.GetRotatorParent(fe);
            if (RP_release == null) return;

            PrepareForCompletion(fe, DefaultDuration.TimeSpan);
            SetAnim(RP_release, Planerator.RotationYProperty, 0);
            SetAnim(RP_release, Planerator.RotationXProperty, 0);
            SetAnim(RP_release, Planerator.DepthProperty, 0);
            return;
        }

        if (!oldValue && newValue)
        {
            if (!GetIsEnabled(fe)) return;

            // Zabalit do Planeratoru
            fe.BeginAnimation(PlaneratorHelper.PlaceIn3DProperty, null);
            PlaneratorHelper.SetPlaceIn3D(fe, true);

            var RP = PlaneratorHelper.GetRotatorParent(fe);
            if (RP == null) return;

            // Pokud ještě nemá rozměry, vynutíme layout, aby RP.ActualWidth/Height nebyly 0.
            if (RP.ActualWidth == 0 || RP.ActualHeight == 0)
            {
                // Pro jistotu synchronně:
                RP.UpdateLayout();
            }

            // Měříme a normalizujeme vůči vizuálu, který je skutečně mapován do 3D (RP.Child)
            var refVisual = RP.Child as FrameworkElement ?? fe;

            Point current = Mouse.GetPosition(refVisual);
            double width = refVisual.ActualWidth > 0 ? refVisual.ActualWidth : fe.ActualWidth;
            double height = refVisual.ActualHeight > 0 ? refVisual.ActualHeight : fe.ActualHeight;

            double tilt = GetTiltFactor(fe);

            bool pressed = Mouse.LeftButton == MouseButtonState.Pressed;
            bool inside = width > 0 && height > 0 &&
                           current.X >= 0 && current.X <= width &&
                           current.Y >= 0 && current.Y <= height;

            if (inside && pressed)
            {
                double yrot = -tilt + current.X * 2 * tilt / width;
                double xrot = -tilt + current.Y * 2 * tilt / height;
                SetAnim(RP, Planerator.RotationYProperty, yrot);
                SetAnim(RP, Planerator.RotationXProperty, xrot);
            }

            SetAnim(RP, Planerator.DepthProperty, Depth);
        }
    }

    private static void PrepareForCompletion(FrameworkElement fe, TimeSpan timeSpan)
    {
        if (DesignMode.DesignModeEnabled)
        {
            return;
        }

        var bauk = new BooleanAnimationUsingKeyFrames() { BeginTime = timeSpan, Duration = TimeSpan.FromMilliseconds(10) };
        bauk.KeyFrames.Add(new DiscreteBooleanKeyFrame(false, KeyTime.FromTimeSpan(TimeSpan.Zero)));
        fe.BeginAnimation(PlaneratorHelper.PlaceIn3DProperty, bauk);
    }

    private static void SetAnim(Planerator pl, DependencyProperty dp, double value)
    {
        if (DesignMode.DesignModeEnabled)
        {
            return;
        }

        DoubleAnimation da = new DoubleAnimation(value, DefaultDuration)
        {
            EasingFunction = new CircleEase() { EasingMode = EasingMode.EaseOut }
        };
        pl.BeginAnimation(dp, da);
    }
}