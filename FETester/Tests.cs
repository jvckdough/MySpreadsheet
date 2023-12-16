using FormulaEvaluator;

/// <summary>
/// This class contains tests for the class Evaluator found in the FormulaEvaluator project. It also contains several methods for testing
/// the functionality of a delegate found in the class Evaluator.
/// Author: Jack Doughty, uID 1330615
/// </summary>
public static class FETester
{
    //method that is of type Lookup
    static int VarIsOne(String s)
    {
        return 1;
    }

    //method that is of type Lookup
    static int VarIsTwo(String s)
    {
        return 2;
    }

    /// This main method contains tests for the method Evaluate of the project FormulaEvaluator. The outcomes of each test
    /// are printed on the terminal screen. The outputs should be compared to the expected values found in the comments
    /// above each test. The tests commented out will throw an illegal arguement as expected since these tests are meant to
    /// check that the method will detect illegal tokens or expressions.
    static void Main(string[] args)
    {
        //test 1 simple addition
        int result = Evaluator.Evaluate(" AAAAAAA123234 + 3 ", VarIsTwo);
        //5 expected
        Console.WriteLine("test 1: " + result);

        //test 2 simple subtraction
        int result2 = Evaluator.Evaluate(" 4- 2 ", VarIsOne);
        //2 expected
        Console.WriteLine("test 2: " + result2);

        //test 3 simple multiplication
        int result3 = Evaluator.Evaluate(" 5 * 3 ", VarIsOne);
        //15 expected
        Console.WriteLine("test 3: " + result3);

        //test 4 simple division
        int result4 = Evaluator.Evaluate(" 4 / 2 ", VarIsOne);
        //2 expected
        Console.WriteLine("test 4: " + result4);

        //test 5 simple variable
        int result5 = Evaluator.Evaluate("A1 * 2 ", VarIsOne);
        //2 expected
        Console.WriteLine("test 5: " + result5);

        //test 6 simple paranthesis
        int result6 = Evaluator.Evaluate("5+ (3*4) + 1 ", VarIsOne);
        //18 expected
        Console.WriteLine("test 6: " + result6);

        ////test 7 divide by zero
        try
        {
            int result7 = Evaluator.Evaluate("(10*8) / (7-7)", VarIsOne);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
        ////test 8 invalid expression
        try
        {
            int result7 = Evaluator.Evaluate(" (8* 9)/ ", VarIsOne);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }

        ////test 9 invalid expression 2
        try
        {
            int result9 = Evaluator.Evaluate(" *(8* 9) ", VarIsOne);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
        //test 10 large expression with variable
        int result10 = Evaluator.Evaluate("9/3 * (5+(8/zr3)) + 12", VarIsTwo);
        //39 expected
        Console.WriteLine("test 10: " + result10);

        //test 11 large expression with variables
        int result11 = Evaluator.Evaluate("((24/(6*2) +10))/(yyq7)+25", VarIsTwo);
        Console.WriteLine("test 11: " + result11);

        //test 12 large expression with paranthesis
        int result12 = Evaluator.Evaluate("3 + 4*(12+3)", VarIsTwo);
        //63 expected
        Console.WriteLine("test 12: " + result12);

        //Test 13 heavy multiplication
        int result13 = Evaluator.Evaluate("3*3*3/3 + B12 - 15", VarIsTwo);
        //-4 expected
        Console.WriteLine("test 13: " + result13);

        //Test 14 large expression with complex paranthesis
        int result14 = Evaluator.Evaluate("(((100*5/2)/(12+4*2))+20)", VarIsTwo);
        //32 expected
        Console.WriteLine("test 14: " + result14);

        //Test 15 missing paranthesis
        try
        {
            int result15 = Evaluator.Evaluate("(9+5)+2/10) + H12", VarIsOne);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }

        //Test 16 illegal amount of operators
        try
        {
            int result15 = Evaluator.Evaluate("10 + 8 */- 23", VarIsOne);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }

        //Test 17 no values
        try
        {
            int result16 = Evaluator.Evaluate("(())", VarIsOne);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }

        //Test 18 Illegal operator
        try
        {
            int result16 = Evaluator.Evaluate("5 $ 12 / 12", VarIsOne);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }

        //Test 19 Illegal operator
        try
        {
            int result16 = Evaluator.Evaluate("  5", VarIsOne);
            Console.WriteLine(result);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
        //Test 20 unary operator
        try
        {
            int result17 = Evaluator.Evaluate("  8*7/12+10-8*9+12*6+1000*(9-*2) ", VarIsOne);
            Console.WriteLine(result17);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
    }
}

