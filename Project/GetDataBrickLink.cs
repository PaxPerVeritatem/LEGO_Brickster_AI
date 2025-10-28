namespace LEGO_Brickster_AI;

using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using DotNetEnv;
sealed class GetDataBrickLink : IGetData
{
    public static string Url { get; set; } = "https://identity.lego.com/en-US/login";
    public static string DownloadFolderPath => @"..\..\..\LEGO_Data\BrickLink_Data";

    public static string UserProfilePath => @"..\..\..\DriverProfile";



    // Global run Properties
    public static int MaxPage => 59;

    public static int PageLimit => 1;

    public static int ExpectedSetsPrPage => 50;

    public static int ExpectedElementClickDeviation => 0;

    public static int ExpectedElementClickAmount { get; set; } = ExpectedSetsPrPage * PageLimit - ExpectedElementClickDeviation;

    public static int ElementClickCounter { get; set; } = 0;




    // Custom run Properties 
    public static bool CustomRun => false;

    public static int StartFromPage => 1;

    public static string UrlPageVarient => "";



    public static void UseCustomStartingPage()
    {
        Url = $"{Url}{UrlPageVarient}{StartFromPage}";
        if (StartFromPage != MaxPage)
        {
            ExpectedElementClickAmount += ExpectedElementClickDeviation;
        }
    }


    public static void AccessWebPage(Bot bot, Dictionary<string, string>? ElementCandidatesDict = null)
    {
        try
        {
            // parse the .env file for the Username/email
            Env.TraversePath().Load();

            bot.GoToMainPage();
            Actions actionBuilder = new(bot.Driver);
            // find and click the ageGateElement
            IWebElement? ageGateElement = bot.FindPageElement("//input[@class='blp-age-gate__input-field']", "xp");
            if (bot.WaitTillExists(ageGateElement))
            {
                Bot.ClickElement(ageGateElement);
                actionBuilder.SendKeys("1");
                Thread.Sleep(1000);
                actionBuilder.Perform();

                actionBuilder.SendKeys("9");
                Thread.Sleep(1000);
                actionBuilder.Perform();

                actionBuilder.SendKeys("9");
                Thread.Sleep(1000);
                actionBuilder.Perform();

                actionBuilder.SendKeys("4");
                actionBuilder.Perform();
                Thread.Sleep(1000);


            }
            // find and press cookie button 
            IWebElement? cookieButton = bot.FindPageElement("//div[@class='cookie-notice__content']//button[contains(text(), 'Just necessary')]", "xp");
            if (bot.WaitTillExists(cookieButton))
            {
                actionBuilder.MoveToElement(cookieButton);
                Thread.Sleep(2000);
                actionBuilder.Click();
                actionBuilder.Perform();
            }

            // find and press login button
            IWebElement? LoginButton = FindDisplayedElement(bot, ElementCandidatesDict);

            //if the LoginButton is inside the burger menu
            if (bot.WaitTillExists(LoginButton) && LoginButton.GetAttribute("id") == "js-trigger-more")
            {

                // click the burger menu

                actionBuilder.MoveToElement(LoginButton);
                Thread.Sleep(2000);
                actionBuilder.Click();
                actionBuilder.Perform();

                //find the 'sign in' element in the burger menu
                LoginButton = bot.FindPageElement(".//button[contains(@class, 'login')]", "xp");
                if (bot.WaitTillExists(LoginButton))
                {
                    actionBuilder.MoveToElement(LoginButton);
                    Thread.Sleep(2000);
                    actionBuilder.Click();
                    actionBuilder.Perform();
                }
            }
            // if the 'sign' in button is on the main page. 
            else if (bot.WaitTillExists(LoginButton))
            {

                Bot.ClickElement(LoginButton);
                Thread.Sleep(2000);
            }

            // find the Username/email input element
            IWebElement? UsernameField = bot.FindPageElement("//input[@id='username']", "xp");
            if (bot.WaitTillExists(UsernameField))
            {
                actionBuilder.MoveToElement(UsernameField);
                Thread.Sleep(2000);
                actionBuilder.Click();
                actionBuilder.Perform();

            }

            /* Get the username/email from the .env file and enter it into the
               Username/email input element */
            string? envEmail = Environment.GetEnvironmentVariable("Email");
            if (envEmail != null)
            {
                foreach (char letter in envEmail)
                {
                    actionBuilder

                        .SendKeys(letter.ToString())
                        .Perform();
                    Thread.Sleep(250);
                }
            }
            // find and click continue button
            IWebElement? ContinueButton = bot.FindPageElement("//button[@type='submit']", "xp");
            if (bot.WaitTillExists(ContinueButton))
            {
                actionBuilder.MoveToElement(ContinueButton);
                Thread.Sleep(2000);
                actionBuilder.Click();
                actionBuilder.Perform();

            }

        }
        catch (BotUrlException ex)
        {
            Console.WriteLine($"Failed to load webpage: {ex.Message}");
        }
        catch (BotDriverException ex)
        {
            Console.WriteLine(ex.Message);
        }
        catch (BotFindElementException ex)
        {
            Console.WriteLine($"Failed to return an html element\n{ex.Message}");
        }
        catch (BotMechanismException ex)
        {
            Console.WriteLine($"By() mechanism is invalid: {ex.Message}\n");
        }
        catch (BotTimeOutException ex)
        {
            Console.WriteLine($"Bot waited _wait time before timing out waiting for an element to appear:{ex.Message}");
        }
        catch (BotStaleElementException ex)
        {
            Console.WriteLine($"{ex.Message}: Element was probably found but page responsiveness or reload caused staleness");
        }
    }


