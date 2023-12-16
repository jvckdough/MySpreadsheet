// Skeleton implementation by: Joe Zachary, Daniel Kopta, Travis Martin for CS 3500
// Last updated: August 2023 (small tweak to API)

namespace SpreadsheetUtilities;

/// <summary>
/// (s1,t1) is an ordered pair of strings
/// t1 depends on s1; s1 must be evaluated before t1
/// 
/// A DependencyGraph can be modeled as a set of ordered pairs of strings.  Two ordered pairs
/// (s1,t1) and (s2,t2) are considered equal if and only if s1 equals s2 and t1 equals t2.
/// Recall that sets never contain duplicates.  If an attempt is made to add an element to a 
/// set, and the element is already in the set, the set remains unchanged.
/// 
/// Given a DependencyGraph DG:
/// 
///    (1) If s is a string, the set of all strings t such that (s,t) is in DG is called dependents(s).
///        (The set of things that depend on s)    
///        
///    (2) If s is a string, the set of all strings t such that (t,s) is in DG is called dependees(s).
///        (The set of things that s depends on) 
//
// For example, suppose DG = {("a", "b"), ("a", "c"), ("b", "d"), ("d", "d")}
//     dependents("a") = {"b", "c"}
//     dependents("b") = {"d"}
//     dependents("c") = {}
//     dependents("d") = {"d"}
//     dependees("a") = {}
//     dependees("b") = {"a"}
//     dependees("c") = {"a"}
//     dependees("d") = {"b", "d"}
//
// Authors: Joe Zachary, Daniel Kopta, Travis Martin, Jack Doughty for CS 3500
// Last updated: September 2023 
/// </summary>
public class DependencyGraph
{

    private int size;
    //dependents
    private Dictionary<string,  HashSet<string>> dependents;
    //dependees
    private Dictionary<string, HashSet<string>> dependees;

    /// <summary>
    /// Creates an empty DependencyGraph.
    /// </summary>
    public DependencyGraph()
    {
        size = 0;
        dependents= new Dictionary<string, HashSet<string>>();
        dependees = new Dictionary<string, HashSet<string>>();
    }
    /// <summary>
    /// The number of ordered pairs in the DependencyGraph.
    /// This is an example of a property.
    /// </summary>
    public int NumDependencies
    {
        get { return size; }
    }


    /// <summary>
    /// Returns the size of dependees(s),
    /// that is, the number of things that s depends on.
    /// </summary>
    /// <param name="s">The value whos number of dependencies is returned</param>
    /// <returns>Number of dependencies</returns>
    public int NumDependees(string s)
    {
        if (dependees.ContainsKey(s))
        {
            return dependees[s].Count;
        }
        return 0;
    }


    /// <summary>
    /// Reports whether dependents(s) is non-empty.
    /// </summary>
    /// <param name="s">The value whos dependents are to be checked</param>
    /// <returns></returns>
    public bool HasDependents(string s)
    {
        if (dependents.ContainsKey(s))
        {
            return dependents[s].Count != 0;
        }
        return false;
    }


    /// <summary>
    /// Reports whether dependees(s) is non-empty.
    /// </summary>
    /// <param name="s">The value whos dependees are to be checked</param>
    /// <returns>boolean value indicating existence of dependees</returns>
    public bool HasDependees(string s)
    {
        if (dependees.ContainsKey(s))
        {
            return dependees[s].Count != 0;
        }
        return false;
    }


    /// <summary>
    /// Enumerates dependents(s).
    /// </summary>
    /// <param name="s">The value whos dependents are retrieved</param>
    /// <returns>each dependent</returns>
    public IEnumerable<string> GetDependents(string s)
    {
        if (dependents.ContainsKey(s))
        {
            foreach (string dependent in dependents[s])
            {
                yield return dependent;
            }
        }
    }


    /// <summary>
    /// Enumerates dependees(s).
    /// </summary>
    /// </summary>
    /// <param name="s">The value whos dependees are retrieved</param>
    /// <returns>each dependee</returns>
    public IEnumerable<string> GetDependees(string s)
    {
        if (dependees.ContainsKey(s))
        {
            foreach (string dependee in dependees[s])
            {
                yield return dependee;
            }
        }
    }


