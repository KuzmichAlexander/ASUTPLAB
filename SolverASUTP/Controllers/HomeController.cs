using SolverASUTP.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.WebControls;
using Microsoft.SolverFoundation.Services;
using Parameter = Microsoft.SolverFoundation.Services.Parameter;

namespace SolverASUTP.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Solve(InputModel input)
        {
            SolverContext context = SolverContext.GetContext();
            Model model = context.CreateModel();
            Set users = new Set(Domain.IntegerNonnegative, "users");

            List<SolverRow> solverList = new List<SolverRow>();
            int othod1 = input.L - (input.T1S * input.T1zag1 + input.T2S * input.T2zag1 + input.T3S * input.T3zag1);
            int othod2 = input.L - (input.T1S * input.T1zag2 + input.T2S * input.T2zag2 + input.T3S * input.T3zag2);
            int othod3 = input.L - (input.T1S * input.T1zag3 + input.T2S * input.T2zag3 + input.T3S * input.T3zag3);
            int othod4 = input.L - (input.T1S * input.T1zag4 + input.T2S * input.T2zag4 + input.T3S * input.T3zag4);
            int othod5 = input.L - (input.T1S * input.T1zag5 + input.T2S * input.T2zag5 + input.T3S * input.T3zag5);
            if (othod1 < 0 || othod2 < 0 || othod3 < 0 || othod4 < 0 || othod5 < 0)
            {
                ViewBag.error = "Значение отхода получилос отрицательным";
                return View("Error");
            }

            solverList.Add(new SolverRow { xId = 1, Othod = othod1, Itogo = 0, T1Zagotovok = input.T1zag1, T2Zagotovok = input.T2zag1, T3Zagotovok = input.T3zag1 });
            solverList.Add(new SolverRow { xId = 2, Othod = othod2, Itogo = 0, T1Zagotovok = input.T1zag2, T2Zagotovok = input.T2zag2, T3Zagotovok = input.T3zag2 });
            solverList.Add(new SolverRow { xId = 3, Othod = othod3, Itogo = 0, T1Zagotovok = input.T1zag3, T2Zagotovok = input.T2zag3, T3Zagotovok = input.T3zag3 });
            solverList.Add(new SolverRow { xId = 4, Othod = othod4, Itogo = 0, T1Zagotovok = input.T1zag4, T2Zagotovok = input.T2zag4, T3Zagotovok = input.T3zag4 });
            solverList.Add(new SolverRow { xId = 5, Othod = othod5, Itogo = 0, T1Zagotovok = input.T1zag5, T2Zagotovok = input.T2zag5, T3Zagotovok = input.T3zag5 });

            Parameter ost = new Parameter(Domain.IntegerNonnegative, "Othod", users);
            ost.SetBinding(solverList, "Othod", "xId");

            Parameter T1Zagotovok = new Parameter(Domain.IntegerNonnegative, "T1Zagotovok", users);
            T1Zagotovok.SetBinding(solverList, "T1Zagotovok", "xId");
            Parameter T2Zagotovok = new Parameter(Domain.IntegerNonnegative, "T2Zagotovok", users);
            T2Zagotovok.SetBinding(solverList, "T2Zagotovok", "xId");
            Parameter T3Zagotovok = new Parameter(Domain.IntegerNonnegative, "T3Zagotovok", users);
            T3Zagotovok.SetBinding(solverList, "T3Zagotovok", "xId");

            model.AddParameters(ost, T1Zagotovok, T2Zagotovok, T3Zagotovok);

            Decision choose = new Decision(Domain.IntegerNonnegative, "choose", users);
            model.AddDecision(choose);

            model.AddGoal("goal", GoalKind.Minimize, Model.Sum(Model.ForEach(users, xId => choose[xId] * ost[xId])));

            int needT1 = input.N * input.T1Kol;
            int needT2 = input.N * input.T2Kol;
            int needT3 = input.N * input.T3Kol;

            model.AddConstraint("lessOrEql1", Model.Sum(Model.ForEach(users, xId => choose[xId] * T1Zagotovok[xId])) == needT1);
            model.AddConstraint("lessOrEql2", Model.Sum(Model.ForEach(users, xId => choose[xId] * T2Zagotovok[xId])) == needT2);
            model.AddConstraint("lessOrEql3", Model.Sum(Model.ForEach(users, xId => choose[xId] * T3Zagotovok[xId])) == needT3);

            try
            {
                Solution solution = context.Solve();

                String reportStr = "";

                for (int i = 0; i < solverList.Count; i++)
                {
                    ViewData["T" + solverList[i].xId] = "Значение X" + (i + 1).ToString() + ": " + choose.GetDouble(solverList[i].xId) + "\n";
                    reportStr += "Значение X" + (i + 1).ToString() + ": " + choose.GetDouble(solverList[i].xId) + "\n";
                }

                ViewBag.solved = solution.Goals.First();
            }
            catch (Exception ex)
            {
                ViewBag.error = "Не получилось рассчитать целевую функцию";
                return View("Error");
            }

            return View("Solved");
        }
    }
}