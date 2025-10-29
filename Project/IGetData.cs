using OpenQA.Selenium;

namespace LEGO_Brickster_AI;

// static abstract interface
interface IGetData
{

  /// <summary>
  /// The mutable string defining the main website url
  /// </summary>
  public static abstract string Url { get; set; }


  /// <summary>
  ///The absolute path to the download folder, should be definied with enough '..' to reach the desired download folder path, from the application's .dll file directory.
  /// </summary>
  static abstract string DownloadFolderPath { get; }



  static abstract bool CustomRun { get; }
  /// <summary>
  /// int defining the int value of the page. Can be utilized if the website have several pages of individual data 
  /// </summary>
  static abstract int StartFromPage { get; }

  /// <summary>
  ///  int defining the number of sets of data pr page. Usage of this field depends on the structure of the website on question. 
  /// </summary>
  static abstract int ExpectedSetsPrPage { get; }


  /// <summary>
  /// The maximum number of pages which can be reached on the webpage, relative to the amount of pages of sets to download.
  /// 
  /// </summary>
  static abstract int MaxPage { get; }

  /// <summary>
  /// The PageLimit defines the absolute amount of pages, where SetAttributeList() is called for each page, to add the 
  /// IdentifierAttribute for all CommonElementStrings to the bot.AttributeList. PageLimit can be set to MaxPage to ensure a complete run
  /// and avoid unexpected errors when trying to access more than the maximum amount of pages.
  /// </summary>
  static abstract int PageLimit { get; }


  /// <summary>
  /// a substring serving as an extension the main website url extension, which can be combined with <param name = "StartFromPage"> to begin a run from a certain subpage.  
  /// </summary>
  static abstract string UrlPageVarient { get; }




  /// <summary>
  /// A simple reference counter for asserting correct amount of elements have been clicked, when finally compared to <param name = "ExpectedElementClickAmount">
  static abstract int ElementClickCounter { get; set; }



  /// <summary>
  /// int defining the expected amount of elements to be clicked, inferred from <param name = "SetsPrPage"> and <param name = "PageLimit">
  /// </summary>
  static abstract int ExpectedElementClickAmount { get; set; }


  ///
  /// user defined deviation of the ExpectedElementClickAmount. Should be implemented after testing whether there some pages which have a different amount of downloadable sets.
  /// 
  static abstract int ExpectedElementClickDeviation { get; }



  // --------------------------------------------------------------------------------------------------------------------------------------------//
  // Functions


  /// <summary>
  /// Define a custom starting page for a custom run, using the <param name = "url">
  /// with the <param name = "UrlPageVarient"> and <param name = "_startFromPage"> to define 
  /// the url of initial page to begin the custom run. Should be called initially before 
  /// <c> AccessWebPage()</c>  within <c> ProcessData()</c>
  /// </summary>
  public static abstract void UseCustomStartingPage();



  /// <summary>
  /// This function should access the main webpage via <c> bot.GoToMainPage(Url)</c> and perform any necessary bot actions to escape popups,
  ///  which may hinder access to the elements of the main page. An optional <c>ElementCandidatesDict&lt;string,string</c>&gt; can be passed in case,
  /// some preliminary actions are based on certain page functionality which is present is several elements.  
  /// </summary>
  /// <param name="bot"></param>
  public static abstract void AccessWebPage(Bot bot, Dictionary<string, string>? ElementCandidatesDict);



  /// <summary>
  ///  Find all the element on the current page which fits the <paramref name="CommonElementString"/> with the 
  /// <paramref name="CommonByMechanism"/> and the optional <paramref name="IdentifierAttribute"/> and add them to the Bot.AttributeList. 
  /// Be aware that depending on the specific element the defined <paramref name="IdentifierAttribute"/> needs to be part of all the elements in question. 
  /// by default <paramref name="IdentifierAttribute"/> is set to 'Text' in bot.FindPageElements(). 
  /// </summary>
  /// <param name="bot"></param>
  /// <param name="CommonElementString"></param>
  /// <param name="CommonByMechanism"></param>
  public static abstract void SetAttributeList(Bot bot, string CommonElementString, string CommonByMechanism, string IdentifierAttribute);


  /// <summary>
  /// Function to find a specific type of element by a map of potential candiates <c>ElementCandidatesDict&lt;string,string</c>&gt;.
  /// This is especially useful in <c>AccessWebPage()</c>. Function iterates over the <c>ElementCandidatesDict&lt;string,string</c>&gt;
  ///  map's key-value pairs. For each key-value pair it calls <c>bot.FindElement()</c>
  ///  to locate a button on the webpage which should be a valid currently displayed element.
  /// </summary>
  /// <param name="bot">The bot instance used to find and interact with page elements.</param>
  /// 
  public abstract static IWebElement FindDisplayedElement(Bot bot, Dictionary<string, string> ElementCandidatesDict);


  /// <summary>
  /// Should perform the necessary bot actions to navigate to the next desired page by clicking the 
  /// <c>NextButtonElement</c>. If several elements on the current page leads to the next desired page,
  ///  but may or may not be present due to responsiveness, then it is recommended to utilize the <c>FindDisplayedElement()</c> function 
  /// with a <c>ElementCandidatesDict&lt;string,string</c>&gt; to return a <c>NextButtonElement</c> and pass that element into <c>GoToNextPage</c>   .    
  /// </summary>
  /// 
  /// <remarks>
  /// <para>Implementation Recommendation: The following is simple implementation example of the <c>GoToNextPage</c>.
  /// Users may find their implementation demands different depending on the website they are working with.</para>
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
  /// try 
  /// {
  ///   if (bot.WaitTillExists(nextButtonElement))
  ///   {
  ///       string oldUrl = bot.Driver.Url;
  ///       bot.ClickElement(nextButtonElement);
  ///       bot.ExplicitWait(oldUrl);
  ///   }
  ///   bot.AttributeList = [];
  /// 
  /// Catch (BotStaleElementException ex)
  ///      {
  ///          Console.WriteLine(ex.Message);
  ///      }
  /// Catch (BotTimeOutException ex)
  ///      {
  ///          Console.WriteLine(ex.Message);
  ///      }
  /// </code>
  /// </remarks>
  public static abstract void GoToNextPage(Bot bot, IWebElement NextButtonElement);



  /// <summary>
  ///  function to perform all the nessesary bot actions to download all elements currently in the <c>Bot.AttributeList</c>
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