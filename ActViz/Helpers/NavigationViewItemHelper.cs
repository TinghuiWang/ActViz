using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;

namespace ActViz.Helpers
{
    public static class NavigationViewItemHelper
    {
        /// <summary>
        /// Find and set the indicator of navigation view item.
        /// </summary>
        /// <param name="item"></param>
        public static void SetNavigationViewItemIndicator(NavigationViewItem item, bool isSelected)
        {
            Rectangle indicator = GetIndicator(item);
            if(indicator != null)
            {
                indicator.Opacity = (isSelected) ? 1 : 0;
            }
        }

        private static Rectangle GetIndicator(NavigationViewItem item)
        {
            int count = VisualTreeHelper.GetChildrenCount(item);
            // The visual tree not populated yet
            if (count == 0) return null;
            // Otherwise, go into the tree and get the indicator.
            return VisualTreeHelper.GetChild(VisualTreeHelper.GetChild(VisualTreeHelper.GetChild(item, 0), 0), 0) as Rectangle;
        }
    }
}
