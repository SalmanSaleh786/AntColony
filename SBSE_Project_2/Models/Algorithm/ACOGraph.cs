using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SBSE_Project_2.Models.Algorithm
{
    internal class ACOGraph
    {
       // private ACOGraphLevel[] levels;
        private CostPhermoneData[][] costPhermoneData;
        internal ACOGraph(int levelsCount)
        {
         //   levels = new ACOGraphLevel[levelsCount];
            costPhermoneData=new CostPhermoneData[levelsCount][];
            
        }
       // internal ACOGraphLevel[] Levels { get => levels; set => levels = value; }
        internal CostPhermoneData[][] CostPhermoneData { get => costPhermoneData;  }
        internal void ClearAll()
        {
            costPhermoneData = null;
            //levels = null;
        }
    }
}
