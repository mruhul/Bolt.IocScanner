using System;

namespace Bolt.IocAttributes
{
    public class AutoBindAttribute : Attribute
    {
        public AutoBindAttribute() : this(LifeCycle.Transient)
        {
        }

        public AutoBindAttribute(LifeCycle lifeCycle)
        {
            LifeCycle = lifeCycle;
        }

        public LifeCycle LifeCycle { get; private set; }
        public bool UseTryAdd { get; set; }
    }
}
