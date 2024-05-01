using Esri.ArcGISRuntime.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SBSE_Project_2.Models.Algorithm
{
    internal class GraphNode
    {
        private MapPoint position;
        private double alt;
        internal GraphNode(MapPoint position, double alt)
        {
            Position = position;
            Alt = alt;
        }

        public MapPoint Position { get => position; set => position = value; }
        public double Alt { get => alt; set => alt = value; }

    }
}
