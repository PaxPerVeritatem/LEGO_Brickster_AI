namespace LEGO_Brickster_AI;

using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
sealed class GetDataBrickLink : IGetData
{
    public static string Url { get; set; } = "https://www.bricklink.com/v3/studio/design.page?idModel";



    // Global run Properties
    public static int MaxPage => 59;

    public static int PageLimit => 1;

    public static int ExpectedSetsPrPage => 50;

    public static int ExpectedElementClickDeviation => 0;

    public static int ExpectedElementClickAmount { get; set; } = ExpectedSetsPrPage * PageLimit - ExpectedElementClickDeviation;

    public static int ElementClickCounter { get; set; } = 0;

    public static string DownloadFolderPath => @"..\..\..\LEGO_Data\BrickLink_Data";


    // Custom run Properties 
    public static bool CustomRun => true;

    public static int StartFromPage => 1;

    public static string UrlPageVarient => "";

    public static Dictionary<string, string> NextPageElements => new() {
        {"//button[@rel='next']", "xp" },
        {"//button[@aria-label='Next']","xp" }
        };





    public static void UseCustomStartingPage()
    {
        Url = $"{Url}{UrlPageVarient}{StartFromPage}";
        if (StartFromPage != MaxPage)
        {
            ExpectedElementClickAmount += ExpectedElementClickDeviation;
        }
    }



    public static void AccessWebPage(Bot bot)
    {
        try
        {
            bot.GoToWebpage();
            Actions actionsBuilder = new(bot.Driver);
            // find and click the ageGateElement
            IWebElement? ageGateElement = bot.FindPageElement("//input[@class='blp-age-gate__input-field']", "xp");
            if (bot.WaitTillExists(ageGateElement))
            {
                Bot.ClickElement(ageGateElement);
                actionsBuilder.SendKeys("1");
                actionsBuilder.SendKeys("9");
                actionsBuilder.SendKeys("9");
                actionsBuilder.SendKeys("4");
                actionsBuilder.Perform();
                actionsBuilder.Reset();
            }
            // find and press cookie button 
            IWebElement? cookieButton = bot.FindPageElement("//div[@class='cookie-notice__content']//button[contains(text(), 'Just necessary')]", "xp");
            if (bot.WaitTillExists(cookieButton))
            {
                Bot.ClickElement(cookieButton);
            }
        }
        catch (BotUrlException ex)
        {
            Console.WriteLine($"Failed to load webpage: {ex.Message}");
        }
        catch (BotDriverException ex)
        {
            Console.WriteLine(ex.Message);
        }
        catch (BotFindElementException ex)
        {
            Console.WriteLine($"Failed to return an html element\n{ex.Message}");
        }
        catch (BotMechanismException ex)
        {
            Console.WriteLine($"By() mechanism is invalid: {ex.Message}\n");
        }
        catch (BotTimeOutException ex)
        {
            Console.WriteLine($"Bot waited _wait time before timing out waiting for an element to appear:{ex.Message}");
        }
        catch (BotStaleElementException ex)
        {
            Console.WriteLine($"{ex.Message}: Element was probably found but page responsiveness or reload caused staleness");
        }
    }


    public static void SetAttributeList(Bot bot, string CommonElementString, string CommonByMechanism, string IdentifierAttribute)
    {
        // Attempt to get the list of LEGO set names for the current main page
        bot.AttributeList = bot.FindPageElements(CommonElementString, CommonByMechanism, IdentifierAttribute);

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
                    Bot.ClickElement(setNameElement);
                    // add for each LEGO set. Should finally match 'downloadAmount'
                    ElementClickCounter += 1;
                }

                // Attempt to find 'Download Studio file' button element on LEGO set page
                IWebElement? downloadButtonElement = bot.FindPageElement("//button[contains(text(),'Download Studio file')]", "xp");

                if (bot.WaitTillExists(downloadButtonElement))
                {
                    // i belive we can forgive here since WaitTillExists checks for null element.
                    string downloadFileSubstring = Bot.GetFileName(downloadButtonElement!, "Text");

                    string downloadFilePath = Path.Combine(bot.AbsDownloadFolderPath!, downloadFileSubstring);

                    if (Bot.IsFileDownloaded(downloadFilePath))
                    {
                        // if the file has already been downloaded, skip it
                        continue;
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
                Console.WriteLine(ex.Message);
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
            catch (BotFindElementException ex)
            {
                throw new BotFindElementException($"{ex.Message}: Element not found, trying next option.");
            }
            catch (BotTimeOutException ex)
            {
                throw new BotTimeOutException($"The referenced element was found but, it was not displayed on the webpage: {ex.Message}");
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
            AccessWebPage(bot);
            for (int i = 0; i < PageLimit; i++)
            {
                SetAttributeList(bot, "//article[contains(@class,'card')]//a[@class='moc-card__name']", "xp", "href");
            
                DownloadPageElements(bot, "lt");

                //     // Find the Next button elements which works, considering page responsiveness
                //     IWebElement nextButtonElement = GetNextPageElement(bot);
                //     GoToNextPage(bot, nextButtonElement);
                // }
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