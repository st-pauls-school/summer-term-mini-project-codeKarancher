using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Animation;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using CSProjectGame;

namespace CSProjectGame
{
    public static class KSTasks
    {
        interface ITaskObjective
        {
            bool? CheckIfCompleted(bool ThrowExcp);
        }

        /// <summary>
        /// Parent class for OutputObjective and MemoryObjective
        /// </summary>
        private class TaskObjective : ITaskObjective
        {
            public virtual bool? CheckIfCompleted(bool ThrowExcp)
            {
                return null;
            }
        }

        private class OutputObjective : TaskObjective
        {
            string sDesiredOutput;

            public OutputObjective(string DesiredOutput)
            {
                sDesiredOutput = DesiredOutput;
            }

            public override bool? CheckIfCompleted(bool ThrowExcp)
            {
                //Check output box
                throw new NotImplementedException();
            }
        }

        private class MemoryObjective : TaskObjective
        {
            string[] sDesiredMemoryContents;
            int iStartPointer;

            public MemoryObjective(int startindex, string[] desiredvalues)
            {
                iStartPointer = startindex;
                sDesiredMemoryContents = new string[desiredvalues.Length];
                desiredvalues.CopyTo(sDesiredMemoryContents, 0);
            }

            public override bool? CheckIfCompleted(bool ThrowExcp)
            {
                try
                {
                    if (sDesiredMemoryContents.Length + iStartPointer > KSGlobal.S_MemoryCells.Length)
                        return false;

                    string[] memToCheck = new string[sDesiredMemoryContents.Length];
                    for (int curloc = iStartPointer; curloc < iStartPointer + memToCheck.Length; curloc++)
                        memToCheck[curloc - iStartPointer] = KSGlobal.S_MemoryCells[curloc];

                    //check if memory contents match desired contents
                    int index;
                    for (index = 0; index < sDesiredMemoryContents.Length; index++)
                        if (sDesiredMemoryContents[index] != memToCheck[index])
                            break;
                    if (index == sDesiredMemoryContents.Length)//no inequalities were encountered
                        return true;
                    return false;
                } catch (Exception exception)
                {
                    if (ThrowExcp)
                        throw exception;
                    return null;
                }
            }
        }

        public class Task
        {
            TaskObjective _TObjective;
            public readonly string sTitle;
            public readonly string sMessage;
            int _CompletionStatus;//0 - incomplete, 1 - complete and reward ready, 2 - complete and reward redeemed
            public int CompletionStatus { get => _CompletionStatus; }
            public readonly int Reward;

            /// <summary>
            /// Initialize a task which is completed when a range of memory locations hold an array of desired values
            /// </summary>
            /// <param name="TaskMessage">A brief explanation of the task for the user</param>
            /// <param name="StartIndex">The index of the first memory location to be checked</param>
            /// <param name="DesiredValues">The array of desired values to compare the memory to</param>
            /// <param name="Reward">The amount of in-game currency earned by completing the task</param>
            public Task(string TaskTitle, string TaskMessage, int StartIndex, string[] DesiredValues, int reward)
            {
                sTitle = TaskTitle;
                sMessage = TaskMessage;
                _TObjective = new MemoryObjective(StartIndex, DesiredValues);
                _CompletionStatus = 0;
                Reward = reward;
            }

            /// <summary>
            /// Initialize a task which is completed when the computer outputs a specific string
            /// </summary>
            /// <param name="TaskMessage">A brief explanation of the task for the user</param>
            /// <param name="DesiredOutput">The string that the output should be compared to (the desired output)</param>
            /// <param name="Reward">The amount of in-game currency earned by completing the task</param>
            public Task(string TaskTitle, string TaskMessage, string DesiredOutput,  int reward)
            {
                sTitle = TaskTitle;
                sMessage = TaskMessage;
                _TObjective = new OutputObjective(DesiredOutput);
                _CompletionStatus = 0;
                Reward = reward;
            }

            /// <summary>
            /// Returns whether the task condition has been satisfied (true) or not (false). 
            /// Returns null upon error, or throws exception depending on argument.
            /// </summary>
            /// <param name="ThrowExcp">Allows the process to throw an exception if necessary.
            /// When false, the process instead returns null</param>
            /// <returns></returns>
            public bool? CheckIfConditionSatisfied(bool ThrowExcp)
            {
                bool? Result = _TObjective.CheckIfCompleted(ThrowExcp);
                if (Result == true && _CompletionStatus == 0)
                    _CompletionStatus = 1;
                return Result;
            }

