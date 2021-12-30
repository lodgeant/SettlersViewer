using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using System.IO;
using System.Xml;
using System;
using System.Text;




[Serializable]
public class RouteRequestPackage
{
    [XmlAttribute("PackageRef")]
    public int packageRef;

    [XmlElement("RouteCorePointList")]
    public List<Point> routeCorePointList;

    [XmlElement("NodeList_Open")]
    public List<Node> nodeList_Open;

    [XmlElement("NodeList_Closed")]
    public List<Node> nodeList_Closed;



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

    public RouteRequestPackage DeserialiseFromXMLString(string XMLString)
    {
        // ** IMPROVED METHOD **           
        var serializer = new XmlSerializer(this.GetType());
        using (TextReader reader = new StringReader(XMLString))
        {
            return (RouteRequestPackage)serializer.Deserialize(reader);
        }
    }


}