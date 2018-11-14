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

        #region Dispatcher Timer Eventhandlers
        private void dtRevealtext_ToRegister_Tick(object sender, EventArgs e)
        {
            text_ToRegister.Visibility = Visibility.Visible;
            //DEBUG - ((memoryDockPanel.Children[0] as StackPanel).Children[int.Parse(text_PC.Text) + 6] as DockPanel).Background = Brushes.SlateGray;
            (sender as DispatcherTimer).Stop();
        }
        private void dtRemovetext_ToRegister_Tick(object sender, EventArgs e)
        {
            (text_ToRegister.Parent as Grid).Children.Remove(text_ToRegister);
            (sender as DispatcherTimer).Stop();
        }
        private void dtRevealtext_AddressBus_Tick(object sender, EventArgs e)
        {
            MainWindow.text_AddressBus.Visibility = Visibility.Visible;
            (sender as DispatcherTimer).Stop();
        }
        private void dtRemovetext_AddressBus_Tick(object sender, EventArgs e)
        {
            (text_AddressBus.Parent as Grid).Children.Remove(text_AddressBus);
            (sender as DispatcherTimer).Stop();
        }
        private void dtRevealtext_DataBus_Tick(object sender, EventArgs e)
        {
            text_DataBus.Visibility = Visibility.Visible;
            (sender as DispatcherTimer).Stop();
        }
        private void dtRemovetext_DataBus_Tick(object sender, EventArgs e)
        {
            (sender as DispatcherTimer).Tick -= dtRemovetext_DataBus_Tick;
            (text_DataBus.Parent as Grid).Children.Remove(text_DataBus);
            (sender as DispatcherTimer).Stop();
        }
        private void dtRemovetext_ToALU_Tick(object sender, EventArgs e)
        {
            gridToALU.Children.Remove(text_ToALU);
            (sender as DispatcherTimer).Stop();
        }
        private void dtRevealtext_ToALU_Tick(object sender, EventArgs e)
        {
            text_ToALU.Visibility = Visibility.Visible;
            (sender as DispatcherTimer).Stop();
        }
        private void dtStoreAndEmphasise_Tick(object sender, EventArgs e)
        {
            char[] ToStore = new char[6];
            int placevalue = 100000;//placevalue of the 6th digit
            for (int i = 0; i < ToStore.Length; i++)
            {
                ToStore[i] = (char)('0' + (int.Parse(text_DataBus.Text) / placevalue) % 10);
                placevalue /= 10;
            }
            int MemLocationNum = int.Parse(text_AddressBus.Text);
            StackPanel MemPanel = (MemLocationNum >= lookup_MemorySpec[MemorySpec] / 2) ? memoryStackPanel2 : memoryStackPanel1;
            (((MemPanel.Children[MemLocationNum % 10]) as DockPanel).Children[1] as TextBlock).Text = new string(ToStore);
            (((MemPanel.Children[MemLocationNum % 10]) as DockPanel).Children[1] as TextBlock).Background = Brushes.Goldenrod;
            DispatcherTimer dtEmphasiseStoredData = new DispatcherTimer();
            dtEmphasiseStoredData.Interval = TimeSpan.FromMilliseconds(lookup_ClockSpeedSpec[ClockSpeedSpec] / 8);
            dtEmphasiseStoredData.Tick += dtEmphasisData_Tick_2;
            dtEmphasiseStoredData.Start();
            (sender as DispatcherTimer).Stop();
        }
        #endregion
    }
}
