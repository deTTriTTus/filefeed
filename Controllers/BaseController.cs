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

namespace filefeed.Controllers
{
    public class BaseController : Controller
    {
        protected readonly string _baseDir;
        public BaseController(IConfiguration configuration)
        {
            _baseDir = configuration.GetSection("BaseDirectory").Get<string>();

            if (!_baseDir.EndsWith("/"))
            {
                _baseDir += "/";
            }
        }

        protected string ValidatePath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                path = _baseDir;
            }
            else
            {
                path = System.Net.WebUtility.UrlDecode(path);
            }

            if (!path.StartsWith("/"))
            {
                path = "/" + path;
            }

            if (!path.EndsWith("/"))
            {
                path = path + "/";
            }

            if (!path.StartsWith(_baseDir))
            {
                path = _baseDir + path;
            }

            return path;
        }
    }
}
