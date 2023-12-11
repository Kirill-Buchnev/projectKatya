using Autodesk.Revit.UI;
using projectKatya.Revit.Shared;
using System;
using System.Windows.Media.Imaging;

namespace projectKatya
{
    public class Application : IExternalApplication
    {
        private const string PluginFormat = ".dll";
        private string PlaceHolders = "projectKatya";
        public Result OnStartup(UIControlledApplication application)
        {
            string pluginsFolder = new CommonViewService().GetPluginsFolder();
            var tabName = "projectKatya";
            application.CreateRibbonTab(tabName);
            var testRibbonPanel = application.CreateRibbonPanel(tabName, "Walls");

            var wallBoundariesBtn = CreateButton(
            panel: testRibbonPanel,
            name: "WallBoundaries",
            text: "Снимает границу помещений со стен отделки",
            assemblyPath: pluginsFolder + PlaceHolders + PluginFormat,
            fullExecuteClassName: PlaceHolders + ".Commands.WallBoundariesCommand",
            imageName: "Icons/wallBoundaries.ico");

            var commandInProgressBtn = CreateButton(
            panel: testRibbonPanel,
            name: "CommandInProgress",
            text: "Команда находится в разработке",
            assemblyPath: pluginsFolder + PlaceHolders + PluginFormat,
            fullExecuteClassName: PlaceHolders + ".Commands.InProgressCommand",
            imageName: "Icons/WallJoin.ico");

            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }

        private PushButton CreateButton(
            RibbonPanel panel,
            string name,
            string text,
            string assemblyPath,
            string fullExecuteClassName,
            string imageName = null,
            string longDescription = null)
        {
            PushButtonData button = new PushButtonData(
                name: name,
                text: name,
                assemblyName: assemblyPath,
                className: fullExecuteClassName);

            PushButton btn = panel.AddItem(button) as PushButton;
            btn.ToolTip = text;

            if (longDescription != null) btn.LongDescription = longDescription;

            if (imageName != null)
            {
                BitmapImage image = new BitmapImage(new Uri($"pack://application:,,,/projectKatya.Revit.Shared;component/Views/Resources/{imageName}"));
                btn.LargeImage = image;
            }
            return btn;
        }
    }
}
