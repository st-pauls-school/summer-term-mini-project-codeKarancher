namespace CSProjectGame
{
    public static class KSGlobal
    {
        private static int numRegisters;
        private static string[] s_Registers;
        private static string[] s_MemoryCells;

        public static int NumRegisters { get => numRegisters; set => numRegisters = value; }
        public static string[] S_Registers { get => s_Registers; set => s_Registers = value; }
        public static string[] S_MemoryCells { get => s_MemoryCells; set => s_MemoryCells = value; }

        public static void SetAll(int NumReg, string[] RegisterContents, string[] MemoryCellContents)
        {
            NumRegisters = NumReg;
            RegisterContents.CopyTo(s_Registers, 0);
            MemoryCellContents.CopyTo(s_MemoryCells, 0);
        }
    }
}
