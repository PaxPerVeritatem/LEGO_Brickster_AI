namespace LEGO_Brickster_AI;

using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using SeleniumUndetectedChromeDriver;
using System;
using OpenQA.Selenium.Support.UI;


/// <summary>
/// A class which wraps and simplifies some functionality of the Selenium ChromeDriver class.
/// Currently not implemented with other stypes of driver (like FirefoxDriver).
/// To be implemented in the future
/// </summary>
public class Bot
{

    // ChromeDriver version must always match the version of chrome. 
    private readonly UndetectedChromeDriver _driver;
    public UndetectedChromeDriver Driver => _driver;

    private readonly ChromeOptions _options;
    public ChromeOptions Options => _options;

    private static readonly string _userProfileDir = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, @"..\..\..\DriverProfile"));
    private static readonly string _preferencesFilePath = $@"{_userProfileDir}\Default\Preferences";


    private readonly Dictionary<string, object>? prefs;


    private readonly WebDriverWait _wait;

    private List<string> _attributeList = [];
    public List<string> AttributeList
    {
        get => _attributeList;
        set => _attributeList = value;
    }

    public string Url { get; set; }

    private readonly List<string> _windowHandles;



    // get a string array of the all the chrome version folders. 
    private static readonly string[] _driverPathFolders = Directory.GetDirectories(
        Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".cache",
            "selenium",
            "chromedriver",
            "win64")
    );

    // The last index value should be the path to the latest version folder of chromedriver.exe
    private readonly string _driverPath = Path.Combine(_driverPathFolders[^1], "chromedriver.exe");


    private readonly string _absDownloadFolderPath;
    public string AbsDownloadFolderPath => _absDownloadFolderPath;


    public Bot(string url, string downloadFolderPath)
    {
        Url = url;

        // get the absolute path to the download folder from the relative provided download folder path
        _absDownloadFolderPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, downloadFolderPath));

        // initialize ChromeOptions
        _options = InitializeBotOptions();

        prefs = new()
        {
            ["download.default_directory"] = _absDownloadFolderPath,
            ["profile.default_content_setting_values"] = new Dictionary<string, object>
            {
                // allow multiple downloads at once. 
                ["multiple_downloads"] = true,
                // avoid manual download confirmations 
                ["automatic_downloads"] = true,
                // allow cookies for chrome.
                ["profile.cookie_controls_mode"] = false
            }
        };

        // construct driver with all arguments
        _driver = UndetectedChromeDriver.Create(
            options: _options,
            userDataDir: _userProfileDir,
            driverExecutablePath: _driverPath,
            prefs: prefs
            );

        // create new WebDriverWait for driver with a timeout of 5 seconds
        _wait = new(_driver, TimeSpan.FromSeconds(5));

        // List containing all tabs open. 
        _windowHandles = [.. _driver.WindowHandles];
    }

    /// <summary>
    /// Initializes Chrome options with preferences for the bot.
    /// allow multiple downloads and prevent the browser from blocking them with a 'allow multiple downloads' prompt,
    /// and set the page loading strategy to "Normal".
    /// If the <paramref name="DownloadFolderPath"/> parameter is null, the bot will use the default download folder. 
    /// </summary>
    /// <param name="DownloadFolderPath">The path to the default download folder for the bot.</param>
    /// <returns>The initialized Chrome options.</returns>
    public static ChromeOptions InitializeBotOptions()
    {
        ChromeOptions options = new()
        {
            PageLoadStrategy = PageLoadStrategy.Normal
        };
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
            throw new BotFindElementException("ElementString argument was null.");
        }
        // No element was found by FindElement() with the designated 'ByMechanism'
        catch (NoSuchElementException)
        {
            throw new BotFindElementException($"No element called '{ElementString}' was found by FindElement() with by mechanism '{ByMechanism}'.");

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
            throw new BotStaleElementException($"The element {ElementString} was stale due to page state:{ex}");
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
    /// <param name="AncestorElement"> optional ancestor element to search for the elements within.
    /// <returns>A list of strings representing the elements found.</returns>
    /// <exception cref="BotElementException">Thrown when the ElementString argument is null.</exception>
    /// <exception cref="BotMechanismException">Thrown when the ElementString did not match to the designated ByMechanism or the ByMechanism did not match any defined ByMechanism.</exception>
    public List<string> FindPageElements(string ElementString, string ByMechanism, string IdentifierAttribute, IWebElement? AncestorElement = null)
    {
        try
        {
            // will return empty collection if no elements are found, hence does not throw NoSuchElementException
            IList<IWebElement> elementList;
            if (AncestorElement != null)
            {
                elementList = ByMechanism switch
                {
                    "xp" => AncestorElement.FindElements(By.XPath(ElementString)),
                    _ => throw new NotImplementedException(""),
                };
            }
            else
            {
                elementList = ByMechanism switch
                {
                    "name" => _driver.FindElements(By.Name(ElementString)),
                    "id" => _driver.FindElements(By.Id(ElementString)),
                    "css" => _driver.FindElements(By.CssSelector(ElementString)),
                    "class" => _driver.FindElements(By.ClassName(ElementString)),
                    "lt" => _driver.FindElements(By.LinkText(ElementString)),
                    "xp" => _driver.FindElements(By.XPath(ElementString)),
                    _ => throw new NotImplementedException(""),
                };
            }
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
            throw new BotFindElementException("ElementString argument is null.");
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
    /// Checks if a file with the inferred filename exists in the bot's download folder.
    /// In some cases the actual filename may differ from the inferred filename,
    /// if ActualFileName is argument is provided, then it will make sure that the InferredFilename matches actual provided ActualFileName.
    /// </summary>
    public bool IsFileDownloaded(string InferredFilename)
    {
        string downloadFilePath = Path.Combine(AbsDownloadFolderPath, InferredFilename);
        if (File.Exists(downloadFilePath))
        {
            Console.WriteLine($"File by the name: {InferredFilename} already exsists in the download folder. Skipping download.\n");
            return true;
        }
        else
        {
            return false;
        }
    }



    public void GetAndRenameFile(string NewFileName)
    {
        try
        {
            // define a cancellation token with a timeout of 10 seconds
            CancellationTokenSource cts = new(TimeSpan.FromSeconds(10));

            //step 1) Get the path to the latest downloaded file in the designated Bot download folder and Wait until file download is confirmed 
            string currentFilePath = ConfirmFileDownload(cts.Token);
            string currentFileName = Path.GetFileName(currentFilePath);


            // step 2) Only rename the file if it is NOT already the desired name.
            if (currentFileName != NewFileName)
            {
                // The old file info object 
                FileInfo fileInfo = new(currentFilePath);

                // The new path is the download folder with the new file name 
                string newFilePath = Path.Combine(AbsDownloadFolderPath, NewFileName);

                // change the name of the file 
                fileInfo.MoveTo(newFilePath);
                Console.WriteLine($"Filename: {currentFileName} was changed to: {NewFileName}\n");

                // Redefine the NewFileName in case the new name from MoveTo() contains "(1)" 
                NewFileName = Path.GetFileName(newFilePath);
                // delete the new file if it is a duplicate 
                if (NewFileName.Contains("(1)"))
                {
                    Console.WriteLine($"Duplicate file found by name:{NewFileName}, deleting duplicate file.\n");
                    File.Delete(currentFilePath);
                }
            }
            Console.WriteLine($"{NewFileName} downloaded successfully.\n");
        }
        catch (IOException ex)
        {
            throw new BotFileRenameException($"The fullFileName:{NewFileName} may have included invalid characters for windows file names: {ex.Message}");
        }
        catch (BotFileDownloadException ex)
        {
            Console.WriteLine(ex.Message);
        }
    }


    private string ConfirmFileDownload(CancellationToken CancellationToken)
    {

        while (!CancellationToken.IsCancellationRequested)
        {
            try
            {

                string? currentFilePath = GetLastDownloadedFilePath();
                // If the download folder was empty prior to current download
                if (currentFilePath == null)
                {
                    Thread.Sleep(300);
                    continue;
                }

                // if the tmp file exsist, then we are still downloading or we caught the file name in the middle of downloading.  
                if (currentFilePath.EndsWith(".crdownload"))
                {
                    Thread.Sleep(300);
                    continue;
                }

                string crdownloadFilePath = currentFilePath + ".crdownload";
                // if we have finished downloading but the tmp file is not deleted yet by chrome
                if (File.Exists(crdownloadFilePath))
                {
                    Thread.Sleep(300);
                    continue;
                }

                // Final file is there but chrome might still have it locked
                if (File.Exists(currentFilePath))
                {
                    try
                    {
                        // may throw IOException if chrome still has the file locked
                        using FileStream stream = File.Open(currentFilePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
                        {
                            return currentFilePath;
                        }
                    }
                    catch (IOException)
                    {
                        Console.WriteLine($"File {Path.GetFileName(currentFilePath)} was still locked after download has finished");
                        Thread.Sleep(300);
                        continue;
                    }
                }
                return currentFilePath;

            }
            catch (BotFileDownloadException ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        throw new BotFileDownloadException("File download confirmation timed out. File might have taken too long to download or chrome locked the file indenfinetly and could not be opened \n File may still work .");
    }




    private string? GetLastDownloadedFilePath()
    {

        string[] filesArray = Directory.GetFiles(AbsDownloadFolderPath);
        string? latestModifiedFile = filesArray.MaxBy(file => File.GetLastWriteTime(file));
        if (latestModifiedFile != null)
        {
            return latestModifiedFile;
        }
        return null;
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
            if (element != null && _wait.Until(_driver => element.Displayed))
            {
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

    /// some combinations of actions may lead to the bot clicking elements which are not yet loaded on the page 
    /// or the bot go though its actions too fast and leads to attempting no longer valid actions.
    public void ExplicitWaitURL(string oldurl)
    {
        try
        {
            _wait.Until(_driver => _driver.Url != oldurl);
        }
        // should catch in case the element is not displayed due to website responsiveness
        catch (WebDriverTimeoutException)
        {
            throw new BotTimeOutException();
        }
    }



    /// <summary>
    /// Attempts to navigate to a webpage based on the provided URL.
    /// </summary>
    /// <param name="Url">The URL of the webpage to navigate to.</param>
    /// <exception cref="BotUrlException">Thrown if the URL is null or if the webpage could not be loaded due to an invalid URL.</exception>
    /// <exception cref="BotDriverException">Thrown if the browser is already closed or if the webdriver is already closed.</exception>
    public void GoToWebPage(string Url)
    {
        try
        {
            _driver.GoToUrl(Url);
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
    /// Attempts to click the referenced IWebElement. If the IWebElement referenced is stale, a BotElementException will be thrown.
    /// </summary>
    /// <param name="element">The IWebElement to click.</param>
    /// <exception cref="BotElementException">Thrown if the referenced element data is stale.</exception>
    public void ClickElement(IWebElement? element)
    {
        try
        {
            if (WaitTillExists(element))
            {
                element!.Click();
            }
        }
        catch (StaleElementReferenceException)
        {
            throw new BotStaleElementException("Referenced element data is stale. Check element state before attempting to click");
        }
    }



    /// <summary>
    /// Opens a new tab with the webpage referenced by the href attribute of the referenced IWebElement.
    /// </summary>
    /// <param name="element">The IWebElement with the href attribute to use for opening the new tab.</param>
    /// <exception cref="BotStaleElementException">Thrown if the referenced element data is stale.</exception>
    /// <remarks>
    /// If the IWebElement referenced is null, the function does nothing.
    /// If the IWebElement referenced does not have an href attribute, the function does nothing.
    /// </remarks>
    public void OpenTabWithElement(IWebElement? element)
    {
        try
        {
            if (element != null && element.GetAttribute("href") != null)
            {
                string elementLink = element.GetAttribute("href")!;
                _driver.SwitchTo().NewWindow(WindowType.Tab);
                GoToWebPage(elementLink);
            }
        }
        catch (StaleElementReferenceException)
        {
            throw new BotStaleElementException("Referenced element data is stale. Check element state before attempting to click");
        }
    }

    /// <summary>
    /// This function is aids in defineing which chrome tab the OS considers active.
    /// It does not visually change the tab on the screen, but only changes the context window of the bot. 
    /// Should be used in conjunction with creating new tabs, where closing a tab is not required. 
    /// </summary>
    /// <param name="TabToSwitchTo"></param>
    public void SwitchTab(int TabToSwitchTo)
    {
        try
        {
            _driver.SwitchTo().Window(_windowHandles[TabToSwitchTo]);
        }
        catch (NoSuchWindowException)
        {
            throw new BotWindowException($"The specified tab:{TabToSwitchTo}, does not exist.");
        }
        catch (ArgumentNullException)
        {
            throw new BotFindElementException("'TabToSwitchTo' argument was null.");
        }
    }

    /// <summary>
    /// Closes the current tab and switches to the tab specified by 'TabToSwitchTo'.
    /// If the tab to switch to does not exist, a BotWindowException is thrown.
    /// </summary>
    /// <param name="TabToSwitchTo">The tab to switch to after closing the current tab.</param>
    /// <exception cref="BotWindowException">Thrown when the tab to switch to does not exist.</exception>
    public void CloseTab(int TabToSwitchTo)
    {
        try
        {
            _driver.Close();
            SwitchTab(TabToSwitchTo);
        }
        catch (BotWindowException)
        {
            Console.WriteLine("Tab to switch to does not exist.");
        }
        catch (BotFindElementException)
        {
            Console.WriteLine("'TabToSwitchTo' argument was null.");
        }

    }


    /// <summary>
    /// Goes back to the previous webpage in the browser history.
    /// </summary>
    public void GoBack()
    {

        _driver.Navigate().Back();
    }


    /// <summary>
    ///  Closes and stops both the browser and the webdriver instance of the Bot. 
    /// </summary>
    public void CloseBot()
    {
        _driver.Quit();
    }



    /// <summary>
    ///  Cleans up the preferences file used by the bot to store download preferences.
    ///  Should be called before bot creation and after bot closure to ensure no stale preferences interfere with future bot runs.
    /// </summary>
    public static void CleanupPreferencesFile()
    {
        try
        {
            if (File.Exists(_preferencesFilePath))
            {
                File.Delete(_preferencesFilePath);
                Console.WriteLine("Preferences file cleaned up");
            }
        }
        catch (Exception ex)
        {
            // Silently fail - we're exiting anyway
            Console.WriteLine($"Cleanup warning: {ex.Message}");
        }
    }
}
