using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace PovertySail.Gps.NMEA
{
    public class NmeaDictionary
    {
        string _dictionaryPath;
        Dictionary<string, NmeaSentenceType> _dictionaryByName;
        Dictionary<string, NmeaSentenceType> _dictionaryByCode;
        public NmeaDictionary(Stream s)
        {
            _dictionaryByName = new Dictionary<string, NmeaSentenceType>();
            _dictionaryByCode = new Dictionary<string, NmeaSentenceType>();
            _dictionaryPath = "Stream";
            XmlDocument doc = new XmlDocument();
            doc.Load(s);
            XmlElement sentences = (XmlElement)doc.ChildNodes[0];
            foreach (XmlElement sentence in sentences.ChildNodes)
            {
                string name = sentence.Attributes["name"].Value;
                string code = sentence.Attributes["code"].Value;
                bool ignore = false;
                if (sentence.Attributes["ignore"] != null)
                {
                    ignore = bool.Parse(sentence.Attributes["ignore"].Value);
                }
                NmeaSentenceType t = new NmeaSentenceType(name, code, ignore);
                XmlElement parts = (XmlElement)sentence.ChildNodes[0];
                foreach (XmlElement part in parts.ChildNodes)
                {
                    int position = int.Parse(part.Attributes["position"].Value);
                    string partName = part.Attributes["name"].Value;
                    NmeaSentenceTypePart p = new NmeaSentenceTypePart(position, partName, t);
                    t.AddPart(p);
                }
                _dictionaryByName[t.Name] = t;
                _dictionaryByCode[t.Code] = t;
            }
        }
        public NmeaDictionary(string path)
        {
            _dictionaryByName = new Dictionary<string, NmeaSentenceType>();
            _dictionaryByCode = new Dictionary<string, NmeaSentenceType>();
            _dictionaryPath = path;
            XmlDocument doc = new XmlDocument();
            doc.Load(path);
            XmlElement sentences = (XmlElement)doc.ChildNodes[0];
            foreach (XmlElement sentence in sentences.ChildNodes)
            {
                string name = sentence.Attributes["name"].Value;
                string code = sentence.Attributes["code"].Value;
                bool ignore = false;
                if (sentence.Attributes["ignore"] != null)
                {
                    ignore = bool.Parse(sentence.Attributes["ignore"].Value);
                }
                NmeaSentenceType t = new NmeaSentenceType(name, code, ignore);
                XmlElement parts = (XmlElement)sentence.ChildNodes[0];
                foreach (XmlElement part in parts.ChildNodes)
                {
                    int position = int.Parse(part.Attributes["position"].Value);
                    string partName = part.Attributes["name"].Value;
                    NmeaSentenceTypePart p = new NmeaSentenceTypePart(position, partName, t);
                    t.AddPart(p);
                }
                _dictionaryByName[t.Name] = t;
                _dictionaryByCode[t.Code] = t;
            }
        }
        public NmeaSentenceType FindByName(string name)
        {
            return _dictionaryByName[name];
        }
        public NmeaSentenceType FindByCode(string code)
        {
            return _dictionaryByCode[code];
        }
    }
}
