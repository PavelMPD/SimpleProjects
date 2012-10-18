using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ED.Repository;
using ED.Repository.Interfaces;
using ED.Domen.Entities;

namespace EnglishDictionary.Controllers
{
    public class TaskController : Controller
    {
        private IRepository<Task> taskRepository = new TaskRepository();
        //
        // GET: /Task/

        public ActionResult Index()
        {

            return View(taskRepository.GetList());
        }

    }
}
