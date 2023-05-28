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
        /// <returns>Истина, если тест пройден успешно</returns>
        public static bool Start(TestData testData)
        {
            Console.WriteLine($"Тест {testData.Path} запущен в проверку");

            //!!!НЕ ЗАБЫТЬ!!!
            //Перестроить использование словарей на использование массивов, так как
            //сейчас массив ключей всё равно совпадает с индексами в массиве значений
            Dictionary<int, int> outputsIndexAddressPairs = testData.PLCOutputsIndexGPIOPairs;
            Dictionary<int, int> inputsIndexAddressPairs = testData.PLCInputsIndexGPIOPairs;

            TestStageData stage;


            Stopwatch stageStopwatch = new Stopwatch();
            Stopwatch iterationStopwatch = new Stopwatch();

            bool errorFlag = false;
            while(testData.Stages.Count > 0 && !errorFlag)
            {
                if(Settings.DebugMode)
                    Console.WriteLine("Следующий этап теста");

                stageStopwatch.Restart();

                stage = testData.Stages.Dequeue();
                UpdateOutputs(stage, outputsIndexAddressPairs);

                //Массив, помогающий отслеживать изменения состояний выходов ПЛК во времени
                bool[] PLCOutputsPreviousStage = ReadInputs(inputsIndexAddressPairs);
                while(stageStopwatch.ElapsedMilliseconds < stage.Duration)
                {
                    iterationStopwatch.Restart();

                    bool[] PLCOutputsCurrentStage = ReadInputs(inputsIndexAddressPairs);
                    if(!CheckInputs(stage, (int)stageStopwatch.ElapsedMilliseconds, PLCOutputsCurrentStage, PLCOutputsPreviousStage, inputsIndexAddressPairs))
                    {
                        errorFlag = true;
                        break;
                    }
                    PLCOutputsPreviousStage = PLCOutputsCurrentStage;


                    int sleepTime = Settings.ChecksFrequency - (int)iterationStopwatch.ElapsedMilliseconds;
                    sleepTime = sleepTime < 0 ? 0 : sleepTime;
                    Thread.Sleep(sleepTime);
                }
            }
            stageStopwatch.Stop();
            iterationStopwatch.Stop();

            DeactivateAllOutputs(outputsIndexAddressPairs);

            if(errorFlag)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Тест {testData.Path} провален!");
                Console.ForegroundColor = ConsoleColor.Gray;
            }
            return !errorFlag;
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
#if !DEBUG
                Program.Controller.Write(raspberryAddress, pinValue);
#endif
            }
        }

        /// <summary>
        /// Проверка выходов ПЛК на соответствие заданным на этапе (входов Raspberry)
        /// </summary>
        private static bool CheckInputs(TestStageData stage, int timeFromStageStart, bool[] currentIterationActivity, bool[] previousIterationActivity, Dictionary<int, int> inputsIndexAddressPairs)
        {
            for(int i = 0; i < previousIterationActivity.Length; i++)
            {
                //Если значение правильное, то пропускаем
                if(currentIterationActivity[i] == stage.PLCOutputs[i])
                    continue;

                //Если значение на входе Raspberry не изменилось (не произошло инзменения с правильного на неправильное)
                //И время на изменение не закончилось (неправильное значение ещё может измениться в будущем без ошибки),
                //то пропускаем
                if(currentIterationActivity[i] == previousIterationActivity[i] && timeFromStageStart < Settings.MaxErrorTime)
                    continue;

                //Иначе, тест провален
                return false;
            }

            return true;
        }

        /// <summary>
        /// Считывание активности выходов ПЛК (входов Raspberry)
        /// </summary>
        private static bool[] ReadInputs(Dictionary<int, int> inputsIndexAddressPairs)
        {
            bool[] inputsActivity = new bool[inputsIndexAddressPairs.Count];

            for(int i = 0; i < inputsIndexAddressPairs.Count; i++)
            {
                int gpio = inputsIndexAddressPairs[i];
#if !DEBUG
                PinValue pinValue = Program.Controller.Read(gpio);
                inputsActivity[i] = pinValue == PinValue.High;
#endif
            }
            return inputsActivity;
        }

        /// <summary>
        /// Установка PinValue.Low на всех выходах Raspberry
        /// </summary>
        private static void DeactivateAllOutputs(Dictionary<int, int> outputsIndexAddressPairs)
        {
            var addresses = outputsIndexAddressPairs.Values;

            foreach(var address in addresses)
            {
#if !DEBUG
                Program.Controller.Write(address, PinValue.Low);
#endif
            }
        }
    }
}