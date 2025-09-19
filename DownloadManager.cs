namespace LEGO_Brickster_AI;

using System;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;


public class DownloadManager
{
    private Bot bot;
    private readonly ChromeOptions options;
    private IList<string> NameList { get; set; }
    public string Downloadfolderstring { get; set; }
    public string Url { get; set; }

    public DownloadManager(string Downloadfolderstring, string Url)
    {
        options = new ChromeOptions();
        NameList = [];
        this.Downloadfolderstring = Downloadfolderstring;
        this.Url = Url;
        bot = InitializeBot();
    }

    public Bot InitializeBot()
    {
        // add preferenced download folder to options preferences.
        options.AddUserProfilePreference("download.default_directory", Downloadfolderstring);
        // to allow for multiple downloads and prevent the browser from blocking them 'allow multiple downloads' prombt
        options.AddUserProfilePreference("disable-popup-blocking", "true");
        // initialize bot with predefined preference
        bot = new Bot(options);
        return bot;
    }

    public static void Main(string[] args)
    {
        // create new DownloadManager and attempt to navigate to main webpage via Bot
        string downloadFolderString = @"C:\Users\admin\OneDrive\Skrivebord\LEGO_Brickster_AI\LEGO_Data";
        string url = "https://library.ldraw.org/omr/sets";
        DownloadManager dm = new(downloadFolderString,url);
        try
        {   
            // Attempt to access webpage
            dm.bot.GoToWebpage(url);

            // Attempt to find some html element via some by mechanism
            dm.NameList = dm.bot.FindPageElements("fi-ta-cell-name", "CLASSNAME");
        }
        catch (BotUrlException ex)
        {
            Console.WriteLine($"Failed to load webpage: {ex.Message}");
            Console.WriteLine("Closeing driver");
            dm.bot.CloseBrowser();
        }
        catch (BotElementException ex)
        {
            Console.WriteLine($"Failed to return an html element\n{ex.Message}");
            Console.WriteLine("Closeing driver");
            dm.bot.CloseBrowser();
        }
        catch (BotMechanismException ex)
        {
            Console.WriteLine($"By() mechanism is invalid: {ex.Message}\n");
            Console.WriteLine("Closeing driver");
            dm.bot.StopBot();
        }
        // for each LEGO set element 
        foreach (string name in dm.NameList)
        {
            try
            {
                // Attempt to find LEGO set LinkTest element
                IWebElement? nameElement = dm.bot.FindPageElement(name, "LT");

                // if current LinkText is not null call Click()
                nameElement?.Click();

                // Attempt to find 'Main Model' element on LEGO set page
                IWebElement? ModelElement = dm.bot.FindPageElement("//div[contains(text(),'Model')]", "XP");

                // wait if ModelElement is not null
                if (dm.bot.WaitIfExists(ModelElement))
                {
                    // if the ModelElement is not null, attempt to find the first download button element
                    IWebElement? downloadButtonElement = dm.bot.FindPageElement(".//following::a[contains(.,'Download')]", "XP", ModelElement);

                    // while there are download buttons on the page find them and press them., 
                    while (true)
                    {
                        dm.bot.WaitIfExists(downloadButtonElement);
                        downloadButtonElement?.Click();
                        downloadButtonElement = dm.bot.FindPageElement(".//following::a[contains(.,'Download')]", "XP", downloadButtonElement);
                    }
                }
                dm.bot.GoBack();
            }
            catch (BotElementException ex)
            {
                // if we cant find the downloadButtonElement there must either be 0 or we have clicked them all

                Console.WriteLine($"Failed to return an html element\n{ex.Message}");
                dm.bot.GoBack();

            }
            catch (BotMechanismException ex)
            {
                Console.WriteLine($"By() mechanism is invalid: {ex.Message}\n");
                Console.WriteLine("Closeing driver");
                dm.bot.StopBot();
            }
        }
        dm.bot.StopBot();
    }
}