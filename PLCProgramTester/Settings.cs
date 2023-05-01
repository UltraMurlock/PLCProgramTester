namespace PLCProgramTester
{
    /// <summary>
    /// Класс для хранения параметров приложения
    /// </summary>
    internal static class Settings
    {
        /// <summary>
        /// Словарь Контакт ПЛК -> Контакт Raspberry
        /// </summary>
        public static Dictionary<string, int> PLCtoRaspberryAddresses = new Dictionary<string, int>();

        /// <summary>
        /// Максимально допустимое время 
        /// запаздания изменения состояния выхода ПЛК
        /// в миллисекундах
        /// </summary>
        public static int MaxErrorTime;

        /// <summary>
        /// Время между проверками входов Raspberry 
        /// во время тестов
        /// </summary>
        public static int ChecksFrequency;

        /// <summary>
        /// Режим дебага
        /// </summary>
        public static bool DebugMode;
    }
}