using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace CSProjectGame
{
    public class KSAssemblyCode
    {
        public static int[] Interpret(string sCodeText, string[] message)
        {
            string[] Lines = sCodeText.Split('\n');
            int[] ToReturn = new int[Lines.Length];
            string[] dictAssembly = new string[] { "LDR", "STR", "ADD", "SUB", "MOV", "CMP", "B", "BEQ", "BNE", "BGT", "BLT", "AND", "ORR" };
            int iReadingCommand;    //0 when reading before command, 1 when reading command, 2 when reading after command
            for (int curLine = 0; curLine < Lines.Length; curLine++)
            {
                char[] curLineChars = Lines[curLine].ToCharArray();
                iReadingCommand = (curLineChars[0] >= 'A' && curLineChars[0] <= 'Z') ? 1 : 0;
                List<char> curLineCommand = new List<char>();
                for (int curChar = 0; curChar < curLineChars.Length; curChar++)
                {
                    if (curLineChars[curChar] == ' ' || curLineChars[curChar] == '\t')//is a break between words
                    {
                        iReadingCommand++;
                        if (iReadingCommand == 2) //command has been read
                        {
                            ToReturn[curLine] = Array.IndexOf(dictAssembly, curLineCommand.ToArray().ToString());
                            message[0] = curLineCommand.ToArray().ToString() + "---" + Array.IndexOf(dictAssembly, curLineCommand.ToArray().ToString());//DEBUG
                        }
                        continue;
                    }
                    if (iReadingCommand == 1)   //currently reading command
                        curLineCommand.Add(curLineChars[curChar]);
                }
            }
            return ToReturn;
        }

        public static string IDE(string sCodeText)
        {
            throw new NotImplementedException();
        }
    }
}
