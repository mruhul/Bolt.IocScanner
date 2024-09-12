using System;
using System.Collections.Generic;
using System.Text;

namespace Bolt.IocScanner.Tests
{
    public abstract class AbstractClass
    {
        public AbstractClass()
        {
            Random = Guid.NewGuid().ToString();
        }

        public string Random { get; set; }
    }
}
