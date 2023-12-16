using SpreadsheetUtilities;

namespace SS;

/// <summary>
/// This is a tester class for the class Spreadsheet. Tests provide 100% code coverage, checking that each method has the
/// expected behavior. In many test cases, exceptions are thrown to ensure that no incorrect input will be given by the
/// user. 
/// </summary>
[TestClass]
public class SpreadSheetTestts
{
    private string toUpperNormalize(string var)
    {
        return var.ToUpper();
    }

    private string badNormalize(string var)
    {
        return "101";
    }

    private bool isValidA(string var)
    {
        return var[0] == 'A';
    }

    private bool isValid(string var)
    {
        return true;
    }
 
    [TestMethod]
    public void SetContentsOfCellDouble()
    {
        AbstractSpreadsheet sheet = new Spreadsheet( s => true, s => s, "version 1");
        sheet.SetContentsOfCell("D1", "=110+30");
        sheet.SetContentsOfCell("F1", "=100/10");
        sheet.SetContentsOfCell("B2", "=F1");
        sheet.SetContentsOfCell("A4", "=D1");
        sheet.SetContentsOfCell("A1", "=B2+A4+D1");
        sheet.SetContentsOfCell("A2", "=A1 + 64/8");
        Assert.AreEqual(sheet.GetCellValue("A2"), 298.0);
        sheet.SetContentsOfCell("D1", "120");
        Assert.AreEqual(sheet.GetCellValue("A2"), 258.0);
    }
    [TestMethod]
    [ExpectedException(typeof(FormulaFormatException))]
    public void SetContentsOfCellString()
    {
        AbstractSpreadsheet sheet3 = new Spreadsheet( s => true, s => s, "version 1");
        sheet3.SetContentsOfCell("D1", "=110+30");
        sheet3.SetContentsOfCell("F1", "=100/10");
        sheet3.SetContentsOfCell("B2", "=F1");
        sheet3.SetContentsOfCell("A4", "=D1");
        sheet3.SetContentsOfCell("A1", "=B2+A4+D1");
        sheet3.SetContentsOfCell("A2", "=A1 + 64/8");
        Assert.AreEqual(sheet3.GetCellValue("A2"), 298.0);
        sheet3.SetContentsOfCell("D1", "tom foolery");
        Assert.AreEqual(sheet3.GetCellValue("A2"), 258.0);
    }
    [TestMethod]
    public void SetContentsOfCellFormula()
    {
        AbstractSpreadsheet sheet3 = new Spreadsheet( s => true, s => s, "version 1");
        sheet3.SetContentsOfCell("D1", "=110+30");
        sheet3.SetContentsOfCell("F1", "=100/10");
        sheet3.SetContentsOfCell("B2", "=F1");
        sheet3.SetContentsOfCell("A4", "=D1");
        sheet3.SetContentsOfCell("A1", "=B2+A4+D1");
        sheet3.SetContentsOfCell("A2", "=A1 + 64/8");
        Assert.AreEqual(sheet3.GetCellValue("A2"), 298.0);
        sheet3.SetContentsOfCell("H3", "101");
        sheet3.SetContentsOfCell("D1", "=F1+H3 -100");
        Assert.AreEqual(sheet3.GetCellValue("A2"), 40.0);
    }

    [TestMethod]
    public void SetContentsOfCellRemoveDependencies()
    {
        AbstractSpreadsheet sheet3 = new Spreadsheet( s => true, s => s, "version 1");
        sheet3.SetContentsOfCell("D1", "=110+30");
        sheet3.SetContentsOfCell("F1", "=100/10");
        sheet3.SetContentsOfCell("B2", "=F1");
        sheet3.SetContentsOfCell("A4", "=D1");
        sheet3.SetContentsOfCell("A1", "=B2+A4+D1");
        sheet3.SetContentsOfCell("A2", "=A1 + 64/8");
        sheet3.SetContentsOfCell("B2", "=11");
        Assert.AreEqual(sheet3.GetCellValue("A2"), 299.0);
        sheet3.SetContentsOfCell("H3", "101");
        sheet3.SetContentsOfCell("D1", "=F1+H3 -100");
        Assert.AreEqual(sheet3.GetCellValue("A2"), 41.0);
    }

    [TestMethod]
    public void SetContentsOfCellNormalizeValidator()
    {
        AbstractSpreadsheet sheet3 = new Spreadsheet( isValidA, toUpperNormalize, "version 1");
        sheet3.SetContentsOfCell("A1", "100");
        sheet3.SetContentsOfCell("A_512", "100");
        sheet3.SetContentsOfCell("A1_", "=A1+A_512");
        Assert.AreEqual(sheet3.GetCellValue("a1_"), 200.0);
    }

