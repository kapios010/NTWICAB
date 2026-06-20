using Spectre.Console;
using System.Text;

namespace NTWICAB
{
    internal class Program
    {

        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.Unicode;
            MainMenu.Show();
        }
    }
}
