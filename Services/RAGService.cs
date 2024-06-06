using dotenv.net;
using Microsoft.EntityFrameworkCore;
using Microsoft.KernelMemory;
using PersonalSiteBackend.Data;
using PersonalSiteBackend.DTO;
using PersonalSiteBackend.Models;
using Document = PersonalSiteBackend.Models.Document;

namespace RAGSystemAPI.Services
{
    public class RagService
    {
        private readonly RagDbContext _context;
        private readonly MemoryServerless _memory;

        public RagService(RagDbContext context)
        {
            _context = context;
            DotEnv.Load();
            var openApiKey = Environment.GetEnvironmentVariable("OPEN_API_KEY") ?? throw new InvalidOperationException("OPEN_API_KEY not found in environment variables");

            _memory = new KernelMemoryBuilder()
                .WithOpenAIDefaults(openApiKey)
                .Build<MemoryServerless>();
        }

        public async Task ImportDocumentAsync(DocumentDto documentDto)
        {
            if (documentDto == null) throw new ArgumentNullException(nameof(documentDto));

            var normalizedPath = NormalizePath(documentDto.Name);

            var document = new Document
            {
                Name = normalizedPath,
                Content = documentDto.Content ?? await File.ReadAllTextAsync(normalizedPath)
            };

            _context.Documents.Add(document);
            await _context.SaveChangesAsync();

            documentDto.Id = document.Id;

            await _memory.ImportDocumentAsync(document.Name, document.Id.ToString());
        }

        public async Task<bool> IsDocumentReadyAsync(int? documentId)
        {
            if (documentId == null) throw new ArgumentNullException(nameof(documentId));

            return await _memory.IsDocumentReadyAsync(documentId.ToString() ?? string.Empty);
        }

        public async Task<Question> AskAsync(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) throw new ArgumentException("Input cannot be null or whitespace.", nameof(input));

            var answer = await _memory.AskAsync(input);

            var question = new Question
            {
                Text = input,
                Answer = answer.Result,
                Sources = answer.RelevantSources.Select(x => $"{x.SourceName} - {x.Link} [{x.Partitions.First().LastUpdate:D}]").ToList()
            };

            _context.Questions.Add(question);
            await _context.SaveChangesAsync();

            return question;
        }

        private static string NormalizePath(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) throw new ArgumentException("Path cannot be null or whitespace.", nameof(path));
            
            return path.Replace("\\", "/").Trim();
        }
        public async Task LoadAllDataAsync()
        {
           
            var documents = await _context.Documents.ToListAsync();
            
            foreach (var document in documents)
            {
                await _memory.ImportDocumentAsync(document.Name, document.Id.ToString());
            }
        }

        public async Task ClearDataBase()
        {
            var documents = _context.Documents.ToList();
            _context.Documents.RemoveRange(documents);
            await _context.SaveChangesAsync();
        }
    }
}
