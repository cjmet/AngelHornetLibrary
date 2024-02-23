using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngelHornetLibrary
{
    public static class AhStrings
    {

        public static string HeadTruncate(string input, int fixedLength = 0)
        {
            var length = Console.BufferWidth - 1;
            if (fixedLength > 0) length = fixedLength;
            if (input.Length <= length) return input;
            return " ... " + input.Substring(input.Length - length + 5);    
        }



        public static string MiddleTruncate(string input, int fixedLength = 0)
        {
            var length = Console.BufferWidth - 1;
            if (fixedLength > 0) length = fixedLength;
            if (input.Length <= length) return input;
            return input.Substring(0, length / 2 - 3) + " ... " + input.Substring(input.Length - length / 2 + 2);
        }



        public static string TailTruncate(string input, int fixedLength = 0)
        {
            var length = Console.BufferWidth - 1;
            if (fixedLength > 0) length = fixedLength;
            if (input.Length <= length) return input;
            return input.Substring(0, length - 5) + " ... ";
        }

    }
}
