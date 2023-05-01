using PLCProgramTester.RunTime;

namespace PLCProgramTester
{
    /// <summary>
    /// Класс для хранения информации о тесте
    /// </summary>
    internal class TestData
    {
        /// <summary>
        /// Путь, по которому хранится данный тест
        /// </summary>
        public readonly string Path;

        /// <summary>
        /// Очередь контрольны точек
        /// </summary>
        public Queue<TestStageData> Stages = new Queue<TestStageData>();


        /// <summary>
        /// Индекс выхода ПЛК -> Порт Raspberry
        /// </summary>
        public Dictionary<int, int> PLCOutputsIndexGPIOPairs = new Dictionary<int, int>();

        /// <summary>
        /// Индекс входа ПЛК -> Порт Raspberry
        /// </summary>
        public Dictionary<int, int> PLCInputsIndexGPIOPairs = new Dictionary<int, int>();



        /// <summary>
        /// Конуструктор, присваивающий значение readonly полю Path
        /// </summary>
        /// <param name="path"></param>
        public TestData(string path)
        {
            Path = path;
        }



        /// <summary>
        /// Выводит информацию об этапах теста в консоль
        /// </summary>
        public void Print()
        {
            Console.WriteLine();
            for(int j = 0; j < Stages.Count; j++)
            {
                TestStageData stage = Stages.ElementAt(j);
                Console.WriteLine($"Продолжительность этапа: {stage.Duration} ms");

                Console.WriteLine("Выходы ПЛК:");
                if(stage.PLCOutputs != null)
                {
                    for(int i = 0; i < stage.PLCOutputs.Length; i++)
                    {
                        int address = PLCOutputsIndexGPIOPairs[i];
                        Console.Write($"{address} = {stage.PLCOutputs[i]}\t");
                    }
                }

                Console.WriteLine("\nВходы ПЛК:");
                if(stage.PLCInputs != null)
                {
                    for(int i = 0; i < stage.PLCInputs.Length; i++)
                    {
                        int address = PLCInputsIndexGPIOPairs[i];
                        Console.Write($"{address} = {stage.PLCInputs[i]}\t");
                    }
                }
                Console.WriteLine();
            }
            Console.WriteLine();
        }
    }
}