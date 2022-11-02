namespace HADBOL;

public static class Program
{
    //This is an implementation of SNOBAL.
    private static void Main(string[] args)
    {
        if (args.Length == 0)
            return;
        var fullpath = Path.GetFullPath(args[0]);
        var entryFile = File.ReadAllText(fullpath);
        Parser parser = new(entryFile);

        Console.Write(entryFile);
        Console.WriteLine();
    }
}