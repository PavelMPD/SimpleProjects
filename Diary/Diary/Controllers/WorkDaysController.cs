using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Diary.DAL;

namespace Diary.Controllers
{
    public class WorkDaysController : Controller
    {
        DIARYEntities entities = new DIARYEntities();

        //
        // GET: /WorkDay/

        public ActionResult Index()
        {
            List<WorkDay> list = entities.WorkDay.Where(wd => wd.Id > 0 && wd.Date <= DateTime.Now).ToList();
            return View(list);
        }

        public ActionResult AddWorkDay()
        {
            return View();
        }
    }
}
