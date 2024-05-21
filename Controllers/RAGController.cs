using Microsoft.AspNetCore.Mvc;
using RAGSystemAPI.Services;
using System.Threading.Tasks;
using PersonalSiteBackend.DTO;

namespace RAGSystemAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RagController(RagService ragService) : ControllerBase
    {
        [HttpPost("import")]
        public async Task<IActionResult> ImportDocument([FromBody] DocumentDto documentDto)
        {
            await ragService.ImportDocumentAsync(documentDto);

            if (await ragService.IsDocumentReadyAsync(documentDto.Id))
            {
                return Ok(new { Message = "Document imported successfully." });
            }
            else
            {
                return StatusCode(500, new { Message = "Document import failed." });
            }
        }

        [HttpPost("ask")]
        public async Task<IActionResult> AskQuestion([FromBody] AskQuestionDto questionDto)
        {
            var result = await ragService.AskAsync(questionDto.Text);
            return Ok(result);
        }
    }
}