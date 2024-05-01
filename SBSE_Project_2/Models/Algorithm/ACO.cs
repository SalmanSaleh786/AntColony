using Esri.ArcGISRuntime.Geometry;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using SBSE_Project_2.Enums;
using SBSE_Project_2.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Networking.Sockets;
using WinRT;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SBSE_Project_2.Models.Algorithm
{
    internal class ACO
    {
        private GraphNode source;
        private GraphNode destination;
        private ACOGraph acoGraph;
        private BilFileReader bilFileReader;
        private int antsCount = 3000;
        private float K = 2.5f;
        //private double subsetRatio = 0.3;
        //private int subsetAntsForInitPop;
        private Ant[] ants;
        private double evaporationRate = 0.4;
        private float distRatio = 0.15f;
        private float altRatio = 0.85f;
        Random rand;
        int terminationCounter = 100;
        internal ACOGraph ACOGraph { get => acoGraph; set => acoGraph = value; }
        internal ACO()
        {
            source = null;
            destination = null;
            bilFileReader = null;
            ants = new Ant[antsCount];
            for (int i = 0; i < antsCount; ++i)
            {
                ants[i] = new Ant();
            }
            // subsetAntsForInitPop = Convert.ToInt32(antsCount * subsetRatio);
            rand = new Random(System.DateTime.Now.Second);
        }


        public void SetBilFileReader(BilFileReader bilFileReader)
        {
            this.bilFileReader = bilFileReader;
        }
        public void UpdateSrcDest(GraphNode pt, PointType ptType)
        {
            if (pt == null) return;
            if (ptType == PointType.SourcePoint)
            {
                source = pt;
            }
            else if (ptType == PointType.DestinationPoint)
            {
                destination = pt;
            }
        }
        public void ClearAll()
        {
            source = null;
            destination = null;
            if(ACOGraph is not null)
                ACOGraph.ClearAll();
            ants = new Ant[antsCount];
            for (int i = 0; i < antsCount; ++i)
            {
                ants[i] = new Ant();
            }
        }
        public async void StartACO(MapViewModel mapVM, TextBlock fitnessTextblock, DispatcherQueue dispatcherQueue)
        {
            
            if (source is null)
                throw new Exception("Source is not found!");
            if (destination is null)
                throw new Exception("Destination is not found!");
            if (ACOGraph.CostPhermoneData.Length == 0)
                throw new Exception("Make sure graph is generated!");
            bool firstTime = true;
            Ant bestAnt = null;
            await Task.Run(() =>
            {
                //total iterations
                for (int termCompleted = 0; termCompleted < terminationCounter; ++termCompleted)
                {
                    //each ant goes from source to destination
                    foreach (Ant ant in ants)
                    {
                        ant.LevelsTraversed.Clear();
                        ant.NodesSelected.Clear();

                        ant.LevelsTraversed.Add(-1);
                        ant.NodesSelected.Add(new Tuple<int, GraphNode>(0, source));
                        for (int i = 0; i < ACOGraph.CostPhermoneData.Length; i++)
                        {
                            var currLevel = ACOGraph.CostPhermoneData[i];
                            var currIdx = ChoosePathBasedOnPheromone(currLevel, firstTime);

                            ant.LevelsTraversed.Add(i);
                            var location = ACOGraph.CostPhermoneData[i][currIdx].Location;
                            var alt = ACOGraph.CostPhermoneData[i][currIdx].Altitude;
                            ant.NodesSelected.Add(new Tuple<int, GraphNode>(currIdx,
                                new GraphNode(location, alt)));
                        }
                        ant.NodesSelected.Add(new Tuple<int, GraphNode>(0, destination));
                        ant.LevelsTraversed.Add(ACOGraph.CostPhermoneData.Length);
                    }
                    firstTime = false;
                    //All ants have reached the destination once, now they will go back to source
                    //and update pheromone
                    foreach (Ant ant in ants)
                    {
                        //Skipping source and destination level
                        for (int i = ant.NodesSelected.Count - 2; i >= 1; i -= 2)
                        {
                            var currLevel = ant.LevelsTraversed[i];
                            var currNode = ant.NodesSelected[i];

                            var prevLevel = ant.LevelsTraversed[i - 1];
                            var prevNode = ant.NodesSelected[i - 1];

                            if (prevLevel == -1)
                                break;
                            var altDist = Utils.FindAltDistanceMeters(bilFileReader,
                                currNode.Item2.Position,
                                prevNode.Item2.Position);
                            double alt = altDist.Item1;
                            //possible altitude is supposed from 0-3000 m
                            alt = Utils.CovnertValueToRange(3000, 0, 1, 0, alt);
                            double dist = altDist.Item2;
                            //possible range in 2 possible levels is 1000-10000
                            dist = Utils.CovnertValueToRange(10000, 1000, 1, 0, dist);
                            var currPhermone = ACOGraph.CostPhermoneData[prevLevel][prevNode.Item1].Phermone;

                            double newPhermone = (1 - alt)* altRatio + (1 - dist) * distRatio;


                            newPhermone = (1 - evaporationRate) * currPhermone + (K / newPhermone);

                            ACOGraph.CostPhermoneData[prevLevel][prevNode.Item1].Phermone = newPhermone;
                           
                        }

                    }
                    double bestLocalFitnessValue = double.MaxValue;
                    Ant bestLocalAnt = null;

                    foreach (Ant ant in ants)
                    {
                        var fitness = ant.GetFitnessValue();
                        if (bestLocalFitnessValue > fitness)
                        {
                            bestLocalFitnessValue = fitness;
                            bestLocalAnt = ant;
                            if (bestAnt == null)
                                bestAnt = bestLocalAnt;
                            else
                            {
                                if (bestAnt.GetFitnessValue() > fitness)
                                    bestAnt = (Ant)bestLocalAnt.Clone();
                            }
                        }
                    }
                    
                    //fitnessTextblock.XamlRoot = xamlRoot;
                    dispatcherQueue.TryEnqueue(() =>
                    {
                        mapVM.DrawPath(bestLocalAnt, bestPath: false);
                        fitnessTextblock.Text = "Term:"+termCompleted+"/"+ terminationCounter +" Alt: " + Math.Round(bestLocalFitnessValue, 1).ToString()+" m";
                    });
                    
                   
                }
            });
            var fitness = bestAnt.GetFitnessValue();
            var range = bestAnt.GetRangeKm();
            dispatcherQueue.TryEnqueue(() =>
            {
                fitnessTextblock.Text = "BEST FITNESS - Alt: " + Math.Round(fitness, 1).ToString()+" m RANGE: "+range+" Km";
            });
            mapVM.DrawPath(bestAnt, bestPath : true);
            //foreach (Ant ant in ants)
            //{
            //    mapVM.DrawPath(ant);
            //}
        }
        private int ChoosePathBasedOnPheromone(CostPhermoneData[] nextLevel, bool firsttime = false)
        {


            int randomIdx = rand.Next(nextLevel.Length);
            int selectedIdx = randomIdx;

            if (firsttime)
                return randomIdx;  //random index 
            
            var sumOfPhermoneNextLevel = nextLevel.Sum(x => x.GetTotalPhermone());
            double maxProbability = 0;
            for (int i = 0; i < nextLevel.Length; ++i)
            {
                CostPhermoneData costPhermone = nextLevel[i];

                var Pi = costPhermone.GetTotalPhermone() / sumOfPhermoneNextLevel;

                if (Pi > maxProbability)
                {
                    selectedIdx = i;
                    maxProbability = Pi;
                }
            }
            double prob = rand.Next(10) * 0.1;
            if (prob > 0.7)
                return randomIdx;
            return selectedIdx;
        }
        public ACOGraph GenerateGraph()
        {
            if (source is null)
                throw new Exception("Source is not found!");
            if (destination is null)
                throw new Exception("Destination is not found!");

            List<MapPoint> points = new List<MapPoint>() { source.Position, destination.Position };
            Polyline stLine = new Polyline(points, SpatialReferences.Wgs84);
            double bearing = Utils.CalculateBearingDeg(source.Position, destination.Position);
            bearing = 360 - bearing;
            double verticalDistMeters = GeometryEngine.LengthGeodetic(stLine, LinearUnits.Meters, GeodeticCurveType.Geodesic);
            double horizontalDistMeters = 5000;
            int verticalDeltaMeters = 1000;  //Y distance between levels
            int horizontalDeltaMeters = 1000; //X distance between nodes
            var levels = Convert.ToInt32(Math.Floor(verticalDistMeters / verticalDeltaMeters)) - 1;
            ACOGraph = new ACOGraph(levels);
            //ACOGraph.ClearAll();
            int levelIdx = -1;
            for (int i = verticalDeltaMeters; i < verticalDistMeters; i += verticalDeltaMeters)
            {
                ++levelIdx;
                if (levelIdx == levels)
                    break;
                var centerMpt = Utils.FindPointAtDistanceFrom(source.Position, bearing, i / 1000.0);
                double leftSide = bearing - 90;
                if (leftSide < 0)
                {
                    leftSide += 360;
                }
                double rightSide = bearing + 90;
                if (rightSide > 360)
                    rightSide -= 360;
                double customBearing = leftSide;
                int count = 0;
                int nodexIdx = -1;
                int nodesCount = Convert.ToInt32(horizontalDistMeters / horizontalDeltaMeters) * 2;
                ACOGraph.CostPhermoneData[levelIdx] = new CostPhermoneData[nodesCount];
                for (int k = 0; k < ACOGraph.CostPhermoneData[levelIdx].Length; ++k)
                {
                    ACOGraph.CostPhermoneData[levelIdx][k] = new CostPhermoneData();
                }
                while (count < 2)
                {
                    for (int j = horizontalDeltaMeters; j < horizontalDistMeters; j += horizontalDeltaMeters)
                    {
                        ++nodexIdx;
                        var horizontalMpt = Utils.FindPointAtDistanceFrom(centerMpt, customBearing, j / 1000.0);
                        double alt = bilFileReader.GetAltitude(horizontalMpt.Y, horizontalMpt.X);
                        ACOGraph.CostPhermoneData[levelIdx][nodexIdx].Location = horizontalMpt;
                        ACOGraph.CostPhermoneData[levelIdx][nodexIdx].Altitude = alt;
                    }

                    customBearing = rightSide;
                    ++nodexIdx;
                    ACOGraph.CostPhermoneData[levelIdx][nodexIdx].Location = centerMpt;
                    ACOGraph.CostPhermoneData[levelIdx][nodexIdx].Altitude = bilFileReader.GetAltitude(centerMpt.Y, centerMpt.X);


                    //ACOGraph.CostPhermoneData[levelIdx][nodexIdx].PhermoneCost = double.MaxValue;
                    //ACOGraph.CostPhermoneData[levelIdx][nodexIdx].Cost = bilFileReader.GetAltitude(centerMpt.Y, centerMpt.X);
                    ++count;
                }

            }
            return ACOGraph;
        }

    }
}
