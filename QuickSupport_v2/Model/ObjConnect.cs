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
    public class HospitalIP
    {
        public string CLIENT_APPNAME { get; set; }
        public string MABENHVIEN { get; set; }
        public string TENBENHVIEN { get; set; }
        public string IP_DATABASE { get; set; }
        public string IP_REMOTE { get; set; }
        public string IP_API { get; set; }
        public string IP_ELASTIC { get; set; }
        public string ELASTICI_INDEXPREFIX { get; set; }
        public string NOTE { get; set; }
        public string LINK_REPORT { get; set; }
        public string LINK_DOWNAPP { get; set; }
        //public string LINK_RUNAPP { get; set; }
        public bool IS_PRODUCTION { get; set; }

    }
}
