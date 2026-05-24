using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Authorization.Application.DTOs.Resource
{
    public record FieldDto(string Name,string BaseType, string ClrType, string? DisplayName);

    public record JoinDto(
        string NavigationName,
        string TargetEntity,
        string TargetKey,      // PK property names on the target entity
        string CurrentKey,     // FK property names on the current entity

        List<FieldDto> TargetScalarFields
    );

    public record ResourceMetadataDto(
        string ResourceKey,
        string EntityName,
        List<FieldDto> ScalarFields,
        List<JoinDto> Joins
    );

}
