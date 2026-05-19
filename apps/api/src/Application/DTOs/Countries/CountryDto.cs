namespace Application.DTOs.Countries;

public sealed record CountryDto(
    int Id,
    string Name,
    string IsoCode);
