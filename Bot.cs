namespace LEGO_Brickster_AI;

using System;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

public class Bot
{
    //makes the driver readonly, by c# suggestions, for improved performance. 
    private readonly ChromeDriver driver = new();

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


    public void ImplicitWait(int waitTime)
    {
        // might need to be changed. 
        driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromMilliseconds(waitTime);

    }



    /// <summary>
    /// Finds an element on the webpage by a given mechanism (link text or XPath).
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

    public void FindWebElements()
    {
        try
        {
            IList<IWebElement> elementList = driver.FindElements(By.ClassName("fi-ta-cell-name"));

            int i = 1;
            foreach (IWebElement e in elementList)
            {
                Console.WriteLine($"html element.{i}:{e.Text}");
                i++;
            }
        }
        catch (System.Exception)
        {

            throw;
        }
    }

    public static void ClickElement(IWebElement element)
    {
        element.Click();
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