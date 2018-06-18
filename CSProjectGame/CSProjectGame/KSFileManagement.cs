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
         * (x) number of strings representing different tabs
         * 1 byte: Number of registers
         * 1 byte: ALU spec
         * 1 byte: Clock Speed spec
         * 1 byte: Memory spec
         */

        const byte EOS = 1;
        public static int GetNumTabs(BinaryReader binRead)
        {
            binRead.BaseStream.Position = 0;
            return binRead.ReadByte();
        }

        public static string[] GetTabTexts(BinaryReader binRead)
        {
            int NumTabs = GetNumTabs(binRead);
            string[] TabTexts = new string[NumTabs];
            for (int curTab = 0; curTab < NumTabs; curTab++)
            {
                byte curByte;
                List<char> curCharString = new List<char>();
                while ((curByte = binRead.ReadByte()) != EOS)
                {
                    binRead.BaseStream.Position--;
                    curCharString.Add(binRead.ReadChar());
                }
                TabTexts[curTab] = new string(curCharString.ToArray());
            }
            return TabTexts;
        }

        static int GetInfoAfterTabTexts(BinaryReader binRead, int info)
        {
            int NumTabs = GetNumTabs(binRead);
            for (int i = 0; i < NumTabs; i++)
            {
                while (binRead.ReadChar() != EOS)
                    continue;
            }
            for (int i = 0; i < info; i++)
                binRead.ReadByte();
            return binRead.ReadByte();
        }

        public static int GetNumRegisters(BinaryReader binRead)
        {
            return GetInfoAfterTabTexts(binRead, 0);
        }

        public static int GetALUSpec(BinaryReader binRead)
        {
            return GetInfoAfterTabTexts(binRead, 1);
        }

        public static int GetClockSpeedSpec(BinaryReader binRead)
        {
            return GetInfoAfterTabTexts(binRead, 2);
        }

        public static int GetMemorySpec(BinaryReader binRead)
        {
            return GetInfoAfterTabTexts(binRead, 3);
        }

        public static void SaveProgress(BinaryWriter binWrite, int NumTabs, string[] TabTexts, int NumRegisters, int ALUSpec, int ClockSpeedSpec, int MemSpec)
        {
            binWrite.Write((byte)NumTabs);
            for (int i = 0; i < NumTabs; i++)
            {
                char[] cAr = TabTexts[i].ToCharArray();
                for (int j = 0; j < TabTexts[i].Length; j++)
                    binWrite.Write(cAr[j]);
            }
            binWrite.Write((byte)NumRegisters);
            binWrite.Write((byte)ALUSpec);
            binWrite.Write((byte)ClockSpeedSpec);
            binWrite.Write((byte)MemSpec);
        }
    }
}
