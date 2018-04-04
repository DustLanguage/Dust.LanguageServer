using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using LanguageServer.Parameters.TextDocument;

namespace Dust.LanguageServer
{
  public class TextDocumentManager
  {
    private List<TextDocument> Documents { get; } = new List<TextDocument>();
    public event Action<TextDocumentChangedEventArgs> OnChanged;

    public TextDocumentManager()
    {
      OnChanged += args => args.Document.Text = Regex.Replace(args.Document.Text, @"\r\n|\n\r|\r/g", "\n");
    }

    public void Add(TextDocument document)
    {
      if (Documents.Contains(document) == false)
      {
        Documents.Add(document);
        OnChanged?.Invoke(new TextDocumentChangedEventArgs(document));
      }
    }

    public TextDocument Get(Uri uri)
    {
      return Documents.Find(item => item.Uri == uri);
    }

    public void Change(Uri uri, long version, TextDocumentContentChangeEvent[] changeEvents)
    {
      TextDocument document = Documents.Find(item => item.Uri == uri);

      if (document == null)
      {
        return;
      }

      if (document.Version >= version)
      {
        return;
      }

      foreach (TextDocumentContentChangeEvent changeEvent in changeEvents)
      {
        Apply(document, changeEvent);
      }

      document.Version = version;
      OnChanged?.Invoke(new TextDocumentChangedEventArgs(document));
    }

    private void Apply(TextDocument document, TextDocumentContentChangeEvent changeEvent)
    {
      if (changeEvent.Range != null)
      {
        int startPos = GetPosition(document.Text, changeEvent.Range.Start.Line, changeEvent.Range.Start.Character);
        int endPos = GetPosition(document.Text, changeEvent.Range.End.Line, changeEvent.Range.End.Character);
        string newText = document.Text.Substring(0, startPos) + changeEvent.Text + document.Text.Substring(endPos);

        document.Text = newText;
      }
      else
      {
        document.Text = changeEvent.Text;
      }
    }

    private static int GetPosition(string text, int line, int character)
    {
      int position = 0;

      int newLineIndex;

      for (; 0 <= line; line--)
      {
        newLineIndex = text.IndexOf('\n', position);

        if (newLineIndex < 0)
        {
          return text.Length;
        }

        position = newLineIndex + 1;
      }

      newLineIndex = text.IndexOf('\n', position);
      int max = 0;

      if (newLineIndex < 0)
      {
        max = text.Length;
      }
      else if (newLineIndex > 0 && text[newLineIndex - 1] == '\r')
      {
        max = newLineIndex - 1;
      }
      else
      {
        max = newLineIndex;
      }

      position += character;
      return position < max ? position : max;
    }

    public void Remove(Uri uri)
    {
      TextDocument document = Documents.Find(item => item.Uri == uri);

      if (document != null)
      {
        Documents.Remove(document);
      }
    }
  }
}