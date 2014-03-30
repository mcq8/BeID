using System;

namespace Be.Mcq8.EidReader
{
    internal class Logger
    {
        public static void Log(String lines)
        {
            try
            {
                Console.WriteLine(lines);
                System.IO.StreamWriter file = new System.IO.StreamWriter("kaartlog.txt", true);
                file.WriteLine(lines);
                file.Close();
            }
            catch (Exception)
            {
            }
        }
    }
}
