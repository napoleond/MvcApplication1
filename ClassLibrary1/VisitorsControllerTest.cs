using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using NUnit.Framework;
using MvcApplication1.Models;
using MvcApplication1.Controllers;

[TestFixture]
public class VisitorsControllerTest
{
    //create mocks
    public class MockCommandAgent : ICommandAgent
    {
        public void SendCommand(object command)
        {
            //do nothing; a more interesting mock might build the list in memory
            //to test adding items
        }
    }

    public class MockListFactory : IListFactory<Visitor>
    {
        public IEnumerable<Visitor> GetList()
        {
            //again, a more interesting mock might actually return some data
            return new List<Visitor>();
        }
    }

    private VisitorsController controller;

    [SetUp]
    public void Init()
    {
        MockCommandAgent mockCommander = new MockCommandAgent();
        MockListFactory mockListFactory = new MockListFactory();
        controller = new VisitorsController(mockCommander, mockListFactory);
    }

    [Test]
    public void DoesReturnViewItem()
    {
        var result = controller.Index() as ViewResult;
        Assert.AreEqual("Index", result.ViewName);
    }
}
