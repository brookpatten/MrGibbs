using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PovertySail.Contracts.Infrastructure;

using log4net;
using Newtonsoft.Json;


namespace PovertySail.Infrastructure
{
    public class Log4NetLogger : ILogger
    {
        private ILog _log;

        public Log4NetLogger(ILog log)
        {
            _log = log;
        }

        public void Debug(string message)
        {
            _log.Debug(message);
        }

        public void Debug(string message, Exception exception)
        {
            _log.Debug(message, exception);
        }

        public void Info(string message)
        {
            _log.Info(message);
        }

        public void Info(string message, Exception exception)
        {
            _log.Info(message, exception);
        }

        public void Warn(string message)
        {
            _log.Warn(message);
        }

        public void Warn(string message, Exception exception)
        {
            _log.Warn(message, exception);
        }

        public void Error(string message)
        {
            _log.Error(message);
        }

        public void Error(string message, Exception exception)
        {
            _log.Error(message, exception);
        }

        public void Fatal(string message)
        {
            _log.Fatal(message);
        }

        public void Fatal(string message, Exception exception)
        {
            _log.Fatal(message, exception);

        }

        private string DumpObjectToJson<T>(T obj)
        {
            string json = JsonConvert.SerializeObject(obj);
            return json;
        }

        private string CreateObjectLogMessage<T>(string message, T obj)
        {
            return string.Format("{0} {1}:{2}", message, typeof(T).Name, DumpObjectToJson(obj));
        }

        public void Debug<T>(string message, T o) where T : class
        {
            _log.Debug(CreateObjectLogMessage(message, o));
        }

        public void Info<T>(string message, T o) where T : class
        {
            _log.Info(CreateObjectLogMessage(message, o));
        }

        public void Warn<T>(string message, T o) where T : class
        {
            _log.Warn(CreateObjectLogMessage(message, o));
        }

        public void Error<T>(string message, T o) where T : class
        {
            _log.Error(CreateObjectLogMessage(message, o));
        }

        public void Fatal<T>(string message, T o) where T : class
        {
            _log.Fatal(CreateObjectLogMessage(message, o));
        }
    }
}
