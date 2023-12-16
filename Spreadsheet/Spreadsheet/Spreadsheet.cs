using Microsoft.VisualBasic;
using SpreadsheetUtilities;
using System.Net.Mime;
using System.Text.RegularExpressions;
using System.Text.Json;
using System;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Xml;
using System.Xml.Linq;
using System.Collections.Immutable;
using System.Text.Json.Nodes;

namespace SS
{
    // Written by Profs Zachary, Kopta and Martin for CS 3500
    // Last updated: August 2023 (small tweak to API)

    //Filled in by: Joel Ronca
    // September 28th, 2023

    /// <summary>
    /// An AbstractSpreadsheet object represents the state of a simple spreadsheet.  A 
    /// spreadsheet consists of an infinite number of named cells.
    /// 
    /// A string is a valid cell name if and only if:
    ///   (1) its first character is an underscore or a letter
    ///   (2) its remaining characters (if any) are underscores and/or letters and/or digits
    /// Note that this is the same as the definition of valid variable from the PS3 Formula class.
    /// 
    /// For example, "x", "_", "x2", "y_15", and "___" are all valid cell  names, but
    /// "25", "2x", and "&" are not.  Cell names are case sensitive, so "x" and "X" are
    /// different cell names.
    /// 
    /// A spreadsheet contains a cell corresponding to every possible cell name.  (This
    /// means that a spreadsheet contains an infinite number of cells.)  In addition to 
    /// a name, each cell has a contents and a value.  The distinction is important.
    /// 
    /// The contents of a cell can be (1) a string, (2) a double, or (3) a Formula.  If the
    /// contents is an empty string, we say that the cell is empty.  (By analogy, the contents
    /// of a cell in Excel is what is displayed on the editing line when the cell is selected).
    /// 
    /// In a new spreadsheet, the contents of every cell is the empty string.
    ///  
    /// We are not concerned with values in PS4, but to give context for the future of the project,
    /// the value of a cell can be (1) a string, (2) a double, or (3) a FormulaError.  
    /// (By analogy, the value of an Excel cell is what is displayed in that cell's position
    /// in the grid). 
    /// 
    /// If a cell's contents is a string, its value is that string.
    /// 
    /// If a cell's contents is a double, its value is that double.
    /// 
    /// If a cell's contents is a Formula, its value is either a double or a FormulaError,
    /// as reported by the Evaluate method of the Formula class.  The value of a Formula,
    /// of course, can depend on the values of variables.  The value of a variable is the 
    /// value of the spreadsheet cell it names (if that cell's value is a double) or 
    /// is undefined (otherwise).
    /// 
    /// Spreadsheets are never allowed to contain a combination of Formulas that establish
    /// a circular dependency.  A circular dependency exists when a cell depends on itself.
    /// For example, suppose that A1 contains B1*2, B1 contains C1*2, and C1 contains A1*2.
    /// A1 depends on B1, which depends on C1, which depends on A1.  That's a circular
    /// dependency.
    /// </summary>

    public class Spreadsheet : AbstractSpreadsheet
    {
        [JsonInclude]
        //Dictionary of cells  in use and their current values
        public Dictionary<string, Cell> Cells { get; protected set; }

        //graph to track dependencies in spreadsheet
        private DependencyGraph graph;

        //Validator for formulas
        private Func<string, bool> isValid;

        //Normalizer for formulas
        private Func<string, string> normalize;


        /// <summary>
        /// Default constructor for SpreadSheet
        /// </summary>
        public Spreadsheet() : base("default")
        {
            this.Cells = new Dictionary<string, Cell>();
            this.graph = new DependencyGraph();
            this.Changed = false;
            this.isValid = s => true;
            this.normalize = s => s;
        }
        /// <summary>
        /// SpreadSheet constructor that takes a custom Validator, a custom normalizer 
        /// and the spreadsheet version as parameters.
        /// </summary>
        /// <param name="isValid"></param>
        /// <param name="normalize"></param>
        /// <param name="version"></param>
        public Spreadsheet(Func<string, bool> isValid, Func<string, string> normalize, string version)
            : base(version)
        {
            this.Cells = new Dictionary<string, Cell>();
            this.graph = new DependencyGraph();
            this.Changed = false;
            this.isValid = isValid;
            this.normalize = normalize;

        }

