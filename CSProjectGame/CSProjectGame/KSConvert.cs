using System;

namespace CSProjectGame
{
    public class KSConvert
    {
        public static int BinaryToDecimal(char[] BinaryNumber)
        {
            int sum = 0;
            for (int i = 0; i < BinaryNumber.Length; i++)
            {
                if (BinaryNumber[i] == '1')
                {
                    sum += (int)Math.Pow(2, BinaryNumber.Length - 1 - i);
                }
            }
            return sum;
        }

        public static char[] DecimalToBinaryForRegisters(int DecimalNumber)
        {
            if (DecimalNumber == 0)
                return new char[] { '0', '0', '0', '0', ' ', '0', '0', '0', '0' };
            char[] ToRetValueArray = new char[(int)(Math.Log(DecimalNumber) / Math.Log(2)) + 1];
            if (ToRetValueArray.Length > 8)
                return new char[] { 'e', 'r', 'r', 'o', 'r' };
            for (int i = ToRetValueArray.Length - 1; i >= 0; i--)
            {
                ToRetValueArray[i] = (char)(DecimalNumber % 2 + '0');
                DecimalNumber /= 2;
            }
            char[] ToReturn = new char[9];
            for (int i = 0; i < 8 - ToRetValueArray.Length; i++)
                ToReturn[i] = '0';
            for (int i = 8 - ToRetValueArray.Length; i < 8; i++)
                ToReturn[i] = ToRetValueArray[i + ToRetValueArray.Length - 8];
            for (int i = 4; i < 8; i++)
                ToReturn[i + 1] = ToReturn[i];
            ToReturn[4] = ' ';
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
