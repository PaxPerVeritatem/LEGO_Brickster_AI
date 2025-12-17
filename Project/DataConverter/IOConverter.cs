using System.Text.RegularExpressions;
using ICSharpCode.SharpZipLib.Zip;
static class IOConverter
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
            string newFileName =  Path.GetFileName(FileNames[i]).Substring(0,Path.GetFileName(FileNames[i]).Length-3);
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

    private static void UnzipAndMove(string[] ZipFilePaths, string[] NewFileNames, string ExtractToFolderPath)
    {   
        Console.WriteLine($"Expected amount of files to unzip: {ZipFilePaths.Length}"); 
        Console.WriteLine($"--------------------------------------------------------"); 
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
                string currentFileName = Path.GetFileName(ZipFilePaths[i]); 

                // if there is already a .ldr file with the same name in the Model_Files folder
                if (File.Exists(newFilePath))
                {
                    Console.WriteLine($"File by the name: {Path.GetFileName(newFilePath)}, already exsisted in the Models folder. Skipping unzipping.\n");
                    AlreadyExistedFileCounter ++; 
                }
                // if the current file's extension is not .io format
                else if (currentFileName.Substring(currentFileName.Length-3,3)!=".io")
                {
                    throw new FileFormatException($"The file: {currentFileName}, was not in '.io' format. Skipping unzip\n"); 
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
            catch (FileFormatException ex)
            {
                Console.WriteLine(ex.Message); 
                corruptedFilesCounter++; 
            }
            catch (FileNotFoundException)
            {
                corruptedFilesCounter++;
                continue;
            }
            
        }
        Console.WriteLine($"Actual amount of '.io' files unziped, renamed and moved: {SucessfullFilescounter}");
        Console.WriteLine($"{AlreadyExistedFileCounter} '.io' files already existed in the Model_Files folder, hence they were not unziped"); 
        Console.WriteLine($"{corruptedFilesCounter} '.io' files were corrupted, could not be unziped or had the wrong file extension");
    }


    public static void ConvertFiles()
    {
        string testDataPath = @"..\..\..\LEGO_Data\TestDataFolder";
        string unzipedDataPath = @"..\..\..\LEGO_Data\TestDataFolder\Model_Files";
        string absDownloadFolderPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, testDataPath));
        string absDestinationFolderPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, unzipedDataPath));


        // Get all file paths for all the downloaded files.  
        string[] filePaths = Directory.GetFiles(absDownloadFolderPath);
        // prune names for invalid characters and add desired .ldr extension for all filenames. Return list of pruned names for renameing unziped entries.  
        string[] newFileNames = PruneFileNames(absDownloadFolderPath);



        
        // Extract all model.ldr zip entry files in ZipFilePaths, then move and rename them to expectedFile names. 
        UnzipAndMove(filePaths, newFileNames, absDestinationFolderPath);
    }
}


