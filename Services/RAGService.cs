using System;
using System.Linq;
using System.Threading.Tasks;
using dotenv.net;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Firestore;
using Microsoft.Extensions.Configuration;
using PersonalSiteBackend.DTO;
using PersonalSiteBackend.Models;
using Document = PersonalSiteBackend.Models.Document;

namespace RAGSystemAPI.Services
{
    public class RagService
    {
        private readonly FirestoreDb _firestoreDb;
        private readonly ExtendedMemoryServerless _memory;

        public RagService(IConfiguration configuration)
        {
            DotEnv.Load();

            string basePath = AppDomain.CurrentDomain.BaseDirectory;
            string credentialsPath = Path.Combine(basePath, "firebase.json");
            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", credentialsPath);

            if (FirebaseApp.DefaultInstance == null)
            {
                FirebaseApp.Create(new AppOptions
                {
                    Credential = GoogleCredential.FromFile(credentialsPath)
                });
            }

            var projectId = configuration["Firebase:ProjectId"];
            if (string.IsNullOrEmpty(projectId))
            {
                throw new InvalidOperationException("Firebase ProjectId is not configured.");
            }

            _firestoreDb = FirestoreDb.Create(projectId);

            var openApiKey = Environment.GetEnvironmentVariable("OPEN_API_KEY") ??
                             throw new InvalidOperationException("OPEN_API_KEY not found in environment variables");

            _memory = new ExtendedMemoryServerless(openApiKey);
        }

        public async Task ImportDocumentAsync(DocumentDto documentDto)
        {
            if (documentDto == null) throw new ArgumentNullException(nameof(documentDto));

            var document = new Document
            {
                Id = Guid.NewGuid().ToString(),
                Name = documentDto.Name,
                Content = documentDto.Content
            };

            DocumentReference docRef = _firestoreDb.Collection("documents").Document(document.Id);
            await docRef.SetAsync(document);

            documentDto.Id = document.Id;

            await _memory.ImportDocumentContentAsync(document.Content, document.Id);
        }

        public async Task<bool> IsDocumentReadyAsync(string documentId)
        {
            if (string.IsNullOrWhiteSpace(documentId)) throw new ArgumentNullException(nameof(documentId));

            return await _memory.IsDocumentReadyAsync(documentId);
        }

        public async Task<Question> AskAsync(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                throw new ArgumentException("Input cannot be null or whitespace.", nameof(input));

            var answer = await _memory.AskAsync(input);

            var question = new Question
            {
                Id = Guid.NewGuid().ToString(),
                Text = input,
                Answer = answer.Result,
                Sources = answer.RelevantSources
                    .Select(x => $"{x.SourceName} - {x.Link} [{x.Partitions.First().LastUpdate:D}]").ToList()
            };

            DocumentReference docRef = _firestoreDb.Collection("questions").Document(question.Id);
            await docRef.SetAsync(question);

            return question;
        }

        public async Task LoadAllDataAsync()
        {
            try
            {
                CollectionReference documents = _firestoreDb.Collection("documents");
                QuerySnapshot snapshot = await documents.GetSnapshotAsync();

                foreach (var doc in snapshot.Documents)
                {
                    var document = doc.ConvertTo<Document>();
                    Console.WriteLine($"Loading Document ID: {document.Id}, Content Length: {document.Content?.Length}");

                    await ImportContentToMemory(document.Content, document.Id);
                }
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Error loading data from Firestore: {ex.Message}");
                throw;
            }
        }


        private async Task ImportContentToMemory(string content, string documentId)
        {
            await _memory.ImportDocumentContentAsync(content, documentId);
        }

        public async Task ClearDataBase()
        {
            CollectionReference documents = _firestoreDb.Collection("documents");
            QuerySnapshot snapshot = await documents.GetSnapshotAsync();

            foreach (var doc in snapshot.Documents)
            {
                await doc.Reference.DeleteAsync();
            }

            CollectionReference questions = _firestoreDb.Collection("questions");
            snapshot = await questions.GetSnapshotAsync();

            foreach (var doc in snapshot.Documents)
            {
                await doc.Reference.DeleteAsync();
            }
        }
    }
}
