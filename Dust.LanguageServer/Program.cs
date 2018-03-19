using System;
using System.Text;
using LanguageServer.Client;
using LanguageServer.Parameters.Window;
using SampleServer;

namespace Dust.LanguageServer
{
  internal class Program
  {
    private static void Main(string[] args)
    {
      Console.OutputEncoding = Encoding.UTF8;

      App app = new App(Console.OpenStandardInput(), Console.OpenStandardOutput());

      Logger.Instance.Attach(app);

      new Proxy(app).Window.ShowMessage(new ShowMessageParams
      {
        Type = MessageType.Warning,
        Message = "Test2"
      });

      try
      {
        app.Listen().Wait();
      }
      catch (AggregateException ex)
      {
        Console.Error.WriteLine(ex.InnerExceptions[0]);
        Environment.Exit(-1);
      }
    }
  }
}