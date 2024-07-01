namespace PizzaServer;

// • NAW van de klant
//  o Naam klant (name)
//  o Address (4digits 2caps name)
// • Per soort pizza:
//  o Naam van de pizza (+int)
//  o Aantal pizza’s (+int)
//  o Aantal extra toppings (+int)
//  o Voor elke extra topping een veld waarin de topping beschreven staat 
//      (name)
// • Datum en tijd van bestelling (2digits/2digits/4digits 2digits:2digits)


public class InterpreterMachine
{
    protected ILineValidator? _validator;
    protected bool _done = false;

    public void Expect(ILineValidator validator)
    {
        _validator = validator;
    }

    public bool PreInterpretLine(string text)
    {
        var line = text.Split('\n')[0];
        if (line.Length == 0)
        {
            return false;
        }
        if (_done)
        {
            return false;
        }

        if (_validator != null && !_validator.Validate(line))
        {
            throw new Exception($"expected {_validator}, but got {line} instead");
        }

        return true;
    }
}

class OrderInterpreterMachine : InterpreterMachine
{
    OrderInterpreterMachine()
    {
        _validator = new NameValidator();
    }

    public void Interpret(string text)
    {
        var lines = text.Split("\n");
        var i = 0;
        
    }
}



public interface ILineValidator
{
    public bool Validate(string line);
}

public abstract class CharsValidator
{
    protected int _length;

    private protected CharsValidator(int len)
    {
        _length = len;
    }
    public abstract bool Validate(string chars);

    public int Length()
    {
        return _length;
    }
}

public class NameValidator : ILineValidator
{
    public bool Validate(string line)
    {
        if (line.Any(char.IsDigit))
        {
            return false;
        }

        return line.Length <= 32;
    }
}

public class Validator(CharsValidator[] charsValidators) : ILineValidator
{
    private CharsValidator[] _charsValidators = charsValidators;

    public bool Validate(string line)
    {
        var i = 0;
        var eol = false;
        foreach (var validator in _charsValidators)
        {
            if (eol) return false;
            if (validator is EndValidator)
            {
                eol = true;
                return (i == line.Length);
            }

            if (i + validator.Length() > line.Length)
            {
                return false;
            }
            var section = validator.Length() != 0 ? line.Substring(i, validator.Length()) : line.Substring(i);
            if (!validator.Validate(section))
            {
                return false;
            }

            i += validator.Length();
        }

        return true;
    }

    public static Validator TimeStamp()
    {
        return new Validator([
            new DigitValidator(2), new ExactValidator("/"), new DigitValidator(2),
            new ExactValidator("/"), new DigitValidator(4), new ExactValidator(" "),
            new DigitValidator(2), new ExactValidator(":"), new DigitValidator(2),
            new EndValidator()
        ]);
    } // 00/00/0000 00:00[End]

    public static Validator Address()
    {
        return new Validator([
            new CapsValidator(), new AtLeastValidator(2, new OrValidator(
                new LetterValidator(), new ExactValidator(" ")
            )),
            new AtLeastValidator(1, new DigitValidator()), new EndValidator()
        ]);
    }// A(a| )(2+) 00[End]

    public static Validator Area()
    {
        return new Validator([
            new DigitValidator(4), new ExactValidator(" "), new CapsValidator(2),
            new ExactValidator(" "), new CapsValidator(),
            new AtLeastValidator(2, new LetterValidator()),
            new EndValidator()
        ]);
    } // 0000 AA A(a)(2+)[End]
}

public class DigitValidator(int len = 1) : CharsValidator(len)
{
    public override bool Validate(string chars)
    {
        return _length == chars.Length && chars.All(char.IsDigit);
    }
}

public class CapsValidator(int len = 1): CharsValidator(len)
{
    public override bool Validate(string chars)
    {
        return _length == chars.Length && chars.All(char.IsUpper);
    }
}

public class LetterValidator(int len = 1) : CharsValidator(len)
{
    public override bool Validate(string chars)
    {
        return _length == chars.Length && chars.All(char.IsLetter);
    }
}

public class ExactValidator(string match) : CharsValidator(match.Length)
{
    public override bool Validate(string chars)
    {
        return (chars == match);
    }
}

public class AtLeastValidator(int minimumMatches, CharsValidator validator) : CharsValidator(0)
{
    public override bool Validate(string chars)
    {
        if (validator.Length() == 0)
            throw new Exception(
                "Validator inside of validators without known length, should have a known length (non 0)");
        var i = 0;
        while (validator.Validate(chars.Substring(i, validator.Length())))
        {
            i += validator.Length();
            if (i >= chars.Length) {break;}
        }

        _length = i;
        return i >= minimumMatches;
    }
}

public class OrValidator(CharsValidator a, CharsValidator b) : CharsValidator(a.Length())
{
    public override bool Validate(string chars)
    {
        if (a.Length() != b.Length() || a.Length() == 0)
        {
            throw new Exception("both A and B should have the same length, and be known");
        }
        return a.Validate(chars) || b.Validate(chars);
    }
}

public class EndValidator() : CharsValidator(0)
{
    public override bool Validate(string chars)
    {
        throw new NotImplementedException("This validator should never be called. Instead when a LineFromCharsValidator encounters this validator, it should check itself");
    }
}