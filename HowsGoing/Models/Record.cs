namespace HowsGoing.Models
{
    public class Record
    {
        public Record(int id, string content, int satisfaction, string usid)
        {
            this.record_id = id;
            this.record_content = content;
            this.satisfaction = satisfaction;
            this.user_id = usid;
        }

        public int record_id { get; set; }
        public string record_content { get; set; }
        public int satisfaction { get; set; }
        public string user_id { get; set; }
    }

}
