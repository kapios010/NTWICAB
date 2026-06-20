using Spectre.Console;
using System.Text;

namespace NTWICAB
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var larry3d = FigletFont.Load(new MemoryStream(Resources.chunky));
            AnsiConsole.Write(new FigletText(larry3d, "NTWICAB") { Justification = Justify.Center, Color = Color.Chartreuse1 });
            AnsiConsole.Write(new Markup("[chartreuse1]Now That's What I Call A Broadcast[/]\n") { Justification = Justify.Center});
            AnsiConsole.Write(new Rule{ Justification = Justify.Center, Style = Style.Parse("chartreuse1 dim")});
        }
    }
}
