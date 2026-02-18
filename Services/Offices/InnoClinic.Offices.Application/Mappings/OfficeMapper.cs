using Riok.Mapperly.Abstractions;
using InnoClinic.Offices.Application.DTOs;
using InnoClinic.Offices.Domain.Models;

namespace InnoClinic.Offices.Application.Mappings
{
    [Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
    public partial class OfficeMapper
    {
        public partial Office ToEntity(OfficeRequestDTO dto);

        public partial OfficeResponseDTO ToResponse(Office office);

        public partial void UpdateEntity(OfficeRequestDTO dto, Office office);

        public partial IEnumerable<OfficeResponseDTO> ToResponseList(IEnumerable<Office> offices);
    }
}