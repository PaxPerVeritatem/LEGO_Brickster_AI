namespace LEGO_Brickster_AI;

using OpenQA.Selenium;
public class GetDataLdraw
{
    private static readonly bool _startFromPage = true;
    private const string _downloadFolderPath = @"..\..\..\LEGO_Data";
    private readonly static string _urlPageVarient = "?page=";

    private static string _url = "https://library.ldraw.org/omr/sets";

    private static readonly int _downloadAmount = 1465;



    private static string StartFromPage(int PageNumber, string Url)
    {

        string StartFromPageUrl = $"{Url}{_urlPageVarient}{PageNumber}";
        return StartFromPageUrl;
    }

    //process the Ldraw website LEGO sets and download them. 
    public static void GetData()
    {
        if (_startFromPage)
        {
            _url = StartFromPage(59, _url);
        }

        int downloadCounter = 0;
        Bot bot = new(_url, _downloadFolderPath);
        try
        {
            // Attempt to access webpage
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

        // initialize the next button class element for the main page
        IWebElement? nextButtonElement;
        try
        {
            do
            {
                // Attempt to get the list of LEGO set names for the current main page
                bot.AttributeList = bot.FindPageElements("fi-ta-cell-name", "class");
                foreach (string name in bot.AttributeList)
                {
                    try
                    {
                        // Attempt to find LEGO set LinkTest element
                        IWebElement? setNameElement = bot.FindPageElement(name, "lt");

                        // if current LinkText is not null call Click()
                        if (bot.WaitTillExists(setNameElement))
                        {
                            Bot.ClickElement(setNameElement);
                        }

                        // add for each LEGO set. Should finally match 'downloadAmount'
                        downloadCounter += 1;
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
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        bot.GoBack();
                    }
                }

                /*We try to find the next button, after all elements for current page 
                have been download. This is to avoid stale data for the next button state.
                Based on the webpage size, the next might be changed due to responsiveness of the page. Hence we need to look for two possible next buttons*/
                nextButtonElement = bot.FindPageElement("//button[@aria-label='Next']", "xp");
                // click next button if it is loaded. 
                if (bot.WaitTillExists(nextButtonElement))
                {
                    Bot.ClickElement(nextButtonElement);
                    // wait until 
                    bot.ExplicitWait();
                }
                bot.AttributeList = [];
                // while there is an other page of sets to go to. 
            } while (nextButtonElement != null);
        }
        catch (BotElementException)
        {
            Console.WriteLine($"No more next buttons. Reached last page.");
            bot.CloseBot();
        }




        try
        {
            if (_downloadAmount == downloadCounter)
            {
                Console.WriteLine($"{_downloadAmount} have been correctly infered and downloaded");
                
            }
            else
            {
                throw new BotDownloadAmountException($"Expected {_downloadAmount}, but downloaded {downloadCounter}");
            }
        }
        catch (BotDownloadAmountException ex)
        {
            Console.WriteLine($"Assumed amount of LEGO Sets was either not correct or something went wrong during downloading:{ex}");
        }
    }
}

