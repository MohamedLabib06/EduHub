using System.Diagnostics;
using EduHub.Services.Interfaces;
using EduHub.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace EduHub.Web.Controllers;

public class HomeController : Controller
{
    private readonly ICourseService _courseService;

    public HomeController(ICourseService courseService)
    {
        _courseService = courseService;
    }

    public async Task<IActionResult> Index(string? keyword)
    {
        var courses = await _courseService.GetAllPublishedCoursesAsync(searchKeyword: keyword);
        var categories = await _courseService.GetAllCategoriesAsync();

        ViewBag.Categories = categories;
        ViewBag.SearchKeyword = keyword;

        return View(courses);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}