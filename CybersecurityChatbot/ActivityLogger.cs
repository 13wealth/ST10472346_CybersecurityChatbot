using System;
using System.Collections.Generic;

public class ActivityLogger
{
    private List<string> _log = new List<string>();

    /*
     * This class is used to log user activity in the chatbot application.
     */
    public void Log(string action)
    {
        string entry = DateTime.Now.ToString("[HH:mm] ") + action;
        _log.Add(entry);
    }

    /*
     * Returns the last 'count' entries as a numbered list. 
     * If there are fewer than 'count' entries, it returns all of them.
     */
    public string GetRecentLog(int count = 10)
    {
        if (_log.Count == 0)
            return "No activity logged yet.";

        int startIndex = _log.Count - count;                                            // Figure out where to start (last 'count' entries)
        if (startIndex < 0)
            startIndex = 0;

        
        string result = "";                                                             // Build the numbered list
        int entryNumber = 1;

        for (int i = startIndex; i < _log.Count; i++)
        {
            result = result + entryNumber + ". " + _log[i] + "\n";
            entryNumber = entryNumber + 1;
        }

        
        return result.TrimEnd();                                                        // Remove the last newline
    }

    /*
     * Returns all entries in the log as a numbered list.
     */
    public string GetFullLog()
    {
        if (_log.Count == 0)
            return "No activity logged yet.";

        string result = ""; 

        for (int i = 0; i < _log.Count; i++)                                            // Iterate through the entire log and build a numbered list
        {
            result = result + (i + 1) + ". " + _log[i] + "\n";                          // Build the numbered list
        }

        return result.TrimEnd();
    }

    /*
     * Returns the number of entries in the log.
     */
    public int GetCount()
    {
        return _log.Count;
    }
}
