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
    /// Initializes a new instance of the <see cref="Bot"/> class with the URL of the webpage to navigate to.
    /// </summary>
    /// <param name="url">The URL of the webpage to navigate to.</param>
    public Bot(string url)
    {
        Url = url;
        _driver = new ChromeDriver();

        // driver must be instantiated before wait can utilize it.  
        _wait = new(_driver, TimeSpan.FromSeconds(2));
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Bot"/> class, with a pre-defined download folder preference.
    /// </summary>
    /// <param name="url">The URL of the webpage to navigate to.</param>
    /// <param name="downloadFolderPath">The path to the download folder.</param>

    public Bot(string url, string downloadFolderPath)
    {
        Url = url;
        _absDownloadFolderPath = GetAbsoluteDownloadFolderPath(downloadFolderPath);
        _options = InitializeBotPrefs(_absDownloadFolderPath);
        _driver = new ChromeDriver(_options);

        // driver must be instantiated before wait can utilize it.  
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
                    "Name" => AncestorElement.FindElement(By.Name(ElementString)),
                    "ID" => AncestorElement.FindElement(By.Id(ElementString)),
                    "CSS" => AncestorElement.FindElement(By.CssSelector(ElementString)),
                    "CLASSNAME" => AncestorElement.FindElement(By.ClassName(ElementString)),
                    "LT" => AncestorElement.FindElement(By.LinkText(ElementString)),
                    "XP" => AncestorElement.FindElement(By.XPath(ElementString)),
                    _ => throw new NotImplementedException(""),
                };
            }
            else
            {
                element = ByMechanism switch
                {
                    "NAME" => _driver.FindElement(By.Name(ElementString)),
                    "ID" => _driver.FindElement(By.Id(ElementString)),
                    "CSS" => _driver.FindElement(By.CssSelector(ElementString)),
                    "CLASSNAME" => _driver.FindElement(By.ClassName(ElementString)),
                    "LT" => _driver.FindElement(By.LinkText(ElementString)),
                    "XP" => _driver.FindElement(By.XPath(ElementString)),
                    _ => throw new NotImplementedException(""),
                };
            }
            Console.WriteLine($"{element.Text} found \n");
            return element;

        }
        // if ElementString was null
        catch (ArgumentNullException)
        {
            throw new BotElementException("ElementString argument is null.");
        }
        // No element was found by FinElement() with the designated 'ByMechanism'
        catch (NoSuchElementException)
        {
            throw new BotElementException($"No element called '{ElementString}' was found by FindElement() with by mechanism '{ByMechanism}'.");

        }
        // The 'ByMechanism' paramater did not match any By class mechanisms
        catch (NotImplementedException)
        {
            throw new BotMechanismException($"mechanism '{ByMechanism}' cannot be passed to By().");
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
                "NAME" => _driver.FindElements(By.Name(ElementString)),
                "ID" => _driver.FindElements(By.Id(ElementString)),
                "CSS" => _driver.FindElements(By.CssSelector(ElementString)),
                "CLASSNAME" => _driver.FindElements(By.ClassName(ElementString)),
                "LT" => _driver.FindElements(By.LinkText(ElementString)),
                "XP" => _driver.FindElements(By.XPath(ElementString)),
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
        // if ElementString was null
        catch (ArgumentNullException)
        {
            throw new BotElementException("ElementString argument is null.");
        }
        // No element was found by FindElement() with the designated 'ByMechanism'
        catch (NoSuchElementException)
        {
            throw new BotElementException($"No element called '{ElementString}' was found by FindElement() with by mechanism '{ByMechanism}'.");
        }
        // The 'ByMechanism' paramater did not match any By class mechanisms
        catch (NotImplementedException)
        {
            throw new BotMechanismException($"mechanism '{ByMechanism}' cannot be passed to By().");
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
    public void CloseBrowser()
    {
        _driver.Close();
    }


    /// <summary>
    /// Only stops the webdriver instance of the Bot .
    /// </summary>
    public void CloseDriver()
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