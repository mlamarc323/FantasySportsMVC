using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FantasySportsMVC.Models
{
    public class TournamentDetails
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Yardage { get; set; }
        public string Par { get; set; }
        public string Date { get; set; }
        public string Year { get; set; }
        public List<SelectListItem> Tournaments { get; set; }
    }
}