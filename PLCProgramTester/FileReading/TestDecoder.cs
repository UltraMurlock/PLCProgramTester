using PLCProgramTester.RunTime;

namespace PLCProgramTester.FileReading
{
    /// <summary>
    /// Класс, отвечающий за чтение теста из файла
    /// </summary>
    internal static class TestDecoder
    {
        private const string COMMENT_MARKER = "//";



        /// <summary>
        /// Загружает текстовый файл и преобразует его в тест
        /// </summary>
        /// <param name="path">Путь до текстового файла с тестом</param>
        /// <param name="test">Возвращаемый тест</param>
        /// <returns>Истина, если тест прочитан успешно</returns>
        public static bool TryDecodeTest(string path, out TestData test)
        {
            if(Settings.DebugMode)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"Чтение теста {path}");
                Console.ForegroundColor = ConsoleColor.Gray;
            }

            test = new TestData(path);
            if(!File.Exists(path))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Файл {path} не существует!");
                Console.ForegroundColor = ConsoleColor.Gray;
                return false;
            }
            string[] lines = File.ReadAllLines(path);

            //Поиск первой строки теста (строки инициализации)
            int i = 0;
            while (true)
            {
                if (i == lines.Length)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Все строки теста {path} являются комментариями или пустыми");
                    Console.ForegroundColor = ConsoleColor.Gray;
                    return false;
                }

                if (IsWhiteSpaceOrComment(lines[i]))
                {
                    i++;
                    continue;
                }

                if (!TryInitialize(lines[i], out test.GPIOinputs, out test.GPIOoutputs))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Ошибка инициализации теста {path} в строке {i}"); 
                    Console.ForegroundColor = ConsoleColor.Gray;
                    return false;
                }
                break;
            }


            //Обработка этапов теста
            for (i++; i < lines.Length; i++)
            {
                if (IsWhiteSpaceOrComment(lines[i]))
                    continue;

                if (!TryDecodeLine(lines[i], out var checkPoint))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Ошибка чтения этапа теста {path} в строке {i}");
                    Console.ForegroundColor = ConsoleColor.Gray;
                    return false;
                }
                test.Stages.Enqueue(checkPoint);
            }

            if(Settings.DebugMode)
                Console.WriteLine($"Тест {path} прочитан успешно");
            return true;
        }



        /// <summary>
        /// Возвращает истину, если строка пустая или являющаяся комментарием
        /// </summary>
        private static bool IsWhiteSpaceOrComment(string line)
        {
            if (line.StartsWith(COMMENT_MARKER))
                return true;
            if (string.IsNullOrWhiteSpace(line))
                return true;

            return false;
        }

        /// <summary>
        /// Преобразует строку инициализации теста 
        /// (первую не пустую и не закомментированную строку в файле теста)
        /// в массивы соответствия индекса столбцов входов/выходов в файле теста
        /// с портами Raspberry
        /// </summary>
        /// <param name="line">Строка для инициализации</param>
        /// <param name="inputGPIOAdresses">Входы Raspberry, расположенные в том же порядке, что и выходы ПЛК</param>
        /// <param name="outputGPIOAdresses">Выходы Raspberry, расположенные в том же порядке, что и входы ПЛК</param>
        /// <returns>Истина, если инициализация прошла успешно</returns>
        private static bool TryInitialize(string line, out int[] GPIOinputs, out int[] GPIOoutputs)
        {
            GPIOinputs = new int[0];
            GPIOoutputs = new int[0];

            List<int> GPIOinputsList = new List<int>();
            List<int> GPIOoutputsList = new List<int>();

            if (!TrySplitLine(line, out var splittedLine))
                return false;

            for (int i = 0; i < splittedLine.PLCOutputs.Length; i++)
            {
                string PLCAddress = splittedLine.PLCOutputs[i];
                if(!Settings.PLCtoRaspberryAddresses.ContainsKey(PLCAddress))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Запрашиваемый порт ПЛК {PLCAddress} не сопряжён с портом Raspberry в файле settings.ini");
                    Console.ForegroundColor = ConsoleColor.Gray;
                    return false;
                }

                int raspberryAddress = Settings.PLCtoRaspberryAddresses[PLCAddress];
                GPIOinputsList.Add(raspberryAddress);

                if(Settings.DebugMode)
                    Console.WriteLine($"Выход Raspberry {raspberryAddress} связан с {PLCAddress}");
            }
            GPIOinputs = GPIOinputsList.ToArray();

            for (int i = 0; i < splittedLine.PLCInputs.Length; i++)
            {
                string PLCAddress = splittedLine.PLCInputs[i];
                if(!Settings.PLCtoRaspberryAddresses.ContainsKey(PLCAddress))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Запрашиваемый порт ПЛК {PLCAddress} не сопряжён с портом Raspberry в файле settings.ini");
                    Console.ForegroundColor = ConsoleColor.Gray;
                    return false;
                }

                int raspberryAddress = Settings.PLCtoRaspberryAddresses[PLCAddress];
                GPIOoutputsList.Add(raspberryAddress);

                if(Settings.DebugMode)
                    Console.WriteLine($"Вход Raspberry {raspberryAddress} связан с {PLCAddress}");
            }
            GPIOoutputs = GPIOoutputsList.ToArray();

            return true;
        }

        /// <summary>
        /// Преобразует сырую строку об этапе теста в TestDtageData
        /// </summary>
        /// <returns>Истина, если преобразование прошло успешно</returns>
        private static bool TryDecodeLine(string line, out TestStageData stage)
        {
            stage = new TestStageData();

            if (!TrySplitLine(line, out var splittedLine))
                return false;

            if (!int.TryParse(splittedLine.Time, out stage.Duration))
                return false;
            if(!TryParseArrayToBool(splittedLine.PLCInputs, out stage.GPIOoutputs))
                return false;
            if (!TryParseArrayToBool(splittedLine.PLCOutputs, out stage.GPIOinputs))
                return false;

            return true;
        }

        /// <summary>
        /// Разбивает строку о этапе теста на строку с временем, 
        /// массив строк выходов и массив строк с входами
        /// для дальнейшей обработки
        /// </summary>
        /// <returns>Истина, если разбиение прошло успешно</returns>
        private static bool TrySplitLine(string line, out SplittedLine result)
        {
            result = new SplittedLine();

            string[] splittedLine = line.Split('(', ')');
            if (splittedLine.Length != 3)
                return false;

            string time = splittedLine[1];
            result.Time = time;

            string[] splittedPinsString = splittedLine[2].Split('|');
            if (splittedPinsString.Length != 2)
                return false;

            string[] PLCinputsString = splittedPinsString[0].Split(' ');
            PLCinputsString = RemoveWhiteSpaceStrings(PLCinputsString);
            result.PLCInputs = PLCinputsString;

            string[] PLCoutputsString = splittedPinsString[1].Split(' ');
            PLCoutputsString = RemoveWhiteSpaceStrings(PLCoutputsString);
            result.PLCOutputs = PLCoutputsString;

            return true;
        }

        /// <summary>
        /// Преобразует массив строк в массив соответствующих булевых значений
        /// </summary>
        /// <returns>Истина, если преобразование прошло успешно</returns>
        private static bool TryParseArrayToBool(string[] strings, out bool[] bools)
        {
            bools = new bool[strings.Length];

            for (int i = 0; i < bools.Length; i++)
            {
                int number;
                if (int.TryParse(strings[i], out number))
                {
                    if (number == 0)
                        bools[i] = false;
                    else if (number == 1)
                        bools[i] = true;
                    else
                        return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Удаляет из массива пустые строки
        /// </summary>
        /// <returns>Массив без пустых строк</returns>
        private static string[] RemoveWhiteSpaceStrings(string[] array)
        {
            List<string> strings = new List<string>(array);
            strings.RemoveAll(s => string.IsNullOrWhiteSpace(s));
            return strings.ToArray();
        }
    }
}