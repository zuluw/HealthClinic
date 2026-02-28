using Riok.Mapperly.Abstractions;
using InnoClinic.Services.Application.DTOs;
using InnoClinic.Services.Domain.Models;

namespace InnoClinic.Services.Application.Mappings
{
    [Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
    public partial class ServiceMapper
    {
        public partial ServiceResponseDTO ToServiceDto(Service entity);

        public SpecializationResponseDTO ToSpecializationDto(Specialization spec)
        {
            return new SpecializationResponseDTO(
                spec.Name,
                spec.Services.Select(ToServiceDto).ToList()
            );
        }
    }
}