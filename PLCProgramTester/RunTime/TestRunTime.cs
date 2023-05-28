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
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"Тест {testData.Path} запущен в проверку");
            Console.ForegroundColor = ConsoleColor.Gray;

            int[] GPIOInputsAddresses = testData.GPIOinputs;
            int[] GPIOOutputsAddresses = testData.GPIOoutputs;

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
                UpdateOutputs(stage, GPIOOutputsAddresses);

                //Массив, помогающий отслеживать изменения состояний входов Raspberry (выходов ПЛК) во времени
                bool[] inputsPreviousStage = ReadInputs(GPIOInputsAddresses);
                while(stageStopwatch.ElapsedMilliseconds < stage.Duration)
                {
                    iterationStopwatch.Restart();

                    bool[] inputsCurrentStage = ReadInputs(GPIOInputsAddresses);
                    if(!CheckInputs(stage, (int)stageStopwatch.ElapsedMilliseconds, inputsCurrentStage, inputsPreviousStage, GPIOInputsAddresses))
                    {
                        errorFlag = true;
                        break;
                    }
                    inputsPreviousStage = inputsCurrentStage;


                    int sleepTime = Settings.ChecksFrequency - (int)iterationStopwatch.ElapsedMilliseconds;
                    sleepTime = sleepTime < 0 ? 0 : sleepTime;
                    Thread.Sleep(sleepTime);
                }
            }
            stageStopwatch.Stop();
            iterationStopwatch.Stop();

            DeactivateAllOutputs(GPIOOutputsAddresses);

            if(errorFlag)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Тест {testData.Path} провален!");
                Console.ForegroundColor = ConsoleColor.Gray;
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"Тест {testData.Path} успешно пройден!");
                Console.ForegroundColor = ConsoleColor.Gray;
            }

            return !errorFlag;
        }



        /// <summary>
        /// Обновление сигнала на выходах Raspberry (входах ПЛК)
        /// </summary>
        /// <param name="stage">Новая контрольная точка</param>
        private static void UpdateOutputs(TestStageData stage, int[] GPIOoutputsAddresses)
        {
            for(int i = 0; i < stage.GPIOoutputs.Length; i++)
            {
                bool active = stage.GPIOoutputs[i];
                PinValue pinValue = active ? PinValue.High : PinValue.Low;

                int raspberryAddress = GPIOoutputsAddresses[i];
#if !DEBUG
                Program.Controller.Write(raspberryAddress, pinValue);
#endif
                if(Settings.DebugMode)
                    Console.WriteLine($"GPIO выход {raspberryAddress} включён на {pinValue}");
            }
        }

        /// <summary>
        /// Проверка входов Raspberry (выходов ПЛК) на соответствие заданным на этапе
        /// </summary>
        private static bool CheckInputs(TestStageData stage, int timeFromStageStart, bool[] currentIterationActivity, bool[] previousIterationActivity, int[] GPIOinputsAddresses)
        {
            for(int i = 0; i < previousIterationActivity.Length; i++)
            {
                //Если значение правильное, то пропускаем
                if(currentIterationActivity[i] == stage.GPIOinputs[i])
                    continue;

                //Если значение на входе Raspberry не изменилось (не произошло инзменения с правильного на неправильное)
                //И время на изменение не закончилось (неправильное значение ещё может измениться в будущем без ошибки),
                //то пропускаем
                if(currentIterationActivity[i] == previousIterationActivity[i] && timeFromStageStart < Settings.MaxErrorTime)
                    continue;

                //Иначе, тест провален
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Ошибка на входе {GPIOinputsAddresses[i]} Raspberry");
                Console.ForegroundColor = ConsoleColor.Gray;
                return false;
            }

            return true;
        }

        /// <summary>
        /// Считывание активности входов Raspberry (выходов ПЛК)
        /// </summary>
        private static bool[] ReadInputs(int[] GPIOinputsAddresses)
        {
            bool[] inputsActivity = new bool[GPIOinputsAddresses.Length];

            for(int i = 0; i < GPIOinputsAddresses.Length; i++)
            {
                int gpio = GPIOinputsAddresses[i];
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
        private static void DeactivateAllOutputs(int[] outputsIndexAddressPairs)
        {
            foreach(var address in outputsIndexAddressPairs)
            {
#if !DEBUG
                Program.Controller.Write(address, PinValue.Low);
#endif
            }
        }
    }
}