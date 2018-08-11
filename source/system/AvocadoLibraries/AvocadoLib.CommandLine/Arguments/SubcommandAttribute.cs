using System;

namespace AvocadoLib.CommandLine.Arguments
{
    [AttributeUsage(AttributeTargets.Method)]
    public class SubcommandAttribute : Attribute { }
}