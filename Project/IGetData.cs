using OpenQA.Selenium;

namespace LEGO_Brickster_AI;

// static abstract interface
interface IGetData
{

    /// <summary>
    /// The mutable string defining the main website url
    /// </summary>
    public static abstract string Url { get; set; }

    static abstract bool CustomRun { get; }
    /// <summary>
    /// int defining the int value of the page. Can be utilized if the website have several pages of individual data 
    /// </summary>
    static abstract int StartFromPage { get; }

    /// <summary>
    ///  int defining the number of sets of data pr page. Usage of this field depends on the structure of the website on question. 
    /// </summary>
    static abstract int ExpectedSetsPrPage { get; }


    static abstract int MaxPage { get; }

    /// <summary>
    /// The PageLimit defines the absolute amount of pages, where SetAttributeList() is called for each page, to add the 
    /// IdentifierAttribute for all CommonElementStrings to the bot.AttributeList 
    /// </summary>
    static abstract int PageLimit { get; }


    /// <summary>
    /// a substring serving as an extension the main website url exte which allows can be combined with <param name = "StartFromPage"> to begin a run from a certain subpage.  
    /// </summary>
    static abstract string UrlPageVarient { get; }


    /// <summary>
    /// Depending on the webpage, and its responsiveness, there might be several next page elements. Depending on the page responsiveness. 
    /// Keys represent the litteral ElementString  while the values is the ByMechanism string utilized by the FindElement function.   
    /// bot may fail to locate a certain NextPage element and throw a WebDriverTimeoutException, which should be handled appropricately, 
    /// allowing for the remaing buttons to be found in the case of webpage responsiveness issues. 
    /// </summary>
    static abstract Dictionary<string, string> NextPageElements { get; }

    /// <summary>
    /// A simple reference counter for asserting correct amount of elements have been clicked, when finally compared to <param name = "ExpectedElementClickAmount">
    static abstract int ElementClickCounter { get; set; }



    /// <summary>
    ///  The absolute path to the download folder, when the Bot class is initialized with a download folder path. Should be definied with enough .. to reach the the desired download folder path, from the application's .dll file directory.
    /// </summary>
    static abstract string DownloadFolderPath { get; }

    /// <summary>
    /// int defining the expected amount of elements to be clicked, inferred from <param name = "SetsPrPage"> and <param name = "PageLimit">
    /// </summary>
    static abstract int ExpectedElementClickAmount { get; set; }


    ///
    /// user defined deviation of the ExpectedElementClickAmount. Should be implemented by the user, after testing whether there some pages which have a different amount of sets.
    /// 
    static abstract int ExpectedElementClickDeviation { get; }



  // --------------------------------------------------------------------------------------------------------------------------------------------//
  // Functions


    /// <summary>
    /// Allows to define a custom starting page for a custom run, using the initial <param name = "url">
    /// with the <param name = "UrlPageVarient"> and param name = "_startFromPage"> to define the url of the custom run page. 
    /// Can be called initially before <strong> AccessWebPage() </strong> is called within <strong> ProcessData()</strong>
    /// </summary>
    public static abstract void UseCustomStartingPage();




    /// <summary>
    /// This function should access the main webpage via <c> bot.GoToWebPage(Url)</c> and perform any necessary bot actions to escape popups,
    ///  which may hinder access to the elements of the main page.  
    /// </summary>
    /// <param name="bot"></param>
    public static abstract void AccessWebPage(Bot bot);



    /// <summary>
    ///  Find all the element on the current page which fits the <paramref name="CommonElementString"/> with the 
    /// <paramref name="CommonByMechanism"/> and the optional <paramref name="IdentifierAttribute"/> and add them to the Bot.AttributeList. 
    /// Be aware that depending on the specific element the defined <paramref name="IdentifierAttribute"/> needs to be part of all the elements in question. 
    /// by default <paramref name="IdentifierAttribute"/> is set to 'Text' in bot.FindPageElements(). 
    /// </summary>
    /// <param name="bot"></param>
    /// <param name="CommonElementString"></param>
    /// <param name="CommonByMechanism"></param>
    public static abstract void SetAttributeList(Bot bot, string CommonElementString, string CommonByMechanism, string? IdentifierAttribute = null);


