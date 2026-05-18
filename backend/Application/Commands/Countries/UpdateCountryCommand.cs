namespace Application.Commands.Countries;

public sealed record UpdateCountryCommand(int Id, string Name, string IsoCode);
