﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace AvocadoUtilities.CommandLine
{
    public sealed class Arguments
    {
        IEnumerable<string> remainingArgs;

        public Arguments(IEnumerable<string> args)
        {
            remainingArgs = args;
        }

        public string PopNextArg()
        {
            var arg = remainingArgs.FirstOrDefault();
            remainingArgs = remainingArgs.Skip(1);
            return arg;
        }

        public T? PopNextArg<T>() where T : struct, IConvertible
        {
            // Retrieve the argument.
            var arg = PopNextArg();
            if (arg == null) return null;

            // Convert the element to the specified type.
            try
            {
                return (T)Convert.ChangeType(arg, typeof(T));
            }
            catch (FormatException) { return null; }
        }

        public IEnumerable<string> PopRemainingArgs()
        {
            var args = remainingArgs;
            remainingArgs = null;
            return args;
        }
    }
}