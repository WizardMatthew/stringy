// Stringy: A simple C# .Net 9.0 console app to convert strings to bulleted list
// Will convert a string of comma-separated values to a bulleted list with an option 
// to sort each section of the list in it's own alphabetical order
// Parentheses determine the depth of the bullet point
// NOTES: 
// 1) Might be overkill for a simple task, but I created a small class and built a structure of the List with depth
// and children to allow for future expansion if needed.
// 2) If I could have talked with the designer/requester, I might have made some different choices. For now,
// I leaned towards readability and maintainability over performance.

// TO USE: "dotnet run" in the terminal from the project directory

// test string(s). Included with and without quotes just in case those quotes on the page were literal.
string data = "(id, name, email, type(id, name, customFields(c1, c2, c3)), externalId)";
string dataWithQuotes = """(id, name, email, type(id, name, customFields(c1, c2, c3)), externalId)""";

try
{
    // Convert the string to a bulleted list
    Console.Write("Bullet list /w Original sort:\n");
    PrintBullets(ConvertToBullets(data));

    Console.WriteLine("\n------------------\n\n");

    Console.Write("Bullet list /w Alphanumerical sort:\n");
    PrintBullets(ConvertToBullets(dataWithQuotes, true));
}
catch (Exception ex)
{
    Console.WriteLine($"Error Type: {ex.GetType().Name} \nErrorMessage: {ex.Message}");
}



// METHODS -------------------------------------------------------------------------------
// recursive method to print the bullet points with indentation based on depth
void PrintBullets(List<Bullet> bullets)
{
    foreach (var bullet in bullets)
    {
        Console.WriteLine($"{new string(' ', bullet.depth * 2)}- {bullet.Text}");
        if (bullet.Children != null && bullet.Children.Count > 0)
        {
            PrintBullets(bullet.Children);
        }
    }
}

// Expects input format: <openparen> <value[nospaces]> <comma> <value[nospaces]> <closeparen> (nested parens allowed)
List<Bullet> ConvertToBullets(string input, bool sort = false, int depth = 0)
{
    if (string.IsNullOrWhiteSpace(input))
        throw new ArgumentException("Input string cannot be null or empty.");

    // Remove quotes around string and extra space
    input = input.Trim().Trim('"'); 

    // Create a list to hold the bullet points
    var rootBullets = new List<Bullet>();

    // Validate parentheses
    int openParenIndex = input.IndexOf('(');
    int closeParenIndex = input.LastIndexOf(')');
    if (openParenIndex == -1 || closeParenIndex == -1 || openParenIndex > closeParenIndex) 
        throw new ArgumentException("Invalud input string. Parentheses are not matched or in the wrong order.");

    var rawStrings = SplitToRawStrings(input);
    
    if (rawStrings.Count == 0) 
        throw new ArgumentException("No valid bullet points found in the input string.");

    // If sorting is enabled, sort bullets points alphabetically
    if (sort)
        rawStrings = rawStrings.OrderBy(x => x).ToList();

    int currentDepth = depth;
    foreach (string rootText in rawStrings)
    {
        // Check for nested parentheses
        int nestedOpenParenIndex = rootText.IndexOf('(');
        int nestedCloseParenIndex = rootText.LastIndexOf(')');

        if (nestedOpenParenIndex != -1 && nestedCloseParenIndex != -1 && nestedOpenParenIndex < nestedCloseParenIndex)
        {
            currentDepth++;

            // Create a new bullet point for the current text (and call recursively for children)
            var bullet = new Bullet
            {
                Text = rootText.Substring(0, nestedOpenParenIndex).Trim(),
                depth = currentDepth-1,
                Children = ConvertToBullets(rootText.Substring(nestedOpenParenIndex), sort, currentDepth)
            };
            rootBullets.Add(bullet);

            currentDepth--;
        }
        else
        {
            // Create a new bullet point for the current text
            var bullet = new Bullet
            {
                Text = rootText.Trim(),
                depth = currentDepth
            };
            rootBullets.Add(bullet);
        }

    }

    return rootBullets;
}

// Helper method to split strings by commas, ignoring commas within parentheses
List<string> SplitToRawStrings(string input)
{
    List<string> rawStrings = new List<string>();
    int parenDepth = 0;
    int lastSplit = 0;

    // remove surrounding parens
    if (input.StartsWith("(") && input.EndsWith(")"))
    {
        input = input[1..^1].Trim();
    }

    for (int i = 0; i < input.Length; i++)
    {
        if (input[i] == '(')
        {
            parenDepth++;
        }
        else if (input[i] == ')')
        {
            parenDepth--;
        }
        else if (input[i] == ',' && parenDepth == 0)
        {
            rawStrings.Add(input.Substring(lastSplit, i - lastSplit).Trim());
            lastSplit = i + 1;
        }
    }

    // Add the final segment
    if (lastSplit < input.Length)
    {
        rawStrings.Add(input.Substring(lastSplit).Trim());
    }

    return rawStrings;
}

// Helper class to represent a bullet point with its text, depth, and children
class Bullet
{
    public required string Text { get; set; }
    public required int depth { get; set; }
    public List<Bullet> Children { get; set; } = new List<Bullet>();
}


