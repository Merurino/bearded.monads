﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Bearded.Monads.Syntax;

namespace Bearded.Monads
{
    public static class TryExtensions
    {
        public static Try<C> SelectMany<A, B, C>(this Try<A> ta, Func<A, Try<B>> map, Func<A, B, C> selector)
        {
            try
            {
                if (ta.IsError) return ta.AsError().Value;

                var a = ta.AsSuccess().Value;

                var tb = map(a);

                if (tb.IsError) return tb.AsError().Value;

                var b = tb.AsSuccess().Value;

                return selector(a, b);
            }
            catch (Exception e)
            {
                return e;
            }
        }

        public async static Task<Try<C>> SelectManyAsync<A, B, C>(this Try<A> ta, Func<A, Task<Try<B>>> map, Func<A, B, Task<C>> selector)
        {
            try
            {
                if (ta.IsError) return ta.AsError().Value;

                var a = ta.AsSuccess().Value;

                var tb = await map(a);

                if (tb.IsError) return tb.AsError().Value;

                var b = tb.AsSuccess().Value;

                return await selector(a, b);
            }
            catch (Exception e)
            {
                return e;
            }
        }

        public static Try<B> SelectMany<A, B>(this Try<A> ta, Func<A, Try<B>> map)
        {
            try
            {
                if (ta.IsError) return ta.AsError().Value;

                var a = ta.AsSuccess().Value;

                return map(a);
            }
            catch (Exception e)
            {
                return e;
            }
        }

        public static async Task<Try<B>> SelectManyAsync<A, B>(this Try<A> ta, Func<A, Task<Try<B>>> map)
        {
            try
            {
                if (ta.IsError) return ta.AsError().Value;

                var a = ta.AsSuccess().Value;

                return await map(a);
            }
            catch (Exception e)
            {
                return e;
            }
        }

        public static OptionUnsafe<A> AsOptionUnsafe<A>(this Try<A> either, Action<Exception> callback)
            => either.Else(OptionUnsafe<A>.Return, e =>
            {
                callback(e);
                return OptionUnsafe<A>.None;
            });

        public static Option<A> AsOption<A>(this Try<A> either, Action<Exception> callback)
            => either.Else(Option<A>.Return, e =>
            {
                callback(e);
                return Option<A>.None;
            });

        public static Either<A, Exception> AsEither<A>(this Try<A> either)
            => either.AsEither(id);

        public static Either<A, Error> AsEither<A, Error>(this Try<A> either, Func<Exception, Error> errorMap)
            => either.Else(Either<A, Error>.Create, ex => errorMap(ex));

        public static Try<A> AsTry<A>(this A thing) => Try<A>.Create(thing);

        public static Try<A> AsTry<A>(this Option<A> option, Func<Exception> errorCallback)
            => option.Select(Try<A>.Create).Else(() => Try<A>.Create(errorCallback()));

        public static Try<A> AsTry<A>(this Option<A> option, Func<string> errorCallback)
            => option.AsTry(() => new Exception(errorCallback()));

        public static Try<A> AsTry<A>(this Option<A> option, string message)
            => option.AsTry(() => message);

        public static Try<A> AsTry<A>(this Try<Option<A>> option, Func<Exception> errorCallback)
            => option.SelectMany(o => o.Select(Try<A>.Create).Else(() => Try<A>.Create(errorCallback())));

        public static Try<A> AsTry<A>(this Try<Option<A>> option, Func<string> errorCallback)
            => option.AsTry(() => new Exception(errorCallback()));

        public static Try<A> AsTry<A>(this Try<Option<A>> option, string message)
            => option.AsTry(() => message);

        public static Try<A> AsTry<A, Error>(this Either<A, Error> either, Func<Error, Exception> errorTransform)
            => either.Unify(Try<A>.Create, e => Try<A>.Create(errorTransform(e)));

        public static Try<A> AsTry<A>(this Either<A, Exception> either)
            => either.Unify(Try<A>.Create, Try<A>.Create);

        public static Try<B> Select<A, B>(this Try<A> either, Func<A, B> projector)
        {
            return either.Map(projector);
        }

        public static Try<A> Where<A>(this Try<A> either,
            Predicate<A> predicate, Func<Exception> errorCallback)
        {
            if (either.IsError) return either;
            if (predicate(either.AsSuccess().Value)) return either;
            return errorCallback();
        }

        public static Try<A> Where<A>(this Try<A> either,
            Predicate<A> predicate, Func<String> errorCallback)
            => either.Where(predicate, () => new Exception(errorCallback()));

        public static Try<A> Where<A>(this Try<A> either,
            Predicate<A> predicate, String message)
            => either.Where(predicate, () => message);

        public static Try<A> WhereNot<A>(this Try<A> incoming,
            Predicate<A> notPredicate, Func<Exception> errorCallback)
            => incoming.Where(x => !notPredicate(x), errorCallback);

        public static Try<A> WhereNot<A>(this Try<A> incoming,
            Predicate<A> notPredicate, Func<String> errorCallback)
            => incoming.WhereNot(notPredicate, () => new Exception(errorCallback()));

        public static Try<A> WhereNot<A>(this Try<A> incoming,
            Predicate<A> notPredicate, String message)
            => incoming.WhereNot(notPredicate, () => message);

        public static Result Else<A, Result>(this Try<A> either,
            Func<A, Result> happy,
            Func<Exception, Result> sad)
        {
            return either.IsSuccess
                ? happy(either.AsSuccess().Value)
                : sad(either.AsError().Value);
        }

        public static Try<Result> SafeCallback<Incoming, Result>(this Incoming item,
            Func<Incoming, Result> callback)
        {
            try
            {
                return callback(item);
            }
            catch (Exception e)
            {
                return e;
            }
        }

        public static Result Unify<Success, Result>(this Try<Success> either,
            Func<Success, Result> successFunc, Func<Exception, Result> errorFunc)
        {
            if (either.IsSuccess) return successFunc(either.AsSuccess().Value);
            return errorFunc(either.AsError().Value);
        }

        public static Success Else<Success>(this Try<Success> either,
            Func<Exception, Success> callback)
        {
            if (either.IsError) return callback(either.AsError().Value);

            return either.AsSuccess().Value;
        }

        public static Try<A> WhenSuccess<A>(this Try<A> either,
            Action<A> callbackForSuccess)
        {
            if (either.IsSuccess)
            {
                callbackForSuccess(either.AsSuccess().Value);
            }

            return either;
        }

        public static Try<A> WhenError<A>(this Try<A> either,
            Action<Exception> callbackForError)
        {
            if (either.IsError)
            {
                callbackForError(either.AsError().Value);
            }

            return either;
        }

        public static A ElseThrow<A>(this Try<A> either)
        {
            return either.Else(exc => { throw exc; });
        }

        public static Try<A> Flatten<A>(
            this Try<Try<A>> ee)
            => ee.SelectMany(id);

        public static Try<IEnumerable<A>> Sequence<A>(
            this IEnumerable<Try<A>> incoming)
            => incoming.Traverse(id);

    }
}
