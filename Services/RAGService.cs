using dotenv.net;
using Microsoft.KernelMemory;
using RAGSystemAPI.Models;

namespace RAGSystemAPI.Services
{
    public class RAGService
    {
        private readonly MemoryServerless _memory;

        public RAGService()
        {
            DotEnv.Load();
            var env = Environment.GetEnvironmentVariable("OPEN_API_KEY");

            _memory = new KernelMemoryBuilder()
                .WithOpenAIDefaults(env)
                .Build<MemoryServerless>();
        }

        public async Task ImportDocumentAsync(string path, string documentId)
        {
            await _memory.ImportDocumentAsync(path, documentId);
        }

        public async Task<bool> IsDocumentReadyAsync(string documentId)
        {
            return await _memory.IsDocumentReadyAsync(documentId);
        }

        public async Task<Question> AskAsync(string input)
        {
            var answer = await _memory.AskAsync(input);

            return new Question
            {
                Text = input,
                Answer = answer.Result,
                Sources = answer.RelevantSources.Select(x => $"{x.SourceName} - {x.Link} [{x.Partitions.First().LastUpdate:D}]").ToList()
            };
        }
    }
}