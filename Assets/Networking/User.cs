namespace Net
{
    /// <summary>
    /// A network client.
    /// </summary>
    public sealed class User
    {
        public readonly ushort id;
        public readonly string name;

        public User(ushort id, string name)
        {
            this.id = id;
            this.name = name;
        }

        public override bool Equals(object obj)
        {
            if (obj is User player)
            {
                return player.id == id;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return id;
        }

        public override string ToString()
        {
            return $"{id}#{name}";
        }

    }
}
