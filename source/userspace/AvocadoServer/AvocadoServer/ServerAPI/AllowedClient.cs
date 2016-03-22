using AvocadoServer.ServerCore;
using System;

namespace AvocadoServer.ServerAPI
{
    [AttributeUsage(AttributeTargets.Method)]
    sealed class AllowedClientAttribute : Attribute
    {
        public ClientType Client { get; }

        public AllowedClientAttribute(ClientType client)
        {
            Client = client;
        }
    }
}