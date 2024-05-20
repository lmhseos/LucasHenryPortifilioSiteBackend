using Microsoft.AspNetCore.Mvc;
using RAGSystemAPI.DTO;
using RAGSystemAPI.Models;
using RAGSystemAPI.Services;

namespace RAGSystemAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RAGController : ControllerBase
    {
        private readonly RAGService _ragService;

        public RAGController(RAGService ragService)
        {
            _ragService = ragService;
        }

        [HttpPost("import")]
        public async Task<IActionResult> ImportDocument([FromBody] Document document)
        {
            await _ragService.ImportDocumentAsync(document.Name, document.Id);

            if (await _ragService.IsDocumentReadyAsync(document.Id))
            {
                return Ok(new { Message = "Document imported successfully." });
            }
            else
            {
                return StatusCode(500, new { Message = "Document import failed." });
            }
        }

        [HttpPost("ask")]
        public async Task<IActionResult> AskQuestion([FromBody] AskQuestionDTO questionDTO)
        {
            var result = await _ragService.AskAsync(questionDTO.Text);
            return Ok(result);
        }
    }
}