namespace LEGO_Brickster_AI;

using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using OpenQA.Selenium.Support.UI;

public class Bot
{
    private readonly ChromeDriver _driver;
    public ChromeDriver Driver => _driver;

    private readonly ChromeOptions? _options;
    public ChromeOptions? Options => _options;

    private readonly WebDriverWait _wait;

    private IList<string> _nameList = [];
    public IList<string> NameList
    {
        get => _nameList;
        set => _nameList = value;
    }

    public string Url { get; set; }

    private readonly string? _absDownloadFolderPath;
    public string? AbsDownloadFolderPath => _absDownloadFolderPath;



   
    /// <summary>
    /// Initializes a new instance of the <see cref="Bot"/> class.
    /// </summary>
    /// <param name="url">The URL of the webpage to navigate to.</param>
    /// <param name="downloadFolderPath">The path to the download folder. If null, the default download folder will be used.</param>
    public Bot(string url, string? downloadFolderPath = null)
    {
        Url = url;
        if (downloadFolderPath != null)
        {
            _absDownloadFolderPath = GetAbsoluteDownloadFolderPath(downloadFolderPath);
            _options = InitializeBotPrefs(_absDownloadFolderPath);
            _driver = new ChromeDriver(_options);
        }
        else
        {
            _driver = new ChromeDriver();
        }
    
            _wait = new(_driver, TimeSpan.FromSeconds(2));
    }


