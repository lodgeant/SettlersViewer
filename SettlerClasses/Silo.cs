using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using System.IO;
using System.Xml;
using System;
using System.Text;




[Serializable]
public class Silo
{
    [XmlAttribute("StorageAreaRef")]
    public string storageAreaRef;                                           // The parent storage area where the silo is located.

    [XmlAttribute("SiloIndex")]
    public int siloIndex;                                                   // The index of the Silo so that the DropZoneLocation can be inferred.

    [XmlAttribute("CommodityType")]
    public string commodityType;                                            // The type of Commodity that can be stored by the Silo.

    [XmlAttribute("MaxCount")]
    public int maxCount;                                                    // The maximum number of objects that can be stored in the Silo.

    public List<string> commodityRefList = new List<string>();              // List of all the objects currently be stored in the Silo (this + below should not be more than maxCount)
    public List<string> reservedCommodityRefList = new List<string>();      // List of all the objects currently due to be stored in the Silo (this + above should not be more than maxCount)
    //public Vector3 storageAreaPosition;                                     // The real world position of the parent Storage Area. 

}


[Serializable]
public class SiloCollection
{
    [XmlElement("Silo")]
    public List<Silo> SiloList = new List<Silo>();

}