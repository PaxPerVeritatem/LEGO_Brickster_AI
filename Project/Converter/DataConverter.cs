using LEGO_Brickster_AI;
using System.IO.Compression;
using System.Text.RegularExpressions;


static class DataConverter
{
    private static string[] PruneFileNames(string AbsDownloadFolderPath, int EndIndex, string FileExtension)
    {
        string[] filenames = GetFileNames(AbsDownloadFolderPath, EndIndex);
        for (int i = 0; i < filenames.Length; i++)
        {
            // Prune each file names for invalid characters. 
            string prunedFileName = RemoveInvalidChars(filenames[i], FileExtension);
            // simply replace each filename with pruned version
            filenames[i] = prunedFileName;
        }
        return filenames;
    }


    private static string[] GetFileNames(string AbsDownloadFolderPath,int EndIndex)
    {
        // array of downloaded .IO files paths
        string[] FileNames = Directory.GetFiles(AbsDownloadFolderPath);
        for (int i = 0; i < FileNames.Length; i++)
        {
            // get the Filename without their paths and ".<extension>" ending. 
            string currentFileName = Path.GetFileName(FileNames[i]);
            currentFileName = currentFileName.Substring(0, currentFileName.Length - EndIndex);
            FileNames[i] = currentFileName;
        }
        // after changeing every index value, we return the array but with the paths pruned from the names of the files. 
        return FileNames;
    }


    // Prune and create the expected .ldr file name which the model.ldr should be changed into. 
    private static string RemoveInvalidChars(string FileName, string FileExtension)
    {
        // In addition to invalid filename chars, we also need to prune #$@, to simplify names
        string invalidCharacters = new string(Path.GetInvalidFileNameChars()) + "$#@";

        string invalidCharacterPattern = $"[{Regex.Escape(invalidCharacters)}]";

        string fullFileName = Regex.Replace(FileName, invalidCharacterPattern, "").Trim() + FileExtension;
        return fullFileName;
    }
 




    // Function to get all of the actual filenames, remove any unnessary characters
    private static void GetExpectedFileNames(string AbsDownloadFolderPath)
    {
        // array of downloaded .IO files
        string[] ioFileNames = Directory.GetFiles(AbsDownloadFolderPath);


    }

    public static void ConvertFiles()
    {
        //string mpdTestDataPath = @"..\..\..\LEGO_Data\BrickLink_Data\TestDataFolder\mpd";
        string ioTestDataPath = @"..\..\..\LEGO_Data\TestDataFolder\IO";
        int endIndex = 3;
        string fileExtension = ".ldr";
        string absDownloadFolderPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, ioTestDataPath));
        
        // prune names for invalid characters and add desired .ldr extension to all filenames 
        string[] expectedFileNames = PruneFileNames(absDownloadFolderPath,endIndex,fileExtension);
    }
}
// // change the name of the file 
// fileInfo.MoveTo(newFilePath);
// Console.WriteLine($"Filename: {currentFileName} was changed to: {NewFileName}\n");