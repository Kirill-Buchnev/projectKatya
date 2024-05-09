using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace projectKatya.Revit.Shared.Services
{
    public class FamilyLoadService
    {
        Document _document;
        private CommonViewService _viewService;
        private IEnumerable<string> _rfas;
        private Assembly _assemblyWithRfas;
        private string _assemblyName;

        public IEnumerable<string> NecessaryRFAs { get; private set; }

        public FamilyLoadService(Document doc)
        {
            _document = doc;
            _viewService = new CommonViewService();

            _assemblyWithRfas = Assembly.LoadFrom(Path.Combine(_viewService.GetPluginsFolder(), "projectKatya.Revit.Shared.dll"));
            _rfas = _assemblyWithRfas.GetManifestResourceNames();

            _assemblyName = _assemblyWithRfas.GetName().Name;
            NecessaryRFAs = _rfas.Select(x => GetRFANameFromResource(x));
        }

        private string GetRFANameFromResource(string resourceRFA)
        {
            return resourceRFA.Replace($"{_assemblyName}.Families.", string.Empty);
        }

         public void LoadAllInstanceFamilies()
        {
            LoadFamilies();
        }

        public void LoadSelectedInstanceFamilies(List<string> familiesInstance)
        {
            LoadFamilies(familiesInstance);
        }
        public void LoadSelectedInstanceFamilies(string familiesInstance)
        {
            LoadFamilies(new List<string>() { familiesInstance });
        }

        private void LoadFamilies(List<string> familiesInstance)
        {
            string tempFolder = Path.GetTempPath();
            //copy rfa files to temp folder
            IEnumerable<string> copiedFiles = CopyFilesToFolder(tempFolder, _assemblyWithRfas, _rfas);

            //load rfas to project
            using (Transaction loadTr = new Transaction(_document, "Загрузка семейства"))
            {
                loadTr.Start();

                foreach (string familyName in familiesInstance)
                {
                    foreach (string rfaPath in copiedFiles)
                    {

                        if (rfaPath.Contains(familyName))
                        {
                            Family family;
                            _document.LoadFamily(rfaPath, new FamilyOption(), out family);
                            _document.Regenerate();
                            (_document?.GetElement(family.GetFamilySymbolIds().FirstOrDefault()) as FamilySymbol).Activate();
                            break;
                        }
                    }
                }

                loadTr.Commit();
            }

            RemoveFilesFromFolder(copiedFiles);
        }

        private void LoadFamilies()
        {
            string tempFolder = Path.GetTempPath();
            //copy rfa files to temp folder
            IEnumerable<string> copiedFiles = CopyFilesToFolder(tempFolder, _assemblyWithRfas, _rfas);

            //load rfas to project
            using (Transaction loadTr = new Transaction(_document, "Загрузка семейства"))
            {
                loadTr.Start();
                foreach (string rfaPath in copiedFiles)
                {
                    Family family;
                    _document.LoadFamily(rfaPath, new FamilyOption(), out family);
                }
                _document.Regenerate();

                loadTr.Commit();
            }

            RemoveFilesFromFolder(copiedFiles);
        }

        private IEnumerable<string> CopyFilesToFolder(string folderPath, Assembly assembly, IEnumerable<string> rfas)
        {
            List<string> result = new List<string>();
            foreach (string totalRfaName in rfas)
            {
                string rfaName = GetRFANameFromResource(totalRfaName);
                string outName = folderPath + "/" + rfaName;

                using (Stream input = assembly.GetManifestResourceStream(totalRfaName))
                {
                    using (Stream output = File.Create(outName))
                    {
                        try
                        {
                            CopyStream(input, output);
                            result.Add(outName);
                        }

                        finally
                        {
                            output.Close();
                        }
                    }
                    input.Close();
                }
            }
            return result;
        }

        internal class FamilyOption : IFamilyLoadOptions
        {
            public bool OnFamilyFound(bool familyInUse, out bool overwriteParameterValues)
            {
                overwriteParameterValues = true;
                return true;
            }

            public bool OnSharedFamilyFound(Family sharedFamily, bool familyInUse, out FamilySource source, out bool overwriteParameterValues)
            {
                source = FamilySource.Family;
                overwriteParameterValues = true;
                return true;
            }
        }

        private void CopyStream(Stream input, Stream output)
        {
            // Insert null checking here for production
            byte[] buffer = new byte[8192];

            int bytesRead;
            while ((bytesRead = input.Read(buffer, 0, buffer.Length)) > 0)
            {
                output.Write(buffer, 0, bytesRead);
            }
        }

        private void RemoveFilesFromFolder(IEnumerable<string> files)
        {
            foreach (string file in files)
            {
                try
                {
                    File.Delete(file);
                }
                catch
                {
                    continue;
                }
            }
        }


        public Document OpenDocumentFile(UIApplication uIApplication, string filePath, bool audit = false, bool detachFromCentral = false)
        {
            Application app = uIApplication.Application;

            //declare open options for user to pick to audit or not
            OpenOptions openOpts = new OpenOptions();
            openOpts.Audit = audit;
            if (detachFromCentral == false)
            {
                openOpts.DetachFromCentralOption = DetachFromCentralOption.DoNotDetach;
            }
            else
            {
                openOpts.DetachFromCentralOption = DetachFromCentralOption.DetachAndPreserveWorksets;
            }

            ModelPath modelPath = ModelPathUtils.ConvertUserVisiblePathToModelPath(filePath);

            return app.OpenDocumentFile(modelPath, openOpts);
        }
    }
}

