namespace LEGO_Brickster_AI; 
using System;
public class BotException : Exception
//Base BotException class
{
    public BotException() : base("An error has occured with the driver") { }

    public BotException(string message) : base(message) { }

    public BotException(string message, Exception inner) : base(message, inner) { }
}


/// <summary>
/// Exception thrown when a bot fails to navigate to a webpage due to an invalid URL.
/// </summary>
public class BotUrlException : BotException
{ 
    public BotUrlException() : base() {}

    public BotUrlException(string message) : base(message) {}

    public BotUrlException(string message, Exception inner) : base(message, inner) { }
}

/// <summary>
/// Exception thrown when a bot fails to navigate to a webpage due to an invalid 'By' lolocator mechanism (Link Text, XPath etc).
/// </summary>
public class BotElementException : BotException
{
    public BotElementException() : base() { }

    public BotElementException(string message) : base(message) { }

    public BotElementException(string message, Exception inner) : base(message, inner) { }
}

public class BotMechanismException : BotException
{
    public BotMechanismException() : base() { }

    public BotMechanismException(string message) : base(message) { }

    public BotMechanismException(string message, Exception inner) : base(message, inner) { }
}

public class BotDownloadAmountException : BotException
{
    public BotDownloadAmountException() : base() { }

    public BotDownloadAmountException(string message) : base(message) { }

    public BotDownloadAmountException (string message, Exception inner) : base(message, inner) {} 
}
