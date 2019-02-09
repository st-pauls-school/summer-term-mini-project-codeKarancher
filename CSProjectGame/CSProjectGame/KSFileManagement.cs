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
         * 2 bytes: Number of parts
         * KSGlobal.NumberOfQuests bytes: 1 byte per quest's status
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

        private static int _NumTabsFromFile;
        private static string[] _TabNamesFromFile;
        private static string[] _TabTextsFromFile;
        private static int _NumRegFromFile;
        private static int _ALUSpecFromFile;
        private static int _ClockSpeedSpecFromFile;
        private static int _MemSpecFromFile;
        private static UInt16 _NumParts;
        private static int[] _CompletionStats;

        public static int NumTabsFromFile { get => _NumTabsFromFile; }
        public static string[] TabNamesFromFile { get => _TabNamesFromFile; }
        public static string[] TabTextsFromFile { get => _TabTextsFromFile; }
        public static int NumRegFromFile { get => _NumRegFromFile; }
        public static int ALUSpecFromFile { get => _ALUSpecFromFile; }
        public static int ClockSpeedSpecFromFile { get => _ClockSpeedSpecFromFile; }
        public static int MemSpecFromFile { get => _MemSpecFromFile; }
        public static UInt16 NumParts { get => _NumParts; }
        public static int[] CompletionStats { get => _CompletionStats; }

        public static byte[] HashOfCorrectPasscode(BinaryReader binRead)
        {
            byte[] Hash = new byte[20];
            for (int i = 0; i < 20; i++)
                Hash[i] = binRead.ReadByte();

            return Hash;
        }

        public static void RetrieveProgress(BinaryReader binRead)
        {
            try
            {
                binRead.BaseStream.Position = 20;
                _NumTabsFromFile = binRead.ReadByte();
                _TabNamesFromFile = new string[_NumTabsFromFile];
                _TabTextsFromFile = new string[_NumTabsFromFile];
                for (int curTab = 0; curTab < _NumTabsFromFile; curTab++)
                {
                    byte curByte;
                    List<char> curCharString = new List<char>();
                    while ((curByte = binRead.ReadByte()) != TABSEP)
                    {
                        binRead.BaseStream.Position--;
                        curCharString.Add(binRead.ReadChar());
                    }
                    _TabNamesFromFile[curTab] = new string(curCharString.ToArray());
                    curCharString = new List<char>();
                    while ((curByte = binRead.ReadByte()) != TABEND)
                    {
                        binRead.BaseStream.Position--;
                        curCharString.Add(binRead.ReadChar());
                    }
                    TabTextsFromFile[curTab] = new string(curCharString.ToArray());
                }
                _NumRegFromFile = binRead.ReadByte();
                _ALUSpecFromFile = binRead.ReadByte();
                _ClockSpeedSpecFromFile = binRead.ReadByte();
                _MemSpecFromFile = binRead.ReadByte();
                _NumParts = (UInt16)(binRead.ReadByte() * 256 + binRead.ReadByte());
                //for (int i = 0; i < 1/*KSGlobal.NUMBEROFQUESTS*/; i++)
                //    _CompletionStats[i] = binRead.ReadByte();
                binRead.Close();
            }
            catch (Exception e)
            {
                throw e;//new Exception("Progress was attempted to be retrieved from a file that progress had not been saved to.");
            }
        }

        public static void SaveProgress(BinaryWriter binWrite, int NumTabs, string[] TabNames, string[] TabTexts, int NumRegisters, int ALUSpec, int ClockSpeedSpec, int MemSpec, UInt16 Parts, KSTasks.Task[] Quests)
        {
            binWrite.BaseStream.Position = 20;
            binWrite.Write((byte)NumTabs);
            for (int i = 0; i < NumTabs; i++)
            {
                char[] cArName = TabNames[i].ToCharArray();
                for (int j = 0; j < TabNames[i].Length; j++)
                    binWrite.Write(cArName[j]);
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
            byte bparts2 = (byte)(Parts % 256);
            Parts /= 256;
            byte bparts1 = (byte)Parts;
            binWrite.Write(bparts1);
            binWrite.Write(bparts2);
            for (int curQ = 0; curQ < 1/*KSGlobal.NUMBEROFQUESTS DEBUG*/; curQ++)
                binWrite.Write((byte)Quests[curQ].CompletionStatus);
            binWrite.Close();
        }
    }
}
