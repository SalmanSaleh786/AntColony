using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI;
using SBSE_Project_2.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SBSE_Project_2.Models
{
    internal class MapUtils
    {
       internal static readonly SimpleMarkerSymbol sourceSymbol = new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Circle, System.Drawing.Color.Blue, 10);
       internal static readonly SimpleMarkerSymbol destSymbol = new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Circle, System.Drawing.Color.Red, 10);
       internal static readonly SimpleMarkerSymbol nodeSymbol = new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Triangle, System.Drawing.Color.Yellow, 6);
        internal static readonly SimpleLineSymbol lineSymbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, System.Drawing.Color.White, 2);
        internal static readonly SimpleLineSymbol lineSymbolBlue = new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, System.Drawing.Color.Blue, 1);
        internal static Graphic GenerateGraphic(MapPoint pt, PointType ptType)
        {
            
            SimpleMarkerSymbol symbol = ptType == PointType.SourcePoint ? sourceSymbol : 
                                    ptType == PointType.DestinationPoint ? destSymbol : nodeSymbol;
            Graphic graphic = new Graphic(pt, symbol);
            graphic.Attributes["ID"] = ptType.ToString();
            return graphic;
        }
        internal static Graphic GenerateGraphicLine(List<MapPoint> pts, bool bestPath=false)
        {
            var polyline = new Polyline(pts);

            Graphic graphic = new Graphic(polyline, bestPath==true ? lineSymbol : lineSymbolBlue);
            graphic.Attributes["ID"] = PointType.Path.ToString();
            return graphic;
        }
    }
}
