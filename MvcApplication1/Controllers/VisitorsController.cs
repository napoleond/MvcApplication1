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
    public interface ICommandAgent
    {
        void SendCommand(object command);
    }

    public class CommandQueueAgent : ICommandAgent
    {
        public CommandQueueAgent()
        {
            //TODO init azure queue
        }

        public void SendCommand(object command)
        {
            //TODO add to queue
        }
    }

    public interface IListFactory<T>
    {
        IEnumerable<T> GetList();
    }

    public class VisitorListFactory : IListFactory<Visitor>
    {
        public VisitorListFactory() 
        {
            //TODO init blob storage
        }

        public IEnumerable<Visitor> GetList()
        {
            //TODO fetch and desrialize JSON from storage
        }
    }

    public class VisitorsController : Controller
    {
        private MvcApplication1Context context = new MvcApplication1Context();

        //TODO: make constructor for visitorscontroller, inject dependencies using autofac

        //
        // GET: /Visitors/

        public ViewResult Index()
        {
            //TODO: generate visitor list using factory interface
            return View(context.Visitors.ToList());
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
            //TODO: create command for queue
            if (ModelState.IsValid)
            {
                context.Visitors.Add(visitor);
                context.SaveChanges();
                return RedirectToAction("Index");  
            }

            return View(visitor);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) {
                context.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}