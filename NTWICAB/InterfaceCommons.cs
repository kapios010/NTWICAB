using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Text;

namespace NTWICAB
{
    public struct MenuOption
    {
        public string name;
        public Action call;
    }

    internal static class InterfaceCommons
    {
        public static void WriteBanner()
        {
            var larry3d = FigletFont.Load(new MemoryStream(Resources.chunky));
            AnsiConsole.Write(new FigletText(larry3d, "NTWICAB") { Justification = Justify.Center, Color = Color.Chartreuse1 });
            AnsiConsole.Write(new Markup("[chartreuse1]Now That's What I Call A Broadcast[/]\n") { Justification = Justify.Center });
            AnsiConsole.Write(new Rule { Justification = Justify.Center, Style = Style.Parse("chartreuse1 dim") });
            AnsiConsole.WriteLine();
        }
    }
}