    public static void SetAttributeList(Bot bot, string CommonElementString, string CommonByMechanism, string IdentifierAttribute)
    {
        // Attempt to get the list of LEGO set names for the current main page
        bot.AttributeList = bot.FindPageElements(CommonElementString, CommonByMechanism, IdentifierAttribute);

    }


    public static void DownloadPageElements(Bot bot, string ByMechanism)
    {
        foreach (string IdentifierAttribute in bot.AttributeList)
        {
            try
            {
                // Attempt to find LEGO set LinkTest element
                IWebElement? setNameElement = bot.FindPageElement(IdentifierAttribute, ByMechanism);

                // if current LinkText is not null click the set 
                if (bot.WaitTillExists(setNameElement))
                {
                    Bot.ClickElement(setNameElement);
                    // add for each LEGO set. Should finally match 'downloadAmount'
                    ElementClickCounter += 1;
                }

                // Attempt to find 'Download Studio file' button element on LEGO set page
                IWebElement? downloadButtonElement = bot.FindPageElement("//button[contains(text(),'Download Studio file')]", "xp");

                if (bot.WaitTillExists(downloadButtonElement))
                {
                    // i belive we can forgive here since WaitTillExists checks for null element.
                    string downloadFileSubstring = Bot.GetFileName(downloadButtonElement!, "Text");

                    string downloadFilePath = Path.Combine(bot.AbsDownloadFolderPath!, downloadFileSubstring);

                    if (Bot.IsFileDownloaded(downloadFilePath))
                    {
                        // if the file has already been downloaded, skip it
                        continue;
                    }
                    else
                    {
                        Bot.ClickElement(downloadButtonElement);
                        // try to find the next download button
                        downloadButtonElement = bot.FindPageElement(".//button[contains(text(),'Download Studio file')]", "xp", downloadButtonElement);
                    }
                }
            }

            catch (BotFindElementException ex)
            {
                // if we cant find the downloadButtonElement there must either be 0 or we have clicked them all, or we have reached a 404 page. 
                Console.WriteLine(ex.Message);
                bot.GoBack();
            }
            catch (BotMechanismException ex)
            {
                Console.WriteLine($"By() mechanism is invalid: {ex.Message}\n");

            }
            // should be thrown in case of stale element or 404 page error.
            catch (BotStaleElementException)
            {
                bot.GoBack();
                bot.GoBack();
            }
        }
    }

    public static IWebElement FindDisplayedElement(Bot bot, Dictionary<string, string> ElementCandidatesDict)
    {
        foreach (KeyValuePair<string, string> Candiate in ElementCandidatesDict)
        {
            try
            {
                IWebElement? nextPageElement = bot.FindPageElement(Candiate.Key, Candiate.Value);
                if (nextPageElement != null && nextPageElement.Displayed)
                {
                    return nextPageElement;
                }
            }
            catch (BotFindElementException ex)
            {
                throw new BotFindElementException($"{ex.Message}: Element not found, trying next option.");
            }
            catch (BotTimeOutException ex)
            {
                throw new BotTimeOutException($"The referenced element was found but, it was not displayed on the webpage: {ex.Message}");
            }
        }
        throw new BotStaleElementException("The referenced element is no longer displayed on the webpage");
    }


    public static void GoToNextPage(Bot bot, IWebElement NextButtonElement)
    {
        try
        {
            // click next button if it is loaded. 
            if (bot.WaitTillExists(NextButtonElement))
            {
                // get the url of the driver before clicking the next button. 
                string oldUrl = bot.Driver.Url;
                Bot.ClickElement(NextButtonElement);

                // Bot should not proceed until the next page is fully loaded indicated by a change in the url.
                bot.ExplicitWait(oldUrl);
            }
            // reset the bot attribute list for next page of elements. 
            bot.AttributeList = [];
        }
        catch (BotStaleElementException ex)
        {
            Console.WriteLine(ex.Message);
        }
        catch (BotTimeOutException ex)
        {
            Console.WriteLine(ex.Message);
        }

    }
    public static bool AssertDownloadAmount()
    {
        try
        {
            if (ExpectedElementClickAmount == ElementClickCounter)
            {
                Console.WriteLine($"Expected {ExpectedElementClickAmount}, matched clicked {ElementClickCounter} set page elements");
                return true;
            }
            else
            {
                throw new BotDownloadAmountException($"Expected {ExpectedElementClickAmount}, but clicked {ElementClickCounter} set page elements");
            }
        }
        catch (BotDownloadAmountException ex)
        {
            Console.WriteLine($"Assumed amount of LEGO Sets was either not correct or something went wrong during clicking set elements:{ex.Message}");
            return false;
        }
    }

