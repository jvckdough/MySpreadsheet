using System.Text.RegularExpressions;

namespace FormulaEvaluator;

/// <summary>
/// The static class Evaluator contains the method Evaluate and supporting helper methods. The purpose of this class is to
/// describe an evaluator method for solving infix expressions.
/// Author: Jack Doughty, uID 1330615
/// </summary>
public static class Evaluator
{
    public delegate int Lookup(String v);

    /// <summary>
    /// The method Evaluate evaluates arithmetic expressions using standard infix notation. It respects the usual precedence rules. 
    /// The evaluator supports expressions with variables whose values are looked up via a delegate.
    /// </summary>
    /// <param name="exp"> The expression to be resolved. </param>
    /// <param name="variableEvaluator"> A delegate for evaluating variables in the expression </param>
    /// <returns> A single int of the resolved expression. </returns>
    /// <exception cref="ArgumentException"> Excepetion thrown when the expression is invalid or contains invalid tokens. </exception>
    public static int Evaluate(String exp, Lookup variableEvaluator)
    {
        //Breaks expression into tokens and stored in an array
        string[] tokens = Regex.Split(exp.Trim(), "(\\()|(\\))|(-)|(\\+)|(\\*)|(/)");

        //Stacks that hold values and operators from the expression
        Stack<int> valStack = new Stack<int>();
        Stack<string> opStack = new Stack<string>();

        foreach (string t in tokens)
        {
            if (string.IsNullOrWhiteSpace(t)) continue;

            //if t is an integer or is variable
            if (int.TryParse(t, out int valT) || isLegalVar(t))
            {
                if (isLegalVar(t))
                {
                    valT = variableEvaluator(t);
                }
                if (topContains(opStack, "*", "/"))
                {
                    int valOther = valStack.Pop();
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

                //Top of operator Stack should be "(" check and pop
                if (opStack.Count == 0 || opStack.Peek() != "(") throw new ArgumentException("Token '(' is missing");

                opStack.Pop();

                if (topContains(opStack, "*", "/"))
                {
                    CalcAndPush(valStack, opStack);
                }
            }

            //if none of the conditionals were met, t is illegal token
            else throw new ArgumentException("Illegal token given in expression");

        }
        if (valStack.Count == 0 || opStack.Count > 1) throw new ArgumentException("Illegal expression given");

        //if Operator stack is empty then the equation is solved completely
        if (opStack.Count == 0) return valStack.Pop();
        
        //if operator stack is not empty then one more calculation must be made
        else
        {
            CalcAndPush(valStack, opStack);
            if (valStack.Count != 1) throw new ArgumentException("Illegal expression given");
            return valStack.Pop();
        }
    }
    /// <summary>
    /// Private helper method that checks whether a given input is considered to be a legal variable meaning it has the form of 1 or more
    /// characters followed by 1 or more digits.
    /// </summary>
    /// <param name="var"> The string to be checked. </param>
    /// <returns> Boolean val. True if var is legal variable </returns>
    private static bool isLegalVar(String var)
    {
        bool numFound = false;
        string varTrim = var.Trim();
        if (Char.IsLetter(varTrim[0]))
        {
            foreach (char s in varTrim)
            {
                if (numFound == true || char.IsWhiteSpace(s))
                {
                    if (!Char.IsDigit(s))
                    {
                        return false;
                    }
                }
                if (Char.IsDigit(s))
                {
                    numFound = true;
                }
            }
        }
        return numFound;
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

    /// <summary>
    /// Private helper method that pops the value Stack twice and pops the operator stack once. The popped values are evaluated
    /// with the popped operator.
    /// </summary>
    /// <param name="valStack"> Stack containing values. </param>
    /// <param name="opStack"> Stack containing operators. </param>
    /// <exception cref="ArgumentException"> Exception thrown when stacks do not contain sufficient items. </exception>
    private static void CalcAndPush(Stack<int> valStack, Stack<string> opStack)
    {
        //there should be at least 2 values in the value stack and 1 in the operator stack. If not, indicates that an invalid expression was given.
        if (valStack.Count < 2 || opStack.Count < 1) throw new ArgumentException("Illegal expression given");
        
        int val1 = valStack.Pop();
        int val2 = valStack.Pop();
        string op = opStack.Pop();

        valStack.Push(Calculate(op, val2, val1));
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
    private static int Calculate(string op, int val1, int val2)
    {
        if (op == "*") return val1 * val2;
        
        if (op == "/")
        {
            if (val2 == 0) throw new ArgumentException("Division by 0");
            return val1 / val2;
        }
        if (op == "+") return val1 + val2;
        
        if (op == "-") return val1 - val2;

        //if none of the above conditions were met then op is an illegal operator.
        else throw new ArgumentException("Illegal Operator");
    }

}

