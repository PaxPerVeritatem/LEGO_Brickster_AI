namespace Project.GetData;
using Project.SeleniumBot;  
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using System.Diagnostics;
using System.Text.RegularExpressions;
sealed class GetDataBrickLink : IGetData
{
    // Global run Properties
    public static string Url { get; set; } = "https://www.bricklink.com/v3/studio/design.page?tab=Staff-Picks";
    public static string DownloadFolderPath => @"..\..\..\LEGO_Data\BrickLink_Data";

    // For this implementation each page will always have 50 sets, so this can be null. 
    public static int? MaxPage => null;

    public static int PageLimit => 10;

    // Page always has 50 sets pr page, so this value only used for calculating ExpectedSetsScraped in this implementation. 
    public static int ExpectedSetsPrPage => 50;

    // so far there does not seems to be any 404 error for any sets, so this can  be 0 in this implementation for now. 
    public static int ExpectedSetClickDeviation => 0;


    public static int ExpectedSetsScraped => ExpectedSetsPrPage * PageLimit - ExpectedSetClickDeviation;

    public static int ExpectedSetClickAmount { get; set; } = ExpectedSetsScraped;

    public static int SetClickCounter { get; set; } = 0;

    public static int FilesDownloadedCounter { get; set; } = 0;

    public static int FilesAlreadyDownloadedCounter { get; set; } = 0;

    public static int FilesDownloadTimedOutCounter { get; set; } = 0;

    public static bool RunCompleted { get; set; } = false;

    // Custom run Properties 
    public static bool CustomRun => true;

    // Not nessesary for this implementation. 
    public static int? StartFromPage => null;

    // We use SubpageElementTuple in this implementation, so we dont need UrlPageVarient or StartFromPage . 
    public static string? UrlPageVarient { get; set; } = null;


    // Each subpage is just a IWebElement with an accompanying the ByMechanism to call FindElement during ConfigureCustomRun(). 
    public static (string ElementString, string ByMechanism)? SubpageElementTuple => ("//li[@data-ts-id='9']", "xp");


    public static bool UseSubpage => false;

    public static void ConfigureCustomRun(Bot bot)
    {
        if (UseSubpage)
        {
            /*find and click the subpage link text to access the subpage. 
            Additionally, we can use forgive operator, since we always manually set SubPageElementTuple.*/
            try
            {
                IWebElement? subPageElement = bot.WaitAndFind(SubpageElementTuple!.Value.ElementString, SubpageElementTuple!.Value.ByMechanism);
                Bot.ClickElement(subPageElement);
                Thread.Sleep(3000);
            }
            catch (BotTimeOutException)
            {
                Console.WriteLine($"Subpage link text was not found or was not present");
            }


        }
    }


    public static void AccessMainPage(Bot bot, Dictionary<string, string>? ElementCandidatesDict = null)
    {
        Actions actionBuilder = new(bot.Driver);
        try
        {
            bot.GoToWebPage(bot.Url);
        }
        catch (BotUrlException ex)
        {
            Console.WriteLine($"Failed to load webpage: {ex.Message}");
        }

        // find and click the ageGateElement. If its not there Throw exception and continue. 
        try
        {
            IWebElement ageGateElement = bot.WaitAndFind("//input[@class='blp-age-gate__input-field']", "xp");
            Bot.ClickElement(ageGateElement);
            actionBuilder.SendKeys("1");
            actionBuilder.SendKeys("9");
            actionBuilder.SendKeys("9");
            actionBuilder.SendKeys("4");
            actionBuilder.Perform();
        }
        catch (BotTimeOutException)
        {
            Console.WriteLine($"Age gate input field was not found or was not present. Continueing");
        }

        // find and press cookie button. If its not there, throw an exception and continue. 
        try
        {
            IWebElement cookieButton = bot.WaitAndFind("//button[@class='blp-cookie-notice__btn blp-cookie-notice__btn--reject']", "xp");
            Bot.ClickElement(cookieButton);
            actionBuilder.Click();
            actionBuilder.Perform();
        }
        catch (BotTimeOutException)
        {
            Console.WriteLine($"Cookie button was not found or was not present. Continueing\n----------------------------------------");
        }
    }

