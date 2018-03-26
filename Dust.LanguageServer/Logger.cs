using LanguageServer;
using LanguageServer.Client;
using LanguageServer.Parameters.Window;

namespace LanguageServer
{
  public class Logger
  {
    public static Logger Instance { get; } = new Logger();

    private Proxy proxy;

    public void Attach(Connection connection)
    {
      proxy = connection == null ? null : new Proxy(connection);
    }

    public void Error(object message)
    {
      Send(MessageType.Error, message.ToString());
    }

    public void Warn(object message)
    {
      Send(MessageType.Warning, message.ToString());
    }

    public void Info(object message)
    {
      Send(MessageType.Info, message.ToString());
    }

    public void Log(object message)
    {
      Send(MessageType.Log, message.ToString());
    }

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