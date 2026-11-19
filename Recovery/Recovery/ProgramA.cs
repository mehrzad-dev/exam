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
            var data = Console.ReadLine() ?? string.Empty;
            string tempBackup = BackupFilePath + ".tmp";

            try
            {
                // تغییر: ابتدا نسخه جدید پشتیبان را کامل در فایل موقت می‌نویسیم.
                File.WriteAllText(tempBackup, data);

                // تغییر: پشتیبان قبلی را فقط پس از آماده‌شدن نسخه جدید جایگزین می‌کنیم.
                if (File.Exists(BackupFilePath))
                    File.Replace(tempBackup, BackupFilePath, null);
                else
                    File.Move(tempBackup, BackupFilePath);

                // تغییر: فقط بعد از آماده‌شدن پشتیبان، اطلاعات اصلی را ذخیره می‌کنیم.
                File.WriteAllText(DataFilePath, data);
            }
            finally
            {
                // تغییر: در پایان اجرای عادی یا وقوع خطا، فایل موقت باقی نماند.
                if (File.Exists(tempBackup))
                    File.Delete(tempBackup);
            }
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