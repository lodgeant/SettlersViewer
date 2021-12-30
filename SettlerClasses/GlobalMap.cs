using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using System.IO;
using System.Xml;
using System;
using System.Text;
using System.Drawing;



[Serializable]
public class GlobalMap
{
    [XmlAttribute("MapScreenWidth")]
    public int MapScreenWidth;

    [XmlAttribute("MapScreenHeight")]
    public int MapScreenHeight;

    [XmlAttribute("MapScreenTopLeftX")]
    public int MapScreenTopLeftX;

    [XmlAttribute("MapScreenTopLeftY")]
    public int MapScreenTopLeftY;

    [XmlAttribute("MapFactor")]
    public int MapFactor;

    [XmlAttribute("MapWidth")]
    public int MapWidth;

    [XmlAttribute("MapHeight")]
    public int MapHeight;

    [XmlAttribute("MapTopLeftX")]
    public int MapTopLeftX;

    [XmlAttribute("MapTopLeftY")]
    public int MapTopLeftY;

    //[XmlElement("MapObject")]
    //public List<MapObject> mapObjectList = new List<MapObject>();

    [XmlAttribute("RouteRequestIndex")]
    public int RouteRequestIndex;

    [XmlAttribute("OrderIndex")]
    public int OrderIndex;

    [XmlElement("RouteRequest_Open")]
    public RouteRequestCollection RouteRequestList_Open = new RouteRequestCollection();

    [XmlElement("RouteRequest_Closed")]
    public RouteRequestCollection RouteRequestList_Closed = new RouteRequestCollection();






    public int GetNextRouteRequestID()
    {
        RouteRequestIndex += 1;
        return RouteRequestIndex;
    }

    public int GetNextOrderID()
    {
        OrderIndex += 1;
        return OrderIndex;
    }


    public string SerializeToString(bool omitDeclaration)
    {
        XmlSerializer xmlSerializer = new XmlSerializer(this.GetType());
        using (StringWriter textWriter = new Utf8StringWriter())
        {
            xmlSerializer.Serialize(textWriter, this);
            if (omitDeclaration)
            {
                return textWriter.ToString().Replace(@"<?xml version=""1.0"" encoding=""utf-8""?>", "");
            }
            else
            {
                return textWriter.ToString();
            }
        }
    }

    public class Utf8StringWriter : StringWriter
    {
        // Use UTF8 encoding but write no BOM to the wire
        public override Encoding Encoding
        {
            get { return new UTF8Encoding(false); } // in real code I'll cache this encoding.
        }
    }

    public GlobalMap DeserialiseFromXMLString(string XMLString)
    {
        // ** IMPROVED METHOD **           
        var serializer = new XmlSerializer(this.GetType());
        using (TextReader reader = new StringReader(XMLString))
        {
            return (GlobalMap)serializer.Deserialize(reader);
        }
    }




