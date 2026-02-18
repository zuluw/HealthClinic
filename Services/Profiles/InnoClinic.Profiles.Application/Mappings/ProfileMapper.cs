using Riok.Mapperly.Abstractions;
using InnoClinic.Profiles.Application.DTOs;
using InnoClinic.Profiles.Domain.Models;

namespace InnoClinic.Profiles.Application.Mappings
{
    [Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
    public partial class ProfileMapper
    {
        public partial Patient ToEntity(PatientRequestDTO dto);
        public partial Doctor ToEntity(DoctorRequestDTO dto);

        public partial void UpdatePatient(PatientRequestDTO dto, Patient patient);
        public partial void UpdateDoctor(DoctorRequestDTO dto, Doctor doctor);

        public partial PatientResponseDTO ToResponse(Patient patient);
        [MapProperty(nameof(Doctor.Specializations), nameof(DoctorResponseDTO.SpecializationNames))]
        public partial DoctorResponseDTO ToResponse(Doctor doctor);

        private IEnumerable<string> MapSpecs(ICollection<Specialization> specs)
        {
            return specs.Select(s => s.Name);
        }
    }
}