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
            public static string Val
            {
                get { return GetPath(RootDir.Val, typeof(Avocado)); }
            }

            public static class Apps
            {
                public static string Val
                {
                    get { return GetPath(Avocado.Val, typeof(Apps)); }
                }

                public static string MyAppPath
                {
                    get
                    {
                        var filePath = Assembly.GetCallingAssembly().Location;
                        return Path.GetDirectoryName(filePath);
                    }
                }
            }

            public static class Sys
            {
                public static string Val
                {
                    get { return GetPath(Avocado.Val, typeof(Sys)); }
                }
            }
        }
    }
}