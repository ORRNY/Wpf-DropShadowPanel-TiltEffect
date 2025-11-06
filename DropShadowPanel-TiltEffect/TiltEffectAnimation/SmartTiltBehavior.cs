using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Media3D;

namespace DropShadowPanel_TiltEffect.TiltEffectAnimation;

/// <summary>
/// Inteligentní tilt behavior, který automaticky volí nejlepší přístup
/// podle typu elementu a jeho vlastností.
/// </summary>
public static class SmartTiltBehavior
{
    #region Attached Properties

    public static readonly DependencyProperty EnableProperty =
        DependencyProperty.RegisterAttached(
            "Enable",
            typeof(bool),
            typeof(SmartTiltBehavior),
            new PropertyMetadata(false, OnEnableChanged));

    public static readonly DependencyProperty StrategyProperty =
        DependencyProperty.RegisterAttached(
            "Strategy",
            typeof(TiltStrategy),
            typeof(SmartTiltBehavior),
            new PropertyMetadata(TiltStrategy.Auto));

    public static readonly DependencyProperty TiltFactorProperty =
        DependencyProperty.RegisterAttached(
            "TiltFactor",
            typeof(double),
            typeof(SmartTiltBehavior),
            new PropertyMetadata(7.0));

    public static readonly DependencyProperty PreserveShadowProperty =
        DependencyProperty.RegisterAttached(
            "PreserveShadow",
            typeof(bool),
            typeof(SmartTiltBehavior),
            new PropertyMetadata(true));

    #endregion

    #region Enums

    public enum TiltStrategy
    {
        /// <summary>
        /// Automaticky zvolit nejlepší strategii
        /// </summary>
        Auto,

        /// <summary>
        /// Stack Overflow přístup - pevné rozměry s přímým efektem
        /// </summary>
        FixedSize,

        /// <summary>
        /// Oddělené vrstvy - shadow a content zvlášť
        /// </summary>
        LayeredShadow,

        /// <summary>
        /// Planerator s kompenzací bounds
        /// </summary>
        CompensatedPlanerator,

        /// <summary>
        /// Bez stínu - pouze tilt
        /// </summary>
        NoShadow
    }

    #endregion

    #region Property Accessors

    public static bool GetEnable(DependencyObject obj)
        => (bool)obj.GetValue(EnableProperty);

    public static void SetEnable(DependencyObject obj, bool value)
        => obj.SetValue(EnableProperty, value);

    public static TiltStrategy GetStrategy(DependencyObject obj)
        => (TiltStrategy)obj.GetValue(StrategyProperty);

    public static void SetStrategy(DependencyObject obj, TiltStrategy value)
        => obj.SetValue(StrategyProperty, value);

    public static double GetTiltFactor(DependencyObject obj)
        => (double)obj.GetValue(TiltFactorProperty);

    public static void SetTiltFactor(DependencyObject obj, double value)
        => obj.SetValue(TiltFactorProperty, value);

    public static bool GetPreserveShadow(DependencyObject obj)
        => (bool)obj.GetValue(PreserveShadowProperty);

    public static void SetPreserveShadow(DependencyObject obj, bool value)
        => obj.SetValue(PreserveShadowProperty, value);

    #endregion

    #region Implementation

