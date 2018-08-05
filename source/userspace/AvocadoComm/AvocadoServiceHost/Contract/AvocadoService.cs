using AvocadoServiceHost.Utilities;

namespace AvocadoServiceHost.Contract
{
    public sealed class AvocadoService : IAvocadoService
    {
        public bool Ping()
        {
            logActionRequest(nameof(Ping));
            return true;
        }

        void logActionRequest(string action)
        {
            ClientIdentifier.GetClientInfo(out string ip, out ClientType type);
            Logging.WriteLine(new[] {
                (ip, (ColorType)type),
                (" requested server action ", ColorType.None),
                (action, ColorType.KeyPhrase),
                (".", ColorType.None)
            });
        }
    }
}
