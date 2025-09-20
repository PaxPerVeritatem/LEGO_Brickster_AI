namespace LEGO_Brickster_AI;

using System.ComponentModel.DataAnnotations;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

public class Bot
{
    private readonly ChromeDriver driver;
    private readonly WebDriverWait wait;
    private IList<string> nameList = [];
    public IList<string> NameList
    {
        get { return nameList; }
        set { nameList = value; }
    }
    public string Url { get; set; }




    /// <summary>
    /// Initializes a new instance of the <see cref="Bot"/> class with the URL of the webpage to navigate to.
    /// </summary>
    /// <param name="url">The URL of the webpage to navigate to.</param>
    public Bot(string url)
    {
        Url = url;
        driver = new();

        // driver must be instantiated before wait can utilize it.  
        wait = new(driver, TimeSpan.FromSeconds(2));
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Bot"/> class, with a pre-defined download folder preference.
    /// </summary>
    /// <param name="url">The URL of the webpage to navigate to.</param>
    /// <param name="Downloadfolderstring">The path to the download folder.</param>

    public Bot(string url, string Downloadfolderstring)
    {
        Url = url;
        ChromeOptions options = InitializeBotPrefs(Downloadfolderstring);
        driver = new(options);

        // driver must be instantiated before wait can utilize it.  
        wait = new(driver, TimeSpan.FromSeconds(2));
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ChromeOptions"/> class, with a pre-defined download folder preference.
    /// </summary>
    /// <param name="Downloadfolderstring">The path to the download folder.</param>
    /// <returns>A new instance of <see cref="ChromeOptions"/> with the pre-defined download folder preference.</returns>
    public static ChromeOptions InitializeBotPrefs(string Downloadfolderstring)
    {
        ChromeOptions options = new();
        // add preferenced download folder to options preferences.
        options.AddUserProfilePreference("download.default_directory", Downloadfolderstring);
        // to allow for multiple downloads and prevent the browser from blocking them 'allow multiple downloads' prombt
        options.AddUserProfilePreference("disable-popup-blocking", "true");
        // initialize bot with predefined preference
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
                    "CLASSNAME" => driver.FindElement(By.ClassName(ElementString)),
                    "LT" => AncestorElement.FindElement(By.LinkText(ElementString)),
                    "XP" => AncestorElement.FindElement(By.XPath(ElementString)),
                    _ => throw new NotImplementedException(""),
                };
            }
            else
            {
                element = ByMechanism switch
                {
                    "NAME" => driver.FindElement(By.Name(ElementString)),
                    "ID" => driver.FindElement(By.Id(ElementString)),
                    "CSS" => driver.FindElement(By.CssSelector(ElementString)),
                    "CLASSNAME" => driver.FindElement(By.ClassName(ElementString)),
                    "LT" => driver.FindElement(By.LinkText(ElementString)),
                    "XP" => driver.FindElement(By.XPath(ElementString)),
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
                "NAME" => driver.FindElements(By.Name(ElementString)),
                "ID" => driver.FindElements(By.Id(ElementString)),
                "CSS" => driver.FindElements(By.CssSelector(ElementString)),
                "CLASSNAME" => driver.FindElements(By.ClassName(ElementString)),
                "LT" => driver.FindElements(By.LinkText(ElementString)),
                "XP" => driver.FindElements(By.XPath(ElementString)),
                _ => throw new NotImplementedException(""),
            };
            foreach (IWebElement e in elementList)
            {

                string name = e.Text;
                if (string.IsNullOrEmpty(name))
                {
                    nameList.Add("null");
                }
                else
                {
                    nameList.Add(name);
                }

            }
            // if the element is not null add it to the list. 
            return nameList;

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





    /// <summary>
    /// Navigates to a webpage via the driver object.
    /// </summary>
    /// <param name="url">The URL of the webpage to navigate to.</param>
    /// <exception cref="BotUrlException">Thrown if the URL is null or the webpage could not be loaded.</exception>
    public void GoToWebpage(string url)
    {
        try
        {
            driver.Navigate().GoToUrl(url);
            Console.WriteLine($"Title of Webpage: {driver.Title}\n");
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
    public bool WaitIfExists(IWebElement? element)
    {
        if (element != null)
        {
            wait.Until(driver => element.Displayed);
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
        driver.Navigate().Back();
    }

    /// <summary>
    /// Closes the browser instance.
    /// </summary>
    public void CloseBrowser()
    {
        driver.Close();
    }

    public void StopBot()
    {
        driver.Quit();
    }
}