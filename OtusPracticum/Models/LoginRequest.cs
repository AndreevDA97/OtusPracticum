namespace OtusPracticum.Models
{
    public class LoginRequest
    {
        public Guid Id { get; set; }
        public string Password { get; set; } = null!;
    }
}
