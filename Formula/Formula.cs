using System.Data;
using System.Text;
using System.Text.RegularExpressions;

namespace SpreadsheetUtilities;

/// <summary>
/// Represents formulas written in standard infix notation using standard precedence
/// rules.  The allowed symbols are non-negative numbers written using double-precision
/// floating-point syntax (without unary preceeding '-' or '+');
/// variables that consist of a letter or underscore followed by
/// zero or more letters, underscores, or digits; parentheses; and the four operator
/// symbols +, -, *, and /.
///
/// Spaces are significant only insofar that they delimit tokens.  For example, "xy" is
/// a single variable, "x y" consists of two variables "x" and y; "x23" is a single variable;
/// and "x 23" consists of a variable "x" and a number "23".
///
/// Associated with every formula are two delegates: a normalizer and a validator.  The
/// normalizer is used to convert variables into a canonical form. The validator is used to
/// add extra restrictions on the validity of a variable, beyond the base condition that
/// variables must always be legal: they must consist of a letter or underscore followed
/// by zero or more letters, underscores, or digits.
/// Their use is described in detail in the constructor and method comments.
/// </summary>
public class Formula
{
    // holds valid tokens for the expression
    private static readonly Regex validTokens = new Regex(@"^[\d\.eE()+\-*/a-zA-Z\s_]+$");
    private static readonly Regex validVarTokens = new Regex(@"^[a-zA-Z_][a-zA-Z0-9_]*$");
    private static readonly Regex validOperators = new Regex(@"^[\+\-\*/]$");
    private static readonly Regex validScientificNotation = new Regex(@"^[+]?\d+(\.\d*)?([eE][+]?\d+)?$");
    private readonly List<string> invariant;
    private readonly string invariantString = "";

    /// <summary>
    /// Creates a Formula from a string that consists of an infix expression written as
    /// described in the class comment.  If the expression is syntactically invalid,
    /// throws a FormulaFormatException with an explanatory Message.
    ///
    /// The associated normalizer is the identity function, and the associated validator
    /// maps every string to true.
    /// </summary>
    public Formula(string formula) :
        this(formula, s => s, s => true)
    {

    }

    /// <summary>
    /// Creates a Formula from a string that consists of an infix expression written as
    /// described in the class comment.  If the expression is syntactically incorrect,
    /// throws a FormulaFormatException with an explanatory Message.
    ///
    /// The associated normalizer and validator are the second and third parameters,
    /// respectively.
    ///
    /// If the formula contains a variable v such that normalize(v) is not a legal variable,
    /// throws a FormulaFormatException with an explanatory message.
    ///
    /// If the formula contains a variable v such that isValid(normalize(v)) is false,
    /// throws a FormulaFormatException with an explanatory message.
    ///
    /// Suppose that N is a method that converts all the letters in a string to upper case, and
    /// that V is a method that returns true only if a string consists of one letter followed
    /// by one digit.  Then:
    ///
    /// new Formula("x2+y3", N, V) should succeed
    /// new Formula("x+y3", N, V) should throw an exception, since V(N("x")) is false
    /// new Formula("2x+y3", N, V) should throw an exception, since "2x+y3" is syntactically incorrect.
    /// </summary>
    public Formula(string formula, Func<string, string> normalize, Func<string, bool> isValid)
    {
        bool prevTokenIsOp = false;
        int RParantheses = 0;
        int LParantheses = 0;
        invariant = new List<string>();
        foreach (string t in GetTokens(formula))
        {
            //Parsing
            if (!validTokens.IsMatch(t)) throw new FormulaFormatException("Invalid token detected. Check that all tokens are allowed as described by constructor header");
            //Number 
            else if (Double.TryParse(t, out double token) || validScientificNotation.IsMatch(t))
            {
                if (invariant.Count < 1 || prevTokenIsOp && invariant[invariant.Count - 1] != ")")
                {
                    invariant.Add(token.ToString());
                    prevTokenIsOp = false;
                }
                else throw new FormulaFormatException("Illegal expression detected. An opening parantheses, operator, or nothing must proceed number. Check operators");
            }
            //Opening parantheses
            else if (t == "(")
            {
                invariant.Add(t);
                LParantheses++;
                prevTokenIsOp = true;
            }
            //Closing parantheses
            else if (t == ")")
            {
                RParantheses++;
                if (RParantheses > LParantheses)
                {
                    throw new FormulaFormatException("Illegal expression detected. Number of closing parentheses is greater than the number of opening parentheses. Fix opening parantheses");
                }
                if (invariant.Count > 1 & !prevTokenIsOp)
                {
                    invariant.Add(t);
                    prevTokenIsOp = false;
                }
            }
            //Operator
            else if (validOperators.IsMatch(t))
            {
                if (!prevTokenIsOp & invariant.Count > 0)
                {
                    invariant.Add(t);
                    prevTokenIsOp = true;
                }
                else throw new FormulaFormatException("Illegal expression detected. Cannot start with operator, operator cannot proceed operator or parantheses. Try adding missing values");
            }

            //Variable
            else if (isLegalVar(t) & isValid(normalize(t)) & isLegalVar(normalize(t))) {
                if (invariant.Count < 1 || prevTokenIsOp && invariant[invariant.Count - 1] != ")")
                {
                    invariant.Add(normalize(t));
                    prevTokenIsOp = false;
                }
                else throw new FormulaFormatException("Illegal variable detected. Check that all variables are valid and validator outputs legal variables..");
            }
            string genFormula = "";
            foreach (string token in invariant)
            {
                genFormula = genFormula + token;
            }
            invariantString = genFormula;
        }
        //Balanced parantheses rule
        if (RParantheses != LParantheses) throw new FormulaFormatException("Unbalanced paranthesis. Check that there are equal closing/opening parantheses");
        if (invariant.Count < 1) throw new FormulaFormatException("Empty expression detected. Try again with expression");
        //Ending Token Rule
        if (validOperators.IsMatch(invariant[invariant.Count - 1]))
        {
            throw new FormulaFormatException("Expression does not end with value or closing parantheses. Check last token of expression");
        }
    }

