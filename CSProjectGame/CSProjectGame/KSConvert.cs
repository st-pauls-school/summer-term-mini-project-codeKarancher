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
    }
}
