using System;
using System.Collections.Generic;
using System.Text;
using Spectre.Console;

namespace NTWICAB
{
    internal static class MainMenu
    {
        private static void InitializeFileStream(string path)
        {
            FileStream file = File.OpenRead(path);
            try
            {
                StreamViewer.Show(file);
            } catch (Exception e)
            {
                file.Close();
                throw;
            }
        }

        public static readonly MenuOption[] options = {
            new MenuOption { Name = "📺 Tune in to a Remote Stream", Call = () => {throw new NotImplementedException(); } },
            new MenuOption { Name = "🎞️ Stream from File", Call = () => { InitializeFileStream(FileSelect.Show()); } },
            new MenuOption { Name = "📡 Broadcast", Call = () => { throw new NotImplementedException(); } },
            new MenuOption { Name = "❎ Quit", Call = () => { AnsiConsole.MarkupLine("See ya! 👋"); Environment.Exit(0); } }
        };

        public static async void Show()
        {
            Console.Clear();
            Commons.WriteBanner();
            var mainMenuPrompt = new SelectionPrompt<MenuOption>()
                .Title("What would you like to do? ✏️")
                .UseConverter(option => $"{option.Name}")
                .AddChoices(options)
                .HighlightStyle(Style.Parse("black on chartreuse1"));
            mainMenuPrompt.DisabledStyle = Style.Parse("chartreuse1 dim");
            MenuOption result = AnsiConsole.Prompt(mainMenuPrompt);
            AnsiConsole.MarkupLine($"[dim chartreuse1]> {result.Name}[/]\n");
            try
            {
                result.Call.Invoke();
            }
            catch (Exception e)
            {
                if (e is NotImplementedException)
                {
                    AnsiConsole.Status()
                        .Spinner(Spinner.Known.Monkey)
                        .Start("Not implemented", ctx =>
                        {
                            Thread.Sleep(3000);
                        });
                }
                else
                {
                    AnsiConsole.WriteException(e);
                    Thread.Sleep(3000);
                }
            }
        }
    }
}
