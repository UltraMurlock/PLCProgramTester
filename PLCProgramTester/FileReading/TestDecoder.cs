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
            test = new TestData(path);
            string[] lines = File.ReadAllLines(path);

            //Поиск первой строки теста (строки инициализации)
            int i = 0;
            while (true)
            {
                if (i == lines.Length)
                {
                    Console.WriteLine($"Все строки теста {path} являются комментариями или пустыми");
                    return false;
                }

                if (IsWhiteSpaceOrComment(lines[i]))
                {
                    i++;
                    continue;
                }

                if (!TryInitialize(lines[i], out var inputIndexAddressPairs, out var outputIndexAddressPairs))
                {
                    Console.WriteLine($"Ошибка инициализации теста {path} в строке {i}");
                    return false;
                }
                test.PLCInputsIndexGPIOPairs = inputIndexAddressPairs;
                test.PLCOutputsIndexGPIOPairs = outputIndexAddressPairs;
                break;
            }


            //Обработка контрольных точек
            for (i++; i < lines.Length; i++)
            {
                if (IsWhiteSpaceOrComment(lines[i]))
                    continue;

                if (!TryDecodeLine(lines[i], out var checkPoint))
                {
                    Console.WriteLine($"Ошибка чтения этапа теста {path} в строке {i}");
                    return false;
                }
                test.Stages.Enqueue(checkPoint);
            }

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
        /// <param name="outputDictionary">Индекс столбца (выхода ПЛК) -> Порт Raspberry</param>
        /// <param name="inputDictionary">Индекс столбца (входа ПЛК) -> Порт Raspberry</param>
        /// <returns>Истина, если инициализация прошла успешно</returns>
        private static bool TryInitialize(string line, out Dictionary<int, int> inputDictionary, out Dictionary<int, int> outputDictionary)
        {
            outputDictionary = new Dictionary<int, int>();
            inputDictionary = new Dictionary<int, int>();

            if (!TrySplitLine(line, out var splittedLine))
                return false;

            for (int i = 0; i < splittedLine.Outputs.Length; i++)
            {
                string PLCAddress = splittedLine.Outputs[i];
                if(!Settings.PLCtoRaspberryAddresses.ContainsKey(PLCAddress))
                {
                    Console.WriteLine($"Запрашиваемый порт ПЛК {PLCAddress} не сопряжён с портом Raspberry в файле settings.ini");
                    return false;
                }

                int raspberryAddress = Settings.PLCtoRaspberryAddresses[PLCAddress];
                outputDictionary.Add(i, raspberryAddress);
            }

            for (int i = 0; i < splittedLine.Inputs.Length; i++)
            {
                string PLCAddress = splittedLine.Inputs[i];
                if(!Settings.PLCtoRaspberryAddresses.ContainsKey(PLCAddress))
                {
                    Console.WriteLine($"Запрашиваемый порт ПЛК {PLCAddress} не сопряжён с портом Raspberry в файле settings.ini");
                    return false;
                }

                int raspberryAddress = Settings.PLCtoRaspberryAddresses[PLCAddress];
                inputDictionary.Add(i, raspberryAddress);
            }

            return true;
        }

        /// <summary>
        /// Преобразует сырую строку об этапе теста в TestDtageData
        /// </summary>
        /// <returns>Истина, если преобразование прошло успешно</returns>
        private static bool TryDecodeLine(string line, out TestStageData checkPoint)
        {
            checkPoint = new TestStageData();

            if (!TrySplitLine(line, out var splittedLine))
                return false;

            if (!int.TryParse(splittedLine.Time, out checkPoint.Duration))
                return false;
            if (!TryParseArrayToBool(splittedLine.Outputs, out checkPoint.PLCOutputs))
                return false;
            if (!TryParseArrayToBool(splittedLine.Inputs, out checkPoint.PLCInputs))
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

            string[] outputsString = splittedPinsString[0].Split(' ');
            outputsString = RemoveWhiteSpaceStrings(outputsString);
            result.Outputs = outputsString;

            string[] inputsString = splittedPinsString[1].Split(' ');
            inputsString = RemoveWhiteSpaceStrings(inputsString);
            result.Inputs = inputsString;

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