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

        public static bool operator ==(Expert e1, Expert e2)
        {
            if (ReferenceEquals(e1, e2))
            {
                return true;
            }

            if (ReferenceEquals(e1, null) || ReferenceEquals(e2, null))
            {
                return false;
            }

            return e1.Name == e2.Name && e1.Identifier == e2.Identifier;
        }

        public static bool operator !=(Expert e1, Expert e2)
        {
            return !(e1 == e2);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj is null || GetType() != obj.GetType())
            {
                return false;
            }

            Expert other = (Expert)obj;
            return Name == other.Name && Identifier == other.Identifier;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + (Name?.GetHashCode() ?? 0);
                hash = hash * 23 + (Identifier?.GetHashCode() ?? 0);
                return hash;
            }
        }

    }
}
