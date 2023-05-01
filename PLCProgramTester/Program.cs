﻿using PLCProgramTester.FileReading;
using PLCProgramTester.RunTime;
using System.Device.Gpio;

namespace PLCProgramTester
{
    internal class Program
    {
        public static GpioController Controller = new GpioController();



        private static void Main()
        {
            SettingsDecoder.ReadSettings();
            OpenPins(Settings.PLCtoRaspberryAddresses);

            DoTest("test.txt");
        }

        private static void DoTest(string path)
        {
            if(!TestDecoder.TryDecodeTest(path, out var test))
            {
                Console.WriteLine($"Тест {path} пропущен из-за ошибки");
                return;
            }

            if(Settings.DebugMode)
                test.Print();

            TestRunTime.Start(test);
        }

        private static void OpenPins(Dictionary<string, int> addressPairs)
        {
            Console.WriteLine("Открытие GPIO-пинов Raspberry и их связывание с ПЛК");
            foreach(var address in addressPairs.Keys)
            {
                int index = addressPairs[address];
                //Пока закомментировано, так как вызывает исключение на Windows
                //Controller.OpenPin(index);

                if(Settings.DebugMode)
                    Console.WriteLine($"GPIO {index} -> {address}");
            }
            Console.WriteLine();
        }
    }
}