    public static void SetAttributeList(Bot bot, string CommonElementString, string CommonByMechanism, string IdentifierAttribute, IWebElement AncestorElement)
    {
        // reset the bot attribute list for next page of elements.
        bot.AttributeList.Clear();
        // Attempt to get the list of LEGO set names for the current main page
        bot.AttributeList = bot.WaitAndFindAll(CommonElementString, CommonByMechanism, IdentifierAttribute, AncestorElement);
    }


    /// <summary>
    /// For this implementation of GetFullFileName, the fileExtension is harcoded to '.io', since all 
    /// BrickLink files will be of this type. The only thing needed to be done is to get each Identifierattribute, 
    /// ,which will the set name for each LEGO set, and append '.io' to it. Finally return the full file name for comparison to 
    /// a potentially downloaded the file.   
    /// </summary>
    /// <param name="FileName"></param>
    /// <returns></returns>
    public static string GetFullFileName(string FileName)
    {

        // create a new string object via the GetInvalidFileNameChars, which gets a char array of all the invalid chars on windows. 
        string invalidCharacters = new(Path.GetInvalidFileNameChars());

        // create a regex for with square brackets, which Regex.Replace will interpret as look for any of the character. Without brackets, all characters would have become a single string. 
        string invalidCharacterPattern = $"[{Regex.Escape(invalidCharacters)}]";

        // Regex.Replace is faster then String.Replace since it uses bitmapping under the hood. 
        string fullFileName = Regex.Replace(FileName, invalidCharacterPattern, "").Trim() + ".io";

        return fullFileName;
    }

    public static void DownloadPageElements(Bot bot, string ByMechanism)
    {
        foreach (string IdentifierAttribute in bot.AttributeList)
        {
            // if there already exists a file by the fullFileName in the download folder, then move on to next set.
            string fullFileName = GetFullFileName(IdentifierAttribute);
            if (bot.IsFileDownloaded(fullFileName))
            {

                FilesAlreadyDownloadedCounter++;
                ExpectedSetClickAmount--;

                continue;
            }
            try
            {
                // try and find the current set element and only click it if it has a downloadable symbol
                IWebElement setNameElement = bot.WaitAndFind(IdentifierAttribute, ByMechanism);
                // We want to catch FindPageElement() BotFindElementException, if the download button is not there, since we then dont want to click the setNameElement 
                IWebElement? DownloadableSymbolElement = bot.FindPageElement($"./ancestor::footer//i[contains(@class,'moc-card__download')]", "xp", setNameElement);
                if (setNameElement != null && DownloadableSymbolElement != null)
                {
                    bot.OpenTabWithElement(setNameElement);
                    SetClickCounter++;
                }
            }
            // if the current set element could not be found and clicked. 
            catch (BotTimeOutException)
            {
                Console.WriteLine($"The set named: {IdentifierAttribute} could not be found. Continueing\n");
                ExpectedSetClickAmount--;
                continue;
            }
            // if the current set element did not have a downloadable symbol. 
            catch (BotFindElementException)
            {
                Console.WriteLine($"The set named: {IdentifierAttribute}did not have a downloadable symbol. Continueing\n");
                ExpectedSetClickAmount--;
                continue;
            }


            try
            {
                // find The download button element on the current set page
                IWebElement? downloadButtonElement = bot.WaitAndFind("//button[contains(text(),'Download Studio file')]", "xp");
                // try to download the file via the downloadButtonElement
                string? currentFilePath = bot.DownloadFile(downloadButtonElement);

                // if the file was downloaded successfully
                if (currentFilePath != null)
                {
                    FilesDownloadedCounter++;
                    // try and rename the downloaded file if nessary
                    bot.GetAndRenameFile(currentFilePath, fullFileName);
                }
                else
                {
                    FilesDownloadTimedOutCounter++;
                }
                bot.CloseTab(0);
            }
            catch (BotTimeOutException)
            {
                Console.WriteLine($"The download button for the set '{IdentifierAttribute}' could not be found on its set page Continueing");
                bot.CloseTab(0);
            }
            // should be thrown in case of stale element or 404 page error.
            catch (BotStaleElementException)
            {
                bot.CloseTab(0);
            }
            // should be thrown in case of the file could not be downloaded for some reason. 
            catch (BotFileDownloadException ex)
            {
                Console.WriteLine(ex.Message);
                bot.CloseTab(0);
            }
            // When the time between clicking a download button and then attempting to rename the file might have been too short.
            catch (BotFileRenameException ex)
            {
                Console.WriteLine(ex.Message);
                bot.CloseTab(0);
            }
        }
    }


