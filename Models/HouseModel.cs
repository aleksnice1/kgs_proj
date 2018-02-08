using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Kadastr.WebApp.Models
{
    public class HouseModel
    {
        private string _HOUSENUM;
        private string _BUILDNUM;
        private int _STRSTATUS;
        private string _POSTALCODE;
        private string _NUMBER;

        public string HOUSENUM { get => _HOUSENUM; set => _HOUSENUM = value; }
        public string BUILDNUM { get => _BUILDNUM; set => _BUILDNUM = value; }
        public int STRSTATUS { get => _STRSTATUS; set => _STRSTATUS = value; }
        public string POSTALCODE { get => _POSTALCODE; set => _POSTALCODE = value; }
        public string NUMBER { get => _NUMBER; set => _NUMBER = value; }
    }
}