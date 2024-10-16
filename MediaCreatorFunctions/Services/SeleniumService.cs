using Microsoft.Extensions.Configuration;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MediaCreatorFunctions.Services
{
    public interface ISeleniumService
    {
        ChromeDriver GetChromeDriver();
        System.Collections.ObjectModel.ReadOnlyCollection<IWebElement> GetMultiple(IWebDriver driver, By by, int expTime = 5000);
        IWebElement Get(IWebDriver driver, By by, int expTime = 5000);
        IWebElement WaitFor(IWebDriver driver, By by, int expTime);
        IWebElement Click(IWebDriver driver, By by, int expTime = 5000);
        IWebElement ClearType(IWebDriver driver, By by, string message, int expTime = 5000);
    }
    public class SeleniumService : ISeleniumService
    {

        private readonly IConfiguration _configuration;

        public SeleniumService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public ChromeDriver GetChromeDriver()
        {
            ChromeOptions options = new ChromeOptions();
            options.AddArgument("--disable-blink-features=AutomationControlled");
            return new ChromeDriver(_configuration["PATH:Chromedriver"], options);
        }

        public System.Collections.ObjectModel.ReadOnlyCollection<IWebElement> GetMultiple(IWebDriver driver, By by, int expTime = 5000)
        {
            System.Collections.ObjectModel.ReadOnlyCollection<IWebElement> result = null;
            try
            {
                var ticker = 0;
                while (ticker < expTime)
                {
                    var webElements = driver.FindElements(by);
                    if (webElements.Count > 0)
                    {
                        result = webElements;
                        break;
                    }
                    Thread.Sleep(500);
                    ticker += 500;
                }
            }
            catch (Exception) { }
            return result;
        }

        public IWebElement Get(IWebDriver driver, By by, int expTime = 5000)
        {
            IWebElement result = null;
            try
            {
                result = GetMultiple(driver, by, expTime)[0];
            }
            catch (Exception) { }
            return result;
        }

        public IWebElement WaitFor(IWebDriver driver, By by, int expTime)
        {
            return Get(driver, by, expTime);
        }

        public IWebElement Click(IWebDriver driver, By by, int expTime = 5000)
        {
            IWebElement result = null;
            result = Get(driver, by, expTime);
            if(result != null) result.Click();
            return result;
        }

        public IWebElement ClearType(IWebDriver driver, By by, string message, int expTime = 5000)
        {
            IWebElement result = null;
            try
            {
                result = Get(driver, by, expTime);
                result.Clear();
                result.SendKeys(message);
            }
            catch (Exception) { }
            return result;
        }
    }
}
