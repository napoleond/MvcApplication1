using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;

namespace MvcApplication1.Models
{
    [DataContract]
    public class Visitor
    {
        [DataMember]
        public int Id { get; set; }

        [DataMember]
        public string FirstName { get; set; }

        [DataMember]
        public string LastName { get; set; }

        [DataMember]
        public string Phrase { get; set; }

        [DataMember]
        public DateTime Date { get; set; }
    }
}