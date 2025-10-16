namespace LEGO_Brickster_AI;

using OpenQA.Selenium;
sealed class GetDataLdraw : IGetData 
{
    // the following static fields are can be used to setup run configurations

    // defining whether running the bot should stop after a certain page. 

    public static string Url {get; set;}  = "https://library.ldraw.org/omr/sets";
    public static bool CustomRun => false;

    public static int StartFromPage => 44;

    public static int SetsPrPage => 25;

    public static int PageLimit => 59;

    public static string UrlPageVarient => "?page=";

    

    // minus 10 because we only have 15 sets on the last page. 
    public static int ExpectedElementClickAmount => SetsPrPage * PageLimit - 10;

    public static int ElementClickCounter { get; set; } = 0; 


    // Can be changed to another path if desired.  
    public static string DownloadFolderPath => @"..\..\..\LEGO_Data";


    public static void AccessWebPage(Bot bot)
    {
        bot.GoToWebpage();
    }

    public static void UseCustomStartingPage()
    {
        Url = $"{Url}{UrlPageVarient}{StartFromPage}";
    }



    //process the Ldraw website LEGO sets and download them. 
    public static void ProcessData()
    {
        if (CustomRun)
        {
            UseCustomStartingPage();
        }

        Bot bot = new(Url, DownloadFolderPath);

        try
        {
            // Attempt to access webpage
            AccessWebPage(bot);
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

        // initialize the 'Next' button element for the main page
        IWebElement? nextButtonElement;
        try
        {
            // currently we go one page over what we should in page limit. But before we had one page less! 
            for (int i = 0; i < PageLimit; i++)
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

                // look for the next page button 
                nextButtonElement = bot.FindPageElement("//button[@aria-label='Next']", "xp");

                // click next button if it is loaded. 
                if (bot.WaitTillExists(nextButtonElement))
                {
                    Bot.ClickElement(nextButtonElement);

                    // Bot should not proceed until the next page to loaded after clicking the 'Next' page button
                    bot.ExplicitWait();
                }
                // reset the bot attribute list for next page of elements. 
                bot.AttributeList = [];

                // while there is an other page of sets to go to. 
            }
        }


        catch (BotElementException)
        {
            Console.WriteLine($"No more next buttons. Reached last page.");
        }
        finally
        {
            bot.CloseBot();
        }
        try
        {
            if (ExpectedElementClickAmount == ElementClickCounter)
            {
                Console.WriteLine($"Expected {ExpectedElementClickAmount}, matched clicked {ElementClickCounter} set page elements");

            }
            else
            {
                throw new BotDownloadAmountException($"Expected {ExpectedElementClickAmount}, but clicked {ElementClickCounter} set page elements");
            }
        }
        catch (BotDownloadAmountException ex)
        {
            Console.WriteLine($"Assumed amount of LEGO Sets was either not correct or something went wrong during clicking set elements:{ex}");
        }
    }
}