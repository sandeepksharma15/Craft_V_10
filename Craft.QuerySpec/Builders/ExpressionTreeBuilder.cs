using System.Collections.Concurrent;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace Craft.QuerySpec;

/// <summary>
/// Provides functionality for building LINQ expression trees from string-based query syntax.
/// Supports complex boolean expressions, arithmetic operations, string methods, and Math functions.
/// </summary>
/// <remarks>
/// This class enables dynamic construction of LINQ expressions from string queries, supporting:
/// - Binary operations: ==, !=, &lt;, &lt;=, &gt;, &gt;=
/// - Arithmetic operations: +, -, *, /, %
/// - Logical operations: &amp;, &amp;&amp;, |, ||
/// - String methods: Contains, StartsWith, EndsWith
/// - Math functions: Math.Abs, Math.Max, Math.Min
/// - Parentheses for grouping
/// - Nested property access (e.g., "Person.Address.Street")
/// - Type-safe conversion and validation
/// 
/// Enhanced to support more complex expressions inspired by advanced expression parsers.
/// </remarks>
public static class ExpressionTreeBuilder
{
    #region Constants and Patterns

    /// <summary>Core pattern for binary expressions without anchors. Supports nested properties with dot notation.</summary>
    private const string BinaryPatternCore = @"\s*(?'leftOperand'\w+(?:\.\w+)*)\s*(?'operator'(==|!=|<|<=|>|>=))\s*(?'rightOperand'\w+(?:\.\w+)*)\s*";

    /// <summary>Core pattern for arithmetic expressions without anchors. Supports nested properties and numeric values.</summary>
    private const string ArithmeticPatternCore = @"\s*(?'leftOperand'\w+(?:\.\w+)*|\d+(?:\.\d+)?)\s*(?'operator'(\+|\-|\*|\/|\%))\s*(?'rightOperand'\w+(?:\.\w+)*|\d+(?:\.\d+)?)\s*";

    /// <summary>Pattern for string method calls like Contains, StartsWith, EndsWith.</summary>
    private const string StringMethodPattern = @"^\s*(?'propertyName'\w+(?:\.\w+)*)\s*\.\s*(?'methodName'(Contains|StartsWith|EndsWith))\s*\(\s*[""'](?'value'[^""']*)[""']\s*\)\s*$";

    /// <summary>Pattern for Math function calls like Math.Abs, Math.Max, Math.Min.</summary>
    private const string MathFunctionPattern = @"^\s*Math\s*\.\s*(?'functionName'(Abs|Max|Min))\s*\(\s*(?'arguments'[^)]+)\s*\)\s*$";

    /// <summary>Pattern for simple binary expressions.</summary>
    private const string BinaryPattern = "^" + BinaryPatternCore + "$";

    /// <summary>Pattern for simple arithmetic expressions.</summary>
    private const string ArithmeticPattern = "^" + ArithmeticPatternCore + "$";

    /// <summary>Pattern for binary expressions wrapped in parentheses. Supports nested properties with dot notation.</summary>
    private const string BinaryWithBracketsPattern = @"^\s*\(" + BinaryPatternCore + @"\)\s*$";

