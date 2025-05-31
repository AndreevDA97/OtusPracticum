using System.Text.Json.Serialization;

namespace OtusPracticum.Entities
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
            First_name = data["first_name"].ToString()!;
            Second_name = data["second_name"].ToString()!;
            Birthdate = data["birthdate"].ToString()!;
            Biography = data["biography"].ToString()!;
            City = data["city"].ToString()!;
            Password = data["password"].ToString()!;
        }

        public User() { }
    }
}
