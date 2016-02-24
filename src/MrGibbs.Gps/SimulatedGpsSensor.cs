using System;
using System.Collections.Generic;
using System.IO;

using MrGibbs.Contracts.Infrastructure;
using MrGibbs.Models;

namespace MrGibbs.Gps
{
    /// <summary>
    /// simulated gps sensor that just outputs data from a file
    /// used for bench testing
    /// </summary>
    public class SimulatedGpsSensor:GpsSensor
    {
        private string _path = "example.nmea";
        StreamReader _reader;
        private string _currentSentence;
        
        public SimulatedGpsSensor(ILogger logger, GpsPlugin plugin):base(logger,plugin,null,0)
        {
			_path = Configuration.ConfigurationHelper.FindNewestFileWithExtension ("nmea");
        }

        /// <inheritdoc />
        public override void Start()
        {
            _buffer = new Queue<string>();
            _reader = new StreamReader(_path);
        }

        /// <inheritdoc />
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

        /// <inheritdoc />
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
