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

        // create new bot
        Bot bot = new();
        try
        {
            // Attempt to access webpage
            string url = "https://library.ldraw.org/omr/sets";
            bot.GoToWebpage(url);

            // Attempt to find some html element via some by mechanism

            IWebElement? setElement = bot.FindPageElement("Metroliner", "LTf");

            // if setElement is not null call Click()  
            setElement?.Click();



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
        
//
//

        //IWebElement? mainModelElement = bot.CheckForMainModel();
        //
        //
        //
        //IWebElement? downloadButtonElement = bot.FindDownloadElement(".//following::a[contains(.,'Download')]", "XP", mainModelElement);
        //
        //downloadButtonElement.Click();
        //
        //bot.CloseBrowser();
        //bot.ImplicitWait(15);




    }
}