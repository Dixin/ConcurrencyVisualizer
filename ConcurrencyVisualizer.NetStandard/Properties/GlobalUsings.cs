global using System.ComponentModel;
global using System.Globalization;
global using System.Reflection;
global using System.Text;
global using System.Diagnostics;
global using System.Diagnostics.CodeAnalysis;
global using System.Runtime.InteropServices;
global using System.Security;
global using System.Runtime.CompilerServices;
global using System.Security.Permissions;
#if NET9_0_OR_GREATER
global using Lock = System.Threading.Lock;
#else
global using Lock = object;
#endif