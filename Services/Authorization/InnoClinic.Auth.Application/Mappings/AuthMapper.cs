using Riok.Mapperly.Abstractions;
using InnoClinic.Auth.Application.DTOs;
using InnoClinic.Auth.Domain.Models;

namespace InnoClinic.Auth.Application.Mappings
{
    [Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
    public partial class AuthMapper
    {
        [MapperIgnoreTarget(nameof(User.PasswordHash))]
        public partial User ToEntity(RegisterRequestDTO dto);

        [MapProperty(nameof(User.Id), nameof(AuthResponseDTO.UserId))]
        public partial AuthResponseDTO ToResponse(User user, string accessToken, string refreshToken, string message);
    }
}