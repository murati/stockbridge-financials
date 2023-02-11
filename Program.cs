using System;
using System.Diagnostics;
using System.Drawing;
using System.Reflection;
using System.Reflection.Metadata;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Linq;
using CefSharp;
using CefSharp.DevTools.CacheStorage;
using CefSharp.DevTools.Page;
using CefSharp.OffScreen;
using HtmlAgilityPack;
using StockbridgeFinancials.Models.DataModels;
using StockbridgeFinancials.Models.ScriptingModels;

namespace MyApp // Note: actual namespace depends on the project name.
{
    internal class Program
    {


        private static Dictionary<int, string> navigationToSucceed = new Dictionary<int, string> { { 1, "Login Completed" }, { 2, "Moving to search results" }, { 3, "Moving to 2nd page" }, { 4, "Checking home delivery" }, { 5, "Searching Model X" }, { 6, "Moving to 2nd page enchaned results" } };
        private static Dictionary<int, string> navigationToFail = new Dictionary<int, string> { { 1, "Login Failed" }, { 2, "Unable to make search" }, { 3, "Unable to move forward" }, { 4, "Unable to check home delivery" }, { 5, "Unable to search Model X" }, { 6, "Unable to move enchanced results" } };

        static async Task Main(string[] args)
        {

            Cef.EnableWaitForBrowsersToClose();
            var settings = new CefSettings { CachePath = Path.GetFullPath("cache") };
            var success = await Cef.InitializeAsync(settings);
            if (!success)
                return;
            var scriptsToExecute = CarsScripting.InitializeCarsScripting();
            await ScrapeCarsDotCom(scriptsToExecute).ContinueWith(p =>
            {
                Cef.WaitForBrowsersToClose();
                Cef.ShutdownWithoutChecks();
            });

        }

