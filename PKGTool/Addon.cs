using HedgeLib.Archives;

namespace PKGTool
{
    public class Addon : HedgeArchiveEditor.Addon
    {
        public override bool OnLoad()
        {
            var archive = new ArchiveAddon();
            archive.ArchiveType = typeof(PKGArchive);
            archive.ArchiveName = "Trails of Cold Steel Archive";
            archive.FileExtensions.Add(".pkg");
            Archives.Add(archive);
            return true;
        }
    }
}
