﻿using INIIO;

namespace PLCProgramTester.FileReading
{
    /// <summary>
    /// Класс, отвечающий за чтение настроек из файла
    /// </summary>
    internal static class SettingsDecoder
    {
        private const string _keyDebug = "debug";
        private const string _keyMaxErrorTime = "max error time";
        private const string _keyChecksFrequency = "checks frequency";
        private static readonly string[] _keysPLCOutputs = new string[] {
            "Y0", "Y1", "Y2", "Y3", "Y4", "Y5", "Y6", "Y7", "Y8", "Y9",
            "YA", "YB", "YC", "YD" };
        private static readonly string[] _keysPLCInputs = new string[] {
            "X0", "X1", "X2", "X3", "X4", "X5", "X6", "X7", "X8", "X9",
            "XA", "XB", "XC", "XD", "XE", "XF" };

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

            if(Settings.DebugMode)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"Чтение файла параметров");
                Console.ForegroundColor = ConsoleColor.Gray;
            }

            int maxErrorTime = ReadMaxErrorTime(handler);
            Settings.MaxErrorTime = maxErrorTime;

            int checksFrequency = ReadChecksFrequency(handler);
            Settings.ChecksFrequency = checksFrequency;

            var PLCtoRaspberryAddresses = ReadPinsSettings(handler);
            Settings.PLCtoRaspberryAddresses = PLCtoRaspberryAddresses;
        }

        /// <summary>
        /// Ищет в файле настроек ключ _keyDebug, 
        /// отвечающий за активирование режима дебага
        /// </summary>
        private static bool ReadDebugMode(INIHandler handler)
        {
            bool debugMode;
            if(!handler.TryGetBool(String.Empty, _keyDebug, out debugMode))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Ошибка при чтении поля \"{_keyDebug}\" настроек. Использовано значение по умолчанию ({_defaultDebug})");
                Console.ForegroundColor = ConsoleColor.Gray;
                return _defaultDebug;
            }
            return debugMode;
        }

        /// <summary>
        /// Ищет в файле настроек ключ _keyChecksFrequency, 
        /// отвечающий за время между проверками входов Raspberry 
        /// во время тестов
        /// </summary>
        /// <returns>Время в миллисекундах</returns>
        private static int ReadChecksFrequency(INIHandler handler)
        {
            int maxErrorTime;
            if(!handler.TryGetInt(String.Empty, _keyChecksFrequency, out maxErrorTime))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Ошибка при чтении поля \"{_keyChecksFrequency}\" настроек. Использовано значение по умолчанию ({_defaultChecksFrequency})");
                Console.ForegroundColor = ConsoleColor.Gray;
                return _defaultMaxErrorTime;
            }
            return maxErrorTime;
        }

        /// <summary>
        /// Ищет в файле настроек ключ _keyMaxErrorTime, 
        /// отвечающий за максимально допустимое время 
        /// запаздания изменения состояния выхода ПЛК
        /// </summary>
        /// <returns>Время в миллисекундах</returns>
        private static int ReadMaxErrorTime(INIHandler handler)
        {
            int maxErrorTime;
            if(!handler.TryGetInt(String.Empty, _keyMaxErrorTime, out maxErrorTime))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Ошибка при чтении поля \"{_keyMaxErrorTime}\" настроек. Использовано значение по умолчанию ({_defaultMaxErrorTime})");
                Console.ForegroundColor = ConsoleColor.Gray;
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
            foreach(var address in _keysPLCOutputs)
            {
                int raspberryAddress;
                if(handler.TryGetInt("Addresses", address, out raspberryAddress))
                    PLCtoRaspberryAddress.Add(address, raspberryAddress);
            }


            //Входы Raspberry
            foreach(var address in _keysPLCInputs)
            {
                int raspberryAddress;
                if(handler.TryGetInt("Addresses", address, out raspberryAddress))
                    PLCtoRaspberryAddress.Add(address, raspberryAddress);
            }

            return PLCtoRaspberryAddress;
        }
    }
}