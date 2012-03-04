// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RiaObservable.cs" author="Anton Dimkov">
//   Copyright (c) Anton Dimkov 2012. All rights reserved.
// </copyright>
// <summary>
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Reactive.RiaServices
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reactive;
    using System.Reactive.Linq;
    using System.Reactive.Subjects;
    using System.ServiceModel.DomainServices.Client;

    /// <summary>
    /// Translates method to observable.
    /// </summary>
    public static class RiaObservable
    {
        /// <summary>
        /// Loads the query.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="query">The query.</param>
        /// <param name="domainContext">The domain context.</param>
        /// <param name="loadBehavior">The load behavior.</param>
        /// <returns> Observable operation loading</returns>
        public static IObservable<IEnumerable<TEntity>> ToObservable<TEntity>(this EntityQuery<TEntity> query, DomainContext domainContext, LoadBehavior loadBehavior = LoadBehavior.KeepCurrent)
            where TEntity : Entity
        {
            return Observable.Defer(
                () =>
                {
                    var asyncSubject = new AsyncSubject<IEnumerable<TEntity>>();

                    domainContext.Load(
                        query,
                        loadBehavior,
                        loadOperation =>
                        {
                            if (loadOperation.HasError)
                            {
                                asyncSubject.OnError(new OperationException(loadOperation.Error, loadOperation));
                                loadOperation.MarkErrorAsHandled();
                            }
                            else
                            {
                                asyncSubject.OnNext(loadOperation.Entities);
                                asyncSubject.OnCompleted();
                            }
                        },
                        null);

                    return asyncSubject;
                });
        }

        /// <summary>
        /// Submits the changes observable.
        /// </summary>
        /// <param name="domainContext">The domain context parameter</param>
        /// <returns>
        /// Result of the SubmitChangesObservable method
        /// </returns>
        public static IObservable<EntityChangeSet> GetObservableSubmitChanges(this DomainContext domainContext)
        {
            return Observable.Defer(
                () =>
                {
                    var asyncSubject = new AsyncSubject<EntityChangeSet>();

                    domainContext.SubmitChanges(
                        submitOperation =>
                        {
                            if (submitOperation.HasError)
                            {
                                asyncSubject.OnError(new OperationException(submitOperation.Error, submitOperation));
                                submitOperation.MarkErrorAsHandled();
                            }
                            else
                            {
                                asyncSubject.OnNext(submitOperation.ChangeSet);
                                asyncSubject.OnCompleted();
                            }
                        },
                        null);

                    return asyncSubject;
                });
        }

        /// <summary>
        /// Gets the invoke operation observable.
        /// </summary>
        /// <typeparam name="TDomain">The type of the domain.</typeparam>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="domain">The domain.</param>
        /// <param name="invoke">The invoke expression.</param>
        /// <returns>Invoke observable.</returns>
        public static IObservable<TResult> GetObservableInvoke<TDomain, TResult>(this TDomain domain, Expression<Func<TDomain, InvokeOperation<TResult>>> invoke)
            where TDomain : DomainContext
            where TResult : Entity
        {
            return Observable.Defer(
                () =>
                    {
                        var subject = new AsyncSubject<TResult>();

                        Action<InvokeOperation<TResult>> userCallback = null;
                        Action<InvokeOperation<TResult>> callback = delegate(InvokeOperation<TResult> operation)
                        {
                            if (userCallback != null)
                            {
                                userCallback(operation);
                            }

                            if (operation.HasError)
                            {
                                subject.OnError(operation.Error);
                                operation.MarkErrorAsHandled();
                            }
                            else
                            {
                                subject.OnNext(operation.Value);
                                subject.OnCompleted();
                            }
                        };

                        userCallback = (Action<InvokeOperation<TResult>>)ParseAndInvokeExpression(domain, (MethodCallExpression)invoke.Body, callback);

                        return subject;
                    });
        }

        /// <summary>
        /// Gets the invoke operation observable.
        /// </summary>
        /// <typeparam name="TDomain">The type of the domain.</typeparam>
        /// <param name="domain">The domain.</param>
        /// <param name="invoke">The invoke.</param>
        /// <returns>Invoke observable.</returns>
        public static IObservable<Unit> GetObservableInvoke<TDomain>(this TDomain domain, Expression<Func<TDomain, InvokeOperation>> invoke)
            where TDomain : DomainContext
        {
            return Observable.Defer(
                () =>
                {
                    var subject = new AsyncSubject<Unit>();

                    Action<InvokeOperation> userCallback = null;
                    Action<InvokeOperation> callback = delegate(InvokeOperation operation)
                    {
                        if (userCallback != null)
                        {
                            userCallback(operation);
                        }

                        if (operation.HasError)
                        {
                            subject.OnError(operation.Error);
                            operation.MarkErrorAsHandled();
                        }
                        else
                        {
                            subject.OnNext(new Unit());
                            subject.OnCompleted();
                        }
                    };

                    userCallback = (Action<InvokeOperation>)ParseAndInvokeExpression(domain, (MethodCallExpression)invoke.Body, callback);

                    return subject;
                });
        }

        /// <summary>
        /// Performs server call and log results.
        /// </summary>
        /// <typeparam name="T">Type of observable</typeparam>
        /// <param name="observable">The observable.</param>
        public static void SilentSubscribe<T>(this IObservable<T> observable)
        {
            observable.Subscribe(
                x => Debug.WriteLine("Data loaded: {0}", x),
                ex => Debug.WriteLine(ex));
        }

        /// <summary>
        /// Performs server call and log results.
        /// </summary>
        /// <typeparam name="T">Type of observable</typeparam>
        /// <param name="observable">The observable.</param>
        /// <param name="onNext">The on next.</param>
        public static void SilentSubscribe<T>(this IObservable<T> observable, Action<T> onNext)
        {
            observable.Subscribe(onNext, ex => Debug.WriteLine(ex));
        }

        /// <summary>
        /// Performs server call and log results.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="observable">The observable.</param>
        /// <param name="onCompleted">The on completed.</param>
        public static void SilentSubscribe<T>(this IObservable<T> observable, Action onCompleted)
        {
            observable.Subscribe(x => Debug.WriteLine("Data loaded: {0}", x), ex => Debug.WriteLine(ex), onCompleted);
        }

        /// <summary>
        /// Parses the and invoke expression.
        /// </summary>
        /// <typeparam name="TDomain">The type of the domain.</typeparam>
        /// <param name="domain">The domain.</param>
        /// <param name="invokeBody">The invocation body.</param>
        /// <param name="callback">The callback.</param>
        /// <returns>User's callback.</returns>
        private static Delegate ParseAndInvokeExpression<TDomain>(TDomain domain, MethodCallExpression invokeBody, Delegate callback)
        {
            object userState = null;
            var methodCallExpression = invokeBody;
            var methodInfo = methodCallExpression.Method;
            var param = new List<object>();
            Delegate userCallback = null;

            param.AddRange(methodCallExpression.Arguments.Select(GetArgumentValue));

            // detect user's callback
            if (param.Any(x => x.GetType() == callback.GetType()))
            {
                Debug.WriteLine("Callback detected. Please use 'Do' or 'Subscribe' or other Rx method instead of callback.");
                userState = param.Last();
                param.Remove(userState);
                userCallback = (Delegate)param.Last();
                param.Remove(userCallback);
            }

            param.Add(callback);
            param.Add(userState);

            methodInfo.DeclaringType.GetMethods()
                .Where(x => x.Name == methodInfo.Name)
                .Single(x => x.GetParameters().Length == param.Count && x.GetParameters().Any(y => y.ParameterType == callback.GetType()))
                .Invoke(domain, param.ToArray());

            return userCallback;
        }

        /// <summary>
        /// Gets the expression argument value.
        /// </summary>
        /// <param name="element">The expression element.</param>
        /// <returns>The value.</returns>
        private static object GetArgumentValue(Expression element)
        {
            LambdaExpression l = Expression.Lambda(Expression.Convert(element, element.Type));
            return l.Compile().DynamicInvoke();
        }
    }
}