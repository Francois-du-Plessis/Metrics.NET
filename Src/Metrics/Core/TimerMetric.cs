﻿using System;
using Metrics.Utils;

namespace Metrics.Core
{
    public sealed class TimerMetric : Timer
    {
        private readonly Clock clock;
        private readonly Meter meter;
        private readonly Histogram histogram;

        public TimerMetric()
            : this(new HistogramMetric(), new MeterMetric(), Clock.Default) { }

        public TimerMetric(SamplingType samplingType)
            : this(new HistogramMetric(samplingType), new MeterMetric(), Clock.Default) { }

        public TimerMetric(SamplingType samplingType, Meter meter, Clock clock)
            : this(new HistogramMetric(samplingType), meter, clock) { }

        public TimerMetric(Histogram histogram, Meter meter, Clock clock)
        {
            this.clock = clock;
            this.meter = meter;
            this.histogram = histogram;
        }

        public void Record(long duration, TimeUnit unit)
        {
            var nanos = unit.ToNanoseconds(duration);
            if (nanos >= 0)
            {
                this.histogram.Update(nanos);
                this.meter.Mark();
            }
        }

        public void Time(Action action)
        {
            var start = this.clock.Nanoseconds;
            try
            {
                action();
            }
            finally
            {
                Record(this.clock.Nanoseconds - start, TimeUnit.Nanoseconds);
            }
        }

        public T Time<T>(Func<T> action)
        {
            var start = this.clock.Nanoseconds;
            try
            {
                return action();
            }
            finally
            {
                Record(this.clock.Nanoseconds - start, TimeUnit.Nanoseconds);
            }
        }

        public IDisposable NewContext()
        {
            var start = this.clock.Nanoseconds;
            return new DisposableAction(() => Record(this.clock.Nanoseconds - start, TimeUnit.Nanoseconds));
        }

        public TimerValue Value
        {
            get
            {
                return new TimerValue(this.meter.Value, this.histogram.Value);
            }
        }
    }
}
