namespace Dndcs2.patterns;

public class Singleton
{
    private static readonly Singleton Reference = new Singleton();

    // Explicit static constructor to tell C# compiler
    // not to mark type as beforefieldinit
    static Singleton()
    {
    }

    protected Singleton()
    {
    }

    public static Singleton Instance
    {
        get
        {
            return Reference;
        }
    }
}