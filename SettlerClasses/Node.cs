using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using System.IO;
using System.Xml;
using System;
using System.Text;
using System.Diagnostics;




[Serializable]
public class Node
{
    [XmlElement("NodePosition")]
    public Point nodePosition;

    [XmlAttribute("F")]
    public int F;

    [XmlAttribute("G")]
    public int G;

    [XmlAttribute("H")]
    public int H;

    [XmlIgnore]
    public Node parentNode;
    //public Point parentNodePosition;
}
