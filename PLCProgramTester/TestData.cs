using System.Device.Gpio;

namespace PLCProgramTester
{
    /// <summary>
    /// Класс для хранения информации о тесте
    /// </summary>
    internal class TestData
    {
        /// <summary>
        /// Очередь контрольны точек
        /// </summary>
        public Queue<CheckPoint> CheckPoints = new Queue<CheckPoint>();


        /// <summary>
        /// Словарь Индекс выхода -> Адрес пина Raspberry
        /// </summary>
        public Dictionary<int, int> OutputsIndexAddressPairs = new Dictionary<int, int>();

        /// <summary>
        /// Словарь Индекс входа -> Адрес пина Raspberry
        /// </summary>
        public Dictionary<int, int> InputsIndexAddressPairs = new Dictionary<int, int>();



        /// <summary>
        /// Выводит информацию о контрольных точках в консоль
        /// </summary>
        public void Print()
        {
            while(CheckPoints.Count > 0)
            {
                CheckPoint checkPoint = CheckPoints.Dequeue();
                Console.WriteLine($"\n\nStart time: {checkPoint.StartTime} ms");

                Console.WriteLine("Outputs:");
                if(checkPoint.Outputs != null)
                {
                    for(int i = 0; i < checkPoint.Outputs.Length; i++)
                    {
                        int address = OutputsIndexAddressPairs[i];
                        Console.Write($"{address} = {checkPoint.Outputs[i]}\t");
                    }
                }

                Console.WriteLine("\nInputs:");
                if(checkPoint.Inputs != null)
                {
                    for(int i = 0; i < checkPoint.Inputs.Length; i++)
                    {
                        int address = InputsIndexAddressPairs[i];
                        Console.Write($"{address} = {checkPoint.Inputs[i]}\t");
                    }
                }
            }
        }
    }
}