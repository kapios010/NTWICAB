using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Text;
using static NTWICAB.StreamViewer;

namespace NTWICAB
{
    internal static class ColorOperations
    {
        public static (byte r, byte g, byte b) GetColorValues(ushort packet)
        {
            return (
                (byte)(((packet & 0x0F00) >> 4) + ((packet & 0x0F00) >> 8)),
                (byte)((packet & 0x00F0) + ((packet & 0x00F0) >> 4)),
                (byte)(((packet & 0x000F) << 4) + (packet & 0x000F))
                );
        }

        public static Color Modify(this Color color, (byte r, byte g, byte b) modifier, (bool r, bool g, bool b) subtract)
        {
            return new Color(
                (byte)(color.R + (subtract.r ? -1 : 1) * modifier.r ),
                (byte)(color.G + (subtract.g ? -1 : 1) * modifier.g ),
                (byte)(color.B + (subtract.b ? -1 : 1) * modifier.b )
                );
        }

        public static bool IsRGBOperation(this StreamOperation so)
        {
            return (byte)so >= 0x1 && (byte)so <= 0x8;
        }

        public static (bool r, bool g, bool b) GetSubtractTuple(this StreamOperation so)
        {
            switch (so)
            {
                case StreamOperation.Add:
                    return (false, false, false);
                case StreamOperation.Subtract:
                    return (true, true, true);
                case StreamOperation.OperatorPPM:
                    return (false, false, true);
                case StreamOperation.OperatorPMP:
                    return (false, true, false);
                case StreamOperation.OperatorMPP:
                    return (true, false, false);
                case StreamOperation.OperatorMMP:
                    return (true, true, false);
                case StreamOperation.OperatorMPM:
                    return (true, false, true);
                case StreamOperation.OperatorPMM:
                    return (false, true, true);
                default:
                    throw new ArgumentOutOfRangeException("Only stream operations that operate on the color are allowed to have a subtract tuple.");
            }
        }
    }
}
