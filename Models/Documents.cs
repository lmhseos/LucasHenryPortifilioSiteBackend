using System.Collections.Generic;
using Google.Cloud.Firestore;

namespace PersonalSiteBackend.Models
{
    [FirestoreData]
    public class Document
    {
        [FirestoreDocumentId]
        public string Id { get; set; }

        [FirestoreProperty]
        public string Name { get; set; }

        [FirestoreProperty]
        public string Content { get; set; }
    }
}