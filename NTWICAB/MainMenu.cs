using System;
using System.Collections.Generic;
using System.Text;
using Spectre.Console;

namespace NTWICAB
{
    internal static class MainMenu
    {
        public static readonly MenuOption[] options = {
            new MenuOption { name = "📺 Tune in to a Remote Stream", call = () => {throw new NotImplementedException(); } },
            new MenuOption { name = "🎞️ Stream from File", call = () => { throw new NotImplementedException(); } },
            new MenuOption { name = "📡 Broadcast", call = () => { throw new NotImplementedException(); } },
            new MenuOption { name = "❎ Quit", call = () => { AnsiConsole.MarkupLine("See ya! 👋"); Environment.Exit(0); } }
        };

        public static void Show()
        {
            Console.Clear();
            InterfaceCommons.WriteBanner();
            var mainMenuPrompt = new SelectionPrompt<MenuOption>()
                .Title("What would you like to do? ✏️")
                .UseConverter(option => $"{option.name}")
                .AddChoices(options)
                .HighlightStyle(Style.Parse("black on chartreuse1"));
            mainMenuPrompt.DisabledStyle = Style.Parse("chartreuse1 dim");
            MenuOption result = AnsiConsole.Prompt(mainMenuPrompt);
            try
            {
                result.call.Invoke();
            }
            catch (Exception e)
            {
                AnsiConsole.WriteException(e);

            }
        }
    }
}
