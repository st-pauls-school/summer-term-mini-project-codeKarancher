using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace CSProjectGame
{
    public static class KSFileManagement
    {
        /*Formatting of file:
         * 1 byte: Number of tabs (x)
         * (x) number of couples representing different tabs
         * * Each couple consists of a string representing the name of the tab, then a TABSEP, then a string representing the text in the tab, then a TABEND
         * 1 byte: Number of registers
         * 1 byte: ALU spec
         * 1 byte: Clock Speed spec
         * 1 byte: Memory spec
         */

        /*How to use class:
         * Reading from the game file:
         *  First execute 'RetrieveProgress'
         *  Fields will now contain respective values obtained from the file
         *  Use fields to assign variables outside the class
         *  
         * Writing to the game file:
         *  Just use 'SaveProgress'
         */

        const byte TABSEP = 1;
        const byte TABEND = 2;

        public static int NumTabsFromFile;
        public static string[] TabNamesFromFile;
        public static string[] TabTextsFromFile;
        public static int NumRegFromFile;
        public static int ALUSpecFromFile;
        public static int ClockSpeedSpecFromFile;
        public static int MemSpecFromFile;

        public static void RetrieveProgress(BinaryReader binRead)
        {
            binRead.BaseStream.Position = 0;
            NumTabsFromFile = binRead.ReadByte();
            TabNamesFromFile = new string[NumTabsFromFile];
            TabTextsFromFile = new string[NumTabsFromFile];
            for (int curTab = 0; curTab < NumTabsFromFile; curTab++)
            {
                byte curByte;
                List<char> curCharString = new List<char>();
                while ((curByte = binRead.ReadByte()) != TABSEP)
                {
                    binRead.BaseStream.Position--;
                    curCharString.Add(binRead.ReadChar());
                }
                TabNamesFromFile[curTab] = new string(curCharString.ToArray());
                while ((curByte = binRead.ReadByte()) != TABEND)
                {
                    binRead.BaseStream.Position--;
                    curCharString.Add(binRead.ReadChar());
                }
                TabTextsFromFile[curTab] = new string(curCharString.ToArray());
            }
            NumRegFromFile = binRead.ReadByte();
            ALUSpecFromFile = binRead.ReadByte();
            ClockSpeedSpecFromFile = binRead.ReadByte();
            MemSpecFromFile = binRead.ReadByte();
        }

        public static void SaveProgress(BinaryWriter binWrite, int NumTabs, string[] TabNames, string[] TabTexts, int NumRegisters, int ALUSpec, int ClockSpeedSpec, int MemSpec)
        {
            binWrite.Write((byte)NumTabs);
            for (int i = 0; i < NumTabs; i++)
            {
                char[] cArName = TabNames[i].ToCharArray();
                for (int j = 0; j < TabNames[i].Length; j++)
                    binWrite.Write(cArName[i]);
                binWrite.Write(TABSEP);
                char[] cArText = TabTexts[i].ToCharArray();
                for (int j = 0; j < TabTexts[i].Length; j++)
                    binWrite.Write(cArText[j]);
                binWrite.Write(TABEND);
            }
            binWrite.Write((byte)NumRegisters);
            binWrite.Write((byte)ALUSpec);
            binWrite.Write((byte)ClockSpeedSpec);
            binWrite.Write((byte)MemSpec);
        }
    }
}
