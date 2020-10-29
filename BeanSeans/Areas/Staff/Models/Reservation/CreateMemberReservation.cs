using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BeanSeans.Areas.Staff.Models.Reservation
{
    public class CreateMemberReservation
    {
        //Person Properties
        public int MemberId { get; set; }

        public SelectList MemberOptions { get; set; }

        //Reservation Properties
        public int SittingId { get; set; }

        public string Sitting { get; set; }

        public int StatusId { get; set; }
        public SelectList StatusOptions { get; set; }

        public int SourceId { get; set; }
        public SelectList SourceOptions { get; set; }

        public int Guest { get; set; }
        public DateTime StartTime { get; set; }
        public int Duration { get; set; }
        public string Note { get; set; }
    }
}
