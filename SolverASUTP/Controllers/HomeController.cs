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
            solverList.Add(new SolverRow { xId = 1, Othod = 19, Itogo = 0, T1Zagotovok = 2, T2Zagotovok = 0, T3Zagotovok = 0 });
            solverList.Add(new SolverRow { xId = 2, Othod = 10, Itogo = 0, T1Zagotovok = 1, T2Zagotovok = 1, T3Zagotovok = 0 });
            solverList.Add(new SolverRow { xId = 3, Othod = 1, Itogo = 0, T1Zagotovok = 1, T2Zagotovok = 0, T3Zagotovok = 1 });
            solverList.Add(new SolverRow { xId = 4, Othod = 1, Itogo = 0, T1Zagotovok = 0, T2Zagotovok = 2, T3Zagotovok = 0 });
            solverList.Add(new SolverRow { xId = 5, Othod = 53, Itogo = 0, T1Zagotovok = 0, T2Zagotovok = 0, T3Zagotovok = 1 });

            Parameter ost = new Parameter(Domain.IntegerNonnegative, "Othod", users);
            ost.SetBinding(solverList, "Othod", "xId");

            Parameter T1Zagotovok = new Parameter(Domain.IntegerNonnegative, "T1Zagotovok", users);
            T1Zagotovok.SetBinding(solverList, "T1Zagotovok", "xId");
            Parameter T2Zagotovok = new Parameter(Domain.IntegerNonnegative, "T2Zagotovok", users);
            T2Zagotovok.SetBinding(solverList, "T2Zagotovok", "xId");
            Parameter T3Zagotovok = new Parameter(Domain.IntegerNonnegative, "T3Zagotovok", users);
            T3Zagotovok.SetBinding(solverList, "T3Zagotovok", "xId");

            //Parameter x2 = new Parameter(Domain.RealNonnegative, "Koef", users);
            //x2.SetBinding(solverList, "Ostatok", "xId");
            //Parameter x3 = new Parameter(Domain.RealNonnegative, "Koef", users);
            //x3.SetBinding(solverList, "Ostatok", "xId");
            //Parameter x4 = new Parameter(Domain.RealNonnegative, "Koef", users);
            //x4.SetBinding(solverList, "Ostatok", "xId");
            //Parameter x5 = new Parameter(Domain.RealNonnegative, "Koef", users);
            //x5.SetBinding(solverList, "Ostatok", "xId");

            //model.AddParameters(x1, x2, x3, x4, x5);
            model.AddParameters(ost, T1Zagotovok, T2Zagotovok, T3Zagotovok);

            Decision choose = new Decision(Domain.IntegerNonnegative, "choose", users);
            model.AddDecision(choose);

            model.AddGoal("goal", GoalKind.Minimize, Model.Sum(Model.ForEach(users, xId => choose[xId] * ost[xId])));

            //ограничения
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
                ViewBag.solved = "Ошибка";
            }

            return View("Solved");
        }
    }
}