        /// <summary>
        /// Constructor for when deserializing a file with information from a previous
        /// Spreadsheet.
        /// </summary>
        /// <param name="filepath"></param>
        /// <param name="isValid"></param>
        /// <param name="normalize"></param>
        /// <param name="version"></param>
        /// <exception cref="SpreadsheetReadWriteException"></exception>
        public Spreadsheet(string filepath, Func<string, bool> isValid, Func<string, string> normalize, string version)
            : base(version)
        {
            this.Cells = new Dictionary<string, Cell>();
            this.graph = new DependencyGraph();
            this.Changed = false;
            this.isValid = isValid;
            this.normalize = normalize;

            try
            {
                //Read the file
                string data = File.ReadAllText(filepath);

                //Deserialize the file
                Spreadsheet? spreadsheetFile = JsonSerializer.Deserialize<Spreadsheet>(data);

                //Check that file is not null
                if (spreadsheetFile != null)
                {
                    //If the versions do not match, throw an exception
                    if (!spreadsheetFile.Version.Equals(version))
                    {
                        throw new SpreadsheetReadWriteException("The version given does not match the versin in the file");
                    }

                    //Build the spreadsheet from the file
                    foreach (var pair in spreadsheetFile.Cells)
                    {
                        this.SetContentsOfCell(pair.Key, pair.Value.StringForm);
                    }
                }
                //If file is null throw an exception
                else
                {
                    throw new SpreadsheetReadWriteException("The spreadSheetFile cannot be null");
                }
            }
            catch (Exception)
            {
                throw new SpreadsheetReadWriteException("There was an error when reading the file");
            }
        }

        /// <summary>
        /// Constructor for JSON to properly deserialize 
        /// </summary>
        /// <param name="cells"></param>
        /// <param name="version"></param>
        [JsonConstructor]
        public Spreadsheet(Dictionary<string, Cell> cells, string version) : this(s => true, s => s, version)
        {
            this.Cells = cells;
        }


        public override object GetCellContents(string name)
        {
            //Check that variable name is valid
            if (!checkVariableName(name))
            {
                throw new InvalidNameException();
            }

            //normalize variable
            name = normalize(name);

            //If name is not in the dict, it is an empty cell and an empty string is returned
            if (!Cells.ContainsKey(name))
            {
                return "";
            }

            //otherwise, return the value of the given cell
            else
            {
                Cell cell = Cells[name];
                return cell.getContent();

            }
        }

        public override object GetCellValue(string name)
        {
            //Check that variable name is valud
            if (!checkVariableName(name))
            {
                throw new InvalidNameException();
            }

            //normalize variable
            name = normalize(name);

            //Try to get the value of the cell
            if (Cells.TryGetValue(name, out var cell))
                return cell.getValue();

            //If the cell does not have a value, return an empty string
            else
                return "";

        }

        public override IEnumerable<string> GetNamesOfAllNonemptyCells()
        {

            //Return keys in dict since dict contains all nonEmpty cells
            return Cells.Keys;
        }

        public override void Save(string filename)
        {
            try
            {
                //Format the spreadsheet correctly
                var options = new JsonSerializerOptions { WriteIndented = true };

                // Serialize to JSON in correcy format
                string json = JsonSerializer.Serialize(this, options);


                // Write to the file
                File.WriteAllText(filename, json);

                Changed = false;
            }
            //If any exceptions are thrown, catch them and throw ReadWrite Exception
            catch (Exception)
            {
                throw new SpreadsheetReadWriteException("There was an error writing to the file");
            }
        }

        protected override IList<string> SetCellContents(string name, double number)
        {

            //Create new cell with new content
            Cell newCell = new Cell(number);

            //Check if cell had previous content
            if (Cells.ContainsKey(name))
            {
                //Replace old content with new content
                Cells[name] = newCell;

                //If cell had  previous content, remove any possible Dependees
                IList<string> replaceDependess = new List<string>();
                graph.ReplaceDependees(name, replaceDependess);
            }
            //If cell did not have previous content, create a new cell
            else
            {
                Cells.Add(name, newCell);
            }

            //Get cells that depend on the cell being changed, cast the list to approiate type and return
            IEnumerable<string> recalcCellsEnumerable = GetCellsToRecalculate(name);
            IList<string> recalcCells = new List<string>(recalcCellsEnumerable);
            return recalcCells;

        }

