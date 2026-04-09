namespace Project.Preprocessing;
using ICSharpCode.SharpZipLib.Zip;

static class Unzipper
{
    public static void UnzipAndMove(string[] ZipFilePaths, string[] NewFileNames, string ExtractToFolderPath, string ZipPassword)
    {
        int expectedFilesUnzipedCounter = ZipFilePaths.Length;
        int sucessfullFilescounter = 0;
        int alreadyExistedFileCounter = 0;
        int corruptedFilesCounter = 0;
        FastZip zipArchive = new()
        {
            Password = ZipPassword
        };

        {
            Console.WriteLine($"Attempting to unzip:{expectedFilesUnzipedCounter}");
            for (int i = 0; i < ZipFilePaths.Length; i++)
            {
                try
                {
                    string newFilePath = Path.Combine(ExtractToFolderPath, NewFileNames[i] + ".ldr");
                    string currentFileName = Path.GetFileName(ZipFilePaths[i]);

                    // if there is already a .ldr file with the same name in the Model_Files folder
                    if (File.Exists(newFilePath))
                    {
                        // {Path.GetFileName(newFilePath)}, already exsisted in the Models folder. Skipping unzipping.\n");
                        alreadyExistedFileCounter++;
                        expectedFilesUnzipedCounter--;
                    }
                    // if the current file's extension is not .io format
                    else if (currentFileName.Substring(currentFileName.Length - 3, 3) != ".io")
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
                        sucessfullFilescounter++;

                    }

                }
                catch (FileFormatException)
                {
                    //Console.WriteLine(ex.Message);
                    corruptedFilesCounter++;
                    expectedFilesUnzipedCounter--;
                    continue;
                }
                catch (FileNotFoundException)
                {
                    corruptedFilesCounter++;
                    expectedFilesUnzipedCounter--;
                    continue;
                }


            }
        }
        if (sucessfullFilescounter == expectedFilesUnzipedCounter)
        {
            Console.WriteLine(
            "Files Unzipped successfully\n" +
            $"--------------------------------------------------------\n" +
            $"Actual amount of '.io' files unziped, renamed and moved: {sucessfullFilescounter}\n" +
            $"{alreadyExistedFileCounter} '.io' files already existed in the Model_Files folder, hence they were not unziped\n" +
            $"{corruptedFilesCounter} '.io' files were corrupted or could not be unziped due to incorrect file extension");
        }
        else
        {
            Console.WriteLine(
            "Something went wrong while unzipping\n" +
             $"--------------------------------------------------------\n" +
            $"Actual amount of '.io' files unziped, renamed and moved: {sucessfullFilescounter}\n" +
            $"{alreadyExistedFileCounter} '.io' files already existed in the Model_Files folder, hence they were not unziped\n" +
            $"{corruptedFilesCounter} '.io' files were corrupted or could not be unziped due to incorrect file extension");
        }
    }
}