    private static void OnEnableChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FrameworkElement element)
        {
            if ((bool)e.NewValue)
            {
                ApplySmartTilt(element);
            }
            else
            {
                RemoveSmartTilt(element);
            }
        }
    }

    private static void ApplySmartTilt(FrameworkElement element)
    {
        var strategy = GetStrategy(element);

        if (strategy == TiltStrategy.Auto)
        {
            strategy = DetermineOptimalStrategy(element);
            // Uložit zvolenou strategii pro debugging
            element.Tag = $"SmartTilt:{strategy}";
        }

        switch (strategy)
        {
            case TiltStrategy.FixedSize:
                ApplyFixedSizeStrategy(element);
                break;

            case TiltStrategy.LayeredShadow:
                ApplyLayeredShadowStrategy(element);
                break;

            case TiltStrategy.CompensatedPlanerator:
                ApplyCompensatedPlaneratorStrategy(element);
                break;

            case TiltStrategy.NoShadow:
                ApplyNoShadowStrategy(element);
                break;
        }
    }

    private static TiltStrategy DetermineOptimalStrategy(FrameworkElement element)
    {
        // Analyzovat element a jeho vlastnosti
        bool hasFixedSize = !double.IsNaN(element.Width) && !double.IsNaN(element.Height);
        bool hasDropShadow = element.Effect is DropShadowEffect;
        bool isButton = element is Button || element is ToggleButton;
        bool isInItemsControl = ItemsControl.ItemsControlFromItemContainer(element) != null;
        bool hasComplexContent = VisualTreeHelper.GetChildrenCount(element) > 3;

        // Rozhodovací logika
        if (hasFixedSize && !hasComplexContent)
        {
            // Pevné rozměry a jednoduchý obsah -> Stack Overflow přístup
            return TiltStrategy.FixedSize;
        }
        else if (hasDropShadow && GetPreserveShadow(element))
        {
            if (isButton && !isInItemsControl)
            {
                // Samostatné tlačítko se stínem -> oddělené vrstvy
                return TiltStrategy.LayeredShadow;
            }
            else if (hasComplexContent)
            {
                // Komplexní obsah -> kompenzovaný planerator
                return TiltStrategy.CompensatedPlanerator;
            }
        }
        else if (!hasDropShadow)
        {
            // Bez stínu -> jednoduchý tilt
            return TiltStrategy.NoShadow;
        }

        // Default: oddělené vrstvy (nejbezpečnější)
        return TiltStrategy.LayeredShadow;
    }

    #region Strategy Implementations

    private static void ApplyFixedSizeStrategy(FrameworkElement element)
    {
        // Implementace Stack Overflow přístupu
        if (double.IsNaN(element.Width) || double.IsNaN(element.Height))
        {
            // Nastavit pevné rozměry podle ActualSize
            element.Width = element.ActualWidth > 0 ? element.ActualWidth : 100;
            element.Height = element.ActualHeight > 0 ? element.ActualHeight : 40;
        }

        // Najít parent panel
        var parent = element.Parent as Panel;
        if (parent == null) return;

        // Zapamatovat si index a odpojit element z Visual tree, než ho použijeme jako Visual
        int index = parent.Children.IndexOf(element);
        parent.Children.Remove(element);

        // Až teď vytvořit Viewport3D, který nastaví Visual = element
        var viewport = CreateFixedSizeViewport3D(element);

        // Vložit zpět na původní pozici
        parent.Children.Insert(index, viewport);

        // Přidat event handlery
        AttachTiltHandlers(viewport, element);
    }

    private static void ApplyLayeredShadowStrategy(FrameworkElement element)
    {
        // Implementace oddělených vrstev
        var parent = element.Parent as Panel;
        if (parent == null) return;

        // Uložit layout vlastnosti
        var width = element.Width;
        var height = element.Height;
        var hAlign = element.HorizontalAlignment;
        var vAlign = element.VerticalAlignment;
        var margin = element.Margin;

        // Zapamatovat si index a odpojit element z původního parenta dříve,
        // než ho přidáme do nového kontejneru
        int index = parent.Children.IndexOf(element);
        parent.Children.Remove(element);

        // Vytvořit Grid kontejner
        var container = new Grid
        {
            Width = width,
            Height = height,
            HorizontalAlignment = hAlign,
            VerticalAlignment = vAlign,
            Margin = margin
        };

        // Shadow layer (kopíruje tvar elementu)
        var shadowBorder = new Border
        {
            Background = new SolidColorBrush(Colors.Black) { Opacity = 0.01 },
            CornerRadius = element is Border b ? b.CornerRadius : new CornerRadius(0)
        };

        // Přesunout efekt na shadow layer
        if (element.Effect != null)
        {
            shadowBorder.Effect = element.Effect;
            element.Effect = null;
        }
        else
        {
            // Výchozí stín
            shadowBorder.Effect = new DropShadowEffect
            {
                BlurRadius = 15,
                ShadowDepth = 5,
                Direction = 270,
                Color = Colors.Black,
                Opacity = 0.3
            };
        }

        // Přidat vrstvy do kontejneru
        container.Children.Add(shadowBorder);

        // Reset element properties pro správné zarovnání uvnitř kontejneru
        element.Margin = new Thickness(0);

        container.Children.Add(element);

        // Nahradit v parent na původní pozici
        parent.Children.Insert(index, container);

        // Aplikovat tilt pouze na content
        TiltEffect.SetIsEnabled(element, true);
        TiltEffect.SetTiltFactor(element, GetTiltFactor(container));
    }

    /*private static void ApplyFixedSizeStrategy(FrameworkElement element)
    {
        // Implementace Stack Overflow přístupu
        if (double.IsNaN(element.Width) || double.IsNaN(element.Height))
        {
            // Nastavit pevné rozměry podle ActualSize
            element.Width = element.ActualWidth > 0 ? element.ActualWidth : 100;
            element.Height = element.ActualHeight > 0 ? element.ActualHeight : 40;
        }

        // Zabalit do Viewport3D s pevnými rozměry
        var parent = element.Parent as Panel;
        if (parent == null) return;

        var viewport = CreateFixedSizeViewport3D(element);
        int index = parent.Children.IndexOf(element);
        parent.Children.Remove(element);
        parent.Children.Insert(index, viewport);

        // Přidat event handlery
        AttachTiltHandlers(viewport, element);
    }

    private static void ApplyLayeredShadowStrategy(FrameworkElement element)
    {
        // Implementace oddělených vrstev
        var parent = element.Parent as Panel;
        if (parent == null) return;

        // Vytvořit Grid kontejner
        var container = new Grid
        {
            Width = element.Width,
            Height = element.Height,
            HorizontalAlignment = element.HorizontalAlignment,
            VerticalAlignment = element.VerticalAlignment,
            Margin = element.Margin
        };

        // Shadow layer (kopíruje tvar elementu)
        var shadowBorder = new Border
        {
            Background = new SolidColorBrush(Colors.Black) { Opacity = 0.01 },
            CornerRadius = element is Border b ? b.CornerRadius : new CornerRadius(0)
        };

        // Přesunout efekt na shadow layer
        if (element.Effect != null)
        {
            shadowBorder.Effect = element.Effect;
            element.Effect = null;
        }
        else
        {
            // Výchozí stín
            shadowBorder.Effect = new DropShadowEffect
            {
                BlurRadius = 15,
                ShadowDepth = 5,
                Direction = 270,
                Color = Colors.Black,
                Opacity = 0.3
            };
        }

        // Přidat vrstvy do kontejneru
        container.Children.Add(shadowBorder);

        // Reset element properties
        element.Margin = new Thickness(0);
        container.Children.Add(element);

        // Nahradit v parent
        int index = parent.Children.IndexOf(element);
        parent.Children.Remove(element);
        parent.Children.Insert(index, container);

        // Aplikovat tilt pouze na content
        TiltEffect.SetIsEnabled(element, true);
        TiltEffect.SetTiltFactor(element, GetTiltFactor(container));
    }*/

    private static void ApplyCompensatedPlaneratorStrategy(FrameworkElement element)
    {
        // Použít vylepšený Planerator s kompenzací
        PlaneratorHelper.SetPlaceIn3D(element, false); // Reset

        // Nastavit kompenzaci
        if (element.Effect is DropShadowEffect dropShadow)
        {
            // Vypočítat kompenzační offset
            double blurCompensation = dropShadow.BlurRadius / 2.0;
            PlaneratorHelper.SetOriginX(element, 0.5 - (blurCompensation / element.ActualWidth));
            PlaneratorHelper.SetOriginY(element, 0.5 - (blurCompensation / element.ActualHeight));
        }

        // Aktivovat tilt
        TiltEffect.SetIsEnabled(element, true);
        TiltEffect.SetTiltFactor(element, GetTiltFactor(element));
    }

    private static void ApplyNoShadowStrategy(FrameworkElement element)
    {
        // Odstranit efekt a použít standardní tilt
        element.Effect = null;

        TiltEffect.SetIsEnabled(element, true);
        TiltEffect.SetTiltFactor(element, GetTiltFactor(element));
    }

    #endregion

    #region Helper Methods

    private static Viewport3D CreateFixedSizeViewport3D(FrameworkElement element)
    {
        var viewport = new Viewport3D
        {
            Width = element.Width,
            Height = element.Height,
            HorizontalAlignment = element.HorizontalAlignment,
            VerticalAlignment = element.VerticalAlignment,
            Margin = element.Margin,
            ClipToBounds = false
        };

        // Kamera
        viewport.Camera = new PerspectiveCamera
        {
            Position = new Point3D(0, 0, 4),
            LookDirection = new Vector3D(0, 0, -1),
            UpDirection = new Vector3D(0, 1, 0),
            FieldOfView = 45
        };

        // Viewport2DVisual3D
        var visual3D = new Viewport2DVisual3D
        {
            Geometry = CreateMeshGeometry(),
            Visual = element,
            Material = new DiffuseMaterial { Brush = Brushes.White }
        };
        visual3D.Material.SetValue(Viewport2DVisual3D.IsVisualHostMaterialProperty, true);

        // Transform group pro animace
        var transformGroup = new Transform3DGroup();
        transformGroup.Children.Add(new RotateTransform3D(
            new AxisAngleRotation3D(new Vector3D(1, 0, 0), 0))
        { /* X rotation */ });
        transformGroup.Children.Add(new RotateTransform3D(
            new AxisAngleRotation3D(new Vector3D(0, 1, 0), 0))
        { /* Y rotation */ });
        visual3D.Transform = transformGroup;

        viewport.Children.Add(visual3D);

        // Světlo
        viewport.Children.Add(new ModelVisual3D
        {
            Content = new DirectionalLight
            {
                Color = Colors.White,
                Direction = new Vector3D(0, 0, -1)
            }
        });

        return viewport;
    }

    private static MeshGeometry3D CreateMeshGeometry()
    {
        return new MeshGeometry3D
        {
            Positions = new Point3DCollection
            {
                new Point3D(-1, 1, 0),
                new Point3D(-1, -1, 0),
                new Point3D(1, -1, 0),
                new Point3D(1, 1, 0)
            },
            TextureCoordinates = new PointCollection
            {
                new Point(0, 0),
                new Point(0, 1),
                new Point(1, 1),
                new Point(1, 0)
            },
            TriangleIndices = new Int32Collection { 0, 1, 2, 0, 2, 3 }
        };
    }

    private static void AttachTiltHandlers(Viewport3D viewport, FrameworkElement originalElement)
    {
        viewport.PreviewMouseLeftButtonDown += (s, e) =>
        {
            AnimateTilt(viewport, true, e.GetPosition(viewport));
        };

        viewport.PreviewMouseLeftButtonUp += (s, e) =>
        {
            AnimateTilt(viewport, false, null);
        };

        viewport.MouseLeave += (s, e) =>
        {
            AnimateTilt(viewport, false, null);
        };
    }

    private static void AnimateTilt(Viewport3D viewport, bool isPressed, Point? mousePosition)
    {
        var visual3D = viewport.Children[0] as Viewport2DVisual3D;
        if (visual3D?.Transform is Transform3DGroup group && group.Children.Count >= 2)
        {
            double targetX = 0, targetY = 0;

            if (isPressed && mousePosition.HasValue)
            {
                // Vypočítat tilt podle pozice myši
                var pos = mousePosition.Value;
                double normalizedX = (pos.X / viewport.Width - 0.5) * 2;
                double normalizedY = (pos.Y / viewport.Height - 0.5) * 2;

                targetY = normalizedX * 10; // Horizontální tilt
                targetX = -normalizedY * 10; // Vertikální tilt
            }

            // Animovat rotace
            if (group.Children[0] is RotateTransform3D rotX &&
                rotX.Rotation is AxisAngleRotation3D angleX)
            {
                angleX.BeginAnimation(AxisAngleRotation3D.AngleProperty,
                    new DoubleAnimation(targetX, TimeSpan.FromMilliseconds(100))
                    {
                        EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
                    });
            }

            if (group.Children[1] is RotateTransform3D rotY &&
                rotY.Rotation is AxisAngleRotation3D angleY)
            {
                angleY.BeginAnimation(AxisAngleRotation3D.AngleProperty,
                    new DoubleAnimation(targetY, TimeSpan.FromMilliseconds(100))
                    {
                        EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
                    });
            }
        }
    }

    private static void RemoveSmartTilt(FrameworkElement element)
    {
        // Odstranit event handlery a obnovit původní stav
        TiltEffect.SetIsEnabled(element, false);
        PlaneratorHelper.SetPlaceIn3D(element, false);

        // Pokud je element ve Viewport3D, extrahovat ho zpět
        if (element.Parent is Viewport2DVisual3D)
        {
            // TODO: Implementovat extrakci z Viewport3D
        }
    }

    #endregion

    #endregion
}