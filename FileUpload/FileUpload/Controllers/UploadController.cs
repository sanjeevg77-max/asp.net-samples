using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FileUpload.Controllers
{
    public class UploadController : Controller
    {
        private readonly IWebHostEnvironment _env;

        public UploadController(IWebHostEnvironment env)
        {
            _env = env;
        }

        // GET: Upload
        public ActionResult Index()
        {
            return View();
        }
        [HttpGet]
        public ActionResult UploadFile()
        {
            return View();
        }
        [HttpPost]
        public ActionResult UploadFile(IFormFile file)
        {
            try
            {
                if (file != null && file.Length > 0)
                {
                    string _FileName = Path.GetFileName(file.FileName);
                    string _path = Path.Combine(_env.WebRootPath ?? _env.ContentRootPath, "UploadedFiles", _FileName);
using (var stream = new FileStream(_path, FileMode.Create))
                    {
                        file.CopyTo(stream);
                    }
                }
                ViewBag.Message = "File Uploaded Successfully!!";
                return View();
            }
            catch
            {
                ViewBag.Message = "File upload failed!!";
                return View();
            }
        }
    }
}
