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

        /// <summary>
        /// Splits the string into an array of strings which were seperated by the string divider given
        /// </summary>
        /// <param name="s"></param>
        /// <param name="divider"></param>
        public static string[] Split(this string s, string divider)
        {
            List<string> ls = new List<string>();
            List<char> curWord = new List<char>();
            char[] sAr = s.ToCharArray();
            char[] dAr = divider.ToCharArray();
            for (int i = 0; i < sAr.Length; i++)
            {
                if (sAr[i] == dAr[0])
                {
                    int i2;
                    for (i2 = 1; i2 < dAr.Length; i2++)
                    {
                        if (sAr[i + i2] != dAr[i2])
                            break;
                    }
                    if (i2 == dAr.Length && curWord.Count > 0)
                    {
                        ls.Add(new string(curWord.ToArray()));
                        curWord = new List<char>();
                        i += i2 - 1;
                    }
                    else
                        curWord.Add(sAr[i]);
                }
                else
                    curWord.Add(sAr[i]);
            }
            ls.Add(new string(curWord.ToArray()));
            return ls.ToArray();
        }

        public static double MyWidth(this ColumnDefinition cd)
        {
            Grid ParentGrid = cd.Parent as Grid;
            double Numerator = cd.Width.Value;
            double Denominator = 0;
            for (int i = 0; i < ParentGrid.ColumnDefinitions.Count; i++)
                Denominator += ParentGrid.ColumnDefinitions[i].Width.Value;
            return ParentGrid.Width * Numerator / Denominator;
        }

        public static double MyHeight(this RowDefinition rd)
        {
            Grid ParentGrid = rd.Parent as Grid;
            double Numerator = rd.Height.Value;
            double Denominator = 0;
            for (int i = 0; i < ParentGrid.RowDefinitions.Count; i++)
                Denominator += ParentGrid.RowDefinitions[i].Height.Value;
            return ParentGrid.Height * Numerator / Denominator;
        }
    }
}
