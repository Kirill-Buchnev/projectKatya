using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using projectKatya.Revit.Shared.Services;
using System.Collections.Generic;
using System.Linq;

namespace projectKatya.Commands
{
    [Regeneration(RegenerationOption.Manual)]
    [Transaction(TransactionMode.Manual)]
    public class MoldingCommand : IExternalCommand
    {
        private string moldingName = "Молдинг_Профиль 40х19";
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uiDoc = commandData.Application.ActiveUIDocument;
            Document doc = uiDoc.Document;
            View activeView = doc.ActiveView;
            FamilyLoadService familyLoadService = new FamilyLoadService(doc);

            List<string> allFamilyNamesInDoc = new FilteredElementCollector(doc).WhereElementIsElementType().ToElements().Select(e => e.Name).ToList();

            if (!allFamilyNamesInDoc.Contains(moldingName))
            {
                familyLoadService.LoadSelectedInstanceFamilies(moldingName);
            }

            FamilySymbol moldingFamilySymbol = new FilteredElementCollector(doc)
               .OfClass(typeof(FamilySymbol))
            .Cast<FamilySymbol>()
               .FirstOrDefault(it => it.Name.Equals(moldingName));

            uiDoc?.PostRequestForElementTypePlacement(moldingFamilySymbol);

            return Result.Succeeded;
        }
    }
}
