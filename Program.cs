using OpenQA.Selenium.Chrome;

namespace LEGO_Brickster_AI;
using OpenQA.Selenium;
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



        // create new DownloadManager and attempt to navigate to main webpage via Bot
        string downloadFolderString = @"C:\Users\admin\OneDrive\Skrivebord\LEGO_Brickster_AI\LEGO_Data";
        string url = "https://library.ldraw.org/omr/sets";
        Bot bot = new(url,downloadFolderString);
        try
        {
            // Attempt to access webpage
            bot.GoToWebpage(url);

            // Attempt to find some html element via some by mechanism
            bot.NameList = bot.FindPageElements("fi-ta-cell-name", "CLASSNAME");
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
        // for each LEGO set element 
        foreach (string name in bot.NameList)
        {
            try
            {
                // Attempt to find LEGO set LinkTest element
                IWebElement? nameElement = bot.FindPageElement(name, "LT");

                // if current LinkText is not null call Click()
                nameElement?.Click();

                // Attempt to find 'Main Model' element on LEGO set page
                IWebElement? ModelElement = bot.FindPageElement("//div[contains(text(),'Model')]", "XP");

                // wait if ModelElement is not null
                if (bot.WaitIfExists(ModelElement))
                {
                    // if the ModelElement is not null, attempt to find the first download button element
                    IWebElement? downloadButtonElement = bot.FindPageElement(".//following::a[contains(.,'Download')]", "XP", ModelElement);

                    // while there are download buttons on the page find them and press them., 
                    while (true)
                    {
                        bot.WaitIfExists(downloadButtonElement);
                        downloadButtonElement?.Click();
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
        bot.StopBot();


    }
}

