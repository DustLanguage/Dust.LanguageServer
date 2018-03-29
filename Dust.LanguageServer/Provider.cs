namespace Dust.LanguageServer
{
  public class Provider
  {
    protected readonly Project project;

    public Provider(Project project)
    {
      this.project = project;
    }
  }
}