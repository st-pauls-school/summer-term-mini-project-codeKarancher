using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace CSProjectGame
{
    public static class KSTimerEvHandlers
    {
        public static Action<object, EventArgs> Generate(string todo, TextBlock Operand)
        {
            Action<TextBlock> Operation;
            switch (todo)
            {
                case "Reveal":
                    Operation = (TextBlock textBlock) =>
                    {
                        textBlock.Visibility = Visibility.Visible;
                    };
                    break;
                case "Remove":
                    Operation = (TextBlock textblock) =>
                    {
                        (textblock.Parent as Grid).Children.Remove(textblock);
                    };
                    break;
                default:
                    Operation = (TextBlock textblock) =>
                    {
                        ;//default
                    };
                    break;
            }
            Action<object, EventArgs> ToReturn = (object sender, EventArgs e) =>
            {
                Operation(Operand);
                (sender as DispatcherTimer).Stop();
            };
            return ToReturn;
        }
        
        public static Action<object, EventArgs> GenerateALUValueChange(TextBlock ToChange, string NewValue)
        {
            Action<object, EventArgs> ToReturn = (object sender, EventArgs e) =>
            {
                ToChange.Text = NewValue;
                (sender as DispatcherTimer).Stop();
            };
            return ToReturn;
        }
    }
}
