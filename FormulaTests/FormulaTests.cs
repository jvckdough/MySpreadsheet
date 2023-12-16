using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using SpreadsheetUtilities;
using static SpreadsheetUtilities.Formula;
namespace FormulaTests;

[TestClass]
public class UnitTest1
{
    private string normalize(string var)
    {
        return "A1";
    }

    private bool isValid(string var)
    {
        return var.Equals("A1");
    }

    private bool badIsValid(string var)
    {
        return var.Equals("A 1");
    }

    //Tests for constructor
    [TestMethod]
    public void TestRegularExpressionsVar()
    {
       //Regular expression
        string varPattern = @"^[a-zA-Z_][a-zA-Z0-9_]*$";
        bool t = Regex.IsMatch("a4", varPattern);
        Assert.IsTrue(Regex.IsMatch("a4", varPattern));
        Assert.IsTrue(Regex.IsMatch("_4t234_23434ret", varPattern));
        Assert.IsFalse(Regex.IsMatch("4t234_23434ret", varPattern));
        Assert.IsTrue(Regex.IsMatch("w", varPattern));
    }

    [TestMethod()]
    public void TestWrongOrder()
    {
        Assert.IsFalse(new Formula("x1+y2").Equals(new Formula("y2+x1")));
    }

    [TestMethod()]
    public void TestGetVariables()
    {
        Formula formula = new Formula("X+y+Z");
        string list = "";
        foreach (string variable in formula.GetVariables())
        {
            list = list + variable;
        }
        Assert.IsTrue(list == "XyZ");
    }

    [TestMethod()]
    public void TestGetVariablesMultipleVars()
    {
        Formula formula = new Formula("X+ X- y+Z +y+X +z");
        string list = "";
        foreach (string variable in formula.GetVariables())
        {
            list = list + variable;
        }
        Assert.IsTrue(list == "XyZz");
    }

    [TestMethod()]
    public void TestGetVariablesSimilarVars()
    {
        Formula formula = new Formula("X+ 3+ x+  12 *( X -y+Z )+y+X +z *12/3 +z1");
        string list = "";
        foreach (string variable in formula.GetVariables())
        {
            list = list + variable;
        }
        Assert.IsTrue(list == "XxyZzz1");
    }

    [TestMethod()]
    public void TestDivideByZero()
    {
        Formula formula = new Formula("5/0");
        Assert.IsTrue(formula.Evaluate(s => 0) is FormulaError);
    }

    [TestMethod()]
    public void TestGetVariablesNoVars()
    {
        Formula formula = new Formula("15 +12e2 - 6          *(10E5)");
        string list = "";
        foreach (string variable in formula.GetVariables())
        {
            list = list + variable;
        }
        Assert.IsTrue(list == "");
    }

    [TestMethod]
    [ExpectedException(typeof(FormulaFormatException))]
    public void TestMoreOpeningParantheses()
    {
        Formula formula;
        formula = new Formula("(88+ _123 ", normalize, isValid);
        formula = new Formula("(12+ 12E1222))) ", normalize, isValid);
        formula = new Formula("12+ A3) ", normalize, isValid);
    }

    [TestMethod]
    [ExpectedException(typeof(FormulaFormatException))]
    public void TestStartClosingParantheses()
    {
        Formula formula;
        formula = new Formula(")88+ _123 ", normalize, isValid);
    }
    [TestMethod]
    [ExpectedException(typeof(FormulaFormatException))]
    public void TestMoreClosingParantheses()
    {
        Formula formula;
        formula = new Formula("(12+ 12E1222))) ", normalize, isValid);
        formula = new Formula("12+ A3) ", normalize, isValid);
    }

    [TestMethod]
    [ExpectedException(typeof(FormulaFormatException))]
    public void TestVariableAfterClosingParantheses()
    {
        Formula formula;
        formula = new Formula("12 + 5 - (8)_A12 ", normalize, isValid);
    }

    [TestMethod]
    [ExpectedException(typeof(FormulaFormatException))]
    public void TestVariableAfterVariable()
    {
        Formula formula;
        formula = new Formula("12 + 5 - _A12 b84", normalize, isValid);
    }

    [TestMethod]
    [ExpectedException(typeof(FormulaFormatException))]
    public void TestUnaryOperator()
    {
        Formula formula;
        formula = new Formula("-88+ _123 * 12 ", normalize, isValid);
    }

