using System.Text;
using Dust.Language.Nodes.Expressions;

namespace Dust.LanguageServer.Extensions
{
  public static class FunctionExtensions
  {
    public static string GetDetail(this Function function)
    {
      StringBuilder builder = new StringBuilder($"let fn {function.Name}");

      builder.Append("(");

      if (function.Parameters.Length > 0)
      {
        for (int i = 0; i < function.Parameters.Length; i++)
        {
          builder.Append($"{function.Parameters[i].Identifier.Name}: any");

          if (i != function.Parameters.Length - 1)
          {
            builder.Append(", ");
          }
        }
      }

      builder.Append("): any");

      return builder.ToString();
    }
  }
}