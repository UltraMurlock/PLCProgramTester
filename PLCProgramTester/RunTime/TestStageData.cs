namespace PLCProgramTester.RunTime
{
    /// <summary>
    /// Класс, хранящий информацию о этапе теста
    /// </summary>
    internal class TestStageData
    {
        /// <summary>
        /// Продолжительность этапа в миллисекундах
        /// </summary>
        public int Duration;

        /// <summary>
        /// Массив активности выходов ПЛК, на который ориентируется алгоритм проверки на этом этапе
        /// </summary>
        public bool[] PLCOutputs;

        /// <summary>
        /// Массив активности входов ПЛК, отвечающий за их состояние на этом этапе
        /// </summary>
        public bool[] PLCInputs;



        public TestStageData()
        {
            PLCOutputs = new bool[0];
            PLCInputs = new bool[0];
        }
    }
}