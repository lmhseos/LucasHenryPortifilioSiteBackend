using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace PersonalSiteBackend.DTO
{
    public class DocumentDto
    {
        [BindNever]
        public string Id { get; set; }

        public string Name { get; set; }

        public string Content { get; set; }
    }
}