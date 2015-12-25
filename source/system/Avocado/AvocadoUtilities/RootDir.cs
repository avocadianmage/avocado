using System;
using System.IO;
using System.Reflection;

namespace AvocadoUtilities
{
    public static class RootDir
    {
        static string GetPath(string parentPath, Type type)
        {
            var name = type.Name.ToLower();
            return Path.Combine(parentPath, name);
        }

        public static string Val
        {
            get 
            {
                var folder = Environment.SpecialFolder.UserProfile;
                return Environment.GetFolderPath(folder);
            }
        }

        public static class Avocado
        {
            public static string Val => GetPath(RootDir.Val, typeof(Avocado));

            public static class Apps
            {
                public static string Val => GetPath(Avocado.Val, typeof(Apps));

                public static string MyAppPath
                {
                    get
                    {
                        var location = Assembly.GetEntryAssembly().Location;
                        return Path.GetDirectoryName(location);
                    }
                }

                public static string MyAppDataPath
                {
                    get
                    {
                        var appDataPath = Path.Combine(MyAppPath, "data");
                        Directory.CreateDirectory(appDataPath);
                        return appDataPath;
                    }
                }
            }

            public static class Sys
            {
                public static string Val => GetPath(Avocado.Val, typeof(Sys));
            }
        }
    }
}