using System;
using System.Collections.Generic;
using System.Text;
using Spectre.Console;

namespace NTWICAB
{
    internal static class FileSelect
    {
        /// <summary>
        /// Shows the file select prompt.
        /// </summary>
        /// <returns>Path to the file selected by the user.</returns>
        public static string Show()
        {
            var prompt = new TextPrompt<string>("Please enter file location: ")
                .PromptStyle(Style.Parse("darkturquoise"))
                .Validate(input => {

                    if (!File.Exists(input))
                        return ValidationResult.Error("[red]Specified path does not exist.[/]");

                    string ext = Path.GetExtension(input);

                    if (ext != ".ntwicab")
                        return ValidationResult.Error($"[red]Incorrect file extension [darkorange]{ext}[/] is not [darkorange].ntwicab[/][/]");

                    return ValidationResult.Success();
                });
            return AnsiConsole.Prompt(prompt);
        }
    }
}
