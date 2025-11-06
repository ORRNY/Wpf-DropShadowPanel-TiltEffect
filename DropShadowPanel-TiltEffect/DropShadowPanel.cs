using System.Collections;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace DropShadowPanel_TiltEffect;

public enum ShadowMode
{
    Content = 0,
    Inner,
    Outer,
}

public class DropShadowPanel : Decorator
{
    public static readonly DependencyProperty BackgroundProperty = DependencyProperty.Register(nameof(Background), typeof(Brush), typeof(DropShadowPanel), new PropertyMetadata((PropertyChangedCallback)null));
    public static readonly DependencyProperty BlurRadiusProperty = DependencyProperty.Register(nameof(BlurRadius), typeof(double), typeof(DropShadowPanel), new PropertyMetadata((object)20.0));
    public static readonly DependencyProperty ColorProperty = DependencyProperty.Register(nameof(Color), typeof(Color), typeof(DropShadowPanel), new PropertyMetadata((object)Colors.Black));
    public static readonly DependencyProperty DirectionProperty = DependencyProperty.Register(nameof(Direction), typeof(double), typeof(DropShadowPanel), new PropertyMetadata((object)315.0));
    public static readonly DependencyProperty ShadowOpacityProperty = DependencyProperty.Register(nameof(ShadowOpacity), typeof(double), typeof(DropShadowPanel), new PropertyMetadata((object)0.8));
    public static readonly DependencyProperty RenderingBiasProperty = DependencyProperty.Register(nameof(RenderingBias), typeof(RenderingBias), typeof(DropShadowPanel), new PropertyMetadata((object)RenderingBias.Performance));
    public static readonly DependencyProperty ShadowDepthProperty = DependencyProperty.Register(nameof(ShadowDepth), typeof(double), typeof(DropShadowPanel), new PropertyMetadata((object)0.0));
    public static readonly DependencyProperty ShadowModeProperty = DependencyProperty.Register(nameof(ShadowMode), typeof(ShadowMode), typeof(DropShadowPanel), new PropertyMetadata((object)ShadowMode.Content));
    public static readonly DependencyProperty CornerRadiusProperty = DependencyProperty.Register(nameof(CornerRadius), typeof(CornerRadius), typeof(DropShadowPanel), new PropertyMetadata((object)new CornerRadius(0.0, 0.0, 0.0, 0.0)));
    public static readonly DependencyProperty BorderThicknessProperty = DependencyProperty.Register(nameof(BorderThickness), typeof(Thickness), typeof(DropShadowPanel), new PropertyMetadata((object)new Thickness(0.0, 0.0, 0.0, 0.0)));

    public static readonly DependencyProperty BorderBrushProperty =
        DependencyProperty.Register(
            nameof(BorderBrush),
            typeof(Brush),
            typeof(DropShadowPanel),
            new PropertyMetadata(new SolidColorBrush(Colors.Gray)));

    private ContainerVisual _internalVisual;

    public Brush Background
    {
        get => (Brush)this.GetValue(DropShadowPanel.BackgroundProperty);
        set => this.SetValue(DropShadowPanel.BackgroundProperty, (object)value);
    }

    public double BlurRadius
    {
        get => (double)this.GetValue(DropShadowPanel.BlurRadiusProperty);
        set => this.SetValue(DropShadowPanel.BlurRadiusProperty, (object)value);
    }

    public Color Color
    {
        get => (Color)this.GetValue(DropShadowPanel.ColorProperty);
        set => this.SetValue(DropShadowPanel.ColorProperty, (object)value);
    }

    public double Direction
    {
        get => (double)this.GetValue(DropShadowPanel.DirectionProperty);
        set => this.SetValue(DropShadowPanel.DirectionProperty, (object)value);
    }

    public double ShadowOpacity
    {
        get => (double)this.GetValue(DropShadowPanel.ShadowOpacityProperty);
        set => this.SetValue(DropShadowPanel.ShadowOpacityProperty, (object)value);
    }

    public RenderingBias RenderingBias
    {
        get => (RenderingBias)this.GetValue(DropShadowPanel.RenderingBiasProperty);
        set => this.SetValue(DropShadowPanel.RenderingBiasProperty, (object)value);
    }