    // We ended up never using this function, so we might consider removing it from the interface.
    // However GetDataLdraw.cs uses it, so we need to figure some alternative out. 
    public static IWebElement FindDisplayedElement(Bot bot, Dictionary<string, string> ElementCandidatesDict)
    {
        foreach (KeyValuePair<string, string> Candiate in ElementCandidatesDict)
        {
            try
            {
                IWebElement? nextPageElement = bot.FindPageElement(Candiate.Key, Candiate.Value);
                if (nextPageElement != null && nextPageElement.Displayed)
                {
                    return nextPageElement;
                }
            }
            catch (BotFindElementException)
            {
                throw new BotFindElementException("Element not found in candidate dict, trying next option.");
            }
            catch (BotTimeOutException ex)
            {
                throw new BotTimeOutException($"The referenced element was found but, it was not displayed on the webpage: {ex.Message}");
            }
            catch (BotStaleElementException ex)
            {
                Console.WriteLine($"{ex.Message}");
            }
        }
        throw new BotFindElementException("No elements from the candidate dict was found. The candiate elements are either not displayed, or not representative of the page state");
    }


    public static void GoToNextPage(Bot bot, IWebElement NextButtonElement, int? ClickAmount)
    {
        try
        {
            // BrickLink page button can be clicked multiple times and load multiple sets with no new page load. 
            for (int i = 0; i < ClickAmount; i++)
            {
                // click next button if it is loaded. 
                Bot.ClickElement(NextButtonElement);
            }
            Thread.Sleep(3000);
        }
        catch (BotStaleElementException ex)
        {
            Console.WriteLine(ex.Message);
        }
        catch (BotTimeOutException ex)
        {
            Console.WriteLine(ex.Message);
        }

    }

    /// <summary>
    /// this also needs to be changed once we implement the feature of checking for if sets can be downloaded before clicking them. 
    /// We will have to check 
    /// </summary>
    /// <returns></returns>
    public static bool AssertDownloadAmount()
    {
        int notDownloadableSets = ExpectedSetsScraped - (FilesAlreadyDownloadedCounter + FilesDownloadedCounter);
        /*A run is completed sucessfully if and only if:
            2) The amount of setsclicked (SetClickCounter) is the same as the amount of files downloaded (FilesDownloadedCounter)
            3) The expected amount of sets to be scraped is the same as the amount of files downloaded + the amount of files already downloaded + the amount of files whoms download timed out + the amount of not downloadable sets
        */
        bool runStatus = RunCompleted && ExpectedSetClickAmount == SetClickCounter && ExpectedSetsScraped == (FilesDownloadedCounter + FilesAlreadyDownloadedCounter + FilesDownloadTimedOutCounter + notDownloadableSets);
        try
        {
            if (!CustomRun && runStatus)
            {
                Console.WriteLine($"Run Sucessfully finished!\n---------------------------------------------------");

            }
            else if (CustomRun && runStatus)
            {

                Console.WriteLine($"Custom run Sucessfully finished!\n---------------------------------------------------");
            }
            else
            {
                throw new BotDownloadAmountException();
            }
            Console.WriteLine(
                $"RunCompleted:       {RunCompleted}\n" +
                $"Sets To Scrape:     {ExpectedSetsScraped}\n" +
                $"Sets Clicked:       {ExpectedSetClickAmount} set(s) were expected to be clicked {SetClickCounter} were actually clicked\n" +
                $"Files Downloaded:   {FilesDownloadedCounter} set(s) were actually downloaded\n" +
                $"Already Downloaded  {FilesAlreadyDownloadedCounter} set(s) were infered to already be downloaded\n" +
                $"Timed Out:          {FilesDownloadTimedOutCounter} set(s) had their download timed out\n" +
                $"Not Downloadable    {notDownloadableSets} set(s) were not downloadable\n" +
                $"---------------------------------------------------\n");
            return true;
        }

        catch (BotDownloadAmountException ex)
        {
            Console.WriteLine($"Run Failed!\n" +
                $"---------------------------------------------------\n" +
                $"RunCompleted:       {RunCompleted}\n" +
                $"Sets To Scrape:     {ExpectedSetsScraped}\n" +
                $"Sets Clicked:       {ExpectedSetClickAmount} set(s) were expected to be clicked {SetClickCounter} were actually clicked\n" +
                $"Files Downloaded:   {FilesDownloadedCounter} set(s) were actually downloaded\n" +
                $"Already Downloaded  {FilesAlreadyDownloadedCounter} set(s) were infered to already be downloaded\n" +
                $"Timed Out:          {FilesDownloadTimedOutCounter} set(s) had their download timed out\n" +
                $"Not Downloadable    {notDownloadableSets} set(s) were not downloadable\n" +
                $"---------------------------------------------------\n" +
                $"{ex.Message}");
            return false;
        }
    }

