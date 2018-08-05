using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace AvocadoServiceHost.Utilities
{
    enum ClientType
    {
        ThisMachine,
        LAN,
        External
    }

    static class ClientIdentifier
    {
        public static string GetIP()
        {
            var prop = OperationContext.Current.IncomingMessageProperties;
            var endpoint = prop[RemoteEndpointMessageProperty.Name];
            return ((RemoteEndpointMessageProperty)endpoint).Address;
        }

        public static ClientType GetClientType()
        {
            var clientIP = GetIP();
            if (clientIP == GetLocalIPAddress()) return ClientType.ThisMachine;

            var addr = IPAddress.Parse(clientIP);
            return isLanIP(addr) ? ClientType.LAN : ClientType.External;
        }

        static string GetLocalIPAddress()
        {
            return Dns.GetHostEntry(Dns.GetHostName()).AddressList
                .Single(ip => ip.AddressFamily == AddressFamily.InterNetwork)
                .ToString();
        }

        // attribution: 
        // - http://stackoverflow.com/questions/7232287/check-if-ip-is-in-lan-behind-firewalls-and-routers
        static bool isLanIP(IPAddress addr)
        {
            foreach (var i in NetworkInterface.GetAllNetworkInterfaces())
            {
                foreach (var iAddr in i.GetIPProperties().UnicastAddresses)
                {
                    if (iAddr.IPv4Mask == null) continue;
                    if (iAddr.Address.AddressFamily
                        != AddressFamily.InterNetwork)
                    {
                        continue;
                    }
                    if (!checkMask(iAddr.Address, iAddr.IPv4Mask, addr))
                    {
                        continue;
                    }

                    return true;
                }
            }
            return false;
        }

        // attribution: 
        // - http://stackoverflow.com/questions/7232287/check-if-ip-is-in-lan-behind-firewalls-and-routers
        static bool checkMask(IPAddress addr, IPAddress mask, IPAddress target)
        {
            if (mask == null) return false;

            var ba = addr.GetAddressBytes();
            var bm = mask.GetAddressBytes();
            var bb = target.GetAddressBytes();

            if (ba.Length != bm.Length || bm.Length != bb.Length) return false;

            for (var i = 0; i < ba.Length; i++)
            {
                var m = bm[i];

                var a = ba[i] & m;
                var b = bb[i] & m;

                if (a != b) return false;
            }

            return true;
        }
    }
}