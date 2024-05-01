using Esri.ArcGISRuntime.Geometry;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;
using Windows.Graphics.Display;

namespace SBSE_Project_2.Models
{
    internal class Utils
    {
        private const double EarthRadiusKilometers = 6371.01;
        internal static double CovnertValueToRange(
            double OldMax, 
            double OldMin,
            double NewMax,
            double NewMin,
            double OldValue)
        {
            double OldRange = (OldMax - OldMin);
            double NewRange = (NewMax - NewMin);
            double NewValue = (((OldValue - OldMin) * NewRange) / OldRange) + NewMin;
            return NewValue;
        }
        internal static MapPoint FindPointAtDistanceFrom(MapPoint startPoint, double trueNorthBearingDegrees, double distanceKilometers)
        {
            // Convert true north bearing from degrees to radians
            double initialBearingRadians = ToRad(360.0 - trueNorthBearingDegrees);

            var distRatio = distanceKilometers / EarthRadiusKilometers;
            var distRatioSine = Math.Sin(distRatio);
            var distRatioCosine = Math.Cos(distRatio);

            var startLatRad = ToRad(startPoint.Y);
            var startLonRad = ToRad(startPoint.X);

            var endLatRad = Math.Asin(Math.Sin(startLatRad) * distRatioCosine +
                                      Math.Cos(startLatRad) * distRatioSine * Math.Cos(initialBearingRadians));

            var endLonRad = startLonRad + Math.Atan2(Math.Sin(initialBearingRadians) * distRatioSine * Math.Cos(startLatRad),
                                                     distRatioCosine - Math.Sin(startLatRad) * Math.Sin(endLatRad));

            var endLatitude = ToDegrees(endLatRad);
            var endLongitude = ToDegrees(endLonRad);

            return new MapPoint(endLongitude, endLatitude, SpatialReferences.Wgs84);

        }
        internal static MapPoint[] FindIntermediatePoints(MapPoint startCoord, MapPoint endCoord, double interval, double dist)
        {
            // Calculate the initial distance
            double initialDistance = dist;

            // Calculate the number of segments
            int numSegments = (int)(initialDistance / interval);

            // Calculate the step size for interpolation
            double step = 1.0 / (numSegments + 1);

            var intermediatePoints = new MapPoint[numSegments];

            for (int i = 1; i <= numSegments; i++)
            {
                // Interpolate the points
                double lat = startCoord.Y + (endCoord.Y - startCoord.Y) * i * step;
                double lon = startCoord.X + (endCoord.X - startCoord.X) * i * step;
                intermediatePoints[i - 1] = new MapPoint(lon, lat);
            }

            return intermediatePoints;
        }
        public static Tuple<int, int> FindAltDistanceMeters(BilFileReader bilReader, MapPoint initPoint, MapPoint endPoint)
        {
            var distance=GeometryEngine.DistanceGeodetic(initPoint, endPoint, LinearUnits.Meters, AngularUnits.Degrees, GeodeticCurveType.Geodesic).Distance;
            //var allPoints=FindIntermediatePoints(initPoint, endPoint, 500, distance);
            double maxDemAlt = -9999;
            //foreach(var pt in allPoints)
            //{
            //    double alt=bilReader.GetAltitude(pt.Y, pt.X);
            //    if (alt > maxDemAlt)
            //        maxDemAlt = alt;
            //}
            double initAlt=bilReader.GetAltitude(initPoint.Y, initPoint.X);
            double endAlt=bilReader.GetAltitude(endPoint.Y, endPoint.X);
            if (initAlt > maxDemAlt)
                maxDemAlt = initAlt;
            if (endAlt > maxDemAlt)
                maxDemAlt = endAlt;

            return new Tuple<int, int>(Convert.ToInt32(maxDemAlt), Convert.ToInt32(distance));
        }

        public static double CalculateBearingDeg(MapPoint pt1, MapPoint pt2)
        {
            double lat1 = pt1.Y;
            double lon1 = pt1.X;

            double lat2 = pt2.Y;
            double lon2 = pt2.X;
            var dLon = ToRad(lon2 - lon1);
            var dPhi = Math.Log(Math.Tan(ToRad(lat2) / 2 + Math.PI / 4) / Math.Tan(ToRad(lat1) / 2 + Math.PI / 4));

            if (Math.Abs(dLon) > Math.PI)
                dLon = dLon > 0 ? -(2 * Math.PI - dLon) : (2 * Math.PI + dLon);
            double finalBearing = ToBearing(Math.Atan2(dLon, dPhi));
            return finalBearing;
        }

        private static double ToRad(double degrees)
        {
            return degrees * (Math.PI / 180);
        }

        private static double ToDegrees(double radians)
        {
            return radians * 180 / Math.PI;
        }

        private static double ToBearing(double radians)
        {
            // Convert radians to degrees (as bearing: 0...360)
            return (ToDegrees(radians) + 360) % 360;
        }
        internal async static void DisplayMessage(string message, XamlRoot xamlRoot)
        {
            ContentDialog dialogMsg = new ContentDialog()
            {

                Title = "Information",
                Content = message,
                CloseButtonText = "OK"
            };
            dialogMsg.XamlRoot = xamlRoot;
            await dialogMsg.ShowAsync();
            
        }
    }
}