    public static void CalculateRoute(Point coreStartPoint, Point coreTargetPoint, int[,] ObjectMap, bool allowDiagonals, int AStarMaxRouteCalcWaitSecs,
        out bool routeFound, out List<Point> routeCorePointList, out List<Node> nodeList_Open, out List<Node> nodeList_Closed)
    {
        // ** Define variables **
        routeFound = false;
        routeCorePointList = new List<Point>();
        nodeList_Open = new List<Node>();
        nodeList_Closed = new List<Node>();
        HashSet<string> openHS = new HashSet<string>();
        HashSet<string> closedHS = new HashSet<string>();
        DateTime overallStartTime = DateTime.Now;
        DateTime overallEndTime;

        // ** Add startPoint to NodeList_Open **
        Node n = new Node();
        n.nodePosition = coreStartPoint;
        nodeList_Open.Add(n);
        openHS.Add(n.nodePosition.ToString());

        // ** Cycle through all nodes and calculate values **   
        int MapWidth = ObjectMap.GetLength(0);
        int MapHeight = ObjectMap.GetLength(1);
        int nodeCount = MapWidth * MapHeight;
        List<string> erroredNodeList = new List<string>();
        for (int a = 0; a < nodeCount; a++)
        {
            // ** If there are no open Nodes left then break **
            if (nodeList_Open.Count == 0)
            {
                break;
            }

            // ** Check if too much time has been taken **            
            TimeSpan timeTakenSoFar = DateTime.Now - overallStartTime;
            if (timeTakenSoFar.TotalSeconds > AStarMaxRouteCalcWaitSecs)
            {
                break;
            }

            #region ** Take the first entry in the list and process - this should be the lowest F value
            Node currentNode = null;
            try
            {
                //if (a == 27472)
                //{
                //    string test2 = "";
                //}
                currentNode = nodeList_Open[0];
                //if (currentNode.nodePosition.X == 2 && currentNode.nodePosition.Y == 2)
                //{
                //    string test2 = "";
                //}

                // ** Mark Node as Closed **
                nodeList_Open.Remove(currentNode);
                openHS.Remove(currentNode.nodePosition.ToString());
                nodeList_Closed.Add(currentNode);
                closedHS.Add(currentNode.nodePosition.ToString());

                #region ** Check if current Node is the target **
                if (currentNode.nodePosition.X == coreTargetPoint.X && currentNode.nodePosition.Y == coreTargetPoint.Y)
                {
                    // ** Work out route of Points **
                    List<Point> prelim_routePointList = new List<Point>();
                    prelim_routePointList.Add(currentNode.nodePosition);
                    Node currentRouteNode = currentNode;
                    for (int b = 0; b < nodeCount; b++)
                    {
                        Node parentNode = currentRouteNode.parentNode;
                        if (parentNode == null)
                        {
                            routeFound = true;
                            break;
                        }
                        prelim_routePointList.Add(parentNode.nodePosition);
                        currentRouteNode = parentNode;
                    }

                    // ** Reverse route list **
                    for (int i = prelim_routePointList.Count - 1; i >= 0; i--)
                    {
                        routeCorePointList.Add(prelim_routePointList[i]);
                    }

                    break;
                }
                #endregion

                #region ** Get all surrounding Positions for Current Node **
                Dictionary<Point, int> surroundingPointDict = new Dictionary<Point, int>();
                Point sp;
                sp = new Point(currentNode.nodePosition.X + 1, currentNode.nodePosition.Y + 0);
                if (sp.X >= 0 && sp.Y >= 0 && sp.X < MapWidth && sp.Y < MapHeight)
                {
                    surroundingPointDict.Add(sp, 10);
                }
                sp = new Point(currentNode.nodePosition.X + 0, currentNode.nodePosition.Y + 1);
                if (sp.X >= 0 && sp.Y >= 0 && sp.X < MapWidth && sp.Y < MapHeight)
                {
                    surroundingPointDict.Add(sp, 10);
                    //surroundingPointDict.Add(sp.ToString(), 10);
                }
                sp = new Point(currentNode.nodePosition.X + -1, currentNode.nodePosition.Y + 0);
                if (sp.X >= 0 && sp.Y >= 0 && sp.X < MapWidth && sp.Y < MapHeight)
                {
                    surroundingPointDict.Add(sp, 10);
                    //surroundingPointDict.Add(sp.ToString(), 10);
                }
                sp = new Point(currentNode.nodePosition.X + 0, currentNode.nodePosition.Y + -1);
                if (sp.X >= 0 && sp.Y >= 0 && sp.X < MapWidth && sp.Y < MapHeight)
                {
                    surroundingPointDict.Add(sp, 10);
                    //surroundingPointDict.Add(sp.ToString(), 10);
                }
                #endregion

                #region ** Check if diagonals are allowed **                       
                if (allowDiagonals)
                {
                    sp = new Point(currentNode.nodePosition.X + 1, currentNode.nodePosition.Y + 1);
                    if (sp.X >= 0 && sp.Y >= 0 && sp.X < MapWidth && sp.Y < MapHeight)
                    {
                        surroundingPointDict.Add(sp, 14);
                    }
                    sp = new Point(currentNode.nodePosition.X + -1, currentNode.nodePosition.Y + 1);
                    if (sp.X >= 0 && sp.Y >= 0 && sp.X < MapWidth && sp.Y < MapHeight)
                    {
                        surroundingPointDict.Add(sp, 14);
                    }
                    sp = new Point(currentNode.nodePosition.X + -1, currentNode.nodePosition.Y + -1);
                    if (sp.X >= 0 && sp.Y >= 0 && sp.X < MapWidth && sp.Y < MapHeight)
                    {
                        surroundingPointDict.Add(sp, 14);
                    }
                    sp = new Point(currentNode.nodePosition.X + 1, currentNode.nodePosition.Y + -1);
                    if (sp.X >= 0 && sp.Y >= 0 && sp.X < MapWidth && sp.Y < MapHeight)
                    {
                        surroundingPointDict.Add(sp, 14);
                    }
                }
                #endregion

                #region ** Cycle through new entries and check if they are INVALID or CLOSED **
                Dictionary<Point, int> validPointDict = new Dictionary<Point, int>();
                foreach (Point p in surroundingPointDict.Keys)
                {
                    int walkValue = ObjectMap[p.X, p.Y];
                    if (walkValue == 0)
                    {
                        //validPointDict.Add(p, surroundingPointDict[p]);
                        if (closedHS.Contains(p.ToString()) == false)
                        {
                            validPointDict.Add(p, surroundingPointDict[p]);
                        }
                    }
                }
                #endregion

                #region ** Create entries for any new Positions **                        
                foreach (Point p in validPointDict.Keys)
                {
                    // ** Check if Node already exists **                            
                    if (openHS.Contains(p.ToString()) == false)
                    {
                        Node newNode = new Node();
                        newNode.nodePosition = p;
                        newNode.G = currentNode.G + surroundingPointDict[newNode.nodePosition];
                        newNode.H = 10 * (Math.Abs(coreTargetPoint.X - newNode.nodePosition.X) + Math.Abs(coreTargetPoint.Y - newNode.nodePosition.Y));
                        newNode.F = newNode.G + newNode.H;
                        newNode.parentNode = currentNode;
                        nodeList_Open.Add(newNode);
                        openHS.Add(newNode.nodePosition.ToString());
                    }
                    else
                    {
                        // NODE ALREADY EXISTS IN OPEN LIST
                        //string ae = "";
                    }
                }
                #endregion

            }
            catch (Exception ex)
            {
                string errorMessage = "";
                if (currentNode == null) errorMessage = a + "| NULL |" + ex.Message;
                else errorMessage = a + "|" + currentNode.nodePosition + "|" + ex.Message;
                erroredNodeList.Add(errorMessage);
            }
            #endregion

            // ** Sort the OPEN Node list ** 
            nodeList_Open.Sort((x, y) => x.F.CompareTo(y.F));

            #region ** DEBUG VARIABLES **
            //List<string> OpenNodeList = new List<string>();
            //List<string> ClosedNodeList = new List<string>();
            //string OpenNodeList_s = "";
            //string ClosedNodeList_s = "";
            //foreach (Node openNode in nodeList_Open)
            //{
            //    string parentPosition = "";
            //    if (openNode.parentNode != null) parentPosition = openNode.parentNode.nodePosition.ToString();
            //    string nodeString = openNode.nodePosition.ToString() + "|" + parentPosition + "|" + openNode.F + "-" + openNode.G + "-" + openNode.H;
            //    OpenNodeList.Add(nodeString);
            //    OpenNodeList_s += nodeString + Environment.NewLine;
            //}
            //foreach (Node closedNode in nodeList_Closed)
            //{
            //    string parentPosition = "";
            //    if (closedNode.parentNode != null) parentPosition = closedNode.parentNode.nodePosition.ToString();
            //    string nodeString = closedNode.nodePosition.ToString() + "|" + parentPosition + "|" + closedNode.F + "-" + closedNode.G + "-" + closedNode.H;
            //    ClosedNodeList.Add(nodeString);
            //    ClosedNodeList_s += nodeString + Environment.NewLine;
            //}
            #endregion

        }
        overallEndTime = DateTime.Now;
        TimeSpan ts = overallEndTime - overallStartTime;
    }


