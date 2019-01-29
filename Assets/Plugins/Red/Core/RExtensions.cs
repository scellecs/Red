﻿#if CSHARP_7_OR_LATER || (UNITY_2018_3_OR_NEWER && (NET_STANDARD_2_0 || NET_4_6))
namespace Red {
    using System;
    using System.Collections.Generic;
    using JetBrains.Annotations;
    using UniRx;
    using UniRx.Async;
    using UnityEngine;

    public static class RContractExtension {
        /// <summary>
        ///     Return instance <see cref="RContract{T0}" /> or null if it doesn't exists for current object
        /// </summary>
        /// <typeparam name="T">Type of contract</typeparam>
        /// <param name="component">The component from which the gameObject is taken</param>
        /// <param name="identifier">Unique identifier for contract</param>
        [CanBeNull]
        public static T TryGet<T>(this Component component, string identifier = "") where T : RContract<T>, new() {
            return RContract<T>.TryGet(component.gameObject, identifier);
        }
        
        /// <summary>
        ///     Return instance <see cref="RContract{T0}" /> or null if it doesn't exists for current object
        /// </summary>
        /// <typeparam name="T">Type of contract</typeparam>
        /// <param name="gameObject">The gameObject that acts as an anchor</param>
        /// <param name="identifier">Unique identifier for contract</param>
        [CanBeNull]
        public static T TryGet<T>(this GameObject gameObject, string identifier = "") where T : RContract<T>, new() {
            return RContract<T>.TryGet(gameObject, identifier);
        }
        
        /// <summary>
        ///     Search for the instance of <see cref="RContract{T0}" />
        /// </summary>
        /// <typeparam name="T">Type of contract</typeparam>
        /// <param name="gameObject">The gameObject that acts as an anchor</param>
        /// <param name="contract">Instance of <see cref="RContract{T0}" /> or null</param>
        /// <param name="identifier">Unique identifier for contract</param>
        /// <returns>True if found, false if instance is null</returns>
        public static bool TryGet<T>(this GameObject gameObject, [CanBeNull] out T contract, string identifier = "") where T : RContract<T>, new() {
            contract = RContract<T>.TryGet(gameObject, identifier);
            return contract != null;
        }        
        
        /// <summary>
        ///     Search for the instance of <see cref="RContract{T0}" />
        /// </summary>
        /// <typeparam name="T">Type of contract</typeparam>
        /// <param name="component">The component from which the gameObject is taken</param>
        /// <param name="contract">Instance of <see cref="RContract{T0}" /> or null</param>
        /// <param name="identifier">Unique identifier for contract</param>
        /// <returns>True if found, false if instance is null</returns>
        public static bool TryGet<T>(this Component component, [CanBeNull] out T contract, string identifier = "") where T : RContract<T>, new() {
            contract = RContract<T>.TryGet(component.gameObject, identifier);
            return contract != null;
        }

        /// <summary>
        ///     Return instance <see cref="RContract{T0}" /> or create it, if it doesn't exists on current gameObject
        /// </summary>
        /// <typeparam name="T">Type of contract</typeparam>
        /// <param name="component">The component from which the gameObject is taken</param>
        /// <param name="identifier">Unique identifier for contract</param>
        public static T GetOrCreate<T>(this Component component, string identifier = "") where T : RContract<T>, new() {
            return RContract<T>.GetOrCreate(component.gameObject, identifier);
        }

        /// <summary>
        ///     Return instance <see cref="RContract{T0}" /> or create it, if it doesn't exists on current gameObject
        /// </summary>
        /// <typeparam name="T">Type of contract</typeparam>
        /// <param name="gameObject">The gameObject that acts as an anchor</param>
        /// <param name="identifier">Unique identifier for contract</param>
        public static T GetOrCreate<T>(this GameObject gameObject, string identifier = "")
            where T : RContract<T>, new() {
            return RContract<T>.GetOrCreate(gameObject, identifier);
        }

        /// <summary>
        ///     Register contract in container
        /// </summary>
        /// <typeparam name="T">Type of contract</typeparam>
        public static void RegisterIn<T>(this T contract, RContainer container) where T : RContract {
            container.Register(contract);
        }
    }

    public static class RLinqExtension {
        /// <summary>
        ///     Default iteration in functional style.
        /// </summary>
        /// <param name="source">Some Enumerable</param>
        /// <param name="action">Functor</param>
        /// <typeparam name="T">Type of Enumerable</typeparam>
        public static void ForEach<T>(this IEnumerable<T> source, Action<T> action) {
            foreach (var element in source) {
                action(element);
            }
        }

