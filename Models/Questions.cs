

namespace RAGSystemAPI.Models
{
    public class Question
    {
        public string Text { get; set; }
        public string Answer { get; set; }
        public List<string> Sources { get; set; }
    }
}