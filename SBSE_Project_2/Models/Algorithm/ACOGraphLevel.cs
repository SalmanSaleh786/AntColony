using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SBSE_Project_2.Models.Algorithm
{
    internal class ACOGraphLevel
    {
        private List<GraphNode> nodes=new List<GraphNode>();
        public List<GraphNode> Nodes
        {
            get
            {
                return nodes;   
            }
            set
            {
                nodes = value;
            }
        }
    }
}
