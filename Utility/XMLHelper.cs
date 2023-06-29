using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using static WY_App.Utility.Parameters;

namespace WY_App.Utility
{
    public static class XMLHelper
    {     
        public static void serialize<T>(T s, string path)
        {
            FileStream fileStream = new FileStream(path, FileMode.Create);
            XmlSerializer formatter = new XmlSerializer(typeof(T));
            formatter.Serialize(fileStream, s);
            fileStream.Close();
        }
        public static T BackSerialize<T>(string path)
        {
            FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
            XmlSerializer formatter = new XmlSerializer(typeof(T));
            T s = (T)formatter.Deserialize(fileStream);
            fileStream.Close();
            return s;
        }
    }
}