    [TestMethod]
    public void TestSingleCharVarOperator()
    {
        Formula formula;
        formula = new Formula("x+y3", normalize, isValid);
    }

    [TestMethod]
    [ExpectedException(typeof(FormulaFormatException))]
    public void TestDoubleOperator()
    {
        Formula formula;
        formula = new Formula("88+* _123 /* 12 ", normalize, isValid);
    }


    [TestMethod]
    [ExpectedException(typeof(FormulaFormatException))]
    public void TestIncorrectTokens()
    {
        Formula formula;
        formula = new Formula(" \\ E_12 $ /* 12 ", normalize, isValid);
    }

    [TestMethod]
    [ExpectedException(typeof(FormulaFormatException))]
    public void TestEmptyExpression()
    {
        Formula formula;
        formula = new Formula("     ", normalize, isValid);
    }


    [TestMethod]
    [ExpectedException(typeof(FormulaFormatException))]
    public void TestEndsWithOpening()
    {
        Formula formula;
        formula = new Formula("45 + i_12 -    (", normalize, isValid);
    }

    [TestMethod]
    [ExpectedException(typeof(FormulaFormatException))]
    public void TestInvalidScientificNotation()
    {
        Formula formula;
        formula = new Formula(".45E-12 + i_12 -    (", normalize, isValid);
    }


    [TestMethod]
    [ExpectedException(typeof(FormulaFormatException))]
    public void TestBadNormalize()
    {
        Formula formula;
        formula = new Formula("a12 + i_12 -  24  ", s => "a 5", badIsValid);
    }

    [TestMethod]
    [ExpectedException(typeof(FormulaFormatException))]
    public void TestBadVariable()
    {
        Formula formula;
        formula = new Formula("5a12 + i_12 -  24  ", normalize, isValid);
    }

    [TestMethod]
    [ExpectedException(typeof(FormulaFormatException))]
    public void TestDoubleVariable()
    {
        Formula formula;
        formula = new Formula("(5a12) i_12 -  24  ", normalize, isValid);
    }
    [TestMethod]
    public void TestGoodSyntaxExpressions()
    {
        Formula formula = new Formula("12.0 + (A3-8) / (a4)      ", normalize, isValid);
        Formula formula1 = new Formula("(12) + A3-8 / 16.1234 ", normalize, isValid);
        Formula formula2 = new Formula("(((12    -_12e) * 5 / 2) + 12.45e10 / 1) ", normalize, isValid);
        Formula formula3 = new Formula("1.23435E124  * ((12.00001 / rt_) / _22rfe__) + 1234.002 ", normalize, isValid);
        Formula formula4 = new Formula("(((a2)))", normalize, isValid);
        Formula formula5 = new Formula("(((a2)))", normalize, isValid);
        Formula formula6 = new Formula("e3", normalize, isValid);

    }

    //Tests for evaluate
    [TestMethod(), Timeout(5000)]
    [TestCategory("1")]
    public void TestSingleNumber()
    {
        Formula formula = new Formula("5", normalize, isValid);
        Assert.AreEqual(5.0, formula.Evaluate(s => 0));
    }

    [TestMethod(), Timeout(5000)]
    [TestCategory("2")]
    public void TestSingleVariable()
    {
        Formula formula = new Formula("X5", normalize, isValid);
        Assert.AreEqual(13.0, formula.Evaluate(s => 13));
    }

    [TestMethod(), Timeout(5000)]
    [TestCategory("3")]
    public void TestAddition()
    {
        Formula formula = new Formula("5+3", normalize, isValid);
        Assert.AreEqual(8.0, formula.Evaluate(s => 0));
    }

    [TestMethod(), Timeout(5000)]
    [TestCategory("4")]
    public void TestSubtraction()
    {
        Formula formula = new Formula("18-10", normalize, isValid);
        Assert.AreEqual(8.0, formula.Evaluate(s => 0));
    }

    [TestMethod(), Timeout(5000)]
    [TestCategory("5")]
    public void TestMultiplication()
    {
        Formula formula = new Formula("2*4", normalize, isValid);
        Assert.AreEqual(8.0, formula.Evaluate(s => 0));
    }

    [TestMethod(), Timeout(5000)]
    [TestCategory("6")]
    public void TestDivision()
    {
        Formula formula = new Formula("16/2", normalize, isValid);
        Assert.AreEqual(8.0, formula.Evaluate(s => 0));
    }

