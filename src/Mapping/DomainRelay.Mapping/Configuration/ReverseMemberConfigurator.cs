using System.Linq.Expressions;
using DomainRelay.Mapping.Abstractions.Configuration;

namespace DomainRelay.Mapping.Configuration;

internal static class ReverseMemberConfigurator
{
    public static bool TryConfigureReverseMember<TForwardSource, TForwardDestination>(
        IMapExpression<TForwardDestination, TForwardSource> reverseMap,
        MemberMapDefinition memberDefinition)
    {
        if (memberDefinition.Ignored)
        {
            return TryConfigureIgnoredReverseMember(reverseMap, memberDefinition.DestinationMemberName);
        }

        if (memberDefinition.SourceExpression is null)
        {
            return false;
        }

        if (!TryExtractDirectSourceMemberName(memberDefinition.SourceExpression, out var sourceMemberName))
        {
            return false;
        }

        return TryConfigureDirectReverseMember(
            reverseMap,
            forwardSourceMemberName: sourceMemberName,
            forwardDestinationMemberName: memberDefinition.DestinationMemberName);
    }

    public static bool TryConfigureIgnoredReverseMember<TSource, TDestination>( 
        IMapExpression<TSource, TDestination> reverseMap,
        string memberName)
    {
        var destinationProperty = typeof(TDestination).GetProperty(memberName);
        if (destinationProperty is null || !destinationProperty.CanWrite)
        {
            return false;
        }

        var destinationParameter = Expression.Parameter(typeof(TDestination), "dest");
        var destinationMember = Expression.Property(destinationParameter, destinationProperty);
        var destinationLambda = Expression.Lambda(destinationMember, destinationParameter);

        var method = typeof(ReverseMemberConfigurator)
            .GetMethod(nameof(ApplyIgnore), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!
            .MakeGenericMethod(typeof(TSource), typeof(TDestination), destinationProperty.PropertyType);

        method.Invoke(null, new object[] { reverseMap, destinationLambda });
        return true;
    }

    public static bool TryConfigureDirectReverseMember<TSource, TDestination>(
        IMapExpression<TSource, TDestination> reverseMap,
        string forwardSourceMemberName,
        string forwardDestinationMemberName)
    {
        var reverseDestinationProperty = typeof(TDestination).GetProperty(forwardSourceMemberName);
        var reverseSourceProperty = typeof(TSource).GetProperty(forwardDestinationMemberName);

        if (reverseDestinationProperty is null || reverseSourceProperty is null)
        {
            return false;
        }

        if (!reverseDestinationProperty.CanWrite || !reverseSourceProperty.CanRead)
        {
            return false;
        }

        var sourceParameter = Expression.Parameter(typeof(TSource), "src");
        var sourceMember = Expression.Property(sourceParameter, reverseSourceProperty);
        var sourceLambda = Expression.Lambda(sourceMember, sourceParameter);

        var destinationParameter = Expression.Parameter(typeof(TDestination), "dest");
        var destinationMember = Expression.Property(destinationParameter, reverseDestinationProperty);
        var destinationLambda = Expression.Lambda(destinationMember, destinationParameter);

        var method = typeof(ReverseMemberConfigurator)
            .GetMethod(nameof(ApplyMapFrom), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!
            .MakeGenericMethod(typeof(TSource), typeof(TDestination), reverseDestinationProperty.PropertyType);

        method.Invoke(null, new object[] { reverseMap, destinationLambda, sourceLambda });
        return true;
    }

    private static bool TryExtractDirectSourceMemberName(
        LambdaExpression sourceExpression,
        out string memberName)
    {
        memberName = string.Empty;

        Expression body = sourceExpression.Body;
        if (body is UnaryExpression unary &&
            unary.NodeType is ExpressionType.Convert or ExpressionType.ConvertChecked)
        {
            body = unary.Operand;
        }

        if (body is not MemberExpression memberExpression)
        {
            return false;
        }

        if (memberExpression.Expression != sourceExpression.Parameters[0])
        {
            return false;
        }

        memberName = memberExpression.Member.Name;
        return true;
    }

    private static void ApplyMapFrom<TSource, TDestination, TMember>(
        IMapExpression<TSource, TDestination> map,
        Expression<Func<TDestination, TMember>> destination,
        Expression<Func<TSource, TMember>> source)
    {
        map.ForMember(destination, o => o.MapFrom(source));
    }

    private static void ApplyIgnore<TSource, TDestination, TMember>(
        IMapExpression<TSource, TDestination> map,
        Expression<Func<TDestination, TMember>> destination)
    {
        map.Ignore(destination);
    }
}