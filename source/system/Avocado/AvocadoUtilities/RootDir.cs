using System;
using System.Diagnostics;
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
                    => Path.GetDirectoryName(getParentAssembly().Location);

                public static string MyAppDataPath
                {
                    get
                    {
                        var appDataPath = Path.Combine(MyAppPath, "data");
                        Directory.CreateDirectory(appDataPath);
                        return appDataPath;
                    }
                }

                static Assembly getParentAssembly()
                {
                    var thisAsm = Assembly.GetExecutingAssembly();
                    var frames = new StackTrace().GetFrames();
                    foreach (var frame in frames)
                    {
                        var frameAsm = frame.GetMethod().DeclaringType.Assembly;

                        // Ignore .NET framework assemblies.
                        if (frameAsm.FullName.Contains(
                            "PublicKeyToken=b77a5c561934e089")) continue;

                        if (frameAsm != thisAsm) return frameAsm;
                    }
                    throw new Exception(
                        "The callstack for method 'getParentAssembly' must originate from a different assembly.");
                }
            }

            public static class Sys
            {
                public static string Val => GetPath(Avocado.Val, typeof(Sys));
            }
        }
    }
}