    public double ShadowDepth
    {
        get => (double)this.GetValue(DropShadowPanel.ShadowDepthProperty);
        set => this.SetValue(DropShadowPanel.ShadowDepthProperty, (object)value);
    }

    public ShadowMode ShadowMode
    {
        get => (ShadowMode)this.GetValue(DropShadowPanel.ShadowModeProperty);
        set => this.SetValue(DropShadowPanel.ShadowModeProperty, (object)value);
    }

    public CornerRadius CornerRadius
    {
        get => (CornerRadius)this.GetValue(DropShadowPanel.CornerRadiusProperty);
        set => this.SetValue(DropShadowPanel.CornerRadiusProperty, (object)value);
    }

    public Thickness BorderThickness
    {
        get => (Thickness)this.GetValue(DropShadowPanel.BorderThicknessProperty);
        set => this.SetValue(DropShadowPanel.BorderThicknessProperty, (object)value);
    }

    public Brush BorderBrush
    {
        get => (Brush)this.GetValue(DropShadowPanel.BorderBrushProperty);
        set => this.SetValue(DropShadowPanel.BorderBrushProperty, (object)value);
    }

    private ContainerVisual InternalVisual
    {
        get
        {
            if (this._internalVisual == null)
            {
                this._internalVisual = new ContainerVisual();
                this.AddVisualChild((Visual)this._internalVisual);
            }
            return this._internalVisual;
        }
    }

    private UIElement InternalChild
    {
        get
        {
            VisualCollection children = this.InternalVisual?.Children;
            return children != null && children.Count != 0 ? children[0] as UIElement : null;
        }
        set
        {
            VisualCollection children = this.InternalVisual?.Children;
            if (children != null)
            {
                if (children.Count != 0)
                    children.Clear();
                children.Add((Visual)value);
            }
        }
    }


    public override UIElement Child
    {
        get => this.InternalChild;
        set
        {
            UIElement internalChild = this.InternalChild;
            if (internalChild == value)
                return;

            this.RemoveLogicalChild((object)internalChild);
            UIElement internalVisual = this.CreateInternalVisual(value);
            if (internalVisual != null)
                this.AddLogicalChild((object)internalVisual);
            this.InternalChild = internalVisual;
            this.InvalidateMeasure();
        }
    }

