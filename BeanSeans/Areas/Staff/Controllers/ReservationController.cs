using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BeanSeans.Data;
using Microsoft.AspNetCore.Identity;
using BeanSeans.Areas.Staff.Models.Reservation;
using BeanSeans.Areas.Staff.Models.Person;

namespace BeanSeans.Areas.Administration
{
    [Area("Staff")]
    
    public class ReservationController : AdministrationAreaController
    {

        public ReservationController(SignInManager<IdentityUser> sim, UserManager<IdentityUser> um, ApplicationDbContext _db) : base(sim, um, _db)
        {

        }

        //we have to have sittings to make reserv
        //when we add reservation, first we add siting
        //model: Sitting, List Tep
        public async Task<IActionResult> Sittings()
        {

            var sittings = await _db.Sittings
                                   .Include(s => s.SittingType)
                                   .ToListAsync();
            return View(sittings);
        }

        [HttpGet]
        public async Task<IActionResult> CreateMemberReservation(int id)
        {

            var sitting = await _db.Sittings
                                   .Include(s => s.SittingType)
                                   .FirstOrDefaultAsync(s  =>  s.Id == id); 
            if(sitting == null)
            {
                return NotFound(); 
            }

            var members = _db.Members.Select(me => new {
                Id = me.Id,
                Name = $"{me.LastName}, {me.FirstName}"
            }).ToList(); 

            var m = new CreateMemberReservation
            {
                SittingId = sitting.Id,
                Sitting = $"{sitting.SittingType.Name} {sitting.Start}",
                StatusId = 1, //initial status is pending with id of 1
                StatusOptions = new SelectList(_db.ReservationStatuses.ToList(),"Id", "Name"),
                SourceId = 1, //set the id of source to 1 i.e. online
                SourceOptions = new SelectList(_db.ReservationSources.ToList(),"Id","Name"),
                MemberOptions = new SelectList(members, "Id","Name")
            }; 

            return View(m);
        }

        [HttpPost]
        public async Task<IActionResult> CreateMemberReservation(CreateMemberReservation m)
        {

            
            return View();
        }





        // GET: Administration/Reservation
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _db.Reservations.Include(r => r.Person)
                                                       .Include(r => r.Sitting)
                                                        .Include(r => r.Source)
                                                        .Include(r => r.Status);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Administration/Reservation/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var reservation = await _db.Reservations
                .Include(r => r.Person)
                .Include(r => r.Sitting)
                .Include(r => r.Source)
                .Include(r => r.Status)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (reservation == null)
            {
                return NotFound();
            }

            return View(reservation);
        }

        //[HttpGet]
        //// GET: Administration/Reservation/Create
        //public async Task<IActionResult> Create()
        //{
        //    var people = await _db.People
        //                       //   .Where(p=>p.IsStaff != true) //only for member and guest
        //                       .Select(p => new
        //                       {
        //                           IdOfPerson = p.Id,
        //                           Description = p.FirstName + ' ' + p.LastName
        //                       })
        //                       .ToListAsync();
            
            
        //    //pull the data first!
        //    ViewBag.PersonId = new SelectList(people, "IdOfPerson", "Description");

        //    var sittings = await _db.Sittings
        //                            .Include(s=>s.SittingType)
        //                            .Select(s=> new { 
                                    
        //                                IdOfSitting=s.Id,
        //                                Description= s.SittingType.Name+' '+s.Start.ToString("dddd, dd MMMM yyyy")+' '+ s.Start.ToString("hh:mm tt") +' '+s.End.ToString("hh:mm tt")
        //                            })
        //                            .ToListAsync(); //pull the data first!


        //    ViewBag.SittingId = new SelectList(sittings, "IdOfSitting", "Description");

        //    var source = await _db.ReservationSources.ToListAsync(); //pull the data first!
        //    ViewBag.SourceId = new SelectList(source, "Id", "Name");

           


        //    return View();
        //}


        //Person for manager to enter reser_person details(not member)
        [HttpGet]
        public async Task<IActionResult> Person()
        {

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Person(CreatePerson m)
        {
            if (ModelState.IsValid)
            {
                //add to Person
                var person = new Person
                {

                    FirstName= m.FirstName,
                    LastName=m.LastName,
                    Email=m.Email,
                    Phone=m.Phone
                };

                _db.People.Add(person);
                await _db.SaveChangesAsync();
                return RedirectToAction("Create");

            }

            return View();
        }
     
        // GET: Administration/Reservation/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var reservation = await _db.Reservations.FindAsync(id);
            if (reservation == null)
            {
                return NotFound();
            }
            ViewData["PersonId"] = new SelectList(_db.People, "Id", "Discriminator", reservation.PersonId);
            ViewData["SittingId"] = new SelectList(_db.Sittings, "Id", "Id", reservation.SittingId);
            ViewData["SourceId"] = new SelectList(_db.ReservationSources, "Id", "Id", reservation.SourceId);
            ViewData["StatusId"] = new SelectList(_db.ReservationStatuses, "Id", "Id", reservation.StatusId);
            return View(reservation);
        }

        // POST: Administration/Reservation/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("PersonId,SittingId,StatusId,SourceId,Id,Guest,StartTime,Duration,Note")] Reservation reservation)
        {
            if (id != reservation.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _db.Update(reservation);
                    await _db.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ReservationExists(reservation.Id))
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
            ViewData["PersonId"] = new SelectList(_db.People, "Id", "Discriminator", reservation.PersonId);
            ViewData["SittingId"] = new SelectList(_db.Sittings, "Id", "Id", reservation.SittingId);
            ViewData["SourceId"] = new SelectList(_db.ReservationSources, "Id", "Id", reservation.SourceId);
            ViewData["StatusId"] = new SelectList(_db.ReservationStatuses, "Id", "Id", reservation.StatusId);
            return View(reservation);
        }

        // GET: Administration/Reservation/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var reservation = await _db.Reservations
                .Include(r => r.Person)
                .Include(r => r.Sitting)
                .Include(r => r.Source)
                .Include(r => r.Status)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (reservation == null)
            {
                return NotFound();
            }

            return View(reservation);
        }

        // POST: Administration/Reservation/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var reservation = await _db.Reservations.FindAsync(id);
            _db.Reservations.Remove(reservation);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ReservationExists(int id)
        {
            return _db.Reservations.Any(e => e.Id == id);
        }
    }
}
