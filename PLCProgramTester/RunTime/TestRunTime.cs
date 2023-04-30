using System;
using System.Collections.Generic;
using System.Device.Gpio;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            Dictionary<int, int> outputsIndexAddressPairs = testData.OutputsIndexAddressPairs;
            Dictionary<int, int> inputsIndexAddressPairs = testData.InputsIndexAddressPairs;

            CheckPoint checkPoint;
            int nextCheckTime;

            

            while(testData.CheckPoints.Count > 0)
            {
                checkPoint = testData.CheckPoints.Dequeue();
                nextCheckTime = testData.CheckPoints.Peek().StartTime;

                UpdateOutputs(checkPoint, outputsIndexAddressPairs);
                //Цикл, проверяющий входы раз в n миллисекунд



                //CheckPoint nextCheckPoint = 
                
            }


            DeactivateAllOutputs(outputsIndexAddressPairs);
        }



        /// <summary>
        /// Обновление выходов Raspberry в начале новой контрольной точки
        /// </summary>
        /// <param name="checkPoint">Новая контрольная точка</param>
        private static void UpdateOutputs(CheckPoint checkPoint, Dictionary<int, int> outputsIndexAddressPairs)
        {
            for(int i = 0; i < checkPoint.Outputs.Length; i++)
            {
                bool active = checkPoint.Outputs[i];
                PinValue pinValue = active ? PinValue.High : PinValue.Low;

                int raspberryAddress = outputsIndexAddressPairs[i];
                Program.Controller.Write(raspberryAddress, pinValue);
            }
            
        }

        /// <summary>
        /// Проверка входов Raspberry
        /// </summary>
        public static void ReadInputs()
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