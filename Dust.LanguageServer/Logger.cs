using LanguageServer;
using LanguageServer.Client;
using LanguageServer.Parameters.Window;

namespace SampleServer
{
  public class Logger
  {
    public static Logger Instance { get; } = new Logger();

    private Proxy proxy;

    public void Attach(Connection connection)
    {
      proxy = connection == null ? null : new Proxy(connection);
    }

    public void Error(string message) => Send(MessageType.Error, message);
    public void Warn(string message) => Send(MessageType.Warning, message);
    public void Info(string message) => Send(MessageType.Info, message);
    public void Log(string message) => Send(MessageType.Log, message);

    private void Send(MessageType type, string message)
    {
      proxy?.Window.LogMessage(new LogMessageParams
      {
        Type = type,
        Message = message
      });
    }
  }
}