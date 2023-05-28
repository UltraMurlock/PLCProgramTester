using PLCProgramTester.FileReading;
using PLCProgramTester.RunTime;
using System.Device.Gpio;

namespace PLCProgramTester
{
    internal class Program
    {
        public static GpioController Controller = new GpioController();


        /* Примечание: все тесты должны храниться в папке tests в корневом каталоге программы.
         * Путь к тестам и директориям также вводится относительно папки tests.
         * Тесты должны иметь расширение .txt, а имя должно включать расширение */
        private static void Main()
        {
            Initialize();

            while(true)
            {
                Console.WriteLine("\nВведите имя файла или название директории с тестами");
                string? rawInput = Console.ReadLine();
                if(String.IsNullOrEmpty(rawInput))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Ничего не введено!");
                    Console.ForegroundColor = ConsoleColor.Gray;
                    continue;
                }

                ReadInput(rawInput);
            }
        }



        private static void Initialize()
        {
            SettingsDecoder.ReadSettings();
            OpenPins(Settings.PLCtoRaspberryAddresses);
        }

        /// <summary>
        /// Определяет, что содержит строка и запускает либо одиночный тест,
        /// либо очередь тестов
        /// </summary>
        private static void ReadInput(string path)
        {
            if(path.EndsWith(".txt"))
            {
                DoTest(Settings.TestDirectory + path);
            }
            else
            {
                DoQueueOfTests(Settings.TestDirectory + path);
            }
        }

        /// <summary>
        /// Запускает в проверку очередь тестов
        /// </summary>
        /// <param name="path">Путь к директории</param>
        /// 
        private static bool DoQueueOfTests(string path)
        {
            if(!Directory.Exists(path))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Директория {path} не существует");
                Console.ForegroundColor = ConsoleColor.Gray;
                return false;
            }

            foreach(var testPath in Directory.EnumerateFiles(path))
            {
                if(!DoTest(testPath))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Программа не смогла пройти все тесты!");
                    Console.ForegroundColor = ConsoleColor.Gray;
                    return false;
                }
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Программа успешно прошла все тесты");
            Console.ForegroundColor = ConsoleColor.Gray;
            return true;
        }

        /// <summary>
        /// Запускает в проверку одиночный тест
        /// </summary>
        /// <param name="path">Путь к файлу</param>
        private static bool DoTest(string path)
        {
            if(!TestDecoder.TryDecodeTest(path, out var test))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Тест {path} пропущен из-за ошибки чтения");
                Console.ForegroundColor = ConsoleColor.Gray;
                return false;
            }

            if(Settings.DebugMode)
                test.Print();

            return TestRunTime.Start(test);
        }

        /// <summary>
        /// Открытие GPIO-пинов Raspberry и их связывание с ПЛК
        /// </summary>
        private static void OpenPins(Dictionary<string, int> addressPairs)
        {
            if(Settings.DebugMode)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Открытие GPIO-пинов Raspberry и их связывание с ПЛК");
                Console.ForegroundColor = ConsoleColor.Gray;
            }

            foreach(var address in addressPairs.Keys)
            {
                int index = addressPairs[address];
#if !DEBUG
                Controller.OpenPin(index);
#endif
                if(Settings.DebugMode)
                    Console.WriteLine($"GPIO {index} -> {address}");
            }
            Console.WriteLine();
        }
    }
}