﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using NetTopologySuite;
using NetTopologySuite.Geometries;
using GeoAPI.Geometries;
using netDxf;
using netDxf.Entities;
using netDxf.Collections;

using ACG.Core.Objects;

namespace ACG.Core.Readers
{
    /// <summary>
    /// Data reader for Autodesk DXF files.
    /// </summary>
    public class AcgDxfReader : AcgReader
    {
        /// <inheritdoc/>
        public override List<IAcgObject> Read(string filePath)
        {
            List<IAcgObject> objectList = new List<IAcgObject>();

            DxfDocument dxf = DxfDocument.Load(filePath);

            if (dxf.LwPolylines.Count > 0)
            {
                foreach (LwPolyline polyline in dxf.LwPolylines)
                {
                    XDataDictionary xdatadic = polyline.XData;
                    string appID = "";

                    foreach (XData xdata in xdatadic.Values)
                    {
                        appID = xdata.ApplicationRegistry.Name;
                    }

                    List<Coordinate> points = new List<Coordinate>();
                    foreach (LwPolylineVertex vertex in polyline.Vertexes)
                    {
                        Vector2 location = vertex.Location;
                        points.Add(new Coordinate(location.X, location.Y));
                    }
                    points.Add(points[0]);

                    LinearRing linearing = new LinearRing(points.ToArray());
                    Polygon polygon = new Polygon(linearing);

                    switch (appID)
                    {
                        case "ACG_BUILDING":
                            AcgBuilding building = new AcgBuilding();
                            foreach (XData xdata in xdatadic.Values)
                            {
                                List<XDataRecord> rekordlist = xdata.XDataRecord;
                                if (rekordlist.Count == 3) continue;
                                for (int i = 1; i < rekordlist.Count; i++)
                                {
                                    if (i == 12) building.CountryName = (string)rekordlist.ElementAt(i).Value;
                                }
                            }
                            building.Geometry = polygon;
                            objectList.Add(building);
                            break;

                        case "ACG_PARCEL":
                            AcgParcel parcel = new AcgParcel();
                            foreach (XData xdata in xdatadic.Values)
                            {
                                List<XDataRecord> rekordlist = xdata.XDataRecord;
                                foreach (XDataRecord rekord in rekordlist)
                                {
                                    parcel.Metadata += rekord.Value;
                                }
                            }
                            parcel.Geometry = polygon;
                            objectList.Add(parcel);
                            break;

                        default:
                            break;
                    }
                }
            }
            return objectList;
        }
    }
}
