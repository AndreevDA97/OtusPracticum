using System.Text.Json;

namespace OtusPracticum
{
    public static  class Consts
    {
        public static JsonSerializerOptions JsonSerializerOptions { get; set; } = new(JsonSerializerDefaults.Web);
    }
}
