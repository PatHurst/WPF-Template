using System;
using System.Collections.Generic;
using System.Text;

namespace InnoJob.Services.Logging;

internal class FileLogger : ILogger
{
    public void Error(string message) => throw new NotImplementedException();
    public void Error(Exception exception, string message = "") => throw new NotImplementedException();
    public void Info(string message) => throw new NotImplementedException();
    public void Warn(string message) => throw new NotImplementedException();
}