    [TestMethod(), Timeout(5000)]
    [TestCategory("7")]
    public void TestArithmeticWithVariable()
    {
        Formula formula = new Formula("2+X1", normalize, isValid);
        Assert.AreEqual(6.0, formula.Evaluate(s => 4));
    }

    [TestMethod(), Timeout(5000)]
    [TestCategory("8")]
    [ExpectedException(typeof(ArgumentException))]
    public void TestUnknownVariable()
    {
        Formula formula = new Formula("2+X1", normalize, isValid);
        Assert.AreEqual(8.0, formula.Evaluate(s => { throw new ArgumentException("Unknown variable"); })); 
    }

    [TestMethod(), Timeout(5000)]
    [TestCategory("9")]
    public void TestLeftToRight()
    {

        Formula formula = new Formula("2*6+3", normalize, isValid);
        Assert.AreEqual(15.0, formula.Evaluate(s => 0));
    }

    [TestMethod(), Timeout(5000)]
    [TestCategory("10")]
    public void TestOrderOperations()
    {
        Formula formula = new Formula("2+6*3", normalize, isValid);
        Assert.AreEqual(20.0, formula.Evaluate(s => 0));
    }

    [TestMethod(), Timeout(5000)]
    [TestCategory("11")]
    public void TestParenthesesTimes()
    {
        Formula formula = new Formula("(2+6)*3", normalize, isValid);
        Assert.AreEqual(24.0, formula.Evaluate(s => 40));
    }

    [TestMethod(), Timeout(5000)]
    [TestCategory("12")]
    public void TestTimesParentheses()
    {
        Formula formula = new Formula("2*(3+5)", normalize, isValid);
        Assert.AreEqual(16.0, formula.Evaluate(s => 0));
    }

    [TestMethod(), Timeout(5000)]
    [TestCategory("13")]
    public void TestPlusParentheses()
    {
        Formula formula = new Formula("2+(3+5)", normalize, isValid);
        Assert.AreEqual(10.0, formula.Evaluate(s => 0));
    }

    [TestMethod(), Timeout(5000)]
    [TestCategory("14")]
    public void TestPlusComplex()
    {
        Formula formula = new Formula("2-(3+5*9)", normalize, isValid);
        Assert.AreEqual(-46.0, formula.Evaluate(s => 0));
    }

    [TestMethod(), Timeout(5000)]
    [TestCategory("15")]
    public void TestOperatorAfterParens()
    {
        Formula formula = new Formula("(1*1)-2/2", normalize, isValid);
        Assert.AreEqual(0.0, formula.Evaluate(s => 0));
    }

    [TestMethod(), Timeout(5000)]
    [TestCategory("16")]
    public void TestComplexTimesParentheses()
    {
        Formula formula = new Formula("2+3*(3+5)", normalize, isValid);
        Assert.AreEqual(26.0, formula.Evaluate(s => 0));
    }

    [TestMethod(), Timeout(5000)]
    [TestCategory("17")]
    public void TestComplexAndParentheses()
    {
        Formula formula = new Formula("2+3*5+(3+4*8)*5+2", normalize, isValid);
        Assert.AreEqual(194.0, formula.Evaluate(s => 0));
    }


    [TestMethod(), Timeout(5000)]
    [TestCategory("19")]
    [ExpectedException(typeof(FormulaFormatException))]
    public void TestSingleOperator()
    {
        Formula formula = new Formula("+", normalize, isValid);
        formula.Evaluate(s => 0);
    }

    [TestMethod(), Timeout(5000)]
    [TestCategory("20")]
    [ExpectedException(typeof(FormulaFormatException))]
    public void TestExtraOperator()
    {
        Formula formula = new Formula("2+5+", normalize, isValid);

        formula.Evaluate(s => 0);
    }

    [TestMethod(), Timeout(5000)]
    [TestCategory("21")]
    [ExpectedException(typeof(FormulaFormatException))]
    public void TestExtraParentheses()
    {
        Formula formula = new Formula("2+5*7*", normalize, isValid);
        formula.Evaluate(s => 0);
    }

    [TestMethod(), Timeout(5000)]
    [TestCategory("22")]
    [ExpectedException(typeof(FormulaFormatException))]
    public void TestInvalidVariable()
    {
        Formula formula = new Formula("x-", normalize, isValid);
        formula.Evaluate(s => 0);
    }

