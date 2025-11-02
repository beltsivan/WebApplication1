using Microsoft.AspNetCore.Mvc;
using WebApplication1.Data;
using WebApplication1.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System.IO;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace WebApplication1.Controllers
{
    public class GatePassController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public GatePassController(ApplicationDbContext db, IWebHostEnvironment webHostEnvironment)
        {
            _db = db;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: Gatepass
        public IActionResult Index()
        {
            // Show create form (Views/GatePass/Index.cshtml)
            return View();
        }

        // Displays all gatepass records to match Views/GatePass/List.cshtml
        public IActionResult List()
        {
            IEnumerable<GatePassModel> gatepasses = _db.GatePasses.OrderByDescending(g => g.DateSubmitted).ToList();
            return View(gatepasses);
        }

        // GET: GatePass/Create -> keep for explicit route
        [HttpGet]
        public IActionResult Create()
        {
            return View("Index");
        }

        // POST: GatePass/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(GatePassModel model, IFormFile OrDocumentPath, IFormFile CrDocumentPath)
        {
            // Validate uploaded files
            var allowedExt = new[] { ".pdf", ".jpg", ".jpeg", ".png" };
            var allowedTypes = new[] { "application/pdf", "image/jpeg", "image/png" };
            const long maxFileSize = 5 * 1024 * 1024; // 5 MB

            if (OrDocumentPath == null || OrDocumentPath.Length == 0)
            {
                ModelState.AddModelError("OrDocumentPath", "Please upload the OR file.");
            }
            else
            {
                var ext = Path.GetExtension(OrDocumentPath.FileName)?.ToLowerInvariant();
                if (string.IsNullOrEmpty(ext) || !allowedExt.Contains(ext))
                    ModelState.AddModelError("OrDocumentPath", "OR file must be .pdf, .jpg, .jpeg or .png");

                if (!allowedTypes.Contains(OrDocumentPath.ContentType) && !OrDocumentPath.ContentType.StartsWith("image/"))
                    ModelState.AddModelError("OrDocumentPath", "Invalid OR file content type.");

                if (OrDocumentPath.Length > maxFileSize)
                    ModelState.AddModelError("OrDocumentPath", "OR file must be 5 MB or less.");
            }

            if (CrDocumentPath == null || CrDocumentPath.Length == 0)
            {
                ModelState.AddModelError("CrDocumentPath", "Please upload the CR file.");
            }
            else
            {
                var ext = Path.GetExtension(CrDocumentPath.FileName)?.ToLowerInvariant();
                if (string.IsNullOrEmpty(ext) || !allowedExt.Contains(ext))
                    ModelState.AddModelError("CrDocumentPath", "CR file must be .pdf, .jpg, .jpeg or .png");

                if (!allowedTypes.Contains(CrDocumentPath.ContentType) && !CrDocumentPath.ContentType.StartsWith("image/"))
                    ModelState.AddModelError("CrDocumentPath", "Invalid CR file content type.");

                if (CrDocumentPath.Length > maxFileSize)
                    ModelState.AddModelError("CrDocumentPath", "CR file must be 5 MB or less.");
            }

            if (!ModelState.IsValid)
            {
                return View("Index", model);
            }

            string webRoot = _webHostEnvironment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            string uploadFolder = Path.Combine(webRoot, "uploads");

            if (!Directory.Exists(uploadFolder))
                Directory.CreateDirectory(uploadFolder);

            // Upload OR file
            if (OrDocumentPath != null && OrDocumentPath.Length > 0)
            {
                string orFileName = Guid.NewGuid().ToString() + Path.GetExtension(OrDocumentPath.FileName);
                string orPath = Path.Combine(uploadFolder, orFileName);
                using (var stream = new FileStream(orPath, FileMode.Create))
                {
                    await OrDocumentPath.CopyToAsync(stream);
                }
                model.OrDocumentPath = "/uploads/" + orFileName;
            }

            // Upload CR file
            if (CrDocumentPath != null && CrDocumentPath.Length > 0)
            {
                string crFileName = Guid.NewGuid().ToString() + Path.GetExtension(CrDocumentPath.FileName);
                string crPath = Path.Combine(uploadFolder, crFileName);
                using (var stream = new FileStream(crPath, FileMode.Create))
                {
                    await CrDocumentPath.CopyToAsync(stream);
                }
                model.CrDocumentPath = "/uploads/" + crFileName;
            }

            model.DateSubmitted = DateTime.Now;
            model.Status ??= "Pending";

            _db.GatePasses.Add(model);
            _db.SaveChanges();
            return RedirectToAction("List");
        }
    }
}
