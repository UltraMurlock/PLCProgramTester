using PLCProgramTester.FileReading;
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

            if(!TestDecoder.TryDecodeTest("test.txt", out var test))
                return;
            
            if(Settings.DebugMode)
                test.Print();
        }

        private static void OpenPins(Dictionary<string, int> addressPairs)
        {
            foreach(var address in addressPairs.Keys)
            {
                int index = addressPairs[address];
                //Пока закомментировано, так как вызывает исключение на Windows
                //Controller.OpenPin(index);

                if(Settings.DebugMode)
                    Console.WriteLine($"Пин {index} открыт и подключён к {address}");
            }
        }
    }
}