namespace QuickSupport_v2.Model
{
    public class ObjConnect
    {
        public string code { get; set; }
        public string display { get; set; }

        public string Benhvien_id { get; set; }
        public string value { get; set; }

        public ObjConnect(string code,string display,string value = null, string Benhvien_id = null )
        {
            this.code = code;
            this.display = display;
            this.value = value;
            this.Benhvien_id = Benhvien_id;
        }
    }
}
