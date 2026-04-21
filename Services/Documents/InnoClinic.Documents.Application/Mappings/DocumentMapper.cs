using InnoClinic.Documents.Application.DTOs;
using InnoClinic.Documents.Domain.Models;
using Riok.Mapperly.Abstractions;

namespace InnoClinic.Documents.Application.Mappings
{
    [Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
    public partial class DocumentMapper
    {
        public partial DocumentResponseDTO ToDto(Document entity);
    }
}