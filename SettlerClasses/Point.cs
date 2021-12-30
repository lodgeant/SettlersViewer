using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using System.IO;
using System.Xml;
using System;
using System.Text;




public class Point
{
    [XmlAttribute("X")]
    public int X;

    [XmlAttribute("Y")]
    public int Y;

    public Point()
    {
    }

    public Point(int x, int y)
    {
        X = x;
        Y = y;
    }

    public Point(string pointString)
    {
        //X = int.Parse(pointString.Replace("{", "").Replace("}","").Split(',')[0]);
        //Y = int.Parse(pointString.Replace("{", "").Replace("}", "").Split(',')[1]);
        string[] ps = pointString.Replace("{", "").Replace("}","").Split(',');
		X = int.Parse(ps[0]);
		Y = int.Parse(ps[1]);	
    }


    public override string ToString()
    {
        return "{" + X + "," + Y + "}";
    }




    // CORE -> REAL
    public static Point ConvertCorePointToRealPoint(Point corePoint, Point topLeft)
    {
        Point realPoint = new Point();
        realPoint.X = corePoint.X + topLeft.X;
        realPoint.Y = topLeft.Y - corePoint.Y;
        return realPoint;
    }

    // CORE -> ACTUAL
    public static Point ConvertCorePointToActualPoint(Point corePoint, Point topLeft, int mapFactor)
    {        
        Point realPoint = Point.ConvertCorePointToRealPoint(corePoint, topLeft);
        Point actualPoint = Point.ConvertRealPointToActualPoint(realPoint, mapFactor);
        return actualPoint;
    }

    // REAL -> CORE
    public static Point ConvertRealPointToCorePoint(Point realPoint, Point topLeft)
    {
        Point corePoint = new Point();
        corePoint.X = realPoint.X - topLeft.X;
        corePoint.Y = topLeft.Y - realPoint.Y;
        return corePoint;
    }

    // REAL -> ACTUAL
    public static Point ConvertRealPointToActualPoint(Point realPoint, int mapFactor)
    {
        Point actualPoint = new Point();
        actualPoint.X = realPoint.X / mapFactor;
        actualPoint.Y = realPoint.Y / mapFactor;
        return actualPoint;
    }

    // ACTUAL -> REAL
    public static Point ConvertActualPointToRealPoint(Point actualPoint, int mapFactor)
    {
        Point realPoint = new Point();
        realPoint.X = actualPoint.X * mapFactor;
        realPoint.Y = actualPoint.Y * mapFactor;
        return realPoint;
    }

    // ACTUAL -> CORE
    public static Point ConvertActualPointToCorePoint(Point actualPoint, Point topLeft, int mapFactor)
    {
        Point realPoint = Point.ConvertActualPointToRealPoint(actualPoint, mapFactor);
        Point corePoint = Point.ConvertRealPointToCorePoint(realPoint, topLeft);
        return corePoint;
    }


    // ** GET POINT DETAILS - MOVE CONVERSIONS TO CLASSES **
    //Point actualStartPoint = new Point(-21, 11);
    //Point actualTargetPoint = new Point(-21, 7);
    //Point realStartPoint = new Point(actualStartPoint.X * globalMap.MapFactor, actualStartPoint.Y * globalMap.MapFactor);
    //Point realTargetPoint = new Point(actualTargetPoint.X * globalMap.MapFactor, actualTargetPoint.Y * globalMap.MapFactor);
    //Point coreStartPoint = Point.ConvertRealPointToCorePoint(realStartPoint, topLeft);
    //Point coreTargetPoint = Point.ConvertRealPointToCorePoint(realTargetPoint, topLeft);





}
