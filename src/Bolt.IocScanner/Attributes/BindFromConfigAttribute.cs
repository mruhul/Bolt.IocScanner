using System;

namespace Bolt.IocScanner.Attributes;

public class BindFromConfigAttribute(string sectionName = null, bool isOptional = false) : Attribute
{
    public string SectionName { get; private set; } = sectionName;
    public bool IsOptional { get; private set; } = isOptional;
}