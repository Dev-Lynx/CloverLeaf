using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace CloverLeaf.Common.Infrastructure.Extensions
{
    public static class UIHelper
    {
        #region Properties
        public static event EventHandler ViewChanged;
        public static event EventHandler LoadComplete;
        #endregion

        public static void RequestNavigateToView(this IRegionManager manager, string region, string view)
        {
            try
            {
                var control = (System.Windows.Controls.UserControl)manager.Regions[region].GetView(view);
                if (control == null)
                    throw new NullReferenceException("The region manager could not locate the specified region or view.");

                manager.RequestNavigate(region, control.GetType().Name);
                ViewChanged?.Invoke(null, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                Core.Log.Error(ex);
            }
        }

        #region Animations
        public static async Task FadeOut(this FrameworkElement element, double speed = 1, bool reappear = false)
        {
            if (speed == 0) speed = 1;

            if (element.Opacity <= 0)
                element.Opacity = 1;

            // time = distance / speed
            var time = (double)1 / speed;
            var from = element.Opacity;

            if (double.IsNaN(from) || double.IsInfinity(from))
                from = 1;

            var animation = new DoubleAnimation
            {
                From = from,
                To = 0,
                FillBehavior = FillBehavior.Stop,
                Duration = new System.Windows.Duration(TimeSpan.FromSeconds(time))
            };

            EventHandler fadeHandler = null;
            bool completed = false;

            animation.Completed += fadeHandler = (s, e) =>
            {
                if (reappear) element.Opacity = 1;
                else element.Opacity = 0;
                completed = true;
            };

            element.BeginAnimation(FrameworkElement.OpacityProperty, animation);

            while (!completed)
                await Task.Delay(10);
        }

        public static async Task FadeIn(this FrameworkElement element, double speed = 1, bool disappear = false)
        {
            if (speed <= 0) speed = 1;

            // time = distance / speed
            var time = 1.0 / speed;

            var animation = new DoubleAnimation
            {
                From = 0,
                To = 1,
                FillBehavior = FillBehavior.Stop,
                Duration = new Duration(TimeSpan.FromSeconds(time))
            };

            EventHandler fadeHandler = null;
            bool completed = false;

            animation.Completed += fadeHandler = (s, e) =>
            {
                if (disappear) element.Opacity = 0;
                else element.Opacity = 1;
                completed = true;
            };

            element.BeginAnimation(FrameworkElement.OpacityProperty, animation);

            while (!completed)
                await Task.Delay(10);
        }
        #endregion

        public static T FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            T parent = null;
            try
            {
                parent = VisualTreeHelper.GetParent(child) as T;
            }
            catch { return null; }
            

            if (parent != null)
                return parent;
            else
                return FindParent<T>(parent);
        }

        public static void LoadInitiated() => LoadComplete?.Invoke(null, EventArgs.Empty);




        #region ComboBox Helper
        public static readonly DependencyProperty EditBackgroundProperty = DependencyProperty.RegisterAttached(
        "EditBackground", typeof(Brush), typeof(UIHelper), new PropertyMetadata(default(Brush), EditBackgroundChanged));

        [AttachedPropertyBrowsableForType(typeof(ComboBox))]
        public static Brush GetEditBackground(DependencyObject element) => (Brush)element.GetValue(EditBackgroundProperty);

        [AttachedPropertyBrowsableForType(typeof(ComboBox))]
        public static void SetEditBackground(DependencyObject element, Brush value) => element.SetValue(EditBackgroundProperty, value);


        private static void EditBackgroundChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is ComboBox)) return;
            var combo = d as ComboBox;
            
            if (!combo.IsLoaded)
            {
                RoutedEventHandler loaded = null;
                combo.Loaded += loaded = (s, ev) =>
                {
                    EditBackgroundChanged(d, e);
                    combo.Loaded -= loaded;
                };
                combo.Loaded += loaded;
                return;
            }

            var part = combo.Template.FindName("PART_EditableTextBox", combo);
            if (!(part is TextBox)) return;

            var parent = ((TextBox)part).Parent;
            if (!(parent is Border)) return;
            ((Border)parent).Background = (Brush)e.NewValue;
        }
        #endregion
    }
}
