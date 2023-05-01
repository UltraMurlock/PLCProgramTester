using INIIO;

namespace PLCProgramTester.FileReading
{
    /// <summary>
    /// Класс, отвечающий за чтение настроек из файла
    /// </summary>
    internal static class SettingsDecoder
    {
        private const bool _defaultDebug = false;
        private const int _defaultMaxErrorTime = 500;
        private const int _defaultChecksFrequency = 50;



        /// <summary>
        /// Загружает файл и считывает файл settings.ini
        /// </summary>
        public static void ReadSettings()
        {
            INIHandler handler = new INIHandler("settings.ini");

            bool debugMode = ReadDebugMode(handler);
            Settings.DebugMode = debugMode;

            int maxErrorTime = ReadMaxErrorTime(handler);
            Settings.MaxErrorTime = maxErrorTime;

            int checksFrequency = ReadChecksFrequency(handler);
            Settings.ChecksFrequency = checksFrequency;

            var PLCtoRaspberryAddresses = ReadPinsSettings(handler);
            Settings.PLCtoRaspberryAddresses = PLCtoRaspberryAddresses;
        }

        /// <summary>
        /// Ищет в файле настроек ключ "debug", 
        /// отвечающий за активирование режима дебага
        /// </summary>
        private static bool ReadDebugMode(INIHandler handler)
        {
            bool debugMode;
            if(!handler.TryGetBool(String.Empty, "debug", out debugMode))
            {
                Console.WriteLine($"Ошибка при чтении поля \"debug\" настроек. Использовано значение по умолчанию ({_defaultDebug})");
                return _defaultDebug;
            }
            return debugMode;
        }

        /// <summary>
        /// Ищет в файле настроек ключ "checks frequency", 
        /// отвечающий за время между проверками входов Raspberry 
        /// во время тестов
        /// </summary>
        /// <returns>Время в миллисекундах</returns>
        private static int ReadChecksFrequency(INIHandler handler)
        {
            int maxErrorTime;
            if(!handler.TryGetInt(String.Empty, "checks frequency", out maxErrorTime))
            {
                Console.WriteLine($"Ошибка при чтении поля \"checks frequency\" настроек. Использовано значение по умолчанию ({_defaultChecksFrequency})");
                return _defaultMaxErrorTime;
            }
            return maxErrorTime;
        }

        /// <summary>
        /// Ищет в файле настроек ключ "max time error", 
        /// отвечающий за максимально допустимое время 
        /// запаздания изменения состояния выхода ПЛК
        /// </summary>
        /// <returns>Время в миллисекундах</returns>
        private static int ReadMaxErrorTime(INIHandler handler)
        {
            int maxErrorTime;
            if(!handler.TryGetInt(String.Empty, "max error time", out maxErrorTime))
            {
                Console.WriteLine($"Ошибка при чтении поля \"max time error\" настроек. Использовано значение по умолчанию ({_defaultMaxErrorTime})");
                return _defaultMaxErrorTime;
            }
            return maxErrorTime;
        }

        /// <summary>
        /// Считывает входы/выходы из файла настроек
        /// </summary>
        /// <returns>Словарь контакт ПЛК - контакт Raspberry</returns>
        private static Dictionary<string, int> ReadPinsSettings(INIHandler handler)
        {
            var PLCtoRaspberryAddress = new Dictionary<string, int>();

            //Выходы Raspberry
            string[] PLCOutputs = new string[] { 
                "Y0", "Y1", "Y2", "Y3", "Y4", 
                "Y5", "Y6", "Y7", "Y8", "Y9",
                "YA", "YB", "YC", "YD" };
            foreach(var address in PLCOutputs)
            {
                int raspberryAddress;
                if(handler.TryGetInt("Addresses", address, out raspberryAddress))
                    PLCtoRaspberryAddress.Add(address, raspberryAddress);
            }


            //Входы Raspberry
            string[] PLCInputs = new string[] {
                "X0", "X1", "X2", "X3", "X4",
                "X5", "X6", "X7", "X8", "X9",
                "XA", "XB", "XC", "XD", "XE", 
                "XF" };
            foreach(var address in PLCInputs)
            {
                int raspberryAddress;
                if(handler.TryGetInt("Addresses", address, out raspberryAddress))
                    PLCtoRaspberryAddress.Add(address, raspberryAddress);
            }

            return PLCtoRaspberryAddress;
        }
    }
}