using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using System.IO;
using System.Xml;
using System;
using System.Text;



[Serializable]
public class RouteRequestCollection
{
    [XmlElement("RouteRequest")]
    public List<RouteRequest> RouteRequestList = new List<RouteRequest>();

}


[Serializable]
public class RouteRequest
{
    [XmlAttribute("RequestID")]
    public int requestID;

    [XmlAttribute("CreatedTS")]
    public DateTime CreatedTS;

    [XmlAttribute("RequestingObject")]
    public string requestingObject;

    [XmlAttribute("RequestStatus")]
    public RequestStatus requestStatus;

    [XmlAttribute("RequestPriority")]
    public RequestPriority requestPriority;

    [XmlAttribute("RouteFound")]
    public bool routeFound;

    [XmlAttribute("TimeTakenRequest")]
    public double timeTaken_Request;

    [XmlAttribute("TimeTakenCalculation")]
    public double timeTaken_Calculation;

    [XmlAttribute("ActualStartPoint")]
    public string ActualStartPoint;

    [XmlAttribute("ActualTargetPoint")]
    public string ActualTargetPoint;

    [XmlAttribute("CoreStartPoint")]
    public string CoreStartPoint;

    [XmlAttribute("CoreTargetPoint")]
    public string CoreTargetPoint;

    [XmlAttribute("RealStartPoint")]
    public string RealStartPoint;

    [XmlAttribute("RealTargetPoint")]
    public string RealTargetPoint;

    [XmlAttribute("UseDiagonals")]
    public bool useDiagonals;

    [XmlAttribute("NodeCountOpen")]
    public int nodeCountOpen;

    [XmlAttribute("NodeCountClosed")]
    public int nodeCountClosed;

    [XmlAttribute("IgnoredObjectRefs")]
    public string ignoredObjectRefs;



    //public List<Vector3> routeActualVectorList;
    public byte[] ObjectMapByte;
    public byte[] MapScreenShot;
    public string MapScreenShotFilename;
    public string PackageFilename;
    public string PerformanceData;
    private int[,] ObjectMap;




    public enum RequestStatus
    {
        OPEN,
        IN_PROGRESS,
        COMPLETED,
        CLOSED
    }

    public enum RequestPriority
    {
        HIGH,
        LOW
    }



    public void SetObjectMap(int[,] newObjectMap)
    {
        this.ObjectMap = newObjectMap;
    }

    public int[,] GetObjectMap()
    {
        return ObjectMap;
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

    public RouteRequest DeserialiseFromXMLString(string XMLString)
    {
        // ** IMPROVED METHOD **           
        var serializer = new XmlSerializer(this.GetType());
        using (TextReader reader = new StringReader(XMLString))
        {
            return (RouteRequest)serializer.Deserialize(reader);
        }
    }



}







