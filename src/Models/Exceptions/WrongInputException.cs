using System.Collections;

namespace Models.Exceptions;

public class WrongInputException(string input) : Exception
{
    public override IDictionary Data => new Dictionary<string, object>
    {
        ["input"] = input
    };
}
