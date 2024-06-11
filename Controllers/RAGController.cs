using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using RAGSystemAPI.Services;
using PersonalSiteBackend.DTO;
using System.Threading.Tasks;
using PersonalSiteBackend.Models;

namespace RAGSystemAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RagController : ControllerBase
    {
        private readonly RagService _ragService;

        public RagController(RagService ragService)
        {
            _ragService = ragService;
        }

        [HttpPost("import")]
        public async Task<IActionResult> ImportDocument([FromBody] DocumentDto documentDto)
        {
            if (string.IsNullOrWhiteSpace(documentDto.Name))
            {
                return BadRequest(new { Message = "The document name is required." });
            }

            await _ragService.ImportDocumentAsync(documentDto);

            if (await _ragService.IsDocumentReadyAsync(documentDto.Id))
            {
                return Ok(new { Message = "Document imported successfully." });
            }
            else
            {
                return StatusCode(500, new { Message = "Document import failed." });
            }
        }

        [HttpPost("ask")]
        [EnableCors("AllowSpecificOrigin")]
        public async Task<IActionResult> AskQuestion([FromBody] AskQuestionDTO questionDto)
        {
            var result = await _ragService.AskAsync(questionDto.Text);
            return Ok(result);
        }

        [HttpPost("deleteDB")]
        public async Task<IActionResult> DeleteDataBase()
        {
            await _ragService.ClearDataBase();
            return Ok(new { Message = "Database successfully deleted" });
        }
    }
}