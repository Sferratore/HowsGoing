namespace HowsGoing.Models
{
    public class Record
    {
        public Record(int id, string content, int satisfaction)
        {
            this.record_id = id;
            this.record_content = content;
            this.satisfaction = satisfaction;
        }

        public int record_id { get; set; }
        public string record_content { get; set; }
        public int satisfaction { get; set; }
    }

}
