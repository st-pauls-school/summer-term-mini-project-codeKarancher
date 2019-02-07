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
            string _sMessage;

            /// <summary>
            /// Initialize a task which is completed when a range of memory locations hold an array of desired values
            /// </summary>
            /// <param name="TaskMessage">A brief explanation of the task for the user</param>
            /// <param name="StartIndex">The index of the first memory location to be checked</param>
            /// <param name="DesiredValues">The array of desired values to compare the memory to</param>
            public Task(string TaskMessage, int StartIndex, string[] DesiredValues)
            {
                _sMessage = TaskMessage;
                _TObjective = new MemoryObjective(StartIndex, DesiredValues);
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
                return _TObjective.CheckIfCompleted(ThrowExcp);
            }
        }
    }
}
