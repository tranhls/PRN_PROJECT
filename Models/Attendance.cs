namespace PRN222_Assm.Models
{
    public class Attendance
    {
        public int Id { get; set; }
        public int Day { get; set; }

        public bool isPresent { get; set; }
        public virtual StudentClass? Class { get; set; }
    }
}
