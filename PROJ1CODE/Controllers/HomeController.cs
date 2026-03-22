/*!
 * @file Controllers/HomeController.cs
 * @brief Standard application home controller for site pages (Index, Privacy, Error).
 * @ingroup Controllers
 */

using Microsoft.AspNetCore.Mvc;
using Project_6___Group_4___CSCN73060_SEC_1.Models;
using System.Diagnostics;

namespace Project_6___Group_4___CSCN73060_SEC_1.Controllers
{
    /// <summary>
    /// Standard application home controller that serves the main site pages
    /// such as Index and Privacy, and provides the Error view.
    /// </summary>
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        /// <summary>
        /// Initializes a new instance of <see cref="HomeController"/>.
        /// </summary>
        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Render the homepage view.
        /// </summary>
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Render the privacy page view.
        /// </summary>
        public IActionResult Privacy()
        {
            return View();
        }

        /// <summary>
        /// Render the error page with request id information.
        /// </summary>
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
