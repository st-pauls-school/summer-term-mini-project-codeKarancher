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
    public static class KSGlobal
    {
        private static int numRegisters;
        private static string[] s_Registers;
        private static string[] s_MemoryCells;
        private static int earnings;
        private static TimeSpan runDuration;
        private static char[][] c_c_Instructions;
        public static readonly string GAMECURRENCYNAME = "Parts";
        public static Style ButtonStyleRedeem;
        public static readonly int NUMQUESTS = 3;

        public static int NumRegisters { get => numRegisters; set => numRegisters = value; }
        public static string[] S_Registers { get => s_Registers; set => s_Registers = value; }
        public static string[] S_MemoryCells { get => s_MemoryCells; set => s_MemoryCells = value; }
        public static int Earnings { get => earnings; set => earnings = value; }
        public static TimeSpan RunDuration { get => runDuration; }
        public static char[][] C_c_Instructions { get => c_c_Instructions; }

        public static void SetAll(int NumReg, string[] RegisterContents, string[] MemoryCellContents, int Earnings)
        {
            NumRegisters = NumReg;
            s_Registers = new string[NumReg];
            RegisterContents.CopyTo(s_Registers, 0);
            s_MemoryCells = new string[MemoryCellContents.Length];
            MemoryCellContents.CopyTo(s_MemoryCells, 0);
            earnings = Earnings;
        }

        public static void SetDurationOfRun(TimeSpan ts)
        {
            runDuration = ts;
        }

        public static bool InstructionsContain(int iCommandToCheck)
        {
            //See KSAssemblyCode.cs for how the instructions are stored in the instructions array
            for (int curLine = 0; curLine < c_c_Instructions.Length; curLine++)
                if (c_c_Instructions[curLine][0] == (char)('0' + iCommandToCheck))
                    return true;
            return false;
        }
    }
}
