namespace Recovery
{
    class Program
    {
        static string DataFilePath = "data.txt";
        static string BackupFilePath = "backup.txt";

        static void Main(string[] args)
        {
            Console.WriteLine("1. Write Data");
            Console.WriteLine("2. Simulate Disaster (Delete Data)");
            Console.WriteLine("3. Recover Data");
            Console.WriteLine("4. Show Data");
            Console.WriteLine("5. Exit");

            while (true)
            {
                Console.Write("Select option: ");
                var input = Console.ReadLine();
                if (input == "1")
                {
                    WriteData();
                }
                else if (input == "2")
                {
                    SimulateDisaster();
                }
                else if (input == "3")
                {
                    RecoverData();
                }
                else if (input == "4")
                {
                    ShowData();
                }
                else if (input == "5")
                {
                    break;
                }
            }
        }

        static void WriteData()
        {
            Console.Write("Enter data: ");
            var data = Console.ReadLine();
            File.WriteAllText(DataFilePath, data);
        }

        static void SimulateDisaster()
        {
            if (File.Exists(DataFilePath))
            {
                File.Delete(DataFilePath);
            }
        }

        static void RecoverData()
        {
            if (File.Exists(BackupFilePath))
            {
                File.Copy(BackupFilePath, DataFilePath, true);
            }
            else
            {
                Console.WriteLine("No backup available.");
            }
        }

        static void ShowData()
        {
            if (File.Exists(DataFilePath))
            {
                var data = File.ReadAllText(DataFilePath);
                Console.WriteLine($"Data: {data}");
            }
            else
            {
                Console.WriteLine("No data file found.");
            }
        }
    }
}