        private static async Task ScrapeCarsDotCom(List<CarsScripting> carsScriptings)
        {
            //Reduce rendering speed to one frame per second so it's easier to take screen shots
            var browserSettings = new BrowserSettings { WindowlessFrameRate = 1 };
            var requestContextSettings = new RequestContextSettings { CachePath = Path.GetFullPath("cache") };

            // RequestContext can be shared between browser instances and allows for custom settings
            // e.g. CachePath
            using (var requestContext = new RequestContext(requestContextSettings))
            using (var browser = new ChromiumWebBrowser("https://www.cars.com/", browserSettings, requestContext))
            {

                await browser.WaitForInitialLoadAsync();

                //Check preferences on the CEF UI Thread
                await Cef.UIThreadTaskFactory.StartNew(delegate
                {
                    var preferences = requestContext.GetAllPreferences(true);

                    //Check do not track status
                    var doNotTrack = (bool)preferences["enable_do_not_track"];

                    Debug.WriteLine("DoNotTrack: " + doNotTrack);
                });
                var onUi = Cef.CurrentlyOnThread(CefThreadIds.TID_UI);

                var contentSize = await browser.GetContentSizeAsync();
                var viewport = new Viewport
                {
                    Height = contentSize.Height,
                    Width = contentSize.Width,
                    Scale = 1.0
                };

                //execute the JS in the given order
                JavascriptResponse scriptResult;
                WaitForNavigationAsyncResponse navResult;
                int navigationIndex = 1;
                List<string> results = new List<string>();
                for (int i = 0; i < carsScriptings.Count; i++)
                {
                    var scripting = carsScriptings.ElementAt(i);
                    scriptResult = await browser.EvaluateScriptAsync(scripting.Script);
                    PrintJSResult(scripting.Message, scriptResult);
                    if (scripting.IsNavigation)
                    {
                        navResult = await browser.WaitForNavigationAsync(new TimeSpan(0, 0, 3));
                        PrintWaitResult(navigationIndex, navResult);
                        await browser.WaitForRenderIdleAsync();
                        if (navResult.Success)
                        {
                            await browser.WaitForInitialLoadAsync();
                            var ss = await browser.CaptureScreenshotAsync(viewport: viewport);
                            var screenshotPath = Path.Combine($"SS - {navigationToSucceed[navigationIndex]} - {DateTime.Now.Ticks}.png");
                            Console.WriteLine("Screenshot ready. Saving to {0}", screenshotPath);
                            File.WriteAllBytes(screenshotPath, ss);
                            Console.WriteLine("Screenshot ready. Saving to {0}", navigationToSucceed[navigationIndex]);
                            navigationIndex++;
                            if (scripting.IsResultsPage)
                            {
                                var source = await browser.GetSourceAsync();
                                results.Add(source);
                            }
                        }
                        else
                        {
                            Console.WriteLine("Failed to execute designed flow!");
                            break;
                        }
                    }

                }
                HtmlDocument document = new HtmlDocument();
                HtmlNode dataNode;
                VehicleModel vehicle;
                List<VehicleModel> vehicles = new();

                for (int i = 0; i < results.Count(); i++)
                {
                    document.LoadHtml(results[i]);
                    var vehicleCards = document.DocumentNode.SelectNodes("//div[starts-with(@id,'vehicle-card-')]");
                    for (int j = 0; j < vehicleCards.Count(); j++)
                    {
                        document.LoadHtml(vehicleCards.ElementAt(j).InnerHtml);
                        dataNode = document.DocumentNode.SelectSingleNode("//div[@class='vehicle-details']/a[contains(@class,'vehicle-card-link')]/h2[@class='title']");
                        if (dataNode != null)
                        {
                            vehicle = new();
                            vehicle.Title = dataNode.InnerText;
                            dataNode = document.DocumentNode.SelectSingleNode("//div[@class='vehicle-details']/div[@class='mileage']");
                            vehicle.Mileage = dataNode != null ? dataNode.InnerText : "";
                            dataNode = document.DocumentNode.SelectSingleNode("//div[@class='vehicle-details']/div[contains(@class,'price-section')]/span[@class='primary-price']");
                            vehicle.Price = dataNode != null ? dataNode.InnerText : "";
                            dataNode = document.DocumentNode.SelectSingleNode("//div[@class='vehicle-details']/div[@class='vehicle-dealer']/div[@class='dealer-name']");
                            vehicle.Dealer = dataNode != null ? dataNode.InnerText : "";
                            dataNode = document.DocumentNode.SelectSingleNode("//div[@class='vehicle-details']/div[@class='vehicle-dealer']/div[contains(@class,'miles-from ')]");
                            vehicle.Distance = dataNode != null ? dataNode.InnerText : "";
                            vehicles.Add(vehicle);
                        }
                    }
                    //to discriminate the file names
                    File.WriteAllText($"Results - {navigationToSucceed[i + 2]} - {DateTime.Now.Ticks}.json", JsonSerializer.Serialize(vehicles));

                }
            }
        }

        private static void PrintJSResult(string message, JavascriptResponse clickResult)
        {
            Console.ForegroundColor = clickResult.Success ? ConsoleColor.Green : ConsoleColor.Red;
            if (clickResult.Success)
                Console.WriteLine($"{message} is successful");
            else Console.WriteLine($"{message} is unsuccessful");
            Console.ForegroundColor = ConsoleColor.White;
        }
        private static void PrintWaitResult(int navigationIndex, WaitForNavigationAsyncResponse response)
        {
            Console.ForegroundColor = response.Success ? ConsoleColor.Cyan : ConsoleColor.Magenta;
            if (response.Success)
                Console.WriteLine($"{navigationToSucceed[navigationIndex]} is succeeded : {response.Success}\nHttpCode : {response.HttpStatusCode}");
            else Console.WriteLine($"{navigationToFail[navigationIndex]} is failed : {response.Success}\nHttpCode : {response.HttpStatusCode}");
            Console.ForegroundColor = ConsoleColor.White;
        }
        private static void PrintInitResult(LoadUrlAsyncResponse response)
        {
            Console.ForegroundColor = response.Success ? ConsoleColor.Yellow : ConsoleColor.DarkYellow;
            Console.WriteLine($"Is Successful : {response.Success}\nHttpCode : {response.HttpStatusCode}");
            Console.ForegroundColor = ConsoleColor.White;
        }
    }
}