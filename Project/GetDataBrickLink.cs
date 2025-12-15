namespace LEGO_Brickster_AI;

using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using System.Text.RegularExpressions;
sealed class GetDataBrickLink : IGetData
{
    // Global run Properties
    public static string Url { get; set; } = "https://www.bricklink.com/v3/studio/design.page?tab=Staff-Picks";
    public static string DownloadFolderPath => @"..\..\..\LEGO_Data\BrickLink_Data";

    public static int MaxPage => 40;

    public static int PageLimit => MaxPage;

    public static int ExpectedSetsPrPage => 50;

    // so far there does not seems to be any 404 error for any sets. 
    public static int ExpectedElementClickDeviation => 0;

    public static int ExpectedElementClickAmount { get; set; } = ExpectedSetsPrPage * PageLimit - ExpectedElementClickDeviation;

    public static int ElementClickCounter { get; set; } = 0;


    // Custom run Properties 
    public static bool CustomRun => true;

    // Not nessesary for this implementation. 
    public static int StartFromPage => 0;

    // We use SubPageElement in this implementation, so we dont need UrlPageVarien. 
    public static string? UrlPageVarient { get; set; } = null;


    // Each subpage is just a IWebElement with an accompanying the ByMechanism to call FindElement during ConfigureCustomRun(). 
    public static (string ElementString, string ByMechanism)? SubpageElementTuple { get; set; } = ("//li[@data-ts-id='1']", "xp");


    public static int DataDownloadAmount = 0;



    public static void ConfigureCustomRun(Bot bot)
    {
        //find and click the subpage link text to acess subpage. Additionally, we can use forgive operator, since we always manually set SubPageElementTuple. 
        IWebElement? subPageElement = bot.FindPageElement(SubpageElementTuple!.Value.ElementString, SubpageElementTuple!.Value.ByMechanism!);
        bot.ClickElement(subPageElement);
        Thread.Sleep(1000);
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
                // Attempt to find LEGO set LinkTest element
                IWebElement? setNameElement = bot.FindPageElement(IdentifierAttribute, ByMechanism);

                // if current LinkText is not null click the set 
                if (bot.WaitTillExists(setNameElement))
                {
                    bot.OpenTabWithElement(setNameElement);
                    // increment for each clicked LEGO set. Should finally match 'downloadAmount'
                    ElementClickCounter += 1;
                }

                // The main div containing set info on each set page
                IWebElement? Modeldiv = bot.FindPageElement("//div[@class='studio-model__meta-block studio-model__meta-block--main']", "xp");
                // wait until ModelElement has rendered on page
                if (bot.WaitTillExists(Modeldiv))
                {
                    // find The download button element on each set page
                    IWebElement? downloadButtonElement = bot.FindPageElement("//button[contains(text(),'Download Studio file')]", "xp");

                    //Use IdentifierAttribute as filename, since it matches the name of the downloaded file. 
                    string fullFileName = GetFullFileName(IdentifierAttribute);

                    // if the downloadbutton is there but the file is already downloaded, go back to main page.
                    if (bot.IsFileDownloaded(fullFileName))
                    {
                        bot.CloseTab(0);
                    }
                    else
                    {
                        bot.ClickElement(downloadButtonElement);
                        DataDownloadAmount++;
                        Thread.Sleep(500);
                        bot.GetAndRenameFile(fullFileName);
                        bot.CloseTab(0);
                    }
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
            bot.AttributeList = [];
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
    public static bool AssertDownloadAmount()
    {
        try
        {
            if (ExpectedElementClickAmount == ElementClickCounter)
            {
                Console.WriteLine($"Expected {ExpectedElementClickAmount}, matched clicked {ElementClickCounter} set page elements");
                return true;
            }
            else
            {
                throw new BotDownloadAmountException($"Expected {ExpectedElementClickAmount}, but clicked {ElementClickCounter} set page elements");
            }
        }
        catch (BotDownloadAmountException ex)
        {
            Console.WriteLine($"Assumed amount of clicked LEGO Sets was either not correct or something went wrong during clicking set elements:{ex.Message}");
            return false;
        }
    }

    // ---------------------------------------------------------------------------------------------------------------------------------------------//

    //process the Ldraw website LEGO sets and download them. 
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
                    Console.Write($"current root:{pageRootElement!.Text}\n");
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
            Console.WriteLine("Download Amount during current run: " + DataDownloadAmount);
            bot.CloseBot();
        }
    }
}
