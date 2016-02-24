using System;

namespace MrGibbs.Contracts.Infrastructure
{
    /// <summary>
    /// simple logging wrapper to avoid dependencies on outside assemblies
    /// </summary>
    public interface ILogger
    {
        void Debug(string message);
        void Debug(string message, Exception exception);
        void Debug<T>(string message, T o) where T : class;
        void Info(string message);
        void Info(string message, Exception exception);
        void Info<T>(string message, T o) where T : class;
        void Warn(string message);
        void Warn(string message, Exception exception);
        void Warn<T>(string message, T o) where T : class;
        void Error(string message);
        void Error(string message, Exception exception);
        void Error<T>(string message, T o) where T : class;
        void Fatal(string message);
        void Fatal(string message, Exception exception);
        void Fatal<T>(string message, T o) where T : class;
    }
}