    #region Override Methods

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
    }

    #endregion
    protected internal UIElement CreateInternalVisual(UIElement value)
    {
        DropShadowEffect target1 = new DropShadowEffect();
        BindingOperations.SetBinding((DependencyObject)target1, DropShadowEffect.BlurRadiusProperty, (BindingBase)new Binding("BlurRadius") { Source = (object)this });
        BindingOperations.SetBinding((DependencyObject)target1, DropShadowEffect.ColorProperty, (BindingBase)new Binding("Color") { Source = (object)this });
        BindingOperations.SetBinding((DependencyObject)target1, DropShadowEffect.DirectionProperty, (BindingBase)new Binding("Direction") { Source = (object)this });
        BindingOperations.SetBinding((DependencyObject)target1, DropShadowEffect.OpacityProperty, (BindingBase)new Binding("ShadowOpacity") { Source = (object)this });
        BindingOperations.SetBinding((DependencyObject)target1, DropShadowEffect.RenderingBiasProperty, (BindingBase)new Binding("RenderingBias") { Source = (object)this });
        BindingOperations.SetBinding((DependencyObject)target1, DropShadowEffect.ShadowDepthProperty, (BindingBase)new Binding("ShadowDepth") { Source = (object)this });

        Style style1 = new Style();
        DataTrigger dataTrigger = new DataTrigger()
        {
            Binding = (BindingBase)new Binding("ShadowMode")
            {
                Source = (object)this
            },
            Value = (object)ShadowMode.Outer
        };

        // Použijeme skutečný "donut" klip (outer minus inner), aby stín neprosvítal pod hranou.
        dataTrigger.Setters.Add((SetterBase)new Setter()
        {
            Property = UIElement.ClipProperty,
            Value = (object)new MultiBinding()
            {
                Bindings = {
                    (BindingBase) new Binding("ActualWidth") { Source = (object) this },
                    (BindingBase) new Binding("ActualHeight"){ Source = (object) this },
                    (BindingBase) new Binding("BlurRadius")  { Source = (object) this },
                    (BindingBase) new Binding("BorderThickness"){ Source = (object) this },
                    (BindingBase) new Binding("CornerRadius"){ Source = (object) this }
                },
                Converter = (IMultiValueConverter)new ClipInnerRectConverter()
            }
        });

        dataTrigger.Setters.Add((SetterBase)new Setter()
        {
            Property = ClipBorder.ClipBorder.BackgroundProperty,
            Value = (object)new Binding("BorderBrush")
            {
                Source = (object)this
            }
        });

        dataTrigger.Setters.Add((SetterBase)new Setter()
        {
            Property = FrameworkElement.MarginProperty,
            Value = (object)new Thickness(0.0, 0.0, 0.0, 0.0)
        });
        style1.Triggers.Add((TriggerBase)dataTrigger);

        BindingOperations.SetBinding((DependencyObject)new Border()
        {
            BorderThickness = new Thickness(0.0)

        }, Border.CornerRadiusProperty, (BindingBase)new Binding("CornerRadius")
        {
            Source = (object)this
        });

        ClipBorder.ClipBorder clipBorder1 = new ClipBorder.ClipBorder();
        clipBorder1.Effect = (Effect)target1;
        clipBorder1.Style = style1;
        clipBorder1.OptimizeClipRendering = false;
        clipBorder1.BorderThickness = new Thickness(0.0);
        ClipBorder.ClipBorder clipBorder2 = clipBorder1;

        BindingOperations.SetBinding((DependencyObject)clipBorder2, ClipBorder.ClipBorder.CornerRadiusProperty, (BindingBase)new Binding("CornerRadius")
        {
            Source = (object)this
        });

        Grid grid = new Grid()
        {
            SnapsToDevicePixels = true,
            UseLayoutRounding = true
        };
        Border target2 = new Border();
        BindingOperations.SetBinding((DependencyObject)target2, Border.CornerRadiusProperty, (BindingBase)new Binding("CornerRadius")
        {
            Source = (object)this
        });
        Style style2 = new Style()
        {
            TargetType = typeof(Border)
        };
        Setter setter = new Setter();
        setter.Property = UIElement.ClipProperty;
        MultiBinding multiBinding = new MultiBinding()
        {
            Converter = (IMultiValueConverter)new BorderClipConverter()
        };
        multiBinding.Bindings.Add((BindingBase)new Binding()
        {
            Path = new PropertyPath("ActualWidth", Array.Empty<object>()),
            RelativeSource = new RelativeSource(RelativeSourceMode.Self)
        });
        multiBinding.Bindings.Add((BindingBase)new Binding()
        {
            Path = new PropertyPath("ActualHeight", Array.Empty<object>()),
            RelativeSource = new RelativeSource(RelativeSourceMode.Self)
        });
        multiBinding.Bindings.Add((BindingBase)new Binding()
        {
            Path = new PropertyPath("CornerRadius", Array.Empty<object>()),
            RelativeSource = new RelativeSource(RelativeSourceMode.Self)
        });
        setter.Value = (object)multiBinding;
        style2.Setters.Add((SetterBase)setter);
        Border border1 = new Border();
        border1.Style = style2;
        Border border2 = border1;
        BindingOperations.SetBinding((DependencyObject)border2, Border.CornerRadiusProperty, (BindingBase)new Binding("CornerRadius")
        {
            Source = (object)this
        });
        grid.Children.Add((UIElement)clipBorder2);
        if (value != null)
        {
            border2.Child = value;
            grid.Children.Add((UIElement)border2);
        }
        target2.Child = (UIElement)grid;
        return (UIElement)target2;
    }

    protected override int VisualChildrenCount => 1;

    protected override Visual GetVisualChild(int index)
    {
        if (index != 0)
            throw new ArgumentOutOfRangeException();
        return (Visual)this.InternalVisual;
    }

    protected override IEnumerator LogicalChildren
    {
        get
        {
            if (this.InternalChild == null)
                return (IEnumerator)null;
            return (IEnumerator)new List<UIElement>()
            {
                this.InternalChild
            }.GetEnumerator();
        }
    }
}