    /// <summary>
    /// Evaluates this Formula, using the lookup delegate to determine the values of
    /// variables.  When a variable symbol v needs to be determined, it should be looked up
    /// via lookup(normalize(v)). (Here, normalize is the normalizer that was passed to
    /// the constructor.)
    ///
    /// For example, if L("x") is 2, L("X") is 4, and N is a method that converts all the letters
    /// in a string to upper case:
    ///
    /// new Formula("x+7", N, s => true).Evaluate(L) is 11
    /// new Formula("x+7").Evaluate(L) is 9
    ///
    /// Given a variable symbol as its parameter, lookup returns the variable's value
    /// (if it has one) or throws an ArgumentException (otherwise).
    ///
    /// If no undefined variables or divisions by zero are encountered when evaluating
    /// this Formula, the value is returned.  Otherwise, a FormulaError is returned.
    /// The Reason property of the FormulaError should have a meaningful explanation.
    ///
    /// This method should never throw an exception.
    /// </summary>
    public object Evaluate(Func<string, double> lookup)
    {
        //try evaluate. If divide by 0 occurs it is caught.
        try
        {
            //Stacks that hold values and operators from the expression
            Stack<double> valStack = new Stack<double>();
            Stack<string> opStack = new Stack<string>();

            foreach (string t in invariant)
            {
                //if t is a double
                if (Double.TryParse(t, out double valT))
                {
                    if (topContains(opStack, "*", "/"))
                    {
                        double valOther = valStack.Pop();
                        string op = opStack.Pop();
                        valStack.Push(Calculate(op, valOther, valT));
                    }
                    else
                        valStack.Push(valT);
                }

                //if t is equal to + or - 
                else if (t == "+" || t == "-")
                {
                    if (topContains(opStack, "+", "-"))
                    {
                        CalcAndPush(valStack, opStack);
                    }
                    opStack.Push(t);
                }
                //if t is * or /
                else if (t == "*" || t == "/")
                {
                    opStack.Push(t);
                }
                //if t is a left paranthesis "("
                else if (t == "(")
                {
                    opStack.Push(t);
                }
                //if t is a right paranthesis ")"
                else if (t == ")")
                {
                    if (topContains(opStack, "+", "-"))
                    {
                        CalcAndPush(valStack, opStack);
                    }

                    opStack.Pop();

                    if (topContains(opStack, "*", "/"))
                    {
                        CalcAndPush(valStack, opStack);
                    }
                }
                //t must be variable
                else
                {
                    valT = lookup(t);
                    if (topContains(opStack, "*", "/"))
                    {
                        double valOther = valStack.Pop();
                        string op = opStack.Pop();
                        valStack.Push(Calculate(op, valOther, valT));
                    }
                    else
                        valStack.Push(valT);
                }

            }

            //if Operator stack is empty then the equation is solved completely
            if (opStack.Count == 0) return valStack.Pop();

            //if operator stack is not empty then one more calculation must be made
            else
            {
                CalcAndPush(valStack, opStack);
                return valStack.Pop();
            }
        }
        //divide by zero caught here and FormulaError is thrown.
        catch(DivideByZeroException)
        {
            return new FormulaError("divide by zero");
        }
    }

