using Google.Cloud.Firestore;

namespace PersonalSiteBackend.Models
{
    public class AskQuestionDTO
    {
        public string Text { get; set; }

        public AskQuestionDTO()
        {
            // Parameterless constructor required for Firestore
        }
    }
}