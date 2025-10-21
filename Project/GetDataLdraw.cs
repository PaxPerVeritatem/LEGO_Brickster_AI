namespace LEGO_Brickster_AI;

using OpenQA.Selenium;
sealed class GetDataLdraw : IGetData
{
    // the following static fields are can be used to setup run configurations

    // defining whether running the bot should stop after a certain page. 

    // must have setter to be mutable and for UseCustomStartingPage to work. 
    public static string Url { get; set; } = "https://library.ldraw.org/omr/sets";
    public static bool CustomRun => false;

    public static int StartFromPage => 44;

    public static int SetsPrPage => 25;

    public static int PageLimit => 59;

    public static string UrlPageVarient => "?page=";

    public static Dictionary<string, string> NextPageElements => new() {
        {"//button[@rel='next']", "xp" },
        {"//button[@aria-label='Next']","xp" }

        };


    // minus 11 because we only have 15 sets on the last page and 1 set leads to 404 error page . 
    public static int ExpectedElementClickAmount => SetsPrPage * PageLimit - 11;

    public static int ElementClickCounter { get; set; } = 0;


    // Can be changed to another path if desired.  
    public static string DownloadFolderPath => @"..\..\..\LEGO_Data";



    public static void UseCustomStartingPage()
    {
        Url = $"{Url}{UrlPageVarient}{StartFromPage}";
    }



    public static void AccessWebPage(Bot bot)
    {
        try
        {
            bot.GoToWebpage();
        }
        catch (BotUrlException ex)
        {
            Console.WriteLine($"Failed to load webpage: {ex.Message}");
            bot.CloseBot();
        }
        catch (BotElementException ex)
        {
            Console.WriteLine($"Failed to return an html element\n{ex.Message}");
            Console.WriteLine("Closeing driver");
            bot.CloseBot();
        }
        catch (BotMechanismException ex)
        {
            Console.WriteLine($"By() mechanism is invalid: {ex.Message}\n");
            Console.WriteLine("Closeing driver");
            bot.CloseBot();
        }
    }


    public static void SetAttributeList(Bot bot, string CommonElementString, string CommonByMechanism)
    {
        // currently we go one page over what we should in page limit. But before we had one page less! 
        for (int i = 0; i < PageLimit; i++)
        {
            // Attempt to get the list of LEGO set names for the current main page
            bot.AttributeList = bot.FindPageElements(CommonElementString, CommonByMechanism);
        }
    }


    public static void DownloadPageElements(Bot bot, string ElementString, string ByMechanism)
    {
        for (int i = 0; i < PageLimit; i++)
        {
            try
            {
                // Attempt to find LEGO set LinkTest element
                IWebElement? setNameElement = bot.FindPageElement(ElementString, ByMechanism);

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

            catch (BotElementException ex)
            {
                // if we cant find the downloadButtonElement there must either be 0 or we have clicked them all, or we have reached a 404 page. 
                Console.WriteLine($"No more download buttons on current set page:{ex.Message}");
                bot.GoBack();
            }
            catch (BotMechanismException ex)
            {
                Console.WriteLine($"By() mechanism is invalid: {ex.Message}\n");

            }
            // should be thrown in case of stale element or 404 page error. Shitty fix 
            catch (BotException ex)
            {
                Console.WriteLine(ex.Message);
                bot.GoBack();
                bot.GoBack();
            }
        }
    }


    public static IWebElement GetNextPageElement(Bot bot)
    {
        foreach (KeyValuePair<string, string> Elementtuple in NextPageElements)
        {
            try
            {
                IWebElement? nextPageElement = bot.FindPageElement(Elementtuple.Key, Elementtuple.Value);
                if (nextPageElement != null && nextPageElement.Displayed)
                {
                    return nextPageElement;
                }
            }
            catch (BotElementException ex)
            {
                Console.WriteLine($"{ex}: Element not found, trying next option.");
            }
            catch (WebDriverTimeoutException ex)
            {
                throw new BotTimeOutException($"The referenced element was found but, it was not displayed on the webpage: {ex}");
            }
        }
        throw new BotElementException("No valid next page element found - all options in NextPageElements dictionary were either not found or not displayed.");
    }


    public static void GoToNextPage(Bot bot, IWebElement NextButtonElement)
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

        Bot bot = new(Url, DownloadFolderPath);
        if (CustomRun)
        {
            UseCustomStartingPage();
        }

        AccessWebPage(bot);
        try
        {
            SetAttributeList(bot, "fi-ta-cell-name", "class");

            foreach (string name in bot.AttributeList)
            {
                DownloadPageElements(bot, name, "lt");
            }
            // Find the Next button elements which works, considering page responsiveness
            IWebElement nextButtonElement = GetNextPageElement(bot);
            GoToNextPage(bot, nextButtonElement);

        }
        catch (BotElementException)
        {
            Console.WriteLine($"No more next buttons. Reached last page.");
        }
        finally
        {
            bot.CloseBot();
        }
    }
}