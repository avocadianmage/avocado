using System.Collections.Generic;
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
        ThisMachine = 3,
        LAN = 4,
        External = 5
    }

    static class ClientIdentifier
    {
        public static void GetClientInfo(out string ip, out ClientType type)
        {
            ip = getClientIP();
            type = GetLocalIPs().Contains(IPAddress.Parse(ip))
                ? ClientType.ThisMachine
                : isLanIP(IPAddress.Parse(ip))
                    ? ClientType.LAN : ClientType.External;
        }

        static string getClientIP()
        {
            var prop = OperationContext.Current.IncomingMessageProperties;
            var endpoint = prop[RemoteEndpointMessageProperty.Name];
            return ((RemoteEndpointMessageProperty)endpoint).Address;
        }

        static IEnumerable<IPAddress> GetLocalIPs()
        {
            return Dns.GetHostEntry(Dns.GetHostName()).AddressList
                .Where(ip => ip.AddressFamily == AddressFamily.InterNetwork);
        }

        // Attribution: 
        // http://stackoverflow.com/questions/7232287/check-if-ip-is-in-lan-behind-firewalls-and-routers
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

        // Attribution: 
        // http://stackoverflow.com/questions/7232287/check-if-ip-is-in-lan-behind-firewalls-and-routers
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