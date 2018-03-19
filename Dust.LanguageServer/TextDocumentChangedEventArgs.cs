using LanguageServer.Parameters.TextDocument;

namespace Dust.LanguageServer
{
  public class TextDocumentChangedEventArgs
  {
    public TextDocumentItem Document { get; }
    
    public TextDocumentChangedEventArgs(TextDocumentItem document)
    {
      Document = document;
    }
  }
}