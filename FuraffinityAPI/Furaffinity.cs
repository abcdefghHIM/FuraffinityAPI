using FuraffinityAPI.Enum;
using FuraffinityAPI.Page;
using FuraffinityAPI.Struct;
using HtmlAgilityPack;
using System.Globalization;
using System.Web;

namespace FuraffinityAPI
{
    public class Furaffinity
    {
        internal HttpClientFactory factory;
        internal OrderedSemaphore semaphore;

        private Furaffinity(int maxRequest)
        {
            factory = new HttpClientFactory();
            semaphore = new OrderedSemaphore(maxRequest);
        }

        public static Furaffinity Create(int maxRequest)
        {
            return new Furaffinity(maxRequest);
        }

        public static Furaffinity Create(string a, string b, int maxRequest)
        {
            var obj = new Furaffinity(maxRequest);
            obj.factory.SetCookie(a, b);
            return obj;
        }

        public (string? a, string? b) GetCookie()
        {
            var obj = factory.GetCookies();
            var a = obj["a"];
            var b = obj["b"];
            if (a == null || b == null)
                return (null, null);
            return (a.Value, b.Value);
        }

        public HomePage GetHomePage()
        {
            var page = new HomePage(factory.CreateClient(""), semaphore);
            return page;
        }

        public UserPage GetUserPage(string userName)
        {
            var page = new UserPage(factory.CreateClient(userName.ToLower()), semaphore, userName);
            return page;
        }

        public PageCollection<GalleryPage> GetGallery(string userName)
        {
            var obj = new PageCollection<GalleryPage>(factory.CreateClient(userName.ToLower()), semaphore, userName);
            return obj;
        }

        public PageCollection<FavoritesPage> GetFavorites(string userName)
        {
            var obj = new PageCollection<FavoritesPage>(factory.CreateClient(userName.ToLower()), semaphore, userName);
            return obj;
        }

        public ViewPage GetViewPage(string view)
        {
            var obj = new ViewPage(factory.CreateClient(""), semaphore, $"/view/{view}");
            return obj;
        }

        public ViewContainer[] GetViewContainers(ResourceContainer[] resources)
        {
            var containers = new ViewContainer[resources.Length];
            var tasks = new Task<ViewContainer>[resources.Length];
            var uuid=Guid.NewGuid().ToString();
            for (int i = 0; i < resources.Length; i++)
            {
                var resource = resources[i];
                var task = new Task<ViewContainer>(() =>
                {
                    var obj = new ViewPage(factory.CreateClient(uuid), semaphore, resource.Url);
                    return obj.GetViewContainerAsync().Result;
                });
                tasks[i] = task;
                task.Start();
            }
            Task.WaitAll(tasks);
            for (int i = 0; i < resources.Length; i++)
            {
                containers[i] = tasks[i].Result;
            }
            return containers;
        }




        public static Rating ParseRestriction(string value)
        {
            switch (value)
            {
                case "r-general":
                    return Rating.General;
                case "r-mature":
                    return Rating.Mature;
                case "r-adult":
                    return Rating.Adult;
                case "General":
                    return Rating.General;
                case "Mature":
                    return Rating.Mature;
                case "Adult":
                    return Rating.Adult;

            }
            return Rating.General;
        }

        public static ResourceType ParseResourceType(string value)
        {
            switch (value)
            {
                case "t-image":
                    return ResourceType.Image;
                case "t-text":
                    return ResourceType.Text;
                case "r-audio":
                    return ResourceType.Audio;
            }
            if (value.Contains("Artwork"))
                return ResourceType.Image;
            if (value.Contains("Story"))
                return ResourceType.Text;
            if (value.Contains("Poetry"))
                return ResourceType.Text;
            if(value.Contains("Music"))
                return ResourceType.Audio;
            return ResourceType.Image;
        }