    //public UnityEngine.Texture2D ConvertObjectMapToTexture(int[,] ObjectMap)
    //{
    //    try
    //    {
    //        int mapWidth = ObjectMap.GetLength(0);
    //        int mapHeight = ObjectMap.GetLength(1);
    //        UnityEngine.Texture2D t = new UnityEngine.Texture2D(mapWidth, mapHeight);
    //        for (int x = 0; x < mapWidth; x++)
    //        {
    //            for (int y = 0; y < mapHeight; y++)
    //            {
    //                int WalkValue = ObjectMap[x, y];
    //                if (WalkValue == -1)
    //                {
    //                    Point tPoint = new Point(x, mapHeight - y);
    //                    t.SetPixel(tPoint.X, tPoint.Y, UnityEngine.Color.black);
    //                }
    //            }
    //        }
    //        return t;
    //    }
    //    catch (Exception)
    //    {
    //        return null;
    //    }
    //}


    public static string ConvertObjectMapToCSV(int[,] ObjectMap, string seperator, bool includeAxis)
    {
        // ** GET VARIABLES **
        StringBuilder sb = new StringBuilder();
        int width = ObjectMap.GetLength(0);
        int height = ObjectMap.GetLength(1);

        // ** ADD AXIS HEADER (IF REQUIRED) **
        if (includeAxis)
        {
            sb.Append(seperator);
            for (int x = 0; x < width; x++) sb.Append(x + seperator);
            sb.Append(Environment.NewLine);
        }

        // ** ADD ROWS **
        for (int y = 0; y < height; y++)
        {
            if (includeAxis) sb.Append(y + seperator);
            for (int x = 0; x < width; x++)
            {
                sb.Append(ObjectMap[x, y] + seperator);
            }
            sb.Append(Environment.NewLine);
        }
        return sb.ToString();
    }

