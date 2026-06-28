using FileUploadwithModelandPreview.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static FileUploadwithModelandPreview.Models.UploadFile;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;


namespace FileUploadwithModelandPreview.Controllers
{
    public class FileController : Controller
    {
        private readonly IWebHostEnvironment _env;

        public FileController(IWebHostEnvironment env)
        {
            _env = env;
        }

        private UploadFileDBContext CreateDbContext()
        {
            // EF6 on .NET 8 does not resolve "Name=" entries from appsettings.json;
            // the connection string value is passed directly from IConfiguration instead.
            // The connection string is read each time a context is needed so that
            // the controller itself stays free of IConfiguration constructor injection
            // (keeping the change minimal).  In practice, inject IConfiguration and
            // cache the value in a field if preferred.
            var config = HttpContext.RequestServices
                .GetRequiredService<Microsoft.Extensions.Configuration.IConfiguration>();
            var cs = config.GetConnectionString("FileUploadwithModelandPreview");
            return new UploadFileDBContext(cs!);
        }

        // GET: File
        public ActionResult UploadForm()
        {
            return View("UploadFile");
        }

        //one file upload version :
        /*
        [HttpPost]
        public ActionResult Upload(IFormFile file)
        {
            if (file != null && file.Length > 0)
            {
                string _FileName = Path.GetFileName(file.FileName);
                string _path = Path.Combine(_env.WebRootPath, "UploadedFiles", _FileName);
                using (var stream = new FileStream(_path, FileMode.Create))
                {
                    file.CopyTo(stream);
                }

                var model = new UploadFile
                {
                    FileName = _FileName,
                    ContentType = file.ContentType
                };
                ViewBag.Message = "File Uploaded Successfully!!";
                return RedirectToAction("UploadForm");
            }
            ViewBag.Message = "File upload failed!!";
            return RedirectToAction("UploadForm");
        }*/
        [HttpPost]
        public ActionResult Upload(IFormFile file1, IFormFile file2)
        {
            if ((file1 != null && file1.Length > 0) && (file2 != null && file2.Length > 0))
            {
                string uploadsDir = Path.Combine(_env.ContentRootPath, "UploadedFiles");
                Directory.CreateDirectory(uploadsDir);

                string _FileName1 = Path.GetFileName(file1.FileName);
                string _path1 = Path.Combine(uploadsDir, _FileName1);
                using (var stream1 = new FileStream(_path1, FileMode.Create))
                {
                    file1.CopyTo(stream1);
                }

                // Save the file information to the database or session, if needed.
                //Saving the data in the DB
                byte[] fileContent1;
                using (var binaryReader1 = new BinaryReader(file1.OpenReadStream()))
                {
                    fileContent1 = binaryReader1.ReadBytes((int)file1.Length);
                }
                var model1 = new UploadFile
                {
                    FileName = file1.FileName,
                    ContentType = file1.ContentType,
                    Content = fileContent1
                };
                // Save the uploadedFile object to the database using your DbContext
                using (var db = CreateDbContext())
                {
                    db.UploadFiles.Add(model1);
                    db.SaveChanges();
                }

                string _FileName2 = Path.GetFileName(file2.FileName);
                string _path2 = Path.Combine(uploadsDir, _FileName2);
                using (var stream2 = new FileStream(_path2, FileMode.Create))
                {
                    file2.CopyTo(stream2);
                }

                // Save the file information to the database or session, if needed.
                //Saving the data in the DB
                byte[] fileContent2;
                using (var binaryReader2 = new BinaryReader(file2.OpenReadStream()))
                {
                    fileContent2 = binaryReader2.ReadBytes((int)file2.Length);
                }
                var model2 = new UploadFile
                {
                    FileName = file2.FileName,
                    ContentType = file2.ContentType,
                    Content = fileContent2
                };
                ViewBag.Message = "File Uploaded Successfully!!";

                // Save the uploadedFile object to the database using your DbContext
                using (var db = CreateDbContext())
                {
                    db.UploadFiles.Add(model2);
                    db.SaveChanges();
                }
            }
            else
            {
                ViewBag.Message = "!!! WARNING !!! Both files must be uploaded!!";
                return View("UploadFile");
            }

            return View("UploadFile");
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return BadRequest();
            }
            using (var db = CreateDbContext())
            {
                UploadFile UploadedFile = db.UploadFiles.Find(id);
                if (UploadedFile == null)
                {
                    return NotFound();
                }
                db.UploadFiles.Remove(UploadedFile);
                db.SaveChanges();
            }
            return RedirectToAction("ViewOldUpload");
        }

        public ActionResult ViewOldUpload()
        {
            using (var db = CreateDbContext())
            {
                return View(db.UploadFiles.ToList());
            }
        }
    }
}
