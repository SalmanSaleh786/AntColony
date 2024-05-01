using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Rasters;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UI.Controls;
using SBSE_Project_2.Enums;
using SBSE_Project_2.Models;
using SBSE_Project_2.Models.Algorithm;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SBSE_Project_2.ViewModels
{
    public class MapViewModel : INotifyPropertyChanged
    {
        public MapView MapView;
        public MapViewModel()
        {
            SetupMap();
        }
        internal void DrawPath(Ant ant, bool bestPath=false)
        {
            List<MapPoint> mpts = new List<MapPoint>();
            foreach (var tuple in ant.NodesSelected)
            {
                mpts.Add(tuple.Item2.Position);

            }
            GraphicsOverlay overlay = MapView.GraphicsOverlays["mainOverlay"];
            var graphic = MapUtils.GenerateGraphicLine(mpts, bestPath);
            if (bestPath)
                graphic.ZIndex = 1;
            overlay.Graphics.Add(graphic);
            

        }
        internal void DrawSubsetAnts(CostPhermoneData[][] costPhermoneMatrix)
        {
            for (int i = 0; i < costPhermoneMatrix.Length; ++i)
            {
                var level = costPhermoneMatrix[i];
                List<MapPoint> mapPoints = new List<MapPoint>();
                for (int j = 0; j < costPhermoneMatrix[i].Length; ++j)
                {
                    //var level = ant.LevelsTraversed[j];
                    //  level.
                }
                //Polyline polyline=new Polyline(new List<MapPoint> { ant.no})
            }
        }
        internal void ClearGraphic(PointType ptType)
        {
            if (MapView is not null)
            {
                var graphics = MapView
                    .GraphicsOverlays["mainOverlay"]
                    .Graphics
                    .Where(x => x.Attributes.ContainsKey("ID")
                                && x.Attributes["ID"].ToString() == ptType.ToString()).ToList();
                if (graphics is not null)
                {

                    for (int i = 0; i < graphics.Count; ++i)
                    {

                        Graphic graphic = graphics[i];
                        MapView.GraphicsOverlays["mainOverlay"].Graphics.Remove(graphic);


                    }
                }
            }
        }
        public void ClearAll()
        {
            if (MapView is not null)
            {
                MapView.GraphicsOverlays["mainOverlay"].Graphics.Clear();
            }
        }
#nullable enable
        public event PropertyChangedEventHandler? PropertyChanged;
#nullable disable
        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
#nullable enable
        private Map? _map;
        public Map? Map
#nullable disable
        {
            get { return _map; }
            set
            {
                _map = value;
                OnPropertyChanged();
            }
        }

        private void SetupMap()
        {

            // Create a new map with a 'ArcGISImagery' basemap.
            Map = new Map(BasemapStyle.ArcGISImagery);


        }

        internal void AddPoint(MapPoint mpt, PointType ptType)
        {

            if (ptType != PointType.Unknown)
            {
                GraphicsOverlay overlay = MapView.GraphicsOverlays["mainOverlay"];
                overlay.Graphics.Add(MapUtils.GenerateGraphic(mpt, ptType));
            }
        }


    }

}