    [TestMethod]
    [ExpectedException(typeof(InvalidNameException))]
    public void SetContentsOfCellNormalizeValidatorCatch()
    {
        AbstractSpreadsheet sheet3 = new Spreadsheet( isValidA, toUpperNormalize, "version 1");
        sheet3.SetContentsOfCell("A1", "100");
        sheet3.SetContentsOfCell("A_512", "100");
        sheet3.SetContentsOfCell("b1_", "=A1+A_512");
        Assert.AreEqual(sheet3.GetCellValue("b1_"), 200.0);
    }

    [TestMethod]
    [ExpectedException(typeof(InvalidNameException))]
    public void SetContentsOfCellBadNormalizeValidator()
    {
        AbstractSpreadsheet sheet3 = new Spreadsheet( isValidA, badNormalize, "version 1");
        sheet3.SetContentsOfCell("A1", "100");
        Assert.AreEqual(sheet3.GetCellValue("b1_"), 200.0);
    }
    [TestMethod]
    public void SpreadSheetSetAndGetDouble()
    {
        AbstractSpreadsheet sheet = new Spreadsheet( isValid, toUpperNormalize, "version 1");
        sheet.SetContentsOfCell("x1", "5.0");
        Assert.AreEqual(sheet.GetCellContents("x1"), 5.0);
    }

