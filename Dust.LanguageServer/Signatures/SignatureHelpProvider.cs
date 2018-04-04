using System;
using System.Linq;
using System.Text;
using Dust.Language;
using Dust.Language.Nodes.Expressions;
using Dust.LanguageServer.Extensions;
using LanguageServer.Parameters;
using LanguageServer.Parameters.TextDocument;

namespace Dust.LanguageServer.Signatures
{
  public class SignatureHelpProvider : Provider
  {
    public SignatureHelpProvider(Project project)
      : base(project)
    {
    }

    public SignatureHelp GetSignatureHelp(TextDocument document, Position position)
    {
      string text = document.Text;
      int index = document.GetPosition(position);
      string[] lines = text.Split('\n');
      string line = lines[position.Line];

      int startIndex = line.Substring(0, position.Character).LastIndexOf("(", StringComparison.Ordinal);

      if (startIndex != -1)
      {
        string functionName = line.Substring(0, startIndex).Trim().Split(" ").Last();

        // Remove the current line because it might contain errors.
        lines[position.Line] = "";

        DustContext currentContext = document.GetContextAtPosition(position, Project.CompileFile(string.Join('\n', lines)).GlobalContext);

        Function function = currentContext.GetFunction(functionName);

        StringBuilder labelBuilder = new StringBuilder($"{function.Name}(");

        if (function.Parameters.Length > 0)
        {
          for (int i = 0; i < function.Parameters.Length; i++)
          {
            labelBuilder.Append($"{function.Parameters[i].Identifier.Name}: any");

            if (i != function.Parameters.Length - 1)
            {
              labelBuilder.Append(", ");
            }
          }
        }

        labelBuilder.Append("): any");

        return new SignatureHelp
        {
          ActiveParameter = line.Substring(startIndex, line.IndexOf(")", startIndex, StringComparison.Ordinal) - startIndex).Count(character => character == ','),
          ActiveSignature = 0,
          Signatures = new[]
          {
            new SignatureInformation
            {
              Label = labelBuilder.ToString(),
              Parameters = function.Parameters.Select(parameter => new ParameterInformation
              {
                Label = parameter.Identifier.Name + ": any"
              }).ToArray()
            }
          }
        };
      }

      return new SignatureHelp();
    }
  }
}