using Esri.ArcGISRuntime.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SBSE_Project_2.Models.Algorithm
{
    internal class Ant : ICloneable
    {
        private List<int> _levelsTraversed=new List<int>();
        private List<Tuple<int, GraphNode>> _nodesSelected=new List<Tuple<int, GraphNode>>();
        internal Ant()
        {

        }
        public List<int> LevelsTraversed { get => _levelsTraversed; set => _levelsTraversed = value; }
        public List<Tuple<int, GraphNode>> NodesSelected { get => _nodesSelected; set => _nodesSelected = value; }

        public object Clone()
        {
            Ant clonedAnt = new Ant();
            for(int i=0; i<LevelsTraversed.Count;  i++)
            {
                clonedAnt.LevelsTraversed.Add(LevelsTraversed[i]);
                clonedAnt.NodesSelected.Add(NodesSelected[i]);
            }
            return clonedAnt;
        }

        public double GetFitnessValue()
        {
            return _nodesSelected.Average(x => x.Item2.Alt);
            //double max=double.MinValue;
            
            //foreach (var item in _nodesSelected)
            //{
            //    if (item.Item2.Alt > max)
            //    {
            //        max = item.Item2.Alt;
            //    }
            //}
            //return max;
        }
        public double GetRangeKm()
        {
            double range = 0;
            List<MapPoint> mpts = new List<MapPoint>();
            for(int i=0; i<_nodesSelected.Count-1; i++)
            {
                var pt1=_nodesSelected[i].Item2.Position;
                var pt2= _nodesSelected[i+1].Item2.Position;
                range+=GeometryEngine.DistanceGeodetic(pt1, pt2, LinearUnits.Meters, AngularUnits.Degrees, GeodeticCurveType.Geodesic).Distance;
            }
            return Math.Round(range/1000.0, 1);

        }
    }
}
