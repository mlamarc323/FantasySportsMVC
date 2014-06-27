using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using System.Web.Services;
using System.Web.UI.WebControls;
using System.Xml;
using FantasySportsMVC.Models;
using FantasySportsMVC.ViewModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace FantasySportsMVC.Controllers
{
    public class LeaderboardController : Controller
    {
        private const string leaderboardUrl =
            "http://api.sportsdatallc.org/golf-t1/leaderboard/pga/2013/tournaments/8d463c8b-e259-482d-8729-3c9efa877a22/leaderboard.xml?api_key=zyrf5exkkpj8r89qqrcdckgy";

        private const string scheduleUrl =
            "http://api.sportsdatallc.org/golf-t1/schedule/pga/2013/tournaments/schedule.xml?api_key=zyrf5exkkpj8r89qqrcdckgy";

        //private const string DATA = @"{""object"":{""name"":""Name""}}";
        private static string response;
        private static XmlDocument doc;
        private static XmlNodeList nodeList;
        private static XmlAttribute attribute;
        //private static List<XmlAttribute> attributeList;
        private static DataTable dtAttributeList;
        private static DataTable dtSchedule;

        private static JsonSerializerSettings JsonSetting
        {
            get
            {
                var settings = new JsonSerializerSettings();

                settings.ContractResolver = new DefaultContractResolver()
                {
                    IgnoreSerializableAttribute = true,
                    IgnoreSerializableInterface = true
                };
                return settings;
            }
        }


        //
        // GET: /Leaderboard/

        //public ActionResult Index()
        //{
        //    return View();
        //}
        
        public ActionResult PositionWithScores()
        {
            ViewBag.Tournaments = GetTournamentDetailsSelectList();
            return View();
        }

        [HttpPost]
        public ActionResult PositionWithScores(PlayerDetails player)
        {
            ViewBag.Tournaments = GetTournamentDetailsSelectList();
            var tournamentId = Request.Form["ddlTournaments"];
            ViewData["TournamentId"] = tournamentId;
            ViewData["TournamentName"] = Request.Form["TournamentName"];
            var url = ConstructLeaderboardUrl(tournamentId);
            var xmlToJsonUrl = ConvertXmltoJson(url);
            List<PlayerDetails> details = BindDataTablePlayerDetailsWithScores(xmlToJsonUrl);
            //ViewData["ddlTournaments"] = tournamentId;
            return View(details);
        }

        public ActionResult Position()
        {
            var year = "2013";
            ViewBag.Years = GetYears();
            ViewBag.Tournaments = GetTournamentDetailsSelectList();
            return View();
        }

        private List<SelectListItem> GetYears()
        {
            var list = new List<SelectListItem>();
            var item = new SelectListItem { Value = "", Text = "Select Year" };
            var item1 = new SelectListItem {Value = "2014", Text = "2014"};
            var item2 = new SelectListItem { Value = "2013", Text = "2013" };
            var item3 = new SelectListItem { Value = "2012", Text = "2012" };

            list.Add(item);
            list.Add(item1);
            list.Add(item2);
            list.Add(item3);

            return list;

        }
        
        [HttpPost]
        public ActionResult Position(PlayerDetails player)
        {
            var year = Request.Form["ddlYear"];

            // Populates Tournaments for the given year toipopulate Tournament dropdown
            ViewBag.Tournaments = GetTournamentDetailsSelectList_ByYear(year);
            ViewBag.Years = GetYears();

            var tournamentId = Request.Form["ddlTournaments"];
            ViewData["TournamentId"] = tournamentId;
            ViewData["Year"] = year;
            
            // Constructs url to be passed to retrieve proper data from API
            var url = ConstructLeaderboardUrl(tournamentId, year);

            // Retrieves data from API
            var xmlToJsonUrl = ConvertXmltoJson(url);

            // Binds data to View
            List<PlayerDetails> details = BindDataTablePlayerDetails(xmlToJsonUrl);

            return View(details);
        }

        private string ConstructLeaderboardUrl(string tournamentId)
        {
            string urlBase = "http://api.sportsdatallc.org/golf-t1/leaderboard/pga";
            string urlYear = "2013";
            string urlSuffix = string.Format("tournaments/{0}/leaderboard.xml?api_key=", tournamentId);
            string urlApiKey = "zyrf5exkkpj8r89qqrcdckgy";
            return string.Format("{0}/{1}/{2}{3}", urlBase, urlYear, urlSuffix, urlApiKey);
        }

        private string ConstructLeaderboardUrl(string tournamentId, string year)
        {
            string urlBase = "http://api.sportsdatallc.org/golf-t1/leaderboard/pga";
            string urlYear = year;
            string urlSuffix = string.Format("tournaments/{0}/leaderboard.xml?api_key=", tournamentId);
            string urlApiKey = "zyrf5exkkpj8r89qqrcdckgy";
            return string.Format("{0}/{1}/{2}{3}", urlBase, urlYear, urlSuffix, urlApiKey);
        }

        private static string ConvertXmltoJson(string url)
        {
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "GET";

            try
            {
                WebResponse webResponse = request.GetResponse();
                Thread.Sleep(500);
                Stream webStream = webResponse.GetResponseStream();
                Thread.Sleep(500);
                doc = new XmlDocument();
                doc.Load(webStream);
                string jsonText = JsonConvert.SerializeXmlNode(doc);
                return jsonText;
            }
            catch (Exception e)
            {
                throw new Exception();
            }
        }

        private static List<PlayerDetails> BindDataTablePlayerDetails(string url)
        {
            dtAttributeList = new DataTable();
            var details = new List<PlayerDetails>();

            try
            {
                //ConvertXmltoJson(url);

                // Construct Datatable
                dtAttributeList.Columns.Add("Last Name", typeof(string));
                dtAttributeList.Columns.Add("First Name", typeof(string));
                dtAttributeList.Columns.Add("Position", typeof(string));
                dtAttributeList.Columns.Add("Final Score", typeof(string));
                //dtAttributeList.Columns.Add("Money Earned", typeof(string));

                // Add rows to Datatable from Json
                for (int i = 0; i < doc.GetElementsByTagName("player").Count; i++)
                {
                    //string money = string.Empty;
                    //if (doc.GetElementsByTagName("player").Item(i).Attributes["money"].Value != null ||
                    //    doc.GetElementsByTagName("player").Item(i).Attributes["money"].Value != string.Empty)
                    //{
                    //    money = doc.GetElementsByTagName("player").Item(i).Attributes["money"].Value;
                    //}
                    //else
                    //{
                    //    money = "0";

                    //}

                    dtAttributeList.Rows.Add(
                        doc.GetElementsByTagName("player").Item(i).Attributes["last_name"].Value,
                        doc.GetElementsByTagName("player").Item(i).Attributes["first_name"].Value,
                        doc.GetElementsByTagName("player").Item(i).Attributes["position"].Value,
                        doc.GetElementsByTagName("player").Item(i).Attributes["score"].Value);
                }


                // Add rows from Datatable to PlayerDetails
                foreach (DataRow row in dtAttributeList.Rows)
                {
                    var player = new PlayerDetails();
                    player.LastName = row["Last Name"].ToString();
                    player.FirstName = row["First Name"].ToString();
                    player.Position = row["Position"].ToString();
                    player.TotalScore = row["Final Score"].ToString();
                    //player.MoneyEarned = decimal.Parse(row["Money Earned"].ToString(), NumberStyles.Currency);
                    //player.MoneyEarned = String.Format("{0:C0}",row["Money Earned"]);
                    details.Add(player);
                }
            }
            catch (Exception e)
            {
                throw new Exception();
            }

            return details;

        }

        public static List<PlayerDetails> BindDataTablePlayerDetailsWithScores(string url)
        {
            dtAttributeList = new DataTable();
            var details = new List<PlayerDetails>();

            try
            {

                //ConvertXmltoJson(leaderboardUrl);

                var playerTag = doc.GetElementsByTagName("player");

                // Construct Datatable
                dtAttributeList.Columns.Add("Last Name", typeof(string));
                dtAttributeList.Columns.Add("First Name", typeof(string));
                dtAttributeList.Columns.Add("Position", typeof(string));
                dtAttributeList.Columns.Add("Score", typeof(string));
                dtAttributeList.Columns.Add("R1", typeof(string));
                dtAttributeList.Columns.Add("R2", typeof(string));
                dtAttributeList.Columns.Add("R3", typeof(string));
                dtAttributeList.Columns.Add("R4", typeof(string));

                // Add rows to Datatable from Json

                foreach (XmlNode player in playerTag)
                {
                    string r1 = string.Empty;
                    string r2 = string.Empty;
                    string r3 = string.Empty;
                    string r4 = string.Empty;
                    string lastName = player.Attributes["last_name"].Value;
                    string firstName = player.Attributes["first_name"].Value;
                    string position = player.Attributes["position"].Value;
                    string totalScore = player.Attributes["score"].Value;

                    // Itterate thru all rounds for given player
                    if (player.HasChildNodes)
                    {
                        var roundNode = player.LastChild.FirstChild;
                        for (int i = 0; i < player.LastChild.ChildNodes.Count; i++)
                        {
                            if (i == 0)
                            {
                                r1 = roundNode.Attributes["strokes"].Value;
                            }

                            if (i == 1)
                            {
                                r2 = roundNode.Attributes["strokes"].Value;
                            }
                            if (i == 2)
                            {
                                r3 = roundNode.Attributes["strokes"].Value;
                            }
                            if (i == 3)
                            {
                                r4 = roundNode.Attributes["strokes"].Value;
                            }

                        }

                    }

                    // Add rows to DataTable
                    dtAttributeList.Rows.Add(
                        lastName,
                        firstName,
                        position,
                        totalScore,
                        r1, r2, r3, r4);
                }

                // Add rows from Datatable to PlayerDetails
                foreach (DataRow row in dtAttributeList.Rows)
                {
                    var player = new PlayerDetails();
                    player.LastName = row["Last Name"].ToString();
                    player.FirstName = row["First Name"].ToString();
                    player.Position = row["Position"].ToString();
                    player.TotalScore = row["Score"].ToString();
                    player.RoundOneScore = row["R1"].ToString();
                    player.RoundTwoScore = row["R2"].ToString();
                    player.RoundThreeScore = row["R3"].ToString();
                    player.RoundFourScore = row["R4"].ToString();
                    details.Add(player);
                }
            }
            catch (Exception e)
            {
                throw new Exception();
            }

            return details;

        }
        
        private static string ConstructTournamentUrl(string year)
        {
            string urlBase = "http://api.sportsdatallc.org/golf-t1/schedule/pga"; ;
            string urlYear = year;
            string urlSuffix = "tournaments/schedule.xml?api_key=";
            string urlApiKey = "zyrf5exkkpj8r89qqrcdckgy";
            return string.Format("{0}/{1}/{2}{3}", urlBase, urlYear, urlSuffix, urlApiKey);
        }

        private static List<SelectListItem> GetTournamentDetailsSelectList()
        {
            var vm = new PositionViewModel();
            vm.TournamentsSelectList = new List<SelectListItem>();
            
            // Only for 2013
            var url = ConstructTournamentUrl("2013");
            ConvertXmltoJson(url);

            var scheduleTag = doc.GetElementsByTagName("tournament");

            var firstItem = new SelectListItem() {Value = string.Empty, Text = "Select Tournament"};
            vm.TournamentsSelectList.Add(firstItem);
            
            // Add rows to Collection
            foreach (XmlNode tournament in scheduleTag)
            {
                string name = tournament.Attributes["name"].Value;
                string id = tournament.Attributes["id"].Value;

                var item = new SelectListItem()
                {
                    Value=id,
                    Text=name
                };

                vm.TournamentsSelectList.Add(item);
            }

            return vm.TournamentsSelectList;
        }

        private static List<SelectListItem> GetTournamentDetailsSelectList_ByYear(string year)
        {
            var vm = new PositionViewModel();
            vm.TournamentsSelectList = new List<SelectListItem>();

            string url = ConstructTournamentUrl(year);
            ConvertXmltoJson(url);

            var scheduleTag = doc.GetElementsByTagName("tournament");

            var firstItem = new SelectListItem() { Value = string.Empty, Text = "Select Tournament" };
            vm.TournamentsSelectList.Add(firstItem);

            // Add rows to Collection
            foreach (XmlNode tournament in scheduleTag)
            {
                string name = tournament.Attributes["name"].Value;
                string id = tournament.Attributes["id"].Value;

                var item = new SelectListItem()
                {
                    Value = id,
                    Text = name
                };

                vm.TournamentsSelectList.Add(item);
            }

            return vm.TournamentsSelectList;
        }

        //public ActionResult PositionWithDynamicScores()
        //{
        //    List<PlayerDetails> details = BindDataTablePlayerDetailsWithDynamicScores();
        //    return View(details);
        //}

        //public List<PlayerDetails> BindDataTablePlayerDetailsWithDynamicScores(string tournamentId)
        //{
        //    dtAttributeList = new DataTable();
        //    var details = new List<PlayerDetails>();

        //    var url = ConstructLeaderboardUrl(tournamentId);
        //    ConvertXmltoJson(url);

        //    return details;
        //}

        //public ActionResult PopulateTournamentDropDownList()
        //{
        //    var list = GetTournamentDetails();
        //    return Content(JsonConvert.SerializeObject(list, Newtonsoft.Json.Formatting.Indented, JsonSetting));
        //}

        //private static List<TournamentDetails> GetTournamentDetails()
        //{
        //    dtSchedule = new DataTable();
        //    var details = new List<TournamentDetails>();

        //    ConvertXmltoJson(scheduleUrl);

        //    var scheduleTag = doc.GetElementsByTagName("tournament");

        //    // Construct Datatable

        //    dtSchedule.Columns.Add("Start Date", typeof(string));
        //    dtSchedule.Columns.Add("Name", typeof(string));
        //    dtSchedule.Columns.Add("ID", typeof(string));
        //    //dtSchedule.Columns.Add("Yardage", typeof(string));
        //    //dtSchedule.Columns.Add("Par", typeof(string));


        //    // Add rows to Datatable from Json
        //    foreach (XmlNode tournament in scheduleTag)
        //    {
        //        string startDate = tournament.Attributes["start_date"].Value;
        //        string name = tournament.Attributes["name"].Value;
        //        string id = tournament.Attributes["id"].Value;
        //        //string yardage = tournament.Attributes["yardage"].Value;
        //        //string par = tournament.Attributes["par"].Value;


        //        // Add rows to DataTable
        //        dtSchedule.Rows.Add(
        //            startDate,
        //            name,
        //            id);
        //    }

        //    // Add rows from Datatable to PlayerDetails
        //    foreach (DataRow row in dtSchedule.Rows)
        //    {
        //        var tournament = new TournamentDetails();
        //        tournament.Date = row["Start Date"].ToString();
        //        tournament.Name = row["Name"].ToString();
        //        tournament.Id = row["ID"].ToString();
        //        details.Add(tournament);
        //    }
        //    return details;
        //}


        


    }
}
