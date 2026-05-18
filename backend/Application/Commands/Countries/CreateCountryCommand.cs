namespace Application.Commands.Countries;

public sealed record CreateCountryCommand(string Name, string IsoCode);
