using System.Text.RegularExpressions;

namespace HADBOL;

public class Parser
{
    readonly string _source;
    int _index = 0;

    List<string> tokens = new List<string>();//temporary for debugging.

    public Parser(string entryFile)
    {
        //_entryFile = entryFile;
        _source = String.Empty;
        AProgram();
    }

    void AProgram()
    {
        bool more;
        do
            more = AStatement();
        while (more);
    }

    bool AStatement()
    {
        /* Example statements:
         * There are four basic types of statement:
         *  The assignment statement
         *  The pattern matching statement
         *  The replacement statement
         *  The end statement
        OUTPUT = "Hello, World!"                                                // Setting a special variable.
        Username = INPUT                                                        // Getting from a special variable.
END                                                                             // Label.
        Username "J"                                        :S(LOVE)            // Pattern matching with a go to to label LOVE if success.
        Username "K"                                        :S(HATE) F(LOVE)    // Pattern matching with both success and failure go to.
MEH     OUTPUT = "Hi, " Username                            :(END)              // Both label, setting variable, automatic concatenation and go to.
        NameCount = 0                                                           // Setting an ordinary variable.
        NameCount = NameCount + 1                                               // Operator call.
        PersonalName LEN(1)                                 :S(AGAIN)           // Function call.
        word = 'gird'                                                           // Assignment statement.
        word 'i' = ou                                                           // Replacement statement resulting in word becoming 'gourd'.
         */

        // statement             => end_statement | assignment_statement | match_statement | replacement_statement | degenerate_statement
        // end_statement         => 'end' (blanks (label | 'end')?)? eos
        // assignment_statement  => label? subject equal object? goto? eos
        // match_statement       => label? subject pattern goto? eos
        // replacement_statement => label? subject pattern equal object? goto? eos
        // degenerate_statement  => label? subject? goto? eos
        bool result = false;
        if (Is("end"))
        {//Note: end statement.
            result = Maybe(SomeBlanks()
                        && Maybe(Maybe("end") || ALabel()))
                  && TheEndOfTheStatement();
        }
        else
        {
            ALabel();
            if (AGoto())
            {//Note: degenerate statement.
                 TheEndOfTheStatement();
            }
            else if (TheEndOfTheStatement())
            {//Note: degenerate statement.

            }
            else
            {//Note: could still be degenerate.
                var hasSubject = ASubject();
                if (Is("="))
                {//Note: assignment statement.
                    AnObject();
                    AGoto();
                    TheEndOfTheStatement();
                }
                else if (AGoto())
                {//Note: degenerate statement

                }
                else
                {
                    APattern();
                    if (Is("="))
                    {//Note: match statement.
                    }
                    else
                    {//Note: replacement statement.
                        var hasGoto = AGoto();
                        if (!hasGoto)
                        {
                            //error!!!!!!!!!!!!!!!!!!!!!!!!
                        }
                    }
                }
            }
        }
        return result;
    }

    bool ALabel()
    {
        // label => [a-zA-Z0-9] . -'end'
        throw new NotImplementedException();

        //void ALabel()
        //{
        //    //FixMe: this is wrong. Labels can be dynamically generated so can contain really weird characters.
        //    if (Match(@"[A-Z0-9]+", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant, out Match match))
        //        tokens.Add(match.Value);
        //    SkipWhitespace();
        //}
    }

    private bool ASubject()
    {
        throw new NotImplementedException();
    }

    private void APattern()
    {
        throw new NotImplementedException();
    }

    private void AnObject()
    {
        throw new NotImplementedException();
    }

    bool SomeBlanks()
    {
        //Ponder: maybe handle comments here?
        Match(@"\s*", RegexOptions.CultureInvariant);
        return true;
    }

    bool TheEndOfTheStatement()
    {
        throw new NotImplementedException();
    }

    bool Maybe(string v)
    {
        //FixMe: this is not a parser. I don't know what I was thinking.
        var fixMe = string.Empty;
        return String.Equals(v, fixMe, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Wraps parsers that are allowed to fail.
    /// </summary>
    /// <param name="_"></param>
    /// <returns></returns>
    bool Maybe(bool _) => true;

    bool Is(string v)
    {
        throw new NotImplementedException();
    }

    bool AGoto()
    {
        //FixMe: this is wrong. Labels can be dynamically generated so can contain really weird characters.

        var options = RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace | RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture | RegexOptions.Multiline;
        _index--;//Note: this is to correct for previous parsers skipping whitespace.
        //todo: look for pattern where : is preceded by non-whitespace and then throw error.
        if (Match(@"\s : \s* \( (?<label> \s* [A-Z0-9]+ \s* ) \)", options, out var labels, "label"))
        {//Note: Matches :(label)
            tokens.Add("Plain label: " + labels[0]);
        }
        else if (Match(@"\s : \s* S \s* \( (?<successLabel> \s* [A-Z0-9]+ \s* ) \) \s* F \s* \( (?<failLabel> \s* [A-Z0-9]+ \s* ) \)", options, out labels, "successLabel", "failLabel"))
        {//Note: Matches :S(label1) F(label2)
            tokens.Add("Success label: " + labels[0]);
            tokens.Add("Fail label: " + labels[1]);
        }
        else if (Match(@"\s : \s* F \s* \( (?<failLabel> \s* [A-Z0-9]+ \s* ) \) \s* S \s* \( (?<successLabel> \s* [A-Z0-9]+ \s* ) \)", options, out labels, "failLabel", "successLabel"))
        {//Note: Matches :F(label1) S(label2)
            tokens.Add("Fail label: " + labels[0]);
            tokens.Add("Success label: " + labels[1]);
        }
        else if (Match(@"\s : \s* S \s* \( (?<successLabel> \s* [A-Z0-9]+ \s* ) \)", options, out labels, "successLabel"))
        {//Note: Matches :S(label)
            tokens.Add("Success label: " + labels[0]);
        }
        else if (Match(@"\s : \s* F \s* \( (?<failLabel> \s* [A-Z0-9]+ \s* ) \)", options, out labels, "failLabel"))
        {//Note: Matches :F(label)
            tokens.Add("Fail label: " + labels[0]);
        }

        return true;
    }

    /// <summary>
    /// Moves the cursor to the first column of the next line if it exists. Returns false if it doesn't
    /// </summary>
    bool SkipToNextLine()
    {
        RegexOptions regexOptions = RegexOptions.CultureInvariant;
        Regex regex = new(Environment.NewLine, regexOptions);
        var match = regex.Match(_source, _index);
        _index = match.Index + match.Length;
        return match.Success;
    }


    bool Match(string pattern, RegexOptions regexOptions) => Match(pattern, regexOptions, out Match _);

    bool Match(string pattern, RegexOptions regexOptions, out Match match)
    {
        Regex regex = new(pattern, regexOptions);
        match = regex.Match(_source, _index);
        var isMatch = match.Success && match.Index == _index;
        if (isMatch)
            _index += match.Length;
        return isMatch;
    }

    bool Match(string pattern, RegexOptions regexOptions, out List<string> results, params string[] names)
    {
        results = new List<string>();
        Regex regex = new(pattern, regexOptions);
        var match = regex.Match(_source, _index);
        var isMatch = match.Success && match.Index == _index;
        if (isMatch)
        {
            var gs = match.Groups;
            for (int i = 1; i < gs.Count; i++)
            {
                var g = gs[i];
                if (g.Name == names[i - 1])
                    results.Add(g.Value);
                else throw new Exception("this is bug #1");
            }
            _index += match.Length;
            if (names.Length != results.Count)
                throw new Exception("This is bug #2");
        }
        return isMatch;
    }
}
