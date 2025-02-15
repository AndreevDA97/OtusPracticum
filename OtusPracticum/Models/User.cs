using System.Text.Json.Serialization;

namespace OtusPracticum.Models
{
    public class User
    {
        public Guid User_id { get; set; }
        public string First_name { get; set; } = null!;
        public string Second_name { get; set; } = null!;
        public string Birthdate { get; set; } = null!;
        public string Biography { get; set; } = null!;
        public string City { get; set; } = null!;
        [JsonIgnore]
        public string Password { get; set; } = null!;

        public User(Guid id, Dictionary<string, object> data)
        {
            User_id = id;
            First_name = data[nameof(First_name)].ToString()!;
            Second_name = data[nameof(Second_name)].ToString()!;
            Birthdate = data[nameof(Birthdate)].ToString()!;
            Biography = data[nameof(Biography)].ToString()!;
            City = data[nameof(City)].ToString()!;
            Password = data[nameof(Password)].ToString()!;
        }

        public User() { }
    }
}
