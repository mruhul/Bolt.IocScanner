using System;
using System.Collections.Generic;

namespace Bolt.IocScanner;

public class IocScannerOptions
{
    public IEnumerable<Type> InterfacesToExclude { get; set; } = [typeof(IDisposable)];
    public Func<Type, bool> BindServicesOnMatch { get; set; }
}