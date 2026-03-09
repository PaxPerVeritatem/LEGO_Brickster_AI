namespace LEGO_Brickster_AI;

using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using System.Text.RegularExpressions;
sealed class GetDataBrickLink : IGetData
{
    // Global run Properties
    public static string Url { get; set; } = "https://www.bricklink.com/v3/studio/design.page?tab=Staff-Picks";
    public static string DownloadFolderPath => @"..\..\..\LEGO_Data\BrickLink_Data";

    // We cant infer the MaxPage for this implementation.
    public static int? MaxPage => null;

    public static int PageLimit => 1;


    public static int ExpectedSetsPrPage => 50;

    // so far there does not seems to be any 404 error for any sets, so this can  be 0 in this implementation for now. 
    public static int ExpectedSetClickDeviation => 0;


    public static int ExpectedSetClickAmount { get; set; } = ExpectedSetsPrPage * PageLimit - ExpectedSetClickDeviation;


    public static int SetClickCounter { get; set; } = 0;

    public static int FileDownloadCounter { get; set; } = 0;


    // Custom run Properties 
    public static bool CustomRun => true;

    // Not nessesary for this implementation. 
    public static int? StartFromPage => null;

    // We use SubpageElementTuple in this implementation, so we dont need UrlPageVarient or StartFromPage . 
    public static string? UrlPageVarient { get; set; } = null;


    // Each subpage is just a IWebElement with an accompanying the ByMechanism to call FindElement during ConfigureCustomRun(). 
    public static (string ElementString, string ByMechanism)? SubpageElementTuple => ("//li[@data-ts-id='3']", "xp");


    public static bool UseSubpage => true;

    public static void ConfigureCustomRun(Bot bot)
    {
        if (UseSubpage)
        {
            /*find and click the subpage link text to access the subpage. 
            Additionally, we can use forgive operator, since we always manually set SubPageElementTuple.*/
            IWebElement? subPageElement = bot.FindPageElement(SubpageElementTuple!.Value.ElementString, SubpageElementTuple!.Value.ByMechanism!);
            bot.ClickElement(subPageElement);
            Thread.Sleep(1000);
            /*
                a note for the next next commit. 
                We can actually skip this and do a check for all downloadable within the attribute list, since there is a symbol indicateing 
                if sets can be downloaded on their initial cards. This can help us skip the "check if there is a download button and make the scrapeing even faster
                when that is implemented, we can actually remove the below line on subpages, since it works on all pages via the attribute list names. 
            */
            IWebElement? ShowOnlyDownloadableSets = bot.FindPageElement("//option[contains(text(),'Downloadable')]", "xp");
            bot.ClickElement(ShowOnlyDownloadableSets);
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
            IWebElement? ageGateElement = bot.FindPageElement("//input[@class='blp-age-gate__input-field']", "xp");
            bot.ClickElement(ageGateElement);
            actionBuilder.SendKeys("1");
            actionBuilder.SendKeys("9");
            actionBuilder.SendKeys("9");
            actionBuilder.SendKeys("4");
            actionBuilder.Perform();
        }
        catch (BotFindElementException)
        {
            Console.WriteLine($"Age gate input field was not found or was not present. Continueing");
        }

        // find and press cookie button. If its not there, throw an exception and continue. 
        try
        {
            IWebElement? cookieButton = bot.FindPageElement("//article[@class='blp-cookie-notice__content']//button[contains(text(), 'Reject all')]", "xp");
            bot.ClickElement(cookieButton);
            actionBuilder.Click();
            actionBuilder.Perform();
        }
        catch (BotFindElementException)
        {
            Console.WriteLine($"Cookie button was not found or was not present. Continueing");
        }
    }

