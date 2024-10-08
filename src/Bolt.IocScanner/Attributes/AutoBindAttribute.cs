﻿using System;

namespace Bolt.IocScanner.Attributes;

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
}