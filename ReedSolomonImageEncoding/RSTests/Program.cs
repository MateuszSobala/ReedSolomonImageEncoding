using System;

namespace RSTests
{
    public class Program
    {
        static void Main(string[] args)
        {
            var option = 0;
            while (option < 1 || option > 4)
            {
                var optionStr = Console.ReadLine();
                try
                {
                    option = int.Parse(optionStr);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Podaj liczbę całkowitą z zakresu 1-4!");
                }
            }
            IRsTests rsTests;
            switch (option)
            {
                case 1:
                    rsTests = new TimeTests();
                    break;
                case 2:
                    rsTests = new OptimalTests();
                    break;
                case 3:
                    rsTests = new ErrorDistributionTests();
                    break;
                case 4:
                    rsTests = new DataDiversityTests();
                    break;
                default:
                    rsTests = new TimeTests();
                    break;
            }

            rsTests.Initialize();
            rsTests.Perform();
            rsTests.SaveResults();
        }
    }
}
