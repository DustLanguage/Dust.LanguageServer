using Dust.Language;
using Dust.LanguageServer.Extensions;
using LanguageServer.Json;
using LanguageServer.Parameters;
using LanguageServer.Parameters.TextDocument;

namespace Dust.LanguageServer.Hovers
{
  public class HoverProvider : Provider
  {
    public HoverProvider(Project project)
      : base(project)
    {
    }

    public Hover GetHover(TextDocument document, Position position)
    {
      string word = document.GetWordAtPosition(position);

      DustContext context = document.GetContextAtPosition(position, Project.CompileFile(document.Text).GlobalContext);

      string content = "";

      if (context.ContainsPropety(word))
      {
        content = context.GetProperty(word).GetDetail();
      }

      if (context.ContainsFunction(word))
      {
        content = context.GetFunction(word).GetDetail();
      }

      if (string.IsNullOrEmpty(content) == false)
      {
        return new Hover
        {
          Contents = (StringOrObject<MarkedString>) $@"```dust
{content}
```"
        };
      }

      return new Hover();
/*

      string name = document.GetWordAtPosition(position);
      string[] lines = document.Text.Split('\n');

      lines[position.Line] = "";

      DustContext context = document.GetContextAtPosition(position, Project.CompileFile(string.Join('\n', lines)));

      Function function = context.GetFunction(name);

      if (function != null)
      {
        return new Hover
        {
          Contents = new StringOrObject<MarkedString>(new MarkedString
          {
            Value = function.GetDetail(),
            Language = "dust"
          })
        };
      }

      return new Hover();
*/
    }
  }
}