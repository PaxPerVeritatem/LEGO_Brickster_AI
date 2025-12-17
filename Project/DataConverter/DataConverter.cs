using System.Text.RegularExpressions;
using ICSharpCode.SharpZipLib.Zip;
static class DataConverter
{
    private static string[] PruneFileNames(string AbsDownloadFolderPath)
    {
        string[] filenames = GetFileNames(AbsDownloadFolderPath);
        for (int i = 0; i < filenames.Length; i++)
        {
            // Prune each file names for invalid characters. 
            string prunedFileName = RemoveInvalidChars(filenames[i]);
            // simply replace each filename with pruned version
            filenames[i] = prunedFileName;
        }
        return filenames;
    }


    private static string[] GetFileNames(string AbsDownloadFolderPath)
    {
        // array of downloaded .IO files paths
        string[] FileNames = Directory.GetFiles(AbsDownloadFolderPath);
        for (int i = 0; i < FileNames.Length; i++)
        {
            // get the Filename without their paths and ".<extension>" ending. 
            string currentFileName = Path.GetFileName(FileNames[i]);
            currentFileName = currentFileName.Substring(0, currentFileName.Length - 3);
            FileNames[i] = currentFileName;
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

    private static void UnzipAndMove(string[] ZipFilePaths, string[] NewFileNames, string ExtractToFolderPath)
    {
        int SucessfullFilescounter = 0;
        int AlreadyExistedFileCounter = 0; 
        int corruptedFilesCounter = 0;

        FastZip zipArchive = new()
        {
            Password = "soho0909"
        };
        for (int i = 0; i < ZipFilePaths.Length; i++)
        {
            try
            {
                string newFilePath = Path.Combine(ExtractToFolderPath, NewFileNames[i] + ".ldr");
                if (IsFileUnziped(newFilePath))
                {
                    AlreadyExistedFileCounter ++; 
                }
                else
                {
                    // Extract the current model.ldr from ZipFilePaths[i] to ExtractToFolderPath
                    zipArchive.ExtractZip(ZipFilePaths[i], ExtractToFolderPath, "model.ldr");

                    /* define the oldFilePath path string to the current unziped model.ldr file.
                    Then create a newFilePath from the ExtractToFolderPath with the currentModelFilePath*/
                    string oldFilePath = Path.Combine(ExtractToFolderPath, "model.ldr");
                    

                    // make FileInfo object from noldFilePath and change the file name to newFilePath
                    FileInfo currentModelFileInfo = new(oldFilePath);
                    currentModelFileInfo.MoveTo(newFilePath);
                    SucessfullFilescounter++;
                }

            }
            catch (FileNotFoundException)
            {
                corruptedFilesCounter++;
                continue;
            }
        }
        Console.WriteLine($"{SucessfullFilescounter} .io files were unziped, renamed and moved");
        Console.WriteLine($"{AlreadyExistedFileCounter} .io files already existed in the Model_Files folder, hence they were not unziped"); 
        Console.WriteLine($"{corruptedFilesCounter} .io files were corrupted or could not be unziped");
    }
    public static bool IsFileUnziped(string filePath)
    {

        if (File.Exists(filePath))
        {
            Console.WriteLine($"File by the name:{Path.GetFileName(filePath)} already exsisted in the Models folder. Skipping unzipping.\n");
            return true;
        }
        else
        {
            return false;
        }
    }



    public static void ConvertFiles()
    {
        // make hash map to store filePaths as keys and newFileNames as values. 
        Dictionary<string, string> filesMap = [];

        string testDataPath = @"..\..\..\LEGO_Data\TestDataFolder";
        string unzipedDataPath = @"..\..\..\LEGO_Data\TestDataFolder\Model_Files";
        string absDownloadFolderPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, testDataPath));
        string absDestinationFolderPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, unzipedDataPath));


        // Get all file paths for all the downloaded files.  
        string[] filePaths = Directory.GetFiles(absDownloadFolderPath);
        // prune names for invalid characters and add desired .ldr extension for all filenames. Return list of pruned names for renameing unziped entries.  
        string[] newFileNames = PruneFileNames(absDownloadFolderPath);



        // the default path for all extract model.ldr entries, before they get renamed
        // Extract all model.ldr zip entry files in ZipFilePaths, then move and rename them to expectedFile names. 
        UnzipAndMove(filePaths, newFileNames, absDestinationFolderPath);
    }
}


