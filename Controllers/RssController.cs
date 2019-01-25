using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Linq;
using System.Xml;
using System.ServiceModel.Syndication;
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
        public FileResult Index(string path, bool recursive = false, string title = null)
        {
            path = ValidatePath(path);

            var baseDirInfo = new DirectoryInfo(path);

            var feed = new SyndicationFeed(
                title ?? baseDirInfo.Name,
                $"{(recursive ? "Recursive " : "")}Filefeed generated feed for {baseDirInfo.FullName}",
                new Uri($"{HttpContext.Request.Scheme}://{HttpContext.Request.Host.Value}")
            );

            var folderImage = baseDirInfo.GetFiles("folder.*").FirstOrDefault();
            if (folderImage != null)
            {
                feed.ImageUrl = new Uri($"{HttpContext.Request.Scheme}://{HttpContext.Request.Host.Value}" + Url.Action("GetFile", new { Path = folderImage.FullName }));
            }

            var files = baseDirInfo.EnumerateFiles(
                "*",
                new EnumerationOptions { RecurseSubdirectories = recursive })
                    .Where(x => !x.Attributes.HasFlag(FileAttributes.Hidden) && !x.Name.ToLower().StartsWith("folder."))
                    .Take(100)
                    .OrderBy(x => x.FullName);

            var items = new List<SyndicationItem>(files.Count());

            //We fake the date to get a publish date in the same order as alphabetical
            var pubDate = new DateTime(DateTime.Now.Year - 1, 1, 1, 12, 0, 0);
            foreach (var file in files)
            {
                if (!_contentTypeProvider.TryGetContentType(file.Name, out var type))
                {
                    type = System.Net.Mime.MediaTypeNames.Application.Octet;
                }

                var item = new SyndicationItem
                {
                    Title = new TextSyndicationContent(file.Name),
                    Id = Url.Action("GetFile", new { Path = file.FullName }),
                    PublishDate = pubDate,
                };
                item.Links.Add(SyndicationLink.CreateMediaEnclosureLink(
                    new Uri($"{HttpContext.Request.Scheme}://{HttpContext.Request.Host.Value}" + Url.Action("GetFile", new { Path = file.FullName })),
                    type,
                    file.Length));
                pubDate = pubDate.AddDays(1);

                items.Add(item);
            }
            feed.Items = items;

            var settings = new XmlWriterSettings
            {
                Encoding = System.Text.Encoding.UTF8,
                NewLineHandling = NewLineHandling.Entitize,
                NewLineOnAttributes = true,
                Indent = true
            };

            Rss20FeedFormatter rssFormatter = new Rss20FeedFormatter(feed);
            rssFormatter.SerializeExtensionsAsAtom = false;
            using (var stream = new MemoryStream())
            {
                using (var writer = XmlWriter.Create(stream, settings))
                {
                    rssFormatter.WriteTo(writer);
                    writer.Flush();
                    return File(stream.ToArray(), "application/rss+xml; charset=utf-8");
                }
            }
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
