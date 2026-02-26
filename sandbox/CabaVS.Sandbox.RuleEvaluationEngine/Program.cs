#pragma warning disable IDE0011 // Add braces
#pragma warning disable S125 // Sections of code should not be commented out
#pragma warning disable S3903 // Types should be defined in named namespaces

using System.Globalization;

const string input =
    """
    TYPE=Felony;YEARS=7;ACTION=Reject
    TYPE=Misdemeanor;COUNT=3;YEARS=5;ACTION=ManualReview;PRIORITY=1
    STATE_MISMATCH=true;ACTION=ManualReview
    """;

var rules = input.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
if (rules.Length == 0)
{
    Console.WriteLine("No rules found.");
    return;
}

var candidate = new Candidate
{
    Id = "12345",
    State = "NY"
};

var offenses = new List<OffenseRecord>
{
    //new OffenseRecord
    //{
    //    Type = "Felony",
    //    Date = DateTime.UtcNow.AddYears(-5),
    //    State = "NY"
    //},
    //new OffenseRecord
    //{
    //    Type = "Misdemeanor",
    //    Date = DateTime.UtcNow.AddYears(-3),
    //    State = "NY"
    //},
    new OffenseRecord
    {
        Type = "Misdemeanor",
        Date = DateTime.UtcNow.AddYears(-2),
        State = "NJ"
    },
    new OffenseRecord
    {
        Type = "Misdemeanor",
        Date = DateTime.UtcNow.AddYears(-1),
        State = "NY"
    }
};

//ScreeningResult result = ScreeningService.Evaluate(candidate, offenses, [.. rules]);
//Console.WriteLine($"Final screening result: {result}");

var engine = new ParsedRuleEngine(rules);
ScreeningResult result = engine.Evaluate(candidate, offenses);
Console.WriteLine($"Final screening result: {result}");

internal sealed class Candidate
{
    public string Id { get; set; } = default!;
    public string State { get; set; } = default!;
}

internal sealed class OffenseRecord
{
    public string Type { get; set; } = default!; // "Felony" or "Misdemeanor"
    public DateTime Date { get; set; }
    public string State { get; set; } = default!;
}

internal enum ScreeningResult
{
    Approve,
    Reject,
    ManualReview
}

internal sealed class ParsedRule
{
    public string RawRuleInText { get; set; }

    public string? Type { get; set; }
    public string? State { get; set; }
    public int Years { get; set; }
    public int Count { get; set; }
    public int Priority { get; set; } = int.MaxValue; // Default to lowest priority
    public bool StateMismatch { get; set; }
    public ScreeningResult Action { get; set; }

