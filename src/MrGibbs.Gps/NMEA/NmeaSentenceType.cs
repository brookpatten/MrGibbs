using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MrGibbs.Gps.NMEA
{
    public class NmeaSentenceType
    {
        private string _name;
        private string _code;
        private bool _ignore;

        private List<NmeaSentenceTypePart> _partsByPosition;
        private Dictionary<string, NmeaSentenceTypePart> _partsByName;

        public NmeaSentenceType(string name, string code, bool ignore)
        {
            _partsByPosition = new List<NmeaSentenceTypePart>();
            _partsByName = new Dictionary<string, NmeaSentenceTypePart>();
            _name = name;
            _code = code;
            _ignore = ignore;
        }
        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                _name = value;
            }
        }
        public string Code
        {
            get
            {
                return _code;
            }
            set
            {
                _code = value;
            }
        }
        public bool Ignore
        {
            get
            {
                return _ignore;
            }
            set
            {
                _ignore = value;
            }
        }
        public void AddPart(NmeaSentenceTypePart part)
        {
            while (_partsByPosition.Count - 1 < part.Position)
            {
                _partsByPosition.Add(null);
            }
            _partsByPosition[part.Position] = part;
            _partsByName[part.Name] = part;
        }
        public int PartCount
        {
            get
            {
                return _partsByPosition.Count;
            }
        }
        public NmeaSentenceTypePart FindPartByName(string name)
        {
            return _partsByName[name];
        }
        public NmeaSentenceTypePart FindPartByPosition(int position)
        {
            return _partsByPosition[position];
        }
    }
}
