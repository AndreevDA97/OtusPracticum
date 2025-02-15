namespace OtusPracticum.Models
{
    public class UserRegisterRequest
    {
        public string First_name { get; set; } = null!;
        public string Second_name { get; set; } = null!;
        public string Birthdate { get; set; } = null!;
        public string Biography { get; set; } = null!;
        public string City { get; set; } = null!;
        public string Password { get; set; } = null!;
    }
}