    [TestMethod(), Timeout(5000)]
    [TestCategory("23")]
    [ExpectedException(typeof(FormulaFormatException))]
    public void TestPlusInvalidVariable()
    {
        Formula formula = new Formula("5+x-", normalize, isValid);
        formula.Evaluate(s => 0);
    }

    [TestMethod(), Timeout(5000)]
    [TestCategory("24")]
    [ExpectedException(typeof(FormulaFormatException))]
    public void TestParensNoOperator()
    {
        Formula formula = new Formula("5.0+7.5+(5.5)8", normalize, isValid);
        formula.Evaluate(s => 0);
    }


    [TestMethod(), Timeout(5000)]
    [TestCategory("25")]
    [ExpectedException(typeof(FormulaFormatException))]
    public void TestEmpty()
    {
        Formula formula = new Formula("", normalize, isValid);
        formula.Evaluate(s => 0);
    }

    [TestMethod(), Timeout(5000)]
    [TestCategory("26")]
    public void TestComplexMultiVar()
    {
        Formula formula = new Formula("(10E3 +v_1234 *(12.5+ 0.5 - 1.1)) /10.0  ", normalize, isValid);
        Assert.AreEqual((10E3 + 1 * (12.5 + 0.5 - 1.1)) / 10.0, formula.Evaluate(s => 1));
    }

    [TestMethod(), Timeout(5000)]
    [TestCategory("27")]
    public void TestComplexNestedParensRight()
    {
        Formula formula = new Formula("x1+(x2+(x3+(x4+(x5+x6))))", normalize, isValid);
        Assert.AreEqual(6.0, formula.Evaluate(s => 1));
    }

    [TestMethod(), Timeout(5000)]
    [TestCategory("28")]
    public void TestComplexNestedParensLeft()
    {
        Formula formula = new Formula("10E1 + ((((x1 + x2) + x3* 2.345) + x4) + x5) + x6 +10e1", normalize, isValid);
        Assert.AreEqual(214.69000, formula.Evaluate(s => 2));
    }

    [TestMethod(), Timeout(5000)]
    [TestCategory("29")]
    public void TestRepeatedVar()
    {
        Formula formula = new Formula("a4-a4*a4/a4", normalize, isValid);

        Assert.AreEqual(0.0, formula.Evaluate(s => 3));
    }

    [TestMethod()]
    [TestCategory("30")]
    public void TestAreEqual()
    {
        Assert.IsTrue(new Formula("x1+y2 + 10e100", normalize, s => true).Equals(new Formula("A1  +  A1 + 10E100")));
    }

    [TestMethod()]
    [TestCategory("31")]
    public void TestAreEqualSpaceyExpressions()
    {
        Assert.IsTrue(new Formula("12+      8/ ( 9  -2)", normalize, s => true).Equals(new Formula("   12 + 8 /(9-2    )      ")));
    }

    [TestMethod()]
    public void TestFalseDifferentVariableSyntax()
    {
        Assert.IsFalse(new Formula("x1+y2").Equals(new Formula("X1+Y2")));
    }
    [TestMethod()]
    public void TestGetHashCode()
    {
        Assert.AreEqual(new Formula(" 57/ 12 -     x1 +y2 -(8* 3)").GetHashCode(), new Formula("  57/     12 -x1 +    y2-(8   * 3)").GetHashCode());
    }
    [TestMethod()]
    public void TestAreEqualFalse()
    {
        Assert.IsFalse(new Formula("x5 -1").Equals(new List<string>()));
    }
    [TestMethod()]
    public void TestAreEqualOverideFalse()
    {
        Assert.IsFalse(new Formula("x5 -1") == new Formula("y5 -1"));
    }
    [TestMethod()]
    public void TestAreEqualOverideTrue()
    {
        Assert.IsTrue(new Formula("x5 -1") == new Formula("   x5 - 1    "));
    }
    [TestMethod()]
    public void TestAreEqualOverideFalse2()
    {
        Assert.IsFalse(new Formula("x5 -1") != new Formula("   x5 - 1    "));
    }
    [TestMethod()]
    public void TestAreEqualFalseNull()
    {
        Assert.IsFalse(new Formula("x5 -1").Equals(null));
    }
}
