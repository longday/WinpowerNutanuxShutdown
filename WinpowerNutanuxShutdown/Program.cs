using System;
using WinpowerNutanuxShutdown.Infrastrucure;

namespace WinpowerNutanuxShutdown
{
    class Program
    {
        static void Main(string[] args)
        {
            var manager = new Manager();
            manager.Run();
            Console.WriteLine("End");
            Console.ReadKey();
        }
    }
}
