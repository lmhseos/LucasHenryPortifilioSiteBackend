using System.Collections.Generic;
using Google.Cloud.Firestore;

namespace PersonalSiteBackend.Models
{
    [FirestoreData]
    public class Question
    {
        [FirestoreDocumentId]
        public string Id { get; set; }
        
        [FirestoreProperty]
        public string Text { get; set; }

        [FirestoreProperty]
        public string Answer { get; set; }

        [FirestoreProperty]
        public List<string> Sources { get; set; }
    }
}