    /// <summary>Pattern for escaped binary expressions with quoted values.</summary>
    private const string EscapedBinaryPattern = @"^\s*(\""\s*(?'leftOperand'.*)\s*\""\s*(?'operator'(==|!=|<|<=|>|>=))\s*(?'rightOperand'\w+)|(?'leftOperand'\w+)\s*(?'operator'(==|!=|<|<=|>|>=))\s*\""\s*(?'rightOperand'.*)\s*\""\s*)\s*$";

    /// <summary>Pattern for evaluation expressions with logical operators.</summary>
    private const string EvalPattern = @"^(?'leftOperand'\S{1,}\s*(==|!=|<|<=|>|>=)\s*\S{1,})\s*(?'evaluator_first'((\|{1,2})|(\&{1,2})))\s*(?'rightOperand'.*)\s*$";

    /// <summary>Pattern for expressions containing brackets with logical operators.</summary>
    private const string HasBrackets = @"^\s*(?'leftOperand'[^\|\&]*)\s*(?'evaluator_first'((\|)*|(\&)*))\s*(?'brackets'(\(\s*(.*)s*\)))\s*(?'evaluator_second'((\|{1,2})|(\&{1,2}))*)\s*(?'rightOperand'.*)\s*$";

    /// <summary>Pattern for expressions with only surrounding brackets.</summary>
    private const string HasSurroundingBracketsOnly = @"^\s*\(\s*(?'leftOperand'([^\(\)])+)\s*\)\s*$";

    // Named group constants for better maintainability
    private const string Brackets = "brackets";
    private const string EvaluatorFirst = "evaluator_first";
    private const string EvaluatorSecond = "evaluator_second";
    private const string LeftOperand = "leftOperand";
    private const string Operator = "operator";
    private const string RightOperand = "rightOperand";

    #endregion

    #region Static Readonly Dictionaries

    /// <summary>Maps binary operators to their corresponding expression factory functions.</summary>
    private static readonly IReadOnlyDictionary<string, Func<MemberExpression, object, BinaryExpression>> BinaryExpressionBuilder =
        new Dictionary<string, Func<MemberExpression, object, BinaryExpression>>
        {
            {"==", (me, value) => Expression.MakeBinary(ExpressionType.Equal, me, Expression.Constant(value))},
            {"!=", (me, value) => Expression.MakeBinary(ExpressionType.NotEqual, me, Expression.Constant(value))},
            {">", (me, value) => Expression.MakeBinary(ExpressionType.GreaterThan, me, Expression.Constant(value))},
            {">=", (me, value) => Expression.MakeBinary(ExpressionType.GreaterThanOrEqual, me, Expression.Constant(value))},
            {"<", (me, value) => Expression.MakeBinary(ExpressionType.LessThan, me, Expression.Constant(value))},
            {"<=", (me, value) => Expression.MakeBinary(ExpressionType.LessThanOrEqual, me, Expression.Constant(value))},
        };

    /// <summary>Maps arithmetic operators to their corresponding expression factory functions.</summary>
    private static readonly IReadOnlyDictionary<string, Func<Expression, Expression, BinaryExpression>> ArithmeticExpressionBuilder =
        new Dictionary<string, Func<Expression, Expression, BinaryExpression>>
        {
            {"+", (left, right) => Expression.Add(left, right)},
            {"-", (left, right) => Expression.Subtract(left, right)},
            {"*", (left, right) => Expression.Multiply(left, right)},
            {"/", (left, right) => Expression.Divide(left, right)},
            {"%", (left, right) => Expression.Modulo(left, right)},
        };

    /// <summary>Maps string method names to their corresponding method info.</summary>
    private static readonly IReadOnlyDictionary<string, Func<MemberExpression, string, MethodCallExpression>> StringMethodBuilder =
        new Dictionary<string, Func<MemberExpression, string, MethodCallExpression>>
        {
            {"Contains", (prop, value) => Expression.Call(prop, typeof(string).GetMethod("Contains", [typeof(string)])!, Expression.Constant(value))},
            {"StartsWith", (prop, value) => Expression.Call(prop, typeof(string).GetMethod("StartsWith", [typeof(string)])!, Expression.Constant(value))},
            {"EndsWith", (prop, value) => Expression.Call(prop, typeof(string).GetMethod("EndsWith", [typeof(string)])!, Expression.Constant(value))},
        };

    /// <summary>Maps Math function names to their corresponding method info.</summary>
    private static readonly IReadOnlyDictionary<string, Func<Expression[], MethodCallExpression?>> MathFunctionBuilder =
        new Dictionary<string, Func<Expression[], MethodCallExpression?>>
        {
            {"Abs", args => args.Length == 1 ? Expression.Call(typeof(Math).GetMethod("Abs", [args[0].Type])!, args[0]) : null},
            {"Max", args => args.Length == 2 && args[0].Type == args[1].Type ? Expression.Call(typeof(Math).GetMethod("Max", [args[0].Type, args[1].Type])!, args) : null},
            {"Min", args => args.Length == 2 && args[0].Type == args[1].Type ? Expression.Call(typeof(Math).GetMethod("Min", [args[0].Type, args[1].Type])!, args) : null},
        };

    /// <summary>Maps logical operators to their corresponding expression factory functions.</summary>
    private static readonly IReadOnlyDictionary<string, Func<Expression, Expression, Expression>> EvaluationExpressionBuilder =
        new Dictionary<string, Func<Expression, Expression, Expression>>
        {
            {"&", (left, right) => Expression.And(left, right)},
            {"&&", (left, right) => Expression.AndAlso(left, right)},
            {"|", (left, right) => Expression.Or(left, right)},
            {"||", (left, right) => Expression.OrElse(left, right)},
        };

    #endregion

    #region Caching

    /// <summary>Cache for type property descriptors to improve performance.</summary>
    private static readonly ConcurrentDictionary<Type, PropertyDescriptorCollection> _typePropertyCollection = new();

    /// <summary>Cache for compiled regular expressions to improve performance.</summary>
    private static readonly ConcurrentDictionary<string, Regex> _regexCache = new();

    #endregion

    #region Public API

    /// <summary>
    /// Builds a strongly-typed LINQ expression from a string query.
    /// </summary>
    /// <typeparam name="T">The type of the entities to filter.</typeparam>
    /// <param name="query">The string query to parse into an expression.</param>
    /// <returns>A compiled LINQ expression, or null if the query is invalid or empty.</returns>
    /// <remarks>
    /// Supports complex boolean expressions with parentheses, logical operators, property comparisons,
    /// arithmetic operations, string methods, and Math functions.
    /// Returns null for invalid queries rather than throwing exceptions.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Basic comparison
    /// var expr1 = ExpressionTreeBuilder.BuildBinaryTreeExpression&lt;Person&gt;("Age &gt; 18 &amp;&amp; Name == \"John\"");
    /// 
    /// // Nested properties
    /// var expr2 = ExpressionTreeBuilder.BuildBinaryTreeExpression&lt;Person&gt;("Address.City == \"Seattle\"");
    /// 
    /// // String methods
    /// var expr3 = ExpressionTreeBuilder.BuildBinaryTreeExpression&lt;Person&gt;("Name.StartsWith(\"J\")");
    /// 
    /// // Math functions
    /// var expr4 = ExpressionTreeBuilder.BuildBinaryTreeExpression&lt;Person&gt;("Math.Abs(Score) &gt; 100");
    /// 
    /// var predicate = expr1?.Compile();
    /// </code>
    /// </example>
    public static Expression<Func<T, bool>>? BuildBinaryTreeExpression<T>([AllowNull] string? query)
    {
        if (IsInvalidQuery(query))
            return null;

        var trimmed = TrimAllOuterBracketsAndWhitespace(query!);
        if (IsInvalidQuery(trimmed))
            return null;

        try
        {
            var exp = BuildBinaryTreeExpression(typeof(T), trimmed);
            return exp as Expression<Func<T, bool>>;
        }
        catch (Exception ex) when (IsExpectedParsingException(ex))
        {
            // Log the exception if a logger is available in the future
            return null;
        }
    }

    /// <summary>
    /// Builds a lambda expression from a string query for a specific type.
    /// </summary>
    /// <param name="type">The type of the entities to filter.</param>
    /// <param name="query">The string query to parse into an expression.</param>
    /// <returns>A lambda expression, or null if the query is invalid or empty.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="type"/> is null.</exception>
    public static LambdaExpression? BuildBinaryTreeExpression([NotNull] Type type, [AllowNull] string? query)
    {
        ArgumentNullException.ThrowIfNull(type, nameof(type));

        if (IsInvalidQuery(query))
            return null;

        var trimmed = TrimAllOuterBracketsAndWhitespace(query!);
        if (IsInvalidQuery(trimmed))
            return null;

        try
        {
            var parameterExpression = Expression.Parameter(type, "x");
            return BuildBinaryTreeExpressionWorker(type, trimmed, parameterExpression);
        }
        catch (Exception ex) when (IsExpectedParsingException(ex))
        {
            return null;
        }
    }

    /// <summary>
    /// Builds a lambda expression for a specific property comparison.
    /// </summary>
    /// <param name="type">The type containing the property.</param>
    /// <param name="propertyName">The name of the property to compare.</param>
    /// <param name="operator">The comparison operator (==, !=, &lt;, &lt;=, &gt;, &gt;=).</param>
    /// <param name="value">The value to compare against.</param>
    /// <param name="parameterExpression">The parameter expression for the lambda.</param>
    /// <returns>A lambda expression, or null if the parameters are invalid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when required parameters are null.</exception>
    public static LambdaExpression? BuildBinaryTreeExpression(
        [NotNull] Type type, 
        [NotNull] string propertyName, 
        [NotNull] string @operator, 
        [NotNull] string value, 
        [NotNull] ParameterExpression parameterExpression)
    {
        ArgumentNullException.ThrowIfNull(type, nameof(type));
        ArgumentNullException.ThrowIfNull(propertyName, nameof(propertyName));
        ArgumentNullException.ThrowIfNull(@operator, nameof(@operator));
        ArgumentNullException.ThrowIfNull(value, nameof(value));
        ArgumentNullException.ThrowIfNull(parameterExpression, nameof(parameterExpression));

        if (!propertyName.IsNonEmpty() || !@operator.IsNonEmpty() || !value.IsNonEmpty())
            return null;

        if (!BinaryExpressionBuilder.ContainsKey(@operator))
            return null;

        try
        {
            var props = GetTypeProperties(type);
            var prop = GetPropertyByName(props, propertyName);

            if (prop == null) 
                return null;

            var memberExpression = BuildNestedPropertyExpression(parameterExpression, propertyName);
            var convertedValue = ConvertValueToType(value, memberExpression.Type);
            
            if (convertedValue == null)
                return null;

            var body = BinaryExpressionBuilder[@operator](memberExpression, convertedValue);
            var delegateType = typeof(Func<,>).MakeGenericType(type, typeof(bool));
            
            return Expression.Lambda(delegateType, body, parameterExpression);
        }
        catch (Exception ex) when (IsExpectedParsingException(ex))
        {
            return null;
        }
    }

    /// <summary>
    /// Builds a compiled predicate function from a dictionary filter.
    /// </summary>
    /// <typeparam name="T">The type of the entities to filter.</typeparam>
    /// <param name="filter">A dictionary of property names and their expected values.</param>
    /// <returns>A compiled predicate function, or null if the filter is invalid.</returns>
    /// <remarks>
    /// All conditions in the dictionary are combined with AND logic.
    /// Property values are converted to the appropriate types automatically.
    /// </remarks>
    public static Func<T, bool>? BuildBinaryTreeExpression<T>([AllowNull] IDictionary<string, string>? filter)
    {
        var exp = ToExpression<T>(filter);
        return exp?.Compile();
    }

    /// <summary>
    /// Converts a dictionary filter to a LINQ expression.
    /// </summary>
    /// <typeparam name="T">The type of the entities to filter.</typeparam>
    /// <param name="filter">A dictionary of property names and their expected values.</param>
    /// <returns>A LINQ expression, or null if the filter is invalid or empty.</returns>
    public static Expression<Func<T, bool>>? ToExpression<T>([AllowNull] IDictionary<string, string>? filter)
    {
        if (filter?.Any() != true) 
            return null;

        try
        {
            var props = GetTypeProperties(typeof(T));
            var parameterExpression = Expression.Parameter(typeof(T), "x");
            var binaryExpressions = new List<BinaryExpression>();

            foreach (var kvp in filter)
            {
                var fieldName = kvp.Key;
                var prop = GetPropertyByName(props, fieldName);

                if (prop == null) 
                    return null;

                var memberExpression = BuildNestedPropertyExpression(parameterExpression, fieldName);
                var convertedValue = ConvertValueToType(kvp.Value, memberExpression.Type);
                
                if (convertedValue == null)
                    return null;

                var binaryExpression = Expression.Equal(memberExpression, Expression.Constant(convertedValue));
                binaryExpressions.Add(binaryExpression);
            }

            var combinedExpression = binaryExpressions.Aggregate((left, right) => Expression.AndAlso(left, right));
            return Expression.Lambda<Func<T, bool>>(combinedExpression, [parameterExpression]);
        }
        catch (Exception ex) when (IsExpectedParsingException(ex))
        {
            return null;
        }
    }

    #endregion

    #region Internal Test Helpers

    /// <summary>
    /// Gets the pattern strings for testing purposes.
    /// </summary>
    internal static class TestPatterns
    {
        internal const string BinaryPatternValue = BinaryPattern;
        internal const string BinaryWithBracketsPatternValue = BinaryWithBracketsPattern;
        internal const string EscapedBinaryPatternValue = EscapedBinaryPattern;
        internal const string EvalPatternValue = EvalPattern;
        internal const string HasBracketsValue = HasBrackets;
        internal const string HasSurroundingBracketsOnlyValue = HasSurroundingBracketsOnly;
        internal const string ArithmeticPatternValue = ArithmeticPattern;
        internal const string StringMethodPatternValue = StringMethodPattern;
        internal const string MathFunctionPatternValue = MathFunctionPattern;
    }

    /// <summary>
    /// Gets the expression builders for testing purposes.
    /// </summary>
    internal static class TestBuilders
    {
        internal static IReadOnlyDictionary<string, Func<MemberExpression, object, BinaryExpression>> BinaryExpressionBuilders => BinaryExpressionBuilder;
        internal static IReadOnlyDictionary<string, Func<Expression, Expression, Expression>> EvaluationExpressionBuilders => EvaluationExpressionBuilder;
        internal static IReadOnlyDictionary<string, Func<Expression, Expression, BinaryExpression>> ArithmeticExpressionBuilders => ArithmeticExpressionBuilder;
        internal static IReadOnlyDictionary<string, Func<MemberExpression, string, MethodCallExpression>> StringMethodBuilders => StringMethodBuilder;
        internal static IReadOnlyDictionary<string, Func<Expression[], MethodCallExpression?>> MathFunctionBuilders => MathFunctionBuilder;
    }

    /// <summary>
    /// Builds a member expression for potentially nested property access.
    /// </summary>
    /// <param name="parameterExpression">The parameter expression.</param>
    /// <param name="propertyPath">The property path (e.g., "Address.Street").</param>
    /// <returns>A member expression for the property path.</returns>
    private static MemberExpression BuildNestedPropertyExpression([NotNull] ParameterExpression parameterExpression, [NotNull] string propertyPath)
    {
        ArgumentNullException.ThrowIfNull(parameterExpression, nameof(parameterExpression));
        ArgumentNullException.ThrowIfNull(propertyPath, nameof(propertyPath));

        if (!propertyPath.Contains('.', StringComparison.Ordinal))
        {
            return Expression.PropertyOrField(parameterExpression, propertyPath);
        }

        var propertyParts = propertyPath.Split('.');
        Expression currentExpression = parameterExpression;

        foreach (var propertyName in propertyParts)
        {
            currentExpression = Expression.Property(currentExpression, propertyName);
        }

        return (MemberExpression)currentExpression;
    }

    /// <summary>
    /// Parses an operand which could be a property path or a literal value.
    /// </summary>
    /// <param name="operand">The operand string.</param>
    /// <param name="parameterExpression">The parameter expression.</param>
    /// <param name="targetType">The target type for conversion (if operand is literal).</param>
    /// <returns>An expression representing the operand.</returns>
    private static Expression? ParseOperand([NotNull] string operand, [NotNull] ParameterExpression parameterExpression, Type? targetType = null)
    {
        ArgumentNullException.ThrowIfNull(operand, nameof(operand));
        ArgumentNullException.ThrowIfNull(parameterExpression, nameof(parameterExpression));

        // Check if operand is a numeric literal
        if (decimal.TryParse(operand, out var numericValue))
        {
            if (targetType != null)
            {
                var convertedValue = ConvertValueToType(operand, targetType);
                return convertedValue != null ? Expression.Constant(convertedValue) : null;
            }
            return Expression.Constant(numericValue);
        }

        // Assume it's a property path
        try
        {
            return BuildNestedPropertyExpression(parameterExpression, operand);
        }
        catch (Exception ex) when (IsExpectedParsingException(ex))
        {
            return null;
        }
    }

    /// <summary>
    /// Builds an arithmetic expression from two operands and an operator.
    /// </summary>
    /// <param name="type">The entity type.</param>
    /// <param name="leftOperand">The left operand string.</param>
    /// <param name="operator">The arithmetic operator.</param>
    /// <param name="rightOperand">The right operand string.</param>
    /// <param name="parameterExpression">The parameter expression.</param>
    /// <returns>A lambda expression for the arithmetic operation.</returns>
    private static LambdaExpression? BuildArithmeticExpression([NotNull] Type type, [NotNull] string leftOperand, 
        [NotNull] string @operator, [NotNull] string rightOperand, [NotNull] ParameterExpression parameterExpression)
    {
        if (!ArithmeticExpressionBuilder.ContainsKey(@operator))
            return null;

        try
        {
            var leftExpression = ParseOperand(leftOperand, parameterExpression);
            var rightExpression = ParseOperand(rightOperand, parameterExpression);

            if (leftExpression == null || rightExpression == null)
                return null;

            var body = ArithmeticExpressionBuilder[@operator](leftExpression, rightExpression);
            var delegateType = typeof(Func<,>).MakeGenericType(type, body.Type);

            return Expression.Lambda(delegateType, body, parameterExpression);
        }
        catch (Exception ex) when (IsExpectedParsingException(ex))
        {
            return null;
        }
    }

    /// <summary>
    /// Builds a string method call expression.
    /// </summary>
    /// <param name="type">The entity type.</param>
    /// <param name="propertyName">The property name.</param>
    /// <param name="methodName">The string method name.</param>
    /// <param name="value">The value to pass to the method.</param>
    /// <param name="parameterExpression">The parameter expression.</param>
    /// <returns>A lambda expression for the string method call.</returns>
    private static LambdaExpression? BuildStringMethodExpression([NotNull] Type type, [NotNull] string propertyName,
        [NotNull] string methodName, [NotNull] string value, [NotNull] ParameterExpression parameterExpression)
    {
        if (!StringMethodBuilder.ContainsKey(methodName))
            return null;

        try
        {
            var props = GetTypeProperties(type);
            var prop = GetPropertyByName(props, propertyName);

            if (prop == null || prop.PropertyType != typeof(string))
                return null;

            var memberExpression = BuildNestedPropertyExpression(parameterExpression, propertyName);
            var methodCall = StringMethodBuilder[methodName](memberExpression, value);
            var delegateType = typeof(Func<,>).MakeGenericType(type, typeof(bool));

            return Expression.Lambda(delegateType, methodCall, parameterExpression);
        }
        catch (Exception ex) when (IsExpectedParsingException(ex))
        {
            return null;
        }
    }

    /// <summary>
    /// Builds a Math function call expression.
    /// </summary>
    /// <param name="type">The entity type.</param>
    /// <param name="functionName">The Math function name.</param>
    /// <param name="argumentsString">The comma-separated arguments string.</param>
    /// <param name="parameterExpression">The parameter expression.</param>
    /// <returns>A lambda expression for the Math function call.</returns>
    private static LambdaExpression? BuildMathFunctionExpression([NotNull] Type type, [NotNull] string functionName,
        [NotNull] string argumentsString, [NotNull] ParameterExpression parameterExpression)
    {
        if (!MathFunctionBuilder.ContainsKey(functionName))
            return null;

        try
        {
            // Parse arguments (simple comma split for now)
            var argumentStrings = argumentsString.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim())
                .ToArray();

            var argumentExpressions = new List<Expression>();
            
            foreach (var argString in argumentStrings)
            {
                var argExpression = ParseOperand(argString, parameterExpression);
                if (argExpression == null)
                    return null;
                argumentExpressions.Add(argExpression);
            }

            var methodCall = MathFunctionBuilder[functionName](argumentExpressions.ToArray());
            if (methodCall == null)
                return null;

            var delegateType = typeof(Func<,>).MakeGenericType(type, methodCall.Type);
            return Expression.Lambda(delegateType, methodCall, parameterExpression);
        }
        catch (Exception ex) when (IsExpectedParsingException(ex))
        {
            return null;
        }
    }

    #endregion

    #region Private Helper Methods

    /// <summary>
    /// Determines if a query string is invalid (null, empty, whitespace, or only brackets).
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsInvalidQuery([AllowNull] string? query) 
        => string.IsNullOrWhiteSpace(query) || IsNullOrBracketsOnly(query);

    /// <summary>
    /// Checks if a string contains only brackets and whitespace.
    /// </summary>
    private static bool IsNullOrBracketsOnly([AllowNull] string? s)
    {
        if (string.IsNullOrWhiteSpace(s)) 
            return true;

        foreach (char c in s)
            if (!char.IsWhiteSpace(c) && c != '(' && c != ')')
                return false;

        return true;
    }

    /// <summary>
    /// Determines if an exception is expected during parsing and should be handled gracefully.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsExpectedParsingException(Exception ex)
    {
        return ex is FormatException 
            || ex is InvalidCastException 
            || ex is OverflowException 
            || ex is ArgumentException
            || ex is InvalidOperationException;
    }

    /// <summary>
    /// Removes all outer brackets and whitespace from a string.
    /// </summary>
    private static string TrimAllOuterBracketsAndWhitespace([NotNull] string s)
    {
        if (string.IsNullOrWhiteSpace(s)) 
            return string.Empty;

        s = s.Trim();

        while (s.Length >= 2 && s[0] == '(' && s[^1] == ')')
        {
            var inner = s[1..^1].Trim();

            // Only trim if the inner string is strictly shorter (prevents infinite loop)
            if (inner.Length < s.Length - 1)
                s = inner;
            else
                break;
        }

        return s;
    }

    /// <summary>
    /// Attempts to convert a string value to the specified type.
    /// </summary>
    private static object? ConvertValueToType([NotNull] string value, [NotNull] Type targetType)
    {
        try
        {
            // Handle nullable types
            var underlyingType = Nullable.GetUnderlyingType(targetType) ?? targetType;
            
            // Handle string types directly
            if (underlyingType == typeof(string))
                return value;

            return Convert.ChangeType(value, underlyingType, System.Globalization.CultureInfo.InvariantCulture);
        }
        catch (Exception ex) when (IsExpectedParsingException(ex))
        {
            return null;
        }
    }

    /// <summary>
    /// Core worker method for building expression trees from string queries.
    /// </summary>
    private static LambdaExpression? BuildBinaryTreeExpressionWorker([NotNull] Type type, [NotNull] string query, [NotNull] ParameterExpression parameterExpression)
    {
        var q = TrimAllOuterBracketsAndWhitespace(query);

        if (IsInvalidQuery(q))
            return null;

        // Handle surrounding brackets only
        var surroundingBracketsMatch = GetCachedRegexMatch(q, HasSurroundingBracketsOnly);

        if (surroundingBracketsMatch.Success)
        {
            var inner = q[1..^1].Trim();

            inner = TrimAllOuterBracketsAndWhitespace(inner);

            if (IsInvalidQuery(inner) || inner == q)
                return null;

            return BuildBinaryTreeExpressionWorker(type, inner, parameterExpression);
        }

        // Handle binary operations
        var binaryOperationMatch = GetMatch(q, BinaryPattern, EscapedBinaryPattern, BinaryWithBracketsPattern);

        if (binaryOperationMatch?.Success == true)
            return BuildBinaryTreeExpression(type,
                binaryOperationMatch.Groups[LeftOperand].Value,
                binaryOperationMatch.Groups[Operator].Value,
                binaryOperationMatch.Groups[RightOperand].Value,
                parameterExpression);

        // Handle string method calls
        var stringMethodMatch = GetCachedRegexMatch(q, StringMethodPattern);
        if (stringMethodMatch.Success)
        {
            return BuildStringMethodExpression(type,
                stringMethodMatch.Groups["propertyName"].Value,
                stringMethodMatch.Groups["methodName"].Value,
                stringMethodMatch.Groups["value"].Value,
                parameterExpression);
        }

        // Handle arithmetic expressions
        var arithmeticMatch = GetCachedRegexMatch(q, ArithmeticPattern);
        if (arithmeticMatch.Success)
        {
            return BuildArithmeticExpression(type,
                arithmeticMatch.Groups[LeftOperand].Value,
                arithmeticMatch.Groups[Operator].Value,
                arithmeticMatch.Groups[RightOperand].Value,
                parameterExpression);
        }

        // Handle Math function calls
        var mathFunctionMatch = GetCachedRegexMatch(q, MathFunctionPattern);
        if (mathFunctionMatch.Success)
        {
            return BuildMathFunctionExpression(type,
                mathFunctionMatch.Groups["functionName"].Value,
                mathFunctionMatch.Groups["arguments"].Value,
                parameterExpression);
        }

        // Handle expressions with brackets
        var hasBrackets = GetCachedRegexMatch(q, HasBrackets);

        if (hasBrackets.Success)
        {
            var leftOp = hasBrackets.Groups[LeftOperand];
            var evaluatorFirst = hasBrackets.Groups[EvaluatorFirst];
            var brackets = hasBrackets.Groups[Brackets];
            var evaluatorSecond = hasBrackets.Groups[EvaluatorSecond];
            var rightOp = hasBrackets.Groups[RightOperand];

            if (leftOp.Value.IsNonEmpty() && rightOp.Value.IsNonEmpty() &&
                evaluatorFirst.Value.IsNonEmpty() && evaluatorSecond.Value.IsNonEmpty() && brackets.Value.IsNonEmpty())
            {
                var e = evaluatorSecond.Value;
                var firstEvaluatorIndex = q.IndexOf(e, StringComparison.Ordinal) + e.Length;

                return SendToEvaluation(type, leftOp.Value, e, q[firstEvaluatorIndex..], parameterExpression);
            }

            string leftQuery = GetValueOrDefault(leftOp.Value, brackets.Value);
            string evaluator = GetValueOrDefault(evaluatorFirst.Value, evaluatorSecond.Value);
            string rightQuery = GetValueOrDefault(rightOp.Value, brackets.Value);
            
            return SendToEvaluation(type, leftQuery, evaluator, rightQuery, parameterExpression);
        }

        // Handle evaluation expressions
        var evalMatch = GetCachedRegexMatch(q, EvalPattern);
        if (evalMatch.Success)
        {
            return SendToEvaluation(type, 
                evalMatch.Groups[LeftOperand].Value, 
                evalMatch.Groups[EvaluatorFirst].Value, 
                evalMatch.Groups[RightOperand].Value, 
                parameterExpression);
        }

        return null;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static string GetValueOrDefault(string source, string defaultValue)
            => source.IsNonEmpty() ? source : defaultValue;
    }

    /// <summary>
    /// Gets a regex match using cached compiled regex patterns.
    /// </summary>
    private static Match GetCachedRegexMatch([NotNull] string input, [NotNull] string pattern)
    {
        var regex = _regexCache.GetOrAdd(pattern, p => new Regex(p, RegexOptions.Compiled | RegexOptions.CultureInvariant));

        return regex.Match(input);
    }

    /// <summary>
    /// Gets the first successful regex match from multiple patterns.
    /// </summary>
    private static Match? GetMatch([NotNull] string q, params string[] patterns)
    {
        foreach (var pattern in patterns)
        {
            var match = GetCachedRegexMatch(q, pattern);

            if (match.Success)
                return match;
        }
        return null;
    }

    /// <summary>
    /// Gets a property descriptor by name, supporting nested properties.
    /// </summary>
    private static PropertyDescriptor? GetPropertyByName([NotNull] PropertyDescriptorCollection props, [NotNull] string fieldName)
    {
        if (!fieldName.Contains('.', StringComparison.Ordinal))
            return props.Find(fieldName, ignoreCase: true);

        var fieldNameProperty = fieldName.Split('.');

        if (fieldNameProperty.Length != 2)
            return null;

        return props
            .Find(fieldNameProperty[0], ignoreCase: true)?
            .GetChildProperties()
            .Find(fieldNameProperty[1], ignoreCase: true);
    }

    /// <summary>
    /// Gets cached property descriptors for a type.
    /// </summary>
    private static PropertyDescriptorCollection GetTypeProperties([NotNull] Type type) 
        => _typePropertyCollection.GetOrAdd(type, t => TypeDescriptor.GetProperties(t));

    /// <summary>
    /// Combines two expressions with a logical operator.
    /// </summary>
    private static LambdaExpression? SendToEvaluation([NotNull] Type type, [NotNull] string leftQuery, 
        [NotNull] string evaluator, [NotNull] string rightQuery, [NotNull] ParameterExpression parameterExpression)
    {
        if (!EvaluationExpressionBuilder.ContainsKey(evaluator))
            return null;

        var leftBinaryExpression = BuildBinaryTreeExpressionWorker(type, leftQuery, parameterExpression);
        var rightBinaryExpression = BuildBinaryTreeExpressionWorker(type, rightQuery, parameterExpression);

        if (leftBinaryExpression == null || rightBinaryExpression == null) 
            return null;

        try
        {
            var builder = EvaluationExpressionBuilder[evaluator];
            var body = builder(leftBinaryExpression.Body, rightBinaryExpression.Body);
            var delegateType = typeof(Func<,>).MakeGenericType(type, typeof(bool));

            return Expression.Lambda(delegateType, body, leftBinaryExpression.Parameters);
        }
        catch (Exception ex) when (IsExpectedParsingException(ex))
        {
            return null;
        }
    }

    #endregion
}
