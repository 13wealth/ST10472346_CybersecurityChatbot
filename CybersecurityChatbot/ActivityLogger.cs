using System;
using System.Collections.Generic;

public class ActivityLogger
{
    private List<string> _log = new List<string>();

    /// <summary>
    /// Logs an action with a timestamp
    /// </summary>
    public void Log(string action)
    {
        string entry = DateTime.Now.ToString("[HH:mm] ") + action;
        _log.Add(entry);
    }

    /// <summary>
    /// Returns the last 'count' entries as a numbered list
    /// If fewer than count entries exist, returns all of them
    /// </summary>
    public string GetRecentLog(int count = 10)
    {
        if (_log.Count == 0)
            return "No activity logged yet.";

        // Figure out where to start (last 'count' entries)
        int startIndex = _log.Count - count;
        if (startIndex < 0)
            startIndex = 0;

        // Build the numbered list
        string result = "";
        int entryNumber = 1;

        for (int i = startIndex; i < _log.Count; i++)
        {
            result = result + entryNumber + ". " + _log[i] + "\n";
            entryNumber = entryNumber + 1;
        }

        // Remove the last newline
        return result.TrimEnd();
    }

    /// <summary>
    /// Returns all entries as a numbered list
    /// </summary>
    public string GetFullLog()
    {
        if (_log.Count == 0)
            return "No activity logged yet.";

        string result = "";

        for (int i = 0; i < _log.Count; i++)
        {
            result = result + (i + 1) + ". " + _log[i] + "\n";
        }

        return result.TrimEnd();
    }

    /// <summary>
    /// Returns the total number of entries in the log
    /// </summary>
    public int GetCount()
    {
        return _log.Count;
    }
}
