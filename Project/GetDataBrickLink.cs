namespace LEGO_Brickster_AI;

using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using DotNetEnv;
sealed class GetDataBrickLink : IGetData
{
    public static string Url { get; set; } = "https://www.bricklink.com/v3/studio/design.page";
    public static string DownloadFolderPath => @"..\..\..\LEGO_Data\BrickLink_Data";


    // Global run Properties
    public static int MaxPage => 59;

    public static int PageLimit => 1;

    public static int ExpectedSetsPrPage => 50;

    public static int ExpectedElementClickDeviation => 0;

    public static int ExpectedElementClickAmount { get; set; } = ExpectedSetsPrPage * PageLimit - ExpectedElementClickDeviation;

    public static int ElementClickCounter { get; set; } = 0;




    // Custom run Properties 
    public static bool CustomRun => false;

    public static int StartFromPage => 1;

    public static string UrlPageVarient => "";



    public static void UseCustomStartingPage()
    {
        Url = $"{Url}{UrlPageVarient}{StartFromPage}";
        if (StartFromPage != MaxPage)
        {
            ExpectedElementClickAmount += ExpectedElementClickDeviation;
        }
    }


    public static void AccessMainPage(Bot bot, Dictionary<string, string>? ElementCandidatesDict = null)
    {
        Actions actionBuilder = new(bot.Driver);
        try
        {
            bot.GoToWebPage();
        }
        catch (BotUrlException ex)
        {
            Console.WriteLine($"Failed to load webpage: {ex.Message}");
        }

        // find and click the ageGateElement. If its not there Throw exception and continue. 
        try
        {
            IWebElement? ageGateElement = bot.FindPageElement("//input[@class='blp-age-gate__input-field']", "xp");
            if (bot.WaitTillExists(ageGateElement))
            {
                Bot.ClickElement(ageGateElement);
                actionBuilder.SendKeys("1");
                Thread.Sleep(1000);
                actionBuilder.Perform();

                actionBuilder.SendKeys("9");
                Thread.Sleep(1000);
                actionBuilder.Perform();

                actionBuilder.SendKeys("9");
                Thread.Sleep(1000);
                actionBuilder.Perform();

                actionBuilder.SendKeys("4");
                actionBuilder.Perform();

            }
        }
        catch (BotFindElementException)
        {
            Console.WriteLine($"Age gate input field was not found or was not present. Continueing");
        }

        // find and press cookie button. If its not there, throw an exception and continue. 
        try
        {
            IWebElement? cookieButton = bot.FindPageElement("//article[@class='blp-cookie-notice__content']//button[contains(text(), 'Reject all')]", "xp");
            if (bot.WaitTillExists(cookieButton))
            {
                Bot.ClickElement(cookieButton);
                actionBuilder.Click();
                actionBuilder.Perform();
            }
        }
        catch (BotFindElementException)
        {
            Console.WriteLine($"Cookie button was not found or was not present. Continueing");
        }
    }




    public static void SetAttributeList(Bot bot, string CommonElementString, string CommonByMechanism, string? IdentifierAttribute)
    {
        // Attempt to get the list of LEGO set names for the current main page
        bot.AttributeList = bot.FindPageElements(CommonElementString, CommonByMechanism);
    }

    public static string GetFullFileName(string FileName)
    {
        string fileExtension = ".io";
        string? fullFileName = FileName + fileExtension;
        return fullFileName;
    }

    public static void DownloadPageElements(Bot bot, string ByMechanism)
    {
        // we need to specifically find the "Back To Studio Gallery" button on each set page, in order to return to the main page at the same page location. 
        IWebElement? BackToMainPageButton;  
        foreach (string IdentifierAttribute in bot.AttributeList)
        {
            try
            {
                // Attempt to find LEGO set LinkTest element
                IWebElement? setNameElement = bot.FindPageElement(IdentifierAttribute, ByMechanism);

                // if current LinkText is not null click the set 
                if (bot.WaitTillExists(setNameElement))
                {
                    Bot.ClickElement(setNameElement);

                    // add for each LEGO set. Should finally match 'downloadAmount'
                    ElementClickCounter += 1;
                }
                // Attempt to find 'Download Studio file' button element on LEGO set page
                IWebElement? downloadButtonElement = bot.FindPageElement("//button[contains(text(),'Download Studio file')]", "xp");

                if (bot.WaitTillExists(downloadButtonElement))
                {

                    string fullFileName = GetFullFileName(IdentifierAttribute);

                    if (bot.IsFileDownloaded(fullFileName))
                    {

                        BackToMainPageButton = bot.FindPageElement("//div[contains(text(),'Back to Studio Gallery')]", "xp");
                        Bot.ClickElement(BackToMainPageButton);
                    }
                    else
                    {
                        Bot.ClickElement(downloadButtonElement);
                        // try to find the next download button
                        downloadButtonElement = bot.FindPageElement(".//button[contains(text(),'Download Studio file')]", "xp", downloadButtonElement);
                    }
                }
            }
            catch (BotFindElementException ex)
            {
                // if we cant find the downloadButtonElement there must either be 0 or we have clicked them all, or we have reached a 404 page. 
                Console.WriteLine($"No more download buttons on current set page:{ex.Message}");
                BackToMainPageButton= bot.FindPageElement("//div[contains(text(),'Back to Studio Gallery')]", "xp");
                Bot.ClickElement(BackToMainPageButton);
            }

            // should be thrown in case of stale element or 404 page error.
            catch (BotStaleElementException)
            {
                // first go back to set page, and then press main page button on the set page in question. 
                bot.GoBack();
                BackToMainPageButton= bot.FindPageElement("//div[contains(text(),'Back to Studio Gallery')]", "xp");
                Bot.ClickElement(BackToMainPageButton);
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
                bot.ExplicitWaitURL(oldUrl);
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
            Console.WriteLine($"Assumed amount of LEGO Sets was either not correct or something went wrong during clicking set elements:{ex.Message}");
            return false;
        }
    }

    // ---------------------------------------------------------------------------------------------------------------------------------------------//

    //process the Ldraw website LEGO sets and download them. 
    public static void ProcessData()
    {
        // initial check if run is custom or not
        if (CustomRun)
        {
            UseCustomStartingPage();
        }
        Bot bot = new(Url, DownloadFolderPath);

        try
        {
            AccessMainPage(bot);
            for (int i = 0; i < PageLimit; i++)
            {
                // we dont use Identifier attribute for simplicity
                SetAttributeList(bot, "//article[contains(@class,'card')]//a[@class='moc-card__name']", "xp", null);
                DownloadPageElements(bot, "lt");
                // Find the Next button elements which works, considering page responsiveness
                IWebElement? nextButtonElement = bot.FindPageElement("//button[constains(text(),'Load more creations')]", "xp");
                if (nextButtonElement != null)
                {
                    GoToNextPage(bot, nextButtonElement);
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
            bot.CloseBot();
        }
    }
}