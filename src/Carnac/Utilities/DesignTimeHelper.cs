using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media;

namespace Carnac.Utilities
{
    public static class DesignTimeHelper
    {
        static bool? inDesignMode;

        /// <summary>
        /// Indicates whether or not the framework is in design-time mode.
        /// </summary>
        static bool InDesignMode
        {
            get
            {
                if (inDesignMode == null)
                {
                    var prop = DesignerProperties.IsInDesignModeProperty;
                    inDesignMode = (bool)DependencyPropertyDescriptor.FromProperty(prop, typeof(FrameworkElement)).Metadata.DefaultValue;

                    if (!inDesignMode.GetValueOrDefault(false) && Process.GetCurrentProcess()
                            .ProcessName.StartsWith("devenv", StringComparison.Ordinal))
                        inDesignMode = true;
                }

                return inDesignMode.GetValueOrDefault(false);
            }
        }

        public static DependencyProperty BackgroundProperty = DependencyProperty.RegisterAttached(
            "Background", typeof(Brush), typeof(DesignTimeHelper),
            new PropertyMetadata(BackgroundChanged));

        public static Brush GetBackground(DependencyObject dependencyObject)
        {
            return (Brush)dependencyObject.GetValue(BackgroundProperty);
        }
        public static void SetBackground(DependencyObject dependencyObject, Brush value)
        {
            dependencyObject.SetValue(BackgroundProperty, value);
        }
        static void BackgroundChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!InDesignMode)
                return;

            d.GetType().GetProperty("Background").SetValue(d, e.NewValue, null);
        }
    }
}
