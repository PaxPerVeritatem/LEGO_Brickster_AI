using OpenQA.Selenium;

namespace LEGO_Brickster_AI;

/// <summary>
/// Abstract interface for defining a data collector. Implementations should provide a way to scrape data from a target website.
/// Note that if the target website requires login and/or authentication, users should manually login once on chrome.
/// The bot will then save credentials via cookies. This allows for simpler implementation of interface methods such as AccessMainPage().
/// A 'Set', in the context of this interface, is defined as an element which has either one or multiple downloadable files associated with it.
/// Implementations should provide a way to configure custom runs of the data collector, access the main webpage, 
/// set the list of attributes to scrape and find the download button to download sets.
/// </summary>

interface IGetData
{
  // Bot Properties


  /// <summary>
  /// The mutable string defining the main website url
  /// </summary>
  public static abstract string Url { get; set; }


  /// <summary>
  /// The absolute path to the download folder, should be definied with enough '..' to reach 
  /// the desired download folder path, from the application's .dll file directory.
  /// </summary>
  static abstract string DownloadFolderPath { get; }


  // --------------------------------------------------------------------------------------------------------------------------------------------//

  // Global Properties

  /// <summary>
  /// The absolute maximum number of pages of sets which can be reached on the from the first page of a run. 
  /// if it is not possible to determine the maximum amount of pages, this value should be set to null
  /// </summary>
  static abstract int? MaxPage { get; }


  /// <summary>
  /// The amount of pages of sets to be scraped. If MaxPage is not null PageLimit can be set to its valie, to ensure a complete run
  /// and avoid unexpected errors, when trying to access more than the absolute maximum amount of pages. If MaxPage is null, 
  /// PageLimit can be set to scape any arbitrary amount of pages.
  /// 
  /// </summary>
  static abstract int PageLimit { get; }



  /// <summary>
  /// Int defining the number of expected sets pr page. 
  /// </summary>
  static abstract int ExpectedSetsPrPage { get; }


  /// <summary>
  /// The deviation from the ExpectedSetClickAmount. The default value should be set to 0, and can be increased if there are
  /// some pages which have a different amount of clickable sets, than the ExpectedSetsPrPage.
  /// </summary>
  static abstract int ExpectedSetClickDeviation { get; }


  /// <summary>
  ///  The total expected amount of sets to be scraped during a run.
  /// </summary>
  static abstract int ExpectedSetsScraped { get; }

  /// <summary>
  /// The total expected amount of sets to be clicked during a run. Should be subtracted by ExpectedSetClickDeviation
  /// </summary>
  static abstract int ExpectedSetClickAmount { get; set; }






  /// <summary>
  /// A simple counter for asserting correct amount of sets have been clicked, 
  /// inferred from ExpectedSetClickAmount 
  ///  /// </summary>
  static abstract int SetClickCounter { get; set; }

  /// <summary>
  ///  The total amount of files downloaded during a run. 
  /// </summary>
  static abstract int FilesDownloadedCounter { get; set; }


  /// <summary>
  ///  The total amount of files infered to already be downloaded during a run. 
  /// </summary>
  static abstract int FilesAlreadyDownloadedCounter { get; set; }


  // --------------------------------------------------------------------------------------------------------------------------------------------//


  // Custom run Properties 

  /// <summary>
  /// Bool defining whether a custom run should be performed on subpages or the main page.
  /// </summary>
  static abstract bool CustomRun { get; }



  /// <summary>
  /// Int defining the starting page to begin a run from. 
  /// the value is used in combination with the <param name = "UrlPageVarient" to construct the full url to access a certain subpage,
  /// if the website orders subpages with a page variable in the url, For example, 'www.website.com/page=1', 
  /// then StartFromPage can be set to 1, and UrlPageVarient can be set to 'page=' to construct the full url for the starting page.
  /// </summary>
  /// 
  static abstract int? StartFromPage { get; }


  /// <summary>
  /// A substring serving as an extension the main website url extension, 
  /// which can be combined with <param name = "StartFromPage"> to begin a run from a certain subpage.  
  /// </summary>
  static abstract string? UrlPageVarient { get; set; }

