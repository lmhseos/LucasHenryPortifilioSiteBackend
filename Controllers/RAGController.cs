﻿using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using RAGSystemAPI.Services;
using PersonalSiteBackend.Dto;
using PersonalSiteBackend.DTO;
using SQLitePCL;

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
        [EnableCors("AllowSpecificOrigin")]
        public async Task<IActionResult> AskQuestion([FromBody] AskQuestionDto questionDto)
        {
            var result = await ragService.AskAsync(questionDto.Text);
            return Ok(result);
        }
        [HttpPost("deleteDB")]
        public async Task<IActionResult> DeleteDataBase()
        {
            
          await ragService.ClearDataBase();
          return Ok(new { Message = "Database successfully implemented" });
          
        }
        
    }
}