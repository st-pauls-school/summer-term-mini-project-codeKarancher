using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using CSProjectGame;


namespace CSProjectGame
{
    public static class ExtMeth
    {
        public static void PrepForDisplay(this TextBlock control)
        {
            control.Visibility = Visibility.Visible;
            control.VerticalAlignment = VerticalAlignment.Center;
            control.HorizontalAlignment = HorizontalAlignment.Center;
            control.Height = 400;
            control.Width = 400;
        }

        public static void CollapseElements(this UIElementCollection list)
        {
            foreach (UIElement a in list)
                a.Visibility = Visibility.Collapsed;
        }

        public static void ShowAllElements(this UIElementCollection list)
        {
            foreach (UIElement a in list)
                a.Visibility = Visibility.Visible;
        }

        public static void HideAllElements(this UIElementCollection list)
        {
            foreach (UIElement a in list)
                a.Visibility = Visibility.Hidden;
        }
    }
}
