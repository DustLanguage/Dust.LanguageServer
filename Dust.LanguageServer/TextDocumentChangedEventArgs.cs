using LanguageServer.Parameters.TextDocument;

namespace Dust.LanguageServer
{
  public class TextDocumentChangedEventArgs
  {
    public TextDocument Document { get; }
    
    public TextDocumentChangedEventArgs(TextDocument document)
    {
      Document = document;
    }
  }
}