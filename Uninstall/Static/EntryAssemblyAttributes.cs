using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Uninstall.Static
{
    public static class EntryAssemblyAttributes
    {
        private static Assembly ExecutingAssembly = Assembly.GetEntryAssembly();
        private static AssemblyTitleAttribute assemblyTitle = ExecutingAssembly.GetCustomAttributes(typeof(AssemblyTitleAttribute), false)[0] as AssemblyTitleAttribute;
        private static AssemblyDescriptionAttribute assemblyDescription = ExecutingAssembly.GetCustomAttributes(typeof(AssemblyDescriptionAttribute), false)[0] as AssemblyDescriptionAttribute;
        private static AssemblyCompanyAttribute assemblyCompany = ExecutingAssembly.GetCustomAttributes(typeof(AssemblyCompanyAttribute), false)[0] as AssemblyCompanyAttribute;
        private static AssemblyCopyrightAttribute assemblyCopyright = ExecutingAssembly.GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false)[0] as AssemblyCopyrightAttribute;
        private static AssemblyProductAttribute assemblyProduct = ExecutingAssembly.GetCustomAttributes(typeof(AssemblyProductAttribute), false)[0] as AssemblyProductAttribute;
        //private static AssemblyVersionAttribute assemblyVersion = ExecutingAssembly.GetCustomAttributes(typeof(AssemblyVersionAttribute), false)[0] as AssemblyVersionAttribute;
        public static String AssemblyTitle { get { return assemblyTitle.Title; } }
        public static String AssemblyDescription { get { return assemblyDescription.Description; } }
        public static String AssemblyCompany { get { return assemblyCompany.Company; } }
        public static String AssemblyCopyright { get { return assemblyCopyright.Copyright; } }
        public static String AssemblyProduct { get { return assemblyProduct.Product; } }
        //public static String AssemblyVersion { get { return assemblyVersion.Version; } }
    }
}
