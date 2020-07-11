using System;
using JetBrains.Annotations;

namespace GMTK2020.Data.Settings
{
    [AttributeUsage(AttributeTargets.Class)]
    [MeansImplicitUse(ImplicitUseKindFlags.Assign, ImplicitUseTargetFlags.WithMembers)]
    public class SettingsProviderAttribute : Attribute
    {
    }
}