    public ParsedRule(string raw)
    {
        var ruleParts = raw
            .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(part => part.Split('=', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            .Where(parts => parts.Length == 2)
            .ToDictionary(parts => parts[0], parts => parts[1]);
        if (ruleParts is { Count: 0 })
        {
            throw new InvalidOperationException("Invalid rule format.");
        }

        RawRuleInText = raw;

        if (ruleParts.TryGetValue("TYPE", out var type)) Type = type;
        if (ruleParts.TryGetValue("STATE", out var state)) State = state;
        if (ruleParts.TryGetValue("YEARS", out var yearsStr) && int.TryParse(yearsStr, NumberStyles.Integer, CultureInfo.InvariantCulture, out var years)) Years = years;
        if (ruleParts.TryGetValue("COUNT", out var countStr) && int.TryParse(countStr, NumberStyles.Integer, CultureInfo.InvariantCulture, out var count)) Count = count;
        if (ruleParts.TryGetValue("PRIORITY", out var priorityStr) && int.TryParse(priorityStr, NumberStyles.Integer, CultureInfo.InvariantCulture, out var priority)) Priority = priority;
        if (ruleParts.TryGetValue("STATE_MISMATCH", out var stateMismatchStr) && bool.TryParse(stateMismatchStr, out var stateMismatch)) StateMismatch = stateMismatch;
        if (ruleParts.TryGetValue("ACTION", out var actionStr) && Enum.TryParse<ScreeningResult>(actionStr, ignoreCase: true, out ScreeningResult action)) Action = action;
    }
}

internal sealed class ParsedRuleEvaluator(ParsedRule rule)
{
    public ParsedRule Rule => rule;

    public ScreeningResult? Evaluate(Candidate candidate, List<OffenseRecord> offenses)
    {
        IEnumerable<OffenseRecord> offensesToEvaluate = offenses;

        if (rule.Type is { })
            offensesToEvaluate = offensesToEvaluate.Where(offense => offense.Type.Equals(rule.Type, StringComparison.OrdinalIgnoreCase));
        if (rule.State is { })
            offensesToEvaluate = offensesToEvaluate.Where(offense => offense.State.Equals(rule.State, StringComparison.OrdinalIgnoreCase));
        if (rule.Years > 0)
            offensesToEvaluate = offensesToEvaluate.Where(offense => offense.Date.AddYears(rule.Years) >= DateTime.UtcNow);
        if (rule.StateMismatch)
            offensesToEvaluate = offensesToEvaluate.Where(offense => offense.State != candidate.State);

        if (rule.Count > 0)
        {
            var matchingOffensesCount = offensesToEvaluate.Count();
            return matchingOffensesCount >= rule.Count ? rule.Action : null;
        }
        else
        {
            return offensesToEvaluate.Any() ? rule.Action : null;
        }
    }
}

internal sealed class ParsedRuleEngine(IEnumerable<string> rules)
{
    private readonly List<ParsedRuleEvaluator> _ruleEvaluators = rules
        .Select(rule => new ParsedRule(rule))
        .OrderByDescending(rule => rule.Priority > 0)
        .ThenBy(rule => rule.Priority)
        .Select(rule => new ParsedRuleEvaluator(rule))
        .ToList();

    public ScreeningResult Evaluate(Candidate candidate, List<OffenseRecord> offenses)
    {
        ScreeningResult? finalResult = null;
        foreach (ParsedRuleEvaluator evaluator in _ruleEvaluators)
        {
            ScreeningResult? result = evaluator.Evaluate(candidate, offenses);
            if (result is not null)
            {
                Console.WriteLine($"Rule triggered: {evaluator.Rule.RawRuleInText}");
                finalResult ??= result.Value;
            }
        }

        return finalResult ?? ScreeningResult.Approve;
    }
}

//internal static class ScreeningService
//{
//    public static ScreeningResult Evaluate(Candidate candidate, List<OffenseRecord> offenses, List<string> ruleDefinitions)
//    {
//        Console.WriteLine($"Starting evaluation for candidate: {candidate.Id} from {candidate.State}");

//        if (offenses is { Count: 0 } || ruleDefinitions is { Count: 0 })
//        {
//            Console.WriteLine("No offenses or rules to evaluate.");
//            return ScreeningResult.Approve;
//        }

//        foreach (var ruleDefinition in ruleDefinitions)
//        {
//            Console.WriteLine($"Evaluating rule: {ruleDefinition}");

//            IRule? rule = RuleFactory.CreateRule(ruleDefinition);
//            if (rule is null)
//            {
//                Console.WriteLine("Failed to parse rule, skipping.");
//                continue;
//            }

//            ScreeningResult? evaluationResult = rule.Evaluate(candidate, offenses);
//            if (evaluationResult is null)
//            {
//                Console.WriteLine("Rule did not apply to candidate, moving to next rule.");
//                continue;
//            }

//            return evaluationResult.Value;
//        }

//        Console.WriteLine("No rules triggered, approving candidate.");

//        return ScreeningResult.Approve;
//    }
//}

//internal interface IRule
//{
//    ScreeningResult? Evaluate(Candidate candidate, List<OffenseRecord> offenses);
//}

//internal static class RuleFactory
//{
//    public static IRule? CreateRule(string raw)
//    {
//        var ruleParts = raw.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
//        if (ruleParts is { Length: 0 })
//        {
//            throw new InvalidOperationException("Invalid rule format.");
//        }

//        var rulePartsMap = ruleParts
//            .Select(part => part.Split('=', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
//            .Where(parts => parts.Length == 2)
//            .ToDictionary(parts => parts[0], parts => parts[1]);
//        if (rulePartsMap.Keys.Count == 0)
//        {
//            throw new InvalidOperationException("No valid rule parts found.");
//        }

//        _ = rulePartsMap.TryGetValue("TYPE", out var type);

//        return (type ?? string.Empty).ToUpperInvariant() switch
//        {
//            "FELONY" => new FelonyRule(
//                int.Parse(rulePartsMap["YEARS"], CultureInfo.InvariantCulture),
//                Enum.Parse<ScreeningResult>(rulePartsMap["ACTION"], ignoreCase: true)),
//            "MISDEMEANOR" => new MisdemeanorRule(
//                int.Parse(rulePartsMap["COUNT"], CultureInfo.InvariantCulture),
//                int.Parse(rulePartsMap["YEARS"], CultureInfo.InvariantCulture),
//                Enum.Parse<ScreeningResult>(rulePartsMap["ACTION"], ignoreCase: true)),
//            _ => new StateMismatchRule(
//                bool.Parse(rulePartsMap["STATE_MISMATCH"]),
//                Enum.Parse<ScreeningResult>(rulePartsMap["ACTION"], ignoreCase: true))
//        };
//    }
//}

//internal sealed record FelonyRule(int Years, ScreeningResult Action) : IRule
//{
//    public ScreeningResult? Evaluate(Candidate candidate, List<OffenseRecord> offenses)
//    {
//        OffenseRecord? mostRecent = offenses
//            .Where(offense => offense.Type.Equals("Felony", StringComparison.OrdinalIgnoreCase))
//            .OrderByDescending(offenses => offenses.Date)
//            .FirstOrDefault();
//        if (mostRecent is null)
//        {
//            return null;
//        }

//        return mostRecent.Date.AddYears(Years) >= DateTime.UtcNow
//            ? Action
//            : null;
//    }
//}

//internal sealed record MisdemeanorRule(int Count, int Years, ScreeningResult Action) : IRule
//{
//    public ScreeningResult? Evaluate(Candidate candidate, List<OffenseRecord> offenses)
//    {
//        var numberOfOffenses = offenses
//            .Where(offense => offense.Type.Equals("Misdemeanor", StringComparison.OrdinalIgnoreCase))
//            .Count(offense => offense.Date.AddYears(Years) >= DateTime.UtcNow);
//        return numberOfOffenses >= Count
//            ? Action
//            : null;
//    }
//}

//internal sealed record StateMismatchRule(bool StateMismatch, ScreeningResult Action) : IRule
//{
//    public ScreeningResult? Evaluate(Candidate candidate, List<OffenseRecord> offenses)
//    {
//        var anyOffenseStateMismatch = offenses.Any(offense => offense.State != candidate.State);
//        return anyOffenseStateMismatch == StateMismatch
//            ? Action
//            : null;
//    }
//}

#pragma warning restore IDE0011 // Add braces
#pragma warning restore S125 // Sections of code should not be commented out
#pragma warning restore S3903 // Types should be defined in named namespaces
