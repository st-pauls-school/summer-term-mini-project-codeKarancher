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
    public class Output : object
    {
        public readonly int ioutputType;//0->string, 1->int
        public readonly string sOut;
        public readonly int iOut;

        public Output(string sOutput)
        {
            sOut = sOutput;
            ioutputType = 0;
        }

        public Output(int iOutput)
        {
            iOut = iOutput;
            ioutputType = 1;
        }

        public Output MakeCopy()
        {
            if (ioutputType == 0)
                return new Output(sOut);
            return new Output(iOut);
        }
    }

    public class TaskObjective
    {


        virtual public bool? CheckIfTaskCompleted()
        {
            throw new Exception("TaskObjective has not been assigned to OutputObjective or MemoryObjective");
        }
    }

    public class OutputObjective : TaskObjective
    {
        Output _DesiredOut;

        public OutputObjective(Output DesiredOut)
        {
            _DesiredOut = DesiredOut.MakeCopy();
        }

        public override bool? CheckIfTaskCompleted()
        {
            Output opToCompare = new Output(0);//DEBUG
            if (_DesiredOut == opToCompare)
                return true;
            return false;
        }
    }

    public class MemoryObjective : TaskObjective
    {

    }

    public class Task
    {
        TaskObjective _TObjective;
        string _sMessage;


    }
}
