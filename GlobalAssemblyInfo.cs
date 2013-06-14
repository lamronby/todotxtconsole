using System.Reflection;

[assembly: AssemblyProduct("Todo.txt Console")]

[assembly: AssemblyCopyright("Copyright © 2013 Chris Jansen")]

#if DEBUG
    [assembly: AssemblyConfiguration("Debug")]
#else
[assembly: AssemblyConfiguration("Release")]
#endif

[assembly: AssemblyVersion("0.9.0.0")]
