using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.UI;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using SBSE_Project_2.Enums;
using SBSE_Project_2.Models;
using SBSE_Project_2.Models.Algorithm;
using SBSE_Project_2.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Popups;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SBSE_Project_2
{
    /// <summary>
    /// An empty window that. can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        //string bilPath = "C:\\Users\\Salman\\Desktop\\DEM-90m\\dem90m.bil";
        string bilPath = "DEM90m\\dem90m.bil";

        BilFileReader reader;
        ACO antColonyOptimizationAlgo = new ACO();
        PointType pointType = PointType.Unknown;
        private DispatcherQueue dispatcherQueue;
        public MainWindow()
        {
            this.InitializeComponent();
            dispatcherQueue= DispatcherQueue.GetForCurrentThread();
            reader = new BilFileReader(AppDomain.CurrentDomain.BaseDirectory+"\\"+bilPath);
            antColonyOptimizationAlgo.SetBilFileReader(reader);
            InitMapView();
        }
        private void InitMapView()
        {
            MapViewModel mapVM = (MapViewModel)MainMapView.DataContext;
            mapVM.MapView = MainMapView;

            GraphicsOverlay graphicsOverlay = new GraphicsOverlay();
            graphicsOverlay.Id = "mainOverlay";
            MainMapView.GraphicsOverlays.Add(graphicsOverlay);


            MapPoint mapCenterPoint = new MapPoint(73.0, 33.0, SpatialReferences.Wgs84);
            MainMapView.SetViewpoint(new Viewpoint(mapCenterPoint, 100000));


        }
        private void MainMapView_PointerMoved(object sender, PointerRoutedEventArgs e)
        {

            MapPoint mpt = MainMapView.ScreenToLocation(e.GetCurrentPoint(MainMapView).Position);
            if (mpt is not null)
            {
                mpt = (MapPoint)GeometryEngine.Project(mpt, SpatialReferences.Wgs84);
                double lat = Math.Round(mpt.Y, 5); double lon = Math.Round(mpt.X, 5);
                double alt = reader.GetAltitude(lat, lon);
                LatLngTextblock.Text = string.Format("  Latitude {0}, Longitude {1}, Altitude {2} m", lat, lon, alt);
            }
        }

        private void MainMapView_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            MapViewModel mapVM = (MapViewModel)MainMapView.DataContext;
            if (mapVM is not null)
            {
                MapPoint mpt = MainMapView.ScreenToLocation(e.GetCurrentPoint(MainMapView).Position);
                mpt = (MapPoint)GeometryEngine.Project(mpt, SpatialReferences.Wgs84);
                mapVM.AddPoint(mpt, pointType);
                
                antColonyOptimizationAlgo.UpdateSrcDest(new GraphNode(mpt, reader.GetAltitude(mpt.Y, mpt.X)), pointType);
                pointType = PointType.Unknown;
            }
        }

        private void AddSource_Click(object sender, RoutedEventArgs e)
        {
            MapViewModel mapVM = (MapViewModel)MainMapView.DataContext;
            if (mapVM is not null)
            {
                pointType = PointType.SourcePoint;
                mapVM.ClearGraphic(pointType);
                Utils.DisplayMessage("Click anywhere on map to add source!", this.Content.XamlRoot);
            }

        }
        private void AddDestination_Click(object sender, RoutedEventArgs e)
        {
            MapViewModel mapVM = (MapViewModel)MainMapView.DataContext;
            if (mapVM is not null)
            {
                pointType = PointType.DestinationPoint;
                mapVM.ClearGraphic(pointType);
                Utils.DisplayMessage("Click anywhere on map to add destination!", this.Content.XamlRoot);
            }
        }

        private void GenerateGraph_Click(object sender, RoutedEventArgs e)
        {
            MapViewModel mapVM = (MapViewModel)MainMapView.DataContext;
            if (mapVM is not null)
            {

                pointType = PointType.GraphPoint;
                mapVM.ClearGraphic(pointType);

                try
                {
                    var acoGraph = antColonyOptimizationAlgo.GenerateGraph();
                    foreach (var level in acoGraph.CostPhermoneData)
                    {
                        foreach (var node in level)
                        {
                            mapVM.AddPoint(node.Location, PointType.GraphPoint);
                        }
                    }
                    //Utils.DisplayMessage(.ToString(), this.Content.XamlRoot);
                }
                catch (Exception ex)
                {
                    Utils.DisplayMessage(ex.Message.ToString(), this.Content.XamlRoot);
                }
                finally
                {
                    pointType = PointType.Unknown;
                }
            }
        }


        private void ClearAll_Click(object sender, RoutedEventArgs e)
        {
            MapViewModel mapVM = (MapViewModel)MainMapView.DataContext;
            if (mapVM is not null)
            {
                mapVM.ClearAll();
                antColonyOptimizationAlgo.ClearAll();
            }
        }

        private void StartACO_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                MapViewModel mapVM = (MapViewModel)MainMapView.DataContext;
                if (mapVM is not null)
                    antColonyOptimizationAlgo.StartACO(mapVM, FitnessTextblock, dispatcherQueue);
                
                //MapViewModel mapVM = (MapViewModel)MainMapView.DataContext;
                //if (mapVM is not null)
                //{
                //    mapVM.DrawSubsetAnts(antColonyOptimizationAlgo.ACOGraph.CostPhermoneData);
                //}
            }
            catch (Exception ex)
            {
                Utils.DisplayMessage(ex.Message.ToString(), this.Content.XamlRoot);
            }
        }
    }
}
