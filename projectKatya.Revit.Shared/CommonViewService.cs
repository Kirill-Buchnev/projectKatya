using System.IO;
using System.Reflection;

namespace projectKatya.Revit.Shared
{
    public class CommonViewService
    {
        public string GetPluginsFolder()
        {
            string codeBase = Assembly.GetExecutingAssembly().CodeBase;
            System.UriBuilder uri = new System.UriBuilder(codeBase);
            string path = System.Uri.UnescapeDataString(uri.Path);
            string executeFolderPath = Path.GetDirectoryName(path);

            string pluginsFolder = $"{executeFolderPath}\\";

            return pluginsFolder;
        }
    }
}
