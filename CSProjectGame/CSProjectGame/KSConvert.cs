using System;

namespace CSProjectGame
{
    public class KSConvert
    {
        public static int BinaryToDecimalForRegisters(char[] BinaryNumber)
        {
            int sum = 0;
            int power = 7;
            for (int i = 0; i < 9; i++)
            {
                if (i == 4)
                    i++;
                if (BinaryNumber[i] == '1')
                {
                    sum += (int)Math.Pow(2, power);
                }
                power--;
            }
            return sum;
        }

        public static char[] DecimalToBinaryForRegisters(int DecimalNumber)
        {
            char[] ToReturn = new char[9];
            ToReturn[4] = ' ';
            int ToConvert = DecimalNumber;
            int power = 7;
            for (int index = 0; index < 9; index++)
            {
                if (index == 4)
                    index++;
                if (ToConvert >= Math.Pow(2, power))
                {
                    ToReturn[index] = '1';
                    ToConvert -= (int)Math.Pow(2, power);
                }
                else
                    ToReturn[index] = '0';
                power--;
            }
            return ToReturn;
        }

        /// <summary>
        /// Customized algorithm to be used only for one or two digit integers
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static char[] IntTo2DigCharArray(int input)
        {
            if (input / 10 == 0)    //single digit
                return new char[] { '0', (char)(input + '0') };
            else    //double digit
                return new char[] { (char)(input / 10 + '0'), (char)(input % 10 + '0') };
        }
    }
}
