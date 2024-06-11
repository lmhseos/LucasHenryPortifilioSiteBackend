using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using dotenv.net;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Firestore;
using Microsoft.Extensions.Configuration;
using Microsoft.KernelMemory;
using PersonalSiteBackend.DTO;
using PersonalSiteBackend.Models;
using Document = PersonalSiteBackend.Models.Document;

namespace RAGSystemAPI.Services
{
    public class RagService
    {
        private readonly FirestoreDb _firestoreDb;
        private readonly MemoryServerless _memory;

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

            _memory = new KernelMemoryBuilder()
                .WithOpenAIDefaults(openApiKey)
                .Build<MemoryServerless>();
        }

        public async Task ImportDocumentAsync(DocumentDto documentDto)
        {
            if (documentDto == null) throw new ArgumentNullException(nameof(documentDto));

            string content;
            if (documentDto.Name.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
            {
                content = ExtractTextFromPdf(documentDto.Name);
            }
            else
            {
                content = documentDto.Content ?? await File.ReadAllTextAsync(documentDto.Name);
            }

            var document = new Document
            {
                Id = Guid.NewGuid().ToString(),
                Name = NormalizePath(documentDto.Name),
                Content = content
            };

            DocumentReference docRef = _firestoreDb.Collection("documents").Document(document.Id);
            await docRef.SetAsync(document);

            documentDto.Id = document.Id; // Assign the generated Id back to the DTO

            await _memory.ImportDocumentAsync(document.Name, document.Id);
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

        private static string NormalizePath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentException("Path cannot be null or whitespace.", nameof(path));

            return path.Replace("\\", "/").Trim();
        }

        private static string ExtractTextFromPdf(string path)
        {
            using (var document = UglyToad.PdfPig.PdfDocument.Open(path))
            {
                var text = new StringBuilder();

                foreach (var page in document.GetPages())
                {
                    text.Append(page.Text);
                }

                return text.ToString();
            }
        }

        public async Task LoadAllDataAsync()
        {
            CollectionReference documents = _firestoreDb.Collection("documents");
            QuerySnapshot snapshot = await documents.GetSnapshotAsync();

            foreach (var doc in snapshot.Documents)
            {
                var document = doc.ConvertTo<Document>();
                await _memory.ImportDocumentAsync(document.Name, document.Id);
            }
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
