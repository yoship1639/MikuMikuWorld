using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MikuMikuWorld.Scripts
{
    class InitLoading : GameComponent
    {
        public static readonly string DataRootPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\mmw\data\";

        private List<ImportedObject> presetObjects = new List<ImportedObject>();
        private List<ImportedObject> presetCharacters = new List<ImportedObject>();
        private List<ImportedObject> presetStages = new List<ImportedObject>();

        private List<ImportedObject> freeObjects = new List<ImportedObject>();
        private List<ImportedObject> freeCharacters = new List<ImportedObject>();
        private List<ImportedObject> freeStages = new List<ImportedObject>();

        public ImportedObject[] PresetObjects { get { return presetObjects.ToArray(); } }
        public ImportedObject[] PresetCharacters { get { return presetCharacters.ToArray(); } }
        public ImportedObject[] PresetStages { get { return presetStages.ToArray(); } }

        public ImportedObject[] FreeObjects { get { return freeObjects.ToArray(); } }
        public ImportedObject[] FreeCharacters { get { return freeCharacters.ToArray(); } }
        public ImportedObject[] FreeStages { get { return freeStages.ToArray(); } }

        public string[] PresetObjectFiles { get; private set; }
        public string[] PresetCharacterFiles { get; private set; }
        public string[] PresetStageFiles { get; private set; }

        public string[] FreeObjectFiles { get; private set; }
        public string[] FreeCharacterFiles { get; private set; }
        public string[] FreeStageFiles { get; private set; }

        public int PresetObjectNum { get { if (PresetObjectFiles == null) return 0; return PresetObjectFiles.Length; } }
        public int PresetCharacterNum { get { if (PresetCharacterFiles == null) return 0; return PresetCharacterFiles.Length; } }
        public int PresetStageNum { get { if (PresetStageFiles == null) return 0; return PresetStageFiles.Length; } }

        public int FreeObjectNum { get { if (FreeObjectFiles == null) return 0; return FreeObjectFiles.Length; } }
        public int FreeCharacterNum { get { if (FreeCharacterFiles == null) return 0; return FreeCharacterFiles.Length; } }
        public int FreeStageNum { get { if (FreeStageFiles == null) return 0; return FreeStageFiles.Length; } }

        public LoadingState State { get; private set; } = LoadingState.BeforeLoading;
        public string NowLoadingFile { get; private set; }

        private Task task;

        public enum LoadingState
        {
            BeforeLoading,
            LoadNumCheck,
            PresetObject,
            PresetStage,
            PresetCharacter,
            FreeObject,
            FreeStage,
            FreeCharacter,
            Finished,
        }

        protected override void OnLoad()
        {
            task = Task.Factory.StartNew(() =>
            {
                Thread.Sleep(1000);

                // 読み込むファイルの列挙
                State = LoadingState.LoadNumCheck;

                PresetObjectFiles = GetFiles(DataRootPath + @"preset\object", new string[] { "*.mwo", "*.mwoe", "*.mqo", "*.pmd", "*.pmx", "*.vmd" });
                PresetCharacterFiles = GetFiles(DataRootPath + @"preset\character", new string[] { "*.mwc", "*.mwce", "*.mqo", "*.pmd", "*.pmx" });
                PresetStageFiles = GetFiles(DataRootPath + @"preset\stage", new string[] { "*.mws", "*.mwse", "*.mqo", "*.pmd", "*.pmx" });
                FreeObjectFiles = GetFiles(DataRootPath + @"free\object", new string[] { "*.mwo", "*.mwoe", "*.mqo", "*.pmd", "*.pmx", "*.vmd" });
                FreeCharacterFiles = GetFiles(DataRootPath + @"free\character", new string[] { "*.mwc", "*.mwce", "*.mqo", "*.pmd", "*.pmx" });
                FreeStageFiles = GetFiles(DataRootPath + @"free\stage", new string[] { "*.mws", "*.mwse", "*.mqo", "*.pmd", "*.pmx" });

                // Preset objectの読み込み
                State = LoadingState.PresetObject;
                LoadObjects(PresetObjectFiles, ref presetObjects);

                // Preset characterの読み込み
                State = LoadingState.PresetCharacter;
                LoadObjects(PresetCharacterFiles, ref presetCharacters);

                // Preset stageの読み込み
                State = LoadingState.PresetStage;
                LoadObjects(PresetStageFiles, ref presetStages);

                // Free objectの読み込み
                State = LoadingState.FreeObject;
                LoadObjects(FreeObjectFiles, ref freeObjects);

                // Free characterの読み込み
                State = LoadingState.FreeCharacter;
                LoadObjects(FreeCharacterFiles, ref freeCharacters);

                // Free stageの読み込み
                State = LoadingState.FreeStage;
                LoadObjects(FreeStageFiles, ref freeStages);

                State = LoadingState.Finished;
            });
        }

        private string[] GetFiles(string path, string[] patterns)
        {
            var list = new List<string>();
            foreach (var p in patterns)
            {
                try
                {
                    list.AddRange(Directory.GetFiles(path, p, SearchOption.AllDirectories));
                }
                catch { }
            }
            return list.ToArray();
        }

        private void LoadObjects(string[] files, ref List<ImportedObject> res)
        {
            if (files == null) return;

            foreach (var f in files)
            {
                NowLoadingFile = f;
                var importer = MMW.GetSupportedImporter(f);
                if (importer == null) continue;
                var imps = importer.Import(f, Importers.ImportType.OverviewOnly);

                // test
                if (importer is Importers.VmdImporter)
                    importer.Import(f, Importers.ImportType.Full);

                foreach (var i in imps)
                {
                    if (i.Result != Result.Success) continue;
                    if (string.IsNullOrWhiteSpace(i.Name)) i.Name = "No Name";
                    res.Add(i);
                }
                LoadCompleted(this, new LoadingEventArgs() { State = State });
            }
        }

        public event EventHandler<LoadingEventArgs> LoadCompleted = delegate { };

        public override GameComponent Clone()
        {
            throw new NotImplementedException();
        }
    }

    class LoadingEventArgs : EventArgs
    {
        public InitLoading.LoadingState State;
    }
}