    // ---------------------------------------------------------------------------------------------------------------------------------------------//

    //process the Ldraw website LEGO sets and download them. 
    public static void ProcessData()
    {
        // initial check if run is custom or not
        if (CustomRun)
        {
            UseCustomStartingPage();
        }
        Bot bot = new(Url, DownloadFolderPath,UserProfilePath);

        // dict for the two Sigin button cases. 
        Dictionary<string, string> LoginCandiateDict = new() {
            {"//button[@id = 'js-trigger-sign-in']", "xp"},
            {"//button[@id = 'js-trigger-more']","xp"}
        };
        try
        {

            AccessWebPage(bot, LoginCandiateDict);
            for (int i = 0; i < PageLimit; i++)
            {
                SetAttributeList(bot, "//article[contains(@class,'card')]//a[@class='moc-card__name']", "xp", "href");

                DownloadPageElements(bot, "lt");

                //     // Find the Next button elements which works, considering page responsiveness
                //     IWebElement nextButtonElement = GetNextPageElement(bot);
                //     GoToNextPage(bot, nextButtonElement);
                // }
            }
        }
        catch (StaleElementReferenceException)
        {
            Console.WriteLine($"No more next buttons. Reached last page.");
        }
        finally
        {
            AssertDownloadAmount();
            bot.CloseBot();
        }
    }

    public static void TestDetection()
    {
        // initial check if run is custom or not
        if (CustomRun)
        {
            UseCustomStartingPage();
        }
        Bot bot = new(Url, DownloadFolderPath,UserProfilePath);
        // dict for the two Sigin button cases. 
        Dictionary<string, string> LoginButtonsCandiateDict = new() {
            {"//button[@id = 'js-trigger-sign-in']", "xp"},
            {"//button[@id = 'js-trigger-more']","xp"}
        };
        try
        {

            TestAccessWebPage(bot, LoginButtonsCandiateDict);
        }
        finally
        {
            bot.CloseBot();
        }
    }
    public static void TestAccessWebPage(Bot bot, Dictionary<string, string> ElementCandidatesDict)
    {
        try
        {
            // parse the .env file for the Username/email
            Env.TraversePath().Load();

            bot.GoToMainPage();
            Actions actionBuilder = new(bot.Driver);

            // wait for the login page to load. 
            string oldUrl = bot.Driver.Url;
            bot.ExplicitWait(oldUrl);
            //find the Username / email input element
            IWebElement? UsernameField = bot.FindPageElement("//input[@id='username']", "xp");

            if (bot.WaitTillExists(UsernameField))
            {
                actionBuilder.MoveToElement(UsernameField);
                actionBuilder.Click();
                actionBuilder.Perform();
            }

            //find and click continue button
            IWebElement? ContinueButton = bot.FindPageElement("//button[@type='submit']", "xp");
            if (bot.WaitTillExists(ContinueButton))
            {
                actionBuilder.MoveToElement(ContinueButton);

                actionBuilder.Click();
                actionBuilder.Perform();
            }
            Thread.Sleep(1000);
            //find and click signin button
            IWebElement? SignInButton = bot.FindPageElement("//button[@type='submit']", "xp");
            if (bot.WaitTillExists(ContinueButton))
            {
                actionBuilder.MoveToElement(SignInButton);
                actionBuilder.Click();
                actionBuilder.Perform();
            }
            Thread.Sleep(5000);
        }
        catch (BotUrlException ex)
        {
            Console.WriteLine($"Failed to load webpage: {ex.Message}");
        }
        catch (BotDriverException ex)
        {
            Console.WriteLine(ex.Message);
        }
        catch (BotFindElementException ex)
        {
            Console.WriteLine($"Failed to return an html element\n{ex.Message}");
        }
        catch (BotMechanismException ex)
        {
            Console.WriteLine($"By() mechanism is invalid: {ex.Message}\n");
        }
        catch (BotTimeOutException ex)
        {
            Console.WriteLine($"Bot waited _wait time before timing out waiting for an element to appear:{ex.Message}");
        }
        catch (BotStaleElementException ex)
        {
            Console.WriteLine($"{ex.Message}: Element was probably found but page responsiveness or reload caused staleness");
        }
    }

}