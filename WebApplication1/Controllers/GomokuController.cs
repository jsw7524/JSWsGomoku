using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebApplication1.Helper;

namespace WebApplication1.Controllers
{
    public class GomokuController : Controller
    {
        // GET: Gomoku
        public ActionResult Play()
        {
            Session.Clear();
            return View();
        }

        //public ActionResult ResetAll()
        //{
        //    Session.Clear();
        //    roundCount = 0;
        //    rule.myTable = null;
        //    return Json(new {});
        //}
        public ActionResult AI(int posY, int posX,int side=-1)
        {
            Rule rule = new Rule();
            AI myAI = new AI();
            int roundCount = 0;
            if (posY < 0 || posY > 14 || posX < 0 || posX > 14)
            {
                return new EmptyResult();
            }

            rule.myTable = (int[,])Session["myTable"] ?? new int[15, 15];
            roundCount = (int?)Session["roundCount"] ?? 0;
            myAI.DepthLimit = (int?)Session["DepthLimit"] ?? 6;
            myAI.timeSpanTotal = (TimeSpan?)Session["timeSpanTotal"] ?? new TimeSpan(0,0,0);
            myAI.roundCount = roundCount;
            //Player Part
            rule.myTable[posY, posX] = side;
            roundCount++;

            if (rule.Referee(posY, posX, side))
            {
                rule.ResetGame();
                Session["myTable"] = rule.myTable;
                Session["roundCount"] = 0;
                Session.Remove("myTable");
                Session.Remove("roundCount");
                Session.Clear();
                Session.Abandon();
                return Json(new { yAI = posY, xAI = posX, sideAI = (side == 1) ? -1 : 1, flag=99,message="Player wins." }, JsonRequestBehavior.AllowGet); // Player wins the game.
            }
            //

            //AI Part
            if (1 == roundCount)
            {
                int rx = 0;
                if ( posX + 1 <14)
                {
                    rule.myTable[posY, posX + 1] = (side == 1) ? -1 : 1;
                    rx = posX + 1;
                }
                else
                {
                    rule.myTable[posY, posX - 1] = (side == 1) ? -1 : 1;
                    rx = posX - 1;
                }

                Session["myTable"] = rule.myTable;
                roundCount++;
                Session["roundCount"] = roundCount;
                return Json(new { yAI = posY, xAI = rx, sideAI = (side == 1) ? -1 : 1, flag = 0, message = "" }, JsonRequestBehavior.AllowGet); 
            }
            myAI.LoadWeightTable();
            myAI.MyTable = rule.myTable;
            Tuple<int, int> moveAI = myAI.NextMove((side == 1) ? -1 : 1);
            rule.myTable[moveAI.Item1, moveAI.Item2] = (side == 1) ? -1 : 1;
            roundCount++;
            if (rule.Referee(moveAI.Item1, moveAI.Item2, (side == 1) ? -1 : 1))
            {
                rule.ResetGame();
                Session["myTable"] = rule.myTable;
                Session["roundCount"] = 0;
                Session.Remove("myTable");
                Session.Remove("roundCount");
                Session.Clear();
                Session.Abandon();
                return Json(new { yAI = moveAI.Item1, xAI = moveAI.Item2, sideAI = (side == 1) ? -1 : 1, flag = -99, message = "Computer wins." }, JsonRequestBehavior.AllowGet); // AI wins the game.
            }
            //
            Session["roundCount"] = roundCount;
            Session["myTable"] = rule.myTable;
            Session["DepthLimit"] = myAI.DepthLimit;
            Session["timeSpanTotal"] = myAI.timeSpanTotal;
            return Json(new { yAI = moveAI.Item1, xAI = moveAI.Item2, sideAI = (side == 1) ? -1 : 1, flag = 0, message = "" }, JsonRequestBehavior.AllowGet);
        }
    }
}