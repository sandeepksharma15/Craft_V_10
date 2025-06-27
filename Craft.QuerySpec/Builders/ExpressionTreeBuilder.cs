using System.Collections.Concurrent;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Text.RegularExpressions;

namespace Craft.QuerySpec;

public class ExpressionTreeBuilder
{
    protected const string BinaryPattern = "^" + BinaryPatternCore + "$";

    protected const string BinaryWithBracketsPattern = @"^\s*\(" + BinaryPatternCore + @"\)\s*$";

    protected const string EscapedBinaryPattern = @"^\s*(\""\s*(?'leftOperand'.*)\s*\""\s*(?'operator'(==|!=|<|<=|>|>=))\s*(?'rightOperand'\w+)|(?'leftOperand'\w+)\s*(?'operator'(==|!=|<|<=|>|>=))\s*\""\s*(?'rightOperand'.*)\s*\""\s*)\s*$";

    protected const string EvalPattern = @"^(?'leftOperand'\S{1,}\s*(==|!=|<|<=|>|>=)\s*\S{1,})\s*(?'evaluator_first'((\|{1,2})|(\&{1,2})))\s*(?'rightOperand'.*)\s*$";

    protected const string HasBrackets = @"^\s*(?'leftOperand'[^\|\&]*)\s*(?'evaluator_first'((\|)*|(\&)*))\s*(?'brackets'(\(\s*(.*)s*\)))\s*(?'evaluator_second'((\|{1,2})|(\&{1,2}))*)\s*(?'rightOperand'.*)\s*$";

    protected const string HasSurroundingBracketsOnly = @"^\s*\(\s*(?'leftOperand'([^\(\)])+)\s*\)\s*$";

    protected static readonly IReadOnlyDictionary<string, Func<MemberExpression, object, BinaryExpression>> BinaryExpressionBuilder =
        new Dictionary<string, Func<MemberExpression, object, BinaryExpression>>
        {
                        {"==", (me, value) => Expression.MakeBinary(ExpressionType.Equal, me, Expression.Constant(value))},
                        {"!=", (me, value) => Expression.MakeBinary(ExpressionType.NotEqual, me, Expression.Constant(value))},
                        {">", (me, value) => Expression.MakeBinary(ExpressionType.GreaterThan, me, Expression.Constant(value))},
                        {">=", (me, value) => Expression.MakeBinary(ExpressionType.GreaterThanOrEqual, me, Expression.Constant(value))},
                        {"<", (me, value) => Expression.MakeBinary(ExpressionType.LessThan, me, Expression.Constant(value))},
                        {"<=", (me, value) => Expression.MakeBinary(ExpressionType.LessThanOrEqual, me, Expression.Constant(value))},
        };

    protected static readonly IReadOnlyDictionary<string, Func<Expression, Expression, Expression>> EvaluationExpressionBuilder =
    new Dictionary<string, Func<Expression, Expression, Expression>>
    {
            {"&", (left, right) => Expression.And(left,right)},
            {"&&", (left, right) => Expression.AndAlso(left,right)},
            {"|", (left, right) => Expression.Or(left,right)},
            {"||", (left, right) => Expression.OrElse(left,right)},
    };

    private const string BinaryPatternCore = @"\s*(?'leftOperand'\w+)\s*(?'operator'(==|!=|<|<=|>|>=))\s*(?'rightOperand'\w+)\s*";

    private const string Brackets = "brackets";

    private const string EvaluatorFirst = "evaluator_first";

    private const string EvaluatorSecond = "evaluator_second";

    private const string LeftOperand = "leftOperand";

    private const string Operator = "operator";

    private const string RightOperand = "rightOperand";

    private static readonly ConcurrentDictionary<Type, PropertyDescriptorCollection> _typePropertyCollection
                                                                    = new();

    private static string TrimAllOuterBracketsAndWhitespace(string s)
    {
        if (string.IsNullOrWhiteSpace(s)) return string.Empty;
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

    public static Expression<Func<T, bool>>? BuildBinaryTreeExpression<T>(string? query)
    {
        if (string.IsNullOrWhiteSpace(query) || IsNullOrBracketsOnly(query))
            return null;
        var trimmed = TrimAllOuterBracketsAndWhitespace(query);
        if (string.IsNullOrEmpty(trimmed) || IsNullOrBracketsOnly(trimmed))
            return null;
        var exp = BuildBinaryTreeExpression(typeof(T), trimmed);
        return exp as Expression<Func<T, bool>>;
    }

    public static LambdaExpression? BuildBinaryTreeExpression(Type type, string? query)
    {
        if (string.IsNullOrWhiteSpace(query) || IsNullOrBracketsOnly(query))
            return null;
        var trimmed = TrimAllOuterBracketsAndWhitespace(query);
        if (string.IsNullOrEmpty(trimmed) || IsNullOrBracketsOnly(trimmed))
            return null;
        var pe = Expression.Parameter(type, "x");
        return BuildBinaryTreeExpressionWorker(type, trimmed, pe);
    }

    public static LambdaExpression? BuildBinaryTreeExpression(Type type, string propertyName, string @operator, string value, ParameterExpression parameterExpression)
    {
        if (!propertyName.IsNonEmpty() || !@operator.IsNonEmpty() || !value.IsNonEmpty())
            return null;

        var props = GetTypeProperties(type);

        var prop = GetPropertyByName(props, propertyName);

        if (prop == null) return null;

        var me = Expression.PropertyOrField(parameterExpression, propertyName);

        try
        {
            object v = Convert.ChangeType(value, me.Type);
            var body = BinaryExpressionBuilder[@operator](me, v);
            var delegateType = typeof(Func<,>).MakeGenericType(type, typeof(bool));
            return Expression.Lambda(delegateType, body, parameterExpression);
        }
        catch
        {
            return null;
        }
    }

    public static Func<T, bool>? BuildBinaryTreeExpression<T>(IDictionary<string, string> filter)
    {
        var exp = ToExpression<T>(filter);

        return exp?.Compile();
    }

    public static Expression<Func<T, bool>>? ToExpression<T>(IDictionary<string, string> filter)
    {
        if (filter?.Any() != true) return null;

        var props = GetTypeProperties(typeof(T));
        var pe = Expression.Parameter(typeof(T));

        var expCol = new List<BinaryExpression>();

        foreach (var kvp in filter)
        {
            var fieldName = kvp.Key;

            var prop = GetPropertyByName(props, fieldName);

            if (prop == null) return null;

            var me = Expression.PropertyOrField(pe, fieldName);

            object value;
            try
            {
                value = Convert.ChangeType(kvp.Value, me.Type);
            }
            catch
            {
                return null;
            }
            var be = Expression.Equal(me, Expression.Constant(value));
            expCol.Add(be);
        }

        var allBinaryExpressions = expCol.Aggregate((left, right) 
            => Expression.AndAlso(left, right));

        return Expression.Lambda<Func<T, bool>>(allBinaryExpressions, [pe]);
    }

    private static bool IsNullOrBracketsOnly(string s)
    {
        if (string.IsNullOrWhiteSpace(s)) return true;
        foreach (char c in s)
            if (!char.IsWhiteSpace(c) && c != '(' && c != ')')
                return false;
        return true;
    }

    private static LambdaExpression? BuildBinaryTreeExpressionWorker(Type type, string query, ParameterExpression parameterExpression)
    {
        var q = TrimAllOuterBracketsAndWhitespace(query);
        if (string.IsNullOrEmpty(q) || IsNullOrBracketsOnly(q))
            return null;

        var m = Regex.Match(q, HasSurroundingBracketsOnly);
        if (m.Success)
        {
            var inner = q[1..^1].Trim();
            inner = TrimAllOuterBracketsAndWhitespace(inner);
            if (string.IsNullOrEmpty(inner) || IsNullOrBracketsOnly(inner) || inner == q)
                return null;
            return BuildBinaryTreeExpressionWorker(type, inner, parameterExpression);
        }

        var binaryOperationMatch = GetMatch(q, BinaryPattern, EscapedBinaryPattern, BinaryWithBracketsPattern);
        if (binaryOperationMatch?.Success == true)
            return BuildBinaryTreeExpression(type,
                binaryOperationMatch.Groups[LeftOperand].Value,
                binaryOperationMatch.Groups[Operator].Value,
                binaryOperationMatch.Groups[RightOperand].Value,
                parameterExpression);

        var hasBrackets = GetMatch(q, HasBrackets); //DO NOT CHANGE EXPRESSIONS ORDER!!!
        if (hasBrackets?.Success == true)
        {
            Group leftOp = hasBrackets.Groups[LeftOperand],
                evaluatorFirst = hasBrackets.Groups[EvaluatorFirst],
                brackets = hasBrackets.Groups[Brackets],
                evaluatorSecond = hasBrackets.Groups[EvaluatorSecond],
                rightOp = hasBrackets.Groups[RightOperand];

            if (leftOp.Value.IsNonEmpty() && rightOp.Value.IsNonEmpty() &&
                evaluatorFirst.Value.IsNonEmpty() && evaluatorSecond.Value.IsNonEmpty() && brackets.Value.IsNonEmpty())
            {
                var e = evaluatorSecond.Value;
                var firstEvaluatorIndex = q.IndexOf(e) + e.Length;
                return SendToEvaluation(type, leftOp.Value, e, q[firstEvaluatorIndex..], parameterExpression);
            }

            string leftQuery = GetValueOrReplaceIfEmptyOrNull(leftOp.Value, brackets.Value),
                evaluator = GetValueOrReplaceIfEmptyOrNull(evaluatorFirst.Value, evaluatorSecond.Value),
                rightQuery = GetValueOrReplaceIfEmptyOrNull(rightOp.Value, brackets.Value);
            return SendToEvaluation(type, leftQuery, evaluator, rightQuery, parameterExpression);
        }

        var evalMatch = Regex.Match(q, EvalPattern);
        if (evalMatch.Success)
            return SendToEvaluation(type, evalMatch.Groups[LeftOperand].Value, evalMatch.Groups[EvaluatorFirst].Value, evalMatch.Groups[RightOperand].Value, parameterExpression);
        return null;
        static string GetValueOrReplaceIfEmptyOrNull(string source, string onEmptyOrNullValue)
            => source.IsNonEmpty() ? source : onEmptyOrNullValue;
    }

    private static Match? GetMatch(string q, params string[] patterns)
    {
        for (int i = 0; i < patterns.Length; i++)
        {
            var m = Regex.Match(q, patterns[i]);

            if (m.Success)
                return m;
        }
        return null;
    }

    private static PropertyDescriptor? GetPropertyByName(PropertyDescriptorCollection props, string fieldName)
    {
        if (!fieldName.Contains('.'))
            return props.Find(fieldName, true);

        var fieldNameProperty = fieldName.Split('.');

        return props
            .Find(fieldNameProperty[0], true)?
            .GetChildProperties().Find(fieldNameProperty[1], true);
    }

    private static PropertyDescriptorCollection GetTypeProperties(Type type)
    {
        if (!_typePropertyCollection.TryGetValue(type, out PropertyDescriptorCollection? value))
        {
            value = TypeDescriptor.GetProperties(type);
            _typePropertyCollection.TryAdd(type, value);
        }

        return value;
    }

    private static LambdaExpression? SendToEvaluation(Type type, string leftQuery, string evaluator, string rightQuery, ParameterExpression parameterExpression)
    {
        var leftBinaryExpression = BuildBinaryTreeExpressionWorker(type, leftQuery, parameterExpression);
        var rightBinaryExpression = BuildBinaryTreeExpressionWorker(type, rightQuery, parameterExpression);

        if (leftBinaryExpression == null || rightBinaryExpression == null) return null;

        var builder = EvaluationExpressionBuilder[evaluator];
        var body = builder(leftBinaryExpression.Body, rightBinaryExpression.Body);

        var delegateType = typeof(Func<,>).MakeGenericType(type, typeof(bool));

        return Expression.Lambda(delegateType, body, leftBinaryExpression.Parameters);
    }
}
