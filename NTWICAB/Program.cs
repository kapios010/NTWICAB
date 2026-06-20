using Spectre.Console;
using System.Text;

namespace NTWICAB
{
    internal class Program
    {
        internal static void WriteBanner()
        {
            var larry3d = FigletFont.Load(new MemoryStream(Resources.chunky));
            AnsiConsole.Write(new FigletText(larry3d, "NTWICAB") { Justification = Justify.Center, Color = Color.Chartreuse1 });
            AnsiConsole.Write(new Markup("[chartreuse1]Now That's What I Call A Broadcast[/]\n") { Justification = Justify.Center });
            AnsiConsole.Write(new Rule { Justification = Justify.Center, Style = Style.Parse("chartreuse1 dim") });
            AnsiConsole.WriteLine();
        }

        public struct MainMenuOption
        {
            public string name;
            public Action call;
        }

        public static readonly MainMenuOption[] options = {
            new MainMenuOption { name = "📺 Tune in to a Remote Stream", call = () => {throw new NotImplementedException(); } },
            new MainMenuOption { name = "🎞️ Stream from File", call = () => { throw new NotImplementedException(); } },
            new MainMenuOption { name = "📡 Broadcast", call = () => { throw new NotImplementedException(); } },
            new MainMenuOption { name = "❎ Quit", call = () => { AnsiConsole.MarkupLine("See ya! 👋"); Environment.Exit(0); } }
        };

        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.Unicode;
            WriteBanner();
            var mainMenuPrompt = new SelectionPrompt<MainMenuOption>()
                .Title("What would you like to do? ✏️")
                .UseConverter(option => $"{option.name}")
                .AddChoices(options)
                .HighlightStyle(Style.Parse("black on chartreuse1"));
            mainMenuPrompt.DisabledStyle = Style.Parse("chartreuse1 dim");
            MainMenuOption result = AnsiConsole.Prompt(mainMenuPrompt);
            try
            {
                result.call.Invoke();
            } catch (Exception e)
            {
                AnsiConsole.WriteException(e);

            }

        }
    }
}
