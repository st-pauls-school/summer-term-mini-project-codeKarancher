using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Animation;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using CSProjectGame;

namespace CSProjectGame
{
    public static class StoreProcedures
    {
        /// <summary>
        /// Handles the event of the user attempting to buy a register, returning 0 for a success, 1 for 'not enough earnings', -1 for 'cannot purchase more registers'.
        /// </summary>
        /// <param name="NumReg">Number of registers owned by player</param>
        /// <param name="Earnings">Earnings in player's wallet</param>
        /// <returns></returns>
        public static int CanUserBuyRegister(int NumReg, int Earnings)
        {
            if (NumReg == 6)
                return -1;
            int cost = CostOfNewRegister(NumReg);
            if (Earnings < cost)
                return 1;
            return 0;
        }

        public static int CostOfNewRegister(int NumReg)
        {
            return NumReg * 20;
        }

        public static int UpgradeCS(ref int CSSpec, int Earnings)
        {
            if (CSSpec == 6)
                return -1;
            int cost = CostOfUpgradeCS(CSSpec);
            if (Earnings < cost)
                return 1;
            Earnings -= cost;
            CSSpec++;
            return 0;
        }

        public static int CostOfUpgradeCS(int CSSpec)
        {
            return CSSpec * 30 + 25;
        }

        public static int UpgradeMem(ref int MemSpec, int Earnings)
        {
            if (MemSpec == 6)
                return -1;
            int cost = CostOfUpgradeMem(MemSpec);
            if (Earnings < cost)
                return 1;
            Earnings -= cost;
            MemSpec++;
            return 0;
        }

        public static int CostOfUpgradeMem(int MemSpec)
        {
            return MemSpec * 15 + 15;
        }
    }
}
