namespace LEGO_Brickster_AI;
using System;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

public class Bot
{
    //makes the driver readonly, by c# suggestions, for improved performance. 
    private readonly ChromeDriver driver = new();

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


    public void CloseBrowser()
    {
        driver.Close();
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
    /// <returns>The found element, or null if not found.</returns>
    public IWebElement? FindPageElement(string ElementString, string ByMechanism, IWebElement? element = null)
    {
        if (element == null)
        {
            try
            {
                element = ByMechanism switch
                {
                    "Name" => driver.FindElement(By.Name(ElementString)),
                    "ID" => driver.FindElement(By.Id(ElementString)),
                    "LT" => driver.FindElement(By.LinkText(ElementString)),
                    "XP" => driver.FindElement(By.XPath(ElementString)),
                    _ => throw new NotImplementedException(""),
                };
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
        else
        {
            try
            {
                element = ByMechanism switch
                {
                    "Name" => element.FindElement(By.Name(ElementString)),
                    "LT" => element.FindElement(By.LinkText(ElementString)),
                    "XP" => element.FindElement(By.XPath(ElementString)),
                    _ => throw new ArgumentException($"Unknown locator mechanism: {ByMechanism}"),
                };
            }
            catch (ArgumentException)
            {


            }
        }

        // Element LinkText value needs to be dynamic and also other By mechanism
        try
        {


            // If found, print the element text and return the element
            if (element != null)
            {
                Console.WriteLine($"{element.Text} found \n");
                return element;
            }
            else
            {
                // If not found, close the browser and print an error message
                Console.WriteLine("Element not found on webpage, check input or try another By object.");
                return null;
            }
        }
        catch (NoSuchElementException)
        {
            // If not found, close the browser and print an error message
            Console.WriteLine("Element not found on webpage, check input or try another By object.");
            return null;
        }
    }



    public IWebElement? CheckForMainModel()
    {
        try
        {
            IWebElement? element = FindPageElement("//div[contains(text(),'Main Model')]", "XP");

            if (element == null)
            {
                throw new ArgumentNullException(nameof(element), $"{element} cannot be null.");
            }
            else if (element.Text != "Main Model")
            {
                Console.WriteLine($"Incorrect element found: {element.Text}.");
                return null;
            }
            return element;
        }
        catch (ArgumentNullException)
        {
            Console.WriteLine("Element is null and cannot be checked.");
            return null;
        }

    }

    public IWebElement? FindDownloadElement(string ElementString, string ByMechanism, IWebElement mainModelElement)
    {
        if (CheckForMainModel() != null)
        {
            IWebElement? downloadButtonElement = FindPageElement(ElementString, ByMechanism, mainModelElement);
            return downloadButtonElement;
        }
        else
        {
            return null;
        }
    }

    public static void ClickElement(IWebElement element)
    {
        element.Click();
    }

}