        protected override IList<string> SetCellContents(string name, string text)
        {

            //Create new cell with new content
            Cell newCell = new Cell(text);

            //Check if the cell already has a value
            if (Cells.ContainsKey(name))
            {
                //Check if the given text is empty
                if (string.IsNullOrEmpty(text))
                {
                    //Remove cell from dict since cell is being set to be "empty".
                    Cells.Remove(name);
                }

                //If given text is not empty, replace old text with new text
                else
                {

                    Cells[name] = newCell;
                }

                //If cell had previous content, remove any possible Dependees
                IList<string> replaceDependess = new List<string>();
                graph.ReplaceDependees(name, replaceDependess);
            }


            //If cell did not have previous content, create a new cell if string is not empty
            else if (!string.IsNullOrEmpty(text))
            {
                Cells.Add(name, newCell);
            }

            //Get cells that depend on the cell being changed, cast the list to approiate type and return
            IEnumerable<string> recalcCellsEnumerable = GetCellsToRecalculate(name);
            IList<string> recalcCells = new List<string>(recalcCellsEnumerable);
            return recalcCells;

        }

        protected override IList<string> SetCellContents(string name, Formula formula)
        {


            //create new cell with new content
            Cell newCell = new Cell(formula, getVariableValue);

            //Check if cell had previous content
            if (Cells.ContainsKey(name))
            {
                //Replace old content with new content
                Cells[name] = newCell;

                //Since cell had revious content, remove any possible Dependees
                IList<string> replaceDependess = new List<string>();
                graph.ReplaceDependees(name, replaceDependess);

            }

            //Create new cell with given formula
            else
            {
                Cells.Add(name, newCell);
            }

            //Get Dependees for the given cell based on the formula
            DependeesForVariable(name);

            //Get cells that depend on the cell being changed, cast the list to approiate type and return
            IEnumerable<string> recalcCellsEnumerable = GetCellsToRecalculate(name);
            IList<string> recalcCells = new List<string>(recalcCellsEnumerable);
            return recalcCells;

        }




        public override IList<string> SetContentsOfCell(string name, string content)
        {
            //List of dependents to return
            IList<string> dependents;

            //If a circular exception is thrown, track the cells old content so it can be reset
            string oldContent = "";


            //Check that variable name is valid, throw exception if not
            if (!checkVariableName(name))
            {
                throw new InvalidNameException();
            }

            //normalize variable
            name = normalize(name);

            //Check if varable is double
            if (double.TryParse(content, out var num))
            {
                dependents = new List<string>(SetCellContents(name, num));
            }

            //Check if variable is formula
            else if (content.StartsWith("="))
            {
                //Get rid of = 
                content = content.Substring(1, content.Length - 1);

                //Create a formula from the new content 
                Formula formula = new Formula(content, normalize, isValid);

                //If the cell had an old value, get it and store it in case an exception is thrown
                if (Cells.TryGetValue(name, out var cell))
                {
                    if (cell != null)
                    {
                        oldContent = cell.StringForm;
                    }
                }

                //Try to change cells content to formula
                try
                {
                    dependents = new List<string>(SetCellContents(name, formula));

                }


                // if changing the cell's content to the given value creates a circular dependency
                catch (CircularException)
                {
                    //Restore graph with old content
                    SetContentsOfCell(name, oldContent);

                    //Throw a circular exception
                    throw new CircularException();
                }
            }

            //otherwise the content is a string
            else
            {
                dependents = new List<string>(SetCellContents(name, content));
            }

            //Recalculate the values of all dependent cells
            foreach (string cell in dependents)

            {
                if (Cells.TryGetValue(cell, out var value))
                {
                    value.reEvaluateFormulaValue(getVariableValue);
                }
            }

            Changed = true;
            return dependents;


        }


        protected override IEnumerable<string> GetDirectDependents(string name)
        {
            //Return the dependents for the given variable
            return graph.GetDependents(name);
        }

