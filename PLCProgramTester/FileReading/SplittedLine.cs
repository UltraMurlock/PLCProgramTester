namespace PLCProgramTester.FileReading
{
    internal struct SplittedLine
    {
        public string Time;
        public string[] PLCOutputs;
        public string[] PLCInputs;



        public SplittedLine()
        {
            Time = String.Empty;
            PLCOutputs = new string[0];
            PLCInputs = new string[0];
        }
    }
}