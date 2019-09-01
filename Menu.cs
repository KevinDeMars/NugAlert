using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using HtmlAgilityPack;

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

        public Menu(DateTime day, Location location, Meal meal, HtmlDocument html)
        {
            Day = day;
            Location = location;
            Meal = meal;
            Categories = new List<MenuCategory>();

            var documentCategories = html.DocumentNode.SelectNodes("//*[@class='menu__station']");

            if (documentCategories == null)
            {
                Console.WriteLine($"No menu__station nodes found for {location} {day} {meal}");
                return;
            }

            foreach (var documentCategory in documentCategories)
            {
                var category = new MenuCategory();
                category.Name = documentCategory.SelectSingleNode(".//*[contains(@class, 'section-subtitle')]").GetDirectInnerText();
                category.Name = WebUtility.HtmlDecode(category.Name);

                var docItems = documentCategory.SelectNodes("./div[@class='menu__category']//a[@class='viewItem']");
                var docAddonItems = documentCategory.SelectNodes("./div[@class='menu__addOns']//a[@class='viewItem']");

                if (docItems != null)
                {
                    foreach (var item in docItems)
                    {
                        category.Items.Add(WebUtility.HtmlDecode(item.GetDirectInnerText()));
                    }
                }
                if (docAddonItems != null)
                {
                    foreach (var item in docAddonItems)
                    {
                        category.AddonItems.Add(WebUtility.HtmlDecode(item.GetDirectInnerText()));
                    }
                }
                
                if (category.Items.Count > 0 || category.AddonItems.Count > 0)
                    Categories.Add(category);
            }
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
