using System.Security.Policy;
using OpenQA.Selenium;


namespace LEGO_Brickster_AI;

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

        // create new bot, and attempt to navigate to webpage, find LEGO set element and click it, 
        // leading to set page. 
        Bot bot = new();
        string url = "https://library.ldraw.org/omr/sets";
        bot.GoToWebpage(url);
        bot.FindWebElements();
        bot.StopBot(); 




        try
        {
            // Attempt to access webpage

            bot.GoToWebpage(url);

            // Attempt to find some html element via some by mechanism
            IWebElement? setElement = bot.FindPageElement("Metroliner", "LT");

            // if setElement is not null call Click()  
            setElement?.Click();


            // Attempt to find 'Main Model' element on LEGO set page
            IWebElement? mainModelElement = bot.FindPageElement("//div[contains(text(),'Main Model')]", "XP");


            // if the mainModelElement is not null, attempt to find the download button element
            //  else go back to main set page. 
            if (mainModelElement?.Text == "Main Model")
            {
                IWebElement? downloadButtonElement = bot.FindPageElement(".//following::a[contains(.,'Download')]", "XP", mainModelElement);
                downloadButtonElement?.Click();
            }
            else
            {
                bot.GoBack();
            }
        }
        // Catches should maybe be handled better, but for now its fine. 
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
            bot.CloseBrowser();
        }
    }
}