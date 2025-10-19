namespace LEGO_Brickster_AI;

using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using OpenQA.Selenium.Support.UI;

/// <summary>
/// A class which wraps and simplifies some functionality of the Selenium ChromeDriver class.
/// Currently not implemented with other stypes of driver (like FirefoxDriver).
/// To be implemented in the future
/// </summary>
public class Bot
{
    private readonly ChromeDriver _driver;
    public ChromeDriver Driver => _driver;

    private readonly ChromeOptions? _options;
    public ChromeOptions? Options => _options;

    private readonly WebDriverWait _wait;

    private IList<string> _attributeList = [];
    public IList<string> AttributeList
    {
        get => _attributeList;
        set => _attributeList = value;
    }


    public string Url { get; set; }

    private readonly string? _absDownloadFolderPath;
    public string? AbsDownloadFolderPath => _absDownloadFolderPath;




    /// <summary>
    /// Initializes a new instance of the <see cref="Bot"/> class with the given URL and optional download folder path.
    /// </summary>
    /// <param name="url">The URL of the webpage to access.</param>
    /// <param name="downloadFolderPath">The path to the default download folder for the bot. If null, the default download folder is used.</param>
    /// <remarks>
    /// If <paramref name="downloadFolderPath"/> is not null, the bot will use the specified download folder path.
    /// Otherwise, the bot will use the default download folder path.
    /// </remarks>
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
            _options = InitializeBotPrefs();
            _driver = new ChromeDriver();
        }

        _wait = new(_driver, TimeSpan.FromSeconds(2));
    }


    /// <summary>
    /// Returns the absolute path to the download folder by combining the provided relative download folder path with the application's base directory.
    /// </summary>
    /// <param name="DownloadFolderPath">The relative path to the download folder.</param>
    /// <returns>The absolute path to the download folder.</returns>
    public static string GetAbsoluteDownloadFolderPath(string DownloadFolderPath)
    {
        string absDownloadFolderPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, DownloadFolderPath));
        return absDownloadFolderPath;
    }




    /// <summary>
    /// Initializes Chrome options with preferences for the bot.
    /// allow multiple downloads and prevent the browser from blocking them with a 'allow multiple downloads' prompt,
    /// and set the page loading strategy to "Normal".
    /// If the <paramref name="DownloadFolderPath"/> parameter is null, the bot will use the default download folder. 
    /// </summary>
    /// <param name="DownloadFolderPath">The path to the default download folder for the bot.</param>
    /// <returns>The initialized Chrome options.</returns>
    public static ChromeOptions InitializeBotPrefs(string? DownloadFolderPath = null)
    {
        ChromeOptions options = new();
        if (DownloadFolderPath != null)
        {
            // add preferenced download folder to optionsPreferences.
            options.AddUserProfilePreference("download.default_directory", DownloadFolderPath);
            // to allow for multiple downloads and prevent the browser from blocking them 'allow multiple downloads' prombt
            options.AddUserProfilePreference("disable-popup-blocking", "true");
            options.PageLoadStrategy = PageLoadStrategy.Normal;
        }
        else
        {
            options.AddUserProfilePreference("disable-popup-blocking", "true");
            options.PageLoadStrategy = PageLoadStrategy.Normal;
        }
        return options;
    }



    /// <summary>
    /// Finds a page element based on the ElementString and ByMechanism provided.
    /// If AncestorElement is not null, the function will search for the element within the AncestorElement.
    /// </summary>
    /// <param name="ElementString">The string to use for finding the element.</param>
    /// <param name="ByMechanism">The mechanism to use for finding the element, such as By.Name, By.Id, By.CssSelector, etc.</param>
    /// <param name="AncestorElement">The ancestor element to search for the element within. If null, search the entire webpage.</param>
    /// <returns>The found element, or null if no element was found.</returns>
    /// <exception cref="BotElementException">Thrown when the ElementString argument is null.</exception>
    /// <exception cref="BotMechanismException">Thrown when the ElementString did not match to the designated ByMechanism or the ByMechanism did not match any defined ByMechanism.</exception>
    /// <exception cref="NoSuchElementException">Thrown when no element was found by FindElement() with the designated 'ByMechanism'.</exception>
    /// <exception cref="InvalidSelectorException">Thrown when the ElementString is syntactically invalid for the valid chosen ByMechanism, eg missing a [] in xp.</exception>
    /// <exception cref="NotImplementedException">Thrown when the 'ByMechanism' parameter did not match any defined ByMechanism.</exception>
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
        // if the element is stale due to page state.  
        catch (StaleElementReferenceException ex)
        {
            throw new BotException($"The element {ElementString} was stale due to page state:{ex}");
        }
    }

    /// <summary>
    /// Finds a list of elements on a webpage based on the ByMechanism and ElementString provided.
    /// The list of elements is then iterated over and the attribute specified by IdentifierAttribute is added to the Bot._nameList for each element.
    /// If IdentifierAttribute is not provided, the Text of the element is utilized as default.
    /// </summary>
    /// <param name="ElementString">The string to use for finding the elements.</param>
    /// <param name="ByMechanism">The mechanism to use for finding the elements, such as By.Name, By.Id, By.CssSelector, etc.</param>
    /// <param name="IdentifierAttribute">The attribute of the element to use when adding to the Bot._nameList. If not provided, uses the text of the element.</param>
    /// <returns>A list of strings representing the elements found.</returns>
    /// <exception cref="BotElementException">Thrown when the ElementString argument is null.</exception>
    /// <exception cref="BotMechanismException">Thrown when the ElementString did not match to the designated ByMechanism or the ByMechanism did not match any defined ByMechanism.</exception>
    public IList<string> FindPageElements(string ElementString, string ByMechanism, string IdentifierAttribute = "Text")
    {

        try
        {
            // will return empty collection if not elements are found, hence does not throw NoSuchElementException
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

            if (IdentifierAttribute != "Text")
            {
                foreach (IWebElement e in elementList)
                {
                    string? elementAspect = e.GetAttribute(IdentifierAttribute);
                    if (!string.IsNullOrEmpty(elementAspect))
                    {
                        _attributeList.Add(elementAspect);
                    }
                }
            }
            else
            {
                foreach (IWebElement e in elementList)
                {
                    string? elementText = e.Text;
                    if (!string.IsNullOrEmpty(elementText))
                    {
                        _attributeList.Add(elementText);
                    }
                }
            }
            return _attributeList;
        }
        //ElementString was null
        catch (ArgumentNullException)
        {
            throw new BotElementException("ElementString argument is null.");
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
    /// Navigates to the webpage specified by the URL property.
    /// </summary>
    /// <exception cref="BotUrlException">Thrown when the URL is null or invalid.</exception>
    /// <exception cref="BotDriverException">Thrown when the webdriver failed to access the website due to the browser already being closed or the webdriver already being closed.</exception>
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
    /// Waits until the referenced IWebElement exists on the webpage.
    /// If the IWebElement referenced is null, the function will return false.
    /// Note that if the driver is instanceiated without a generous PageLoadStrategy, 
    /// some combinations of actions may lead to the blot clicking elements which are not yet loaded on the page 
    /// or the bot go though its actions to fast and leads to attempting no longer valid actions.
    /// </summary>
    /// <param name="element">The IWebElement to wait for.</param>
    /// <returns>true if the element is found, false if the element is null.</returns>
    public bool WaitTillExists(IWebElement? element)
    {
        try
        {
            if (element != null)
            {
                _wait.Until(_driver => element.Displayed);
                return true;
            }
            return false;
        }
        // should catch in case the element is not displayed due to website responsiveness
        catch (WebDriverTimeoutException)
        {
            throw new BotTimeOutException();
        }
    }




    /// <summary>
    /// Attempts to click the referenced IWebElement. If the IWebElement referenced is stale, a BotElementException will be thrown.
    /// </summary>
    /// <param name="element">The IWebElement to click.</param>
    /// <exception cref="BotElementException">Thrown if the referenced element data is stale.</exception>
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

    public static string GetFileName(IWebElement DownloadButtonElement, string ElementAttribute)
    {
        try
        {
            string? downloadFileName = DownloadButtonElement.GetAttribute(ElementAttribute);
            if (downloadFileName != null)
            {
                string downloadFileSubstring = Path.GetFileName(downloadFileName);
                return downloadFileSubstring;
            }
            return "";
        }
        catch (StaleElementReferenceException)
        {
            throw new BotElementException("Referenced element data is stale. Check element state before attempting to click");
        }
    }

    public static bool IsFileDownloaded(string DownloadFilePath)
    {
        if (File.Exists(DownloadFilePath))
        {
            return true;

        }
        else
        {
            return false;
        }
    }


    // wait until a certain element is present on the page. 
    public void ExplicitWait()
    {
        string oldUrl = _driver.Url;
        _wait.Until(_driver => _driver.Url != oldUrl);
    }

    /// <summary>
    /// Goes back to the previous webpage in the browser history.
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
        _driver.Quit();
    }
}
