using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FantasySportsMVC.Models;

namespace FantasySportsMVC.ViewModel
{
    public class PositionViewModel
    {
        public PlayerDetails Players { get; set; }
        public TournamentDetails Tournaments { get; set; }
        public List<SelectListItem> TournamentsSelectList { get; set; }
    }
}