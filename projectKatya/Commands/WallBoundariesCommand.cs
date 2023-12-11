using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Linq;

namespace projectKatya.Commands
{
    [Regeneration(RegenerationOption.Manual)]
    [Transaction(TransactionMode.Manual)]
    public class WallBoundariesCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            var uiDoc = commandData.Application.ActiveUIDocument;
            var doc = uiDoc.Document;
            var activeView = doc.ActiveView;
            var finishingWalls = new FilteredElementCollector(doc, activeView.Id)
                                .OfCategory(BuiltInCategory.OST_Walls)
                                .WhereElementIsNotElementType().ToElements().Cast<Wall>()
                                .Where(wall => wall.WallType?.get_Parameter(BuiltInParameter.WALL_ATTR_WIDTH_PARAM)?.AsDouble() * 304.8 <= 40 
                                && wall.WallType?.get_Parameter(BuiltInParameter.WALL_ATTR_WIDTH_PARAM)?.AsDouble() * 304.8 != 0);
            if (finishingWalls.ToList().Count() != 0)
            {
                using (var t = new Transaction(doc, "Снятие границ отделки"))
                {
                    t.Start();
                    foreach (var wall in finishingWalls)
                    {
                        wall.get_Parameter(BuiltInParameter.WALL_ATTR_ROOM_BOUNDING).Set(0);
                    }
                    t.Commit();
                }
                TaskDialog.Show("Wall Boundaries", "Параметр Граница помещения отключен!");
            }
            else
            {
                TaskDialog.Show("Wall Boundaries", "На данном виде отсутствуют стены отделки!");
            }
            return Result.Succeeded;
        }
    }
}
