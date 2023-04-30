namespace PLCProgramTester.FileReading
{
    internal struct SplittedLine
    {
        public string Time;
        public string[] Outputs;
        public string[] Inputs;



        public SplittedLine()
        {
            Time = String.Empty;
            Outputs = new string[0];
            Inputs = new string[0];
        }
    }
}