    [TestMethod]
    public void SpreadSheetSetAndGetString()
    {
        AbstractSpreadsheet sheet = new Spreadsheet( isValid, toUpperNormalize, "version 1");
        sheet.SetContentsOfCell("x1", "hello:");
        sheet.SetContentsOfCell("x1", "goodbye");
        sheet.SetContentsOfCell("___", "55");
        sheet.SetContentsOfCell("___", ("x5+234 - s12"));
        sheet.SetContentsOfCell("___", "good morning");
        Assert.AreEqual(sheet.GetCellContents("x1"), "goodbye");
        Assert.AreEqual(sheet.GetCellContents("___"), "good morning");
    }
    [TestMethod]
    public void SpreadSheetSetAndGetFormula()
    {
        AbstractSpreadsheet sheet = new Spreadsheet( isValid, toUpperNormalize, "version 1");
        sheet.SetContentsOfCell("X5", "=100");
        sheet.SetContentsOfCell("S12", "=100");
        sheet.SetContentsOfCell("___", "=x5+234");
        sheet.SetContentsOfCell("___", "=x5+234 - s12");
        Assert.AreEqual(sheet.GetCellContents("___"), new Formula("X5+234 - S12"));
    }
    [TestMethod]
    public void SpreadSheetSetAndGetEmpty()
    {
        AbstractSpreadsheet sheet = new Spreadsheet( isValid, toUpperNormalize, "version 1");
        sheet.SetContentsOfCell("x1", "5.0");
        Assert.AreEqual(sheet.GetCellContents("x2"), "");
        Assert.AreEqual(sheet.GetCellContents("___3234"), "");
        Assert.AreEqual(sheet.GetCellContents("X1"), 5.0);
        Assert.AreEqual(sheet.GetCellContents("xP3"), "");
    }
    [TestMethod]
    [ExpectedException(typeof(InvalidNameException))]
    public void SpreadSheetGetInvalid()
    {
        AbstractSpreadsheet sheet = new Spreadsheet( isValid, toUpperNormalize, "version 1");
        sheet.SetContentsOfCell("x1", "5.0");
        Assert.AreEqual(sheet.GetCellContents("31"), 5.0);
    }
    [TestMethod]
    [ExpectedException(typeof(InvalidNameException))]
    public void SpreadSheetSetInvalid()
    {
        AbstractSpreadsheet sheet = new Spreadsheet( isValid, toUpperNormalize, "version 1");
        sheet.SetContentsOfCell("&", "5.0");
        Assert.IsFalse(sheet.GetCellContents("&").Equals(5.0));
    }
    [TestMethod]
    public void GetNamesOfAllNonEmptyCells()
    {
        List<string> list = new List<string>();
        AbstractSpreadsheet sheet = new Spreadsheet( isValid, toUpperNormalize, "version 1");
        sheet.SetContentsOfCell("x1", "5.0");
        sheet.SetContentsOfCell("x1", "6.0");
        sheet.SetContentsOfCell("___23", "111.0");
        sheet.SetContentsOfCell("xxx111", "Pineapple pizza");
        sheet.SetContentsOfCell("slam132_12", "=45 + 34");
        sheet.SetContentsOfCell("slam132_12", "345.134");

        foreach (string cell in sheet.GetNamesOfAllNonemptyCells())
        {
            list.Add(cell);
        }
        Assert.AreEqual(4, list.Count);
    }
    [TestMethod]
    [ExpectedException(typeof(CircularException))]
    public void SpreadSheetCircularException()
    {
        AbstractSpreadsheet sheet = new Spreadsheet( isValid, toUpperNormalize, "version 1");
        sheet.SetContentsOfCell("A1", "4.0");
        sheet.SetContentsOfCell("B1", ("1"));
        sheet.SetContentsOfCell("B2", ("1"));
        sheet.SetContentsOfCell("A4", ("1"));
        sheet.SetContentsOfCell("D6", ("1"));
        sheet.SetContentsOfCell("B1", ("=A1+5 + D6"));
        sheet.SetContentsOfCell("B2", ("=12+A1+D6"));
        sheet.SetContentsOfCell("A4", ("=B1 + A1 + D6 - 10"));
        sheet.SetContentsOfCell("D6", ("=B2+4"));
    }
    [TestMethod]
    [ExpectedException(typeof(CircularException))]
    public void SpreadSheetCircularExceptionEdge()
    {
        AbstractSpreadsheet sheet = new Spreadsheet( isValid, toUpperNormalize, "version 1");
        sheet.SetContentsOfCell("B1", ("2"));
        sheet.SetContentsOfCell("c1", ("2"));
        sheet.SetContentsOfCell("A1", ("=B1*2"));
        sheet.SetContentsOfCell("B1", ("=C1*2"));
        sheet.SetContentsOfCell("C1", ("=A1*2"));
    }
    [TestMethod]
    [ExpectedException(typeof(CircularException))]
    public void SpreadSheetCircularExceptionCellinCell()
    {
        AbstractSpreadsheet sheet = new Spreadsheet( isValid, toUpperNormalize, "version 1");
        sheet.SetContentsOfCell("A1", "4.0");
        sheet.SetContentsOfCell("d6", "4.0");
        sheet.SetContentsOfCell("c5", "4.0");
        sheet.SetContentsOfCell("B1", ("=A1+5 + D6"));
        sheet.SetContentsOfCell("B2", ("=12+C5+D6"));
        sheet.SetContentsOfCell("A4", ("=B1 + A1 + D6 - 10"));
        sheet.SetContentsOfCell("D6", ("=D6+4"));
    }
    [TestMethod]
    public void SpreadSheetSetDoubleMultiple()
    {
        AbstractSpreadsheet sheet = new Spreadsheet( isValid, toUpperNormalize, "version 1");
        sheet.SetContentsOfCell("A1", ("2"));
        sheet.SetContentsOfCell("B1", ("=A1*2"));
        sheet.SetContentsOfCell("C1", ("=B1+A1"));
        Assert.IsTrue(sheet.SetContentsOfCell("A1", "5.0").Count == 3);
    }
    [TestMethod]
    [ExpectedException(typeof(FormulaFormatException))]
    public void SpreadSheetSetStringMultiple()
    {
        AbstractSpreadsheet sheet = new Spreadsheet( isValid, toUpperNormalize, "version 1");
        sheet.SetContentsOfCell("A1", ("1"));
        sheet.SetContentsOfCell("B1", ("=A1*2"));
        sheet.SetContentsOfCell("C1", ("=B1+A1"));
        Assert.IsTrue(sheet.SetContentsOfCell("A1", "").Count == 3);
    }
    [TestMethod]
    [ExpectedException(typeof(InvalidNameException))]
    public void SpreadSheetSetContentsOfCellStringInvalid()
    {
        AbstractSpreadsheet sheet = new Spreadsheet( isValid, toUpperNormalize, "version 1");
        sheet.SetContentsOfCell("", "hello");
    }
    [TestMethod]
    [ExpectedException(typeof(InvalidNameException))]
    public void SpreadSheetSetContentsOfCellStringInvalid2()
    {
        AbstractSpreadsheet sheet = new Spreadsheet( isValid, toUpperNormalize, "version 1");
        sheet.SetContentsOfCell(" ", "hello");
    }
    [TestMethod]
    [ExpectedException(typeof(InvalidNameException))]
    public void SpreadSheetSetContentsOfCellStringInvalid3()
    {
        AbstractSpreadsheet sheet = new Spreadsheet( isValid, toUpperNormalize, "version 1");
        sheet.SetContentsOfCell("12x", ("=x12+45"));
    }
    [TestMethod]
    public void SpreadSheetGetValue()
    {
        AbstractSpreadsheet sheet = new Spreadsheet( isValid, toUpperNormalize, "version 1");
        sheet.SetContentsOfCell("_x12", ("=12+45"));
    }
    [TestMethod]
    [ExpectedException(typeof(FormulaFormatException))]
    public void SpreadSheetSetContentsOfCellFormulaFormat()
    {
        AbstractSpreadsheet sheet = new Spreadsheet( isValid, toUpperNormalize, "version 1");
        sheet.SetContentsOfCell("x1", ("=12z+45"));
    }
    [TestMethod]
    public void SpreadSheetSetContentsOfCellFormulaError()
    {
        AbstractSpreadsheet sheet = new Spreadsheet( isValid, toUpperNormalize, "version 1");
        sheet.SetContentsOfCell("X", ("=45/0"));
        Assert.IsTrue(sheet.GetCellValue("x") is FormulaError);
    }
    [TestMethod]
    public void SpreadSheetSetContentsOfCellVarToFormulaError()
    {
        AbstractSpreadsheet sheet = new Spreadsheet( isValid, toUpperNormalize, "version 1");
        sheet.SetContentsOfCell("x", ("0"));
        sheet.SetContentsOfCell("A", ("=12/x"));
        Assert.IsTrue(sheet.GetCellValue("A") is FormulaError);
    }
    [TestMethod]
    public void SpreadSheetSave()
    {
        AbstractSpreadsheet sheet = new Spreadsheet();
        sheet.SetContentsOfCell("A1", "5");
        sheet.SetContentsOfCell("B3", "=A1+2");
        sheet.Save("saveFile");
    }
    [TestMethod]
    public void SpreadSheetSaveEmpty()
    {
        AbstractSpreadsheet sheet = new Spreadsheet();
        sheet.Save("saveFile");
    }
    [TestMethod]
    public void SpreadSheetSaveStress()
    {
        AbstractSpreadsheet sheet = new Spreadsheet();
        sheet.SetContentsOfCell("A1", "5");
        sheet.SetContentsOfCell("B3", "=A1+2");
        sheet.SetContentsOfCell("C1", "Hello world");
        sheet.SetContentsOfCell("D1", "Hello cell D!");
        sheet.SetContentsOfCell("F1", "B3 +100/0");
        sheet.SetContentsOfCell("D1", "Goodbye cell D.");
        sheet.Save("saveFile");      
    }
    [TestMethod]
    public void SpreadSheetDeserialize()
    {
        AbstractSpreadsheet sheet = new Spreadsheet();
        sheet.SetContentsOfCell("A1", "5");
        sheet.SetContentsOfCell("B3", "=A1+2");
        sheet.SetContentsOfCell("C12", "Lebron");
        sheet.Save("saveFile");
        Spreadsheet deserialized = new Spreadsheet("saveFile", isValid, toUpperNormalize, "default");
    }
    [TestMethod]
    [ExpectedException(typeof(FileNotFoundException))]
    public void SpreadSheetDeserializeBadPath()
    {
        AbstractSpreadsheet sheet = new Spreadsheet();
        sheet.SetContentsOfCell("A1", "5");
        sheet.SetContentsOfCell("B3", "=A1+2");
        sheet.SetContentsOfCell("C12", "Lebron");
        sheet.Save("saveFile");
        Spreadsheet deserialized = new Spreadsheet("saveFile!", isValid, toUpperNormalize, "default");
    }
    [TestMethod]
    [ExpectedException(typeof(SpreadsheetReadWriteException))]
    public void SpreadSheetDeserializeWrongVersion()
    {
        AbstractSpreadsheet sheet = new Spreadsheet();
        sheet.SetContentsOfCell("A1", "5");
        sheet.SetContentsOfCell("B3", "=A1+2");
        sheet.SetContentsOfCell("C12", "Lebron");
        sheet.Save("saveFile");
        Spreadsheet deserialized = new Spreadsheet("saveFile", isValid, toUpperNormalize, "version 1.1");
    }
    [TestMethod]
    public void SpreadSheetDeserializeEmpty()
    {
        AbstractSpreadsheet sheet = new Spreadsheet(isValid, toUpperNormalize, "version1");
        sheet.SetContentsOfCell("A1", "5.0");
        sheet.Save("saveFile2");
        Spreadsheet deserialized = new Spreadsheet("saveFile2", isValid, toUpperNormalize, "version1");
    }
    [TestMethod]
    [ExpectedException(typeof(InvalidNameException))]
    public void SpreadSheetDeserializeBadNames()
    {
        AbstractSpreadsheet sheet = new Spreadsheet(isValid, toUpperNormalize, "version1");
        sheet.SetContentsOfCell("A1", "5.0");
        sheet.SetContentsOfCell("B1", "5.0");
        sheet.Save("saveFile2");
        Spreadsheet deserialized = new Spreadsheet("saveFile2", isValidA, toUpperNormalize, "version1");
    }
}