    /// <summary>
    /// Function to iterate over the NextPageElements&lt;string,string&gt; map's key-value pairs. 
    /// For each key-value pair it calls <c>bot.FindElement()</c> to locate a button on the webpage 
    /// which should function as a next button. The predetermined amount of tuples in the map should 
    /// be manually defined in the NextPageElements map.
    /// </summary>
    /// <param name="bot">The bot instance used to find and interact with page elements.</param>
    public static abstract IWebElement GetNextPageElement(Bot bot);


    /// <summary>
    /// Should perform the necessary bot actions to navigate to the next page by clicking the chosen 
    /// <c>NextButtonElement</c> from the <c>NextPageElements</c> dictionary.  
    /// </summary>
    /// <param name="bot">The bot instance to perform navigation.</param>
    /// <remarks>
    /// <para>Implementation Recommendation: The following is an implementation recommendation, but 
    /// users may define their own GotoNextPage() from scratch if they wish.</para>
    /// <list type="number">
    /// <item>
    /// <description>
    /// <strong>Wait for NextButtonElement to exist:</strong> Use <c>bot.WaitTillExists(NextButtonElement)</c> 
    /// before using <c>bot.ClickElement(NextButtonElement)</c> to ensure the NextButtonElement is present in the DOM.
    /// </description>
    /// 
    /// </item>
    /// <br/>
    /// <item>
    /// <description>
    /// <strong>Capture current Driver url:</strong> Get the bot's current url via <c>bot.Driver.Url</c> before clicking 
    /// the NextButtonElement. This will be used to detect when navigation completes. Take note, <c>bot.Driver.Url</c> is a dynamic property of the
    /// <c>Selenium webdriver</c>, and different from the <c>Bot.Url</c> property.
    /// </description>
    /// </item>
    /// <br/>
    /// <item>
    /// <description>
    /// <strong>Click the NextButtonElement:</strong> Use <c>bot.ClickElement(nextButtonElement)</c> to trigger navigation.
    /// </description>
    /// </item>
    /// <br/>
    /// <item>
    /// <description>
    /// <strong>Wait for page transition:</strong> Use <c>bot.ExplicitWait(oldUrl)</c> after clicking to ensure the page has 
    /// fully navigated before proceeding.
    /// </description>
    /// </item>
    /// <br/>
    /// <item>
    /// <description>
    /// <strong>Reset AttributeList:</strong> Set <c>bot.AttributeList = []</c> at the end 
    /// to clear previous page's elements. Failure to do this will cause next call to SetAttributeList() 
    /// to add new elements without deleting the previous ones. This can induce staleElement exceptions or recursive iteration of the 
    /// same page elements.
    /// </description>
    /// </item>
    /// <br/><br/>
    /// </list>
    /// <para><strong>Example Implementation:</strong></para>
    /// <code>
    /// IWebElement nextButtonElement = GetNextPageElement(bot);
    /// 
    /// if (bot.WaitTillExists(nextButtonElement))
    /// {
    ///     string oldUrl = bot.Driver.Url;
    ///     bot.ClickElement(nextButtonElement);
    ///     bot.ExplicitWait(oldUrl);
    /// }
    /// bot.AttributeList = [];
    /// </code>
    /// </remarks>
    public static abstract void GoToNextPage(Bot bot, IWebElement NextButtonElement);



    /// <summary>
    ///  function to perform all the bot actions nessesary to download all elements currently in the Bot.AttributeList
    /// </summary>
    /// <param name="bot"></param>
    public static abstract void DownloadPageElements(Bot bot, string ByMechanism);


    /// <summary>
    ///  Function to assert that the expected number of clicked set elements matches the actual amount of clicked set elements. 
    /// </summary>
    /// <returns></returns>
    public static abstract bool AssertDownloadAmount();



    /// <summary>
    /// Should serve as the control point used to initiate the data download process. 
    /// All other interface functions should, at some point, be called within <c>ProcessData()</c>. 
    /// </summary>
    public static abstract void ProcessData();

}