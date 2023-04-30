namespace PLCProgramTester
{
    /// <summary>
    /// Класс, хранящий информацию о контрольной точке теста
    /// </summary>
    internal class CheckPoint
    {
        /// <summary>
        /// Время старта этого чекпоинта от начала теста в миллисекундах
        /// </summary>
        public int StartTime;

        /// <summary>
        /// Массив активности выходов, отвечающий за их состояние на этом этапе
        /// </summary>
        public bool[] Outputs;

        /// <summary>
        /// Массив активности входов, на который ориентируется алгоритм проверки на этом этапе
        /// </summary>
        public bool[] Inputs;



        public CheckPoint()
        {
            Outputs = new bool[0];
            Inputs = new bool[0];
        }
    }
}