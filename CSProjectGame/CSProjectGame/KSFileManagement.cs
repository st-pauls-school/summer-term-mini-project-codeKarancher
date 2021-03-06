﻿using System;
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
         * 1 byte: Clock Speed spec
         * 1 byte: Memory spec
         * 1 byte: Currency
         * NUMQUESTS bytes: Quests status of each quest in game.
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
        public static int ClockSpeedSpecFromFile;
        public static int MemSpecFromFile;
        public static int EarningsFromFile;
        public static int[] QuestStatsFromFile;

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
                    curCharString = new List<char>();
                    while ((curByte = binRead.ReadByte()) != TABEND)
                    {
                        binRead.BaseStream.Position--;
                        curCharString.Add(binRead.ReadChar());
                    }
                    TabTextsFromFile[curTab] = new string(curCharString.ToArray());
                }
                NumRegFromFile = binRead.ReadByte();
                ClockSpeedSpecFromFile = binRead.ReadByte();
                MemSpecFromFile = binRead.ReadByte();
                EarningsFromFile = binRead.ReadByte();
                QuestStatsFromFile = new int[KSGlobal.NUMQUESTS];
                for (int curQ = 0; curQ < KSGlobal.NUMQUESTS; curQ++)
                    QuestStatsFromFile[curQ] = (int)binRead.ReadByte();
                binRead.Close();
            }
            catch (Exception e)
            {
                throw new Exception("Progress was attempted to be retrieved from a file of incorrect format");
            }
}

        public static void SaveProgress(BinaryWriter binWrite, int NumTabs, string[] TabNames, string[] TabTexts, int NumRegisters, int ClockSpeedSpec, int MemSpec, int Currency, int[] QuestsStats)
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
            binWrite.Write((byte)ClockSpeedSpec);
            binWrite.Write((byte)MemSpec);
            binWrite.Write((byte)Currency);
            for (int curQ = 0; curQ < KSGlobal.NUMQUESTS; curQ++)
                binWrite.Write((byte)QuestsStats[curQ]);
            binWrite.Close();
        }
    }
}
