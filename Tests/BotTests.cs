namespace Tests;

using LEGO_Brickster_AI;
using Xunit;
using Xunit.Abstractions;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Chrome;

/* Quick Decision Guide  for tests

- Single test case, no parameters? → Use [Fact]
- Same test, multiple inputs? → Use [Theory] with [InlineData]
- Complex or reusable test data? → Use [Theory] with [MemberData] or [ClassData]
- Need to categorize tests? → Add [Trait] to any test
*/

public sealed class BotTest(ITestOutputHelper output)
{
    private const string TestDownloadFolderPath = @"..\..\..\LEGO_Data";

    private const string TestUrl_1 = "https://library.ldraw.org/omr/sets";

    private const string TestUrl_2 = "https://www.google.com";

    private const string TestUrl_3 = "https://www.bricklink.com/v3/studio/gallery.page";

    private readonly ITestOutputHelper _output = output;



    [Fact]
    public void InitializeBotTest()
    {
        Bot basicBot = new(TestUrl_1);
        Bot configuredBot = new(TestUrl_1, TestDownloadFolderPath);
        try
        {
            Assert.NotNull(basicBot.Driver);
            Assert.NotNull(configuredBot.Driver);
            string? testAbsDownloadFolderPath = Bot.GetAbsoluteDownloadFolderPath(TestDownloadFolderPath);
            Assert.NotNull(testAbsDownloadFolderPath);
            Assert.Equal(testAbsDownloadFolderPath, configuredBot.AbsDownloadFolderPath);
            basicBot.CloseBot();
        }
        finally
        {
            configuredBot.CloseBot();
        }
    }

    /// <summary>
    /// This function is useful for making preliminary actions on a test webpage.
    /// The different actions needs to be implemented on a case by case basis.
    /// Additionally, this function is also needed for the BotTests class.
    /// </summary>
    /// <param name="TestUrl">The URL of the webpage to access and make preliminary actions on.</param>
    /// <param name="ConfiguredBot">The configured Bot object to use for accessing the webpage and making preliminary actions.</param>
    private static void AccessTestWebPage(string TestUrl, Bot ConfiguredBot)
    {
        ConfiguredBot.GoToWebpage();

        //parse Testurl to make preliminary actions on specific test webpage. 
        switch (TestUrl)
        {
            case TestUrl_2:
                //press reject to cookies on google.com if TestUrl_2 is utilized
                IWebElement? RejectButton = ConfiguredBot.FindPageElement("//button[@id='W0wltc']", "xp");
                Assert.NotNull(RejectButton);
                Bot.ClickElement(RejectButton);
                break;

            case TestUrl_3:
                ChromeDriver testdriver = ConfiguredBot.Driver;
                Actions actionsBuilder = new(testdriver);
                // find and click the ageGateElement
                IWebElement? ageGateElement = ConfiguredBot.FindPageElement("//input[@class='blp-age-gate__input-field']", "xp");
                if (ConfiguredBot.WaitTillExists(ageGateElement))
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
                IWebElement? cookieButton = ConfiguredBot.FindPageElement("//div[@class='cookie-notice__content']//button[contains(text(), 'Just necessary')]", "xp");
                if (ConfiguredBot.WaitTillExists(cookieButton))
                {
                    Bot.ClickElement(cookieButton);
                }
                break;

            default:
                break;
        }
    }
    private static void FindPageElementException(Bot TestBot, string? FirstElementString, string FirstByMechanism, string? SecondElementString, string? SecondByMechanism,bool UseAncestorElementPattern = false)
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
    private static void FindPageElementsException(Bot TestBot, string? ElementString, string ByMechanism)
    {
        // Forgive possible null reference for ElementString
        TestBot.FindPageElements(ElementString!, ByMechanism);
    }

