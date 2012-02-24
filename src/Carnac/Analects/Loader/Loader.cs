using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Analects.Loader
{
    public class Loader
    {
        private const string LibsFolder = "Libs";

        private static readonly Dictionary<string, Assembly> libraries = new Dictionary<string, Assembly>();
        private static readonly Dictionary<string, Assembly> reflectionOnlyLibraries = new Dictionary<string, Assembly>();

        private const string X86Environment = "x86";
        private const string X64Environment = "x64";

        public static void Initialize()
        {
            AppDomain.CurrentDomain.AssemblyResolve += FindAssembly;

            PreloadUnmanagedLibraries();
        }

        [DllImport("kernel32.dll")]
        private static extern IntPtr LoadLibrary(string dllToLoad);

        private static void PreloadUnmanagedLibraries()
        {
            // Preload correct library
            var bittyness = X86Environment;
            if (IntPtr.Size == 8)
                bittyness = X64Environment;

            var assemblyName = Assembly.GetExecutingAssembly().GetName();

            var libraries = Assembly.GetExecutingAssembly().GetManifestResourceNames()
                .Where(s => s.StartsWith(String.Format("{1}.{2}.{0}.", bittyness, assemblyName.Name, LibsFolder)))
                .ToArray();

            var dirName = Path.Combine(Path.GetTempPath(), String.Format("{2}.{1}.{0}", assemblyName.Version, bittyness, assemblyName.Name));
            if (!Directory.Exists(dirName))
                Directory.CreateDirectory(dirName);

            foreach (var lib in libraries)
            {
                string dllPath = Path.Combine(dirName, String.Join(".", lib.Split('.').Skip(3)));

                if (!File.Exists(dllPath))
                {
                    using (Stream stm = Assembly.GetExecutingAssembly().GetManifestResourceStream(lib))
                    {
                        // Copy the assembly to the temporary file
                        try
                        {
                            using (Stream outFile = File.Create(dllPath))
                            {
                                stm.CopyTo(outFile);
                            }
                        }
                        catch
                        {
                            // This may happen if another process has already created and loaded the file.
                            // Since the directory includes the version number of this assembly we can
                            // assume that it's the same bits, so we just ignore the excecption here and
                            // load the DLL.
                        }
                    }
                }

                // We must explicitly load the DLL here because the temporary directory 
                // is not in the PATH.
                // Once it is loaded, the DllImport directives that use the DLL will use
                // the one that is already loaded into the process.
                LoadLibrary(dllPath);
            }
        }

        private static Assembly LoadAssembly(string fullName)
        {
            Assembly a;

            var executingAssembly = Assembly.GetExecutingAssembly();

            var assemblyName = executingAssembly.GetName();

            var shortName = new AssemblyName(fullName).Name;
            if (libraries.ContainsKey(shortName))
                return libraries[shortName];

            var resourceName = String.Format("{0}.{2}.{1}.dll", assemblyName.Name, shortName, LibsFolder);
            var actualName = executingAssembly.GetManifestResourceNames().FirstOrDefault(n => string.Equals(n, resourceName, StringComparison.OrdinalIgnoreCase));

            if (string.IsNullOrEmpty(actualName))
            {
                // The library might be a mixed mode assembly. Try loading from the bitty folders.
                var bittyness = X86Environment;
                if (IntPtr.Size == 8)
                    bittyness = X64Environment;

                resourceName = String.Format("{0}.{3}.{1}.{2}.dll", assemblyName.Name, bittyness, shortName, LibsFolder);
                actualName = executingAssembly.GetManifestResourceNames().FirstOrDefault(n => string.Equals(n, resourceName, StringComparison.OrdinalIgnoreCase));

                if (string.IsNullOrEmpty(actualName))
                {
                    libraries[shortName] = null;
                    return null;
                }

                // Ok, mixed mode assemblies cannot be loaded through Assembly.Load.
                // See http://stackoverflow.com/questions/2945080/ and http://connect.microsoft.com/VisualStudio/feedback/details/97801/
                // But, since it's an unmanaged library we've already dumped it to disk to preload it into the process.
                // So, we'll just load it from there.
                var dirName = Path.Combine(Path.GetTempPath(), String.Format("{2}.{1}.{0}", assemblyName.Version, bittyness, assemblyName.Name));
                var dllPath = Path.Combine(dirName, String.Join(".", actualName.Split('.').Skip(3)));

                if (!File.Exists(dllPath))
                {
                    libraries[shortName] = null;
                    return null;
                }

                a = Assembly.LoadFile(dllPath);
                libraries[shortName] = a;
                return a;
            }

            using (var s = executingAssembly.GetManifestResourceStream(actualName))
            {
                var data = new BinaryReader(s).ReadBytes((int)s.Length);

                byte[] debugData = null;
                if (executingAssembly.GetManifestResourceNames().Contains(String.Format("{0}.{2}.{1}.pdb", assemblyName.Name, shortName, LibsFolder)))
                {
                    using (var ds = executingAssembly.GetManifestResourceStream(String.Format("{0}.{2}.{1}.pdb", assemblyName.Name, shortName, LibsFolder)))
                    {
                        debugData = new BinaryReader(ds).ReadBytes((int)ds.Length);
                    }
                }

                if (debugData != null)
                {
                    a = Assembly.Load(data, debugData);
                    libraries[shortName] = a;
                    return a;
                }
                a = Assembly.Load(data);
                libraries[shortName] = a;
                return a;
            }
        }

        private static Assembly ReflectionOnlyLoadAssembly(string fullName)
        {
            var executingAssembly = Assembly.GetExecutingAssembly();

            var assemblyName = Assembly.GetExecutingAssembly().GetName();

            string shortName = new AssemblyName(fullName).Name;
            if (reflectionOnlyLibraries.ContainsKey(shortName))
                return reflectionOnlyLibraries[shortName];

            var resourceName = String.Format("{0}.{2}.{1}.dll", assemblyName.Name, shortName, LibsFolder);

            if (!executingAssembly.GetManifestResourceNames().Contains(resourceName))
            {
                reflectionOnlyLibraries[shortName] = null;
                return null;
            }

            using (var s = executingAssembly.GetManifestResourceStream(resourceName))
            {
                var data = new BinaryReader(s).ReadBytes((int)s.Length);

                var a = Assembly.ReflectionOnlyLoad(data);
                reflectionOnlyLibraries[shortName] = a;

                return a;
            }
        }

        private static Assembly FindAssembly(object sender, ResolveEventArgs args)
        {
            return LoadAssembly(args.Name);
        }

        private static Assembly FindReflectionOnlyAssembly(object sender, ResolveEventArgs args)
        {
            return ReflectionOnlyLoadAssembly(args.Name);
        }

        [System.STAThreadAttribute()]
        public static void Main()
        {
            Loader.Initialize();

            Carnac.App app = new Carnac.App();
            app.InitializeComponent();
            app.Run();
        }
    }
}
