using System;
using System.Collections.Generic;
using LanguageServer.Parameters.TextDocument;
using SampleServer;

namespace Dust.LanguageServer
{
  public class TextDocumentManager
  {
    private List<TextDocumentItem> Documents { get; } = new List<TextDocumentItem>();
    public event Action<TextDocumentChangedEventArgs> OnChanged;

    public void Add(TextDocumentItem document)
    {
      if (Documents.Contains(document) == false)
      {
        Documents.Add(document);
        OnChanged?.Invoke(new TextDocumentChangedEventArgs(document));
      }
    }

    public TextDocumentItem Get(Uri uri)
    {
      return Documents.Find(item => item.Uri == uri);
    }

    public void Change(Uri uri, long version, TextDocumentContentChangeEvent[] changeEvents)
    {
      TextDocumentItem document = Documents.Find(item => item.Uri == uri);

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

    private void Apply(TextDocumentItem document, TextDocumentContentChangeEvent changeEvent)
    {
      if (changeEvent.Range != null)
      {
        int startPos = GetPosition(document.Text, (int) changeEvent.Range.Start.Line, (int) changeEvent.Range.Start.Character);
        int endPos = GetPosition(document.Text, (int) changeEvent.Range.End.Line, (int) changeEvent.Range.End.Character);
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
      TextDocumentItem document = Documents.Find(item => item.Uri == uri);

      if (document != null)
      {
        Documents.Remove(document);
      }
    }
  }
}