using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Kadastr.WebApp.Models
{
    public class AddresModel
    {
        private string _OFFNAME;
        private string _PARENTGUID;
        private string _SHORTNAME;
        private string _AOLEVEL;
        private string _AOGUID;
        private string _POSTALCODE;

        public string OFFNAME { get => _OFFNAME; set => _OFFNAME = value; }
        public string PARENTGUID { get => _PARENTGUID; set => _PARENTGUID = value; }
        public string SHORTNAME { get => _SHORTNAME; set => _SHORTNAME = value; }
        public string AOLEVEL { get => _AOLEVEL; set => _AOLEVEL = value; }
        public string AOGUID { get => _AOGUID; set => _AOGUID = value; }
        public string POSTALCODE { get => _POSTALCODE; set => _POSTALCODE = value; }
    }
}