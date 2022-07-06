using ChenkaCoffeeShop.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ChenkaCoffeeShop.Controllers
{
    public class SitController : Controller
    {
        CoffeeShopEntities db = new CoffeeShopEntities();

        public ActionResult Index()
        {
            var query = db.TableSits.ToList();
            return View(query);
        }

        #region Creating sits
        public ActionResult Create()
        {
            List<tblTable> list = db.tblTable.ToList();
            ViewBag.TableList = new SelectList(list, "tableId","tableId");
            return View();
        }

        [HttpPost]
        public ActionResult Create(tblTable tbl)
        {
            List<tblTable> list = db.tblTable.ToList();
            ViewBag.TableList = new SelectList(list, "tableId", "tableId");
            if (ModelState.IsValid)
            {
                tblSit s = new tblSit();
                s.available = 1;
                s.tableId = tbl.tableId;
                db.tblSit.Add(s);
                var query = db.tblTable.SingleOrDefault(m => m.tableId == s.tableId);
                query.numSeats += 1;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            else
            {
                TempData["msg"] = "Didn't create table";
            }
            return View();
        }
        #endregion

        public ActionResult SaveSit(int SitId, int userId)
        {
            var query = db.tblSit.SingleOrDefault(m => m.sitId == SitId);
            if (query.available == 1)
            {
                query.userId = userId;
                query.available = 0;
                db.SaveChanges();
            }
            else
            {
                TempData["msg"] = "Didn't save sit";
            }
            return RedirectToAction("Checkout", "Home");
        }

        public ActionResult ReleaseSit(int SitId)
        {
            var query = db.tblSit.SingleOrDefault(m => m.sitId == SitId);
            if (query.available == 0)
            {
                query.available = 1;
                query.userId = null;
                db.SaveChanges();
            }
            else
            {
                TempData["msg"] = "Didn't release sit";
            }
            return RedirectToAction("Index", "Table");

        }

        #region Remove Sit
        public ActionResult Deleted(int id)
        {
            var query = db.tblSit.SingleOrDefault(m => m.sitId == id);
            db.tblSit.Remove(query);
            db.SaveChanges();
            return RedirectToAction("Index");
        }
        #endregion
    }
}