internal class ClipInnerRectConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (((IEnumerable<object>)values).Any(o => o == DependencyProperty.UnsetValue || o == null))
            return null;

        double w = (double)values[0];
        double h = (double)values[1];
        double blur = (double)values[2];
        Thickness borderThickness = (Thickness)values[3];
        CornerRadius cornerRadius = (CornerRadius)values[4];

        // Vnější a vnitřní rounded rect
        Geometry outer = GetRoundRectangle(new Rect(-blur, -blur, w + 2 * blur, h + 2 * blur), borderThickness, cornerRadius);
        outer.Freeze();
        Geometry inner = GetRoundRectangle(new Rect(0.0, 0.0, w, h), borderThickness, cornerRadius);
        inner.Freeze();

        // Exclude => "donut" (vnější minus vnitřní)
        var donut = new CombinedGeometry(GeometryCombineMode.Exclude, outer, inner);
        donut.Freeze();
        return donut;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        => throw new NotImplementedException();

    private static Geometry GetRoundRectangle(Rect baseRect, Thickness borderThickness, CornerRadius cornerRadius)
    {
        if (cornerRadius.TopLeft < double.Epsilon) cornerRadius.TopLeft = 0.0;
        if (cornerRadius.TopRight < double.Epsilon) cornerRadius.TopRight = 0.0;
        if (cornerRadius.BottomLeft < double.Epsilon) cornerRadius.BottomLeft = 0.0;
        if (cornerRadius.BottomRight < double.Epsilon) cornerRadius.BottomRight = 0.0;

        double left = Math.Max(0.0, borderThickness.Left * 0.5);
        double top = Math.Max(0.0, borderThickness.Top * 0.5);
        double right = Math.Max(0.0, borderThickness.Right * 0.5);
        double bottom = Math.Max(0.0, borderThickness.Bottom * 0.5);

        Rect r1 = new Rect(baseRect.X, baseRect.Y, Math.Max(0.0, cornerRadius.TopLeft - left), Math.Max(0.0, cornerRadius.TopLeft - right));
        Rect r2 = new Rect(baseRect.X + baseRect.Width - cornerRadius.TopRight + right, baseRect.Y, Math.Max(0.0, cornerRadius.TopRight - right), Math.Max(0.0, cornerRadius.TopRight - top));
        Rect r3 = new Rect(baseRect.X + baseRect.Width - cornerRadius.BottomRight + right, baseRect.Y + baseRect.Height - cornerRadius.BottomRight + bottom, Math.Max(0.0, cornerRadius.BottomRight - right), Math.Max(0.0, cornerRadius.BottomRight - bottom));
        Rect r4 = new Rect(baseRect.X, baseRect.Y + baseRect.Height - cornerRadius.BottomLeft + bottom, Math.Max(0.0, cornerRadius.BottomLeft - left), Math.Max(0.0, cornerRadius.BottomLeft - bottom));

        if (r1.Right > r2.Left)
        {
            double v = r1.Width / (r1.Width + r2.Width) * baseRect.Width;
            r1 = new Rect(r1.X, r1.Y, v, r1.Height);
            r2 = new Rect(baseRect.Left + v, r2.Y, Math.Max(0.0, baseRect.Width - v), r2.Height);
        }
        if (r2.Bottom > r3.Top)
        {
            double v = r2.Height / (r2.Height + r3.Height) * baseRect.Height;
            r2 = new Rect(r2.X, r2.Y, r2.Width, v);
            r3 = new Rect(r3.X, baseRect.Top + v, r3.Width, Math.Max(0.0, baseRect.Height - v));
        }
        if (r3.Left < r4.Right)
        {
            double v = r4.Width / (r4.Width + r3.Width) * baseRect.Width;
            r4 = new Rect(r4.X, r4.Y, v, r4.Height);
            r3 = new Rect(baseRect.Left + v, r3.Y, Math.Max(0.0, baseRect.Width - v), r3.Height);
        }
        if (r4.Top < r1.Bottom)
        {
            double v = r1.Height / (r1.Height + r4.Height) * baseRect.Height;
            r1 = new Rect(r1.X, r1.Y, r1.Width, v);
            r4 = new Rect(r4.X, baseRect.Top + v, r4.Width, Math.Max(0.0, baseRect.Height - v));
        }

        StreamGeometry g = new StreamGeometry();
        using (var ctx = g.Open())
        {
            ctx.BeginFigure(r1.BottomLeft, true, true);
            ctx.ArcTo(r1.TopRight, r1.Size, 0.0, false, SweepDirection.Clockwise, true, true);
            ctx.LineTo(r2.TopLeft, true, true);
            ctx.ArcTo(r2.BottomRight, r2.Size, 0.0, false, SweepDirection.Clockwise, true, true);
            ctx.LineTo(r3.TopRight, true, true);
            ctx.ArcTo(r3.BottomLeft, r3.Size, 0.0, false, SweepDirection.Clockwise, true, true);
            ctx.LineTo(r4.BottomRight, true, true);
            ctx.ArcTo(r4.TopLeft, r4.Size, 0.0, false, SweepDirection.Clockwise, true, true);
        }
        return g;
    }
}

