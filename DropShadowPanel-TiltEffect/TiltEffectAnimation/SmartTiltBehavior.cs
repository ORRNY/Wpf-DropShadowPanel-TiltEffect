using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Media3D;

namespace DropShadowPanel_TiltEffect.TiltEffectAnimation;

/// <summary>
/// Intelligent tilt behavior that automatically chooses the best approach
/// based on the element type and its properties.
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
        /// Automatically choose the best strategy
        /// </summary>
        Auto,

        /// <summary>
        /// Stack Overflow approach — fixed dimensions with a direct effect
        /// </summary>
        FixedSize,

        /// <summary>
        /// Separated layers — shadow and content separately
        /// </summary>
        LayeredShadow,

        /// <summary>
        /// Planerator with bounds compensation
        /// </summary>
        CompensatedPlanerator,

        /// <summary>
        /// No shadow — tilt only
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
            // Store chosen strategy for debugging
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
        // Analyze the element and its properties
        bool hasFixedSize = !double.IsNaN(element.Width) && !double.IsNaN(element.Height);
        bool hasDropShadow = element.Effect is DropShadowEffect;
        bool isButton = element is Button || element is ToggleButton;
        bool isInItemsControl = ItemsControl.ItemsControlFromItemContainer(element) != null;
        bool hasComplexContent = VisualTreeHelper.GetChildrenCount(element) > 3;

        // Decision logic
        if (hasFixedSize && !hasComplexContent)
        {
            // Fixed size and simple content -> Stack Overflow approach
            return TiltStrategy.FixedSize;
        }
        else if (hasDropShadow && GetPreserveShadow(element))
        {
            if (isButton && !isInItemsControl)
            {
                // Standalone button with shadow -> layered approach
                return TiltStrategy.LayeredShadow;
            }
            else if (hasComplexContent)
            {
                // Complex content -> compensated Planerator
                return TiltStrategy.CompensatedPlanerator;
            }
        }
        else if (!hasDropShadow)
        {
            // No shadow -> simple tilt
            return TiltStrategy.NoShadow;
        }

        // Default: layered approach (safest)
        return TiltStrategy.LayeredShadow;
    }

    #region Strategy Implementations

    private static void ApplyFixedSizeStrategy(FrameworkElement element)
    {
        // Implementation of the Stack Overflow approach
        if (double.IsNaN(element.Width) || double.IsNaN(element.Height))
        {
            // Set fixed dimensions based on ActualSize
            element.Width = element.ActualWidth > 0 ? element.ActualWidth : 100;
            element.Height = element.ActualHeight > 0 ? element.ActualHeight : 40;
        }

        // Find the parent panel
        var parent = element.Parent as Panel;
        if (parent == null) return;

        // Remember the index and detach the element from the visual tree before using it as a Visual
        int index = parent.Children.IndexOf(element);
        parent.Children.Remove(element);

        // Only now create the Viewport3D that sets Visual = element
        var viewport = CreateFixedSizeViewport3D(element);

        // Insert back at the original position
        parent.Children.Insert(index, viewport);

        // Attach event handlers
        AttachTiltHandlers(viewport, element);
    }

    private static void ApplyLayeredShadowStrategy(FrameworkElement element)
    {
        // Implementation of the layered approach
        var parent = element.Parent as Panel;
        if (parent == null) return;

        // Preserve layout properties
        var width = element.Width;
        var height = element.Height;
        var hAlign = element.HorizontalAlignment;
        var vAlign = element.VerticalAlignment;
        var margin = element.Margin;

        // Remember the index and detach the element from the original parent
        // before adding it to the new container
        int index = parent.Children.IndexOf(element);
        parent.Children.Remove(element);

        // Create a Grid container
        var container = new Grid
        {
            Width = width,
            Height = height,
            HorizontalAlignment = hAlign,
            VerticalAlignment = vAlign,
            Margin = margin
        };

        // Shadow layer (mirrors the element's shape)
        var shadowBorder = new Border
        {
            Background = new SolidColorBrush(Colors.Black) { Opacity = 0.01 },
            CornerRadius = element is Border b ? b.CornerRadius : new CornerRadius(0)
        };

        // Move the effect to the shadow layer
        if (element.Effect != null)
        {
            shadowBorder.Effect = element.Effect;
            element.Effect = null;
        }
        else
        {
            // Default shadow
            shadowBorder.Effect = new DropShadowEffect
            {
                BlurRadius = 15,
                ShadowDepth = 5,
                Direction = 270,
                Color = Colors.Black,
                Opacity = 0.3
            };
        }

        // Add layers to the container
        container.Children.Add(shadowBorder);

        // Reset element properties for proper alignment within the container
        element.Margin = new Thickness(0);

        container.Children.Add(element);

        // Replace in the parent at the original position
        parent.Children.Insert(index, container);

        // Apply tilt only to the content
        TiltEffect.SetIsEnabled(element, true);
        TiltEffect.SetTiltFactor(element, GetTiltFactor(container));
    }

    /*private static void ApplyFixedSizeStrategy(FrameworkElement element)
    {
        // Implementation of the Stack Overflow approach
        if (double.IsNaN(element.Width) || double.IsNaN(element.Height))
        {
            // Set fixed dimensions based on ActualSize
            element.Width = element.ActualWidth > 0 ? element.ActualWidth : 100;
            element.Height = element.ActualHeight > 0 ? element.ActualHeight : 40;
        }

        // Wrap in Viewport3D with fixed size
        var parent = element.Parent as Panel;
        if (parent == null) return;

        var viewport = CreateFixedSizeViewport3D(element);
        int index = parent.Children.IndexOf(element);
        parent.Children.Remove(element);
        parent.Children.Insert(index, viewport);

        // Attach event handlers
        AttachTiltHandlers(viewport, element);
    }

    private static void ApplyLayeredShadowStrategy(FrameworkElement element)
    {
        // Implementation of the layered approach
        var parent = element.Parent as Panel;
        if (parent == null) return;

        // Create a Grid container
        var container = new Grid
        {
            Width = element.Width,
            Height = element.Height,
            HorizontalAlignment = element.HorizontalAlignment,
            VerticalAlignment = element.VerticalAlignment,
            Margin = element.Margin
        };

        // Shadow layer (mirrors the element's shape)
        var shadowBorder = new Border
        {
            Background = new SolidColorBrush(Colors.Black) { Opacity = 0.01 },
            CornerRadius = element is Border b ? b.CornerRadius : new CornerRadius(0)
        };

        // Move the effect to the shadow layer
        if (element.Effect != null)
        {
            shadowBorder.Effect = element.Effect;
            element.Effect = null;
        }
        else
        {
            // Default shadow
            shadowBorder.Effect = new DropShadowEffect
            {
                BlurRadius = 15,
                ShadowDepth = 5,
                Direction = 270,
                Color = Colors.Black,
                Opacity = 0.3
            };
        }

        // Add layers to the container
        container.Children.Add(shadowBorder);

        // Reset element properties
        element.Margin = new Thickness(0);
        container.Children.Add(element);

        // Replace in the parent
        int index = parent.Children.IndexOf(element);
        parent.Children.Remove(element);
        parent.Children.Insert(index, container);

        // Apply tilt only to the content
        TiltEffect.SetIsEnabled(element, true);
        TiltEffect.SetTiltFactor(element, GetTiltFactor(container));
    }*/

    private static void ApplyCompensatedPlaneratorStrategy(FrameworkElement element)
    {
        // Use an improved Planerator with compensation
        PlaneratorHelper.SetPlaceIn3D(element, false); // Reset

        // Configure compensation
        if (element.Effect is DropShadowEffect dropShadow)
        {
            // Calculate compensation offset
            double blurCompensation = dropShadow.BlurRadius / 2.0;
            PlaneratorHelper.SetOriginX(element, 0.5 - (blurCompensation / element.ActualWidth));
            PlaneratorHelper.SetOriginY(element, 0.5 - (blurCompensation / element.ActualHeight));
        }

        // Enable tilt
        TiltEffect.SetIsEnabled(element, true);
        TiltEffect.SetTiltFactor(element, GetTiltFactor(element));
    }

    private static void ApplyNoShadowStrategy(FrameworkElement element)
    {
        // Remove the effect and use the standard tilt
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

        // Camera
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

        // Transform group for animations
        var transformGroup = new Transform3DGroup();
        transformGroup.Children.Add(new RotateTransform3D(
            new AxisAngleRotation3D(new Vector3D(1, 0, 0), 0))
        { /* X rotation */ });
        transformGroup.Children.Add(new RotateTransform3D(
            new AxisAngleRotation3D(new Vector3D(0, 1, 0), 0))
        { /* Y rotation */ });
        visual3D.Transform = transformGroup;

        viewport.Children.Add(visual3D);

        // Light
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
                // Compute tilt based on mouse position
                var pos = mousePosition.Value;
                double normalizedX = (pos.X / viewport.Width - 0.5) * 2;
                double normalizedY = (pos.Y / viewport.Height - 0.5) * 2;

                targetY = normalizedX * 10; // Horizontal tilt
                targetX = -normalizedY * 10; // Vertical tilt
            }

            // Animate rotations
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
        // Remove event handlers and restore the original state
        TiltEffect.SetIsEnabled(element, false);
        PlaneratorHelper.SetPlaceIn3D(element, false);

        // If the element is inside a Viewport3D, extract it back
        if (element.Parent is Viewport2DVisual3D)
        {
            // TODO: Implement extraction from Viewport3D
        }
    }

    #endregion

    #endregion
}