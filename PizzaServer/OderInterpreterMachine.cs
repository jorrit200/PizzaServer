using System.Collections;
using System.Runtime.InteropServices.JavaScript;

namespace PizzaServer;

/// <summary>
/// Allows you to populate the <see cref="InterpretLines"/> method, in which you can write sequential logic, to interpret multiple lines.
/// </summary>
/// <implementation>
/// implement <see cref="InterpretLines"/>, and keep track of the data.
/// Typically, each InterpreterMachine represents some model that gets serialized and serialized. You can also create a To{Model}() method that creates said model, after some checks 
/// </implementation>
public abstract class InterpreterMachine
{
    protected ILineValidator? _validator;
    protected bool _done = false;
    
    private readonly IEnumerator<string> _lines;
    protected string _currentLine;

    protected IEnumerator? _interpretState;

    protected InterpreterMachine(IEnumerator<string> lines) => _lines = lines;

    /// <summary>
    /// When moving to the next line <see cref="NextLine"/>, enforce that line is of a valid format.
    /// Call this method while using <see cref="InterpretLines"/>, to prepare for the next line.
    /// </summary>
    /// <param name="validator">The formatter that must return true on the following line, or `null` to allow any line</param>
    protected void Expect(ILineValidator? validator)
    {
        _validator = validator;
    }

    /// <summary>
    /// Iterate the lines enumerator, and check it is valid with the validator from <see cref="Expect"/>
    /// </summary>
    /// <returns>Weather or not the line can be iterated, and this line is valid</returns>
    protected bool NextLine()
    {
        if (!_lines.MoveNext())
        {
            return false;
        }

        if (_done)
        {
            return false;
        }
        var line = _lines.Current;
        _currentLine = line;
        return _validator is null || _validator.Validate(line);
    }
    
    /// <summary>
    /// Begin the process of interpreting
    /// </summary>
    public void Interpret()
    {
        while (NextLine())
        {
            _interpretState ??= InterpretLines();
            if (_interpretState.MoveNext()) continue;
            break;
        }

        if (!_done)
        {
            Console.WriteLine("Interpreter machine failure!");
        }
    }
    /// <summary>
    /// Tippically only called by <see cref="Interpret"/>
    /// </summary>
    /// <returns>Enumerator to keep calling</returns>
    /// <implementation>
    /// Write sequential logic to deal with each line. After each `yield return` `_currentLine` becomes the next line.
    /// Before iterating use <see cref="Expect"/> to enforce a certain format from the next line.
    /// This method should focus on the happy path.
    /// </implementation>
    protected abstract IEnumerator InterpretLines();
}


// • NAW van de klant
//  o Naam klant (name)
//  o Address (name +int)
//  o Area (4digits 2caps name)
// • Per soort pizza:
//  o Naam van de pizza (+int)
//  o Aantal pizza’s (+int)
//  o Aantal extra toppings (+int)
//  o Voor elke extra topping een veld waarin de topping beschreven staat 
//      (name)
// • Datum en tijd van bestelling (2digits/2digits/4digits 2digits:2digits)

public class OrderInterpreterMachine : InterpreterMachine
{
    private string? _name;
    private string? _address;
    private string? _area;
    private string? _time;
    private List<Pizza> _pizzas = new();
    private List<int> _pizzaCounts = new List<int>();

    private Dictionary<string, int> _menu;
    private Dictionary<string, int> _toppings;
    
    public OrderInterpreterMachine(IEnumerator<string> lines, Dictionary<string, int> menu, Dictionary<string, int> toppings) : base(lines)
    {
        _validator = new NameValidator();
        _menu = menu;
        _toppings = toppings;
    }

    protected override IEnumerator InterpretLines()
    {
        _name = _currentLine;
        Expect(Validator.Address());
        yield return null;
        
        _address = _currentLine;
        Expect(Validator.Area());
        yield return null;

        _area = _currentLine;
        Expect(null);
        yield return null;

        while (true)
        {
            var pizzaOrTime = _currentLine;
            if (Validator.TimeStamp().Validate(pizzaOrTime))
            {
                _time = pizzaOrTime;
                _done = true;
                yield break;
            }

            var currentPizza = new Pizza(pizzaOrTime, _menu);
            Expect(new Validator([new AtLeastValidator(1, new DigitValidator()), new EndValidator()]));
            yield return null;

            var pizzaCount = int.Parse(_currentLine);
            _pizzaCounts.Add(pizzaCount);
            Expect(new Validator([new AtLeastValidator(1, new DigitValidator()), new EndValidator()]));
            yield return null;
            
            var toppingCount = int.Parse(_currentLine);
            Expect(new NameValidator());
            yield return null;

            for (var i = 0; i < toppingCount; i++)
            {
                currentPizza = currentPizza.WithTopping(_currentLine, _toppings);
                Expect(new NameValidator());

                if (i == toppingCount - 1)
                {
                    _pizzas.Add(currentPizza);
                    Expect(Validator.TimeStamp());
                }
                yield return null;
            }
            
        }
    }

    public Order ToOrder()
    {
        if (_name is null || _address is null || _area is null || _time is null)
        {
            throw new Exception($"Not all fields where filled in (name, address, area, date/time) ({_name}, {_address}, {_area}, {_time})");
        }

        if (_pizzas.Count == 0)
        {
            throw new Exception("and order was made with not valid pizzas");
        }

        if (_pizzas.Count != _pizzaCounts.Count)
        {
            throw new Exception("Pizza formatting is wrong");
        }

        var newOrder = new Order(_name, _area, _address, _time);
        foreach ((Pizza pizza, int count) pizzaData in _pizzas.Zip(_pizzaCounts))
        {
            newOrder.AddPizza(pizzaData.pizza, pizzaData.count);
        }

        return newOrder;
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