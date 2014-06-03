using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using NUnit.Framework;
using ConsoleApplication1;


[TestFixture]
public class ConsoleWorkerTest
{
    [Test]
    public void MockedIFacesWork()
    {
        //this is obviously a pretty useless test, just shows that interfaces abstract out network calls
        MockStore store = new MockStore();
        var message = store.queue.GetMessage();
        Assert.AreEqual(null, message);
    }
}