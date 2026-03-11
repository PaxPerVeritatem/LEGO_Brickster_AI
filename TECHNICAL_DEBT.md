# Technical Debt & Future Improvements

## Chrome / ChromeDriver Portability

**Current setup:**
- `chromedriver.exe` is on the system PATH
- `chrome.exe` relies on default Chrome install location (`C:\Program Files\Google\Chrome\Application\chrome.exe`)
- Versions must be manually kept in sync by the developer

**Problem:**
- Environment-dependent — breaks on new machines or after a Chrome update without a matching ChromeDriver update
- Not distributable to other users without manual setup steps

**Future fix:**
- Bundle a portable `chrome.exe` and matching `chromedriver.exe` inside the project (e.g. `drivers/chrome/` and `drivers/chromedriver.exe`)
- Set `_options.BinaryLocation` to point to the bundled `chrome.exe`
- Set `_driverPath` to point to the bundled `chromedriver.exe`
- Consider automating version sync (e.g. using `WebDriverManager` NuGet package to auto-download matching versions at runtime)

---
## BrickLink Set Downloadability Pre-check

**Current behavior:**
- `GetDataBrickLink()` clicks into each LEGO set individually to check if it is downloadable
- This is slow — one navigation per set

**Potential improvement:**
- Investigate whether downloadability can be determined from the **set listing page** by looking at the downloadable symbol on each set card picture. 

**Impact:**
- Would require changes to the assertion/validation logic that currently assumes a set page has been navigated to
- Assertion code would need to handle the case where downloadability is inferred rather than confirmed from a full page load
