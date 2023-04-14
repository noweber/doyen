namespace Doyen.API.Dtos
{
    public class Expert
    {
        public Expert(string? firstName, string? lastName, string? identifier)
        {

            Identifier = identifier;
            if (firstName != null)
            {
                Name += firstName;
            }
            if (firstName != null && lastName != null)
            {
                Name += " ";
            }
            if (lastName != null)
            {
                Name += lastName;
            }
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
