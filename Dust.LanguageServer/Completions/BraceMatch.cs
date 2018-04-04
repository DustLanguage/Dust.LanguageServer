using System;

namespace Dust.LanguageServer.Completion
{
  public class BraceMatch : IEquatable<BraceMatch>
  {
    public int Start { get; }
    public int End { get; }

    public BraceMatch(int start, int end)
    {
      Start = start;
      End = end;
    }

    public bool Equals(BraceMatch other)
    {
      if (ReferenceEquals(null, other)) return false;
      if (ReferenceEquals(this, other)) return true;
      
      return Start == other.Start && End == other.End;
    }

    public override bool Equals(object obj)
    {
      if (ReferenceEquals(null, obj)) return false;
      if (ReferenceEquals(this, obj)) return true;
      
      return obj.GetType() == GetType() && Equals((BraceMatch) obj);
    }

    public override int GetHashCode()
    {
      unchecked
      {
        return (Start * 397) ^ End;
      }
    }
  }
}