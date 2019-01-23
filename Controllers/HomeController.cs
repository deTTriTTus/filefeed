using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.IO;
using filefeed.Models;

namespace filefeed.Controllers
{
    public class HomeController : BaseController
    {
        public HomeController(IConfiguration configuration) : base(configuration) { }

        [HttpGet("{path?}")]
        public IActionResult Index(string path = "")
        {
            path = ValidatePath(path);

            path = path.Replace('!', '/');

            var baseDir = new System.IO.DirectoryInfo(path);
            var model = new RssListViewModel
            {
                CurrentDir = baseDir,
                Directories = baseDir.EnumerateDirectories()
                    .Where(x => !x.Attributes.HasFlag(FileAttributes.Hidden))
                    .Take(100)
                    .OrderBy(x => x.Name),
                Files = baseDir.EnumerateFiles()
                    .Where(x => !x.Attributes.HasFlag(FileAttributes.Hidden))
                    .Take(100)
                    .OrderBy(x => x.Name),
            };

            model.DisplayParentLink = path != _baseDir && model.CurrentDir.Parent != null;

            return View(model);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
