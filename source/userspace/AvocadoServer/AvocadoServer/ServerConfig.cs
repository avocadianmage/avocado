namespace AvocadoServer
{
    static class ServerConfig
    {
        public static string BaseAddress
        {
            get
            {
                const int PORT = 8000;
                const string SUBDOMAIN = "AvocadoServer";
                const string URI_FORMAT = "http://localhost:{0}/{1}";
                return string.Format(URI_FORMAT, PORT, SUBDOMAIN);
            }
        }
    }
}