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
       
        // The relative path of the data folder 
        string downloadFolderString = Path.GetFullPath(@"..\..\..\LEGO_Data");
        Console.WriteLine(downloadFolderString);
        string url = "https://library.ldraw.org/omr/sets";
        Bot bot = new(url,downloadFolderString);
        try
        {
            // Attempt to access webpage
            bot.GoToWebpage(url);
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
            bot.StopBot();
        }

        //find the first next button
        IWebElement? nextButtonClassElement = bot.FindPageElement("fi-pagination-next-btn", "CLASSNAME");
        // while there is an other page of set to go to. 
        while (nextButtonClassElement != null)
        {
            // Attempt to get the list of LEGO set names for the current main page
            bot.NameList = bot.FindPageElements("fi-ta-cell-name", "CLASSNAME");
            foreach (string name in bot.NameList)
            {
                try
                {
                    // Attempt to find LEGO set LinkTest element
                    IWebElement? nameElement = bot.FindPageElement(name, "LT");

                    // if current LinkText is not null call Click()
                    Bot.ClickElement(nameElement);

                    // Attempt to find 'Model' element on LEGO set page
                    IWebElement? ModelElement = bot.FindPageElement("//div[contains(text(),'Model')]", "XP");

                    // wait until ModelElement has rendered on page
                    if (bot.WaitIfExists(ModelElement))
                    {
                        // if the ModelElement is not null, attempt to find the first download button element
                        IWebElement? downloadButtonElement = bot.FindPageElement(".//following::a[contains(.,'Download')]", "XP", ModelElement);

                        // while there are download buttons on the page find them and press them., 
                        while (true)
                        {
                            bot.WaitIfExists(downloadButtonElement);
                            // we can an add a check here later to avoid downloads of files we already have
                            Bot.ClickElement(downloadButtonElement);
                            downloadButtonElement = bot.FindPageElement(".//following::a[contains(.,'Download')]", "XP", downloadButtonElement);
                        }
                    }
                    bot.GoBack();
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
                    Console.WriteLine("Closeing driver");
                    bot.StopBot();
                }
            }
            nextButtonClassElement = bot.FindPageElement("fi-pagination-next-btn", "CLASSNAME");
            /*We try to find the next button, after all elements for current page 
            have been download. This is to avoid stale data for the next button state. */
            // find the next button class element
            //click the button if it is there
            Bot.ClickElement(nextButtonClassElement); 
            bot.NameList = []; 
        }
    }
}