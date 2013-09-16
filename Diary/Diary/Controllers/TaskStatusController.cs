using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Diary.DAL;

namespace Diary.Controllers
{
    public class TaskStatusController : Controller
    {
        private DIARYEntities db = new DIARYEntities();

        //
        // GET: /TaskStatus/

        public ActionResult Index()
        {
            return View(db.TaskStatus.ToList());
        }

        //
        // GET: /TaskStatus/Details/5

        public ActionResult Details(int id = 0)
        {
            TaskStatus taskstatus = db.TaskStatus.Single(t => t.Id == id);
            if (taskstatus == null)
            {
                return HttpNotFound();
            }
            return View(taskstatus);
        }

        //
        // GET: /TaskStatus/Create

        public ActionResult Create()
        {
            return View();
        }

        //
        // POST: /TaskStatus/Create

        [HttpPost]
        public ActionResult Create(TaskStatus taskstatus)
        {
            if (ModelState.IsValid)
            {
                db.TaskStatus.AddObject(taskstatus);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(taskstatus);
        }

        //
        // GET: /TaskStatus/Edit/5

        public ActionResult Edit(int id = 0)
        {
            TaskStatus taskstatus = db.TaskStatus.Single(t => t.Id == id);
            if (taskstatus == null)
            {
                return HttpNotFound();
            }
            return View(taskstatus);
        }

        //
        // POST: /TaskStatus/Edit/5

        [HttpPost]
        public ActionResult Edit(TaskStatus taskstatus)
        {
            if (ModelState.IsValid)
            {
                db.TaskStatus.Attach(taskstatus);
                db.ObjectStateManager.ChangeObjectState(taskstatus, EntityState.Modified);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(taskstatus);
        }

        //
        // GET: /TaskStatus/Delete/5

        public ActionResult Delete(int id = 0)
        {
            TaskStatus taskstatus = db.TaskStatus.Single(t => t.Id == id);
            if (taskstatus == null)
            {
                return HttpNotFound();
            }
            return View(taskstatus);
        }

        //
        // POST: /TaskStatus/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            TaskStatus taskstatus = db.TaskStatus.Single(t => t.Id == id);
            db.TaskStatus.DeleteObject(taskstatus);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}