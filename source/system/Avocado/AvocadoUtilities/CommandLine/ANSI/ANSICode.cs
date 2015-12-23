using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Media;

namespace AvocadoUtilities.CommandLine.ANSI
{
    public static class ANSICode
    {
        const string ANSI_PREIX = "\x1b[";

        public static bool ContainsANSICodes(string str)
            => str.Contains(ANSI_PREIX);

        public static void WriteLine(SolidColorBrush brush, string line)
        {
            var color = brush.Color;
            Console.WriteLine(
                $"{ANSI_PREIX}38;2;{color.R};{color.G};{color.B}m{line}");
        }

        public static ICollection<ANSISegment> GetColorSegments(string line)
        {
            var ret = new List<ANSISegment>();

            // Split into separate segments for each ANSI sequence.
            var segments = line.Split(
                new string[] { ANSI_PREIX },
                StringSplitOptions.None);

            // If we have a non-empty first segment, then it did not have
            // an ANSI code prefix.
            var firstSeg = segments.First();
            if (!string.IsNullOrEmpty(firstSeg))
            {
                ret.Add(new ANSISegment(null, firstSeg));
            }

            // Any following segments are guaranteed to have ANSI codes 
            // prefixes.
            Brush brush = null;
            for (var i = 1; i < segments.Length; i++)
            {
                var seg = segments[i];
                var text = seg;

                // Find the index of the command. If it doesn't exist, the 
                // command is malformed, so just add the segment as regular 
                // text.
                var match = Regex.Match(seg, "[a-zA-Z]");
                if (match.Success)
                {
                    var commandIndex = match.Index;

                    // Retrieve the text to display.
                    text = seg.Substring(commandIndex + 1);

                    // Only parse the command if it is for Select Graphic 
                    // Rendition (the only one that is currently supported).
                    if (seg[commandIndex] == 'm')
                    {
                        var sequence = seg.Substring(0, commandIndex);
                        brush = codeSequenceToBrush(sequence);
                    }
                }

                // Add the text segment, if it is not empty.
                if (string.IsNullOrEmpty(text)) continue;
                ret.Add(new ANSISegment(brush, text));
            }

            return ret;
        }

        // Look for extended foreground color set sequence '38;2;{R};{G};{B}'.
        // This is the only thing we support.
        static Brush codeSequenceToBrush(string sequence)
        {
            // Loop through the semicolon-delimited list of codes.
            var codes = sequence.Split(';');
            for (var codeIdx = 0; codeIdx < codes.Length; codeIdx++)
            {
                // Quit if there are not enough pieces left to make a valid,
                // supported code.
                if (codeIdx + 4 >= codes.Length) return null;

                // Skip if the code is not supported (only code '38' is
                // supported).
                byte n;
                var foregroundRule = new Predicate<byte>(x => x == 38);
                if (!tryParsePiece(codes[codeIdx], out n, foregroundRule))
                { continue; }

                // Skip if malformed (the next piece must be '2').
                var rgbPrefixRule = new Predicate<byte>(x => x == 2);
                if (!tryParsePiece(codes[codeIdx + 1], out n, rgbPrefixRule))
                { continue; }

                // Parse out the RGB values.
                var rgb = new byte[3];
                var rgbIdx = 0;
                for (; rgbIdx < rgb.Length; rgbIdx++)
                {
                    if (!tryParsePiece(
                        codes[codeIdx + 2 + rgbIdx],
                        out rgb[rgbIdx]))
                    {
                        break;
                    }
                }

                // Skip if any of the RGB values were invalid.
                if (rgbIdx != 3) continue;

                // Convert the RGB to a Brush object and return.
                var i = 0;
                return rgbToBrush(rgb[i++], rgb[i++], rgb[i++]);
            }

            // Quit if no valid sequence was found.
            return null;
        }

        static Brush rgbToBrush(byte r, byte g, byte b)
            => new SolidColorBrush(Color.FromRgb(r, g, b));

        static bool tryParsePiece(
            string piece,
            out byte result,
            params Predicate<byte>[] rules)
        {
            // Fail if piece cannot be converted to an integer.
            if (!byte.TryParse(piece, out result)) return false;

            // Verify all rules pass.
            var temp = result;
            return rules.All(rule => rule(temp));
        }
    }
}