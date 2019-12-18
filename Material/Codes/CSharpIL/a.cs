class zzz
{
    public static void Main()
    {
        System.Console.WriteLine("hi");
        new zzz();
    }

    zzz()
    {
        System.Console.WriteLine("bye");
    }

    static zzz()
    {
        System.Console.WriteLine("byes");
    }
}
