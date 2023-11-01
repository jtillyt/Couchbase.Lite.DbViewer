using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DbViewer.Hub.Controllers
{
	public class DatabaseController : Controller
	{
		public ActionResult Index()
		{
			return View();
		}

		// GET: HubController/Details/5
		public ActionResult Details(int id)
		{
			return View();
		}

		// GET: HubController/Create
		public ActionResult Create()
		{
			return View();
		}

		// POST: HubController/Create
		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult Create(IFormCollection collection)
		{
			try
			{
				return RedirectToAction(nameof(Index));
			}
			catch
			{
				return View();
			}
		}

		// GET: HubController/Edit/5
		public ActionResult Edit(int id)
		{
			return View();
		}

		// POST: HubController/Edit/5
		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult Edit(int id, IFormCollection collection)
		{
			try
			{
				return RedirectToAction(nameof(Index));
			}
			catch
			{
				return View();
			}
		}

		// GET: HubController/Delete/5
		public ActionResult Delete(int id)
		{
			return View();
		}

		// POST: HubController/Delete/5
		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult Delete(int id, IFormCollection collection)
		{
			try
			{
				return RedirectToAction(nameof(Index));
			}
			catch
			{
				return View();
			}
		}
	}
}