internal class BorderClipConverter : IMultiValueConverter
{
    public object Convert(object?[]? values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values is [double width, double height, ..])
        {
            if (width < 1.0 || height < 1.0)
            {
                return Geometry.Empty;
            }

            CornerRadius cornerRadius = default;
            Thickness borderThickness = default;
            if (values.Length > 2 && values[2] is CornerRadius radius)
            {
                cornerRadius = radius;
                if (values.Length > 3 && values[3] is Thickness thickness)
                {
                    borderThickness = thickness;
                }
            }

            var geometry = GetRoundRectangle(new Rect(0, 0, width, height), borderThickness, cornerRadius);
            geometry.Freeze();

            return geometry;
        }

        return DependencyProperty.UnsetValue;
    }

    public object?[]? ConvertBack(object? value, Type[] targetTypes, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }

    // https://wpfspark.wordpress.com/2011/06/08/clipborder-a-wpf-border-that-clips/
    private static Geometry GetRoundRectangle(Rect baseRect, Thickness borderThickness, CornerRadius cornerRadius)
    {
        // Normalizing the corner radius
        if (cornerRadius.TopLeft < double.Epsilon)
        {
            cornerRadius.TopLeft = 0.0;
        }

        if (cornerRadius.TopRight < double.Epsilon)
        {
            cornerRadius.TopRight = 0.0;
        }

        if (cornerRadius.BottomLeft < double.Epsilon)
        {
            cornerRadius.BottomLeft = 0.0;
        }

        if (cornerRadius.BottomRight < double.Epsilon)
        {
            cornerRadius.BottomRight = 0.0;
        }

        // Taking the border thickness into account
        var leftHalf = borderThickness.Left * 0.5;
        if (leftHalf < double.Epsilon)
        {
            leftHalf = 0.0;
        }

        var topHalf = borderThickness.Top * 0.5;
        if (topHalf < double.Epsilon)
        {
            topHalf = 0.0;
        }

        var rightHalf = borderThickness.Right * 0.5;
        if (rightHalf < double.Epsilon)
        {
            rightHalf = 0.0;
        }

        var bottomHalf = borderThickness.Bottom * 0.5;
        if (bottomHalf < double.Epsilon)
        {
            bottomHalf = 0.0;
        }

        // Create the rectangles for the corners that needs to be curved in the base rectangle 
        // TopLeft Rectangle 
        var topLeftRect = new Rect(
            baseRect.Location.X,
            baseRect.Location.Y,
            Math.Max(0.0, cornerRadius.TopLeft - leftHalf),
            Math.Max(0.0, cornerRadius.TopLeft - rightHalf));

        // TopRight Rectangle 
        var topRightRect = new Rect(
            baseRect.Location.X + baseRect.Width - cornerRadius.TopRight + rightHalf,
            baseRect.Location.Y,
            Math.Max(0.0, cornerRadius.TopRight - rightHalf),
            Math.Max(0.0, cornerRadius.TopRight - topHalf));

        // BottomRight Rectangle
        var bottomRightRect = new Rect(
            baseRect.Location.X + baseRect.Width - cornerRadius.BottomRight + rightHalf,
            baseRect.Location.Y + baseRect.Height - cornerRadius.BottomRight + bottomHalf,
            Math.Max(0.0, cornerRadius.BottomRight - rightHalf),
            Math.Max(0.0, cornerRadius.BottomRight - bottomHalf));

        // BottomLeft Rectangle 
        var bottomLeftRect = new Rect(
            baseRect.Location.X,
            baseRect.Location.Y + baseRect.Height - cornerRadius.BottomLeft + bottomHalf,
            Math.Max(0.0, cornerRadius.BottomLeft - leftHalf),
            Math.Max(0.0, cornerRadius.BottomLeft - bottomHalf));

        // Adjust the width of the TopLeft and TopRight rectangles so that they are proportional to the width of the baseRect 
        if (topLeftRect.Right > topRightRect.Left)
        {
            var newWidth = (topLeftRect.Width / (topLeftRect.Width + topRightRect.Width)) * baseRect.Width;
            topLeftRect = new Rect(topLeftRect.Location.X, topLeftRect.Location.Y, newWidth, topLeftRect.Height);
            topRightRect = new Rect(
                baseRect.Left + newWidth,
                topRightRect.Location.Y,
                Math.Max(0.0, baseRect.Width - newWidth),
                topRightRect.Height);
        }

        // Adjust the height of the TopRight and BottomRight rectangles so that they are proportional to the height of the baseRect
        if (topRightRect.Bottom > bottomRightRect.Top)
        {
            var newHeight = (topRightRect.Height / (topRightRect.Height + bottomRightRect.Height)) * baseRect.Height;
            topRightRect = new Rect(topRightRect.Location.X, topRightRect.Location.Y, topRightRect.Width, newHeight);
            bottomRightRect = new Rect(
                bottomRightRect.Location.X,
                baseRect.Top + newHeight,
                bottomRightRect.Width,
                Math.Max(0.0, baseRect.Height - newHeight));
        }

        // Adjust the width of the BottomLeft and BottomRight rectangles so that they are proportional to the width of the baseRect
        if (bottomRightRect.Left < bottomLeftRect.Right)
        {
            var newWidth = (bottomLeftRect.Width / (bottomLeftRect.Width + bottomRightRect.Width)) * baseRect.Width;
            bottomLeftRect = new Rect(bottomLeftRect.Location.X, bottomLeftRect.Location.Y, newWidth, bottomLeftRect.Height);
            bottomRightRect = new Rect(
                baseRect.Left + newWidth,
                bottomRightRect.Location.Y,
                Math.Max(0.0, baseRect.Width - newWidth),
                bottomRightRect.Height);
        }

        // Adjust the height of the TopLeft and BottomLeft rectangles so that they are proportional to the height of the baseRect
        if (bottomLeftRect.Top < topLeftRect.Bottom)
        {
            var newHeight = (topLeftRect.Height / (topLeftRect.Height + bottomLeftRect.Height)) * baseRect.Height;
            topLeftRect = new Rect(topLeftRect.Location.X, topLeftRect.Location.Y, topLeftRect.Width, newHeight);
            bottomLeftRect = new Rect(
                bottomLeftRect.Location.X,
                baseRect.Top + newHeight,
                bottomLeftRect.Width,
                Math.Max(0.0, baseRect.Height - newHeight));
        }

        var roundedRectGeometry = new StreamGeometry();

        using (var context = roundedRectGeometry.Open())
        {
            // Begin from the Bottom of the TopLeft Arc and proceed clockwise
            context.BeginFigure(topLeftRect.BottomLeft, true, true);

            // TopLeft Arc
            context.ArcTo(topLeftRect.TopRight, topLeftRect.Size, 0, false, SweepDirection.Clockwise, true, true);

            // Top Line
            context.LineTo(topRightRect.TopLeft, true, true);

            // TopRight Arc
            context.ArcTo(topRightRect.BottomRight, topRightRect.Size, 0, false, SweepDirection.Clockwise, true, true);

            // Right Line
            context.LineTo(bottomRightRect.TopRight, true, true);

            // BottomRight Arc
            context.ArcTo(bottomRightRect.BottomLeft, bottomRightRect.Size, 0, false, SweepDirection.Clockwise, true, true);

            // Bottom Line
            context.LineTo(bottomLeftRect.BottomRight, true, true);

            // BottomLeft Arc
            context.ArcTo(bottomLeftRect.TopLeft, bottomLeftRect.Size, 0, false, SweepDirection.Clockwise, true, true);
        }

        return roundedRectGeometry;
    }
}

