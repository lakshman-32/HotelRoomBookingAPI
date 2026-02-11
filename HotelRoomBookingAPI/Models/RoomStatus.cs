using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HotelRoomBookingAPI.Models
{
    public class RoomStatus
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Booking")]
        public int BookingId { get; set; }
        public virtual Booking? Booking { get; set; }

        public bool RoomCleaned { get; set; }
        public bool BathroomCleaned { get; set; }
        public bool BedCleaned { get; set; }
        public bool WaterBottlesProvided { get; set; }
        public bool BedsheetProvided { get; set; }
        public bool TowelProvided { get; set; }
        public bool DustbinCleaned { get; set; }

        public string? Remarks { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
