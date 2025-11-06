using System.ComponentModel;
using System.Windows;

namespace DropShadowPanel_TiltEffect;

public static class DesignMode
{
    private static readonly Lazy<bool> _designModeEnabled = new Lazy<bool>((Func<bool>)(() => DesignerProperties.GetIsInDesignMode(new DependencyObject())));

    public static bool DesignModeEnabled => DesignMode._designModeEnabled.Value;
}