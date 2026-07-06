using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Core.Application.Helper
{
   
    public static class EmbeddedSqlHelper
    {
        //private const string RootNamespace = "YourApp.Persistence.SqlScripts"; // فضای نام پروژه خود را جایگزین کنید

        public static string Read(string RootNamespace , string fileName)
        {
            var resourceName = $"{RootNamespace}.{fileName}";
            var assembly = Assembly.GetExecutingAssembly();

            using var stream = assembly.GetManifestResourceStream(resourceName);
            if (stream == null)
                throw new FileNotFoundException($"Embedded SQL script not found: {resourceName}");

            using var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }
    }
}