        /// <summary>
        /// Private helper method that will determine if a variable name has a valid format.
        /// If the variable follows the rules of the spreadsheet, this method will return true, 
        /// else if will return false.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        /// <exception cref="InvalidNameException"></exception>
        private bool checkVariableName(string name)
        {
            // pattern for a valid variable
            string varPattern = @"^[A-Za-z_][A-Za-z0-9_]*$";

            // check if the variable matches the specified patteron or is null
            if (!Regex.IsMatch(name, varPattern) || name == null)
            {
                //if so, return false
                return false;
            }

            name = normalize(name);
            //if the name is not valid according to the user, return false
            if (!isValid(name))
            {
                return false;
            }

            // otherwise, return true
            else
            {

                return true;
            }



        }

        /// <summary>
        /// Private helper method that will add all the given dependees for a cell
        /// when a formula is added to that cell as its content. All the dependees
        /// for that cell will be any variables that are present in the given formula
        /// </summary>
        /// <param name="name"></param>
        private void DependeesForVariable(string name)
        {

            Formula expression = (Formula)Cells[name].getContent();
            IEnumerable<string> variables = expression.GetVariables();

            foreach (string variable in variables)
            {
                graph.AddDependency(variable, name);

            }



        }


        /// <summary>
        /// Private nested class that represents a single cell in a spreadsheet. The class has 
        /// three constructors depending on the type of content that is being held in the cell(text,
        /// double, or a formula). Each cell has three private variables that are initiated in the constructor,
        /// the cells content,the cells value, and the stringForm of the cells contents. The content is the actual text in the cell(the text itself,
        /// the double itself, or the actual formula expression itself). The value is the actual value of what is 
        /// in the cell (the value of a text cell is just text, the value of a double cell is the double,
        /// and the value of a formula is the actual value of the formula once it has been calculated). The stringForm
        /// is just the cell contents, but is needed for proper deserialization.
        /// </summary>
        public class Cell
        {
            //public variable for JSON deserialization
            [JsonInclude()]
            public string StringForm { get; protected set; }
            //Private variable for cells content
            private object content;

            //Private variable for cells value
            private object value;



            /// <summary>
            /// Constructor for a cell with simple text as its content, will also be 
            /// the cell constrcutor for deserializing.
            /// </summary>
            /// <param name="name"></param>

            [JsonConstructor]
            public Cell(string StringForm)
            {
                this.content = StringForm;
                this.value = StringForm;
                this.StringForm = StringForm;
            }

            /// <summary>
            /// Constructor for a cell with a double as its content
            /// </summary>
            /// <param name="number"></param>
            public Cell(double number)
            {
                this.content = number;
                this.value = number;
                this.StringForm = number.ToString();
            }
            /// <summary>
            /// Constructor for a Cell with a formula as its content.
            /// For this assignment, a formula cell's value is simply set to 0.
            /// </summary>
            /// <param name="formula"></param>
            public Cell(Formula formula, Func<string, double> lookup)
            {
                this.content = formula;
                this.value = formula.Evaluate(lookup);
                this.StringForm = "=" + formula.ToString();
            }

            /// <summary>
            /// Get the content of a given cell
            /// </summary>
            /// <returns></returns>
            public object getContent()
            {
                return content;
            }

            /// <summary>
            /// Get the value of a given cell.
            /// </summary>
            /// <returns></returns>
            public object getValue()
            {
                return value;
            }
            /// <summary>
            /// Private helper method to recalculate the value of a cell if a 
            /// cell value it depends on has been changed.
            /// </summary>
            /// <param name="lookup"></param>
            public void reEvaluateFormulaValue(Func<string, double> lookup)
            {
                if (content.GetType().Equals(typeof(Formula)))
                {
                    Formula f = (Formula)content;
                    value = f.Evaluate(lookup);
                }
            }



        }
        /// <summary>
        /// Private helper method that serves as the lookup delegate for evaluating formulas
        /// when getting the value of a variable. If the variables value can be parsed as a double
        /// it will return that double, otherwise it will throw an exception.
        /// </summary>
        /// <param name="cellName"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        private double getVariableValue(string cellName)
        {
            if (Cells.TryGetValue(cellName, out var cell))
            {
                if (cell.getValue().GetType() == typeof(double))
                {
                    return (double)cell.getValue();
                }

            }
            throw new ArgumentException();



        }
    }
}


