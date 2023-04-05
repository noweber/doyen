namespace Doyen.API.Dtos
{
    public class Expert
    {
        public Expert(string firstName, string lastName, string? identifier)
        {

            Identifier = identifier;
            Name = firstName + " " + lastName;
        }

        public Expert(string name, string? identifier)
        {
            Identifier = identifier;
            Name = name;
        }

        public string? Identifier { get; set; }

        public string? Name { get; set; }
    }
}
