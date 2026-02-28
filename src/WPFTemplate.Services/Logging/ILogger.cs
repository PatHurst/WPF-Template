namespace InnoJob.Services.Logging;

public interface ILogger
{
    void Info(string message);
    void Warn(string message);
    void Error(string message);
    void Error(Exception exception, string message = "");
}
