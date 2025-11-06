using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Media3D;

namespace DropShadowPanel_TiltEffect.TiltEffectAnimation;

/// <summary>
/// Direct implementation of the tilt effect using Viewport2DVisual3D without a Planerator.
/// Solves the DropShadowEffect issue by using fixed dimensions and proper camera configuration.
/// </summary>
public class DirectViewportTiltBehavior
{
    #region Attached Properties

    public static readonly DependencyProperty IsEnabledProperty =
        DependencyProperty.RegisterAttached(
            "IsEnabled",
            typeof(bool),
            typeof(DirectViewportTiltBehavior),
            new PropertyMetadata(false, OnIsEnabledChanged));

    public static readonly DependencyProperty TiltFactorProperty =
        DependencyProperty.RegisterAttached(
            "TiltFactor",
            typeof(double),
            typeof(DirectViewportTiltBehavior),
            new PropertyMetadata(10.0));

    public static readonly DependencyProperty UseFixedSizeProperty =
        DependencyProperty.RegisterAttached(
            "UseFixedSize",
            typeof(bool),
            typeof(DirectViewportTiltBehavior),
            new PropertyMetadata(true));

    #endregion

    #region Property Accessors

    public static bool GetIsEnabled(DependencyObject obj)
    {
        return (bool)obj.GetValue(IsEnabledProperty);
    }

    public static void SetIsEnabled(DependencyObject obj, bool value)
    {
        obj.SetValue(IsEnabledProperty, value);
    }

    public static double GetTiltFactor(DependencyObject obj)
    {
        return (double)obj.GetValue(TiltFactorProperty);
    }

    public static void SetTiltFactor(DependencyObject obj, double value)
    {
        obj.SetValue(TiltFactorProperty, value);
    }

    public static bool GetUseFixedSize(DependencyObject obj)
    {
        return (bool)obj.GetValue(UseFixedSizeProperty);
    }

    public static void SetUseFixedSize(DependencyObject obj, bool value)
    {
        obj.SetValue(UseFixedSizeProperty, value);
    }

    #endregion

    #region Implementation

