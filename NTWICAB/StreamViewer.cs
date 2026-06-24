using Spectre.Console;
using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Text;
using static NTWICAB.ColorOperations;

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
            public required byte FP4S;
        }

        public enum StreamOperation
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
            BcstEndFrame = 0xA,
            BcstUpdateMeta = 0xB,
            BcstEnd = 0xF
        }

        public static StreamOperation GetOperation(ushort packet)
        {
            return (StreamOperation)(packet >> 12);
        }

        public static void DrawBlackBg(this Canvas canvas)
        {
            for (int x = 0; x < canvas.Width; x++)
                for (int y = 0; y < canvas.Height; y++)
                    canvas.SetPixel(x, y, Color.Black);
        }

        public static StreamMetadata GetStreamMetadata(Stream stream)
        {
            int nameLength = stream.ReadByte();
            if (nameLength == 0)
                throw new ArgumentOutOfRangeException("Name field can't have length of 0.");
            byte[] nameBuffer = new byte[nameLength];
            stream.ReadExactly(nameBuffer, 0, nameLength);
            string Name = Encoding.Unicode.GetString(nameBuffer).Trim();
            int authorLength = stream.ReadByte();
            if (authorLength == 0)
                throw new ArgumentOutOfRangeException("Author field can't have length of 0.");
            byte[] authorBuffer = new byte[authorLength];
            stream.ReadExactly(authorBuffer, 0, authorLength);
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
            if (numericBuffer.Any(val => val == 0))
                throw new ArgumentOutOfRangeException("Width, Height, and fp4s can't be equal to 0.");
            return new StreamMetadata()
            {
                Name = Name, Author = Author, BroadcasterIdent = BroadcasterIdent,
                Width = numericBuffer[0], Height = numericBuffer[1], FP4S = numericBuffer[2]
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

        private enum FooterMode
        {
            Normal,
            Loading,
            Ended
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

            var infoPanel = new Panel($"[bold]{metadata.Name.EscapeMarkup()}[/]\n{metadata.Author.EscapeMarkup()}\n\n{metadata.Width}x{metadata.Height}px @ {metadata.FP4S}fp4s").Expand();
            layout["Title"].Update(infoPanel);

            var identPanel = new Panel(metadata.BroadcasterIdent.ToFormattedBcstIdent(16).EscapeMarkup());
            layout["BcastIdent"].Update(identPanel);

            FooterMode footerMode = FooterMode.Normal;
            layout["Footer"].Update(new Markup("Press [aqua]Esc[/] to quit watching."));

            var canvas = new Canvas(metadata.Width, metadata.Height);
            canvas.DrawBlackBg();
            layout["Body"].Update(Align.Center(canvas, VerticalAlignment.Middle));

            TimeSpan oneFrame = new TimeSpan(0, 0, 4).Divide(metadata.FP4S);

            AnsiConsole.Clear();
            AnsiConsole.Live(layout)
                .AutoClear(true)
                .Start(ctx =>
                {
                    ctx.Refresh();
                    byte[] buffer = new byte[2];
                    ushort processedBuffer = 0;
                    Color cursor = new Color(0, 0, 0);
                    (int x, int y) cursorPosition = (0, 0);
                    DateTime delta1 = DateTime.Now;
                    DateTime delta2 = DateTime.Now;
                    TimeSpan delta = new(0,0,5);
                    while (true)
                    {
                        if (Console.KeyAvailable && Console.ReadKey(true).Key == ConsoleKey.Escape)
                            break;
                        if (oneFrame.Subtract(delta).Ticks > 0)
                        {
                            delta2 = DateTime.Now;
                            delta = new(delta2.Ticks - delta1.Ticks);
                            continue;
                        }
                        delta = new(0,0,5);
                        delta1 = delta2;
                        try { 
                            stream.ReadExactly(buffer, 0, 2);
                            if (footerMode != FooterMode.Normal)
                            {
                                footerMode = FooterMode.Normal;
                                layout["Footer"].Update(new Markup("Press [aqua]Esc[/] to quit watching."));
                                ctx.Refresh();
                            }
                        } catch {
                            if (footerMode != FooterMode.Loading)
                            {
                                footerMode = FooterMode.Loading;
                                layout["Footer"].Update(new Markup("[black on chartreuse1] Loading... [/] Press [aqua]Esc[/] to quit watching."));
                                ctx.Refresh();
                            }
                            ctx.Refresh();
                            continue;
                        }
                        processedBuffer = BinaryPrimitives.ReadUInt16BigEndian(buffer);
                        StreamOperation operation = GetOperation(processedBuffer);
                        if (operation == StreamOperation.Hold || operation.IsRGBOperation())
                        {
                            if (operation.IsRGBOperation())
                                cursor = cursor.Modify(GetColorValues(processedBuffer), operation.GetSubtractTuple());
                            canvas.SetPixel(cursorPosition.x, cursorPosition.y, cursor);
                            cursorPosition.x++;
                            if (cursorPosition.x == canvas.Width)
                            {
                                cursorPosition.x = 0;
                                if (cursorPosition.y == canvas.Height - 1)
                                {
                                    cursorPosition.y = 0; ctx.Refresh();
                                }
                                else
                                    cursorPosition.y++;
                            }
                        }
                        else if (operation == StreamOperation.BcstEndFrame)
                        {
                            ctx.Refresh();
                            cursorPosition = (0, 0);
                            delta2 = DateTime.Now;
                            delta = new(delta2.Ticks - delta1.Ticks);
                        }
                        else if (operation == StreamOperation.BcstUpdateMeta)
                        {
                            metadata = GetStreamMetadata(stream);
                            layout["Title"].Update(new Panel($"[bold]{metadata.Name.EscapeMarkup()}[/]\n{metadata.Author.EscapeMarkup()}\n\n{metadata.Width}x{metadata.Height}px @ {metadata.FP4S}fp4s").Expand());
                            layout["BcastIdent"].Update(new Panel(metadata.BroadcasterIdent.ToFormattedBcstIdent(16).EscapeMarkup()));
                            var canvas = new Canvas(metadata.Width, metadata.Height);
                            canvas.DrawBlackBg();
                            layout["Body"].Update(Align.Center(canvas, VerticalAlignment.Middle));
                            cursor = new Color(0, 0, 0);
                            
                            ctx.Refresh();
                        }
                        else if (operation == StreamOperation.BcstEnd)
                            break;
                    }
                    footerMode = FooterMode.Ended;
                    layout["Footer"].Update(new Text("Stream Ended. Press any key to exit..."));
                    ctx.Refresh();
                    stream.Close();
                    Console.ReadKey(true);
                });
        }
    }
}
