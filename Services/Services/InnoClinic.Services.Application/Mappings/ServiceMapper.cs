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
                spec.Id,
                spec.Name, 
                spec.IsActive, 
                spec.Services.Select(ToServiceDto).ToList()
            );
        }

        public partial Specialization ToEntity(SpecializationRequestDTO dto);
        public partial Service ToEntity(ServiceRequestDTO dto);
        
        [MapperIgnoreTarget(nameof(Specialization.Services))]
        [MapperIgnoreTarget(nameof(Specialization.Id))]
        public partial void UpdateSpecialization(UpdateSpecializationRequestDTO dto, Specialization entity);
        public partial SpecializationListDTO ToSpecializationListDto(Specialization spec);
    }
}