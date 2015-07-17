using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PovertySail.Gps.NMEA
{
    public class NmeaSentenceTypePart
    {
        private int _position;
        private string _name;
        private NmeaSentenceType _sentenceType;
        public NmeaSentenceTypePart(int position, string name, NmeaSentenceType sentenceType)
        {
            _position = position;
            _name = name;
            _sentenceType = sentenceType;
        }
        public NmeaSentenceType SentenceType
        {
            get
            {
                return _sentenceType;
            }
            set
            {
                _sentenceType = value;
            }
        }
        public int Position
        {
            get
            {
                return _position;
            }
            set
            {
                _position = value;
            }
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
    }
}
