namespace UtilityData
{
    public interface Identifiable
    {
        string GetInternalName ();
        int GetUniqueID ();
        Identifier GetIdentifier ();
    }

    public class Identifier
    {
        public string nameInternal = "Not_Defined";
        public string icon = "Not_Defined";

        public Identifier ()
        {

        }

        public Identifier (string nameInternal, string icon)
        {
            this.nameInternal = nameInternal;
            this.icon = icon;
        }

        public Identifier (string nameInternal)
        {
            this.nameInternal = nameInternal;
        }

        private int id = 0;
        public int uniqueID
        {
            get
            {
                if (id == 0)
                    id = nameInternal.GetHashCode ();
                return id;
            }
        }

        private string displayNameKey = "";
        public string displayName
        {
            get
            {
                if (displayNameKey == "")
                    displayNameKey = nameInternal + "_Name";

                return displayNameKey;
            }
        }

        private string descriptionKey = "";
        public string description
        {
            get
            {
                if (descriptionKey == "")
                    descriptionKey = nameInternal + "_Description";

                return descriptionKey;
            }
        }
    }
}