using CommunityToolkit.Maui.Storage;
using SpreadsheetUtilities;
using SS;
using System.ComponentModel.Design;
using System.Text;
using System.Threading;

namespace SpreadsheetGUI;

/// <summary>
/// Example of using a SpreadsheetGUI object
/// </summary>
public partial class MainPage : ContentPage
{
    CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
    Spreadsheet spreadsheet = new Spreadsheet(s => true, s => s.ToUpper(), "ps6");
    Stack<Tuple<int, int, string>> spreadsheetStack = new Stack<Tuple<int, int, string>>();
    /// <summary>
    /// Constructor for the demo
    /// </summary>
	public MainPage()
    {
        InitializeComponent();
        // This an example of registering a method so that it is notified when
        // an event happens.  The SelectionChanged event is declared with a
        // delegate that specifies that all methods that register with it must
        // take a SpreadsheetGrid as its parameter and return nothing.  So we
        // register the displaySelection method below.
        spreadsheetGrid.SelectionChanged += displaySelection;
        spreadsheetGrid.SetSelection(0, 0);
        displaySelection(spreadsheetGrid);
    }

    private void displaySelection(ISpreadsheetGrid grid)
    {
        spreadsheetGrid.GetSelection(out int col, out int row);
        string cell = cellName(col, row);
        nameText.Text = cell;
        if (spreadsheet.Cells.ContainsKey(cell))
        {
            contentText.Text = spreadsheet.Cells[cell].StringForm;
            valueText.Text = spreadsheet.GetCellValue(cell).ToString();
        }
        else
        {
            valueText.Text = "";
            contentText.Text = "";
        }
    }

    private async void NewClicked(object sender, EventArgs e)
    {
        bool answer = false;
        if (spreadsheet.Changed)
        {
            answer = await DisplayAlert("Warning", "The current Spreadsheet has not been saved. Would you like to save it before continuing?", "Yes", "No");
        }
        if (answer)
        {
            SaveFile(spreadsheet, cancellationTokenSource);
        }
        valueText.Text = " ";
        contentText.Text = "";
        spreadsheetGrid.Clear();
        spreadsheet = new Spreadsheet(s => true, s => s.ToUpper(), "ps6");
    }

