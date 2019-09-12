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

            var result = new List<Menu>();

            foreach (var kvp in futureResponses)
            {
                var (location, meal) = kvp.Key;
                var response = await kvp.Value;

                string html = await response.Content.ReadAsStringAsync();

                var menu = new Menu(date, location, meal);
                if (await Task.Run(() => menu.LoadHtml(html)))
                    result.Add(menu);
                else
                    Console.WriteLine($"Failed loading menu for {date} {kvp.Key.Item1} {kvp.Key.Item2}");
            }

            return result;
        }

    }
}
