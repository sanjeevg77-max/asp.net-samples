using System;
using System.Linq;
using WebApplication5DemoDTjs.Models;
using System.Data.Entity;
using Microsoft.AspNetCore.Mvc;
using System.Linq.Dynamic.Core;


namespace WebApplication5DemoDTjs.Controllers
{
    public class DemoController : Controller
    {
        // GET: Demo
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult LoadData()
        {
            try
            {
                //Creating instance of DatabaseContext class  
                using (CustomersDBContext _context = new CustomersDBContext("Name=test (webApplication5DemoDTjs)"))
                {
                    var draw = Request.Form["draw"].FirstOrDefault();
                    var start = Request.Form["start"].FirstOrDefault();
                    var length = Request.Form["length"].FirstOrDefault();
                    var orderColIndex = Request.Form["order[0][column]"].FirstOrDefault();
                    var sortColumn = Request.Form["columns[" + orderColIndex + "][name]"].FirstOrDefault();
                    var sortColumnDir = Request.Form["order[0][dir]"].FirstOrDefault();
                    var searchValue = Request.Form["search[value]"].FirstOrDefault();


                    //Paging Size (10,20,50,100)    
                    int pageSize = length != null ? Convert.ToInt32(length) : 0;
                    int skip = start != null ? Convert.ToInt32(start) : 0;
                    int recordsTotal = 0;

                    // Getting all Customer data    
                    var customerData = (from tempcustomer in _context.Customerss
                                        select tempcustomer);

                    //Sorting    
                    if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
                    {
                        customerData = customerData.OrderBy(sortColumn + " " + sortColumnDir);
                    }
                    //Search    
                    if (!string.IsNullOrEmpty(searchValue))
                    {
                        customerData = customerData.Where(m => m.CompanyName == searchValue);
                    }

                    //total number of rows count     
                    recordsTotal = customerData.Count();
                    //Paging     
                    var data = customerData.Skip(skip).Take(pageSize).ToList();
                    //Returning Json Data    
                    return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });
                }
            }
            catch (Exception)
            {
                throw;
            }

        }

        [HttpGet]
        public ActionResult Edit(int? ID)
        {
            try
            {
                using (CustomersDBContext _context = new CustomersDBContext("Name=test (webApplication5DemoDTjs)"))
                {
                    var Customer = (from customer in _context.Customerss
                                    where customer.CustomerID == ID
                                    select customer).FirstOrDefault();

                    return View(Customer);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        [HttpPost]
        public JsonResult DeleteCustomer(int? ID)
        {
            using (CustomersDBContext _context = new CustomersDBContext("Name=test (webApplication5DemoDTjs)"))
            {
                var customer = _context.Customerss.Find(ID);
                if (ID == null)
                    return Json("Not Deleted");
                _context.Customerss.Remove(customer);
                _context.SaveChanges();

                return Json("Deleted");
            }
        }
    }
}