        internal static ResourceContainer[] ParseResourceContainer(HtmlNodeCollection figures)
        {
            ResourceContainer[] resourceContainers = new ResourceContainer[figures.Count];
            for (int i = 0; i < figures.Count; i++)
            {
                var fig = figures[i];
                var classes = fig.GetClasses().ToArray();
                var img = fig.SelectSingleNode(".//img");
                var title = fig.SelectSingleNode("./figcaption/p[1]/a");
                var user = fig.SelectSingleNode("./figcaption/p[2]/a");
                ResourceContainer container = new ResourceContainer()
                {
                    Rating = ParseRestriction(classes[0]),
                    Type = ParseResourceType(classes[1]),
                    User = user.InnerText.Trim(),
                    Title = title.InnerText.Trim(),
                    Url = fig.SelectSingleNode(".//a").Attributes["href"].Value,
                    ImageUrl = "https:" + img.Attributes["src"].Value,
                    Sid = long.Parse(fig.Attributes["id"].Value.Substring(4)),
                    Width = float.Parse(img.Attributes["data-width"].Value),
                    Height = float.Parse(img.Attributes["data-height"].Value)
                };
                resourceContainers[i] = container;
            }
            return resourceContainers;
        }

        internal static ViewContainer ParseViewContainer(HtmlDocument doc)
        {
            string format = "MMM d, yyyy h:mm tt";
            string[] wh = doc.DocumentNode.SelectSingleNode("//section[@class='info text']/div[4]/span").InnerText.Trim().Replace(" ", "").Split('x');

            var tags = doc.DocumentNode.SelectNodes("//section[@class='tags-row']//a");
            string[] a = { };
            if (tags != null)
            {
                a = tags.Select(e => e.InnerText.Trim()).ToArray();
            }

            ViewContainer container = new ViewContainer()
            {
                Rating = ParseRestriction(doc.DocumentNode.SelectSingleNode("//div[@class='rating']/span[1]").InnerText.Trim()),
                Type = ParseResourceType(doc.DocumentNode.SelectSingleNode("//section[@class='info text']/div[1]//span[1]").InnerText.Trim()),
                Title = doc.DocumentNode.SelectSingleNode("//div[@class='submission-title']").InnerText.Trim(),
                Description = HttpUtility.HtmlDecode(doc.DocumentNode.SelectSingleNode("//div[contains(@class,'submission-description')]").InnerText.Trim()),
                ResourceUrl = "http:" + doc.DocumentNode.SelectSingleNode("//a[normalize-space(text())='Download']").Attributes["href"].Value,
                User = doc.DocumentNode.SelectSingleNode("//div[@class='submission-id-sub-container']//a").InnerText.Trim(),
                AvatarUrl = "https:" + doc.DocumentNode.SelectSingleNode("//img[contains(@class,'submission-user-icon')]").Attributes["src"].Value,
                PostTime = DateTime.ParseExact(doc.DocumentNode.SelectSingleNode("//div[@class='submission-id-sub-container']/strong/span").Attributes["title"].Value, format, CultureInfo.InvariantCulture),
                Views = int.Parse(doc.DocumentNode.SelectSingleNode("//div[@class='views']/span[1]").InnerText.Trim()),
                Comments = int.Parse(doc.DocumentNode.SelectSingleNode("//div[@class='comments']/span[1]").InnerText.Trim()),
                Favorites = int.Parse(doc.DocumentNode.SelectSingleNode("//div[@class='favorites']/span[1]").InnerText.Trim()),
                Category = doc.DocumentNode.SelectSingleNode("//span[@class='type-name']").InnerText.Trim(),
                Species = doc.DocumentNode.SelectSingleNode("//section[@class='info text']/div[2]/span").InnerText.Trim().Replace(" ", "").Split('/'),
                Gender = doc.DocumentNode.SelectSingleNode("//section[@class='info text']/div[3]/span").InnerText.Trim(),
                Width = int.Parse(wh[0]),
                Height = int.Parse(wh[1]),
                Tags = a
            };

            return container;
        }
    }
}