    [Fact]
    public void GoToWebpageTest()
    {
        Bot basicBot = new(TestUrl_1);
        try
        {
            basicBot.GoToWebpage();
            Assert.Equal(TestUrl_1, basicBot.Driver.Url);
            basicBot.CloseBot();
        }
        catch (BotUrlException)
        {
            basicBot.CloseBot();
        }
        catch (BotDriverException)
        {
            basicBot.CloseBot();
        }
    }
    [Theory]
    [InlineData("tableSearch", "name")]
    [InlineData("main-logo", "id")]
    [InlineData(".fi-ta-header-heading", "css")]
    [InlineData("fi-ta-header-heading", "class")]
    [InlineData("Metroliner", "lt")]
    [InlineData("//img[@id='main-logo']", "xp")]
    public void FindElementTest(string ElementString, string ByMechanism)
    {
        Bot configuredBot = new(TestUrl_1, TestDownloadFolderPath);
        configuredBot.GoToWebpage();
        try
        {
            IWebElement? pageElement = configuredBot.FindPageElement(ElementString, ByMechanism);
            // make sure the IWeb element is not null.
            Assert.NotNull(pageElement);
            string? elementOuter = pageElement.GetAttribute("outerHTML");
            _output.WriteLine($"{ElementString} outer HTML via {ByMechanism}:\n {elementOuter}\n------------------------------------------------\n");
            configuredBot.CloseBot();
        }
        finally
        {
            configuredBot.CloseBot();
        }
    }