    public static void SetAttributeList(Bot bot, string CommonElementString, string CommonByMechanism, string IdentifierAttribute, IWebElement AncestorElement)
    {
        // Attempt to get the list of LEGO set names for the current main page
        bot.AttributeList = bot.FindPageElements(CommonElementString, CommonByMechanism, IdentifierAttribute, AncestorElement);
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
            try
            {
                // if there already exists a file by the fullFileName in the download folder, then move on to next set.
                string fullFileName = GetFullFileName(IdentifierAttribute);
                if (bot.IsFileDownloaded(fullFileName))
                {
                    ExpectedSetClickAmount--;
                    continue;
                }

                // Attempt to find LEGO set LinkTest element, if its file is not already downloaded.
                // Here we can add another check and only click if its not also downloadable by the card indicator
                IWebElement? setNameElement = bot.FindPageElement(IdentifierAttribute, ByMechanism);
                if (bot.WaitTillExists(setNameElement))
                {
                    bot.OpenTabWithElement(setNameElement);
                    SetClickCounter++;
                }

                // The main div containing set info on each set page
                IWebElement? Modeldiv = bot.FindPageElement("//div[@class='studio-model__meta-block studio-model__meta-block--main']", "xp");
                // wait until ModelElement has rendered on page
                if (bot.WaitTillExists(Modeldiv))
                {
                    // find The download button element on each set page
                    IWebElement? downloadButtonElement = bot.FindPageElement("//button[contains(text(),'Download Studio file')]", "xp");
                    bot.ClickElement(downloadButtonElement);
                    // increment for each downloaded LEGO set. 
                    FileDownloadCounter++;
                    Thread.Sleep(500);
                    bot.GetAndRenameFile(fullFileName);
                    bot.CloseTab(0);

                }
            }

            // if we cant find the downloadButtonElement there must either be 0 or we have clicked them all, or we have reached a 404 page. 
            catch (BotFindElementException)
            {

                //Console.WriteLine($"No more download buttons on current set page:{ex.Message}");
                bot.CloseTab(0);
            }
            // should be thrown in case of stale element or 404 page error.
            catch (BotStaleElementException)
            {
                // first go back to set page, and then press main page button on the set page in question. 
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
            }

        }
    }

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
                bot.ClickElement(NextButtonElement);
            }
            // reset the bot attribute list for next page of elements.
            bot.AttributeList.Clear();
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
        bool runStatus = ExpectedSetClickAmount == SetClickCounter;
        try
        {
            if (!CustomRun && runStatus)
            {
                Console.WriteLine($"Run on main page Sucessfully finished!");
            }
            else if (CustomRun && runStatus)
            {

                Console.WriteLine($"Custom run Sucessfully finished!");
            }
            else
            {
                throw new BotDownloadAmountException($"Expected to click:{ExpectedSetClickAmount} sets. Actually clicked:{SetClickCounter}. Downloaded: {FileDownloadCounter} LEGO sets");
            }
            Console.WriteLine($"Total amount of LEGO sets expected to be scaped in run: {ExpectedSetClickAmount}.");
            Console.WriteLine($"Amount of LEGO set pages checked for potential download: {SetClickCounter}");
            Console.WriteLine($"{FileDownloadCounter} amount of LEGO sets could actually be downloaded!\n");
            return true;
        }
        catch (BotDownloadAmountException ex)
        {
            Console.WriteLine($"Assumed amount of clicked LEGO Sets was either not correct or something went wrong during clicking set elements\n{ex.Message}");
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

            // the first page root which is the ancestor div of all set elements on the main page.
            IWebElement? pageRootElement = bot.FindPageElement("//div[@class='studio-gallery__card-container']", "xp");
            for (int i = 0; i < PageLimit; i++)
            {
                // we "Text" as identifier for simplicity 
                if (pageRootElement != null)
                {
                    SetAttributeList(bot, $".//following::a[@class='moc-card__name']", "xp", "Text", pageRootElement);
                    DownloadPageElements(bot, "lt");
                }

                /* Set the pageRootElement as the last element in the attribute list. Find it from the previous pageRootElement.
                Escape double quotes which will allow for pageRootElement to have single or double quotes in its name, but not both*/
                pageRootElement = bot.FindPageElement($"//a[contains(text(),\"{bot.AttributeList[^1]}\")]", "xp");
                bot.WaitTillExists(pageRootElement);

                // Find the Next button elements which works, considering page responsiveness
                IWebElement? nextButtonElement = bot.FindPageElement("//button[contains(text(),'Load more creations')]", "xp");
                // We need i< PageLimit-1 since we dont want to set a new root, even if there is one, for future pages if current page is last page 
                if (nextButtonElement != null && i < PageLimit - 1)
                {
                    Console.Write($"current root: {pageRootElement!.Text}\n");
                    GoToNextPage(bot, nextButtonElement, 1);
                    // This long sleep is nessesary to load next ExpectedSetsPrPage 
                    Thread.Sleep(1000);
                }
            }
        }
        catch (BotFindElementException ex)
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
