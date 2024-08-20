using OpenQA.Selenium.Chrome;
using OpenQA.Selenium;
using System.Diagnostics;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Firefox;
using System;
using System.Reflection;
using System.Web;
using _2CaptchaAPI;


internal class Program
{
    static string myAPI;
    private static void Main(string[] args)
    {
        


        string filePath = "emails.txt";
        CreateFileIfNotExists(filePath);
        Console.WriteLine(" Entre your 2Captcha API: ");
        myAPI = Console.ReadLine();


        IWebDriver driver = InitializeWebDriver();
        fillLoginFields(driver, 1, filePath);




    }



    private static IWebDriver InitializeWebDriver()
    {
        FirefoxOptions options = new FirefoxOptions();
        Proxy proxy = new Proxy();
        proxy.HttpProxy = "45.43.71.235:6833";
        options.Proxy = proxy;  


        IWebDriver driver = new FirefoxDriver(options);
        driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);

       // try
       // {
            string url = "https://mail.yahoo.com/d/folders/1";
            driver.Manage().Window.Maximize();
            driver.Navigate().GoToUrl(url);
       // }
      //  catch (Exception)
       // {
      //      Console.WriteLine("--------------error in openning browser!!");
      //  }

        return driver;
    }

    private static string filePath(string filename)
    {


        // Get the current directory
        string currentDirectory = Directory.GetCurrentDirectory();

        // file
        string filePath = Path.Combine(currentDirectory, filename);
        


        return filePath;

    }


    private static void CreateFileIfNotExists(string filePath)
    {
        try
        {
            // Check if the file already exists
            if (File.Exists(filePath))
            {
                Console.WriteLine($"File '{Path.GetFileName(filePath)}' already exists in the following path:");
                Console.WriteLine(filePath);
            }
            else
            {
                // Create the file if it doesn't exist
                using (File.Create(filePath))

                    Console.WriteLine($"File '{Path.GetFileName(filePath)}' created successfully in the following path:");
                Console.WriteLine(filePath);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }
    }

    private static void fillLoginFields(IWebDriver driver, int index,string emailFilePath)
    {
        try
        {


            string email = extractfile(0, index, emailFilePath, driver);
            IWebElement loginInput = driver.FindElement(By.Id("login-username-form"));
            loginInput = driver.FindElement(By.Name("username"));
            loginInput.SendKeys(email);
            loginInput.SendKeys(Keys.Enter);

            Console.WriteLine("captcha...........................!");


                passeTheCaptcha(driver, 1, emailFilePath);


             // Output the sitekey

            IWebElement passInput = driver.FindElement(By.CssSelector(""));
            string password = extractfile(1, index, emailFilePath, driver);
            passInput.SendKeys(password);
            passInput.SendKeys(Keys.Enter);
       }
        catch (Exception)
        {

            Console.WriteLine("--------------error in login into email");
        }
        
    }


    
    private static string extractfile(int zeroOrOne, int index, string filePath, IWebDriver driver = null)
    {
        string result = null;
        try
        {
            string[] lines = File.ReadAllLines(filePath);

            if (lines.Length > 0)
            {
                string[] parts = lines[index].Split(':');

                if (parts.Length > 1)
                {
                    result = parts[zeroOrOne];
                }
                else
                {
                    Console.WriteLine("--------------Invalid format in the file. Each line should be in the format 'text1:text2'.");
                    if (driver != null) driver.Quit();
                }
            }
            else
            {
                Console.WriteLine($"--------------The file is empty  {filePath}");
                if (driver != null) driver.Quit();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("--------------An error occurred: " + ex.Message);
            if (driver != null) driver.Quit();
        }
        return result;
    }


    static async Task passeTheCaptcha(IWebDriver driver, int index, string domainsFile)
    {
        //extracting captcha key 
        string captchaKey = extractCaptchaKey(driver);

        // get url 
        string currentUrl = driver.Url;

        //send captcha request 
        string captchaId = await SubmitCaptchaTo2Captcha(myAPI, captchaKey, currentUrl);
        string captchaToken = await RetrieveCaptchaSolution(myAPI, captchaId);


        if (!isCaptchaReturnError(captchaToken, driver))
        {
            Console.WriteLine($"Successfully solved the CAPTCHA");
        }

        // Set the solved CAPTCHA in the textarea
        solveCaptcha(driver, captchaToken);
    }


    static string extractCaptchaKey(IWebDriver driver)
    {
        driver.SwitchTo().Frame("recaptcha-iframe");

        // IWebElement recaptchaElement = driver.FindElement(By.Id("recaptcha-iframe"));
        IWebElement recaptchaElement = driver.FindElement(By.Id("g-recaptcha"));
        // Get the value of the data-sitekey attribute
        string siteKey = recaptchaElement.GetAttribute("data-sitekey");

        driver.SwitchTo().DefaultContent();

        return siteKey;
    }

    /*static string captchaRequest(string myAPI, string siteKey, string currentUrl, IWebDriver driver)
    {

        // Solve the CAPTCHA
        Console.WriteLine("Solving CAPTCHA........");
        var service = new _2CaptchaAPI._2Captcha(myAPI);
        var response = service.SolveReCaptchaV2(siteKey, currentUrl).Result;
        string code = response.Response;
        return code;
    }*/

    static void solveCaptcha(IWebDriver driver, string code)
    {
        IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
        IWebElement recaptchaResponseElement = driver.FindElement(By.Id("g-recaptcha-response"));
        js.ExecuteScript("arguments[0].removeAttribute('style');", recaptchaResponseElement);
        js.ExecuteScript($"arguments[0].value = '{code}';", recaptchaResponseElement);
    }



    static bool isCaptchaReturnError(string code, IWebDriver driver)
    {
        bool result;
        switch (code)
        {
            case "ERROR_WRONG_USER_KEY":
                Console.WriteLine(" ERROR_WRONG_USER_KEY You've provided the key parameter value in an incorrect format. Check your API key.");
                closeBrowser(driver);
                result = true;
                break;
            case "ERROR_KEY_DOES_NOT_EXIST":
                Console.WriteLine(" ERROR_KEY_DOES_NOT_EXIST The key you've provided does not exist. Check your API key.");
                closeBrowser(driver);
                result = true;
                break;
            case "ERROR_ZERO_BALANCE":
                Console.WriteLine(" ERROR_ZERO_BALANCE You don't have funds on your account. Deposit your account to continue solving captchas.");
                closeBrowser(driver);
                result = true;
                break;
            case "ERROR_PAGEURL":
                Console.WriteLine(" ERROR_PAGEURL pageurl parameter is missing in your request. Stop sending requests and change your code to provide valid pageurl parameter.");
                closeBrowser(driver);
                result = true;
                break;
            case "ERROR_NO_SLOT_AVAILABLE":
                Console.WriteLine(" ERROR_NO_SLOT_AVAILABLE You can receive this error in two cases...");
                closeBrowser(driver);
                result = true;
                break;
            case "ERROR_ZERO_CAPTCHA_FILESIZE":
                Console.WriteLine(" ERROR_ZERO_CAPTCHA_FILESIZE Image size is less than 100 bytes. Check the image file.");
                closeBrowser(driver);
                result = true;
                break;
            case "ERROR_TOO_BIG_CAPTCHA_FILESIZE":
                Console.WriteLine(" ERROR_TOO_BIG_CAPTCHA_FILESIZE Image size is more than 600 kB or image is bigger than 1000px on any side. Check the image file.");
                closeBrowser(driver);
                result = true;
                break;
            case "ERROR_WRONG_FILE_EXTENSION":
                Console.WriteLine(" ERROR_WRONG_FILE_EXTENSION Image file has an unsupported extension. Accepted extensions: jpg, jpeg, gif, png. Check the image file.");
                closeBrowser(driver);
                result = true;
                break;
            case "ERROR_IMAGE_TYPE_NOT_SUPPORTED":
                Console.WriteLine(" ERROR_IMAGE_TYPE_NOT_SUPPORTED Server can't recognize the image file type. Check the image file.");
                closeBrowser(driver);
                result = true;
                break;
            case "ERROR_UPLOAD":
                Console.WriteLine(" ERROR_UPLOAD Server can't get file data from your POST-request. Check your code that makes POST request.");
                closeBrowser(driver);
                result = true;
                break;
            case "ERROR_IP_NOT_ALLOWED":
                Console.WriteLine(" ERROR_IP_NOT_ALLOWED The request is sent from an IP that is not on the list of your allowed IPs. Check the list of your allowed IPs.");
                closeBrowser(driver);
                result = true;
                break;
            case "IP_BANNED":
                Console.WriteLine(" IP_BANNED Your IP address is banned due to many frequent attempts to access the server using wrong authorization keys. Ban will be automatically lifted after 5 minutes.");
                closeBrowser(driver);
                result = true;
                break;
            case "ERROR_BAD_TOKEN_OR_PAGEURL":
                Console.WriteLine(" ERROR_BAD_TOKEN_OR_PAGEURL You can get this error code when sending reCAPTCHA V2. That happens if your request contains an invalid pair of googlekey and pageurl. Explore the code of the page carefully to find valid pageurl and sitekey values.");
                closeBrowser(driver);
                result = true;
                break;
            case "ERROR_GOOGLEKEY":
                Console.WriteLine(" ERROR_GOOGLEKEY You can get this error code when sending reCAPTCHA V2. That means that the sitekey value provided in your request is incorrect: it's blank or malformed. Check your code that gets the sitekey and makes requests to our API.");
                closeBrowser(driver);
                result = true;
                break;
            case "ERROR_PROXY_FORMAT":
                Console.WriteLine(" ERROR_PROXY_FORMAT You use incorrect proxy format in your request to in.php. Use proper format as described in the section Using proxies.");
                closeBrowser(driver);
                result = true;
                break;
            case "ERROR_WRONG_GOOGLEKEY":
                Console.WriteLine(" ERROR_WRONG_GOOGLEKEY googlekey parameter is missing in your request. Check your code that gets the sitekey and makes requests to our API.");
                closeBrowser(driver);
                result = true;
                break;
            case "ERROR_CAPTCHAIMAGE_BLOCKED":
                Console.WriteLine(" ERROR_CAPTCHAIMAGE_BLOCKED You've sent an image that is marked in our database as unrecognizable. Try to override the website's limitations.");
                closeBrowser(driver);
                result = true;
                break;
            case "TOO_MANY_BAD_IMAGES":
                Console.WriteLine(" TOO_MANY_BAD_IMAGES You are sending too many unrecognizable images. Make sure that your last captchas are visible and check unrecognizable images we saved for analysis. Then fix your software to submit images properly.");
                closeBrowser(driver);
                result = true;
                break;
            case "MAX_USER_TURN":
                Console.WriteLine(" MAX_USER_TURN You made more than 60 requests to in.php within 3 seconds. Your account is banned for 10 seconds. Ban will be lifted automatically. Set at least 100 ms timeout between requests to in.php.");
                closeBrowser(driver);
                result = true;
                break;
            case "ERROR_BAD_PARAMETERS":
                Console.WriteLine(" ERROR_BAD_PARAMETERS  The error code is returned if some required parameters are missing in your request or the values have incorrect format. Or in case if you have SandBox mode and 100% recognition options enabled at the same time. Check that your request contains all the required parameters and the values are in the proper format. Use debug mode to see which values you send to our API.");
                closeBrowser(driver);
                result = true;
                break;
            case "ERROR_BAD_PROXY":
                Console.WriteLine(" ERROR_BAD_PROXY  You can get this error code when sending a captcha via a proxy server which is marked as BAD by our API. Use a different proxy server in your requests.");
                closeBrowser(driver);
                result = true;
                break;
            case "ERROR_SITEKEY":
                Console.WriteLine(" ERROR_SITEKEY   You can get this error code when sending hCaptcha. That means that the sitekey value provided in your request is incorrect: it's blank or malformed. Check your code that gets the sitekey and makes requests to our API.");
                closeBrowser(driver);
                result = true;
                break;
            default:
                result = false;
                break;

        }
        return result;
    }


    static void closeBrowser(IWebDriver driver)
    {
        if (IsWebDriverAlive(driver))
        {
            driver.Close();
            driver.Quit();
            driver = null;

        }
        driver = null;

        bool IsWebDriverAlive(IWebDriver driver)
        {
            try
            {
                // Check if the WebDriver instance is null or not closed
                return driver != null && driver.WindowHandles.Count > 0;
            }
            catch (WebDriverException)
            {
                // WebDriver is closed
                return false;
            }
        }
    }

    // Submit captcha to 2Captcha
    private static async Task<string> SubmitCaptchaTo2Captcha(string apiKey, string siteKey, string pageUrl)
    {
        using (HttpClient client = new HttpClient())
        {
            // Set a timeout to avoid indefinite hanging
            client.Timeout = TimeSpan.FromSeconds(10);

            // Construct the request URL
            string requestUrl = $"https://2captcha.com/in.php?key={apiKey}&method=userrecaptcha&googlekey={siteKey}&pageurl={pageUrl}";

            Console.WriteLine($"Request URL: {requestUrl}");

           // try
        //    {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
                Console.WriteLine("Sending request to 2Captcha...");

                // Send the GET request to 2Captcha
                HttpResponseMessage response =  client.Send(request);

                Console.WriteLine("Response received.");

                // Ensure the response was successful
                response.EnsureSuccessStatusCode();

                // Read the response content
                string result = await response.Content.ReadAsStringAsync();

                Console.WriteLine($"Response: {result}");

                // Check if the response starts with "OK|"
                if (result.StartsWith("OK|"))
                {
                    return result.Substring(3); // Return the captcha ID
                }
                else
                {
                Console.WriteLine($"Error submitting captcha: {result}");
                throw new Exception($"Error submitting captcha: {result}");
                }
          //  }
          //  catch (TaskCanceledException ex)
          //  {
            //    Console.WriteLine("Request timed out.");
        //        throw new Exception("Request timed out.", ex);
         ////   }
         //   catch (HttpRequestException ex)
         //   {
        //        Console.WriteLine($"Error sending request to 2Captcha: {ex.Message}");
        //       throw new Exception("Error sending request to 2Captcha.", ex);
         //   }
         //   catch (Exception ex)
         ///   {
         //      Console.WriteLine($"An unexpected error occurred: {ex.Message}");
         /////       throw new Exception("An unexpected error occurred.", ex);
         //   }
        }
    }


    // Retrieve captcha solution from 2Captcha
    private static async Task<string> RetrieveCaptchaSolution(string apiKey, string captchaId)
    {
        using (HttpClient client = new HttpClient())
        {
            string requestUrl = $"https://2captcha.com/res.php?key={apiKey}&action=get&id={captchaId}";
            string result;

            do
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
                Task.Delay(15000); // Wait 15 seconds
                HttpResponseMessage response =  client.Send(request);
                result = await response.Content.ReadAsStringAsync();
            }
            while (result == "CAPCHA_NOT_READY");

            if (result.StartsWith("OK|"))
            {
                return result.Substring(3); // Return the captcha token
            }
            else
            {
                Console.WriteLine($"Error submitting captcha: {result}");
                throw new Exception($"Error submitting captcha: {result}");
            }
        }
    }



}