  /// <summary>
  /// A tuple containing the ElementString and ByMechanism for an subpage element.
  /// The element is clicked at the start of a custom run to access a certain subpage. 
  /// This is a alternative to utilizing UrlPageVarient and StartFromPage, when the website does not order subpages with a page variable in the url,
  ///  but rather with a clickable element on the main page.
  /// </summary>
  static abstract (string ElementString, string ByMechanism)? SubpageElementTuple { get; }


  /// <summary>
  /// Bool defining whether a custom run should be run on subpage or from the main page. 
  /// </summary>
  static abstract bool UseSubpage { get; }





  // --------------------------------------------------------------------------------------------------------------------------------------------//

  // Functions


  /// <summary>
  /// Function which should be called if CustomRun = true. Should be used to configure custom run parameters, 
  /// such as <c>Url</c> and <c> ExpectedElementClickAmount</c> via the custom run Properties.
  /// <c> AccessWebPage()</c>  within <c> ProcessData()</c>
  /// </summary>
  public static abstract void ConfigureCustomRun(Bot bot);



  /// <summary>
  ///  This function should access the main webpage via <c> bot.GoToMainPage(Url)</c> and perform any necessary bot actions,
  ///  which may hinder access to the elements of the main page. An optional <c>ElementCandidatesDict&lt;string,string</c>&gt; can be passed in case,
  ///  some preliminary actions are based on certain page functionality which is present in several displayed elements. Addtionally,
  ///  depending on if there are several prelimiary pages before reaching the actual main page, i would recommend implementing
  ///  some staic inline helper functions within AccessMainpage, for each preliminary page, which then can handle their
  ///  own set of preliminary actions. Alternatively, if certain actions are only needed on the initial page visit, then 
  ///  and utilizeing the saved cookies for the chrome driver. This can vastely simply the implementation of AccessMainPage(). 
  /// </summary>
  public static abstract void AccessMainPage(Bot bot, Dictionary<string, string> ElementCandidatesDict);


  /// <summary>
  /// Find all the element on the current page which fits the <paramref name="CommonElementString"/> with the 
  /// <paramref name="CommonByMechanism"/> and the optional <paramref name="IdentifierAttribute"/> and add them to the bot AttributeList. 
  /// Be aware that depending on the specific element the defined <paramref name="IdentifierAttribute"/> needs to be part of all the elements in question. 
  /// by default <paramref name="IdentifierAttribute"/> is set to 'Text' in bot.FindPageElements(). 
  /// </summary>
  public static abstract void SetAttributeList(Bot bot, string CommonElementString, string CommonByMechanism, string IdentifierAttribute, IWebElement AncestorElement);


  /// <summary>
  /// Function to find a specific type of element by a map of potential candiates <c>ElementCandidatesDict&lt;string,string</c>&gt;.
  /// This is especially useful in <c>AccessWebPage()</c>. Function iterates over the <c>ElementCandidatesDict&lt;string,string</c>&gt;
  ///  map's key-value pairs. For each key-value pair it calls <c>bot.FindElement()</c>
  ///  to locate a button on the webpage which should be a valid currently displayed element.
  /// </summary> 


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
  /// <strong>Reset AttributeList:</strong> <c>bot.AttributeList</c> should ALWAYS be reset at the end of the 
  /// function in order to clear previous page's elements. Failure to do this will cause next call to SetAttributeList() 
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
  public static abstract void GoToNextPage(Bot bot, IWebElement NextButtonElement, int? ClickAmount);



  /// <summary>
  ///  Function to define and perform all the nessesary bot actions to download
  ///  all elements currently in the <c>Bot.AttributeList</c>
  /// </summary>
  public static abstract void DownloadPageElements(Bot bot, string ByMechanism);



  /// <summary>
  ///  Function that should return the full file name for comparison to a potentially downloaded file, utilizeing the provided 
  /// <c>FileName</c>, depending on how the names of the files to be downloaded are extracted. 
  /// </summary>
  /// <param name="FileName"></param>
  public static abstract string GetFullFileName(string FileName);




  /// <summary>
  ///  Function to assert that the expected number of clicked set elements matches the actual amount of clicked set elements. 
  /// </summary>
  public static abstract bool AssertDownloadAmount();



  /// <summary>
  /// Should serve as the control point used to initiate the data download process. 
  /// All other interface functions should, at some point, be called within <c>ProcessData()</c>. 
  /// </summary>
  public static abstract void ProcessData();

}