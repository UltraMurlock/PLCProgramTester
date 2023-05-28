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
        /// Индекс выхода ПЛК -> Вход Raspberry
        /// </summary>
        public int[] GPIOinputs = new int[0];
        /// <summary>
        /// Индекс входа ПЛК -> Выход Raspberry
        /// </summary>
        public int[] GPIOoutputs = new int[0];



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

                Console.WriteLine("Выходы Raspberry:");
                if(stage.GPIOoutputs != null)
                {
                    for(int i = 0; i < stage.GPIOoutputs.Length; i++)
                    {
                        int address = GPIOoutputs[i];
                        Console.Write($"{address} = {stage.GPIOoutputs[i]}\t");
                    }
                }
                Console.WriteLine();

                Console.WriteLine("Входы Raspberry:");
                if(stage.GPIOinputs != null)
                {
                    for(int i = 0; i < stage.GPIOinputs.Length; i++)
                    {
                        int address = GPIOinputs[i];
                        Console.Write($"{address} = {stage.GPIOinputs[i]}\t");
                    }
                }
                Console.WriteLine();
                Console.WriteLine();
            }
            Console.WriteLine();
        }
    }
}