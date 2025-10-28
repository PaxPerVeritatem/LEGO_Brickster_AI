namespace Tests;

using LEGO_Brickster_AI;
using Xunit;
using Xunit.Abstractions;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Chrome;


public sealed class BotTest(ITestOutputHelper output)
{
    // some simple consts for testing. Can be added too. 
    private const string TestDownloadFolderPath = @"..\..\..\LEGO_Data";

    private const string TestUserProfilePath = @"Tests\DriverProfile";

    private const string TestUrl_1 = "https://library.ldraw.org/omr/sets";

    private const string TestUrl_2 = "https://www.google.com";

    private const string TestUrl_3 = "https://www.bricklink.com/v3/studio/gallery.page";

    // an Xunit test output helper which allows to output debug information in the console during the test
    private readonly ITestOutputHelper _output = output;



    /// <summary>
    /// A test to verify that the Bot class can be initialized successfully with or without a download folder path.
    /// </summary>
    /// <remarks>
    /// Verifies that the Bot class can be initialized successfully with or without a download folder path.
    /// It also verifies that the absolute download folder path retrieved from the Bot class is as expected.
    /// </remarks>
    [Fact]
    public void InitializeBotTest()
    {
        Bot bot = new(TestUrl_1, TestDownloadFolderPath,TestUserProfilePath);
        try
        {
            Assert.NotNull(bot.Driver);
            Assert.NotNull(bot.AbsDownloadFolderPath);
            Assert.Equal(bot.AbsDownloadFolderPath,Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, TestDownloadFolderPath)) );
            bot.CloseBot();
        }
        finally
        {
            bot.CloseBot();
        }
    }


    /// <summary>
    /// A private helper function to define the different cases and preliminary actions nessesary to access the main page of a test web site.
    /// </summary>
    /// <param name="TestUrl">The URL of the test webpage.</param>
    /// <param name="bot">The configured bot instance.</param>
    private static void AccessTestWebPage(Bot bot,string TestUrl)
    {
        bot.GoToMainPage();

        //parse Testurl to make preliminary actions on specific test webpage. 
        switch (TestUrl)
        {
            case TestUrl_2:
                //press reject to cookies on google.com if TestUrl_2 is utilized
                IWebElement? RejectButton = bot.FindPageElement("//button[@id='W0wltc']", "xp");
                Assert.NotNull(RejectButton);
                Bot.ClickElement(RejectButton);
                break;

            case TestUrl_3:
                ChromeDriver testdriver = bot.Driver;
                Actions actionsBuilder = new(testdriver);
                // find and click the ageGateElement
                IWebElement? ageGateElement = bot.FindPageElement("//input[@class='blp-age-gate__input-field']", "xp");
                if (bot.WaitTillExists(ageGateElement))
                {
                    Bot.ClickElement(ageGateElement);
                    actionsBuilder.SendKeys("1");
                    actionsBuilder.SendKeys("9");
                    actionsBuilder.SendKeys("9");
                    actionsBuilder.SendKeys("4");
                    actionsBuilder.Perform();
                    actionsBuilder.Reset();
                }
                // find and press cookie button 
                IWebElement? cookieButton = bot.FindPageElement("//div[@class='cookie-notice__content']//button[contains(text(), 'Just necessary')]", "xp");
                if (bot.WaitTillExists(cookieButton))
                {
                    Bot.ClickElement(cookieButton);
                }
                break;

            default:
                break;
        }
    }
    /// <summary>
    /// A privat helper function. Utilized for cases where FindPageElement() should throw some exception.
    /// </summary>
    /// <param name="TestBot">The bot instance to test.</param>
    /// <param name="FirstElementString">The first element string to use for finding the element.</param>
    /// <param name="FirstByMechanism">The first mechanism to use for finding the element.</param>
    /// <param name="SecondElementString">The second element string to use for finding the element, if applicable.</param>
    /// <param name="SecondByMechanism">The second mechanism to use for finding the element, if applicable.</param>
    /// <param name="UseAncestorElementPattern">Whether to use the ancestor element pattern matching or not. Default to false.</param>
    private static void FindPageElementException(Bot TestBot, string? FirstElementString, string FirstByMechanism, string? SecondElementString, string? SecondByMechanism, bool UseAncestorElementPattern = false)
    {
        // if we are testing for ancestor element pattern matching
        if (UseAncestorElementPattern)
        {
            // Forgive possible null reference for FirstElementString
            IWebElement? FirstElement = TestBot.FindPageElement(FirstElementString!, FirstByMechanism);
            Assert.NotNull(FirstElement);
            // Forgive possible null reference for SecondElementString and SecondByMechanism
            TestBot.FindPageElement(SecondElementString!, SecondByMechanism!, FirstElement);
        }
        else
        {
            // Forgive possible null reference for FirstElementString
            TestBot.FindPageElement(FirstElementString!, FirstByMechanism);
        }
    }
    /// <summary>
    /// A private helper function. Utilized for cases where FindPageElements() should throw some exception.
    /// </summary>
    /// <param name="TestBot">The bot instance to test.</param>
    /// <param name="ElementString">The element string to use for finding the elements.</param>
    /// <param name="ByMechanism">The mechanism to use for finding the elements.</param>
    private static void FindPageElementsException(Bot TestBot, string? ElementString, string ByMechanism)
    {
        // Forgive possible null reference for ElementString
        TestBot.FindPageElements(ElementString!, ByMechanism);
    }

    /// <summary>
    /// Tests that GoToMainPage() correctly navigates to a webpage and that the Bot.Driver.Url is updated accordingly.
    /// </summary>
    /// <remarks>
    /// Asserts that the Bot.Driver.Url is equal to TestUrl_1 after calling GoToMainPage().
    /// </remarks>
    [Fact]
    public void GoToMainPageTest()
    {
        Bot bot = new(TestUrl_1,TestDownloadFolderPath,TestUserProfilePath);
        try
        {
            bot.GoToMainPage();
            Assert.Equal(TestUrl_1, bot.Driver.Url);
            bot.CloseBot();
        }
        catch (BotUrlException)
        {
            bot.CloseBot();
        }
        catch (BotDriverException)
        {
            bot.CloseBot();
        }
    }
    /// <summary>
    /// Tests the Bot.FindPageElement() method with various inputs and their respective ByMechanism.
    /// </summary>
    /// <remarks>
    /// Asserts that the Bot.FindPageElement() method returns a non-null IWebElement given the ElementString and ByMechanism.
    /// </remarks>
    [Theory]
    [InlineData("tableSearch", "name")]
    [InlineData("main-logo", "id")]
    [InlineData(".fi-ta-header-heading", "css")]
    [InlineData("fi-ta-header-heading", "class")]
    [InlineData("Metroliner", "lt")]
    [InlineData("//img[@id='main-logo']", "xp")]
    public void FindElementTest(string ElementString, string ByMechanism)
    {
        Bot bot = new(TestUrl_1,TestDownloadFolderPath,TestUserProfilePath);
        bot.GoToMainPage();
        try
        {
            IWebElement? pageElement = bot.FindPageElement(ElementString, ByMechanism);
            // make sure the IWeb element is not null.
            Assert.NotNull(pageElement);
            string? elementOuter = pageElement.GetAttribute("outerHTML");
            _output.WriteLine($"{ElementString} outer HTML via {ByMechanism}:\n {elementOuter}\n------------------------------------------------\n");
            bot.CloseBot();
        }
        finally
        {
            bot.CloseBot();
        }
    }

    /// <summary>
    /// Tests that Bot.FindPageElement() correctly finds an element when given an ancestor element string and its corresponding ByMechanism.
    /// </summary>
    /// <param name="TestUrl">The URL of the webpage to test.</param>
    /// <param name="AncestorElementString">The string to use for finding the ancestor element.</param>
    /// <param name="DecendentElementString">The string to use for finding the dependent element.</param>
    /// <param name="AncestorByMechanism">The mechanism to use for finding the ancestor element.</param>
    /// <param name="DecendentByMechanism">The mechanism to use for finding the dependent element.</param>
    [Theory]
    [InlineData(TestUrl_1, "//h2[@class='fi-ta-header-heading']", ".//following::a[@href='https://library.ldraw.org/omr/sets/657']", "xp", "xp")]
    public void FindElementWithAncestorTest(string TestUrl, string AncestorElementString, string DecendentElementString, string AncestorByMechanism, string DecendentByMechanism)
    {
        Bot bot = new(TestUrl, TestDownloadFolderPath,TestUserProfilePath);

        //local function
        static List<string> InterpolateByMechanism(IWebElement element, string elementByMechanism)
        {
            // use null forgiveness in order to test
            List<string> elementAttributeList = [];
            string? attributeString;
            attributeString = element.GetAttribute("class");
            elementAttributeList.Add("class");
            elementAttributeList.Add(attributeString!);
            return elementAttributeList;
        }
        try
        {
            bot.GoToMainPage();

            // AncestorElement
            IWebElement? AncestorElement = bot.FindPageElement(AncestorElementString, AncestorByMechanism);
            Assert.NotNull(AncestorElement);
            List<string> a_AttributeList = InterpolateByMechanism(AncestorElement, AncestorByMechanism);
            Assert.Equal(2, a_AttributeList.Count);
            _output.WriteLine($"The value of the ancestor element {a_AttributeList[0]} attribute:{a_AttributeList[1]}");


            //DecendentElement
            IWebElement? DecendentElement = bot.FindPageElement(DecendentElementString, DecendentByMechanism, AncestorElement);
            Assert.NotNull(DecendentElement);
            IList<string> d_AttributeList = InterpolateByMechanism(DecendentElement, DecendentByMechanism);
            Assert.Equal(2, d_AttributeList.Count);
            _output.WriteLine($"The value of the decendent element {d_AttributeList[0]} attribute:{d_AttributeList[1]})\n------------------------------");
        }
        finally
        {
            bot.CloseBot();
        }
    }

    /// <summary>
    /// Tests the Bot.FindPageElements() method with various inputs and their respective ByMechanism.
    /// </summary>
    /// <param name="TestUrl">The URL of the webpage to access.</param>
    /// <param name="ExpectedElementAmount">The expected amount of elements to find.</param>
    /// <param name="ElementString">The string to use for finding the elements.</param>
    /// <param name="ByMechanism">The mechanism to use for finding the elements, such as By.Name, By.Id, By.CssSelector, etc.</param>
    /// <param name="IdentifierAttribute">The attribute of the element to use when adding to the Bot._nameList. If not provided, uses the text of the element.</param>
    /// <remarks>
    /// Asserts that the Bot.FindPageElements() method returns a non-null IWebElement given the ElementString and ByMechanism.
    /// </remarks>
    [Theory]
    [InlineData(TestUrl_1, 1, "tableSearch", "name", "class")]
    [InlineData(TestUrl_1, 1, "main-logo", "id", "src")]
    [InlineData(TestUrl_1, 25, ".fi-ta-cell-name", "css")]
    [InlineData(TestUrl_1, 25, "fi-ta-cell-name", "class")]
    [InlineData(TestUrl_3, 1, "Bouldering Rock Wall", "lt")]
    [InlineData(TestUrl_3, 50, "//article[contains(@class,'card')]//div[@class='moc-card__designer-name']", "xp")]
    public void FindElementsTest(string TestUrl, int ExpectedElementAmount, string ElementString, string ByMechanism, string IdentifierAttribute = "Text")
    {
        Bot bot = new(TestUrl, TestDownloadFolderPath,TestUserProfilePath);
        AccessTestWebPage(bot,TestUrl);
        try
        {
            bot.AttributeList = bot.FindPageElements(ElementString, ByMechanism, IdentifierAttribute);
            _output.WriteLine($"{bot.AttributeList.Count} of element(s) by{IdentifierAttribute} added to the Bot._nameList");
            _output.WriteLine("--------------------");
            foreach (string name in bot.AttributeList)
            {
                _output.WriteLine(name);
            }
            _output.WriteLine("--------------------\n\n");
            Assert.Equal(ExpectedElementAmount, bot.AttributeList.Count);
        }
        finally
        {
            bot.CloseBot();
        }
    }



    /// <summary>
    /// Tests the Bot.WaitTillExists() method with various inputs.
    /// </summary>
    /// <param name="TestUrl">The URL of the webpage to access.</param>
    /// <param name="ElementString">The string to use for finding the element.</param>
    /// <param name="Goback">Whether the bot should go back to the previous webpage after clicking the element.</param>
    /// <remarks>
    /// Important to use a configured bot and not a basic bot, since the configured bot defines a wait strategy in the Bot.InitializeBotPrefs(). 
    /// Using a basic bot can sometimes incur that the Bot will click elements which are not loaded yet or actions are performed too early before an element is loaded.
    /// </remarks>
    [Theory]
    [InlineData(TestUrl_1, "//a[@href='https://library.ldraw.org/documentation']", true)]
    [InlineData(TestUrl_2, "//div[contains(@class,'FPdoLc')]//input[@class='RNmpXc']", true)]
    [InlineData(TestUrl_3, "//div[@class='studio-staff-picks__bar l-margin-top--md']", false)]
    public void WaitTilElementExistsTest(string TestUrl, string ElementString, bool Goback)
    {
        /// <note>
        /// Important to use a configured bot and not a basic bot, since the configured bot defines a wait strategy in the Bot.InitializeBotPrefs(). 
        /// Using a basic bot can sometimes incur that the Bot will click elements which are not loaded yet.
        /// </note>

        Bot bot = new(TestUrl, TestDownloadFolderPath,TestUserProfilePath);
        try
        {
            AccessTestWebPage(bot,TestUrl);
            ClickElementTest(ElementString, bot, Goback);
        }
        finally
        {
            bot.CloseBot();
        }
        static void ClickElementTest(string ElementString, Bot testBot, bool GoBack)
        {
            IWebElement? clickableElement;
            if (GoBack)
            {
                for (int i = 0; i < 2; i++)
                {
                    clickableElement = testBot.FindPageElement(ElementString, "xp");
                    Assert.NotNull(clickableElement);
                    if (testBot.WaitTillExists(clickableElement))
                    {
                        Bot.ClickElement(clickableElement);
                        testBot.GoBack();
                    }
                }
            }
            else
            {
                clickableElement = testBot.FindPageElement(ElementString, "xp");
                if (testBot.WaitTillExists(clickableElement))
                {
                    Bot.ClickElement(clickableElement);
                }
            }
        }
    }
    /// <summary>
    /// Tests whether WaitTillExists correctly returns false when the provided element is null.
    /// </summary>
    [Fact]
    public void WaitTillExistsFalseTest()
    {
        Bot bot = new(TestUrl_1,TestDownloadFolderPath,TestUserProfilePath);
        bot.GoToMainPage();
        try
        {
            IWebElement? element = null;
            Assert.False(bot.WaitTillExists(element));
        }
        finally
        {
            bot.CloseBot();
        }
    }
    /// <summary>
    /// Tests whether the ClickElement() function throws a BotElementException when the referenced element is stale.
    /// </summary>
    /// <param name="elementString">The XPath string to use for finding the element.</param>
    [Theory]
    [InlineData("//a[@href='https://library.ldraw.org/documentation']")]
    public void ClickElementExceptionTest(string elementString)
    {
        Bot bot = new(TestUrl_1,TestDownloadFolderPath,TestUserProfilePath);
        bot.GoToMainPage();
        IWebElement? element = bot.FindPageElement(elementString, "xp");
        Assert.NotNull(element);
        try
        {
            bot.GoToMainPage();
            Assert.Throws<BotStaleElementException>(() => Bot.ClickElement(element));
        }
        finally
        {
            bot.CloseBot();
        }
    }

    /// <summary>
    /// Tests that the FindPageElement() and FindPageElements() functions throw the correct exceptions when null arguments are passed.
    /// </summary>
    /// <param name="TestURl">The URL to test.</param>
    /// <param name="FirstElementString">The XPath string to use for finding the first element.</param>
    /// <param name="FirstByMechanism">The ByMechanism to use for finding the first element.</param>
    /// <param name="SecondElementString">The XPath string to use for finding the second element if UseAncestorElementPattern is true.</param>
    /// <param name="SecondByMechanism">The ByMechanism to use for finding the second element if UseAncestorElementPattern is true.</param>
    /// <param name="UseFindElements">Whether to test FindPageElements() or FindPageElement().</param>
    /// <param name="UseAncestorElementPattern">Whether to test FindPageElement() with an ancestor element pattern.</param>
    [Theory]
    // Call FindPageElement() and catch exception 
    [InlineData(TestUrl_1, null, "xp", null, "xp")]
    // Call FindPageElements() and catch exception
    [InlineData(TestUrl_1, null, "xp", null, "xp", true)]
    // Call FindPageElement() but with an ancestor element pattern and catch exception
    [InlineData(TestUrl_1, "//h2[@class='fi-ta-header-heading']", "xp", null, "xp", false, true)]
    public void ArgumentNullExceptionTest(string TestUrl, string? FirstElementString, string FirstByMechanism, string? SecondElementString, string SecondByMechanism, bool UseFindElements = false, bool UseAncestorElementPattern = false)
    {
        Bot bot = new(TestUrl, TestDownloadFolderPath,TestUserProfilePath);
        AccessTestWebPage(bot,TestUrl);
        try
        {
            // if UseFindElements is true, we will check exceptions in FindElements() as opposed to FindElement()
            if (UseFindElements)
            {
                Assert.Throws<BotFindElementException>(() => FindPageElementsException(bot, FirstElementString, FirstByMechanism));
            }
            else
            {
                Assert.Throws<BotFindElementException>(() => FindPageElementException(bot, FirstElementString, FirstByMechanism, SecondElementString, SecondByMechanism, UseAncestorElementPattern));
            }
        }
        finally
        {
            bot.CloseBot();
        }
    }



    /// <summary>
    /// Tests whether the FindPageElement() and FindPageElements() functions throw a NoSuchElementException when the referenced element does not exist.
    /// </summary>
    /// <param name="TestUrl">The URL to test.</param>
    /// <param name="ElementString">The XPath string to use for finding the element.</param>
    /// <param name="ByMechanism">The ByMechanism to use for finding the element.</param>
    [Theory]
    [InlineData(TestUrl_1, "NO_SUCH_ELEMENT", "lt")]
    public void NoSuchElementExceptionTest(string TestUrl, string ElementString, string ByMechanism)
    {
        Bot bot = new(TestUrl,TestDownloadFolderPath,TestUserProfilePath);
        AccessTestWebPage(bot,TestUrl);
        try
        {
            Assert.Throws<BotFindElementException>(() => FindPageElementException(bot, ElementString, ByMechanism, null, null));
        }
        finally
        {
            bot.CloseBot();
        }
    }


    /// <summary>
    /// Tests whether the FindPageElement() and FindPageElements() functions throw a BotMechanismException when the ElementString is syntactically invalid for the valid chosen ByMechanism, eg missing a [] in xp.
    /// </summary>
    /// <param name="TestURl">The URL to test.</param>
    /// <param name="FirstElementString">The XPath string to use for finding the first element.</param>
    /// <param name="FirstByMechanism">The ByMechanism to use for finding the first element.</param>
    /// <param name="SecondElementString">The XPath string to use for finding the second element if UseAncestorElementPattern is true.</param>
    /// <param name="SecondByMechanism">The ByMechanism to use for finding the second element if UseAncestorElementPattern is true.</param>
    /// <param name="UseFindElements">Whether to test FindPageElements() or FindPageElement().</param>
    /// <param name="UseAncestorElementPattern">Whether to test FindPageElement() with an ancestor element pattern.</param>
    [Theory]
    // Call FindPageElement() and catch exception 
    [InlineData(TestUrl_1, "//h2[@class='fi-ta-header-heading'", "xp", null, null)]
    // Call FindPageElements() and catch exception
    [InlineData(TestUrl_1, "//h2[@class='fi-ta-header-heading'", "xp", null, null, true)]
    // Call FindPageElement() but with an ancestor element pattern and catch exception
    [InlineData(TestUrl_1, "//h2[@class='fi-ta-header-heading']", "xp", ".//following::a[@href='https://library.ldraw.org/omr/sets/657'", "xp", false, true)]
    public void InvalidSelectorTest(string TestUrl, string FirstElementString, string FirstByMechanism, string? SecondElementString, string? SecondByMechanism, bool UseFindElements = false, bool UseAncestorElementPattern = false)
    {
        Bot bot = new(TestUrl, TestDownloadFolderPath,TestUserProfilePath);
        AccessTestWebPage(bot,TestUrl);
        try
        {
            // if UseFindElements is true, we will check exceptions in FindElements() as opposed to FindElement()
            if (UseFindElements)
            {
                Assert.Throws<BotMechanismException>(() => FindPageElementsException(bot, FirstElementString, FirstByMechanism));
            }
            else
            {
                Assert.Throws<BotMechanismException>(() => FindPageElementException(bot, FirstElementString, FirstByMechanism, SecondElementString, SecondByMechanism, UseAncestorElementPattern));
            }
        }
        finally
        {
            bot.CloseBot();
        }
    }


    /// <summary>
    /// Tests whether the FindPageElement() and FindPageElements() functions throw a NotImplementedExceptionException when the ElementString does not match to the designated ByMechanism.
    /// </summary>
    /// <param name="TestURl">The URL to test.</param>
    /// <param name="FirstElementString">The XPath string to use for finding the first element.</param>
    /// <param name="FirstByMechanism">The ByMechanism to use for finding the first element.</param>
    /// <param name="SecondElementString">The XPath string to use for finding the second element if UseAncestorElementPattern is true.</param>
    /// <param name="SecondByMechanism">The ByMechanism to use for finding the second element if UseAncestorElementPattern is true.</param>
    /// <param name="UseFindElements">Whether to test FindPageElements() or FindPageElement().</param>
    /// <param name="UseAncestorElementPattern">Whether to test FindPageElement() with an ancestor element pattern.</param>
    [Theory]
    // Call FindPageElement() and catch exception 
    [InlineData(TestUrl_1, "//h2[@class='fi-ta-header-heading']", "NOT_IMPLEMENTED_BY_MECHANISM", null, null)]
    // Call FindPageElements() and catch exception
    [InlineData(TestUrl_1, "//h2[@class='fi-ta-header-heading']", "NOT_IMPLEMENTED_BY_MECHANISM", null, null, true)]
    // Call FindPageElement() but with an ancestor element pattern and catch exception
    [InlineData(TestUrl_1, "//h2[@class='fi-ta-header-heading']", "xp", ".//following::a[@href='https://library.ldraw.org/omr/sets/657']", "NOT_IMPLEMENTED_BY_MECHANISM", false, true)]
    public void NotImplementedExceptionTest(string TestUrl, string FirstElementString, string FirstByMechanism, string? SecondElementString, string? SecondByMechanism, bool UseFindElements = false, bool UseAncestorElementPattern = false)
    {
        Bot bot = new(TestUrl, TestDownloadFolderPath,TestUserProfilePath);
        AccessTestWebPage(bot,TestUrl);
        try
        {
            // if UseFindElements is true, we will check exceptions in FindElements() as opposed to FindElement()
            if (UseFindElements)
            {
                Assert.Throws<BotMechanismException>(() => FindPageElementsException(bot, FirstElementString, FirstByMechanism));
            }
            else
            {
                Assert.Throws<BotMechanismException>(() => FindPageElementException(bot, FirstElementString, FirstByMechanism, SecondElementString, SecondByMechanism, UseAncestorElementPattern));
            }
        }
        finally
        {
            bot.CloseBot();
        }
    }

    /// <summary>
    /// Tests whether the GoToMainPage() method throws a BotUrlException when an invalid URL is provided to the constructor.
    /// </summary>
    /// <param name="InvalidUrl">The invalid URL to test.</param>
    [Theory]
    [InlineData("www.NotAWebsiteForBots.Bot.com")]
    public void InvalidUrlTest(string InvalidUrl)
    {
        Bot bot = new(InvalidUrl,TestDownloadFolderPath,TestUserProfilePath);
        try
        {
            Assert.Throws<BotUrlException>(bot.GoToMainPage);
        }
        finally
        {
            bot.CloseBot();
        }
    }

    /// <summary>
    /// Tests whether the GoToMainPage() method throws a BotUrlException when a null URL is provided to the constructor.
    /// </summary>
    /// <param name="NullUrl">The null URL to test.</param>
    [Theory]
    [InlineData(null)]
    public void NullUrlTest(string? NullUrl)
    {
        // use null forgiveness to actually make test possible 
        Bot bot = new(NullUrl!,TestDownloadFolderPath,TestUserProfilePath);
        try
        {
            Assert.Throws<BotUrlException>(bot.GoToMainPage);
        }
        finally
        {
            bot.CloseBot();
        }
    }

    /// <summary>
    /// Tests whether the Bot class throws a BotDriverException when calling GoToMainPage() after calling CloseBotBrowser().
    /// </summary>
    [Fact]
    public void CloseBrowserTest()
    {
        Bot bot = new(TestUrl_1,TestDownloadFolderPath,TestUserProfilePath);
        try
        {
            bot.CloseBotBrowser();
            // use a Action type with delegator to reference the method
            Assert.Throws<BotDriverException>(bot.GoToMainPage);
        }
        finally
        {
            bot.CloseBot();
        }
    }

    /// <summary>
    /// Tests whether the Bot class throws a BotDriverException when calling GoToMainPage() after calling CloseBotDriver().
    /// </summary>
    [Fact]
    public void CloseDriverTest()
    {
        Bot bot = new(TestUrl_1,TestDownloadFolderPath,TestUserProfilePath);
        try
        {
            bot.CloseBotDriver();
            // use a Action type with delegator to reference the method
            Assert.Throws<BotDriverException>(bot.GoToMainPage);
        }
        finally
        {
            bot.CloseBot();
        }
    }

/// <summary>
/// Tests whether the Bot class exposes the ChromeOptions used to initialize the ChromeDriver instance.
/// </summary>
    [Fact]
    public void GetChromeOptionsTest()
    {
        Bot bot = new(TestUrl_1,TestDownloadFolderPath,TestUserProfilePath);
        try
        {
            ChromeOptions? actualOptions = bot.Options;
            Assert.NotNull(actualOptions);
        }
        finally
        {
            bot.CloseBot();
        }
    }

        /// <summary>
        /// Tests whether the Bot class exposes the NameList property, which contains a list of names found on the webpage.
        /// </summary>
    [Fact]
    public void GetNameListTest()
    {
        Bot bot = new(TestUrl_1,TestDownloadFolderPath,TestUserProfilePath)
        {
            AttributeList = ["Test_NameList"]
        };
        try
        {
            string[] expectedList = ["Test_NameList"];
            Assert.NotNull(bot.AttributeList);
            Assert.Equal(expectedList, bot.AttributeList);
        }
        finally
        {
            bot.CloseBot();
        }
    }
}
