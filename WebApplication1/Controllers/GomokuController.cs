﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using WebApplication1.Helper;

namespace WebApplication1.Controllers
{
    public partial class GomokuController : Controller
    {
        private const int PlayerWinsFlag = 99;
        private const int ComputerWinsFlag = -99;

        public ActionResult Play()
        {
            StartGame();

            return View();
        }

        public async Task<ActionResult> AI(int posY, int posX, int side = -1)
        {
            var game = RestoreGame();
            var palyerSide = side;
            var aiSide = game.GetSide(side);

            if (posY < 0 || posY > 14 || posX < 0 || posX > 14)
            {
                return new EmptyResult();
            }

            //Player Part
            game.Move(posY, posX, palyerSide);

            if (game.PlayerWins(posY, posX, palyerSide))
            {
                game.Rule1.ResetGame();

                var playerWinsResult = new GameResult(posY, posX, aiSide, PlayerWinsFlag, "Player wins.");
                return Json(playerWinsResult, JsonRequestBehavior.AllowGet);
            }

            //AI Part
            if (game.IsNewGame)
            {
                game.Move(posY, posX + 1, aiSide);

                var gameResult = new GameResult(posY, posX + 1, aiSide, 0, "");
                return Json(gameResult, JsonRequestBehavior.AllowGet);
            }

            var nextMove = await game.AiMove(aiSide);

            if (game.ComputerWins(nextMove, aiSide))
            {
                game.Rule1.ResetGame();

                var computerWinsResult = new GameResult(nextMove.Item1, nextMove.Item2, palyerSide, ComputerWinsFlag, "Computer wins.");
                return Json(computerWinsResult, JsonRequestBehavior.AllowGet); // AI wins the game.
            }

            var result = new GameResult(nextMove.Item1, nextMove.Item2, palyerSide, 0, "");
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public Game StartGame()
        {
            var gameId = Request.Cookies["Id"]?.Value;

            var game = Game.Start(gameId);

            Response.Cookies.Set(new HttpCookie("Id", game.Id.ToString()));

            return game;
        }

        public Game RestoreGame()
        {
            var gameId = Request.Cookies["Id"]?.Value;

            var game = Game.Restore(gameId);

            Response.Cookies.Set(new HttpCookie("Id", game.Id.ToString()));

            return game;
        }

    }
}