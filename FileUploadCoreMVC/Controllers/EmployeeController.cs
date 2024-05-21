using FileUploadCoreMVC.Data;
using FileUploadCoreMVC.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;

namespace FileUploadCoreMVC.Controllers
{
    public class EmployeeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _hostEnvironment;

        public EmployeeController(ApplicationDbContext context, IWebHostEnvironment hostEnvironment)
        {
            _context = context;
            _hostEnvironment = hostEnvironment;
        }

        //List
        public IActionResult Index()
        {
            var employeeList = _context.Employee.ToList();
            return View(employeeList);
        }

        //Create
        [HttpGet]
        public IActionResult Create()
        {
            var newEmployee = new Employee();
            return View(newEmployee);
        }
        [HttpPost]
        public async Task<IActionResult> Create(Employee employee, IFormFile profileFile)
        {
            if (ModelState.IsValid)
            {
                var existEmployee = _context.Employee.FirstOrDefault(e => e.EmployeeEmail == employee.EmployeeEmail);
                if (existEmployee != null)
                {
                    ModelState.AddModelError("EmployeeEmail", "User already registered");
                    return View(employee);
                }

                // Handle file upload
                if (profileFile != null)
                {
                    string uploadDir = Path.Combine(_hostEnvironment.WebRootPath, "uploads");
                    Directory.CreateDirectory(uploadDir);
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(profileFile.FileName);
                    string filePath = Path.Combine(uploadDir, fileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await profileFile.CopyToAsync(fileStream);
                    }

                    employee.Profile = fileName; // Store the file name or path
                }


                _context.Employee.Add(employee);
                await _context.SaveChangesAsync();

                return RedirectToAction("Index");
            }
            return View(employee);
        }

        //Update
        [HttpGet]
        public IActionResult Edit(int id, IFormFile profileFile)
        {
            var employee = _context.Employee.Find(id);

            if (employee == null)
            {
                return NotFound();
            }

            return View(employee);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, Employee employee, IFormFile profileFile)
        {
            if (ModelState.IsValid)
            {
                var existingEmployee = _context.Employee.Find(id);
                if (existingEmployee == null)
                {
                    return NotFound();
                }

                // Update existing fields
                existingEmployee.EmployeeName = employee.EmployeeName;
                existingEmployee.EmployeeEmail = employee.EmployeeEmail;
                existingEmployee.Password = employee.Password;
                existingEmployee.Address = employee.Address;
                existingEmployee.LastUpdate = DateTime.Now;

                // Handle file upload if a new file is provided
                if (profileFile != null)
                {
                    // Delete the old profile picture if exists
                    if (!string.IsNullOrEmpty(existingEmployee.Profile))
                    {
                        var oldFilePath = Path.Combine(_hostEnvironment.WebRootPath, "uploads", existingEmployee.Profile);
                        if (System.IO.File.Exists(oldFilePath))
                        {
                            System.IO.File.Delete(oldFilePath);
                        }
                    }

                    // Save the new profile picture
                    string uploadDir = Path.Combine(_hostEnvironment.WebRootPath, "uploads");
                    Directory.CreateDirectory(uploadDir);
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(profileFile.FileName);
                    string filePath = Path.Combine(uploadDir, fileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await profileFile.CopyToAsync(fileStream);
                    }

                    existingEmployee.Profile = fileName;
                }

                _context.Update(existingEmployee);
                await _context.SaveChangesAsync();

                return RedirectToAction("Index");
            }
            return View(employee);
        }

        //Delete
        [HttpGet]
        public IActionResult Delete(int id)
        {
            var employee = _context.Employee.Find(id);

            if (employee == null)
            {
                return NotFound();
            }

            return View(employee);
        }

        [HttpPost, ActionName("Delete")]
        public IActionResult DeleteConfirmed(int id)
        {
            var employee = _context.Employee.Find(id);

            if (employee != null)
            {
                // Delete the profile image if it exists
                if (!string.IsNullOrEmpty(employee.Profile))
                {
                    var filePath = Path.Combine(_hostEnvironment.WebRootPath, "uploads", employee.Profile);
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                }

                _context.Employee.Remove(employee);
                _context.SaveChanges();
            }

            return RedirectToAction("Index");
        }

    }
}
