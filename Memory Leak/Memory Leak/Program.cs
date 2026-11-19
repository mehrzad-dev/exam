namespace Memory_Leak;

class LargeObject
{
    public byte[] Data = new byte[1024 * 1024 * 10];
}

class LeakyManager
{
    private static List<LargeObject> _objects = new List<LargeObject>();

    public void AddObjects(int count, LargeObject obj)
    {
        for (int i = 0; i < count; i++)
        {
            _objects.Add(obj);
        }
    }
}

class Program
{
    static void Main(string[] args)
    {
        var manager = new LeakyManager();
        var obj = new LargeObject();
        
        for (int i = 0; i < 20; i++)
        {
            manager.AddObjects(5, obj);
            Console.WriteLine($"Iteration {i + 1}: Objects added.");
            Thread.Sleep(500);
        }

        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();
    }
}