    /// <summary>
    /// Enumerates the normalized versions of all of the variables that occur in this
    /// formula.  No normalization may appear more than once in the enumeration, even
    /// if it appears more than once in this Formula.
    ///
    /// For example, if N is a method that converts all the letters in a string to upper case:
    ///
    /// new Formula("x+y*z", N, s => true).GetVariables() should enumerate "X", "Y", and "Z"
    /// new Formula("x+X*z", N, s => true).GetVariables() should enumerate "X" and "Z".
    /// new Formula("x+X*z").GetVariables() should enumerate "x", "X", and "z".
    /// </summary>
    public IEnumerable<string> GetVariables()
    {
        List<string> variableList = new List<string>();
        foreach(string token in invariant)
        {
            if (validVarTokens.IsMatch(token) && !variableList.Contains(token))
            {
                yield return token;
                variableList.Add(token);
            }
        }
    }

    /// <summary>
    /// Returns a string containing no spaces which, if passed to the Formula
    /// constructor, will produce a Formula f such that this.Equals(f).  All of the
    /// variables in the string should be normalized.
    ///
    /// For example, if N is a method that converts all the letters in a string to upper case:
    ///
    /// new Formula("x + y", N, s => true).ToString() should return "X+Y"
    /// new Formula("x + Y").ToString() should return "x+Y"
    /// </summary>
    public override string ToString()
    {
        return invariantString;
    }

    /// <summary>
    /// If obj is null or obj is not a Formula, returns false.  Otherwise, reports
    /// whether or not this Formula and obj are equal.
    ///
    /// Two Formulae are considered equal if they consist of the same tokens in the
    /// same order.  To determine token equality, all tokens are compared as strings
    /// except for numeric tokens and variable tokens.
    /// Numeric tokens are considered equal if they are equal after being "normalized" by
    /// using C#'s standard conversion from string to double (and optionally back to a string).
    /// Variable tokens are considered equal if their normalized forms are equal, as
    /// defined by the provided normalizer.
    ///
    /// For example, if N is a method that converts all the letters in a string to upper case:
    ///
    /// new Formula("x1+y2", N, s => true).Equals(new Formula("X1  +  Y2")) is true
    /// new Formula("x1+y2").Equals(new Formula("X1+Y2")) is false
    /// new Formula("x1+y2").Equals(new Formula("y2+x1")) is false
    /// new Formula("2.0 + x7").Equals(new Formula("2.000 + x7")) is true
    /// </summary>
    public override bool Equals(object? obj)
    {
        if (obj != null && obj is Formula)
        {
            Formula otherFormula = (Formula)obj;
            return this.ToString() == otherFormula.ToString();     
        }  
        return false;
    }

    /// <summary>
    /// Reports whether f1 == f2, using the notion of equality from the Equals method.
    /// Note that f1 and f2 cannot be null, because their types are non-nullable
    /// </summary>
    public static bool operator ==(Formula f1, Formula f2)
    {
        return f1.Equals(f2);

    }

    /// <summary>
    /// Reports whether f1 != f2, using the notion of equality from the Equals method.
    /// Note that f1 and f2 cannot be null, because their types are non-nullable
    /// </summary>
    public static bool operator !=(Formula f1, Formula f2)
    {
        return !f1.Equals(f2);
    }

    /// <summary>
    /// Returns a hash code for this Formula.  If f1.Equals(f2), then it must be the
    /// case that f1.GetHashCode() == f2.GetHashCode().  Ideally, the probability that two
    /// randomly-generated unequal Formulae have the same hash code should be extremely small.
    /// </summary>
    public override int GetHashCode()
    {
        return this.ToString().GetHashCode();
    }

