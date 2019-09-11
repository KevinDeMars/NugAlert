using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace NugAlert
{
    [Serializable]
    class Menu
    {
        public DateTime Day { get; set; }
        public Location Location { get; set; }
        public Meal Meal { get; set; }
        public List<MenuCategory> Categories { get; set; }

        public Menu()
        {
            Day = new DateTime();
            Location = Location.Brooks;
            Meal = Meal.Breakfast;
            Categories = new List<MenuCategory>();
        }

        public Menu(DateTime day, Location location, Meal meal)
        {
            Day = day;
            Location = location;
            Meal = meal;
            Categories = new List<MenuCategory>();
        }

        public bool LoadHtml(string html)
        {
            // xpath notation:
            // categories: /div/div/div/div/div/div[@class='menu__station']
            //    name:   ./h2[contains(@class, 'section-subtitle')]
            //    items:  ./div[@class='menu__category']/ul/li/span/a[@class='viewItem']
            //    addons: ./div[@class='menu__addOns']/ul/li/span/a[@class='viewItem']

            // 1. Find all "section-subtitle"s
            // 2. Everything between next '>' and the following '<' is the section's name
            //     3. Find menu__addOns
            //     4. Find all "viewItems"s
            //          5. If index is before menu__addOns, it's a regular item; else, it's an addon.

            string[] categories = html.Split(new string[] { "menu__station" }, StringSplitOptions.None);
            // Skip the first string in the array, because we're only interested in the stuff after the first instance of "menu__station"
            for (int i = 1; i < categories.Length; ++i)
            {
                var category = new MenuCategory();
                string htmlCategory = categories[i];
                // searches for "section-subtitle", then finds the closing tag and captures the title between the tags
                var nameMatch = Regex.Match(htmlCategory, "section-subtitle.*?>(.*?)<");
                category.Name = nameMatch.Groups[1].Value;

                int addonsIdx = htmlCategory.IndexOf("menu__addOns");
                foreach(Match match in Regex.Matches(htmlCategory, "viewItem.*?>(.*?)<"))
                {
                    string itemName = WebUtility.HtmlDecode(match.Groups[1].Value);
                    if (match.Index < addonsIdx || addonsIdx == -1)
                    {
                        
                        category.Items.Add(itemName);
                    }
                    else
                    {
                        category.AddonItems.Add(itemName);
                    }
                }

                if (category.Items.Count > 0 || category.AddonItems.Count > 0)
                    this.Categories.Add(category);
            }

            return true;
        }
    }

    public enum Location
    {
        Brooks = 6523,
        EastVillage = 6524,
        Penland = 1047,
        Memo = 1045
    }

    public enum Meal
    {
        Breakfast = 1117, Lunch = 1118, Dinner = 1119, Brunch = 1120
    }

    class MealInfo
    {
        public static List<Meal> GetValidMeals(DateTime day, Location location)
        {
            if (day.DayOfWeek == DayOfWeek.Friday)
            {
                if (location == Location.Brooks)
                    return new List<Meal> {Meal.Breakfast, Meal.Lunch};
                else
                    return new List<Meal> { Meal.Breakfast, Meal.Lunch, Meal.Dinner };
            }
            if (day.DayOfWeek == DayOfWeek.Saturday)
            {
                if (location == Location.Penland)
                    return new List<Meal> {Meal.Brunch};
                else
                    return new List<Meal>();
            }
            if (day.DayOfWeek == DayOfWeek.Sunday)
            {
                if (location == Location.Penland)
                    return new List<Meal> { Meal.Brunch, Meal.Dinner };
                if (location == Location.EastVillage)
                    return new List<Meal> { Meal.Dinner };
                else
                    return new List<Meal>();
            }
            return new List<Meal> { Meal.Breakfast, Meal.Lunch, Meal.Dinner};
        }
    }

    [Serializable]
    class MenuCategory
    {
        public string Name { get; set; }
        public List<string> Items { get; set; } = new List<string>();
        public List<string> AddonItems { get; set; } = new List<string>();
    }
}
