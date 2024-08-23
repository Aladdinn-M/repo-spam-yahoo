using OpenQA.Selenium.Chrome;
using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;
using System;
using System.Reflection;
using System.Web;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Text;

internal class Program
{
    static string myAPI;
    private static void Main(string[] args)
    {
        
        string filePath = "emails.txt";
        CreateFileIfNotExists(filePath);
       // Console.WriteLine(" Entre your 2Captcha API: ");
        myAPI = "CAP-393CFC36AD70D8F0EC4105660AD1EC00";


        IWebDriver driver = InitializeWebDriver();
        fillLoginFields(driver, 0, filePath);

    }



    private static IWebDriver InitializeWebDriver()
    {
        FirefoxOptions options = new FirefoxOptions();
        Proxy proxy = new Proxy();
        proxy.HttpProxy = "45.43.71.235:6833";
        options.Proxy = proxy;  


        IWebDriver driver = new FirefoxDriver();
        driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);

       try
       {
            string url = "https://mail.yahoo.com/d/folders/1";
            driver.Manage().Window.Maximize();
            driver.Navigate().GoToUrl(url);
        }
       catch (Exception)
       {
            Console.WriteLine("--------------error in openning browser!!");
       }

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

            Console.WriteLine("captcha...............!");


                passeTheCaptcha(driver, 1, emailFilePath);


             // Output the sitekey

            IWebElement passInput = driver.FindElement(By.Id("login-passwd"));
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

        string captchaUrl = extractCaptchaURL(driver);

        //send captcha request 
        //==========================================================================================================================================================================

        var token =  tokenRequest(captchaKey, captchaUrl, myAPI);
       
        solveCaptcha(driver, token.Result);

        var submitButton = driver.FindElement(By.Id("recaptcha-submit"));
        IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
        js.ExecuteScript("arguments[0].removeAttribute('disabled');", submitButton);


        submitButton.Click();


        //==========================================================================================================================================================================


        }
    private static string extractCaptchaURL(IWebDriver driver)
    {
        driver.SwitchTo().Frame("recaptcha-iframe");
        IWebElement recaptchaElement = driver.FindElement(By.TagName("iframe"));
        string URL = recaptchaElement.GetAttribute("src");
        return URL;
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


    static async Task<string> tokenRequest(string siteKey, string captchaUrl, string apikey)
    {
        var client = new HttpClient();

        // Create an object representing the task
        var taskData = new
        {
            clientKey = apikey,
            task = new
            {
                type = "ReCaptchaV2TaskProxyLess",
                websiteURL = captchaUrl,
                websiteKey = siteKey,
                pageAction = "login"
            }
        };

        // Serialize the object to JSON
        var jsonContent = JsonConvert.SerializeObject(taskData);

        // Prepare the content for the POST request as JSON
        var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

        // Create the HttpRequestMessage
        var request = new HttpRequestMessage(HttpMethod.Post, "https://api.capsolver.com/createTask")
        {
            Content = content
        };

        // Send the request using Send
        var response = client.Send(request);

        // Read and parse the response
        var jsonResponse = await response.Content.ReadAsStringAsync();
        string taskId = JObject.Parse(jsonResponse)["taskId"].ToString();
        Console.WriteLine($"request sent .......!");

        string captchaSolution = "";
        while (captchaSolution == "" || captchaSolution.Contains("processing"))
        {
            await Task.Delay(12000);
            var client2 = new HttpClient();

            var requestData = new
            {
                clientKey = apikey,
                taskId = taskId
            };

            // Serialize the object to JSON
            var jsonContent2 = JsonConvert.SerializeObject(requestData);

            // Prepare the content for the POST request as JSON
            var content2 = new StringContent(jsonContent2, Encoding.UTF8, "application/json");

            // Create the HttpRequestMessage
            var request2 = new HttpRequestMessage(HttpMethod.Post, "https://api.capsolver.com/getTaskResult")
            {
                Content = content2
            };

            // Send the request using Send
            var response2 = client2.Send(request2);

            // Read and parse the response
            var jsonResponse2 = await response2.Content.ReadAsStringAsync();
           
                if (JObject.Parse(jsonResponse2)["solution"]!= null)
            {
                captchaSolution = JObject.Parse(jsonResponse2)["solution"]["gRecaptchaResponse"].ToString();
                Console.WriteLine($"response received......!");
            }
            Console.WriteLine(jsonResponse2);
            
        }
        Console.Out.WriteLine("token is ready......!");
        return captchaSolution;
    }




    static void solveCaptcha(IWebDriver driver, string token)
    {
        IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
        IWebElement recaptchaResponseElement = driver.FindElement(By.Id("g-recaptcha-response"));
        js.ExecuteScript("arguments[0].removeAttribute('style');", recaptchaResponseElement);
        js.ExecuteScript($"arguments[0].value = '{token}';", recaptchaResponseElement);
    }








}