            public void Redeem()
            {
                _CompletionStatus = 2;
            }

            /// <summary>
            /// Be careful! This function shoud only be used in special cases. In expected scenarios the CheckIfConditionSatisfied and Redeem functions will change the completion status automatically.
            /// </summary>
            /// <param name="status"></param>
            public void SetCompletionStatusTo(int status)
            {
                _CompletionStatus = status;
            }
        }

        public class TaskCollection
        {
            List<Task> tasks;
            public int Count { get => tasks.Count; }

            public Task GetTask(int index)
            {
                return tasks[index];
            }

            public TaskCollection(List<Task> Tasks)
            {
                Task[] temp = new Task[Tasks.Count];
                Tasks.CopyTo(temp);
                tasks = temp.ToList();
            }

            /// <summary>
            /// The task collection with all the quests retrieved from the account file. ONLY CALL AFTER CALLING KSFILEMANAGEMENT.RETRIEVEPROGRESS
            /// </summary>
            /// <returns></returns>
            public static TaskCollection QuestsFromFile()
            {
                int[] QuestStats = KSFileManagement.QuestStatsFromFile;
                Task[] Groundstate = new Task[]
                {
                    new Task("My First Program", "Every legend has a beginning. Store the numbers 1 to 5 in memory locations 11 to 15", 11, new string[] { "000001", "000002", "000003", "000004", "000005" }, 10),
                    new Task("Exploring", "Store the value in location 19 + 5 in memory location 15", 15, new string[] { "000005" }, 15),
                    new Task("Third Trial", "Store the number 5 in memory location 0", 0, new string[] {"000005" }, 20)
                };
                //INITQUESTSDEBUG
                //KSGlobal.AllQuests.CopyTo(Groundstate);
                if (Groundstate.Length != KSGlobal.NUMQUESTS)
                    throw new Exception("KSGlobal has conflicting AllQuests and NUMQUESTS");
                for (int curQ = 0; curQ < KSGlobal.NUMQUESTS; curQ++)
                    Groundstate[curQ].SetCompletionStatusTo(QuestStats[curQ]);
                return new TaskCollection(Groundstate.ToList());
            }

            public int[] GetCompletionStats()
            {
                int[] ToReturn = new int[tasks.Count];
                for (int i = 0; i < tasks.Count; i++)
                    ToReturn[i] = tasks[i].CompletionStatus;
                return ToReturn;
            }
        }

