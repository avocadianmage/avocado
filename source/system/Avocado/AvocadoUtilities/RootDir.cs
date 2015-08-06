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

                public static string MyAppPath => 
                    getAppPath(Assembly.GetCallingAssembly());

                public static string MyAppDataPath
                {
                    get
                    {
                        var appPath = getAppPath(Assembly.GetCallingAssembly());
                        var appDataPath = Path.Combine(appPath, "data");
                        Directory.CreateDirectory(appDataPath);
                        return appDataPath;
                    }
                }

                static string getAppPath(Assembly asm) => 
                    Path.GetDirectoryName(asm.Location);
            }

            public static class Sys
            {
                public static string Val => GetPath(Avocado.Val, typeof(Sys));
            }
        }
    }
}