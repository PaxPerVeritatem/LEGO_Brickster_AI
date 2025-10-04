namespace LEGO_Brickster_AI;

using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
static class Program
{
    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
        // To customize application configuration such as set high DPI settings or default font,
        // see https://aka.ms/applicationconfiguration.

        //GUI turned off for now 
        //ApplicationConfiguration.Initialize();
        //Application.Run(new Form1());

        //process the Ldraw website LEGO set and download them. 

        /* Use AppContext.BaseDirectory to the get the static path to the program .dll file. Combine it with the relative
        LEGO_Data path.Finally, GetFullPath to resolve all the relative paths '/..' and get the final absolute path */

        string downloadFolderPath = @"..\..\..\LEGO_Data";
        string url = "https://library.ldraw.org/omr/sets";
        int downloadAmount = 1465;
        int downloadCounter = 0;
        Bot bot = new(url, downloadFolderPath);
        try
        {
            // Attempt to access webpage
            bot.GoToWebpage();
        }
        catch (BotUrlException ex)
        {
            Console.WriteLine($"Failed to load webpage: {ex.Message}");
            Console.WriteLine("Closeing driver");
            bot.CloseBrowser();
        }
        catch (BotElementException ex)
        {
            Console.WriteLine($"Failed to return an html element\n{ex.Message}");
            Console.WriteLine("Closeing driver");
            bot.CloseBrowser();
        }
        catch (BotMechanismException ex)
        {
            Console.WriteLine($"By() mechanism is invalid: {ex.Message}\n");
            Console.WriteLine("Closeing driver");
            bot.Dispose();
        }

        // initialize the next button class element for the main page
        IWebElement? nextButtonClassElement;
        do
        {
            // Attempt to get the list of LEGO set names for the current main page
            bot.NameList = bot.FindPageElements("fi-ta-cell-name", "CLASSNAME");
            foreach (string name in bot.NameList)
            {
                try
                {
                    // Attempt to find LEGO set LinkTest element
                    IWebElement? setNameElement = bot.FindPageElement(name, "LT");

                    // if current LinkText is not null call Click()
                    if (bot.WaitTillExists(setNameElement))
                    {
                        Bot.ClickElement(setNameElement);
                    }
                
                    // add for each LEGO set. Should finally match 'downloadAmount'
                    downloadCounter += 1;
                    // Attempt to find 'Models' element on LEGO set page
                    IWebElement? ModelsElement = bot.FindPageElement("//div[contains(text(),'Model')]", "XP");
                    
                    // wait until ModelElement has rendered on page
                    if (bot.WaitTillExists(ModelsElement))
                    {
                        // if the ModelElement is not null, attempt to find the first download button element
                        IWebElement? downloadButtonElement = bot.FindPageElement(".//following::a[contains(.,'Download')]", "XP", ModelsElement);

                        // while there are download buttons on the page find them and press them., 
                        while (bot.WaitTillExists(downloadButtonElement))
                        {
                            // we can an add a check here later to avoid downloads of files we already have
                            // click current downloadButton
                            Bot.ClickElement(downloadButtonElement);
                            
                            // check if the downloadbutton does  leads to 404 page error
                            if (bot.WaitTillExists(bot.FindPageElement(".px-4.text-lg.text-gray-500.border-r.border-gray-400.tracking-wider", "CSS")))
                            {
                                bot.GoBack();
                            }
                            else
                            {
                                downloadButtonElement = bot.FindPageElement(".//following::a[contains(.,'Download')]", "XP", downloadButtonElement);
                            }
                            
                        }
                    }
                }
                catch (BotElementException ex)
                {
                    // if we cant find the downloadButtonElement there must either be 0 or we have clicked them all
                    Console.WriteLine($"Failed to return an html element\n{ex.Message}");
                    bot.GoBack();
                }
                catch (BotMechanismException ex)
                {
                    Console.WriteLine($"By() mechanism is invalid: {ex.Message}\n");

                }
            }
            /*We try to find the next button, after all elements for current page 
            have been download. This is to avoid stale data for the next button state.*/
            nextButtonClassElement = bot.FindPageElement("//button[@rel='next']", "XP");
            // click next button if it is loaded. 
            if (bot.WaitTillExists(nextButtonClassElement))
            {
                Bot.ClickElement(nextButtonClassElement);
                Thread.Sleep(500);
            }

            bot.NameList = [];
            // while there is an other page of sets to go to. 
        } while (nextButtonClassElement != null);

        try
        {

            if (downloadAmount == downloadCounter)
            {
                Console.WriteLine($"{downloadAmount} have been correctly infered and downloaded");
            }
            else
            {
                throw new BotDownloadAmountException($"{downloadAmount} did not match {downloadCounter}");
            }
        }
        catch (BotDownloadAmountException ex)
        {
            Console.WriteLine($"{ex}: Assumed amount of LEGO Sets was either not correct or something went wrong during downloading");
        }

    }

}