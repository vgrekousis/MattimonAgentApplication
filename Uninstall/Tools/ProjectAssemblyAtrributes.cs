using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Uninstall.Tools
{
    public class ProjectAssemblyAtrributes
    {
        private Assembly TargetAssembly;
        private AssemblyTitleAttribute assemblyTitle;
        private AssemblyDescriptionAttribute assemblyDescription;
        private AssemblyCompanyAttribute assemblyCompany;
        private AssemblyCopyrightAttribute assemblyCopyright;
        private AssemblyProductAttribute assemblyProduct;

        /// <summary>
        /// 
        /// </summary>
        public String AssemblyTitle { get { return assemblyTitle.Title; } }
        /// <summary>
        /// 
        /// </summary>
        public String AssemblyDescription { get { return assemblyDescription.Description; } }
        /// <summary>
        /// 
        /// </summary>
        public String AssemblyCompany { get { return assemblyCompany.Company; } }
        /// <summary>
        /// 
        /// </summary>
        public String AssemblyCopyright { get { return assemblyCopyright.Copyright; } }
        /// <summary>
        /// 
        /// </summary>
        public String AssemblyProduct { get { return assemblyProduct.Product; } }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="fullpath">Full .exe path to load assembly from.</param>
        public ProjectAssemblyAtrributes(String fullpath, Boolean rawAssembly = true)
        {
            if (!rawAssembly)
                LoadAssembly(fullpath);
            else
                LoadRawAssembly(fullpath);
        }
        /// <summary>
        /// Get the loaded assembly
        /// </summary>
        /// <returns>Returns the loaded <code>System.Reflection.Assembly</code></returns>
        public Assembly GetAssembly()
        {
            return TargetAssembly;
        }
        /// <summary>
        /// Get the loaded assembly version
        /// </summary>
        /// <returns>Returns <code>System.Version</code> from the loaded assembly</returns>
        public Version GetAssemblyVersion()
        {
            return TargetAssembly.GetName().Version;
        }
        /// <summary>
        /// Reloads the assembly from an other location
        /// </summary>
        /// /// <param name="fullpath">Full .exe path to load assembly from.</param>
        public void ReloadAssembly(String fullpath)
        {
            LoadAssembly(fullpath);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="fullpath"></param>
        private void LoadAssembly(String fullpath)
        {
            // Load the assembly from 
            TargetAssembly = Assembly.LoadFrom(fullpath);
            InititializeAttributes();
        }

        private void LoadRawAssembly(String fullpath)
        {
            TargetAssembly = System.Reflection.Assembly.Load(System.IO.File.ReadAllBytes(fullpath));
            InititializeAttributes();
        }


        private void InititializeAttributes()
        {
            if (TargetAssembly == null)
            {
                throw new NullReferenceException("Target Assembly was not loaded");
            }
            // Load the assembly title
            assemblyTitle = TargetAssembly.GetCustomAttributes(typeof(AssemblyTitleAttribute), false)[0] as AssemblyTitleAttribute;

            // Load the assembly description
            assemblyDescription = TargetAssembly.GetCustomAttributes(typeof(AssemblyDescriptionAttribute), false)[0] as AssemblyDescriptionAttribute;

            // Load the assembly company
            assemblyCompany = TargetAssembly.GetCustomAttributes(typeof(AssemblyCompanyAttribute), false)[0] as AssemblyCompanyAttribute;

            // Load the assembly copyright
            assemblyCopyright = TargetAssembly.GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false)[0] as AssemblyCopyrightAttribute;

            // Load the assembly product
            assemblyProduct = TargetAssembly.GetCustomAttributes(typeof(AssemblyProductAttribute), false)[0] as AssemblyProductAttribute;
        }
    }
}
