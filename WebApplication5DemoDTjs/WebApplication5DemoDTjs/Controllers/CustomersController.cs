using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using WebApplication5DemoDTjs.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;


namespace WebApplication5DemoDTjs.Controllers
{
    public class CustomersController : Controller
    {
        private readonly CustomersDBContext db;

        public CustomersController(IConfiguration configuration)
        {
            db = new CustomersDBContext(configuration.GetConnectionString("test (webApplication5DemoDTjs)"));
        }

        // GET: Customers
        public ActionResult Index()
        {
            return View(db.Customerss.ToList());
        }

        // GET: Customers/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return BadRequest();
            }
            Customers customers = db.Customerss.Find(id);
            if (customers == null)
            {
                return NotFound();
            }
            return View(customers);
        }

        // GET: Customers/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Customers/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind("CustomerID,CompanyName,ContactName,ContactTitle,Address,City,Region,PostalCode,Country,Phone,Fax")] Customers customers)
        {
            if (ModelState.IsValid)
            {
                db.Customerss.Add(customers);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(customers);
        }

        // GET: Customers/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return BadRequest();
            }
            Customers customers = db.Customerss.Find(id);
            if (customers == null)
            {
                return NotFound();
            }
            return View(customers);
        }

        // POST: Customers/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind("CustomerID,CompanyName,ContactName,ContactTitle,Address,City,Region,PostalCode,Country,Phone,Fax")] Customers customers)
        {
            if (ModelState.IsValid)
            {
                db.Entry(customers).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(customers);
        }

        // GET: Customers/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return BadRequest();
            }
            Customers customers = db.Customerss.Find(id);
            if (customers == null)
            {
                return NotFound();
            }
            return View(customers);
        }

        // POST: Customers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Customers customers = db.Customerss.Find(id);
            db.Customerss.Remove(customers);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