    // ---------------------------------------------------------------------------------------------------------------------------------------------//

    //process the BrickLink website LEGO sets and download them. 
    public static void ProcessData()
    {
        // clean up any existing preferences file from previous bot runs.
        Bot.CleanupPreferencesFile();

        Bot bot = new(Url, DownloadFolderPath);
        try
        {
            AccessMainPage(bot);
            // initial check if run is custom or not
            if (CustomRun)
            {
                /*We need to configure the run after acessing the main page for this implementation, since subpages 
                can only be accessed though the main page by direct bot clicks.*/
                ConfigureCustomRun(bot);
            }

            Stopwatch sw = Stopwatch.StartNew();
            // the first page root which is the ancestor div of all set elements on the main page.
            IWebElement? pageRootElement = bot.WaitAndFind("//div[@class='studio-gallery__card-container']", "xp");
            for (int i = 0; i < PageLimit; i++)
            {
                // we use"Text" as identifier for simplicity 
                if (pageRootElement != null)
                {
                    SetAttributeList(bot, $".//following::a[@class='moc-card__name']", "xp", "Text", pageRootElement);
                    Console.WriteLine($"Page {i}: AttributeList has {bot.AttributeList.Count} items");
                    DownloadPageElements(bot, "lt");
                }

                /* Set the pageRootElement as the last element in the attribute list. Find it from the previous pageRootElement.
                First escape any backslash with double quotes with double quotes and then escape double quotes with single quotes 
                to ensure XPath can find the element*/
                string currentRoot = bot.AttributeList[^1].Replace("\"", "'");
                pageRootElement = bot.WaitAndFind($"(//a[contains(translate(normalize-space(),'\"',\"'\"),\"{currentRoot}\")])[last()]", "xp");



                // Find the Next button elements which works, considering page responsiveness
                IWebElement nextButtonElement = bot.WaitAndFind("//button[contains(text(),'Load more creations')]", "xp");

                // We need i< PageLimit-1 since we dont want to set a new root, even if there is one, for future pages if current page is last page 
                if (i < PageLimit - 1)
                {
                    Console.Write($"current root: {pageRootElement!.Text}\n");
                    GoToNextPage(bot, nextButtonElement, 1);
                }
            }
            sw.Stop();
            Console.WriteLine($"Scraping of current run took: {sw.Elapsed}");
            RunCompleted = true;


        }
        catch (BotTimeOutException ex)
        {
            Console.WriteLine($"{ex}");

        }
        catch (BotMechanismException ex)
        {
            Console.WriteLine($"{ex}");
        }
        finally
        {
            AssertDownloadAmount();
            Bot.CleanupPreferencesFile();
            bot.CloseBot();
        }
    }
}
