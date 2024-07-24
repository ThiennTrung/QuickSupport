using System.Text;

namespace QuickSupport_v2.Model
{
    public class Result
    {
        public Result()
        {
            TextInput = new StringBuilder();
            Query = new StringBuilder();
            IdDao = string.Empty;
        }
        public StringBuilder TextInput { get; set; }
        public StringBuilder Query { get; set; }
        public string IdDao { get; set; }
    }
}
