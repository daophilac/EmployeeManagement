using EmployeeManagement.Models;
using EmployeeManagement.Security;
using EmployeeManagement.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace EmployeeManagement.Controllers {
    [Authorize]
    public class HomeController : Controller {
        private readonly IEmployeeRepository employeeRepository;
        private readonly IHostingEnvironment hostingEnvironment;
        private readonly ILogger logger;
        private readonly IDataProtector protector;

        public HomeController(
            IEmployeeRepository employeeRepository,
            IHostingEnvironment hostingEnvironment,
            ILogger<HomeController> logger,
            IDataProtectionProvider dataProtectionProvider,
            DataProtectionPurposeStrings dataProtectionPurposeStrings) {
            this.employeeRepository = employeeRepository;
            this.hostingEnvironment = hostingEnvironment;
            this.logger = logger;
            protector = dataProtectionProvider.CreateProtector(dataProtectionPurposeStrings.EmployeeIdRouteValue);
        }


        [AllowAnonymous]
        public ViewResult Index() {
            var model = employeeRepository.GettAllEmployee().Select(e => {
                e.EncryptedId = protector.Protect(e.Id.ToString());
                return e;
            });
            return View(model);
        }

        [AllowAnonymous]
        public ViewResult Details(string id) {
            //throw new Exception("Exception from Details View");
            logger.LogTrace("LogTrace Log");
            logger.LogDebug("LogDebug Log");
            logger.LogInformation("LogInformation Log");
            logger.LogWarning("LogWarning Log");
            logger.LogError("LogError Log");
            logger.LogCritical("LogCritical Log");
            int employeeId = Convert.ToInt32(protector.Unprotect(id));
            Employee employee = employeeRepository.GetEmployee(employeeId);
            if(employee == null) {
                Response.StatusCode = 404;
                return View("EmployeeNotFound", employeeId);
            }
            HomeDetailsViewModel homeDetailsViewModel = new HomeDetailsViewModel {
                Employee = employee,
                PageTitle = "Employee Details"
            };
            return View(homeDetailsViewModel);
        }

        [HttpGet]
        public ViewResult Create() {
            return View();
        }

        [HttpPost]
        public IActionResult Create(EmployeeCreateViewModel model) {
            if (ModelState.IsValid) {
                string uniqueFileName = ProcessUploadedFile(model);
                Employee newEmployee = new Employee {
                    Name = model.Name,
                    Email = model.Email,
                    Department = model.Department,
                    PhotoPath = uniqueFileName
                };
                employeeRepository.Add(newEmployee);
                return RedirectToAction("details", new { id = newEmployee.Id });
            }
            return View();
        }

        [HttpGet]
        public ViewResult Edit(int id) {
            Employee employee = employeeRepository.GetEmployee(id);
            EmployeeEditViewModel employeeEditViewModel = new EmployeeEditViewModel {
                Id = employee.Id,
                Name = employee.Name,
                Email = employee.Email,
                Department = employee.Department,
                ExistingPhotoPath = employee.PhotoPath,
            };
            return View(employeeEditViewModel);
        }

        [HttpPost]
        public IActionResult Edit(EmployeeEditViewModel model) {
            if (ModelState.IsValid) {
                Employee employee = employeeRepository.GetEmployee(model.Id);
                employee.Name = model.Name;
                employee.Email = model.Email;
                employee.Department = model.Department;
                if(model.Photo != null) {
                    if (model.ExistingPhotoPath != null) {
                        string filePath = 
                            Path.Combine(hostingEnvironment.WebRootPath, "images", model.ExistingPhotoPath);
                        System.IO.File.Delete(filePath);
                    }
                    employee.PhotoPath = ProcessUploadedFile(model);
                }
                employeeRepository.Update(employee);
                return RedirectToAction("index");
            }
            return View();
        }

        private string ProcessUploadedFile(EmployeeCreateViewModel model) {
            string uniqueFileName = null;
            if (model.Photo != null) {
                string uploadFolder = Path.Combine(hostingEnvironment.WebRootPath, "images");
                uniqueFileName = Guid.NewGuid().ToString() + "_" + model.Photo.FileName;
                string filePath = Path.Combine(uploadFolder, uniqueFileName);
                using (var stream = new FileStream(filePath, FileMode.Create)) {
                    model.Photo.CopyTo(stream);
                }
            }

            return uniqueFileName;
        }
    }
}
