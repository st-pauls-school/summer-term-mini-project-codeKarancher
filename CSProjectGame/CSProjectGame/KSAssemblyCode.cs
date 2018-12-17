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
        /* How instructions are stored:
         * 
         * First digit is the index of the instruction (or instruction type) in the below array 'dictAssembly'
         *  If the first digit indicates an instruction with index less than 4 (either of the first four instructions), the first parameter is a register number, and the second one is always an <operand>/<mem>, so:
         *      digit 2: register number
         *      digit 3: type of addressing (look at case 4)
         *      digit 4: memory location/number - 1st digit
         *      digit 5: memory location/number - 2nd digit
         *      
         *  If the first digit indicates a branching instruction (index 4)
         *      digit 2: type of branch (condition) - 
         *          0: branch no matter what
         *          1: branch if last comparison result was: equal
         *          2: branch if last comparison result was: not equal
         *          3: branch if last comparison result was: greater than
         *          4: branch if last comparison result was: lesser than
         *      digit 3: line number - 1st digit
         *      digit 4: line number - 2nd digit
         *          
         *  If the first digit was more than 4, the first and second parameters are both register numbers, then the third parameter is an <operand>
         *      digit 2: target register number
         *      digit 3: parameter register number
         *      digit 4: type of addressing (look at case 4)
         *      digit 5: memory location/number - 1st digit
         *      digit 6: memory location/number - 2nd digit
         * 
         */

        public static char[][] Interpret(string sCodeText)
        {
            if (sCodeText == null || sCodeText.Length == 0)
                return null;
            string[] RawLines = sCodeText.Split("\r\n");
            List<string> CorrectLines = new List<string>();
            for (int i = 0; i < RawLines.Length; i++)
            {
                if (!(RawLines[i] == null || RawLines[i] == ""))
                    CorrectLines.Add(RawLines[i]);
            }
            string[] Lines = CorrectLines.ToArray();
            char[][] ToReturn = new char[Lines.Length][];
            string[] dictAssembly = new string[] { "LDR", "STR", "MOV", "CMP", "B", "ADD", "SUB", "AND", "ORR" };
            string[] dictBranchConditions = new string[] { "", "EQ", "NE", "GT", "LT" };
            char?[] dictAddressingTypes = new char?[] { null, '#', null, '>' };
            int iWhatIsBeingRead;   //0 when reading before command, 1 when reading command, greater than or equal to 2 when reading parameters
            for (int curLine = 0; curLine < Lines.Length; curLine++)
            {
                bool IsFirstCharInWord = false;
                ToReturn[curLine] = new char[] { '0', '0', '0', '0', '0', '0' };  //6 meaningless digits
                char[] curLineChars = Lines[curLine].ToCharArray();
                iWhatIsBeingRead = (curLineChars[0] >= 'A' && curLineChars[0] <= 'Z') ? 1 : 0;
                if (iWhatIsBeingRead == 1) IsFirstCharInWord = true;
                List<char> curWord = new List<char>();
                for (int curChar = 0; curChar < curLineChars.Length; curChar++)
                {
                    if (curLineChars[curChar] == 'R' && IsFirstCharInWord)
                        continue;
                    if (curLineChars[curChar] == ',' || curLineChars[curChar] == ' ' || curLineChars[curChar] == '\t' || curChar == curLineChars.Length - 1)//is a break between words
                    {
                        if (curChar == curLineChars.Length - 1 && !(curLineChars[curChar] == ',' || curLineChars[curChar] == ' ' || curLineChars[curChar] == '\t' || curLineChars[curChar] == '\n'))
                            curWord.Add(curLineChars[curChar]);
                        if (curWord.Count == 0)//due to multiple consecutive breaks
                            continue;
                        IsFirstCharInWord = true;   //next char will be the first character in the word
                        iWhatIsBeingRead++;
                        switch (iWhatIsBeingRead)
                        {
                            case 2://command has been read
                                if (new string(curWord.ToArray()) == "HALT")
                                {
                                    ToReturn[curLine] = new char[] { '9', '9', '9', '9', '9', '9' };
                                    curWord = new List<char>();
                                    iWhatIsBeingRead = -1;
                                }
                                else
                                    ToReturn[curLine][0] = (char)(Array.IndexOf(dictAssembly, new string(curWord.ToArray())) + '0');
                                break;
                            case 3://first parameter has been read
                                ToReturn[curLine][1] = curWord[0];
                                break;
                            case 4://second parameter has been read, could be a register number - single digit - (ADD, SUB, AND, ORR) or an <op> or <mem> - upto three digits - (LDR, STR, MOV, CMP)
                                if (ToReturn[curLine][0] < '4') // LDR, STR, MOV, CMP -- 3 digits 'xyz' (x-> 0-immediate(#)/1-direct/2-indirect addressing(>), yz-> number)
                                {
                                    int a = Array.IndexOf(dictAddressingTypes, curWord[0]); //will return -1 if direct addressing is being used
                                    ToReturn[curLine][2] = (char)((a + 2) % 3 + '0');//'0' for immediate addressing, '1' for direct, '2' for indirect addressing
                                    switch (a)
                                    {
                                        case -1://curWord is a 1 or 2-digit number
                                            ToReturn[curLine][3] = (curWord.Count == 1) ? '0' : curWord[0];
                                            ToReturn[curLine][4] = curWord[curWord.Count - 1];
                                            break;
                                        default://curWord[1-2] is a 1 or 2-digit number
                                            ToReturn[curLine][3] = (curWord.Count - 1 == 1) ? '0' : curWord[1];
                                            ToReturn[curLine][4] = curWord[curWord.Count - 1];
                                            break;
                                    }
                                    //ToReturn[2-4] contain 'xyz'
                                }
                                else // ADD, SUB, AND, ORR
                                    ToReturn[curLine][2] = curWord[0];
                                break;
                            case 5://third parameter (if any) has been read - must be a 3-digit <op> (ADD, SUB, AND, ORR)
                                int k = Array.IndexOf(dictAddressingTypes, curWord[0]);
                                ToReturn[curLine][3] = (char)((k + 2) % 3 + '0');
                                if (k == -1)
                                {
                                    ToReturn[curLine][4] = (curWord.Count == 1) ? '0' : curWord[0];
                                    ToReturn[curLine][5] = curWord[curWord.Count - 1];
                                }
                                else
                                {
                                    ToReturn[curLine][4] = (curWord.Count - 1 == 1) ? '0' : curWord[1];
                                    ToReturn[curLine][5] = curWord[curWord.Count - 1];
                                }
                                break;
                            case 12://branch statement's condition has been read
                                ToReturn[curLine][1] = (char)(Array.IndexOf(dictBranchConditions, new string(curWord.ToArray())) + '0');
                                break;
                            case 13://line number <num> to branch to has been read
                                if (curWord.Count == 1)//single digit line number
                                {
                                    ToReturn[curLine][2] = '0';
                                    ToReturn[curLine][3] = curWord[0];
                                }
                                else //double digit line number
                                {
                                    ToReturn[curLine][2] = curWord[0];
                                    ToReturn[curLine][3] = curWord[1];
                                }
                                break;
                        }
                        curWord = new List<char>();
                        continue;
                    }
                    else
                    {
                        curWord.Add(curLineChars[curChar]);
                        if (curChar == 0)
                        {
                            if (curWord[0] == 'B')  //The current line is a branching statement
                            {
                                iWhatIsBeingRead = 11;
                                curWord = new List<char>();
                            }
                        }
                    }
                    if (IsFirstCharInWord)
                        IsFirstCharInWord = false;
                    if (iWhatIsBeingRead == -1)
                        break;
                }
                if (iWhatIsBeingRead == -1)
                    break;
            }
            return ToReturn;
        }

        public static string IDE(string sCodeText)
        {
            throw new NotImplementedException();
        }
    }
}
