using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FantasySportsMVC.Models
{
    public class PlayerDetails
    {
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public string Position { get; set; }
        public string TotalScore { get; set; }
        public string RoundOneScore { get; set; }
        public string RoundTwoScore { get; set; }
        public string RoundThreeScore { get; set; }
        public string RoundFourScore { get; set; }
        public decimal MoneyEarned { get; set; }
        public string Tournament { get; set; }

        public List<SelectListItem> TournamentsSelectList { get; set; }

        
    }
}