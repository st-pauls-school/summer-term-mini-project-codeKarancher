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
    public static class KSProcessorInteraction
    {
        /// <summary>
        /// Returns the values in the specified range of memory as strings
        /// </summary>
        /// <param name="startindex">The index of the first value to return</param>
        /// <param name="endindex">The index of the last value to return</param>
        /// <returns></returns>
        public static string[] GetMemoryInRange(int startindex, int endindex)
        {
            string[] ToReturn = new string[endindex - startindex + 1];
            for (int i = startindex; i <= endindex; i++)
                ToReturn[i - startindex] = KSGlobal.S_MemoryCells[i];
            return ToReturn;
        }
    }
}
