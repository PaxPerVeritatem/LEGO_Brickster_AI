namespace LEGO_Brickster_AI;

using OpenQA.Selenium;
sealed class GetDataLdraw : IGetData
{

    // Bot Properties
    public static string Url { get; set; } = "https://library.ldraw.org/omr/sets";


    public static string DownloadFolderPath => @"..\..\..\LEGO_Data\Ldraw_Data";


    public static string UserProfilePath => @"C:\Users\Daniel\Desktop\LEGO_Brickster_AI\Project\DriverProfile";

    // Global run Properties
    public static int MaxPage => 59;

    public static int PageLimit => 1;

    public static int ExpectedSetsPrPage => 25;

    public static int ExpectedElementClickDeviation => 10;

    public static int ExpectedElementClickAmount { get; set; } = ExpectedSetsPrPage * PageLimit - ExpectedElementClickDeviation;

    public static int ElementClickCounter { get; set; } = 0;



    // Custom run Properties 
    public static bool CustomRun => true;

    public static int StartFromPage => 1;

    public static string UrlPageVarient => "?page=";







    public static void UseCustomStartingPage()
    {
        Url = $"{Url}{UrlPageVarient}{StartFromPage}";
        if (StartFromPage != MaxPage)
        {
            ExpectedElementClickAmount += ExpectedElementClickDeviation;
        }
    }



    public static void AccessWebPage(Bot bot, Dictionary<string, string>? CandidateElementDict)
    {
        try
        {
            bot.GoToMainPage();
        }
        catch (BotUrlException ex)
        {
            Console.WriteLine($"Failed to load webpage: {ex.Message}");
            bot.CloseBot();
        }
        catch (BotDriverException ex)
        {
            Console.WriteLine(ex.Message);
        }
    }


    public static void SetAttributeList(Bot bot, string CommonElementString, string CommonByMechanism, string? IdentifierAttribute = null)
    {
        // Attempt to get the list of LEGO set names for the current main page
        bot.AttributeList = bot.FindPageElements(CommonElementString, CommonByMechanism);
    }


    public static void DownloadPageElements(Bot bot, string ByMechanism)
    {
        foreach (string IdentifierAttribute in bot.AttributeList)
        {
            try
            {
                // Attempt to find LEGO set LinkTest element
                IWebElement? setNameElement = bot.FindPageElement(IdentifierAttribute, ByMechanism);

                // if current LinkText is not null call Click()
                if (bot.WaitTillExists(setNameElement))
                {
                    Bot.ClickElement(setNameElement);
                    // add for each LEGO set. Should finally match 'downloadAmount'
                    ElementClickCounter += 1;
                }

                // Attempt to find 'Models' element on LEGO set page
                IWebElement? ModelsElement = bot.FindPageElement("//div[contains(text(),'Model')]", "xp");

                // wait until ModelElement has rendered on page
                if (bot.WaitTillExists(ModelsElement))
                {
                    // if the ModelElement is not null, attempt to find the first download button element
                    IWebElement? downloadButtonElement = bot.FindPageElement(".//following::a[contains(.,'Download')]", "xp", ModelsElement);

                    // while there are download buttons on the page find them and press them., 
                    while (bot.WaitTillExists(downloadButtonElement))
                    {
                        // i belive we can forgive here since WaitTillExists checks for null element.
                        string downloadFileSubstring = Bot.GetFileName(downloadButtonElement!, "href");

                        string downloadFilePath = Path.Combine(bot.AbsDownloadFolderPath!, downloadFileSubstring);
                        // if the file has already been downloaded, skip it
                        if (Bot.IsFileDownloaded(downloadFilePath))
                        {
                            // try to find the next download button. 
                            downloadButtonElement = bot.FindPageElement(".//following::a[contains(.,'Download')]", "xp", downloadButtonElement);
                        }
                        else
                        {
                            Bot.ClickElement(downloadButtonElement);
                            // try to find the next download button
                            downloadButtonElement = bot.FindPageElement(".//following::a[contains(.,'Download')]", "xp", downloadButtonElement);
                        }
                    }
                }
            }
            catch (BotFindElementException ex)
            {
                // if we cant find the downloadButtonElement there must either be 0 or we have clicked them all, or we have reached a 404 page. 
                Console.WriteLine($"No more download buttons on current set page:{ex.Message}");
                bot.GoBack();
            }
            catch (BotMechanismException ex)
            {
                Console.WriteLine($"By() mechanism is invalid: {ex.Message}\n");

            }
            // should be thrown in case of stale element or 404 page error.
            catch (BotStaleElementException)
            {
                bot.GoBack();
                bot.GoBack();
            }
        }
    }

    public static IWebElement FindDisplayedElement(Bot bot, Dictionary<string, string> CandidatElementDict)
    {
        foreach (KeyValuePair<string, string> Candidate in CandidatElementDict)
        {
            try
            {
                IWebElement? nextPageElement = bot.FindPageElement(Candidate.Key, Candidate.Value);
                if (nextPageElement != null && nextPageElement.Displayed)
                {
                    return nextPageElement;
                }
            }
            catch (BotFindElementException ex)
            {
                throw new BotFindElementException($"{ex}: Element not found, trying next option.");
            }
            catch (BotTimeOutException ex)
            {
                throw new BotTimeOutException($"The referenced element was found but, it was not displayed on the webpage: {ex}");
            }
        }
        throw new BotStaleElementException("The referenced element is no longer displayed on the webpage");
    }


    public static void GoToNextPage(Bot bot, IWebElement NextButtonElement)
    {
        try
        {
            // click next button if it is loaded. 
            if (bot.WaitTillExists(NextButtonElement))
            {
                // get the url of the driver before clicking the next button. 
                string oldUrl = bot.Driver.Url;
                Bot.ClickElement(NextButtonElement);

                // Bot should not proceed until the next page is fully loaded indicated by a change in the url.
                bot.ExplicitWait(oldUrl);
            }
            // reset the bot attribute list for next page of elements. 
            bot.AttributeList = [];
        }
        catch(BotStaleElementException ex)
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
            Console.WriteLine($"Assumed amount of LEGO Sets was either not correct or something went wrong during clicking set elements:{ex}");
            return false;
        }
    }

    // ---------------------------------------------------------------------------------------------------------------------------------------------//

    //process the Ldraw website LEGO sets and download them. 
    public static void ProcessData()
    {
        Dictionary<string, string> NextPageCandiates = new()
        {
            { "//button[@rel='next']", "xp" },
            { "//button[@aria-label='Next']","xp" }
        };


        // initial check if run is custom or not
        if (CustomRun)
        {
            UseCustomStartingPage();
        }
        Bot bot = new(Url, DownloadFolderPath,UserProfilePath);


        try
        {
            // no dict needed here.
            AccessWebPage(bot, null);
            for (int i = 0; i < PageLimit; i++)
            {
                SetAttributeList(bot, "fi-ta-cell-name", "class");

                DownloadPageElements(bot, "lt");

                // Find the Next button elements which works, considering page responsiveness
                IWebElement nextButtonElement = FindDisplayedElement(bot, NextPageCandiates);
                GoToNextPage(bot, nextButtonElement);
            }
        }
        catch (StaleElementReferenceException)
        {
            Console.WriteLine($"No more next buttons. Reached last page.");
        }
        finally
        {
            AssertDownloadAmount(); 
            bot.CloseBot();
        }
    }
}