        public static DockPanel dockpanelTaskViewer(Task task, double height, double width, FontFamily fontfam, Color ColourScheme, RoutedEventHandler Button_Redeem_Click, MouseEventHandler Button_Redeem_MouseEnter)
        {
            DockPanel ToReturn = new DockPanel { Height = height, Width = width };
            int csR = ColourScheme.R, csG = ColourScheme.G, csB = ColourScheme.B;
            Color clightest, clight, cdark, cdarkest;
            clightest = Color.FromRgb((byte)(csR * 1.4), (byte)(csG * 1.4), (byte)(csB * 1.4));
            clight = Color.FromRgb((byte)(csR * 1.25), (byte)(csG * 1.25), (byte)(csB * 1.25));
            cdark = Color.FromRgb((byte)(csR * 0.9), (byte)(csG * 0.9), (byte)(csB * 0.9));
            cdarkest = Color.FromRgb((byte)(csR * 0.7), (byte)(csG * 0.7), (byte)(csB * 0.7));
            const int GRADANGLE = 20;//In degrees
            const float FONTSIZE = 13;
            bool IsFontWhite = (csR + csG + csB < 100);
            Brush brushRedeemButtonBackground = new LinearGradientBrush(Color.FromRgb(0, 113, 143), Color.FromRgb(10, 133, 153), GRADANGLE);
            if (task.CompletionStatus == 0)//Quest not yet completed, textblock is sufficient
            {
                ToReturn.Background = new LinearGradientBrush(cdark, clightest, GRADANGLE);
                ToReturn.Children.Add(new TextBlock()
                {
                    Text = ViewableMessageFromLongMessage(task.sMessage, width, FONTSIZE),
                    ToolTip = task.sMessage,
                    Height = height,
                    Width = width,
                    Padding = new Thickness(width / 50, 0, width / 50, 0),
                    FontFamily = fontfam,
                    FontSize = FONTSIZE,
                    TextWrapping = TextWrapping.Wrap,
                    Background = Brushes.Transparent,
                    Foreground = (IsFontWhite) ? Brushes.White : Brushes.Black,
                    Visibility = Visibility.Visible
                });
            }
            else if (task.CompletionStatus == 1)//Quest completed, reward to be redeemed
            {
                ToReturn.Background = new LinearGradientBrush(clight, clightest, GRADANGLE);
                ToReturn.Children.Add(new TextBlock()
                {
                    Text = ViewableMessageFromLongMessage(task.sMessage, 0.8 * width, FONTSIZE),
                    ToolTip = task.sMessage,
                    Height = height,
                    Width = 0.8 * width,
                    Padding = new Thickness(width / 50, 0, width / 50, 0),
                    FontFamily = fontfam,
                    FontSize = FONTSIZE,
                    TextWrapping = TextWrapping.Wrap,
                    Background = Brushes.Transparent,
                    Foreground = (IsFontWhite) ? Brushes.White : Brushes.Black,
                    Visibility = Visibility.Visible
                });
                Button bRedeem = new Button()
                {
                    Content = "Redeem " + task.Reward + " " + KSGlobal.GAMECURRENCYNAME + "!",
                    FontFamily = fontfam,
                    FontSize = FONTSIZE,
                    Width = width * 0.2,
                    Height = height,
                    Style = KSGlobal.ButtonStyleRedeem,
                    Foreground = Brushes.White,
                    Visibility = Visibility.Visible
                };
                bRedeem.Click += Button_Redeem_Click;
                bRedeem.MouseEnter += Button_Redeem_MouseEnter;
                MouseEventHandler mouseLeave = new MouseEventHandler((object sender, MouseEventArgs mea) =>
                {
                    Button b = sender as Button;
                    b.Content = "Redeem " + task.Reward + " " + KSGlobal.GAMECURRENCYNAME + "!";
                    b.FontFamily = fontfam;
                    b.FontSize = FONTSIZE;
                    b.Width = width * 0.2;
                    b.Height = height;
                    b.Style = KSGlobal.ButtonStyleRedeem;
                    b.Foreground = Brushes.White;
                    b.Visibility = Visibility.Visible;
                });
                bRedeem.MouseLeave += mouseLeave;
                ToReturn.Children.Add(bRedeem);
            }
            else if (task.CompletionStatus == 2)//Quest completed, reward redeemed
            {
                ToReturn.Background = new LinearGradientBrush(cdarkest, cdark, GRADANGLE);
                ToReturn.Children.Add(new TextBlock()
                {
                    Text = ViewableMessageFromLongMessage(task.sMessage, 0.8 * width, FONTSIZE),
                    ToolTip = task.sMessage,
                    Height = height,
                    Width = 0.8 * width,
                    Padding = new Thickness(width / 50, 0, width / 50, 0),
                    FontFamily = fontfam,
                    FontSize = FONTSIZE,
                    TextWrapping = TextWrapping.Wrap,
                    Background = Brushes.Transparent,
                    Foreground = (IsFontWhite) ? Brushes.White : Brushes.Black,
                    Visibility = Visibility.Visible
                });
                ToReturn.Children.Add(new TextBlock()
                {
                    Text = "Redeemed " + task.Reward + " " + KSGlobal.GAMECURRENCYNAME,
                    Height = height,
                    Width = width * 0.2,
                    FontFamily = fontfam,
                    FontSize = FONTSIZE,
                    TextAlignment = TextAlignment.Center,
                    Background = new SolidColorBrush(Color.FromRgb(10, 95, 200)),
                    Foreground = Brushes.White,
                    Visibility = Visibility.Visible
                });
            }
            else
            {
                throw new Exception("This KSTasks.Task has a completion status that is none of the expected values");
            }
            return ToReturn;
        }

        private static string ViewableMessageFromLongMessage(string sMessage, double WidthOfTextBlock, float FontSize)
        {
            string ToReturn = "";

            float widthofcharacter = FontSize * 0.8F;
            int MaxLength = (int)(WidthOfTextBlock / widthofcharacter - 4);
            if (MaxLength > sMessage.Length)
                return sMessage;
            char[] car = sMessage.ToCharArray();
            for (int i = 0; i < MaxLength; i++)
                ToReturn += car[i];
            ToReturn += " ...";

            return ToReturn;
        }
    }
}
