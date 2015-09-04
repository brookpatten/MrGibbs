using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using MrGibbs.Contracts;
using MrGibbs.Contracts.Infrastructure;
using MrGibbs.Models;

namespace MrGibbs.Gps
{
    public class SimulatedGpsSensor:GpsSensor
    {
        private string _path = "example.nmea";
        StreamReader _reader;
        private string _currentSentence;
        
        public SimulatedGpsSensor(ILogger logger, GpsPlugin plugin):base(logger,plugin,null,0)
        {

        }

        public override void Start()
        {
            _buffer = new Queue<string>();
            _reader = new StreamReader(_path);
        }

        public override void Update(State state)
        {
            string sentenceType = string.Empty;
            //read from the file and add to the buffer until we find a repeated sentence
            do
            {
                if (string.IsNullOrEmpty(_currentSentence))
                {
                    _currentSentence = _reader.ReadLine();
                    sentenceType = _currentSentence.Substring(0, _currentSentence.IndexOf(","));
                }
                
                _buffer.Enqueue(_currentSentence);

                _currentSentence = _reader.ReadLine();
                sentenceType = _currentSentence.Substring(0, _currentSentence.IndexOf(","));
            }
            while (sentenceType!= "$GPGGA" && !_reader.EndOfStream);
            
            //let the base class parse it as if it came from the port
            base.Update(state);
        }

        public override void Dispose()
        {
            if(_reader!= null)
            {
                _reader.Dispose();
            }
            base.Dispose();
        }
    }
}
