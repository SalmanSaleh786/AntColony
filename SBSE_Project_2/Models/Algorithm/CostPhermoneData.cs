using Esri.ArcGISRuntime.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SBSE_Project_2.Models.Algorithm
{
    internal class CostPhermoneData
    {
        private MapPoint location;
        private double altitude;
        private double pheromone = 0.5;

        private double cost = 1;
        public double Altitude { get => altitude; set => altitude = value; }
        public MapPoint Location { get => location; set => location = value; }
        public double Cost { get => cost; set => cost = value; }
        public double Phermone { get => pheromone; set => pheromone = value; }
        
        public double GetTotalPhermone()
        {
            return pheromone; //PhermoneWrtNodes.Sum(x => x.Item1);
        }
    }
}
