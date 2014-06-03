using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MvcApplication1.Models;

namespace MvcApplication1.Controllers
{
    public class VisitorsController : Controller
    {
        private MvcApplication1Context context = new MvcApplication1Context();
        private ICommandAgent commandAgent;
        private IListFactory<Visitor> visitorListFactory;

        public VisitorsController(ICommandAgent commandAgent, IListFactory<Visitor> visitorListFactory)
        {
            this.commandAgent = commandAgent;
            this.visitorListFactory = visitorListFactory;
        }

        //
        // GET: /Visitors/

        public ViewResult Index()
        {
            return View(this.visitorListFactory.GetList());
        }

        //
        // GET: /Visitors/Create

        public ActionResult Create()
        {
            return View();
        } 

        //
        // POST: /Visitors/Create

        [HttpPost]
        public ActionResult Create(Visitor visitor)
        {
            if (ModelState.IsValid)
            {
                this.commandAgent.SendCommand(visitor);
                return RedirectToAction("Index");  
            }

            return View(visitor);
        }
    }
}