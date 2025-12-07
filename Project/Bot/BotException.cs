namespace LEGO_Brickster_AI;

using System;
public class BotException : Exception
//Base BotException class
{
    public BotException() : base("An error has occured with the Bot") { }

    public BotException(string message) : base(message) { }

    public BotException(string message, Exception inner) : base(message, inner) { }
}

/// <summary>
///  A general Bot exception for when the webdriver itself throws exceptions
/// </summary>
public class BotDriverException : BotException
{
    public BotDriverException() : base() { }

    public BotDriverException(string message) : base(message) { }

    public BotDriverException(string message, Exception inner) : base(message, inner) { }
}


/// <summary>
/// Exception thrown when a bot fails to navigate to a webpage due to an invalid URL.
/// </summary>
public class BotUrlException : BotException
{
    public BotUrlException() : base() { }

    public BotUrlException(string message) : base(message) { }

    public BotUrlException(string message, Exception inner) : base(message, inner) { }
}


/// <summary>
/// Exception thrown when a bot fails to navigate to a webpage due to an invalid 'By' lolocator mechanism (Link Text, XPath etc).
/// </summary>
public class BotFindElementException : BotException
{
    public BotFindElementException() : base() { }

    public BotFindElementException(string message) : base(message) { }

    public BotFindElementException(string message, Exception inner) : base(message, inner) { }
}

public class BotMechanismException : BotException
{
    public BotMechanismException() : base() { }

    public BotMechanismException(string message) : base(message) { }

    public BotMechanismException(string message, Exception inner) : base(message, inner) { }
}

public class BotTimeOutException : BotException
{
    public BotTimeOutException() : base() { }

    public BotTimeOutException(string message) : base(message) { }

    public BotTimeOutException(string message, Exception inner) : base(message, inner) { }
}

public class BotStaleElementException : BotException
{
    public BotStaleElementException() : base() { }

    public BotStaleElementException(string message) : base(message) { }

    public BotStaleElementException(string message, Exception inner) : base(message, inner) { }
}



public class BotDownloadAmountException : BotException
{
    public BotDownloadAmountException() : base() { }

    public BotDownloadAmountException(string message) : base(message) { }

    public BotDownloadAmountException(string message, Exception inner) : base(message, inner) { }
}

public class BotFileDownloadException : BotException
{
    public BotFileDownloadException() : base() { }

    public BotFileDownloadException(string message) : base(message) { }

    public BotFileDownloadException(string message, Exception inner) : base(message, inner) { }
}

public class BotFileRenameException : BotException
{
    public BotFileRenameException() : base() { }

    public BotFileRenameException(string message) : base(message) { }

    public BotFileRenameException(string message, Exception inner) : base(message, inner) { }
}

public class BotWindowException : BotException
{
    public BotWindowException() : base() { }

    public BotWindowException(string message) : base(message) { }

    public BotWindowException(string message, Exception inner) : base(message, inner) { }
}
