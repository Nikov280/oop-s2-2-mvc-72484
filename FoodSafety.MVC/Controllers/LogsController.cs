using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize(Roles = "Admin")] 

public class LogsController : Controller
{
    private readonly IWebHostEnvironment _env;

    public LogsController(IWebHostEnvironment env) => _env = env;

    public IActionResult Index()
    {
        var logPath = Path.Combine(_env.ContentRootPath, "Logs");
        var files = Directory.GetFiles(logPath).Select(Path.GetFileName).ToList();
        return View(files);
    }

    public IActionResult Download(string filename)
    {
        var filePath = Path.Combine(_env.ContentRootPath, "Logs", filename);
        return PhysicalFile(filePath, "text/plain");
    }
}