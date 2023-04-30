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
            test = new TestData();
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

                if (!TryInitialize(lines[i], out var outputIndexAddressPairs, out var inputIndexAddressPairs))
                {
                    Console.WriteLine($"Ошибка инициализации теста {path} в строке {i}");
                    return false;
                }
                test.OutputsIndexAddressPairs = outputIndexAddressPairs;
                test.InputsIndexAddressPairs = inputIndexAddressPairs;
                break;
            }


            //Обработка контрольных точек
            for (i++; i < lines.Length; i++)
            {
                if (IsWhiteSpaceOrComment(lines[i]))
                    continue;

                if (!TryDecodeLine(lines[i], out var checkPoint))
                {
                    Console.WriteLine($"Ошибка чтения контрольной точки теста {path} в строке {i}");
                    return false;
                }
                test.CheckPoints.Enqueue(checkPoint);
            }

            Console.WriteLine($"Тест {path} прочитан успешно");
            return true;
        }



        private static string[] RemoveCommentedLines(string[] lines)
        {
            List<string> linesList = new List<string>(lines);
            for (int i = linesList.Count - 1; i >= 0; i--)
            {
                if (linesList[i].StartsWith(COMMENT_MARKER))
                    linesList.RemoveAt(i);
            }

            return linesList.ToArray();
        }

        private static bool IsWhiteSpaceOrComment(string line)
        {
            if (line.StartsWith(COMMENT_MARKER))
                return true;
            if (string.IsNullOrWhiteSpace(line))
                return true;

            return false;
        }

        private static bool TryInitialize(string line, out Dictionary<int, int> outputDictionary, out Dictionary<int, int> inputDictionary)
        {
            outputDictionary = new Dictionary<int, int>();
            inputDictionary = new Dictionary<int, int>();

            if (!TrySplitLine(line, out var splittedLine))
                return false;

            for (int i = 0; i < splittedLine.Outputs.Length; i++)
            {
                int address;
                if (!int.TryParse(splittedLine.Outputs[i], out address))
                    return false;
                outputDictionary.Add(i, address);
            }

            for (int i = 0; i < splittedLine.Inputs.Length; i++)
            {
                int address;
                if (!int.TryParse(splittedLine.Inputs[i], out address))
                    return false;
                inputDictionary.Add(i, address);
            }

            return true;
        }

        private static bool TryDecodeLine(string line, out CheckPoint checkPoint)
        {
            checkPoint = new CheckPoint();

            if (!TrySplitLine(line, out var splittedLine))
                return false;

            if (!int.TryParse(splittedLine.Time, out checkPoint.StartTime))
                return false;
            if (!TryParseArrayToBool(splittedLine.Outputs, out checkPoint.Outputs))
                return false;
            if (!TryParseArrayToBool(splittedLine.Inputs, out checkPoint.Inputs))
                return false;

            return true;
        }

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

        private static string[] RemoveWhiteSpaceStrings(string[] array)
        {
            List<string> strings = new List<string>(array);
            strings.RemoveAll(s => string.IsNullOrWhiteSpace(s));
            return strings.ToArray();
        }
    }
}