﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ChenkaCoffeeShop.Models;
using System.Windows.Forms;
using ChenkaCoffeeShop.Controllers;
namespace ChenkaCoffeeShop.Controllers
{
    public class HomeController : Controller
    {
        CoffeeShopEntities db = new CoffeeShopEntities();

        List<Cart> li = new List<Cart>();

        #region home page in showing all products 

        public ActionResult Index()
        {
            return View();
        }
        public ActionResult product()
        {

            if (TempData["cart"] != null)
            {
                int x = 0;

                List<Cart> li2 = TempData["cart"] as List<Cart>;
                foreach (var item in li2)
                {
                    x += item.bill;

                }
                TempData["total"] = x;
                TempData["item_count"] = li2.Count();
            }
            TempData.Keep();

            var query = db.tblProducts.ToList();
            return View(query);
        }
        public ActionResult About()
        {

            return View();
        }
        #endregion

        #region add to cart

        public ActionResult AddtoCart(int id)
        {
            var query =  db.tblProducts.Where(x => x.ProID == id).SingleOrDefault();
            return View(query);
        }

        [HttpPost]
        public ActionResult AddtoCart(int id,int qty)
        {
           tblProducts p = db.tblProducts.Where(x => x.ProID == id).SingleOrDefault();
           Cart c = new Cart();
           c.proid = id;
           c.proname = p.Name;
           c.price = Convert.ToInt32(p.Unit);
           c.qty = Convert.ToInt32(qty);
           c.bill = c.price * c.qty;
           if (TempData["cart"] == null)
           {
               li.Add(c);
               TempData["cart"] = li;
           }
           else
           {
               List<Cart> li2 = TempData["cart"] as List<Cart>;
               int flag = 0;
               foreach (var item in li2)
               {
                   if (item.proid == c.proid)
                   {
                       item.qty += c.qty;
                       item.bill += c.bill;
                       flag = 1;
                   }

               }
               if (flag == 0)
               {
                   li2.Add(c);
               }
               TempData["cart"] = li2;
           }
           TempData["customerSits"] = db.tblSit.Where(m => m.available == 1);
           TempData.Keep();
           return RedirectToAction("Product");
        }

        #endregion

        #region remove cart item

        public ActionResult remove(int? id)
        {
            if (TempData["cart"] == null)
            {
                TempData.Remove("total");
                TempData.Remove("cart");
            }
            else
            {
                List<Cart> li2 = TempData["cart"] as List<Cart>;
                Cart c = li2.Where(x => x.proid == id).SingleOrDefault();
                li2.Remove(c);
                int s = 0;
                foreach (var item in li2)
                {
                    s += item.bill;
                }
                TempData["total"] = s;

            }

            return RedirectToAction("Index");
        }
        #endregion

        #region checkout code

        public ActionResult Checkout()
        {
            TempData.Keep();
            return View();
        }

        [HttpPost]
        public ActionResult Checkout(string customerName)
        {
            if (ModelState.IsValid)
            {
                List<Cart> li2 = TempData["cart"] as List<Cart>;
                tblInvoice iv = new tblInvoice();
                iv.UserId = Convert.ToInt32(Session["uid"]);
                iv.InvoiceDate = System.DateTime.Now;
                iv.Bill = (int)TempData["total"];
                iv.Payment = "cash";
                db.tblInvoice.Add(iv);
                db.SaveChanges();
                foreach (var item in li2)
                {
                    tblOrder od = new tblOrder();
                    od.ProID = item.proid;
                    od.Contact = customerName;
                    od.OrderDate = System.DateTime.Now;
                    od.InvoiceId = iv.InvoiceId;
                    od.Qty = item.qty;
                    od.Unit = item.price;
                    od.Total = item.bill;

                    db.tblOrder.Add(od);
                    db.SaveChanges();

                }
                TempData.Remove("total");
                TempData.Remove("cart");
                return RedirectToAction("Index");
            }
            TempData.Keep();
            return View();
        }

        #endregion


        #region all orders for admin 

        public ActionResult GetAllOrderDetail()
        {
            var query = db.Orders.ToList();
            return View(query);
        }

        #endregion

        #region  confirm order by admin

        public ActionResult ConfirmOrder(int InvoiceId,int userId)
        {
            var query = db.Orders.SingleOrDefault(m=>m.InvoiceId == InvoiceId);
            TempData["userID"] = userId;
            TempData.Keep();
            return View(query);
        }

        public void ReleaseSit2(int UserId)
        {
            var query = db.tblSit.SingleOrDefault(m => m.userId == UserId);
            if (query.userId == UserId)
            {
                query.available = 1;
                db.SaveChanges();
            }
            else
            {
                TempData["msg"] = "Didn't release sit";
            }
        }

        [HttpPost]
        public ActionResult ConfirmOrder(Orders o)
         {
            tblInvoice inv = new tblInvoice()
            {
                InvoiceId = o.InvoiceId,
                UserId = (int)TempData["userID"],
                Bill = o.Bill,
                Payment = o.Payment,
                InvoiceDate = DateTime.Now,
                Status = 1,
            };
            db.Entry(inv).State = (System.Data.Entity.EntityState)EntityState.Modified;
            db.SaveChanges();
            DialogResult res = MessageBox.Show("The Payment Was Succesful", "Payment Succesful", MessageBoxButtons.OK, MessageBoxIcon.Information);
            if (res == DialogResult.OK)
            {
                MessageBox.Show("Thank You!");
            }
            ReleaseSit2((int)TempData["userID"]);
            return RedirectToAction("GetAllOrderDetail");
        }
        #endregion

        #region orders for only user
        public ActionResult OrderDetail(int id)
        {
            var query = db.OrderUsers.Where(m => m.UserId == id).ToList();
            return View(query);
        }

         #endregion


        #region  get all users 

        public ActionResult GetAllUser()
        {
            var query = db.tblUser.ToList();
            return View(query);
        }

        #endregion



        #region invoice for  user

        public ActionResult Invoice(int id)
        {
            var query = db.UserInvoices.Where(m => m.InvoiceId == id).ToList();
            return View(query);
        }

        #endregion

    }
}