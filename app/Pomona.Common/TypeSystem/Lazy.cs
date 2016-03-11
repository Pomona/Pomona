#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

namespace Pomona.Common.TypeSystem
{
#if CUSTOM_LAZY_TYPE
    internal class Lazy<T>
    {
        [ThreadStatic]
        private static int recursiveCallCounter;

        private readonly Func<T> factory;
        private readonly LazyThreadSafetyMode lazyThreadSafetyMode;
        private bool isInitialized;
        private T value;


        public Lazy(Func<T> factory, LazyThreadSafetyMode lazyThreadSafetyMode)
        {
            this.lazyThreadSafetyMode = lazyThreadSafetyMode;
            this.factory = factory ?? Expression.Lambda<Func<T>>(Expression.New(typeof(T))).Compile();
        }


        public T Value
        {
            get
            {
                if (!this.isInitialized)
                {
                    try
                    {
                        if (recursiveCallCounter++ > 500)
                        {
                            throw new InvalidOperationException(
                                "Seems like we're going to get a StackOverflowException here, lets fail early to avoid that.");
                        }
                        this.value = this.factory();
                    }
                    finally
                    {
                        recursiveCallCounter--;
                    }
                    Thread.MemoryBarrier();
                    this.isInitialized = true;
                }
                return this.value;
            }
        }
    }
#endif
}