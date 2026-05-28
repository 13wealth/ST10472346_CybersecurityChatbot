namespace Cybersecurity_Chatbot
{
    /**
     * Allows a stored value to be gobally used accross the program. 
     * In this case, it is used to store the user's name after it is collected in UI.GetUserData()
     * get reads the value, set assigns a value to it.
     */
    public static class StateSharing
    {
        public static string Name { get; set; }
    }
}