        /// <summary>
        ///     Sequential iteration for async API.
        /// </summary>
        /// <param name="source">Some Enumerable</param>
        /// <param name="action">Functor</param>
        /// <typeparam name="T">Type of Enumerable</typeparam>
        public static async UniTask ForEachAsync<T>(this IEnumerable<T> source, Func<T, UniTask> action) {
            foreach (var element in source) {
                await action(element);
            }
        }
    }

    public static class RExtensions {
        /// <summary>
        ///     Awaiter for TimeSpan.
        /// </summary>
        /// <param name="timeSpan">Any TimeSpan</param>
        /// <returns>Awaiter</returns>
        public static AsyncSubject<long> GetAwaiter(this TimeSpan timeSpan) {
            return Observable.Timer(timeSpan).GetAwaiter();
        }

        /// <summary>
        ///     Add contract to GameObject. Contract will be disposed when GameObject is destroyed.
        /// </summary>
        /// <param name="disposable"></param>
        /// <param name="contract"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T AddTo<T>(this T disposable, RContract contract) where T : IDisposable {
            if (contract?.Target is GameObject o) {
                disposable.AddTo(o);
                return disposable;
            }

            disposable.Dispose();
            return disposable;
        }

        public static IObservable<TR> Systemize<T, TR>(this IObservable<T> observable, ISystem<T, TR> system) {
            observable.Subscribe(system);
            return system;
        }

        public static IObservable<TR> Systemize<TSystem, T, TR>(this IObservable<T> observable)
            where TSystem : ISystem<T, TR>, new() => observable.Systemize(new TSystem());

    }
}


namespace UniRx {
    using System;
    using System.Collections.Generic;
    using System.Threading;

    public interface IReactiveOperation<T, TR> : IObservable<IOperationContext<T, TR>> {
        IObservable<TR> Execute(T parameter);
    }

    public interface IOperationContext<out T, in TR> : IObserver<TR> {
        T Parameter { get; }
    }

    /// <summary>
    ///     Utility class for <see cref="ReactiveOperation{T,TR}" />
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TR"></typeparam>
    public class OperationContext<T, TR> : IOperationContext<T, TR>, IObservable<TR> {
        public T Parameter { get; }

        private readonly Subject<TR>      subject;
        private readonly Queue<TR>        queue;
        private readonly Queue<Exception> queueExceptions;

        private bool isComplete;
        private int  countObservers;

        public OperationContext(T parameter) {
            this.Parameter = parameter;

            this.subject         = new Subject<TR>();
            this.queue           = new Queue<TR>();
            this.queueExceptions = new Queue<Exception>();

            this.isComplete     = false;
            this.countObservers = 0;
        }

        public IDisposable Subscribe(IObserver<TR> observer) {
            if (this.countObservers == 0) {
                foreach (var value in this.queue) {
                    observer.OnNext(value);
                }

                foreach (var exception in this.queueExceptions) {
                    observer.OnError(exception);
                }

                if (this.isComplete) {
                    observer.OnCompleted();
                }

                this.queue.Clear();
                this.queueExceptions.Clear();
            }

            Interlocked.Increment(ref this.countObservers);
            var decrement = Disposable.Create(() => { Interlocked.Decrement(ref this.countObservers); });

            var subscription = this.subject.Subscribe(observer);

            return new CompositeDisposable(subscription, decrement);
        }

        public void OnCompleted() {
            this.isComplete = true;
            this.subject.OnCompleted();
        }

        public void OnError(Exception error) {
            if (this.countObservers == 0) {
                this.queueExceptions.Enqueue(error);
            }

            this.subject.OnError(error);
        }

        public void OnNext(TR value) {
            if (this.countObservers == 0) {
                this.queue.Enqueue(value);
            }

            this.subject.OnNext(value);
        }
    }

    /// <summary>
    ///     Complex entity which returns Observable Operation at the moment execution
    /// </summary>
    /// <typeparam name="T">Input Type</typeparam>
    /// <typeparam name="TR">Return Type</typeparam>
    public class ReactiveOperation<T, TR> : IReactiveOperation<T, TR>, IDisposable {
        private readonly Subject<OperationContext<T, TR>> trigger = new Subject<OperationContext<T, TR>>();

        public IObservable<TR> Execute(T parameter) {
            var operation = new OperationContext<T, TR>(parameter);
            this.trigger.OnNext(operation);
            return operation;
        }

        public IDisposable Subscribe(IObserver<IOperationContext<T, TR>> observer) {
            return this.trigger.Subscribe(observer);
        }

        public void Dispose() {
            this.trigger?.Dispose();
        }
    }
}


#endif