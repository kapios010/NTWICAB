using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Text;

namespace NTWICAB
{
    internal static class StreamViewer
    {
        public struct StreamMetadata
        {
            public required string Name;
            public required string Author;
            public required string BroadcasterIdent;
            public required byte Width;
            public required byte Height;
            public required byte FPS;
        }

        public enum StreamOperations
        {
            Hold = 0x0,
            Add = 0x1,
            Subtract = 0x2,
            OperatorPPM = 0x3,
            OperatorPMP = 0x4,
            OperatorMPP = 0x5,
            OperatorMMP = 0x6,
            OperatorMPM = 0x7,
            OperatorPMM = 0x8,
            BcstNewFrame = 0xA,
            BcstUpdateMeta = 0xB,

        }

        public static void DrawBlackBg(this Canvas canvas)
        {
            for (int x = 0; x < canvas.Width; x++)
                for (int y = 0; y < canvas.Height; y++)
                    canvas.SetPixel(x, y, Color.Black);
        }

        public static StreamMetadata GetStreamMetadata(Stream stream)
        {
            byte[] nameBuffer = new byte[2 * 64];
            stream.ReadExactly(nameBuffer, 0, 2 * 64);
            string Name = Encoding.Unicode.GetString(nameBuffer).Trim();
            byte[] authorBuffer = new byte[2 * 64];
            stream.ReadExactly(authorBuffer, 0, 2 * 64);
            string Author = Encoding.Unicode.GetString(authorBuffer).Trim();
            string BroadcasterIdent = "░░░░░░░░░░░░░░░░░ NO BCST ░░░░░░░ IDENTIFIER ░░░░░░░░░░░░░░░░░░░";
            byte[] bcastIdentPresentBuffer = new byte[1];
            stream.ReadExactly(bcastIdentPresentBuffer, 0, 1);
            if (bcastIdentPresentBuffer[0] == 0b10000000)
            {
                byte[] bcasterIdentBuffer = new byte[2 * 4 * 16];
                stream.ReadExactly(bcasterIdentBuffer, 0, 2 * 4 * 16);
                BroadcasterIdent = Encoding.Unicode.GetString(bcasterIdentBuffer);
            }
            byte[] numericBuffer = new byte[3];
            stream.ReadExactly(numericBuffer, 0, 3);
            return new StreamMetadata()
            {
                Name = Name, Author = Author, BroadcasterIdent = BroadcasterIdent,
                Width = numericBuffer[0], Height = numericBuffer[1], FPS = numericBuffer[2]
            };
        }

        private static string ToFormattedBcstIdent(this string s, int length)
        {
            for(int i = length ; i < s.Length; i += length+1)
            {
                s = s.Insert(i, "\n");
            }
            return s;
        }

        public static void Show(Stream stream)
        {
            byte[] validSignature = { 0x4E, 0x54, 0x57, 0x49, 0x43, 0x41, 0x42, 0x00 };
            byte[] signatureBuffer = new byte[8];
            stream.ReadExactly(signatureBuffer, 0, 8);
            if (!signatureBuffer.SequenceEqual(validSignature))
                throw new FormatException("Invalid stream signature.");

            StreamMetadata metadata = GetStreamMetadata(stream);

            var layout = new Layout("Root")
                .SplitRows(
                    new Layout("Header")
                    .SplitColumns(
                        new Layout("Title"),
                        new Layout("BcastIdent").Size(20)
                     ).Size(6),
                    new Layout("Body"),
                    new Layout("Footer").Size(1)
                );

            var infoPanel = new Panel($"[bold]{metadata.Name.EscapeMarkup()}[/]\n{metadata.Author.EscapeMarkup()}\n\n{metadata.Width}x{metadata.Height}px @ {metadata.FPS}FPS").Expand();
            layout["Title"].Update(infoPanel);

            var identPanel = new Panel(metadata.BroadcasterIdent.ToFormattedBcstIdent(16).EscapeMarkup());
            layout["BcastIdent"].Update(identPanel);

            layout["Footer"].Update(new Markup("Press [aqua]Esc[/] to quit watching."));

            var canvas = new Canvas(metadata.Width, metadata.Height);
            canvas.Scale = false;
            canvas.DrawBlackBg();
            layout["Body"].Update(Align.Center(canvas, VerticalAlignment.Middle));

            AnsiConsole.Clear();
            AnsiConsole.Live(layout)
                .AutoClear(true)
                .Start(ctx =>
                {
                    ctx.Refresh();
                    Thread.Sleep(5000);
                    byte[] buffer = new byte[2];
                    while (true)
                    {
                        try
                        {
                            stream.ReadExactly(buffer, 0, 2);
                        }
                        catch { break; }
                    }
                    
                    layout["Footer"].Update(new Markup("Stream ended. Press any key to exit."));
                    ctx.Refresh();
                });
            stream.Close();
            Commons.WriteBanner();
            AnsiConsole.MarkupLine($"Stream [bold aqua]{metadata.Name.EscapeMarkup()}[/] ended.");
            AnsiConsole.MarkupLine("\nPress any key to return to main menu.");
            Console.ReadKey(true);
        }
    }
}
