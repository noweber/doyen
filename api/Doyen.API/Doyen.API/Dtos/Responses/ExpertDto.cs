namespace Doyen.API.Dtos.Responses
{
    public class ExpertDto
    {
        public ExpertDto() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExpertDto"/> class with the specified first name, last name, and identifier.
        /// </summary>
        /// <param name="firstName">The first name of the expert.</param>
        /// <param name="lastName">The last name of the expert.</param>
        /// <param name="identifier">The unique identifier of the expert.</param>
        public ExpertDto(string? firstName, string? lastName, string? identifier)
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

        /// <summary>
        /// Initializes a new instance of the <see cref="ExpertDto"/> class with the specified name and identifier.
        /// </summary>
        /// <param name="name">The name of the expert.</param>
        /// <param name="identifier">The unique identifier of the expert.</param>
        public ExpertDto(string name, string? identifier)
        {
            Identifier = identifier;
            Name = name;
        }

        /// <summary>
        /// Gets or sets the unique identifier of the expert.
        /// </summary>
        public string? Identifier { get; set; }

        /// <summary>
        /// Gets or sets the name of the expert.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Determines whether two <see cref="ExpertDto"/> instances are equal.
        /// </summary>
        public static bool operator ==(ExpertDto e1, ExpertDto e2)
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

        /// <summary>
        /// Determines whether two <see cref="ExpertDto"/> instances are not equal.
        /// </summary>
        public static bool operator !=(ExpertDto e1, ExpertDto e2)
        {
            return !(e1 == e2);
        }

        /// <inheritdoc />
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

            ExpertDto other = (ExpertDto)obj;
            return Name == other.Name && Identifier == other.Identifier;
        }

        /// <inheritdoc />
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