    /// <summary>
    /// Opens any file as text and prints its contents.
    /// Note the use of async and await, concepts we will learn more about
    /// later this semester.
    /// </summary>
    private async void OpenClicked(object sender, EventArgs e)
    {
        try
        {
            if (spreadsheet.Changed)
            {
                bool answer = await DisplayAlert("Warning", "The current Spreadsheet has not been saved. Would you like to save it before continuing?", "Yes", "No");

                if (answer)
                {
                    SaveFile(this.spreadsheet, this.cancellationTokenSource);
                }
            }
            FileResult fileResult = await FilePicker.Default.PickAsync();
            if (fileResult != null)
            {
                this.spreadsheet = new Spreadsheet(fileResult.FullPath, s => true, s => s.ToUpper(), "ps6");
                recalculateCells(spreadsheetGrid, this.spreadsheet);
                displaySelection(spreadsheetGrid);
            }
            else
            {
                Console.WriteLine("No file selected.");
                displaySelection(spreadsheetGrid);
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", "There was an error when trying to load the spreadsheet", "ok");
            Console.WriteLine(ex);
            displaySelection(spreadsheetGrid);
        }
    }
    void OnScrollViewScrolled(object sender, EventArgs e)
    {
    }

    private void SaveBtn_Clicked(System.Object sender, System.EventArgs e)
    {
        SaveFile(this.spreadsheet, this.cancellationTokenSource);
    }

    void contentText_Entered(System.Object sender, EventArgs e)
    {
        spreadsheetGrid.GetSelection(out int col, out int row);
        string cell = cellName(col, row);
        if (!spreadsheet.GetNamesOfAllNonemptyCells().Contains(cell))
        {
            spreadsheetStack.Push(new Tuple<int, int, string>(col, row, ""));
        }

        else
            spreadsheetStack.Push(new Tuple<int, int, string>(col, row, contentText.Text));

        try
        {
            spreadsheet.SetContentsOfCell(cell, contentText.Text);
            if (spreadsheet.Cells.ContainsKey(cell))
            {
                recalculateCells(spreadsheetGrid, spreadsheet);
                if (spreadsheet.GetCellValue(cell).GetType() == typeof(FormulaError))
                {
                    contentText.Text = "ERROR";
                    valueText.Text = "ERROR";
                    spreadsheetGrid.SetValue(col, row, contentText.Text);
                }
                else
                {
                    valueText.Text = spreadsheet.GetCellValue(cell).ToString();
                }

            }
            else
            {
                spreadsheetGrid.SetValue(col, row, "");
                valueText.Text = "";
            }

        }
        catch (Exception ex)
        {
            if (ex.GetType() == typeof(CircularException))
            {
                valueText.Text = "A circular dependecy was detected";
            }
            else if (ex.GetType() == typeof(InvalidNameException))
            {
                valueText.Text = "Invalid format for variable in formula";
            }

            else if (ex.GetType() == typeof(ArgumentException))
            {
                valueText.Text = "Variable in the formula does not have a value";
            }

            spreadsheetGrid.SetValue(col, row, "");
            recalculateCells(spreadsheetGrid, spreadsheet);
        }

    }
    void contentText_TextChanged(System.Object sender, Microsoft.Maui.Controls.TextChangedEventArgs e)
    {
        //spreadsheetGrid.GetSelection(out int col, out int row);
        //spreadsheetGrid.SetValue(col,row,contentText.Text);
    }
    private static string cellName(int col, int row)
    {
        char letter = (char)(64 + col + 1);
        return letter + (row + 1).ToString();
    }

    private static void recalculateCells(SpreadsheetGrid spreadsheetGrid, Spreadsheet spreadsheet)
    {
        foreach (string cellName in spreadsheet.GetNamesOfAllNonemptyCells())
        {
            int.TryParse(cellName.Substring(1), out int result);
            spreadsheetGrid.SetValue(cellName[0] - 'A', result - 1, spreadsheet.GetCellValue(cellName).ToString());
        }
    }

    private async void SaveFile(Spreadsheet spreadsheet, CancellationTokenSource cancellationTokenSource)
    {
        try
        {
            string Path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string filePath = System.IO.Path.Combine(Path, "Saved.sprd");
            spreadsheet.Save(filePath);
            var stringD = File.ReadAllText(filePath);
            using var stream = new MemoryStream(Encoding.Default.GetBytes(stringD));
            var path = await FileSaver.SaveAsync("spreadsheet.sprd", stream, cancellationTokenSource.Token);
        }
        catch (Exception)
        {
            valueText.Text = "There was an error when trying to save the spreadsheet";
        }
    }



    private void HelpMenu_Clicked(System.Object sender, System.EventArgs e)
    {

        string helpMenuTxtIntro = "HELP MENU";
        string helpMenuTxt = "\n\nHow to use: \n\nClick on the cell you desire to change. Use the textbox above the grid to change the content of the cell and then press enter. " +
            "The evaluated contents, or value, will be displayed in the cell and above the text box. Use button 'Clear' to empty spreadsheet but be sure to save beforehand." +
            "\n\nTo save the current spreadsheet, navigate to 'file' at the top of the screen and then 'save'. You will be given options to choose file location.\n\nTo open a spreadsheet, " +
            "navigate to 'file' at the top of the screen and then 'open'. You will be given options to choose the file path.\n\nTo undo changes, use the undo button until the desired reversal is achieved.";
        // Create a new MauiPage and present it
        var newPage = new ContentPage();
        var label1 = new Label
        {
            Text = helpMenuTxtIntro,
            FontSize = 24, // Set the font size
            FontFamily = "Lobster-Regular",
            FontAttributes = FontAttributes.Bold,
            TextColor = Color.FromRgb(0, 0, 0) // Set text color
        };

        // Customize the text
        var label2 = new Label
        {
            Text = helpMenuTxt,
            FontSize = 24, // Set the font size
            FontFamily = "Times New Roman",
            TextColor = Color.FromRgb(0, 0, 0) // Set text color
        };

        // Customize the background color
        newPage.BackgroundColor = Color.FromRgb(255, 255, 255);

        // Add the label to the page's content
        newPage.Content = new StackLayout
        {
            Children = { label1, label2 },
            HorizontalOptions = LayoutOptions.Fill,
            VerticalOptions = LayoutOptions.Fill
        };

        Application.Current.MainPage.Navigation.PushAsync(newPage);
    }
    private void UndoBtn_Clicked(System.Object sender, System.EventArgs e)
    {
        if (spreadsheetStack.Count > 0)
        {
            System.Diagnostics.Debug.WriteLine(spreadsheetStack.Peek().ToString());
            Tuple<int, int, string> value = (spreadsheetStack.Pop());
            string cell = cellName(value.Item1, value.Item2);
            spreadsheet.SetContentsOfCell(cell, value.Item3);
            System.Diagnostics.Debug.WriteLine(spreadsheet.GetCellValue("A1"));
            spreadsheetGrid.SetValue(value.Item1, value.Item2, value.Item3);
            valueText.Text = spreadsheet.GetCellValue(cell).ToString();
            contentText.Text = value.Item3;
        }
    }
}