    private static void OnIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FrameworkElement element)
        {
            if ((bool)e.NewValue)
            {
                EnableTilt(element);
            }
            else
            {
                DisableTilt(element);
            }
        }
    }

    private static void EnableTilt(FrameworkElement element)
    {
        element.PreviewMouseLeftButtonDown += OnMouseDown;
        element.PreviewMouseLeftButtonUp += OnMouseUp;
        element.MouseLeave += OnMouseLeave;
    }

    private static void DisableTilt(FrameworkElement element)
    {
        element.PreviewMouseLeftButtonDown -= OnMouseDown;
        element.PreviewMouseLeftButtonUp -= OnMouseUp;
        element.MouseLeave -= OnMouseLeave;

        // Clean up the 3D wrapper if it exists
        RemoveViewport3D(element);
    }

    private static void OnMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is FrameworkElement element)
        {
            ApplyTilt(element, e.GetPosition(element));
        }
    }

    private static void OnMouseUp(object sender, MouseButtonEventArgs e)
    {
        if (sender is FrameworkElement element)
        {
            RemoveTilt(element);
        }
    }

    private static void OnMouseLeave(object sender, MouseEventArgs e)
    {
        if (sender is FrameworkElement element)
        {
            RemoveTilt(element);
        }
    }

    private static void ApplyTilt(FrameworkElement element, Point position)
    {
        // Get or create the Viewport3D wrapper
        var viewport = GetOrCreateViewport3D(element);
        if (viewport == null) return;

        // Find Viewport2DVisual3D
        var visual3D = FindViewport2DVisual3D(viewport);
        if (visual3D == null) return;

        // Compute tilt angles based on mouse position
        double width = element.ActualWidth > 0 ? element.ActualWidth : element.Width;
        double height = element.ActualHeight > 0 ? element.ActualHeight : element.Height;

        if (width <= 0 || height <= 0) return;

        double tiltFactor = GetTiltFactor(element);

        // Normalize mouse position to the range -1 to 1
        double normalizedX = (position.X / width - 0.5) * 2;
        double normalizedY = (position.Y / height - 0.5) * 2;

        // Compute rotation angles
        double rotationY = normalizedX * tiltFactor;  // Horizontal tilt
        double rotationX = -normalizedY * tiltFactor; // Vertical tilt

        // Animate the rotation
        AnimateRotation(visual3D, rotationX, rotationY, TimeSpan.FromMilliseconds(100));
    }

    private static void RemoveTilt(FrameworkElement element)
    {
        var parent = element.Parent as Panel;
        if (parent == null) return;

        // Find the Viewport3D in the same panel
        Viewport3D viewport = null;
        foreach (var child in parent.Children)
        {
            if (child is Viewport3D vp && vp.Tag == element)
            {
                viewport = vp;
                break;
            }
        }

        if (viewport == null) return;

        var visual3D = FindViewport2DVisual3D(viewport);
        if (visual3D != null)
        {
            // Animate back to zero
            AnimateRotation(visual3D, 0, 0, TimeSpan.FromMilliseconds(200));
        }
    }

    private static Viewport3D GetOrCreateViewport3D(FrameworkElement element)
    {
        var parent = element.Parent as Panel;
        if (parent == null) return null;

        // Try to find an existing Viewport3D
        foreach (var child in parent.Children)
        {
            if (child is Viewport3D viewport && viewport.Tag == element)
            {
                return viewport;
            }
        }

        // Create a new Viewport3D wrapper
        return CreateViewport3DWrapper(element);
    }

    private static Viewport3D CreateViewport3DWrapper(FrameworkElement element)
    {
        var parent = element.Parent as Panel;
        if (parent == null) return null;

        // Get dimensions
        double width = element.ActualWidth > 0 ? element.ActualWidth : element.Width;
        double height = element.ActualHeight > 0 ? element.ActualHeight : element.Height;

        // If UseFixedSize is true, apply fixed dimensions
        if (GetUseFixedSize(element))
        {
            if (double.IsNaN(element.Width)) element.Width = width;
            if (double.IsNaN(element.Height)) element.Height = height;
        }

        // Remove the element from the parent
        int index = parent.Children.IndexOf(element);
        parent.Children.Remove(element);

        // Create Viewport3D
        var viewport3D = new Viewport3D
        {
            Tag = element,
            Width = width,
            Height = height,
            HorizontalAlignment = element.HorizontalAlignment,
            VerticalAlignment = element.VerticalAlignment,
            Margin = element.Margin,
            ClipToBounds = false
        };

        // Configure the camera
        viewport3D.Camera = new PerspectiveCamera
        {
            Position = new Point3D(0, 0, 4),
            LookDirection = new Vector3D(0, 0, -1),
            UpDirection = new Vector3D(0, 1, 0),
            FieldOfView = 45
        };

        // Create Viewport2DVisual3D
        var viewport2DVisual3D = new Viewport2DVisual3D();

        // Set transform with composition
        var transform3DGroup = new Transform3DGroup();

        // Rotation X (vertical tilt)
        var rotateX = new RotateTransform3D(
            new AxisAngleRotation3D(new Vector3D(1, 0, 0), 0));

        // Rotation Y (horizontal tilt)
        var rotateY = new RotateTransform3D(
            new AxisAngleRotation3D(new Vector3D(0, 1, 0), 0));

        transform3DGroup.Children.Add(rotateX);
        transform3DGroup.Children.Add(rotateY);

        viewport2DVisual3D.Transform = transform3DGroup;

        // Set geometry (normalized quad)
        viewport2DVisual3D.Geometry = new MeshGeometry3D
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

        // Assign the visual
        viewport2DVisual3D.Visual = element;

        // Set material
        viewport2DVisual3D.Material = new DiffuseMaterial
        {
            Brush = Brushes.White
        };
        viewport2DVisual3D.Material.SetValue(
            Viewport2DVisual3D.IsVisualHostMaterialProperty, true);

        // Add to the viewport
        viewport3D.Children.Add(viewport2DVisual3D);

        // Add a light
        var light = new ModelVisual3D
        {
            Content = new DirectionalLight
            {
                Color = Colors.White,
                Direction = new Vector3D(0, 0, -1)
            }
        };
        viewport3D.Children.Add(light);

        // Reset element properties
        element.Margin = new Thickness(0);
        element.HorizontalAlignment = HorizontalAlignment.Stretch;
        element.VerticalAlignment = VerticalAlignment.Stretch;

        // Insert the viewport at the original position
        parent.Children.Insert(index, viewport3D);

        return viewport3D;
    }

    private static void RemoveViewport3D(FrameworkElement element)
    {
        var parent = element.Parent as Panel;
        if (parent != null)
        {
            // The element is still in its original parent; nothing to do
            return;
        }

        // Find the viewport that hosts the element
        var viewport2D = VisualTreeHelper.GetParent(element) as Viewport2DVisual3D;
        if (viewport2D != null)
        {
            var viewport3D = VisualTreeHelper.GetParent(viewport2D) as Viewport3D;
            if (viewport3D != null)
            {
                parent = viewport3D.Parent as Panel;
                if (parent != null)
                {
                    // Restore the original element
                    int index = parent.Children.IndexOf(viewport3D);
                    parent.Children.Remove(viewport3D);

                    // Restore original properties
                    element.Margin = viewport3D.Margin;
                    element.HorizontalAlignment = viewport3D.HorizontalAlignment;
                    element.VerticalAlignment = viewport3D.VerticalAlignment;
                    element.Width = viewport3D.Width;
                    element.Height = viewport3D.Height;

                    parent.Children.Insert(index, element);
                }
            }
        }
    }

    private static Viewport2DVisual3D FindViewport2DVisual3D(Viewport3D viewport)
    {
        foreach (Visual3D child in viewport.Children)
        {
            if (child is Viewport2DVisual3D visual2D)
            {
                return visual2D;
            }
        }
        return null;
    }

    private static void AnimateRotation(Viewport2DVisual3D visual3D,
        double rotationX, double rotationY, TimeSpan duration)
    {
        if (visual3D.Transform is Transform3DGroup group && group.Children.Count >= 2)
        {
            // Animate rotation X
            if (group.Children[0] is RotateTransform3D rotateXTransform &&
                rotateXTransform.Rotation is AxisAngleRotation3D rotationXAxis)
            {
                var animX = new DoubleAnimation(rotationX, duration)
                {
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
                };
                rotationXAxis.BeginAnimation(AxisAngleRotation3D.AngleProperty, animX);
            }

            // Animate rotation Y
            if (group.Children[1] is RotateTransform3D rotateYTransform &&
                rotateYTransform.Rotation is AxisAngleRotation3D rotationYAxis)
            {
                var animY = new DoubleAnimation(rotationY, duration)
                {
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
                };
                rotationYAxis.BeginAnimation(AxisAngleRotation3D.AngleProperty, animY);
            }
        }
    }

    #endregion
}