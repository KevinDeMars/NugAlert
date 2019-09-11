using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Xml.Serialization;
using Blazor.Extensions.Storage;

namespace NugAlert
{

    class MenuManager
    {
        private LocalStorage storage;

        private Dictionary<DateTime, List<Menu>> cache = new Dictionary<DateTime, List<Menu>>();

        public MenuManager(LocalStorage storage)
        {
            this.storage = storage;
        }
        public async Task<List<Menu>> LoadDay(DateTime day)
        {
            if (cache.TryGetValue(day, out List<Menu> menus))
            {
                return menus;
            }

            string storageKey = day.ToString("yyyy-MM-dd");

            menus = await storage.GetItem<List<Menu>>(storageKey);

            if (menus != null && menus.Count > 0)
            {
                cache[day] = menus;
                return menus;
            }

            // Couldn't load from storage so fetch the day
            menus = await FetchMenus(day);
            cache[day] = menus;
            await storage.SetItem(storageKey, menus);
            return menus;
        }

        private async Task<List<Menu>> FetchMenus(DateTime date)
        {
            Stopwatch watch = new Stopwatch();
            watch.Start();

            var http = new HttpClient();
            var futureResponses = new Dictionary<(Location, Meal), Task<HttpResponseMessage>>();
            foreach (Location location in Enum.GetValues(typeof(Location)))
            {
                foreach (Meal meal in MealInfo.GetValidMeals(date, location))
                {
                    string dateStr = date.ToString("MM/dd/yyyy");
                    string url = $"https://baylor.campusdish.com/api/menus/GetMenu?mode=Daily&locationId={(int)location}&periodId={(int)meal}&date={dateStr}";
                    url = "https://cors-anywhere.herokuapp.com/" + url;

                    futureResponses[(location, meal)] = http.GetAsync(url);
                }
            }

            Console.WriteLine($"Prepared requests in {watch.Elapsed.TotalSeconds} seconds");

            var result = new List<Menu>();

            foreach (var kvp in futureResponses)
            {
                Console.WriteLine($"Handling request for {date} {kvp.Key.Item1} {kvp.Key.Item2}");
                watch.Restart();
                var (location, meal) = kvp.Key;
                var response = await kvp.Value;

                Console.WriteLine($"Got response header in {watch.Elapsed.TotalSeconds} seconds");
                watch.Restart();

                string html = await response.Content.ReadAsStringAsync();

                Console.WriteLine($"Got response body in {watch.Elapsed.TotalSeconds} seconds");
                watch.Restart();

                var menu = new Menu(date, location, meal);
                if (await Task.Run(() => menu.LoadHtml(html)))
                    result.Add(menu);
                else
                    Console.WriteLine($"Failed loading menu for {date} {kvp.Key.Item1} {kvp.Key.Item2}");

                Console.WriteLine($"Parsed menu in {watch.Elapsed.TotalSeconds} seconds");
            }

            return result;
        }

    }
}
