using System;
using System.Runtime.Serialization;

namespace UtilityLib.WCF
{
    [DataContract]
    public sealed class WCFMessage
    {
        [DataMember]
        public bool Success { get; set; }

        [DataMember]
        public string Message { get; set; }
        
        public WCFMessage(bool success, string message)
        {
            Success = success;
            Message = message;
        }

        public void LogToConsole() => 
            (Success ? Console.Out : Console.Error).WriteLine(Message);
    }
}