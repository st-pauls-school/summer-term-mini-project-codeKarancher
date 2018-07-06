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
