using NSubstitute;
using NSubstitute.Core;

namespace Tests.TestSupport;

public static class SubstituteExtensions
{
    public static ConfiguredCall Returns<T>(
        this T value,
        Func<T> func
    )
    {
        return value.Returns(
            _ =>
            {
                T result = func();

                return result;
            }
        );
    }

    public static ConfiguredCall Returns<T, TArg1>(
        this T value,
        Func<TArg1, T> func
    )
    {
        return value.Returns(
            callInfo =>
            {
                TArg1 arg1 = callInfo.ArgAt<TArg1>(0);

                T result = func(arg1);

                return result;
            }
        );
    }

    public static ConfiguredCall Returns<T, TArg1, TArg2>(
        this T value,
        Func<TArg1, TArg2, T> func
    )
    {
        return value.Returns(
            callInfo =>
            {
                TArg1 arg1 = callInfo.ArgAt<TArg1>(0);
                TArg2 arg2 = callInfo.ArgAt<TArg2>(1);

                T result = func(arg1, arg2);

                return result;
            }
        );
    }

    public static ConfiguredCall Returns<T, TArg1, TArg2, TArg3>(
        this T value,
        Func<TArg1, TArg2, TArg3, T> func
    )
    {
        return value.Returns(
            callInfo =>
            {
                TArg1 arg1 = callInfo.ArgAt<TArg1>(0);
                TArg2 arg2 = callInfo.ArgAt<TArg2>(1);
                TArg3 arg3 = callInfo.ArgAt<TArg3>(2);

                T result = func(arg1, arg2, arg3);

                return result;
            }
        );
    }

    public static ConfiguredCall Returns<T, TArg1, TArg2, TArg3, TArg4>(
        this T value,
        Func<TArg1, TArg2, TArg3, TArg4, T> func
    )
    {
        return value.Returns(
            callInfo =>
            {
                TArg1 arg1 = callInfo.ArgAt<TArg1>(0);
                TArg2 arg2 = callInfo.ArgAt<TArg2>(1);
                TArg3 arg3 = callInfo.ArgAt<TArg3>(2);
                TArg4 arg4 = callInfo.ArgAt<TArg4>(3);

                T result = func(arg1, arg2, arg3, arg4);

                return result;
            }
        );
    }

    public static ConfiguredCall Returns<T, TArg1, TArg2, TArg3, TArg4, TArg5>(
        this T value,
        Func<TArg1, TArg2, TArg3, TArg4, TArg5, T> func
    )
    {
        return value.Returns(
            callInfo =>
            {
                TArg1 arg1 = callInfo.ArgAt<TArg1>(0);
                TArg2 arg2 = callInfo.ArgAt<TArg2>(1);
                TArg3 arg3 = callInfo.ArgAt<TArg3>(2);
                TArg4 arg4 = callInfo.ArgAt<TArg4>(3);
                TArg5 arg5 = callInfo.ArgAt<TArg5>(4);

                T result = func(arg1, arg2, arg3, arg4, arg5);

                return result;
            }
        );
    }
}
