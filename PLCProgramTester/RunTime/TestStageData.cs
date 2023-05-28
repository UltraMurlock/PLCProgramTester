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
        /// Массив активности входов Raspberry (выходов ПЛК), на который ориентируется алгоритм проверки на этом этапе
        /// </summary>
        public bool[] GPIOinputs = new bool[0];

        /// <summary>
        /// Массив активности выходов Raspberry (входов ПЛК), отвечающий за их состояние на этом этапе
        /// </summary>
        public bool[] GPIOoutputs = new bool[0];



        public TestStageData() { }
    }
}