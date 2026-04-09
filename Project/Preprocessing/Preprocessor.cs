namespace Project.Preprocessing;

using System.Text.RegularExpressions;

static class Preprocessor
{
    public static string[] PruneFileNames(string DataPath)
    {
        string[] filenames = GetFileNames(DataPath);
        for (int i = 0; i < filenames.Length; i++)
        {
            // Prune each file names for invalid characters. 
            string prunedFileName = RemoveInvalidChars(filenames[i]);
            // simply replace each filename with pruned version
            filenames[i] = prunedFileName;
        }
        return filenames;
    }
    public static string[] GetFileNames(string DataPath)
    {
        // array of downloaded .IO files paths
        string[] FileNames = Directory.GetFiles(DataPath);
        for (int i = 0; i < FileNames.Length; i++)
        {
            // get the Filename without their paths and ".<extension>" ending. 
            string newFileName = Path.GetFileName(FileNames[i])[..^3];
            FileNames[i] = newFileName;
        }
        // after changeing every index value, we return the array but with the paths pruned from the names of the files. 
        return FileNames;
    }
    // Prune and create the expected .ldr file name which the model.ldr should be changed into. 
    private static string RemoveInvalidChars(string FileName)
    {
        // In addition to invalid filename chars, we also need to prune #$@, to simplify names
        string invalidCharacters = new(Path.GetInvalidFileNameChars());

        string invalidCharacterPattern = $"[{Regex.Escape(invalidCharacters)}]";

        string fullFileName = Regex.Replace(FileName, invalidCharacterPattern, "").Trim();
        return fullFileName;
    }



    public static void UnzipFiles(string absDataPath, string absDestinationPath)
    {
        // Get all file paths for all the downloaded files.  
        string[] filePaths = Directory.GetFiles(absDataPath);
        // prune names for invalid characters and add desired .ldr extension for all filenames. Return list of pruned names for renameing unziped entries.  
        string[] newFileNames = PruneFileNames(absDataPath);

        // Extract all model.ldr zip entry files in ZipFilePaths, then move and rename them to expectedFile names. 
        Unzipper.UnzipAndMove(filePaths, newFileNames, absDestinationPath, "soho0909");
    }

    public static void ChangeSetNames(string FilePath)
    {
        string[] FilePaths = GetFileNames(FilePath);
        // 1 open each file 

        // 2 check string value of the name field 

        // 3 change name if the actual name is not the expected name. 
            // What is the expected name? 
            // What if the actual file name is not the expected name? 
    }

    public static void Preprocess()
    {
        bool unzip = false;
        bool changeSetNames = false;

        string sourcePath = @"..\..\..\LEGO_Data\TestDataFolder";
        string destinationPath = @"..\..\..\LEGO_Data\TestDataFolder\Model_Files";
        string absDataPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, sourcePath));
        string absDestinationPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, destinationPath));
        if (unzip)
        {
            // Unzip and move files to expected location with expected names.
            UnzipFiles(absDataPath, absDestinationPath);
        }

        if (changeSetNames)
        {
            // 
            ChangeSetNames(absDestinationPath); 
        }


    }


}