    /// <summary>
    /// Given an expression, enumerates the tokens that compose it.  Tokens are left paren;
    /// right paren; one of the four operator symbols; a legal variable token;
    /// a double literal; and anything that doesn't match one of those patterns.
    /// There are no empty tokens, and no token contains white space.
    /// </summary>
    private static IEnumerable<string> GetTokens(string formula)
    {
        // Patterns for individual tokens
        string lpPattern = @"\(";
        string rpPattern = @"\)";
        string opPattern = @"[\+\-*/]";
        string varPattern = @"[a-zA-Z_](?: [a-zA-Z_]|\d)*";
        string doublePattern = @"(?: \d+\.\d* | \d*\.\d+ | \d+ ) (?: [eE][\+-]?\d+)?";
        string spacePattern = @"\s+";

        // Overall pattern
        string pattern = string.Format("({0}) | ({1}) | ({2}) | ({3}) | ({4}) | ({5})",
                                        lpPattern, rpPattern, opPattern, varPattern, doublePattern, spacePattern);

        // Enumerate matching tokens that don't consist solely of white space.
        foreach (string s in Regex.Split(formula, pattern, RegexOptions.IgnorePatternWhitespace))
        {
            if (!Regex.IsMatch(s, @"^\s*$", RegexOptions.Singleline))
            {
                yield return s;
            }
        }

    }

    /// <summary>
    /// Private helper method that checks whether a given input is considered to be a legal variable meaning it consists of a letter or underscore
    /// followed by zero or more letters, underscores, or digits
    /// </summary>
    /// <param name="var"> The string to be checked. </param>
    /// <returns> Boolean val. True if var is legal variable </returns>
    private static bool isLegalVar(String var)
    {
        bool isLegalVar = validVarTokens.IsMatch(var);
        if (!isLegalVar) throw new FormulaFormatException("Illegal variable detected. Does not consist of a letter or underscore followed by zero or more letters, underscores, or digit. Check normailize function returns valid variable");
        return isLegalVar;
    }

    /// <summary>
    /// Private helper method that takes two integers and an operator and returns the result of the operator performed on the
    /// two integers.
    /// </summary>
    /// <param name="op"> Operator. </param>
    /// <param name="val1"> First value. </param>
    /// <param name="val2"> Second values. </param>
    /// <returns> Value of resolved expression </returns>
    /// <exception cref="ArgumentException"> Exception thrown if division by zero occurs. </exception>
    private static double Calculate(string op, double val1, double val2)
    {
        if (op == "*") return val1 * val2;

        if (op == "/")
        {
            if (val2 == 0) throw new DivideByZeroException();
            return val1 / val2;
        }
        if (op == "+") return val1 + val2;

        else return val1 - val2;
    }

    /// <summary>
    /// Private helper method that pops the value Stack twice and pops the operator stack once. The popped values are evaluated
    /// with the popped operator.
    /// </summary>
    /// <param name="valStack"> Stack containing values. </param>
    /// <param name="opStack"> Stack containing operators. </param>
    /// <exception cref="ArgumentException"> Exception thrown when stacks do not contain sufficient items. </exception>
    private static void CalcAndPush(Stack<double> valStack, Stack<string> opStack)
    {
        double val1 = valStack.Pop();
        double val2 = valStack.Pop();
        string op = opStack.Pop();

        valStack.Push(Calculate(op, val2, val1));
    }

    /// <summary>
    /// Private helper method that checks the top of a stack for a certian string.
    /// </summary>
    /// <param name="opStack"> The stack to be checked. </param>
    /// <param name="op1"> A string sought after. </param>
    /// <param name="op2"> A second string sought after.</param>
    /// <returns> Boolean value describing the existance of one of the strings sought after at the top of the stack. </returns>
    private static bool topContains(Stack<String> opStack, String op1, String op2)
    {
        if (opStack.Count != 0 && (opStack.Peek() == op1 || opStack.Peek() == op2))
            return true;
        return false;
    }
}

/// <summary>
/// Used to report syntactic errors in the argument to the Formula constructor.
/// </summary>
public class FormulaFormatException : Exception
{
    /// <summary>
    /// Constructs a FormulaFormatException containing the explanatory message.
    /// </summary>
    public FormulaFormatException(string message) : base(message)
    {
    }
}

/// <summary>
/// Used as a possible return value of the Formula.Evaluate method.
/// </summary>
public struct FormulaError
{
    /// <summary>
    /// Constructs a FormulaError containing the explanatory reason.
    /// </summary>
    /// <param name="reason"></param>
    public FormulaError(string reason) : this()
    {
        Reason = reason;
    }

    /// <summary>
    ///  The reason why this FormulaError was created.
    /// </summary>
    public string Reason { get; private set; }
}


