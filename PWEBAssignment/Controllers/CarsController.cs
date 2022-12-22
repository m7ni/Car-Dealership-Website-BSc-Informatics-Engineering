﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PWEBAssignment.Data;
using PWEBAssignment.Models;
using PWEBAssignment.ViewModels;
//using Microsoft.AspNetCore.Identity;

namespace PWEBAssignment.Controllers
{
    public class CarsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CarsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Cars
        public async Task<IActionResult> Index()
        {
            var applicationDbContext =  _context.Car.Include(c => c.Company).Include(d=>d.Category);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Cars/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Car == null)
            {
                return NotFound();
            }

            var car = await _context.Car
                .Include(c => c.Company).Include(
                    d=>d.Category)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (car == null)
            {
                return NotFound();
            }

            return View(car);
        }

        // GET: Cars/Create
        public IActionResult Create()
        {
            ViewData["CompanyID"] = new SelectList(_context.Company, "Id", "Name");
            ViewData["Category"] = new SelectList(_context.Category, "Id", "Name");
            return View();
        }

        // POST: Cars/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Model,LicencePlate,Damage,Available,CategoryID,CompanyID")] Car car)
        {
            ModelState.Remove(nameof(car.Category));
            ModelState.Remove(nameof(car.Company));
            if (ModelState.IsValid)
            {
                _context.Add(car);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return RedirectToAction(nameof(Create));
        }

        // GET: Cars/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Car == null)
            {
                return NotFound();
            }

            var car = await _context.Car.FindAsync(id);
            if (car == null)
            {
                return NotFound();
            }
            ViewData["Category"] = new SelectList(_context.Category, "Id", "Name");
            ViewData["CompanyID"] = new SelectList(_context.Company, "Id", "Name", car.CompanyID);
            return View(car);
        }

        // POST: Cars/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        //[Authorize(Roles = "Employe")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Model,LicencePlate,Damage,Available,CategoryID,CompanyID")] Car car)
        {
            if (id != car.Id)
            {
                return NotFound();
            }

            ModelState.Remove(nameof(car.Category));
            ModelState.Remove(nameof(car.Company));
            if (ModelState.IsValid)
            {
                try
                {
                    car.Company = await _context.Company.FindAsync(car.CompanyID);
                    car.Category = await _context.Category.FindAsync(car.CategoryID);

                    _context.Update(car);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CarExists(car.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["CompanyID"] = new SelectList(_context.Company, "Id", "Id", car.CompanyID);
            return View(car);
        }

        // GET: Cars/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Car == null)
            {
                return NotFound();
            }

            var car = await _context.Car
                .Include(c => c.Company)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (car == null)
            {
                return NotFound();
            }

            return View(car);
        }

        // POST: Cars/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        //[Authorize(Roles = "Employe")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Car == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Car'  is null.");
            }
            var car = await _context.Car.FindAsync(id);
            if (car != null)
            {
                _context.Car.Remove(car);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CarExists(int id)
        {
          return (_context.Car?.Any(e => e.Id == id)).GetValueOrDefault();
        }

        public async Task<IActionResult> Buy(int? id)
        {
            if (id == null || _context.Car == null)
            {
                return NotFound();
            }

            var curso = await _context.Car.Include("Category")
                .FirstOrDefaultAsync(m => m.Id == id);
            if (curso == null)
            {
                return NotFound();
            }

            //var carrinhoDeCompras = Carrinho();
            //carrinhoDeCompras.AddItem(curso);
            //HttpContext.Session.SetJson("CarrinhoDeCompras", carrinhoDeCompras);

            //return RedirectToAction(nameof(Carrinho));
            return RedirectToAction(nameof(Index));
        }
        
        //GET
        public async Task<IActionResult> Search( string? textToSearch)
        {
            ViewData["Title"] = "Car´s models´ list with location '" + textToSearch + "'";

            var searchVM = new CarSearchViewModel();
            searchVM.textToSearch = textToSearch;

            if (string.IsNullOrEmpty(textToSearch))
            {
                searchVM.ListOfCars = await _context.Car.ToListAsync();
            } else
            {
                searchVM.ListOfCars = await _context.Car.Include("Address").
                    Where(c => c.Company.Address.Contains(searchVM.textToSearch)).ToListAsync();
            }

            searchVM.NumResults = searchVM.ListOfCars.Count;

            return View(searchVM);
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Search([Bind("textToSearch")] CarSearchViewModel carSearch)
        {
            ViewData["Title"] = "Lista de Cursos com '" + carSearch.textToSearch + "'";

            if (string.IsNullOrWhiteSpace(carSearch.textToSearch))
            {
                carSearch.ListOfCars = await _context.Car.Include("Address").ToListAsync();
            }
            else
            {
                carSearch.ListOfCars = await _context.Car.Include("Address").
                Where(c => c.Company.Address.Contains(carSearch.textToSearch)).ToListAsync();
                carSearch.NumResults = carSearch.ListOfCars.Count;
            }

            return View(carSearch);
        }

        /*
        public async Task<IActionResult> Sort(string sortOrder)
        {
            ViewData["NameSortParm"] = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewData["DateSortParm"] = sortOrder == "Date" ? "date_desc" : "Date";
            ViewData["CurrentFilter"] = searchString;

            var students = from s in _context.Students
                           select s;
            if (!String.IsNullOrEmpty(searchString))
            {
                students = students.Where(s => s.LastName.Contains(searchString)
                                       || s.FirstMidName.Contains(searchString));
            }
            switch (sortOrder)
            {
                case "name_desc":
                    students = students.OrderByDescending(s => s.LastName);
                    break;
                case "Date":
                    students = students.OrderBy(s => s.EnrollmentDate);
                    break;
                case "date_desc":
                    students = students.OrderByDescending(s => s.EnrollmentDate);
                    break;
                default:
                    students = students.OrderBy(s => s.LastName);
                    break;
            }
            return View(await students.AsNoTracking().ToListAsync());
        }*/

    }
}