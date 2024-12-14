# FuraffinityAPI

这是一个API，这是用法

```c#
static void Main(string[] args)
{
    var api = Furaffinity.Create("你的a", "你的b", 10);
    var user = furry.GetUserPage("xxxx");
    var task = user.Notify();
    try
    {
        task.Wait();
    }
    catch
    {
        return;
    }
    var name = user.GetNameAsync().Result;// 获取用户名
    var avatar = user.GetAvatarUrlAsync().Result;// 获取头像
    var page = api.GetGallery(name);
    while (true)
    {
        var c = page.GetResourceContainers();
        if (c == null)
        {
            break;
        }
        var con = api.GetViewContainers(c);
        con.ToList().ForEach(e => Console.WriteLine(e.Title));
    }
}
```