    public static string GetAbsoluteDownloadFolderPath(string DownloadFolderPath)
    {
        string absDownloadFolderPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, DownloadFolderPath));
        return absDownloadFolderPath;
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="ChromeOptions"/> class, with a pre-defined download folder preference.
    /// </summary>
    /// <param name="DownloadFolderPath">The path to the download folder.</param>
    /// <returns>A new instance of <see cref="ChromeOptions"/> with the pre-defined download folder preference.</returns>
    public static ChromeOptions InitializeBotPrefs(string DownloadFolderPath)
    {
        ChromeOptions options = new();
        // add preferenced download folder to options preferences.
        options.AddUserProfilePreference("download.default_directory", DownloadFolderPath);
        // to allow for multiple downloads and prevent the browser from blocking them 'allow multiple downloads' prombt
        options.AddUserProfilePreference("disable-popup-blocking", "true");
        options.PageLoadStrategy = PageLoadStrategy.Normal;
        return options;
    }


    /// <summary>
    /// Finds a single element on the webpage by a given mechanism (link text or XPath).
    /// </summary>
    /// <param name="ElementString">The string to use for finding the element.</param>
    /// <param name="ByMechanism">The mechanism to use for finding the element (Link Text or XPath).</param>
    /// <param name="AncestorElement">The ancestor element to search for the desired element in. If null, the entire webpage is searched.</param>
    /// <returns>The found element, or null if not found.</returns>
    public IWebElement? FindPageElement(string ElementString, string ByMechanism, IWebElement? AncestorElement = null)
    {
        IWebElement element;
        try
        {
            if (AncestorElement != null)
            {
                element = ByMechanism switch
                {
                    "xp" => AncestorElement.FindElement(By.XPath(ElementString)),
                    _ => throw new NotImplementedException(""),
                };
            }
            else
            {
                element = ByMechanism switch
                {
                    "name" => _driver.FindElement(By.Name(ElementString)),
                    "id" => _driver.FindElement(By.Id(ElementString)),
                    "css" => _driver.FindElement(By.CssSelector(ElementString)),
                    "class" => _driver.FindElement(By.ClassName(ElementString)),
                    "lt" => _driver.FindElement(By.LinkText(ElementString)),
                    "xp" => _driver.FindElement(By.XPath(ElementString)),
                    _ => throw new NotImplementedException(""),
                };
            }
            return element;

        }
        //ElementString was null
        catch (ArgumentNullException)
        {
            throw new BotElementException("ElementString argument is null.");
        }
        // No element was found by FindElement() with the designated 'ByMechanism'
        catch (NoSuchElementException)
        {
            throw new BotElementException($"No element called '{ElementString}' was found by FindElement() with by mechanism '{ByMechanism}'.");

        }
        // Thrown when the ElementString is syntactically invalid for the valid chosen ByMechanism, eg missing a [] in xp. 
        catch (InvalidSelectorException)
        {
            throw new BotMechanismException($"The 'ElementString': {ElementString} did not match to the designated 'ByMechanism': {ByMechanism}");
        }

        // The 'ByMechanism' parameter did not match any defined ByMechanism
        catch (NotImplementedException)
        {
            throw new BotMechanismException($"The 'ByMechanism': {ByMechanism} did not match any defined ByMechanism");
        }
    }

    public IList<string> FindPageElements(string ElementString, string ByMechanism)
    {

        try
        {
            //string list for element ID's 
            // find each element. 
            IList<IWebElement> elementList = ByMechanism switch
            {
                "name" => _driver.FindElements(By.Name(ElementString)),
                "id" => _driver.FindElements(By.Id(ElementString)),
                "css" => _driver.FindElements(By.CssSelector(ElementString)),
                "class" => _driver.FindElements(By.ClassName(ElementString)),
                "lt" => _driver.FindElements(By.LinkText(ElementString)),
                "xp" => _driver.FindElements(By.XPath(ElementString)),
                _ => throw new NotImplementedException(""),
            };
            foreach (IWebElement e in elementList)
            {

                string name = e.Text;
                if (string.IsNullOrEmpty(name))
                {
                    _nameList.Add("null");
                }
                else
                {
                    _nameList.Add(name);
                }

            }
            // if the element is not null add it to the list. 
            return _nameList;

        }
        //ElementString was null
        catch (ArgumentNullException)
        {
            throw new BotElementException("ElementString argument is null.");
        }
        // No element was found by FindElement() with the designated 'ByMechanism'
        catch (NoSuchElementException)
        {
            throw new BotElementException($"No element called '{ElementString}' was found by FindElement() with by mechanism '{ByMechanism}'.");

        }
        // The 'ElementString' paramater did not match to the designated 'ByMechanism'
        catch (InvalidSelectorException)
        {
            throw new BotMechanismException($"The 'ElementString': {ElementString} did not match to the designated 'ByMechanism': {ByMechanism}");
        }

        // The 'ByMechanism' paramater did not match any defined ByMechanism
        catch (NotImplementedException)
        {
            throw new BotMechanismException($"The 'ByMechanism': {ByMechanism} did not match any defined ByMechanism");
        }
    }




    /// <summary>
    /// Navigates to the webpage specified by the 'Url' field.
    /// </summary>
    /// <remarks>
    /// Will throw a <see cref="BotUrlException"/> if the 'Url' field is null.
    /// Will throw a <see cref="BotUrlException"/> if the webpage could not be loaded and the URL is invalid.
    /// </remarks>
    public void GoToWebpage()
    {
        try
        {
            _driver.Navigate().GoToUrl(Url);
        }


        //if URL is null
        catch (ArgumentNullException ex)
        {
            throw new BotUrlException("URL was null", ex);
        }

        //if webpage is not found, URL may be wrong. 
        catch (WebDriverArgumentException ex)
        {
            throw new BotUrlException("Webpage could not be loaded, URL may be invalid", ex);
        }
        // if the browser is already closed
        catch (WebDriverException)
        {
            throw new BotDriverException("webdriver failed to access website due to the browser already being closed");
        }
        // if the driver is already closed
        catch (ObjectDisposedException)
        {
            throw new BotDriverException("webdriver failed to access website due to the webdriver alredy being closed");
        }
    }

    /// <summary>
    /// Waits until the given element is displayed.
    /// </summary>
    /// <param name="element">The element to wait for.</param>
    public bool WaitTillExists(IWebElement? element)
    {
        if (element != null)
        {
            _wait.Until(driver => element.Displayed);
            return true;
        }
        return false;
    }

    /// <summary>
    /// Attempts to click the given element.
    /// </summary>
    /// <param name="element">The element to attempt to click.</param>
    /// <exception cref="BotElementException">Thrown if the element data is stale.</exception>
    public static void ClickElement(IWebElement? element)
    {
        try
        {
            element?.Click();
        }
        catch (StaleElementReferenceException)
        {
            throw new BotElementException("Referenced element data is stale. Check element state before attempting to click");
        }
    }
    /// <summary>
    /// Goes back to the previous webpage via the driver object .
    /// </summary>
    public void GoBack()
    {
        _driver.Navigate().Back();
    }

    /// <summary>
    /// Only closes the browser instance of the Bot.
    /// </summary>
    public void CloseBotBrowser()
    {
        _driver.Close();
    }


    /// <summary>
    /// Only stops the webdriver instance of the Bot .
    /// </summary>
    public void CloseBotDriver()
    {
        _driver.Dispose();
    }

    /// <summary>
    ///  Closes and stops both the browser and the webdriver instance of the Bot. 
    /// </summary>
    public void CloseBot()
    {
        _driver.Dispose();
    }
}