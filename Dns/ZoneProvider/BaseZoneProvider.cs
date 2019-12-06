namespace Dns.ZoneProvider
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public abstract class BaseZoneProvider : IObservable<Zone>, IDisposable
    {
        private readonly List<IObserver<Zone>> _observers = new List<IObserver<Zone>>();

        public IDisposable Subscribe(IObserver<Zone> observer)
        {
            this._observers.Add(observer);
            return new Subscription(this, observer);
        }

        private void Unsubscribe(IObserver<Zone> observer)
        {
            this._observers.Remove(observer);
        }

        public abstract void Dispose();

        /// <summary>Subscription memento for IObservable interface</summary>
        public class Subscription : IDisposable
        {
            private readonly IObserver<Zone> _observer;
            private readonly BaseZoneProvider _provider;

            public Subscription(BaseZoneProvider provider, IObserver<Zone> observer)
            {
                this._provider = provider;
                this._observer = observer;
            }

            void IDisposable.Dispose()
            {
                this._provider.Unsubscribe(this._observer);
            }
        }

        /// <summary>Publish zone to all subscribers</summary>
        /// <param name="zone"></param>
        public void Notify(Zone zone)
        {
            int remainingRetries = 3;

            while (remainingRetries > 0)
            {
                ParallelLoopResult result = Parallel.ForEach(this._observers, observer => observer.OnNext(zone));
                if (result.IsCompleted)
                {
                    break;
                }
                remainingRetries--;
            }
        }

        public abstract void Start();

        public abstract void Stop();
    }
}