    /// <summary>
    /// <para>Adds the ordered pair (s,t), if it doesn't exist</para>
    /// 
    /// <para>This should be thought of as:</para>   
    /// 
    ///   t depends on s
    ///
    /// </summary>
    /// <param name="s"> s must be evaluated first. T depends on S</param>
    /// <param name="t"> t cannot be evaluated until s is</param>
    public void AddDependency(string s, string t)
    {
        bool isAdded1 = false;
        bool isAdded2 = false;
        if (!dependents.ContainsKey(s))
        {
            //Adds 's' to both dictionaries 
            dependents.Add(s, new HashSet<string>());
            dependees.Add(s, new HashSet<string>());
        }
        isAdded1 = dependents[s].Add(t);
        if (!dependees.ContainsKey(t))
        {
            //Adds 't' to both dictionaries 
            dependees.Add(t, new HashSet<string>());
            dependents.Add(t, new HashSet<string>());
        }
        isAdded2 = dependees[t].Add(s);

        //If both s and t were not added successfully then the dependency graph did not change.
        if (isAdded1 || isAdded2)
        {
            size++;
        }
    }

    /// <summary>
    /// Removes the ordered pair (s,t), if it exists
    /// </summary>
    /// <param name="s">the dependent</param>
    /// <param name="t">the dependee</param>
    public void RemoveDependency(string s, string t)
    {
        if (dependents.ContainsKey(s) && dependees.ContainsKey(t))
        {
            if (dependents[s].Remove(t) && dependees[t].Remove(s))
            {
                size--;
            }
        }
    }


    /// <summary>
    /// Removes all existing ordered pairs of the form (s,r).  Then, for each
    /// t in newDependents, adds the ordered pair (s,t).
    /// </summary>
    /// <param name="s">The value to recieve new dependents</param>
    /// <param name="newDependents">Contains the new dependents</param>
    public void ReplaceDependents(string s, IEnumerable<string> newDependents)
    {
        if (dependents.ContainsKey(s) && dependents[s].Count != 0)
        {
            //Removes all existing ordered pairs of the form (s, r) described in both dictionaries
            size = size - dependents[s].Count;
            removeVal(s, dependees, false);
            dependents[s].Clear();
            //adds each new dependent
            foreach (string t in newDependents)
            {
                AddDependency(s, t);
            }
        }
        else
            foreach (string t in newDependents)
            {
                AddDependency(s, t);
            }
    }

    /// <summary>
    /// Removes all existing ordered pairs of the form (r,s).  Then, for each 
    /// t in newDependees, adds the ordered pair (t,s).
    /// </summary>
    /// <param name="s">The value to recieve new dependees</param>
    /// <param name="newDependees">Contains the new dependees</param>
    public void ReplaceDependees(string s, IEnumerable<string> newDependees)
    {
        if (dependents.ContainsKey(s) && dependees[s].Count != 0)
        {
            //Removes all existing ordered pairs of the form (r, s) described in both dictionaries
            size = size - dependees[s].Count;
            removeVal(s, dependents, true);
            dependees[s].Clear();
            //adds each new dependee
            foreach (string t in newDependees)
            {
                AddDependency(t, s);
            }
        }
        else
            foreach(string t in newDependees)
            {
                AddDependency(t, s);
            }
    }
    /// <summary>
    /// Private helper method that removes all the occurences of the value given in each hashset of the dicitionary given.
    /// The new dictionary replaces the given dictionary.
    /// </summary>
    /// <param name="s">The value to be removed</param>
    /// <param name="dictionary">The dictionary to be filtered</param>
    /// <param name="isDependents">If the dictionary is 'Dependents' it is updated without 's'. Else it iss dependees</param>
    private void removeVal(string s, Dictionary<string, HashSet<String>> dictionary, bool isDependents)
    {
        //New dictionary to hold the same values as dictionary but with out value 's'
        Dictionary<string, HashSet<string>> updatedDictionary = new Dictionary<string, HashSet<string>>();
        //loops through each set in the dictionary and removes 's'
        foreach (KeyValuePair<string, HashSet<string>> set in dictionary)
        {
            HashSet<string> updatedSet = new HashSet<string>(set.Value);
            updatedSet.Remove(s);
            updatedDictionary.Add(set.Key, updatedSet);
        }
        if (isDependents)
        {
            dependents = updatedDictionary;
        }
        else dependees = updatedDictionary;
    }

}

