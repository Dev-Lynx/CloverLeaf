using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Interactivity;

namespace CloverLeaf.Common.Infrastructure.Resources.Behavior
{
    public class LeftClickContextMenuButtonBehavior : Behavior<Button>
    {
        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.AddHandler(UIElement.MouseDownEvent, new RoutedEventHandler(AssociatedObject_MouseDown), true);
        }

        void AssociatedObject_MouseDown(object sender, RoutedEventArgs e)
        {
            Button source = sender as Button;
            if (source != null && source.ContextMenu != null)
            {
                source.ContextMenu.PlacementTarget = source;
                
                source.ContextMenu.Placement = PlacementMode.Right;
                source.ContextMenu.IsOpen = !source.ContextMenu.IsOpen;
            }
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.RemoveHandler(UIElement.MouseDownEvent, new RoutedEventHandler(AssociatedObject_MouseDown));
        }
    }
}