    public static Bitmap GenerateBitMapFromObjectMap(int[,] ObjectMap)
    {
        int mapWidth = ObjectMap.GetLength(0);
        int mapHeight = ObjectMap.GetLength(1);
        Bitmap bMap = new Bitmap(mapWidth, mapHeight);
        for (int a = 0; a < mapWidth; a++)
        {
            for (int b = 0; b < mapHeight; b++)
            {
                int walkValue = ObjectMap[a, b];
                if (walkValue == 0)
                {
                    bMap.SetPixel(a, b, Color.White);
                }
                else if (walkValue == -1)
                {
                    bMap.SetPixel(a, b, Color.Black);
                }
            }
        }
        return bMap;
    }

    public static int[,] GenerateObjectMapFromBitMap(Bitmap bMap)
    {
        int mapWidth = bMap.Width;
        int mapHeight = bMap.Height;
        int[,] ObjectMap = new int[mapWidth, mapHeight];
        for (int a = 0; a < mapWidth; a++)
        {
            for (int b = 0; b < mapHeight; b++)
            {
                Color pixelColor = bMap.GetPixel(a, b);
                if (pixelColor.A == 255 && pixelColor.B == 255 && pixelColor.G == 255 && pixelColor.R == 255)
                {
                    ObjectMap[a, b] = 0;
                }
                else if (pixelColor.A == 255 && pixelColor.B == 0 && pixelColor.G == 0 && pixelColor.R == 0)
                {
                    ObjectMap[a, b] = -1;
                }
            }
        }
        return ObjectMap;
    }

}

