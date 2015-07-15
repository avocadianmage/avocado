using System;

namespace AvocadoUtilities.CommandLine
{
    [AttributeUsage(AttributeTargets.Method)]
    public class SubcommandAttribute : Attribute { }
}