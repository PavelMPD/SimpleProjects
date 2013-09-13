using System;
using System.Data;
using System.Data.Entity;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Diary.Models;

namespace Diary.Controllers
{
    public class TasksController : Controller
    {
        DIARYEntities entities = new DIARYEntities();
        //
        // GET: /Tasks/

        public ActionResult Index()
        {
            List<TaskStatus> list = entities.TaskStatus.ToList();
            //MvcApplication.logger.Info(entities.TaskStatus.ToTraceString());
            ViewBag.TaskStatuses = GetTaskStatuses(list);
            return View(list);
        }

        public ActionResult AddTask()
        {
            return View();
        }

        private IEnumerable<SelectListItem> GetTaskStatuses(IList<TaskStatus> list)
        {
            return list.Select(ts => new SelectListItem() { Text = ts.Name, Value = ts.Id.ToString()});
        }
    }
}
