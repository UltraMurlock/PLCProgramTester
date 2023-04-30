namespace INIIO
{
    public class INIHandler
    {
        private Dictionary<string, List<string>> _regions;



        /// <summary>
        /// Загружает ini файл и разбивает его на регионы
        /// </summary>
        /// <param name="path">Путь к файлу</param>
        public INIHandler(string path)
        {
            _regions = new Dictionary<string, List<string>>();
            string[] lines = File.ReadAllLines(path);
            SeparateIntoRegions(lines);
        }



        /// <summary>
        /// Метод для получения целочисленного значения по региону и ключу
        /// </summary>
        /// <param name="regionHeader">Ключ заголовка (String.Empty для строк вне заголовков)</param>
        /// <param name="key">Ключ значения</param>
        public bool TryGetInt(string regionHeader, string key, out int value)
        {
            string stringValue = GetValue(regionHeader, key);
            if(!int.TryParse(stringValue, out value))
                return false;

            return true;
        }

        /// <summary>
        /// Метод для получения булевого значения по региону и ключу
        /// </summary>
        /// <param name="regionHeader">Ключ заголовка (String.Empty для строк вне заголовков)</param>
        /// <param name="key">Ключ значения</param>
        public bool TryGetBool(string regionHeader, string key, out bool value)
        {
            string stringValue = GetValue(regionHeader, key);
            if(!bool.TryParse(stringValue, out value))
                return false;

            return true;
        }

        /// <summary>
        /// Метод для получения строкового значения по региону и ключу
        /// </summary>
        /// <param name="regionHeader">Ключ заголовка (String.Empty для строк вне заголовков)</param>
        /// <param name="key">Ключ значения</param>
        public string GetValue(string regionHeader, string key)
        {
            if(!_regions.TryGetValue(regionHeader, out var region))
                return string.Empty;

            for(int i = 0; i < region.Count; i++)
            {
                if(IsCommentOrEmptyLine(region[i]))
                    continue;
                if(!TrySplitLine(region[i], out var pair))
                    continue;

                if(pair.key == key)
                    return pair.value;
            }

            return String.Empty;
        }



        private void SeparateIntoRegions(string[] lines)
        {
            string currentRegion = String.Empty;
            _regions.Add(String.Empty, new List<string>());
            for(int i = 0; i < lines.Length; i++)
            {
                if(IsRegionHeader(lines[i], out var header))
                {
                    currentRegion = header;
                    _regions.Add(header, new List<string>());
                    continue;
                }

                _regions[currentRegion].Add(lines[i]);
            }
        }

        private bool IsRegionHeader(string line, out string header)
        {
            header = String.Empty;
            if(IsCommentOrEmptyLine(line))
                return false;

            line.Trim();
            if(line.Length < 3)
                return false;

            if(line[0] == '[' && line[line.Length - 1] == ']')
            {
                header = line.Substring(1, line.Length - 2);
                return true;
            }
            
            return false;
        }
        
        private bool IsCommentOrEmptyLine(string line)
        {
            if(String.IsNullOrEmpty(line))
                return true;
            if(line.StartsWith("//"))
                return true;

            return false;
        }

        private bool TrySplitLine(string line, out (string key, string value) keyValuePair)
        {
            keyValuePair = (String.Empty, String.Empty);

            string[] splittedLine = line.Split('=');
            if(splittedLine.Length != 2)
                return false;

            splittedLine[0].Trim();
            keyValuePair.key = splittedLine[0];

            splittedLine[1].Trim();
            keyValuePair.value = splittedLine[1];
            return true;
        }
    }
}