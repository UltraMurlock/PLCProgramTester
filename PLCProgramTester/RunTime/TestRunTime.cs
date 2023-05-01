using System.Device.Gpio;
using System.Diagnostics;

namespace PLCProgramTester.RunTime
{
    /// <summary>
    /// Класс, отвечающий за запуск и работу тестов
    /// </summary>
    internal static class TestRunTime
    {
        /// <summary>
        /// Запуск теста
        /// </summary>
        public static void Start(TestData testData)
        {
            Console.WriteLine($"Тест {testData.Path} запущен в проверку");

            Dictionary<int, int> outputsIndexAddressPairs = testData.PLCOutputsIndexGPIOPairs;
            Dictionary<int, int> inputsIndexAddressPairs = testData.PLCInputsIndexGPIOPairs;

            TestStageData stage;


            Stopwatch stageStopwatch = new Stopwatch();
            Stopwatch iterationStopwatch = new Stopwatch();
            while(testData.Stages.Count > 0)
            {
                stageStopwatch.Restart();

                stage = testData.Stages.Dequeue();
                UpdateOutputs(stage, outputsIndexAddressPairs);
                while(stageStopwatch.ElapsedMilliseconds < stage.Duration)
                {
                    iterationStopwatch.Restart();

                    CheckInputs(stage, inputsIndexAddressPairs);

                    int sleepTime = Settings.ChecksFrequency - (int)iterationStopwatch.ElapsedMilliseconds;
                    sleepTime = sleepTime < 0 ? 0 : sleepTime;
                    Thread.Sleep(sleepTime);
                }
            }
            stageStopwatch.Stop();
            iterationStopwatch.Stop();

            DeactivateAllOutputs(outputsIndexAddressPairs);
        }



        /// <summary>
        /// Обновление сигнала на входах ПЛК (выходах Raspberry)
        /// </summary>
        /// <param name="stage">Новая контрольная точка</param>
        private static void UpdateOutputs(TestStageData stage, Dictionary<int, int> outputsIndexAddressPairs)
        {
            for(int i = 0; i < stage.PLCOutputs.Length; i++)
            {
                bool active = stage.PLCOutputs[i];
                PinValue pinValue = active ? PinValue.High : PinValue.Low;

                int raspberryAddress = outputsIndexAddressPairs[i];
                //Пока закомментировано, так как вызывает исключение на Windows
                //Program.Controller.Write(raspberryAddress, pinValue);
            }

        }

        /// <summary>
        /// Проверка выходов ПЛК (входов Raspberry)
        /// </summary>
        public static void CheckInputs(TestStageData stage, Dictionary<int, int> inputsIndexAddressPairs)
        {
            //Проверка входов и запись их в логи
        }


        public static void DeactivateAllOutputs(Dictionary<int, int> outputsIndexAddressPairs)
        {
            var addresses = outputsIndexAddressPairs.Values;

            foreach(var address in addresses)
            {
                //Пока закомментировано, так как вызывает исключение на Windows
                //Program.Controller.Write(address, PinValue.Low);
            }
        }
    }
}