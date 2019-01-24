using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Linq;
using WilderMinds.RssSyndication;
using filefeed.Models;
using Microsoft.AspNetCore.StaticFiles;

namespace filefeed.Controllers
{
    public class RssController : BaseController
    {
        private readonly IContentTypeProvider _contentTypeProvider;

        public RssController(IConfiguration configuration, IContentTypeProvider contentTypeProvider) : base(configuration)
        {
            _contentTypeProvider = contentTypeProvider;
        }

        [HttpGet("rss/{path?}")]
        public FileResult Index(string path, bool recursive = false)
        {
            path = ValidatePath(path);

            var baseDirInfo = new DirectoryInfo(path);

            Feed feed = new Feed()
            {
                Title = $"filefeed for {baseDirInfo.Name}",
                Description = $"{(recursive ? "Recursive " : "")}Filefeed generated feed for {baseDirInfo.FullName}",
                Link = new Uri($"{HttpContext.Request.Scheme}://{HttpContext.Request.Host.Value}"),
            };

            var files = baseDirInfo.EnumerateFiles(
                "*",
                new EnumerationOptions { RecurseSubdirectories = recursive })
                    .Where(x => !x.Attributes.HasFlag(FileAttributes.Hidden))
                    .Take(100)
                    .OrderBy(x => x.FullName);

            feed.Items = new List<Item>(files.Count());

            //We fake the date to get a publish date in the same order as alphabetical
            var pubDate = new DateTime(DateTime.Now.Year - 1, 1, 1, 12, 0, 0);
            foreach (var file in files)
            {
                var enclosure = new Enclosure();
                enclosure.Values.Add(
                    "url",
                    $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host.Value}" + Url.Action("GetFile", new { Path = file.FullName })

                );
                enclosure.Values.Add("length", file.Length.ToString());
                if (!_contentTypeProvider.TryGetContentType(file.Name, out var type))
                {
                    type = System.Net.Mime.MediaTypeNames.Application.Octet;
                }
                enclosure.Values.Add("type", type);

                var item = new Item
                {
                    Title = file.Name,
                    Permalink = Url.Action("GetFile", new { Path = file.FullName }),
                    Enclosures = new List<Enclosure>() { enclosure },
                    PublishDate = pubDate
                };
                pubDate = pubDate.AddDays(1);

                feed.Items.Add(item);
            }

            var rss = feed.Serialize().Replace("utf-16", "utf-8");

            byte[] byteArray = System.Text.Encoding.UTF8.GetBytes(rss);

            return File(byteArray, System.Net.Mime.MediaTypeNames.Text.Xml);
        }

        [HttpGet("GetFile/{path}")]
        public FileResult GetFile(string path)
        {
            path = ValidatePath(path).TrimEnd('/');
            var stream = System.IO.File.OpenRead(path);
            if (!_contentTypeProvider.TryGetContentType(path, out var type))
            {
                type = System.Net.Mime.MediaTypeNames.Application.Octet;
            }
            return File(stream, type, Path.GetFileName(path));
        }
    }
}