    [Theory]
    [InlineData(TestUrl_1, "//h2[@class='fi-ta-header-heading']", ".//following::a[@href='https://library.ldraw.org/omr/sets/657']", "xp", "xp")]
    public void FindElementWithAncestorTest(string TestUrl, string AncestorElementString, string DecendentElementString, string AncestorByMechanism, string DecendentByMechanism)
    {
        Bot configuredBot = new(TestUrl, TestDownloadFolderPath);

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
            configuredBot.GoToWebpage();

            // AncestorElement
            IWebElement? AncestorElement = configuredBot.FindPageElement(AncestorElementString, AncestorByMechanism);
            Assert.NotNull(AncestorElement);
            List<string> a_AttributeList = InterpolateByMechanism(AncestorElement, AncestorByMechanism);
            Assert.Equal(2, a_AttributeList.Count);
            _output.WriteLine($"The value of the ancestor element {a_AttributeList[0]} attribute:{a_AttributeList[1]}");


            //DecendentElement
            IWebElement? DecendentElement = configuredBot.FindPageElement(DecendentElementString, DecendentByMechanism, AncestorElement);
            Assert.NotNull(DecendentElement);
            IList<string> d_AttributeList = InterpolateByMechanism(DecendentElement, DecendentByMechanism);
            Assert.Equal(2, d_AttributeList.Count);
            _output.WriteLine($"The value of the decendent element {d_AttributeList[0]} attribute:{d_AttributeList[1]})\n------------------------------");
        }
        finally
        {
            configuredBot.CloseBot();
        }
    }

    [Theory]
    [InlineData(TestUrl_1, 1, "tableSearch", "name", "class")]
    [InlineData(TestUrl_1, 1, "main-logo", "id", "src")]
    [InlineData(TestUrl_1, 25, ".fi-ta-cell-name", "css")]
    [InlineData(TestUrl_1, 25, "fi-ta-cell-name", "class")]
    [InlineData(TestUrl_3, 1, "Bouldering Rock Wall", "lt")]
    [InlineData(TestUrl_3, 50, "//article[contains(@class,'card')]//div[@class='moc-card__designer-name']", "xp")]
    public void FindElementsTest(string TestUrl, int ExpectedElementAmount, string ElementString, string ByMechanism, string IdentifierAttribute = "Text")
    {
        Bot configuredBot = new(TestUrl, TestDownloadFolderPath);
        AccessTestWebPage(TestUrl, configuredBot);
        try
        {
            configuredBot.NameList = configuredBot.FindPageElements(ElementString, ByMechanism, IdentifierAttribute);
            _output.WriteLine($"{configuredBot.NameList.Count} of element(s) by{IdentifierAttribute} added to the Bot._nameList");
            _output.WriteLine("--------------------");
            foreach (string name in configuredBot.NameList)
            {
                _output.WriteLine(name);
            }
            _output.WriteLine("--------------------\n\n");
            Assert.Equal(ExpectedElementAmount, configuredBot.NameList.Count);
        }
        finally
        {
            configuredBot.CloseBot();
        }
    }

    /// <summary>
    /// A test to verify that the Bot can wait for and click an element on a webpage.
    /// </summary>
    /// <param name="TestUrl">The URL of the webpage to test.</param>
    /// <param name="ElementString">The string to use for finding the element.</param>
    /// <param name="Goback">Whether to go back after clicking the element.</param>
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

        Bot configuredBot = new(TestUrl, TestDownloadFolderPath);
        try
        {
            AccessTestWebPage(TestUrl, configuredBot);
            ClickElementTest(ElementString, configuredBot, Goback);
        }
        finally
        {
            configuredBot.CloseBot();
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



    [Fact]
    public void WaitTillExistsFalseTest()
    {
        Bot basic_bot = new(TestUrl_1);
        basic_bot.GoToWebpage();
        try
        {
            IWebElement? element = null;
            Assert.False(basic_bot.WaitTillExists(element));
        }
        finally
        {
            basic_bot.CloseBot();
        }
    }


    [Theory]
    [InlineData("//a[@href='https://library.ldraw.org/documentation']")]
    public void ClickElementExceptionTest(string elementString)
    {
        Bot basic_bot = new(TestUrl_1);
        basic_bot.GoToWebpage();
        IWebElement? element = basic_bot.FindPageElement(elementString, "xp");
        Assert.NotNull(element);
        try
        {
            basic_bot.GoToWebpage();
            Assert.Throws<BotElementException>(() => Bot.ClickElement(element));
        }
        finally
        {
            basic_bot.CloseBot();
        }
    }

    [Theory]
    // Call FindPageElement() and catch exception 
    [InlineData(TestUrl_1, null, "xp", null, "xp")]
    // Call FindPageElements() and catch exception
    [InlineData(TestUrl_1, null, "xp", null, "xp", true)]
    // Call FindPageElement() but with an ancestor element pattern and catch exception
    [InlineData(TestUrl_1, "//h2[@class='fi-ta-header-heading']", "xp", null, "xp", false, true)]
    public void ArgumentNullExceptionTest(string TestURl, string? FirstElementString, string FirstByMechanism, string? SecondElementString, string SecondByMechanism, bool UseFindElements = false, bool UseAncestorElementPattern = false)
    {
        Bot configuredBot = new(TestURl, TestDownloadFolderPath);
        AccessTestWebPage(TestURl, configuredBot);
        try
        {
            // if UseFindElements is true, we will check exceptions in FindElements() as opposed to FindElement()
            if (UseFindElements)
            {
                Assert.Throws<BotElementException>(() => FindPageElementsException(configuredBot, FirstElementString, FirstByMechanism));
            }
            else
            {
                Assert.Throws<BotElementException>(() => FindPageElementException(configuredBot, FirstElementString, FirstByMechanism, SecondElementString, SecondByMechanism,UseAncestorElementPattern));
            }
        }
        finally
        {
            configuredBot.CloseBot();
        }
    }


    /// <summary>
    /// Tests that the FindPageElement() function throws NoSuchElementException when an element is not found.
    /// Note that the FindElements() function never throws NoSuchElementException, but instead returns an empty collection, hence why it does not have case in this test. 
    /// </summary>
    [Theory]
    [InlineData(TestUrl_1, "NO_SUCH_ELEMENT", "lt")]
    public void NoSuchElementExceptionTest(string TestURl, string ElementString, string ByMechanism)
    {
        Bot basicBot = new(TestURl);
        AccessTestWebPage(TestURl, basicBot);
        try
        {
            Assert.Throws<BotElementException>(() => FindPageElementException(basicBot, ElementString, ByMechanism, null, null));
        }
        finally
        {
            basicBot.CloseBot();
        }
    }


    [Theory]
    // Call FindPageElement() and catch exception 
    [InlineData(TestUrl_1, "//h2[@class='fi-ta-header-heading'", "xp", null, null)]
    // Call FindPageElements() and catch exception
    [InlineData(TestUrl_1, "//h2[@class='fi-ta-header-heading'", "xp", null, null, true)]
    // Call FindPageElement() but with an ancestor element pattern and catch exception
    [InlineData(TestUrl_1, "//h2[@class='fi-ta-header-heading']", "xp", ".//following::a[@href='https://library.ldraw.org/omr/sets/657'", "xp", false, true)]
    public void InvalidSelectorTest(string TestURl, string FirstElementString, string FirstByMechanism, string? SecondElementString, string? SecondByMechanism, bool UseFindElements = false, bool UseAncestorElementPattern = false)
    {
        Bot configuredBot = new(TestURl, TestDownloadFolderPath);
        AccessTestWebPage(TestURl, configuredBot);
        try
        {
            // if UseFindElements is true, we will check exceptions in FindElements() as opposed to FindElement()
            if (UseFindElements)
            {
                Assert.Throws<BotMechanismException>(() => FindPageElementsException(configuredBot, FirstElementString, FirstByMechanism));
            }
            else
            {
                Assert.Throws<BotMechanismException>(() => FindPageElementException(configuredBot, FirstElementString, FirstByMechanism, SecondElementString, SecondByMechanism,UseAncestorElementPattern));
            }
        }
        finally
        {
            configuredBot.CloseBot();
        }
    }


    [Theory]
    // Call FindPageElement() and catch exception 
    [InlineData(TestUrl_1, "//h2[@class='fi-ta-header-heading']", "NOT_IMPLEMENTED_BY_MECHANISM", null, null)]
    // Call FindPageElements() and catch exception
    [InlineData(TestUrl_1, "//h2[@class='fi-ta-header-heading']", "NOT_IMPLEMENTED_BY_MECHANISM", null, null, true)]
    // Call FindPageElement() but with an ancestor element pattern and catch exception
    [InlineData(TestUrl_1, "//h2[@class='fi-ta-header-heading']", "xp", ".//following::a[@href='https://library.ldraw.org/omr/sets/657']", "NOT_IMPLEMENTED_BY_MECHANISM",false,true)]
    public void NotImplementedExceptionTest(string TestURl, string FirstElementString, string FirstByMechanism, string? SecondElementString, string? SecondByMechanism, bool UseFindElements = false,bool UseAncestorElementPattern = false)
    {
        Bot configuredBot = new(TestURl, TestDownloadFolderPath);
        AccessTestWebPage(TestURl, configuredBot);
        try
        {
            // if UseFindElements is true, we will check exceptions in FindElements() as opposed to FindElement()
            if (UseFindElements)
            {
                Assert.Throws<BotMechanismException>(() => FindPageElementsException(configuredBot, FirstElementString, FirstByMechanism));
            }
            else
            {
                Assert.Throws<BotMechanismException>(() => FindPageElementException(configuredBot, FirstElementString, FirstByMechanism, SecondElementString, SecondByMechanism,UseAncestorElementPattern));
            }
        }
        finally
        {
            configuredBot.CloseBot();
        }
    }

    [Theory]
    [InlineData("www.NotAWebsiteForBots.Bot.com")]
    public void InvalidUrlTest(string InvalidUrl)
    {
        Bot basicBot = new(InvalidUrl);
        try
        {
            Assert.Throws<BotUrlException>(basicBot.GoToWebpage);
        }
        finally
        {
            basicBot.CloseBot();
        }
    }

    [Theory]
    [InlineData(null)]
    public void NullUrlTest(string? NullUrl)
    {
        // use null forgiveness to actually make test possible 
        Bot basicBot = new(NullUrl!);
        try
        {
            Assert.Throws<BotUrlException>(basicBot.GoToWebpage);
        }
        finally
        {
            basicBot.CloseBot();
        }
    }

    [Fact]
    public void CloseBrowserTest()
    {
        Bot basicBot = new(TestUrl_1);
        try
        {
            basicBot.CloseBotBrowser();
            // use a Action type with delegator to reference the method
            Assert.Throws<BotDriverException>(basicBot.GoToWebpage);
        }
        finally
        {
            basicBot.CloseBot();
        }
    }

    [Fact]
    public void CloseDriverTest()
    {
        Bot basicBot = new(TestUrl_1);
        try
        {
            basicBot.CloseBotDriver();
            // use a Action type with delegator to reference the method
            Assert.Throws<BotDriverException>(basicBot.GoToWebpage);
        }
        finally
        {
            basicBot.CloseBot();
        }
    }

    [Fact]
    public void GetChromeOptionsTest()
    {
        Bot configuredBot = new(TestUrl_1, TestDownloadFolderPath);
        try
        {
            ChromeOptions? actualOptions = configuredBot.Options;
            Assert.NotNull(actualOptions);
        }
        finally
        {
            configuredBot.CloseBot();
        }
    }

    [Fact]
    public void GetNameListTest()
    {
        Bot configuredBot = new(TestUrl_1, TestDownloadFolderPath)
        {
            NameList = ["Test_NameList"]
        };
        try
        {
            string[] expectedList = ["Test_NameList"];
            Assert.NotNull(configuredBot.NameList);
            Assert.Equal(expectedList, configuredBot.NameList);
        }
        finally
        {
            configuredBot.CloseBot();
        }
    }
}
