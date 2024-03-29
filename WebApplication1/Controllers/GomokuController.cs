﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebApplication1.Helper;

namespace WebApplication1.Controllers
{
    public class GomokuController : Controller
    {
        // GET: Gomoku
        Random rand = new Random();
        Rule rule = new Rule();

        public GomokuController() :base()
        {
            Debug.WriteLine("GomokuController");
        }
        
        private int roundCount = 0;
        public ActionResult Play()
        {
            return View();
        }

        public ActionResult SetDifficulty(int difficulty)
        {
            //MvcApplication.myAI.SetDepthLimit(difficulty);
            rule.myTable = new int[15, 15];
            roundCount = 0;
            Session["difficulty"] = difficulty;
            TempData["roundCount"] = roundCount;
            TempData["myTable"] = rule.myTable;
            return View("Play");
        }

        public ActionResult AIGoesFirst( int side = -1)
        {
            rule.myTable =  new int[15, 15];
            roundCount = 1;


            rule.myTable[7,7] = (side == 1) ? -1 : 1; ;
            

            roundCount++;
            TempData["roundCount"] = roundCount;
            TempData["myTable"] = rule.myTable;
            return Json(new { yAI = 7, xAI =7, sideAI = (side == 1) ? -1 : 1, flag = 0, message = "OK, AI Goes First!" }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult AI(int posY, int posX,int side=-1)
        {
            if (posY < 0 || posY > 14 || posX < 0 || posX > 14)
            {
                return new EmptyResult();
            }

            rule.myTable = (int[,])TempData["myTable"] ?? new int[15, 15];
            roundCount = (int?) TempData["roundCount"] ?? 0;

            //if (roundCount == 0)
            //{
            //    rule.myTable[7,7] = 1;
            //}

            //Player Part
            rule.myTable[posY, posX] = side;
            roundCount++;

            if (rule.Referee(posY, posX, side))
            {
                rule.ResetGame();
                TempData["myTable"] = rule.myTable;
                TempData["roundCount"] = 0;
                return Json(new { yAI = posY, xAI = posX, sideAI = (side == 1) ? -1 : 1, flag=99,message="Player wins." }, JsonRequestBehavior.AllowGet); // Player wins the game.
            }
            //

            //AI Part
            AI myAI = null;
            if (1 == roundCount)
            {
                rule.myTable[posY, posX + 1] = (side == 1) ? -1 : 1;
                TempData["myTable"] = rule.myTable;
                roundCount++;
                TempData["roundCount"] = roundCount;
                myAI = new AI();
                myAI.LoadWeightTable();
                Session["AI"] = myAI;
                return Json(new { yAI = posY, xAI = posX + 1, sideAI = (side == 1) ? -1 : 1, flag = 0, message = "" }, JsonRequestBehavior.AllowGet);
            }

            if (null == Session["AI"])
            {
                myAI = new AI();
                myAI.LoadWeightTable();
                Session["AI"] = myAI;
                
            }
            else
            {
                myAI =(AI)Session["AI"];
            }

            myAI.MyTable = rule.myTable;
            myAI.SetDepthLimit((int)(Session["difficulty"]??6));
            Tuple<int, int> moveAI = myAI.NextMove((side == 1) ? -1 : 1);
            rule.myTable[moveAI.Item1, moveAI.Item2] = (side == 1) ? -1 : 1;
            roundCount++;
            if (rule.Referee(moveAI.Item1, moveAI.Item2, (side == 1) ? -1 : 1))
            {
                rule.ResetGame();
                TempData["myTable"] = rule.myTable;
                TempData["roundCount"] = 0;
                return Json(new { yAI = moveAI.Item1, xAI = moveAI.Item2, sideAI = (side == 1) ? -1 : 1, flag = -99, message = "Computer wins." }, JsonRequestBehavior.AllowGet); // AI wins the game.
            }
            //
            TempData["roundCount"] = roundCount;
            TempData["myTable"] = rule.myTable;
            return Json(new { yAI = moveAI.Item1, xAI = moveAI.Item2, sideAI = (side == 1) ? -1 : 1, flag = 0, message = "" }, JsonRequestBehavior.AllowGet);
        }
    }
}